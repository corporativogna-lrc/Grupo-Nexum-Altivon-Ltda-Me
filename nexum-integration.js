/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
(function() {
  'use strict';

  const PUBLIC_API_BASE_URL = 'https://api.nexumaltivon.com.br';
  const LOCAL_API_BASE_URL = 'http://127.0.0.1:5010';
  const API_RUNTIME_STORAGE_KEY = 'nexum_api_runtime_url';

  function normalizeApiBaseUrl(value) {
    const url = String(value || '').trim().replace(/\/+$/, '');
    return /^https?:\/\//i.test(url) ? url : '';
  }

  function isLocalHost() {
    const hostname = window.location.hostname;
    return hostname === 'localhost' || hostname === '127.0.0.1' || hostname === '::1' || hostname === '';
  }

  function getConfiguredApiBaseUrl() {
    const explicit =
      normalizeApiBaseUrl(window.NEXUM_API_BASE_URL) ||
      normalizeApiBaseUrl(window.NEXUM_API_URL) ||
      normalizeApiBaseUrl(localStorage.getItem(API_RUNTIME_STORAGE_KEY));

    return explicit || (isLocalHost() ? LOCAL_API_BASE_URL : PUBLIC_API_BASE_URL);
  }

  const API_BASE_URL = getConfiguredApiBaseUrl();
  const API_URL = `${API_BASE_URL}/api`;

  // ============================================================
  // 1. BOTÕES DE ACESSO AO ADMIN
  // ============================================================
  function addAdminAccessButtons() {
    // Top bar: link discreto "Painel Admin"
    const topBar = document.querySelector('.top-bar, .topbar, [class*="top-bar"]');
    if (topBar && !document.getElementById('nexum-admin-link-topbar')) {
      const link = document.createElement('a');
      link.id = 'nexum-admin-link-topbar';
      link.href = '/admin-painel.html';
      link.innerHTML = '<i class="fas fa-lock"></i> Painel Admin';
      link.style.cssText = 'color:#C9A227;text-decoration:none;font-weight:600;font-size:0.85rem;margin-left:20px;padding:4px 12px;border:1px solid #C9A227;border-radius:4px;';
      const containers = topBar.querySelectorAll('div');
      const lastContainer = containers[containers.length - 1] || topBar;
      lastContainer.appendChild(link);
    }

    // Botão flutuante (sempre visível)
    if (!document.getElementById('nexum-admin-float-btn')) {
      const floatBtn = document.createElement('a');
      floatBtn.id = 'nexum-admin-float-btn';
      floatBtn.href = '/admin-painel.html';
      floatBtn.innerHTML = '<i class="fas fa-user-shield"></i>';
      floatBtn.title = 'Acessar Painel Administrativo';
      floatBtn.style.cssText = 'position:fixed;bottom:100px;right:30px;width:56px;height:56px;background:linear-gradient(135deg,#C9A227,#8B6F1A);color:#0A0A0A;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:1.5rem;text-decoration:none;box-shadow:0 4px 20px rgba(201,162,39,0.4);z-index:9998;transition:transform 0.3s;';
      floatBtn.onmouseover = () => { floatBtn.style.transform = 'scale(1.1)'; };
      floatBtn.onmouseout = () => { floatBtn.style.transform = 'scale(1)'; };
      document.body.appendChild(floatBtn);
    }

    // Item no menu principal
    const mainNav = document.querySelector('nav ul, .nav-menu, .main-nav, .nav-links');
    if (mainNav && !document.getElementById('nexum-admin-nav-item')) {
      const li = document.createElement('li');
      li.id = 'nexum-admin-nav-item';
      li.innerHTML = '<a href="/admin-painel.html" style="color:#C9A227;font-weight:700;"><i class="fas fa-cog"></i> ADMIN</a>';
      mainNav.appendChild(li);
    }

    // Ícone de busca → abre modal de busca de produtos
    document.querySelectorAll('.fa-search').forEach(icon => {
      const target = icon.closest('a, button, div, li');
      if (target && !target.dataset.nexumHooked) {
        target.dataset.nexumHooked = 'true';
        target.style.cursor = 'pointer';
        target.addEventListener('click', (e) => {
          e.preventDefault();
          openSearchModal();
        });
      }
    });

    // Ícone de sacola/carrinho → admin
    document.querySelectorAll('.fa-shopping-bag, .fa-shopping-cart').forEach(icon => {
      const target = icon.closest('a, button, div, li');
      if (target && !target.dataset.nexumHooked) {
        target.dataset.nexumHooked = 'true';
        target.style.cursor = 'pointer';
        target.addEventListener('click', (e) => {
          e.preventDefault();
          window.location.href = '/admin-painel.html';
        });
      }
    });
  }

  // ============================================================
  // 2. MODAL DE BUSCA DE PRODUTOS
  // ============================================================
  function openSearchModal() {
    if (document.getElementById('nexum-search-modal')) return;
    const modal = document.createElement('div');
    modal.id = 'nexum-search-modal';
    modal.innerHTML = `
      <style>
        #nexum-search-modal { position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.85);z-index:99999;display:flex;align-items:flex-start;justify-content:center;padding-top:80px;font-family:'Montserrat',sans-serif; }
        #nexum-search-box { background:#1A1A1A;border:1px solid #C9A227;border-radius:12px;padding:30px;width:90%;max-width:700px;max-height:80vh;overflow-y:auto;position:relative; }
        #nexum-search-input { width:100%;padding:14px;background:#0A0A0A;border:1px solid rgba(201,162,39,0.3);color:#F5F5F5;border-radius:8px;font-size:1.1rem;box-sizing:border-box; }
        .nexum-result { display:flex;align-items:center;padding:12px;margin-top:10px;background:rgba(255,255,255,0.03);border-radius:8px;border:1px solid rgba(201,162,39,0.1); }
        .nexum-result img { width:60px;height:60px;object-fit:cover;border-radius:6px; }
        .nexum-result-info { flex:1;margin-left:15px;color:#F5F5F5; }
        .nexum-result-info h4 { margin:0;font-size:1rem; }
        .nexum-result-info .price { color:#C9A227;font-weight:700;margin-top:4px; }
        #nexum-search-close { position:absolute;top:15px;right:20px;background:transparent;color:#F5F5F5;border:none;font-size:2rem;cursor:pointer; }
      </style>
      <div id="nexum-search-box">
        <button id="nexum-search-close">&times;</button>
        <h2 style="color:#C9A227;font-family:'Playfair Display',serif;margin:0 0 20px 0;">Buscar Produtos</h2>
        <input id="nexum-search-input" type="text" placeholder="Digite o nome do produto..." autofocus />
        <div id="nexum-search-results" style="margin-top:20px;"></div>
      </div>
    `;
    document.body.appendChild(modal);

    const closeModal = () => modal.remove();
    document.getElementById('nexum-search-close').onclick = closeModal;
    modal.addEventListener('click', (e) => { if (e.target === modal) closeModal(); });

    const input = document.getElementById('nexum-search-input');
    const results = document.getElementById('nexum-search-results');
    let debounce;
    let allProducts = null;

    input.addEventListener('input', async () => {
      clearTimeout(debounce);
      debounce = setTimeout(async () => {
        const q = input.value.trim().toLowerCase();
        if (!q) { results.innerHTML = ''; return; }

        if (!allProducts) {
          try {
            const res = await fetch(`${API_URL}/produtos?itensPorPagina=60`);
            if (!res.ok) {
              throw new Error(`API retornou HTTP ${res.status} ao buscar produtos.`);
            }
            const payload = await res.json();
            allProducts = Array.isArray(payload)
              ? payload
              : Array.isArray(payload.dados)
                ? payload.dados
                : Array.isArray(payload.data)
                  ? payload.data
                  : [];
            if (!Array.isArray(allProducts)) {
              throw new Error('API retornou payload invalido para produtos.');
            }
          } catch (error) {
            console.error('[Nexum] Falha real ao buscar produtos:', error);
            results.innerHTML = '<p style="color:#ef4444;text-align:center;padding:20px;">API indisponivel para busca de produtos. Verifique a API publica.</p>';
            return;
          }
        }

        const filtered = allProducts.filter(p =>
          String(p.nome || p.Nome || '').toLowerCase().includes(q) ||
          String(p.descricao || p.Descricao || p.descricao_curta || p.descricaoCurta || '').toLowerCase().includes(q)
        );

        if (filtered.length === 0) {
          results.innerHTML = '<p style="color:#A0A0A0;text-align:center;padding:20px;">Nenhum produto encontrado</p>';
          return;
        }

        results.innerHTML = filtered.map(p => `
          <div class="nexum-result">
            <img src="${p.imagem_url || p.imagemUrl || p.ImagemUrl || ''}" alt="${p.nome || p.Nome || 'Produto'}" onerror="this.style.display='none'" />
            <div class="nexum-result-info">
              <h4>${p.nome || p.Nome || 'Produto sem nome'}</h4>
              <p style="margin:4px 0;color:#A0A0A0;font-size:0.85rem;">${String(p.descricao || p.Descricao || p.descricao_curta || p.descricaoCurta || '').substring(0, 80)}</p>
              <div class="price">R$ ${Number(p.preco_promocional || p.precoPromocional || p.PrecoPromocional || p.preco || p.Preco || 0).toFixed(2).replace('.', ',')}</div>
            </div>
          </div>
        `).join('');
      }, 300);
    });
  }

  // ============================================================
  // 3. FORMULÁRIOS → CRM
  // ============================================================
  function showMessage(form, type, message) {
    let msgEl = form.querySelector('.form-message');
    if (!msgEl) {
      msgEl = document.createElement('div');
      msgEl.className = 'form-message';
      msgEl.style.cssText = 'padding:15px;border-radius:8px;margin-top:15px;font-weight:600;text-align:center;';
      form.appendChild(msgEl);
    }
    if (type === 'success') {
      msgEl.style.background = 'rgba(34,197,94,0.15)';
      msgEl.style.color = '#22c55e';
      msgEl.style.border = '1px solid #22c55e';
    } else {
      msgEl.style.background = 'rgba(239,68,68,0.15)';
      msgEl.style.color = '#ef4444';
      msgEl.style.border = '1px solid #ef4444';
    }
    msgEl.textContent = message;
    setTimeout(() => { msgEl.remove(); }, 6000);
  }

  function getOrigem(panelId) {
    return ({
      'form-cliente': 'Cliente VIP',
      'form-dropshipping': 'Dropshipping',
      'form-fornecedor': 'Fornecedor',
      'form-parceiro': 'Parceria Comercial'
    })[panelId] || 'Website';
  }

  async function submitForm(form) {
    const panel = form.closest('.form-panel');
    const origem = panel ? getOrigem(panel.id) : 'Website';
    const data = new FormData(form);

    const nome = String(data.get('Nome') || data.get('Empresa') || '').trim();
    const email = String(data.get('Email') || '').trim();
    const telefone = data.get('Telefone') || '';
    const empresa = data.get('Empresa') || '';

    if (!nome) {
      throw new Error('Informe o nome ou empresa para registrar o lead.');
    }

    if (!email) {
      throw new Error('Informe o e-mail para registrar o lead.');
    }

    const extras = [];
    for (const [key, value] of data.entries()) {
      if (!['Nome', 'Email', 'Telefone', 'Empresa', '_subject'].includes(key) && value) {
        extras.push(`${key}: ${value}`);
      }
    }
    const mensagem = extras.length ? extras.join(' | ') : `Interesse em ${origem}`;

    const res = await fetch(`${API_URL}/crm/leads`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ nome, email, telefone, empresa, mensagem, origem: `Site - ${origem}` })
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.detail || 'Erro ao enviar');
    }
    return res.json();
  }

  function setupForms() {
    document.querySelectorAll('.parceiros-section form, .cadastro-box-content form, form').forEach(form => {
      if (form.dataset.nexumHooked || form.id === 'nexum-login-form') return;
      form.dataset.nexumHooked = 'true';

      form.addEventListener('submit', async function(e) {
        e.preventDefault();
        const btn = form.querySelector('button[type="submit"], .submit-btn');
        const origText = btn ? btn.textContent : '';
        if (btn) { btn.disabled = true; btn.textContent = 'Enviando...'; }
        try {
          await submitForm(form);
          showMessage(form, 'success', '✓ Cadastro enviado com sucesso! Entraremos em contato em breve.');
          form.reset();
        } catch (err) {
          showMessage(form, 'error', '✗ ' + (err.message || 'Erro ao enviar. Tente novamente.'));
        } finally {
          if (btn) { btn.disabled = false; btn.textContent = origText; }
        }
      });
    });
  }

  // ============================================================
  // INIT
  // ============================================================
  document.addEventListener('DOMContentLoaded', function() {
    setupForms();
    addAdminAccessButtons();
    console.log('[Nexum] Landing integrada ao backend:', API_URL);
  });
})();
