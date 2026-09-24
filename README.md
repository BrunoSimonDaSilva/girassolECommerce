# Girassol Commerce

Este projeto é uma aplicação de e-commerce em C# com ASP.NET Core, MongoDB e frontend em HTML/JavaScript. O domínio escolhido é gestão de catálogo de produtos e pedidos, com autenticação JWT e autorização por perfil.

## Autor

- Nome completo: Bruno
- Curso: Engenharia de Software
- Turma: 2026.2

## Pré-requisitos

- .NET SDK 10.0.401 ou superior
- MongoDB em execução localmente ou via Docker
- Git

## Executando localmente

1. Clone o projeto e entre na pasta raiz.
2. Inicie o MongoDB com Docker:
   ```bash
   docker compose up -d
   ```
3. Configure as variáveis de ambiente:
   ```bash
   set MONGODB_URI=mongodb://localhost:27017
   set JWT_SECRET_KEY=sua-chave-secreta-aqui
   ```
   No PowerShell, use `$env:MONGODB_URI = "mongodb://localhost:27017"` e `$env:JWT_SECRET_KEY = "sua-chave-secreta-aqui"`.
4. Execute a API:
   ```bash
   dotnet run --project src\GirassolCommerce.Api\GirassolCommerce.Api.csproj
   ```
5. Abra a aplicação em http://localhost:5000 ou em https://localhost:5001 (a porta pode variar conforme a configuração do ASP.NET Core).

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
