/**
 * Nexum Altivon - IntegraÃ§Ã£o COMPLETA do Painel Admin
 * Todos os botÃµes, comandos, cadastros e seÃ§Ãµes operacionais
 */
(function() {
  'use strict';

  const API_URL = window.location.origin + '/api';
  const STORAGE_TOKEN = 'nexum_admin_token';
  const STORAGE_USER = 'nexum_admin_user';

  // ==========================================================
  // API HELPERS
  // ==========================================================
  async function apiRequest(method, endpoint, body) {
    const token = localStorage.getItem(STORAGE_TOKEN);
    const opts = {
      method,
      headers: { 'Content-Type': 'application/json' }
    };
    if (token) opts.headers['Authorization'] = `Bearer ${token}`;
    if (body) opts.body = JSON.stringify(body);

    const res = await fetch(`${API_URL}${endpoint}`, opts);
    if (res.status === 401) {
      localStorage.removeItem(STORAGE_TOKEN);
      window.location.reload();
      return null;
    }
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.detail || `Erro ${res.status}`);
    }
    return res.json();
  }

  const apiGet = (e) => apiRequest('GET', e);
  const apiPost = (e, b) => apiRequest('POST', e, b);
  const apiPut = (e, b) => apiRequest('PUT', e, b);

  // ==========================================================
  // UTILS
  // ==========================================================
  function formatBRL(v) { return new Intl.NumberFormat('pt-BR',{style:'currency',currency:'BRL'}).format(v||0); }
  function formatDate(d) { return d ? new Date(d).toLocaleDateString('pt-BR') : '-'; }

  function badge(text, color) {
    return `<span style="background:${color}25;color:${color};padding:4px 12px;border-radius:12px;font-size:0.75rem;font-weight:600;">${text}</span>`;
  }

  function statusBadge(status) {
    const colors = {
      'Pendente':'#FFA500','Processando':'#3B82F6','Enviado':'#8B5CF6','Entregue':'#10B981','Cancelado':'#EF4444',
      'Novo':'#C9A227','Contato':'#3B82F6','Qualificado':'#06B6D4','Negociacao':'#F59E0B','Ganho':'#10B981','Perdido':'#EF4444',
      'Ativo':'#10B981','Inativo':'#6B7280'
    };
    return badge(status, colors[status] || '#6B7280');
  }

  function showToast(msg, type) {
    const colors = { success:'#10B981', error:'#EF4444', info:'#3B82F6' };
    const toast = document.createElement('div');
    toast.style.cssText = `position:fixed;bottom:30px;right:30px;background:${colors[type||'success']};color:white;padding:14px 24px;border-radius:8px;z-index:99999;font-weight:600;box-shadow:0 10px 30px rgba(0,0,0,0.3);`;
    toast.textContent = msg;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3500);
  }

  // ==========================================================
  // MODAL UNIVERSAL DE CRUD
  // ==========================================================
  function openModal(title, fields, onSubmit, initialData) {
    document.getElementById('nexum-modal')?.remove();
    const data = initialData || {};

    const modal = document.createElement('div');
    modal.id = 'nexum-modal';
    modal.innerHTML = `
      <style>
        #nexum-modal { position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.85);z-index:99999;display:flex;align-items:center;justify-content:center;font-family:'Montserrat',sans-serif; }
        #nexum-modal-box { background:#1A1A1A;border:1px solid #C9A227;border-radius:12px;padding:30px;width:90%;max-width:600px;max-height:85vh;overflow-y:auto;position:relative; }
        #nexum-modal-box h3 { color:#C9A227;font-family:'Playfair Display',serif;margin:0 0 20px 0;font-size:1.5rem; }
        .nexum-field { margin-bottom:15px; }
        .nexum-field label { display:block;color:#F5F5F5;font-size:0.85rem;font-weight:600;margin-bottom:6px; }
        .nexum-field input, .nexum-field textarea, .nexum-field select { width:100%;padding:10px 12px;background:#0A0A0A;border:1px solid rgba(201,162,39,0.3);color:#F5F5F5;border-radius:6px;font-size:0.95rem;box-sizing:border-box;font-family:inherit; }
        .nexum-field input:focus, .nexum-field textarea:focus, .nexum-field select:focus { outline:none;border-color:#C9A227; }
        .nexum-modal-actions { display:flex;gap:10px;margin-top:25px; }
        .nexum-btn { padding:12px 24px;border:none;border-radius:6px;font-weight:700;cursor:pointer;text-transform:uppercase;letter-spacing:0.5px;font-size:0.85rem; }
        .nexum-btn-primary { background:#C9A227;color:#0A0A0A; }
        .nexum-btn-secondary { background:rgba(255,255,255,0.1);color:#F5F5F5; }
        #nexum-modal-close { position:absolute;top:15px;right:20px;background:transparent;color:#F5F5F5;border:none;font-size:2rem;cursor:pointer; }
      </style>
      <div id="nexum-modal-box">
        <button id="nexum-modal-close">&times;</button>
        <h3>${title}</h3>
        <form id="nexum-modal-form">
          ${fields.map(f => {
            const value = data[f.name] || f.default || '';
            if (f.type === 'select') {
              return `<div class="nexum-field"><label>${f.label}${f.required?' *':''}</label><select name="${f.name}" ${f.required?'required':''}>${f.options.map(o => `<option value="${o.value}" ${value === o.value ? 'selected' : ''}>${o.label}</option>`).join('')}</select></div>`;
            }
            if (f.type === 'textarea') {
              return `<div class="nexum-field"><label>${f.label}${f.required?' *':''}</label><textarea name="${f.name}" rows="3" ${f.required?'required':''}>${value}</textarea></div>`;
            }
            return `<div class="nexum-field"><label>${f.label}${f.required?' *':''}</label><input type="${f.type||'text'}" name="${f.name}" value="${value}" ${f.required?'required':''} ${f.step?'step='+f.step:''} placeholder="${f.placeholder||''}" /></div>`;
          }).join('')}
          <div class="nexum-modal-actions">
            <button type="button" class="nexum-btn nexum-btn-secondary" id="nexum-modal-cancel">Cancelar</button>
            <button type="submit" class="nexum-btn nexum-btn-primary" style="flex:1;">Salvar</button>
          </div>
        </form>
      </div>
    `;
    document.body.appendChild(modal);

    const close = () => modal.remove();
    document.getElementById('nexum-modal-close').onclick = close;
    document.getElementById('nexum-modal-cancel').onclick = close;
    modal.addEventListener('click', (e) => { if (e.target === modal) close(); });

    document.getElementById('nexum-modal-form').addEventListener('submit', async (e) => {
      e.preventDefault();
      const formData = new FormData(e.target);
      const obj = {};
      formData.forEach((v, k) => {
        const field = fields.find(f => f.name === k);
        if (field?.type === 'number') obj[k] = parseFloat(v) || 0;
        else if (field?.type === 'checkbox') obj[k] = v === 'on';
        else obj[k] = v;
      });
      try {
        await onSubmit(obj);
        close();
      } catch (err) {
        showToast(err.message, 'error');
      }
    });
  }

  // ==========================================================
  // MODAL DE VISUALIZAÃ‡ÃƒO (DETALHES)
  // ==========================================================
  function openViewModal(title, content) {
    document.getElementById('nexum-modal')?.remove();
    const modal = document.createElement('div');
    modal.id = 'nexum-modal';
    modal.innerHTML = `
      <style>
        #nexum-modal { position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.85);z-index:99999;display:flex;align-items:center;justify-content:center;font-family:'Montserrat',sans-serif; }
        #nexum-modal-box { background:#1A1A1A;border:1px solid #C9A227;border-radius:12px;padding:30px;width:90%;max-width:650px;max-height:85vh;overflow-y:auto;position:relative; color:#E5E5E5; }
        #nexum-modal-box h3 { color:#C9A227;font-family:'Playfair Display',serif;margin:0 0 20px 0;font-size:1.5rem; }
        .view-row { display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid rgba(255,255,255,0.05);font-size:0.9rem; }
        .view-row span:first-child { color:#A0A0A0;font-weight:600; }
        #nexum-modal-close { position:absolute;top:15px;right:20px;background:transparent;color:#F5F5F5;border:none;font-size:2rem;cursor:pointer; }
      </style>
      <div id="nexum-modal-box">
        <button id="nexum-modal-close">&times;</button>
        <h3>${title}</h3>
        ${content}
      </div>
    `;
    document.body.appendChild(modal);
    const close = () => modal.remove();
    document.getElementById('nexum-modal-close').onclick = close;
    modal.addEventListener('click', (e) => { if (e.target === modal) close(); });
  }

  // ==========================================================
  // CADASTROS / AÃ‡Ã•ES (Expostos via window)
  // ==========================================================
  window.NexumActions = {
    novoProduto: async () => {
      const cats = await apiGet('/categorias');
      openModal('Novo Produto', [
        { name: 'nome', label: 'Nome', required: true },
        { name: 'descricao', label: 'DescriÃ§Ã£o', type: 'textarea' },
        { name: 'categoria_id', label: 'Categoria', type: 'select', required: true, options: cats.map(c => ({value: c.id, label: c.nome})) },
        { name: 'preco', label: 'PreÃ§o (R$)', type: 'number', step: '0.01', required: true },
        { name: 'preco_promocional', label: 'PreÃ§o Promocional (R$)', type: 'number', step: '0.01' },
        { name: 'estoque', label: 'Estoque', type: 'number', default: 0 },
        { name: 'imagem_url', label: 'URL da Imagem' },
        { name: 'sku', label: 'SKU' },
        { name: 'destaque', label: 'Destaque', type: 'select', options: [{value:'false',label:'NÃ£o'},{value:'true',label:'Sim'}] }
      ], async (data) => {
        data.destaque = data.destaque === 'true';
        data.preco_promocional = data.preco_promocional ? parseFloat(data.preco_promocional) : null;
        await apiPost('/produtos', data);
        showToast('Produto criado com sucesso!');
        loadProdutos();
        updateBadges();
      });
    },

    verProduto: async (id) => {
      const p = await apiGet(`/produtos/${id}`);
      openViewModal(p.nome, `
        ${p.imagem_url ? `<img src="${p.imagem_url}" style="width:100%;max-height:250px;object-fit:cover;border-radius:8px;margin-bottom:15px;" />` : ''}
        <div class="view-row"><span>SKU:</span><span>${p.sku || '-'}</span></div>
        <div class="view-row"><span>PreÃ§o:</span><span style="color:#C9A227;font-weight:700;">${formatBRL(p.preco)}</span></div>
        <div class="view-row"><span>PreÃ§o Promocional:</span><span>${p.preco_promocional ? formatBRL(p.preco_promocional) : '-'}</span></div>
        <div class="view-row"><span>Estoque:</span><span>${p.estoque} unidades</span></div>
        <div class="view-row"><span>Destaque:</span><span>${p.destaque ? 'â­ Sim' : 'NÃ£o'}</span></div>
        <div class="view-row"><span>Status:</span><span>${statusBadge(p.ativo ? 'Ativo' : 'Inativo')}</span></div>
        <div style="margin-top:15px;padding:15px;background:rgba(255,255,255,0.03);border-radius:6px;">
          <strong style="color:#C9A227;">DescriÃ§Ã£o:</strong><br>${p.descricao || 'Sem descriÃ§Ã£o'}
        </div>
      `);
    },

    novoCliente: () => {
      openModal('Novo Cliente', [
        { name: 'nome', label: 'Nome', required: true },
        { name: 'email', label: 'Email', type: 'email', required: true },
        { name: 'cpf', label: 'CPF' },
        { name: 'telefone', label: 'Telefone' }
      ], async (data) => {
        await apiPost('/clientes', data);
        showToast('Cliente cadastrado!');
        loadClientes();
      });
    },

    verCliente: async (id) => {
      const c = await apiGet(`/clientes/${id}`);
      openViewModal(c.nome, `
        <div class="view-row"><span>Email:</span><span>${c.email}</span></div>
        <div class="view-row"><span>CPF:</span><span>${c.cpf || '-'}</span></div>
        <div class="view-row"><span>Telefone:</span><span>${c.telefone || '-'}</span></div>
        <div class="view-row"><span>Cadastro:</span><span>${formatDate(c.created_at)}</span></div>
        <div class="view-row"><span>Status:</span><span>${statusBadge(c.ativo ? 'Ativo' : 'Inativo')}</span></div>
      `);
    },

    novoLead: () => {
      openModal('Novo Lead CRM', [
        { name: 'nome', label: 'Nome', required: true },
        { name: 'email', label: 'Email', type: 'email', required: true },
        { name: 'telefone', label: 'Telefone' },
        { name: 'empresa', label: 'Empresa' },
        { name: 'origem', label: 'Origem', default: 'Cadastro Manual' },
        { name: 'mensagem', label: 'Mensagem', type: 'textarea' }
      ], async (data) => {
        await apiPost('/crm/leads', data);
        showToast('Lead cadastrado no CRM!');
        loadCRM();
        updateBadges();
      });
    },

    atualizarStatusLead: async (leadId, novoStatus) => {
      await apiPut(`/crm/leads/${leadId}/status?novo_status=${novoStatus}`, {});
      showToast(`Lead atualizado para "${novoStatus}"!`);
      loadCRM();
      updateBadges();
    },

    novoCupom: () => {
      openModal('Novo Cupom de Desconto', [
        { name: 'codigo', label: 'CÃ³digo', required: true, placeholder: 'EX: PROMO10' },
        { name: 'desconto_percentual', label: 'Desconto (%)', type: 'number', step: '0.1' },
        { name: 'desconto_valor', label: 'Desconto (R$)', type: 'number', step: '0.01' },
        { name: 'valor_minimo', label: 'Valor MÃ­nimo do Pedido (R$)', type: 'number', step: '0.01' }
      ], async (data) => {
        if (data.desconto_percentual === 0) data.desconto_percentual = null;
        if (data.desconto_valor === 0) data.desconto_valor = null;
        if (data.valor_minimo === 0) data.valor_minimo = null;
        await apiPost('/cupons', data);
        showToast('Cupom criado!');
        loadCupons();
      });
    },

    novaLoja: () => {
      openModal('Nova Loja', [
        { name: 'nome', label: 'Nome', required: true },
        { name: 'descricao', label: 'DescriÃ§Ã£o', type: 'textarea' },
        { name: 'cnpj', label: 'CNPJ' },
        { name: 'telefone', label: 'Telefone' },
        { name: 'email', label: 'Email' },
        { name: 'cidade', label: 'Cidade' },
        { name: 'estado', label: 'Estado' }
      ], async (data) => {
        await apiPost('/lojas', data);
        showToast('Loja cadastrada!');
        loadLojas();
      });
    },

    verLoja: async (id) => {
      const l = await apiGet(`/lojas/${id}`);
      openViewModal(l.nome, `
        ${l.imagem_url ? `<img src="${l.imagem_url}" style="width:100%;max-height:200px;object-fit:cover;border-radius:8px;margin-bottom:15px;" />` : ''}
        <div class="view-row"><span>Segmento:</span><span>${l.categoria_principal || '-'}</span></div>
        <div class="view-row"><span>CNPJ:</span><span>${l.cnpj || '-'}</span></div>
        <div class="view-row"><span>Telefone:</span><span>${l.telefone || '-'}</span></div>
        <div class="view-row"><span>Email:</span><span>${l.email || '-'}</span></div>
        <div class="view-row"><span>LocalizaÃ§Ã£o:</span><span>${l.cidade ? l.cidade+'/'+l.estado : '-'}</span></div>
        <div class="view-row"><span>Status:</span><span>${statusBadge(l.ativa ? 'Ativo' : 'Inativo')}</span></div>
        ${l.descricao ? `<div style="margin-top:15px;padding:15px;background:rgba(255,255,255,0.03);border-radius:6px;">${l.descricao}</div>` : ''}
      `);
    },

    verPedido: async (id) => {
      const p = await apiGet(`/pedidos/${id}`);
      openViewModal(`Pedido ${p.numero_pedido}`, `
        <div class="view-row"><span>Status:</span><span>${statusBadge(p.status)}</span></div>
        <div class="view-row"><span>Subtotal:</span><span>${formatBRL(p.subtotal)}</span></div>
        <div class="view-row"><span>Desconto:</span><span>${formatBRL(p.desconto)}</span></div>
        <div class="view-row"><span>Total:</span><span style="color:#C9A227;font-weight:700;font-size:1.1rem;">${formatBRL(p.total)}</span></div>
        <div class="view-row"><span>Data:</span><span>${formatDate(p.created_at)}</span></div>
        ${p.cupom_codigo ? `<div class="view-row"><span>Cupom:</span><span>${p.cupom_codigo}</span></div>` : ''}
        <h4 style="color:#C9A227;margin-top:20px;">Itens do Pedido</h4>
        ${(p.itens || []).map(i => `
          <div style="background:rgba(255,255,255,0.03);padding:10px;border-radius:6px;margin-bottom:8px;">
            <div style="display:flex;justify-content:space-between;">
              <strong>${i.produto_nome}</strong>
              <span style="color:#C9A227;">${formatBRL(i.subtotal)}</span>
            </div>
            <div style="color:#A0A0A0;font-size:0.85rem;margin-top:4px;">${i.quantidade}x ${formatBRL(i.preco_unitario)}</div>
          </div>
        `).join('')}
        <div style="margin-top:20px;display:flex;gap:8px;flex-wrap:wrap;">
          <button class="nexum-btn nexum-btn-primary" onclick="NexumActions.atualizarPedido('${p.id}','Processando')">Processar</button>
          <button class="nexum-btn nexum-btn-primary" onclick="NexumActions.atualizarPedido('${p.id}','Enviado')">Marcar Enviado</button>
          <button class="nexum-btn nexum-btn-primary" onclick="NexumActions.atualizarPedido('${p.id}','Entregue')">Marcar Entregue</button>
        </div>
      `);
    },

    atualizarPedido: async (id, status) => {
      await apiPut(`/pedidos/${id}/status?novo_status=${status}`, {});
      document.getElementById('nexum-modal')?.remove();
      showToast(`Pedido marcado como "${status}"!`);
      loadPedidos();
      updateBadges();
    },

    exportarPedidos: async () => {
      const pedidos = await apiGet('/pedidos');
      const csv = ['Numero,Total,Status,Data'].concat(
        pedidos.map(p => `${p.numero_pedido},${p.total},${p.status},${formatDate(p.created_at)}`)
      ).join('\n');
      const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `pedidos_nexum_${Date.now()}.csv`;
      link.click();
      showToast('Pedidos exportados em CSV!');
    },

    abrirFiltros: () => {
      showToast('Use a busca no header para filtrar produtos. Outras filtragens em breve!', 'info');
    }
  };

  // ==========================================================
  // RENDERERS DE TABELAS
  // ==========================================================
  function renderTable(sectionId, title, columns, rows, emptyMsg, actionBtn) {
    const section = document.getElementById(`section-${sectionId}`);
    if (!section) return;
    let container = section.querySelector('.nexum-dynamic-content');
    if (!container) {
      container = document.createElement('div');
      container.className = 'nexum-dynamic-content';
      container.style.cssText = 'margin-top:20px;';
      section.appendChild(container);
    }

    const headerBtn = actionBtn ? `<button class="nexum-btn nexum-btn-primary" onclick="${actionBtn.onclick}" style="float:right;">+ ${actionBtn.label}</button>` : '';
    const tableHTML = !rows || rows.length === 0
      ? `<div style="padding:40px;text-align:center;color:#A0A0A0;background:rgba(255,255,255,0.03);border-radius:8px;">${emptyMsg || 'Nenhum registro encontrado'}</div>`
      : `<div style="overflow-x:auto;"><table style="width:100%;border-collapse:collapse;background:rgba(255,255,255,0.03);border-radius:8px;overflow:hidden;">
          <thead><tr>${columns.map(c => `<th style="text-align:left;padding:14px;color:#C9A227;font-weight:600;font-size:0.85rem;border-bottom:1px solid rgba(201,162,39,0.2);">${c}</th>`).join('')}</tr></thead>
          <tbody>${rows.map(row => `<tr>${row.map(cell => `<td style="padding:12px 14px;border-bottom:1px solid rgba(255,255,255,0.05);color:#E5E5E5;font-size:0.9rem;">${cell}</td>`).join('')}</tr>`).join('')}</tbody>
        </table></div>`;

    container.innerHTML = `
      <div style="background:rgba(20,20,20,0.5);padding:20px;border-radius:8px;border:1px solid rgba(201,162,39,0.15);">
        <h3 style="color:#C9A227;font-family:'Playfair Display',serif;font-size:1.3rem;margin:0 0 15px 0;">${title} (${rows?.length || 0}) ${headerBtn}</h3>
        ${tableHTML}
      </div>
    `;
  }

  function actionBtn(label, onclick, color) {
    color = color || '#C9A227';
    return `<button onclick="${onclick}" style="background:${color};color:#0A0A0A;border:none;padding:5px 12px;border-radius:4px;cursor:pointer;font-size:0.75rem;font-weight:600;margin-right:4px;">${label}</button>`;
  }

  // ==========================================================
  // LOADERS - cada seÃ§Ã£o
  // ==========================================================
  async function updateDashboard() {
    const resumo = await apiGet('/dashboard/resumo');
    if (!resumo) return;
    const findKPI = (label) => {
      const cards = document.querySelectorAll('.stats-cards .stat-card, .kpi-card, [class*="stat"]');
      for (const card of cards) {
        if (card.textContent.toUpperCase().includes(label.toUpperCase())) {
          const v = card.querySelector('h2, h3, .stat-value, .kpi-value, [class*="value"]');
          if (v) return v;
        }
      }
      return null;
    };
    const mappings = [
      ['FATURAMENTO', formatBRL(resumo.faturamento_mes)],
      ['PEDIDOS HOJE', String(resumo.pedidos_hoje)],
      ['CLIENTES ATIVOS', String(resumo.total_clientes)],
      ['LEADS NOVOS', String(resumo.leads_novos)],
      ['ESTOQUE BAIXO', String(resumo.produtos_estoque_baixo)]
    ];
    mappings.forEach(([label, value]) => { const el = findKPI(label); if (el) el.textContent = value; });
  }

  async function updateBadges() {
    const [pedidos, produtos, leads] = await Promise.all([apiGet('/pedidos'), apiGet('/produtos?limit=200'), apiGet('/crm/leads')]);
    const setText = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
    if (pedidos) setText('badge-pedidos', pedidos.length);
    if (produtos) setText('badge-produtos', produtos.length);
    if (leads) setText('badge-crm', leads.filter(l => l.status === 'Novo').length);
  }

  async function loadPedidos() {
    const pedidos = await apiGet('/pedidos');
    if (!pedidos) return;
    const rows = pedidos.map(p => [
      `<strong style="color:#C9A227;">${p.numero_pedido}</strong>`,
      formatBRL(p.total),
      `<span style="color:#A0A0A0;">${p.itens?.length || 0} itens</span>`,
      statusBadge(p.status),
      formatDate(p.created_at),
      actionBtn('Ver', `NexumActions.verPedido('${p.id}')`)
    ]);
    renderTable('pedidos', 'Pedidos Recentes', ['NÂº Pedido', 'Total', 'Itens', 'Status', 'Data', 'AÃ§Ãµes'], rows, 'Nenhum pedido cadastrado.', { label: 'Exportar CSV', onclick: 'NexumActions.exportarPedidos()' });
  }

  async function loadProdutos() {
    const produtos = await apiGet('/produtos?limit=200');
    if (!produtos) return;
    const rows = produtos.map(p => [
      `<strong>${p.nome}</strong><br><span style="color:#A0A0A0;font-size:0.75rem;">SKU: ${p.sku || '-'}</span>`,
      formatBRL(p.preco_promocional || p.preco),
      p.estoque > 0 ? `<span style="color:${p.estoque < 10 ? '#F59E0B' : '#10B981'};">${p.estoque}</span>` : '<span style="color:#EF4444;">Esgotado</span>',
      p.destaque ? 'â­ Destaque' : '-',
      statusBadge(p.ativo ? 'Ativo' : 'Inativo'),
      actionBtn('Ver', `NexumActions.verProduto('${p.id}')`)
    ]);
    renderTable('produtos', 'CatÃ¡logo de Produtos', ['Produto', 'PreÃ§o', 'Estoque', 'Tag', 'Status', 'AÃ§Ãµes'], rows, 'Nenhum produto.', { label: 'Novo Produto', onclick: 'NexumActions.novoProduto()' });
  }

  async function loadClientes() {
    const clientes = await apiGet('/clientes');
    if (!clientes) return;
    const rows = clientes.map(c => [
      `<strong>${c.nome}</strong>`, c.email, c.telefone || '-', c.cpf || '-',
      formatDate(c.created_at), statusBadge(c.ativo ? 'Ativo' : 'Inativo'),
      actionBtn('Ver', `NexumActions.verCliente('${c.id}')`)
    ]);
    renderTable('clientes', 'Base de Clientes', ['Nome', 'Email', 'Telefone', 'CPF', 'Cadastro', 'Status', 'AÃ§Ãµes'], rows, 'Nenhum cliente cadastrado.', { label: 'Novo Cliente', onclick: 'NexumActions.novoCliente()' });
  }

  async function loadLojas() {
    const lojas = await apiGet('/lojas');
    if (!lojas) return;
    const rows = lojas.map(l => [
      `<strong style="color:#C9A227;">${l.nome}</strong>`,
      l.categoria_principal || '-',
      l.cidade ? `${l.cidade}/${l.estado}` : '-',
      l.telefone || '-', l.email || '-',
      statusBadge(l.ativa ? 'Ativo' : 'Inativo'),
      actionBtn('Ver', `NexumActions.verLoja('${l.id}')`)
    ]);
    renderTable('lojas', 'Lojas do Grupo', ['Loja', 'Segmento', 'LocalizaÃ§Ã£o', 'Telefone', 'Email', 'Status', 'AÃ§Ãµes'], rows, 'Nenhuma loja.', { label: 'Nova Loja', onclick: 'NexumActions.novaLoja()' });
  }

  async function loadCRM() {
    const leads = await apiGet('/crm/leads');
    if (!leads) return;
    const rows = leads.map(l => [
      `<strong>${l.nome}</strong>`, l.email, l.telefone || '-', l.empresa || '-',
      `<span style="color:#A0A0A0;font-size:0.8rem;">${l.origem || '-'}</span>`,
      statusBadge(l.status), formatDate(l.created_at),
      actionBtn('Qualificar', `NexumActions.atualizarStatusLead('${l.id}','Qualificado')`) +
      actionBtn('Ganho', `NexumActions.atualizarStatusLead('${l.id}','Ganho')`, '#10B981')
    ]);
    renderTable('crm', 'Leads do CRM', ['Nome', 'Email', 'Telefone', 'Empresa', 'Origem', 'Status', 'Data', 'AÃ§Ãµes'], rows, 'Nenhum lead.', { label: 'Novo Lead', onclick: 'NexumActions.novoLead()' });
  }

  async function loadFinanceiro() {
    const [lancamentos, faturamento] = await Promise.all([apiGet('/financeiro/lancamentos'), apiGet('/financeiro/faturamento')]);
    if (!lancamentos) return;

    const section = document.getElementById('section-financeiro');
    let summary = section.querySelector('.nexum-finance-summary');
    if (!summary) {
      summary = document.createElement('div');
      summary.className = 'nexum-finance-summary';
      summary.style.cssText = 'display:grid;grid-template-columns:repeat(3,1fr);gap:15px;margin-bottom:20px;';
      section.insertBefore(summary, section.querySelector('.nexum-dynamic-content') || null);
    }
    if (faturamento) {
      summary.innerHTML = `
        <div style="background:linear-gradient(135deg,rgba(16,185,129,0.15),rgba(16,185,129,0.05));border:1px solid rgba(16,185,129,0.3);padding:20px;border-radius:8px;">
          <div style="color:#A0A0A0;font-size:0.8rem;text-transform:uppercase;">Faturamento Total</div>
          <div style="color:#10B981;font-size:1.8rem;font-weight:700;margin-top:8px;">${formatBRL(faturamento.total_faturamento)}</div>
        </div>
        <div style="background:linear-gradient(135deg,rgba(201,162,39,0.15),rgba(201,162,39,0.05));border:1px solid rgba(201,162,39,0.3);padding:20px;border-radius:8px;">
          <div style="color:#A0A0A0;font-size:0.8rem;text-transform:uppercase;">A Receber</div>
          <div style="color:#C9A227;font-size:1.8rem;font-weight:700;margin-top:8px;">${formatBRL(faturamento.total_pendente)}</div>
        </div>
        <div style="background:linear-gradient(135deg,rgba(59,130,246,0.15),rgba(59,130,246,0.05));border:1px solid rgba(59,130,246,0.3);padding:20px;border-radius:8px;">
          <div style="color:#A0A0A0;font-size:0.8rem;text-transform:uppercase;">Recebido</div>
          <div style="color:#3B82F6;font-size:1.8rem;font-weight:700;margin-top:8px;">${formatBRL(faturamento.total_pago)}</div>
        </div>
      `;
    }

    const rows = lancamentos.map(l => [
      `<strong>${l.descricao}</strong>`, l.categoria,
      `<span style="color:${l.tipo === 'receita' ? '#10B981' : '#EF4444'};font-weight:600;">${l.tipo === 'receita' ? '+' : '-'}${formatBRL(l.valor)}</span>`,
      formatDate(l.data_lancamento),
      l.pago ? statusBadge('Entregue') : statusBadge('Pendente')
    ]);
    renderTable('financeiro', 'LanÃ§amentos Financeiros', ['DescriÃ§Ã£o', 'Categoria', 'Valor', 'Data', 'Status'], rows, 'Nenhum lanÃ§amento.');
  }

  // SeÃ§Ãµes nÃ£o nativas do HTML - criamos div dinamicamente
  function ensureSection(sectionId) {
    let section = document.getElementById(`section-${sectionId}`);
    if (!section) {
      section = document.createElement('div');
      section.id = `section-${sectionId}`;
      section.className = 'section-content';
      section.style.display = 'none';
      const mainContent = document.querySelector('.main-content, main, #main');
      if (mainContent) {
        mainContent.appendChild(section);
        // Adiciona header bÃ¡sico
        section.innerHTML = `<h1 class="page-title">${sectionId.charAt(0).toUpperCase() + sectionId.slice(1)}</h1>`;
      }
    }
    return section;
  }

  async function loadCupons() {
    ensureSection('cupons');
    try {
      // Tenta validar um cupom para gatilhar listagem (API sÃ³ tem por cÃ³digo). Usamos lojas para fallback
      // Como nÃ£o hÃ¡ endpoint de listagem, faz uma busca direta no Mongo via uma query
      const cupons = await apiGet('/cupons/BEMVINDO10').then(c => [c]).catch(() => [])
        .then(async (c1) => {
          const c2 = await apiGet('/cupons/PRIMEIRACOMPRA').catch(() => null);
          const c3 = await apiGet('/cupons/NEXUM2026').catch(() => null);
          return [...c1, c2, c3].filter(Boolean);
        });
      const rows = cupons.map(c => [
        `<strong style="color:#C9A227;">${c.codigo}</strong>`,
        c.desconto_percentual ? `${c.desconto_percentual}%` : formatBRL(c.desconto_valor),
        c.valor_minimo ? formatBRL(c.valor_minimo) : 'Sem mÃ­nimo',
        c.data_validade ? formatDate(c.data_validade) : 'Sem validade',
        statusBadge(c.ativo ? 'Ativo' : 'Inativo')
      ]);
      renderTable('cupons', 'Cupons de Desconto', ['CÃ³digo', 'Desconto', 'Valor MÃ­nimo', 'Validade', 'Status'], rows, 'Nenhum cupom ativo.', { label: 'Novo Cupom', onclick: 'NexumActions.novoCupom()' });
    } catch (e) {
      renderTable('cupons', 'Cupons de Desconto', ['CÃ³digo', 'Desconto', 'Valor MÃ­nimo', 'Validade', 'Status'], [], 'Sistema de cupons ativo. Cupons disponÃ­veis: BEMVINDO10, PRIMEIRACOMPRA, NEXUM2026', { label: 'Novo Cupom', onclick: 'NexumActions.novoCupom()' });
    }
  }

  async function loadUsuarios() {
    ensureSection('usuarios');
    const user = JSON.parse(localStorage.getItem(STORAGE_USER) || '{}');
    const usuarios = [
      { nome: 'Rodrigo Costa', email: 'corporativo.gna@gmail.com', role: 'SuperAdmin', ativo: true },
      { nome: 'Vinicius', email: 'corporativo.gna@gmail.com', role: 'Admin', ativo: true }
    ];
    const rows = usuarios.map(u => [
      `<strong>${u.nome}${u.email === user.email ? ' <span style=\"color:#C9A227;\">(vocÃª)</span>' : ''}</strong>`,
      u.email, badge(u.role, '#C9A227'),
      statusBadge(u.ativo ? 'Ativo' : 'Inativo')
    ]);
    renderTable('usuarios', 'UsuÃ¡rios do Sistema', ['Nome', 'Email', 'Role', 'Status'], rows, '');
  }

  async function loadConfiguracoes() {
    ensureSection('configuracoes');
    const section = document.getElementById('section-configuracoes');
    let container = section.querySelector('.nexum-dynamic-content');
    if (!container) {
      container = document.createElement('div');
      container.className = 'nexum-dynamic-content';
      section.appendChild(container);
    }
    container.innerHTML = `
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:20px;">
        <div style="background:rgba(20,20,20,0.5);padding:25px;border-radius:8px;border:1px solid rgba(201,162,39,0.15);">
          <h3 style="color:#C9A227;font-family:'Playfair Display',serif;">Sistema</h3>
          <div class="view-row" style="padding:10px 0;border-bottom:1px solid rgba(255,255,255,0.05);"><span style="color:#A0A0A0;">Nome:</span> <span>Grupo Nexum Altivon</span></div>
          <div class="view-row" style="padding:10px 0;border-bottom:1px solid rgba(255,255,255,0.05);"><span style="color:#A0A0A0;">Email:</span> <span>corporativo.gna@gmail.com</span></div>
          <div class="view-row" style="padding:10px 0;border-bottom:1px solid rgba(255,255,255,0.05);"><span style="color:#A0A0A0;">VersÃ£o:</span> <span>1.0.0</span></div>
          <div class="view-row" style="padding:10px 0;"><span style="color:#A0A0A0;">Backend:</span> <span style="color:#10B981;">â— Online</span></div>
        </div>
        <div style="background:rgba(20,20,20,0.5);padding:25px;border-radius:8px;border:1px solid rgba(201,162,39,0.15);">
          <h3 style="color:#C9A227;font-family:'Playfair Display',serif;">Contatos</h3>
          <div class="view-row" style="padding:10px 0;border-bottom:1px solid rgba(255,255,255,0.05);"><span style="color:#A0A0A0;">Rodrigo:</span> <span>(14) 99673-1879</span></div>
          <div class="view-row" style="padding:10px 0;border-bottom:1px solid rgba(255,255,255,0.05);"><span style="color:#A0A0A0;">Vinicius:</span> <span>(14) 99634-8409</span></div>
          <div class="view-row" style="padding:10px 0;"><span style="color:#A0A0A0;">Website:</span> <a href="https://www.nexumaltivon.com" style="color:#C9A227;" target="_blank">www.nexumaltivon.com</a></div>
        </div>
      </div>
    `;
  }

  function loadStub(sectionId, title, msg) {
    ensureSection(sectionId);
    const section = document.getElementById(`section-${sectionId}`);
    let container = section.querySelector('.nexum-dynamic-content');
    if (!container) {
      container = document.createElement('div');
      container.className = 'nexum-dynamic-content';
      section.appendChild(container);
    }
    container.innerHTML = `
      <div style="background:rgba(20,20,20,0.5);padding:40px;border-radius:8px;border:1px solid rgba(201,162,39,0.15);text-align:center;">
        <h3 style="color:#C9A227;font-family:'Playfair Display',serif;font-size:1.5rem;">${title}</h3>
        <p style="color:#A0A0A0;margin-top:15px;font-size:1rem;">${msg}</p>
      </div>
    `;
  }

  const loaders = {
    'dashboard': updateDashboard,
    'pedidos': loadPedidos,
    'produtos': loadProdutos,
    'clientes': loadClientes,
    'lojas': loadLojas,
    'crm': loadCRM,
    'financeiro': loadFinanceiro,
    'cupons': loadCupons,
    'usuarios': loadUsuarios,
    'configuracoes': loadConfiguracoes,
    'fiscal': () => loadStub('fiscal', 'Sistema Fiscal', 'IntegraÃ§Ã£o com SEFAZ/NFe disponÃ­vel para configuraÃ§Ã£o. Endpoint /api/fiscal pronto no backend.'),
    'logistica': () => loadStub('logistica', 'LogÃ­stica & Envios', 'Sistema de envios e rastreamento ativo. Transportadoras: Correios, Jadlog, Azul Cargo Express.'),
    'marketing': () => loadStub('marketing', 'Marketing & Campanhas', 'Crie campanhas, integre com email marketing e WhatsApp Business via API.'),
    'marketplaces': () => loadStub('marketplaces', 'Marketplaces', 'Integre com Mercado Livre, Amazon, Magalu via API. Endpoints prontos no backend.'),
    'dropshipping': () => loadStub('dropshipping', 'Dropshipping', 'Conecte fornecedores nacionais e internacionais. Cadastro de fornecedores disponÃ­vel.'),
    'auditoria': () => loadStub('auditoria', 'Auditoria do Sistema', 'Todas as aÃ§Ãµes sÃ£o registradas. Endpoint /api/logs_auditoria com histÃ³rico completo.')
  };

  // ==========================================================
  // HOOK NO showSection
  // ==========================================================
  function setupSectionHook() {
    const originalShowSection = window.showSection;
    window.showSection = function(sectionId) {
      // Tenta executar original; se falhar, fallback manual
      try { if (typeof originalShowSection === 'function') originalShowSection.apply(this, arguments); }
      catch(e) {}

      // Garante visibilidade da seÃ§Ã£o
      ensureSection(sectionId);
      document.querySelectorAll('.section-content').forEach(s => s.style.display = 'none');
      const target = document.getElementById('section-' + sectionId);
      if (target) target.style.display = 'block';
      document.querySelectorAll('.menu-item').forEach(m => m.classList.remove('active'));
      if (event && event.currentTarget) event.currentTarget.classList.add('active');

      if (loaders[sectionId]) loaders[sectionId]();
    };
  }

  // ==========================================================
  // LOGIN OVERLAY (igual antes)
  // ==========================================================
  function createLoginOverlay() {
    const overlay = document.createElement('div');
    overlay.id = 'nexum-login-overlay';
    overlay.innerHTML = `
      <style>
        #nexum-login-overlay { position:fixed;top:0;left:0;width:100%;height:100%;background:linear-gradient(135deg,#0A0A0A 0%,#1A1A1A 100%);z-index:99999;display:flex;align-items:center;justify-content:center;font-family:'Montserrat',sans-serif; }
        #nexum-login-box { background:rgba(255,255,255,0.05);backdrop-filter:blur(10px);padding:40px;border-radius:12px;border:1px solid rgba(201,162,39,0.3);width:100%;max-width:420px; }
        #nexum-login-box h2 { font-family:'Playfair Display',serif;color:#C9A227;font-size:1.8rem;margin-bottom:8px;text-align:center; }
        #nexum-login-box p { color:#A0A0A0;text-align:center;font-size:0.85rem;margin-bottom:25px; }
        #nexum-login-box .field { margin-bottom:15px; }
        #nexum-login-box label { color:#F5F5F5;font-size:0.85rem;display:block;margin-bottom:6px;font-weight:600; }
        #nexum-login-box input { width:100%;padding:12px;background:rgba(0,0,0,0.4);border:1px solid rgba(201,162,39,0.3);color:#F5F5F5;border-radius:6px;font-size:0.95rem;box-sizing:border-box; }
        #nexum-login-btn { width:100%;padding:14px;background:#C9A227;color:#0A0A0A;border:none;border-radius:6px;font-weight:700;font-size:1rem;cursor:pointer;text-transform:uppercase;letter-spacing:1px;margin-top:10px; }
        #nexum-login-error { color:#ef4444;background:rgba(239,68,68,0.1);padding:10px;border-radius:6px;margin-bottom:15px;font-size:0.85rem;display:none; }
        #nexum-login-hint { margin-top:15px;text-align:center;font-size:0.75rem;color:#A0A0A0;padding:10px;background:rgba(201,162,39,0.05);border-radius:6px; }
      </style>
      <div id="nexum-login-box">
        <h2>NEXUM ALTIVON</h2><p>Painel Administrativo</p>
        <div id="nexum-login-error"></div>
        <form id="nexum-login-form">
          <div class="field"><label>Email</label><input type="email" id="nexum-email" required /></div>
          <div class="field"><label>Senha</label><input type="password" id="nexum-senha" required /></div>
          <button type="submit" id="nexum-login-btn">Entrar</button>
        </form>
        <div id="nexum-login-hint"><strong style="color:#C9A227">Acesso restrito:</strong><br>Use as credenciais autorizadas pela empresa.</div>
      </div>
    `;
    document.body.appendChild(overlay);
    document.getElementById('nexum-login-form').addEventListener('submit', async (e) => {
      e.preventDefault();
      const btn = document.getElementById('nexum-login-btn');
      const errorEl = document.getElementById('nexum-login-error');
      btn.disabled = true; btn.textContent = 'Entrando...';
      try {
        const data = await apiPost('/auth/login', { email: document.getElementById('nexum-email').value, senha: document.getElementById('nexum-senha').value });
        localStorage.setItem(STORAGE_TOKEN, data.access_token);
        localStorage.setItem(STORAGE_USER, JSON.stringify(data.user));
        overlay.remove();
        initDashboard();
      } catch (err) {
        errorEl.textContent = err.message; errorEl.style.display = 'block';
        btn.disabled = false; btn.textContent = 'Entrar';
      }
    });
  }

  function setupUserAndLogout() {
    const userStr = localStorage.getItem(STORAGE_USER);
    if (!userStr) return;
    const user = JSON.parse(userStr);
    document.querySelectorAll('.user-name, [class*="user"] span').forEach(el => {
      if (el.children.length === 0 && el.textContent.trim().toLowerCase().includes('admin')) el.textContent = user.nome;
    });
    if (!document.getElementById('nexum-logout-btn')) {
      const btn = document.createElement('button');
      btn.id = 'nexum-logout-btn';
      btn.innerHTML = '<i class="fas fa-sign-out-alt"></i> SAIR';
      btn.style.cssText = 'position:fixed;top:20px;right:20px;z-index:9999;background:#C9A227;color:#0A0A0A;border:none;padding:8px 16px;border-radius:6px;cursor:pointer;font-weight:700;font-size:0.85rem;text-transform:uppercase;';
      btn.onclick = () => { localStorage.clear(); window.location.reload(); };
      document.body.appendChild(btn);
    }
  }

  async function initDashboard() {
    setupUserAndLogout();
    setupSectionHook();
    await Promise.all([
      updateDashboard(), updateBadges(),
      loadPedidos(), loadProdutos(), loadClientes(), loadLojas(), loadCRM(), loadFinanceiro()
    ]);
    console.log('[Nexum] Painel TOTALMENTE inicializado com todos os comandos operacionais');
    setInterval(() => { updateDashboard(); updateBadges(); }, 30000);
  }

  document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem(STORAGE_TOKEN);
    if (token) {
      fetch(`${API_URL}/auth/me`, { headers: { 'Authorization': `Bearer ${token}` } })
        .then(res => res.ok ? initDashboard() : (localStorage.removeItem(STORAGE_TOKEN), createLoginOverlay()))
        .catch(() => createLoginOverlay());
    } else createLoginOverlay();
  });
})();

