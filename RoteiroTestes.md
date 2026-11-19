# Roteiro de Testes Rápido

## 1. Abrir PowerShell na pasta do projeto

```powershell
cd "C:\Users\Bruno\OneDrive\Área de Trabalho\web\Trabalho\girassolECommerce"
```

## 2. Iniciar a API (Docker opcional)

Para usar MongoDB com persistência, abra o Docker Desktop e execute:

```powershell
docker compose up -d
docker compose ps
```

Se não quiser usar Docker, pule este passo. A API inicia automaticamente com armazenamento mock em memória quando não consegue acessar o MongoDB.

## 3. Restaurar, compilar e iniciar a API

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\GirassolCommerce.Api\GirassolCommerce.Api.csproj
```

Esses são os únicos comandos necessários para executar a API. Por padrão ela usa o MongoDB se estiver disponível; caso contrário, usa dados de demonstração em memória. Os dados em memória são perdidos ao encerrar a API. Para testar esse modo mesmo com MongoDB disponível, execute antes `$env:Storage__Mode = "InMemory"`.

Deixe essa janela do PowerShell aberta enquanto estiver usando a aplicação. A API informa no terminal o endereço em que está ouvindo.

## 4. Abrir a aplicação no navegador

- Aplicação: http://localhost:5197
- Swagger: http://localhost:5197/swagger

Se a porta exibida no terminal for diferente, use-a no lugar de `5197` nos endereços dos passos seguintes.

## 5. Login do admin

```powershell
$body = @{ email='admin@girassol.com'; senha='Admin@123' } | ConvertTo-Json
$resp = Invoke-RestMethod -Method Post -Uri http://localhost:5197/api/auth/login -ContentType 'application/json' -Body $body
$resp
```

## 6. Listar produtos

```powershell
Invoke-RestMethod -Uri http://localhost:5197/api/produtos
```

## 7. Criar produto (admin)

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

## 8. Criar pedido

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

## 9. Usuário padrão

- Email: `admin@girassol.com`
- Senha: `Admin@123`
