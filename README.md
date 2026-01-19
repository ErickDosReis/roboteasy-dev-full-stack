🚀 Desafio Técnico - Full Stack Chat App
Uma aplicação de chat em tempo real desenvolvida com .NET 8 no backend e Vue 3 + TypeScript no frontend, utilizando RabbitMQ para mensageria, PostgreSQL para persistência e Docker para orquestração.

📋 Pré-requisitos
Para rodar este projeto, você precisa apenas de:

Docker Desktop instalado e rodando.

Git (opcional, para clonar o repositório).

⚡ Guia de Execução Rápida (Docker)
Siga os passos abaixo para subir todo o ambiente (Banco, RabbitMQ, API e Frontend) com um único comando.

1. Configuração de Variáveis de Ambiente
   O projeto utiliza dois arquivos .env para segurança e configuração. Execute os comandos abaixo no PowerShell (na raiz do projeto) para criá-los automaticamente:

A. Configuração da Raiz (Backend/Infra): Define o segredo do JWT utilizado pelo Backend.

PowerShell
Set-Content -Path ".env" -Value "JWT_SECRET=chave_secreta_no_minimo_32_caracteres"
B. Configuração do Frontend (Nginx Proxy): Define as rotas relativas para que o Nginx faça o proxy reverso corretamente.

PowerShell
$envContent = @"
VITE_API_HOST_AUTH=/api/auth
VITE_API_HOST_USER=/api/users
VITE_API_HOST_CHAT=/api/chatmessage
"@

Set-Content -Path "frontend\ChatAppFront\.env" -Value $envContent

2. Subindo os Containers
   Na raiz do projeto, execute:

Bash
docker compose up -d --build
Aguarde alguns instantes. O Docker irá:

Baixar as imagens do Postgres e RabbitMQ.

Compilar a API .NET.

Compilar o Frontend Vue e configurar o Nginx.

3. Acessando a Aplicação
   Frontend (Aplicação): http://localhost:5173

API (Swagger - Opcional): http://localhost:5000/swagger (se a porta estiver exposta)

🏗️ Arquitetura da Solução
Stack Tecnológico
Backend: ASP.NET Core 8, Entity Framework Core, SignalR (WebSockets), JWT Authentication.

Frontend: Vue 3 (Composition API), TypeScript, Pinia (State Management), Vue Router, Axios.

Banco de Dados: PostgreSQL 15.

Mensageria: RabbitMQ (para processamento assíncrono de mensagens).

Infraestrutura: Docker Compose & Nginx (Proxy Reverso).

Detalhes de Implementação
Reverse Proxy (Nginx): O container do Frontend roda um servidor Nginx que serve os arquivos estáticos do Vue e atua como proxy para a API.

Chamadas para /api/\* -> Redirecionadas para o container backend-api.

Chamadas para /hubs/\* -> Redirecionadas para o WebSocket do SignalR.

Isso elimina problemas de CORS e simula um ambiente de produção real.

Layout Persistente (Vue): O Frontend utiliza uma arquitetura de layout onde o App.vue gerencia o container principal. Isso permite transições suaves ("Morph effect") ao navegar entre Login, Registro e Chat, sem recarregar a estrutura da página.

Docker Compose: Orquestra a dependência entre serviços. A API só inicia após o Postgres e o RabbitMQ estarem saudáveis (healthcheck).

🛠️ Comandos Úteis
Ver logs em tempo real
Se algo não funcionar, verifique os logs:

Bash
docker compose logs -f
Limpeza Total ("Nuclear")
Para parar tudo, apagar os containers e resetar o banco de dados (apaga todos os dados):

Bash
docker compose down -v --rmi local
Reconstruir sem cache
Se você alterou pacotes npm ou nuget:

Bash
docker builder prune -f
docker compose up -d --build

🧪 Como Testar
Acesse http://localhost:5173.

Clique em "Cadastre-se" e crie um usuário (ex: User1).

Faça Login.

Abra uma Aba Anônima e crie outro usuário (ex: User2).

Na lista de usuários, você verá o outro usuário online.

Clique para iniciar o chat e troque mensagens em tempo real.

Reinicie a página para verificar se o histórico de mensagens foi salvo (PostgreSQL).
