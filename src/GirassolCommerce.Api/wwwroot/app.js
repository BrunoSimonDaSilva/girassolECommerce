const app = document.getElementById('app');
const statusBox = document.getElementById('statusBox');
const state = {
  produtos: [],
  produtoSelecionado: null,
  token: localStorage.getItem('girassol-token') || '',
  cart: JSON.parse(localStorage.getItem('girassol-cart') || '[]')
};

const apiFetch = async (url, options = {}) => {
  const headers = { ...(options.headers || {}) };
  if (!(options.body instanceof FormData)) {
    headers['Content-Type'] = headers['Content-Type'] || 'application/json';
  }

  if (state.token) {
    headers.Authorization = `Bearer ${state.token}`;
  }

  const response = await fetch(url, { ...options, headers });
  const isEmpty = response.status === 204;
  const contentType = response.headers.get('content-type') || '';
  const payload = isEmpty || !contentType.includes('application/json') ? null : await response.json();

  if (!response.ok) {
    const message = payload?.message || 'Erro ao processar a requisição.';
    throw new Error(message);
  }

  return payload;
};

const showStatus = (message, isError = false) => {
  statusBox.textContent = message;
  statusBox.style.display = 'block';
  statusBox.classList.toggle('error', isError);
};

const resetStatus = () => {
  statusBox.textContent = '';
  statusBox.style.display = 'none';
  statusBox.classList.remove('error');
};

const persistCart = () => {
  localStorage.setItem('girassol-cart', JSON.stringify(state.cart));
};

const getCartTotal = () => state.cart.reduce((sum, item) => sum + Number(item.preco) * Number(item.quantidade), 0);

const addToCart = (produto, quantidade = 1) => {
  const itemExistente = state.cart.find((item) => item.produtoId === produto.id);

  if (itemExistente) {
    itemExistente.quantidade += quantidade;
  } else {
    state.cart.push({
      produtoId: produto.id,
      nome: produto.nome,
      preco: Number(produto.preco),
      quantidade: Number(quantidade)
    });
  }

  persistCart();
  showStatus(`${produto.nome} adicionado ao carrinho.`);
};

const removeFromCart = (produtoId) => {
  state.cart = state.cart.filter((item) => item.produtoId !== produtoId);
  persistCart();
  renderHome();
};

const renderCartSummary = () => {
  if (!state.cart.length) {
    return `
      <div class="card">
        <h3>Carrinho</h3>
        <p>Seu carrinho está vazio.</p>
      </div>
    `;
  }

  const total = getCartTotal();
  const itemsHtml = state.cart.map((item) => `
    <div style="display:flex;justify-content:space-between;align-items:center;gap:12px; margin-bottom:10px;">
      <div>
        <strong>${item.nome}</strong><br />
        <small>${item.quantidade}x · R$ ${Number(item.preco).toFixed(2)}</small>
      </div>
      <button class="button secondary" data-remove-item="${item.produtoId}" type="button">Remover</button>
    </div>
  `).join('');

  return `
    <div class="card">
      <h3>Carrinho</h3>
      ${itemsHtml}
      <hr />
      <p><strong>Total:</strong> R$ ${Number(total).toFixed(2)}</p>
      <button class="button" id="checkoutCartBtn" type="button">Finalizar compra</button>
    </div>
  `;
};

const renderHome = async () => {
  resetStatus();
  app.innerHTML = `
    <section class="hero">
      <div>
        <h1>Loja do Girassol</h1>
        <p>Descubra itens selecionados para o seu próximo pedido.</p>
      </div>
      <div>
        <button class="button secondary" id="loadProductsBtn">Atualizar catálogo</button>
      </div>
    </section>
    <div id="cartSummary">${renderCartSummary()}</div>
    <div id="productGrid" class="grid"></div>
  `;

  const removeButtons = document.querySelectorAll('[data-remove-item]');
  removeButtons.forEach((button) => {
    button.addEventListener('click', () => removeFromCart(button.dataset.removeItem));
  });

  const checkoutCartBtn = document.getElementById('checkoutCartBtn');
  if (checkoutCartBtn) {
    checkoutCartBtn.addEventListener('click', () => {
      const items = state.cart.map((item) => ({
        produtoId: item.produtoId,
        nome: item.nome,
        preco: item.preco,
        quantidade: item.quantidade
      }));

      if (!items.length) {
        showStatus('Seu carrinho está vazio.', true);
        return;
      }

      renderCartCheckout(items);
    });
  }

  document.getElementById('loadProductsBtn').addEventListener('click', loadProdutos);
  await loadProdutos();
};

