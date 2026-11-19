# Roteiro de Testes Rápido

## 1. Abrir PowerShell na pasta do projeto

```powershell
cd "C:\Users\Bruno\OneDrive\Área de Trabalho\web\Trabalho\girassolECommerce"
```

## 2. Iniciar o Docker e subir o MongoDB

Abra o Docker Desktop e espere ele ficar pronto. No PowerShell, na pasta do projeto, execute:

```powershell
docker compose up -d
docker compose ps
```

O serviço `mongo` deve aparecer como ativo. Se o comando `docker compose` não for reconhecido, confirme que o Docker Desktop está instalado e em execução.

## 3. Configurar variáveis de ambiente

```powershell
$env:MONGODB_URI = "mongodb://localhost:27017"
$env:JWT_SECRET_KEY = "8a9f2e9d6b7c4d3a2f1e0c9b8a7d6e5f4g3h2j1k0l9m8n7p6q5r4s3t2u1v0w9x8y7z6"
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

## 4. Restaurar, compilar e iniciar a API

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\GirassolCommerce.Api\GirassolCommerce.Api.csproj
```

Deixe essa janela do PowerShell aberta enquanto estiver usando a aplicação. A API informa no terminal o endereço em que está ouvindo.

## 5. Abrir a aplicação no navegador

- Aplicação: http://localhost:5197
- Swagger: http://localhost:5197/swagger

Se a porta exibida no terminal for diferente, use-a no lugar de `5197` nos endereços dos passos seguintes.

## 6. Login do admin

```powershell
$body = @{ email='admin@girassol.com'; senha='Admin@123' } | ConvertTo-Json
$resp = Invoke-RestMethod -Method Post -Uri http://localhost:5197/api/auth/login -ContentType 'application/json' -Body $body
$resp
```

## 7. Listar produtos

```powershell
Invoke-RestMethod -Uri http://localhost:5197/api/produtos
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

Invoke-RestMethod -Method Post -Uri http://localhost:5197/api/produtos -ContentType 'application/json' -Headers @{ Authorization = "Bearer $token" } -Body $body
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

Invoke-RestMethod -Method Post -Uri http://localhost:5197/api/pedidos -ContentType 'application/json' -Headers @{ Authorization = "Bearer $token" } -Body $body
```

## 10. Usuário padrão

- Email: `admin@girassol.com`
- Senha: `Admin@123`
