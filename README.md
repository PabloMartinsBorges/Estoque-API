# Visao Geral da Arquitetura do Sistema de Estoque

```text
Estoque/
├── src/
│   ├── Estoque.Domain          # Entidades, Enums e Interfaces de Repositorio
│   ├── Estoque.Application     # Casos de uso, Servicos, DTOs e Excecoes de Dominio
│   ├── Estoque.Infra.Data      # Contexto EF Core, Mapeamentos e Repositorios Npgsql
│   └── Estoque.Api             # Controllers, Middlewares de Tratamento de Erro e Webhook
└── tests/
    ├── UnitTests               # Testes unitarios de regras de negocio (xUnit, Moq, FluentAssertions)
    └── IntegrationTests        # Testes end-to-end em memoria (WebApplicationFactory, InMemoryDb)
```

---

# Estoque API

A **Estoque API** e um servico backend desenvolvido em **.NET** orientado aos principios da **Clean Architecture**, responsavel pelo gerenciamento do catalogo de produtos, controle de itens em falta e integracao em tempo real com bot do Telegram para notificacoes e comandos operacionais.

---

## Requisitos

- **.NET 10 SDK** (ou .NET 9 SDK): Necessario para compilacao e execucao da API.
- **PostgreSQL / Supabase**: Instancia relacional para persistencia de dados.
- **Git**: Para versionamento e clonagem do codigo-fonte.
- **Token de Bot do Telegram**: Gerado via `@BotFather` para autenticacao da integracao.

---

## Instalacao

1. Clone o repositorio:
   ```bash
   git clone https://github.com/SeuUsuario/Estoque.git
   ```

2. Acesse o diretorio raiz do projeto:
   ```bash
   cd Estoque
   ```

3. Configure o banco de dados PostgreSQL:
   - Caso utilize o **Supabase**, crie um novo projeto e obtenha a string de conexao no modo Transaction/Session Pooler ou Direct Connection.
   - Caso utilize o PostgreSQL localmente, crie a base de dados:
     ```sql
     CREATE DATABASE estoque_db;
     ```

4. Ajuste as configuracoes no arquivo `src/Estoque.Api/appsettings.json` (ou `appsettings.Development.json`):

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=estoque_db;Username=postgres;Password=sua_senha;"
     },
     "Telegram": {
       "BotToken": "SEU_TELEGRAM_BOT_TOKEN",
       "SecretToken": "SEU_SECRET_TOKEN_SEGURO"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

   **Variaveis explicadas:**
   - `ConnectionStrings:DefaultConnection`: String de conexao Npgsql para comunicacao com o PostgreSQL ou Supabase.
   - `Telegram:BotToken`: Token HTTP fornecido pelo Telegram para autenticar as operacoes do bot.
   - `Telegram:SecretToken`: Segredo compartilhado enviado no cabecalho `X-Telegram-Bot-Api-Secret-Token` para autenticar a origem das requisicoes do Webhook.

---

## Inicializacao do Banco de Dados (EF Core Migrations)

O esquema de tabelas e o versionamento estrutural sao gerenciados via Entity Framework Core:

1. Certifique-se de que a ferramenta de linha de comando do EF Core esta disponivel globalmente:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Aplique as migracoes existentes para gerar o esquema no banco de dados configurado:
   ```bash
   dotnet ef database update --project src/Estoque.Infra.Data --startup-project src/Estoque.Api
   ```

---

## Executando a API

1. Inicie a aplicacao via terminal a partir do projeto de inicializacao:
   ```bash
   dotnet run --project src/Estoque.Api
   ```

2. Acesse a documentacao interativa OpenAPI / Swagger (em ambiente de desenvolvimento):
   - **Swagger UI:** [http://localhost:5000/swagger](http://localhost:5000/swagger) ou [https://localhost:7001/swagger](https://localhost:7001/swagger)

---

## Execucao dos Testes

A suite de testes do projeto e dividida entre testes unitarios isolados e testes de integracao de ponta a ponta:

### 1. Testes Unitarios
Validam isoladamente as regras de negocio das camadas de aplicacao utilizando **xUnit**, **Moq** e **FluentAssertions**:
```bash
dotnet test UnitTests
```

### 2. Testes de Integracao
Validam o pipeline HTTP, roteamento, middlewares de excecao e persistencia em memoria utilizando **WebApplicationFactory** e **EF Core InMemory**:
```bash
dotnet test IntegrationTests
```

### 3. Execucao Consolidada
Para executar todas as suites de teste da solucao de forma automatica:
```bash
dotnet test
```

---

## Principais Endpoints da API

| Metodo | Rota | Descricao |
| :--- | :--- | :--- |
| `POST` | `/api/Produto` | Cadastra um novo produto no catalogo. |
| `GET` | `/api/Produto/{id:int}` | Recupera os detalhes de um produto pelo identificador. |
| `GET` | `/api/Produto/listar-falta` | Lista os produtos marcados como indisponiveis nas ultimas 48 horas. |
| `PATCH` | `/api/Produto/{id:int}` | Atualiza o estado de disponibilidade de um produto. |
| `POST` | `/api/telegram/webhook` | Recebe atualizacoes, mensagens de texto e interacoes de botões do Telegram. |

---

## Troubleshooting

### Problemas comuns:

- **Falha de conexao com PostgreSQL ou Supabase:**
  - Verifique se a string de conexao no arquivo `appsettings.json` contem credenciais e portas corretas.
  - Para conexoes com o Supabase, confirme se os parametros de seguranca SSL estao configurados adequadamente (`SSL Mode=Require;Trust Server Certificate=true;`).

- **Erro de Target do VSTest no .NET 10 (`Testing with VSTest target is no longer supported`):**
  - O SDK do .NET 10 descontinuou o alvo VSTest classico. Certifique-se de que pacotes legados como `coverlet.msbuild` foram removidos dos arquivos `.csproj` de testes e defina a propriedade `<EnableMSTestRunner>false</EnableMSTestRunner>` ou `<TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>`.

- **Retorno HTTP 403 Forbidden no Webhook do Telegram:**
  - O endpoint `/api/telegram/webhook` valida obrigatoriamente o cabecalho `X-Telegram-Bot-Api-Secret-Token`. Certifique-se de que o valor informado na chamada corresponde ao valor configurado em `Telegram:SecretToken`.

- **Erro de Roteamento (400 Bad Request em `/listar-falta`):**
  - Garanta que o atributo da Action esteja declarado com rota relativa `[HttpGet("listar-falta")]` (sem barra inicial) e que a rota de busca por identificador numérico possua a restricao de tipo `[HttpGet("{id:int}")]`.
