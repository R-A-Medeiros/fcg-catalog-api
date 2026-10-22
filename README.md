# FCG - CatalogAPI

Microsserviço de **Catálogo** da FIAP Cloud Games (FCG).

Responsável pelo CRUD de jogos, pela biblioteca de jogos do usuário e por **iniciar o
fluxo de compra**. Publica `OrderPlacedEvent` e consome `PaymentProcessedEvent`.

## Papel no fluxo de compra

1. Recebe `POST /api/purchases` (autenticado) e publica **`OrderPlacedEvent`** com `OrderId`, `UserId`, `GameId`, `Price`.
2. O PaymentsAPI processa e publica `PaymentProcessedEvent`.
3. Este serviço **consome `PaymentProcessedEvent`**: se `Approved`, adiciona o jogo à biblioteca do usuário; se `Rejected`, marca o pedido como rejeitado.

## Tecnologias

- .NET 10 (ASP.NET Core Web API)
- PostgreSQL (via EF Core + Npgsql)
- RabbitMQ (via MassTransit) — publisher e consumer
- JWT (valida o token emitido pela UsersAPI, usando a **chave simétrica compartilhada**)

## Endpoints

| Método | Rota                  | Auth | Descrição                                            |
|--------|-----------------------|------|------------------------------------------------------|
| GET    | `/api/games`          | não  | Lista jogos                                          |
| GET    | `/api/games/{id}`     | não  | Obtém um jogo                                        |
| POST   | `/api/games`          | sim  | Cria um jogo                                         |
| PUT    | `/api/games/{id}`     | sim  | Atualiza um jogo                                     |
| DELETE | `/api/games/{id}`     | sim  | Remove um jogo                                       |
| POST   | `/api/purchases`      | sim  | Inicia a compra (publica `OrderPlacedEvent`)         |
| GET    | `/api/library`        | sim  | Lista a biblioteca do usuário autenticado            |
| GET    | `/health`             | não  | Health check                                         |
| GET    | `/scalar/v1`          | não  | Documentação interativa (OpenAPI)                    |

> Endpoints autenticados exigem o header `Authorization: Bearer <token>` obtido no login da UsersAPI.

### Exemplo: comprar um jogo

```bash
curl -X POST http://localhost:5002/api/purchases \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "gameId": "<GUID_DO_JOGO>" }'
```

## Variáveis de ambiente

| Variável                        | Descrição                                        | Exemplo                                                                            |
|---------------------------------|--------------------------------------------------|------------------------------------------------------------------------------------|
| `ConnectionStrings__Postgres`   | String de conexão do PostgreSQL (**secret**)     | `Host=postgres;Port=5432;Database=fcg_catalog;Username=postgres;Password=postgres` |
| `Jwt__Issuer`                   | Emissor do token (deve bater com UsersAPI)       | `FCG`                                                                              |
| `Jwt__Audience`                 | Audiência do token                               | `FCG.Clients`                                                                      |
| `Jwt__SecretKey`                | Chave simétrica do JWT (**secret**, mesma do UsersAPI) | `super_secret_shared_key_min_32_chars_long!!`                                |
| `RabbitMq__Host`                | Host do RabbitMQ                                  | `rabbitmq`                                                                         |
| `RabbitMq__Username`            | Usuário do RabbitMQ (**secret**)                 | `guest`                                                                            |
| `RabbitMq__Password`            | Senha do RabbitMQ (**secret**)                   | `guest`                                                                            |
| `ASPNETCORE_URLS`               | URL de escuta do Kestrel                          | `http://+:8080`                                                                    |

## Banco de dados (migrations)

O banco **não** é criado automaticamente. Script SQL idempotente pronto em [`db/init.sql`](db/init.sql):

```bash
psql -h localhost -U postgres -c "CREATE DATABASE fcg_catalog;"
psql -h localhost -U postgres -d fcg_catalog -f db/init.sql
```

Ou, com o EF Core: `dotnet ef database update --project src/CatalogAPI`.

## Executando localmente

```bash
dotnet run --project src/CatalogAPI
```

## Docker

```bash
docker build -t fcg/catalog-api:latest .
```

## Kubernetes

```bash
kubectl apply -f k8s/
kubectl get pods
```

- `configmap.yaml` — configuração não sensível (RabbitMQ host, issuer/audience do JWT)
- `secret.yaml` — dados sensíveis (connection string, chave JWT, credenciais do RabbitMQ)
- `deployment.yaml` — Deployment do serviço
- `service.yaml` — Service ClusterIP (`catalog-api:80` -> container `:8080`)
