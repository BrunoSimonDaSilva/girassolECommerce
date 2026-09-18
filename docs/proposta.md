# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Bruno · **Curso/Turma:** Engenharia de Software / 2026.2
- **Repositório:** https://github.com/usuario/girassol-commerce

## 1. Domínio e problema

O projeto Girassol é uma aplicação de e-commerce para pequenas lojas que desejam vender produtos digitais e físicos em uma plataforma simples, com gestão de catálogo e pedidos. A solução ajuda o lojista a controlar estoque, acompanhar compras e oferecer uma experiência de compra rápida para o cliente sem depender de serviços pagos.

## 2. Entidades

Entidade A: Produto
- id
- nome
- categoria
- descricao
- preco
- estoque
- ativo
- imagemUrl
- tags

Entidade B: Pedido
- id
- usuarioId
- nomeCliente
- emailCliente
- enderecoEntrega
- status
- total
- dataPedido
- itens[]

Relacionamento: um pedido pode ter vários itens e cada item referencia um produto. A modelagem será mista: a entidade Produto será armazenada como documento principal e os itens do pedido serão referenciados por id de produto, com dados de preço e quantidade embutidos no documento do pedido para garantir histórico de compra e rastreabilidade independentemente do catálogo futuro.

## 3. Endpoints previstos

### Produtos
- GET /api/produtos — lista os produtos
- GET /api/produtos/{id} — busca um produto pelo id
- POST /api/produtos — cria um produto (admin)
- PUT /api/produtos/{id} — atualiza um produto (admin)
- DELETE /api/produtos/{id} — remove um produto (admin)

### Pedidos
- GET /api/pedidos — lista pedidos do usuário ou todos os pedidos (admin)
- GET /api/pedidos/{id} — busca um pedido por id
- POST /api/pedidos — cria um pedido com cálculo de total e atualização de estoque
- PUT /api/pedidos/{id} — atualiza o status do pedido (admin)
- DELETE /api/pedidos/{id} — remove um pedido (admin)

### Autenticação
- POST /api/auth/register — registra novo usuário
- POST /api/auth/login — autentica e devolve JWT
- GET /api/auth/me — retorna dados do usuário autenticado

## 4. Regra de negócio

A regra principal da camada de serviço será validar que o pedido só pode ser criado com itens válidos, quantidade positiva e estoque suficiente. O valor total é calculado automaticamente como a soma do preço do produto multiplicado pela quantidade. Quando um pedido é criado, o estoque de cada produto é reduzido, garantindo consistência entre catálogo e pedidos.

## 5. Casos de erro

- Produto inexistente ao criar ou consultar pedido: 404 Not Found
- Quantidade inválida ou estoque insuficiente: 400 Bad Request
- Registro duplicado ou credenciais inválidas: 400/401

## 6. Stack escolhida

Backend: ASP.NET Core Web API em C#
Banco: MongoDB
Frontend: HTML + JavaScript vanilla com fetch assíncrono

## 7. Segurança e testes

A API usará JWT para autenticação e autorização por perfil, com papéis `Usuario` e `Admin`. Os testes unitários serão implementados com xUnit cobrindo cenários de sucesso e erro da regra de negócio de pedidos e validação de produtos.