const renderProductCard = (produto) => `
  <article class="card">
    <img class="product-image" src="${produto.imagemUrl || 'https://placehold.co/600x400?text=Girassol'}" alt="${produto.nome}" />
    <h3>${produto.nome}</h3>
    <p>${produto.categoria}</p>
    <p>${produto.descricao}</p>
    <div class="price">R$ ${Number(produto.preco).toFixed(2)}</div>
    <p>Estoque: ${produto.estoque}</p>
    <div style="display:flex; gap:8px; flex-wrap:wrap;">
      <button class="button" data-id="${produto.id}" data-action="detail">Ver detalhe</button>
      <button class="button secondary" data-id="${produto.id}" data-action="add-cart">Adicionar ao carrinho</button>
    </div>
  </article>
`;

const loadProdutos = async () => {
  try {
    const produtosRaw = await apiFetch('/api/produtos');
    // debug: inspecionar a resposta no console do navegador
    console.log('produtosRaw', produtosRaw);

    // Normalizar diferentes formatos de resposta possíveis
    let produtosArray = [];
    if (Array.isArray(produtosRaw)) {
      produtosArray = produtosRaw;
    } else if (produtosRaw && Array.isArray(produtosRaw.value)) {
      produtosArray = produtosRaw.value;
    } else if (produtosRaw && Array.isArray(produtosRaw.data)) {
      produtosArray = produtosRaw.data;
    } else if (produtosRaw && produtosRaw.items && Array.isArray(produtosRaw.items)) {
      produtosArray = produtosRaw.items;
    } else {
      // fallback: vazio
      produtosArray = [];
    }

    const normalize = (p) => ({
      id: (p.id || p._id || p.Id || (p._id && p._id.$oid) || '').toString(),
      nome: p.nome || p.Nome || p.name || p.Name || '',
      descricao: p.descricao || p.Descricao || p.description || p.Description || '',
      categoria: p.categoria || p.Categoria || p.category || p.Category || '',
      preco: Number(p.preco ?? p.Preco ?? p.price ?? p.Price ?? 0),
      estoque: Number(p.estoque ?? p.Estoque ?? p.stock ?? p.Stock ?? 0),
      imagemUrl: p.imagemUrl || p.ImagemUrl || p.imageUrl || p.ImageUrl || '',
      tags: p.tags || p.Tags || [] ,
      ativo: p.ativo ?? p.Ativo ?? true
    });

    state.produtos = produtosArray.map(normalize);

    const grid = document.getElementById('productGrid');
    if (!grid) return;

    grid.innerHTML = state.produtos.map(renderProductCard).join('');

    grid.querySelectorAll('[data-action="detail"]').forEach((button) => {
      button.addEventListener('click', async () => {
        const id = button.dataset.id;
        const produto = state.produtos.find((item) => item.id === id);
        if (produto) {
          renderProdutoDetalhe(produto);
        }
      });
    });

    grid.querySelectorAll('[data-action="add-cart"]').forEach((button) => {
      button.addEventListener('click', () => {
        const id = button.dataset.id;
        const produto = state.produtos.find((item) => item.id === id);
        if (produto) {
          addToCart(produto, 1);
          renderHome();
        }
      });
    });
  } catch (error) {
    console.error(error);
    showStatus(error.message || String(error), true);
  }
};

const renderProdutoDetalhe = (produto) => {
  state.produtoSelecionado = produto;
  app.innerHTML = `
    <div class="card">
      <button class="button secondary" id="backToCatalogBtn">Voltar para catálogo</button>
      <h2>${produto.nome}</h2>
      <img class="product-image" src="${produto.imagemUrl || 'https://placehold.co/600x400?text=Girassol'}" alt="${produto.nome}" />
      <p><strong>Categoria:</strong> ${produto.categoria}</p>
      <p>${produto.descricao}</p>
      <p><strong>Preço:</strong> <span class="price">R$ ${Number(produto.preco).toFixed(2)}</span></p>
      <p><strong>Estoque:</strong> ${produto.estoque}</p>
      <div style="display:flex; gap:8px; flex-wrap:wrap;">
        <button class="button" id="addToCartBtn">Adicionar ao carrinho</button>
        <button class="button secondary" id="buyNowBtn">Comprar agora</button>
      </div>
    </div>
  `;

  document.getElementById('backToCatalogBtn').addEventListener('click', renderHome);
  document.getElementById('addToCartBtn').addEventListener('click', () => {
    addToCart(produto, 1);
    renderHome();
  });
  document.getElementById('buyNowBtn').addEventListener('click', () => renderCheckoutForm(produto));
};

