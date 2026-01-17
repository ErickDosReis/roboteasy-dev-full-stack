using System.Text;
using System.Text.Json;
using ChatApp.DTOs;
using ChatApp.Interfaces;
using ChatApp.Services.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatApp.Infrastructure;

public sealed class ChatMessageConsumer(IOptions<RabbitMqOptions> options,
                                        IServiceScopeFactory scopeFactory,
                                        ILogger<ChatMessageConsumer> logger) : BackgroundService, IDisposable
{
    private readonly RabbitMqOptions _opt = options.Value;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<ChatMessageConsumer> _logger = logger;

    private IConnection? _conn;
    private IModel? _ch;

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        // já conectado
        if (_conn is { IsOpen: true } && _ch is { IsOpen: true })
            return;

        // limpa qualquer coisa quebrada
        try { _ch?.Dispose(); } catch { }
        try { _conn?.Dispose(); } catch { }
        _ch = null;
        _conn = null;

        var delay = TimeSpan.FromSeconds(2);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _opt.Host,
                    Port = _opt.Port,
                    UserName = _opt.UserName,
                    Password = _opt.Password,
                    DispatchConsumersAsync = true
                };

                _conn = factory.CreateConnection();
                _ch = _conn.CreateModel();

                _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Direct, durable: true);
                _ch.QueueDeclare(_opt.Queue, durable: true, exclusive: false, autoDelete: false);
                _ch.QueueBind(_opt.Queue, _opt.Exchange, _opt.RoutingKey);
                _ch.BasicQos(prefetchSize: 0, prefetchCount: 20, global: false);

                _logger.LogInformation("Conectado ao RabbitMQ em {Host}:{Port}", _opt.Host, _opt.Port);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao conectar no RabbitMQ. Tentando novamente em {Delay}s...", delay.TotalSeconds);
                await Task.Delay(delay, ct);

                // backoff simples até 10s
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
            }
        }
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await EnsureConnectedAsync(stoppingToken);

            if (_ch is null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            var consumer = new AsyncEventingBasicConsumer(_ch);

            consumer.Received += async (_, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var dto = JsonSerializer.Deserialize<ChatMessageCreatedDto>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (dto is null)
                    {
                        _logger.LogWarning("Mensagem inválida (DTO null). Ack para remover da fila.");
                        _ch.BasicAck(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IChatMessageService>();

                    await service.PersistMessageAsync(dto, stoppingToken);

                    _ch.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON inválido. Nack sem requeue (descarta).");
                    try { _ch?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false); } catch { }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem. Nack com requeue.");
                    try { _ch?.BasicNack(ea.DeliveryTag, multiple: false, requeue: true); } catch { }
                }
            };

            try
            {
                _ch.BasicConsume(queue: _opt.Queue, autoAck: false, consumer: consumer);

                // Mantém este ciclo “vivo” enquanto a conexão estiver aberta.
                while (!stoppingToken.IsCancellationRequested && _conn is { IsOpen: true } && _ch is { IsOpen: true })
                {
                    await Task.Delay(1000, stoppingToken);
                }

                _logger.LogWarning("Conexão/canal RabbitMQ fechou. Irá reconectar.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha no consumo. Irá reconectar.");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }


    public override Task StopAsync(CancellationToken ct)
    {
        try { _ch?.Close(); } catch {}
        try { _conn?.Close(); } catch {}
        return base.StopAsync(ct);
    }

    public override void Dispose()
    {
        try { _ch?.Dispose(); } catch { }
        try { _conn?.Dispose(); } catch { }
        base.Dispose();
    }

}
