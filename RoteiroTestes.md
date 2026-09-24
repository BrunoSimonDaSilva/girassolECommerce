# Roteiro de Testes Rápido

## 1. Abrir PowerShell na pasta do projeto

```powershell
cd "c:\Users\Bruno\OneDrive\Área de Trabalho\web\Trabalho"
```

## 2. Subir o MongoDB com Docker

```powershell
docker compose up -d
```

## 3. Configurar variáveis de ambiente

```powershell
$env:MONGODB_URI = "mongodb://localhost:27017"
$env:JWT_SECRET_KEY = "8a9f2e9d6b7c4d3a2f1e0c9b8a7d6e5f4g3h2j1k0l9m8n7p6q5r4s3t2u1v0w9x8y7z6"
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

## 4. Restaurar e rodar a API

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\GirassolCommerce.Api\GirassolCommerce.Api.csproj
```

## 5. Abrir no navegador

```text
local host que aparecer no "now listening on :..." do passo anterior

```

Swagger:

```text
local host que aparecer no "now listening on :...+/swagger" do passo anterior
```

## 6. Login do admin

```powershell
$body = @{ email='admin@girassol.com'; senha='Admin@123' } | ConvertTo-Json
$resp = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/auth/login -ContentType 'application/json' -Body $body
$resp
```

## 7. Listar produtos

```powershell
Invoke-RestMethod -Uri http://localhost:5000/api/produtos
```

## 8. Criar produto (admin)

```powershell
$token = $resp.token
$body = @{
  nome = "Caneca Girassol"
  descricao = "Caneca de cerâmica com padrão floral"
  categoria = "Casa"
  preco = 59.90
  estoque = 10
  imagemUrl = "https://images.example.com/caneca.jpg"
  tags = @("girassol", "casa", "decor")
  ativo = $true
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/produtos -ContentType 'application/json' -Headers @{ Authorization = "Bearer $token" } -Body $body
```

## 9. Criar pedido

```powershell
$body = @{
  nomeCliente = "Maria"
  emailCliente = "maria@email.com"
  enderecoEntrega = "Rua das Flores, 100"
  itens = @(
    @{ produtoId = "ID_DO_PRODUTO"; quantidade = 2 }
  )
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/pedidos -ContentType 'application/json' -Headers @{ Authorization = "Bearer $token" } -Body $body
```

## 10. Usuário padrão

- Email: `admin@girassol.com`
- Senha: `Admin@123`