const renderCartCheckout = (items) => {
  app.innerHTML = `
    <div class="card">
      <h2>Finalizar compra do carrinho</h2>
      <form id="checkoutForm" class="inline-form">
        ${items.map((item, index) => `
          <div style="border:1px solid #ddd; border-radius:8px; padding:12px;">
            <strong>${item.nome}</strong>
            <input type="hidden" name="produtoId_${index}" value="${item.produtoId}" />
            <label>
              Quantidade
              <input type="number" name="quantidade_${index}" min="1" max="${item.estoque || 99}" value="${item.quantidade}" required />
            </label>
          </div>
        `).join('')}
        <label>
          Nome completo
          <input type="text" name="nomeCliente" placeholder="Seu nome" required />
        </label>
        <label>
          Email
          <input type="email" name="emailCliente" placeholder="voce@email.com" required />
        </label>
        <label>
          Endereço de entrega
          <textarea name="enderecoEntrega" rows="3" required></textarea>
        </label>
        <button type="submit" class="button">Confirmar pedido</button>
        <button type="button" class="button secondary" id="cancelCheckoutBtn">Cancelar</button>
      </form>
    </div>
  `;

  document.getElementById('cancelCheckoutBtn').addEventListener('click', renderHome);
  document.getElementById('checkoutForm').addEventListener('submit', async (event) => {
    event.preventDefault();
    const formData = new FormData(event.target);
    const productInputs = items.map((item, index) => ({
      produtoId: formData.get(`produtoId_${index}`).toString(),
      quantidade: Number(formData.get(`quantidade_${index}`))
    })).filter((item) => item.quantidade > 0);

    const payload = {
      nomeCliente: formData.get('nomeCliente').toString(),
      emailCliente: formData.get('emailCliente').toString(),
      enderecoEntrega: formData.get('enderecoEntrega').toString(),
      itens: productInputs
    };

    try {
      const pedido = await apiFetch('/api/pedidos', {
        method: 'POST',
        body: JSON.stringify(payload)
      });

      state.cart = [];
      persistCart();
      showStatus(`Pedido criado com sucesso! ID: ${pedido.id}`);
      setTimeout(() => renderHome(), 1000);
    } catch (error) {
      showStatus(error.message, true);
    }
  });
};

const renderCheckoutForm = (produto) => {
  const item = {
    produtoId: produto.id,
    nome: produto.nome,
    preco: Number(produto.preco),
    quantidade: 1,
    estoque: Number(produto.estoque)
  };
  renderCartCheckout([item]);
};

const renderLoginForm = () => {
  app.innerHTML = `
    <div class="card">
      <h2>Entrar</h2>
      <form id="loginForm" class="inline-form">
        <label>
          Email
          <input type="email" name="email" required />
        </label>
        <label>
          Senha
          <input type="password" name="senha" required />
        </label>
        <button type="submit" class="button">Login</button>
      </form>
    </div>
  `;

  document.getElementById('loginForm').addEventListener('submit', async (event) => {
    event.preventDefault();
    const formData = new FormData(event.target);
    try {
      const payload = {
        email: formData.get('email').toString(),
        senha: formData.get('senha').toString()
      };

      const response = await apiFetch('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(payload)
      });

      state.token = response.token;
      localStorage.setItem('girassol-token', state.token);
      showStatus(`Login realizado com sucesso. Bem-vindo(a), ${response.nome}!`);
      setTimeout(() => renderHome(), 800);
    } catch (error) {
      showStatus(error.message, true);
    }
  });
};

const renderRegisterForm = () => {
  app.innerHTML = `
    <div class="card">
      <h2>Registrar</h2>
      <form id="registerForm" class="inline-form">
        <label>
          Nome
          <input type="text" name="nome" required />
        </label>
        <label>
          Email
          <input type="email" name="email" required />
        </label>
        <label>
          Senha
          <input type="password" name="senha" required />
        </label>
        <label>
          Perfil
          <select name="role">
            <option value="Usuario">Usuario</option>
            <option value="Admin">Admin</option>
          </select>
        </label>
        <button type="submit" class="button">Criar conta</button>
      </form>
    </div>
  `;

  document.getElementById('registerForm').addEventListener('submit', async (event) => {
    event.preventDefault();
    const formData = new FormData(event.target);
    try {
      const payload = {
        nome: formData.get('nome').toString(),
        email: formData.get('email').toString(),
        senha: formData.get('senha').toString(),
        role: formData.get('role').toString()
      };

      await apiFetch('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(payload)
      });

      showStatus('Cadastro realizado com sucesso! Use o formulário de login.');
      setTimeout(() => renderLoginForm(), 800);
    } catch (error) {
      showStatus(error.message, true);
    }
  });
};

document.getElementById('homeBtn').addEventListener('click', renderHome);
document.getElementById('loginBtn').addEventListener('click', renderLoginForm);
document.getElementById('registerBtn').addEventListener('click', renderRegisterForm);

renderHome();
