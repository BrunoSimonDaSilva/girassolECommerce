# Girassol Commerce

Este projeto é uma aplicação de e-commerce em C# com ASP.NET Core, MongoDB e frontend em HTML/JavaScript. O domínio escolhido é gestão de catálogo de produtos e pedidos, com autenticação JWT e autorização por perfil.

## Autor

- Nome completo: Bruno
- Curso: Engenharia de Software
- Turma: 2026.2

## Pré-requisitos

- .NET SDK 10.0.401 ou superior
- Git

## Executando localmente

1. Clone o projeto e entre na pasta raiz.
2. Execute os comandos abaixo. O Docker não é necessário: por padrão, a API tenta conectar ao MongoDB local e, se ele estiver indisponível, inicia com dados de demonstração em memória.
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project .\src\GirassolCommerce.Api\GirassolCommerce.Api.csproj
   ```
3. Abra a aplicação no endereço informado no terminal (por exemplo, http://localhost:5197).

Para usar o MongoDB, inicie o Docker Desktop e execute:
   ```bash
   docker compose up -d
   ```
   Ao iniciar a API, ela detecta o banco disponível e passa a usar a persistência MongoDB.

No modo em memória, os dados são reiniciados quando a API é encerrada. O usuário administrador e os produtos de exemplo são recriados a cada inicialização.

## Acesso ao Swagger

Após iniciar a API, acesse:

- http://localhost:5000/swagger
- ou https://localhost:5001/swagger

A interface documenta os endpoints, parâmetros e exemplos de resposta.

## Variáveis de ambiente

| Variável | Descrição | Exemplo |
| --- | --- | --- |
| `MONGODB_URI` | String de conexão do MongoDB | `mongodb://localhost:27017` |
| `JWT_SECRET_KEY` | Chave para assinar JWT | `minha-chave-muito-secreta` |
| `Storage__Mode` | `Auto` (padrão), `MongoDB` ou `InMemory` | `InMemory` |

`Auto` usa MongoDB quando está acessível e troca para armazenamento em memória quando não está. Use `Storage__Mode=MongoDB` para exigir o banco ou `Storage__Mode=InMemory` para forçar o modo mock.

## Usuários padrão

O projeto cria automaticamente um usuário administrador ao iniciar:

- Email: `admin@girassol.com`
- Senha: `Admin@123`

## Endpoints principais

### Autenticação

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Produtos

- `GET /api/produtos`
- `GET /api/produtos/{id}`
- `POST /api/produtos` (apenas admin)
- `PUT /api/produtos/{id}` (apenas admin)
- `DELETE /api/produtos/{id}` (apenas admin)

### Pedidos

- `GET /api/pedidos`
- `GET /api/pedidos/{id}`
- `POST /api/pedidos`
- `PUT /api/pedidos/{id}` (apenas admin)
- `DELETE /api/pedidos/{id}` (apenas admin)

## Testes

Para executar a suíte de testes:

```bash
dotnet test
```

## Uso de IA

- Ferramentas usadas: Copilot
- Onde ajudou: organizou a estrutura inicial da API, sugeriu a modelagem de produtos e pedidos e explicou a autenticação JWT.
- O que eu revisei/reescrevi: redefini os endpoints, validei a regra de negócio de estoque e ajustei o tratamento de erro para devolver mensagens consistentes e status HTTP corretos.

## Observações

A aplicação foi desenvolvida com foco no cumprimento dos requisitos da disciplina, respeitando persistência NoSQL real, CRUD completo, documentação Swagger e frontend assíncrono.
