/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import axios from 'axios';
import { HTTP_UNAUTHORIZED, STORAGE_KEYS } from '../constants';

const PUBLIC_API_URL = 'https://api.nexumaltivon.com.br';
const RUNTIME_API_CONFIG_URL = '/api-runtime.json';
const RUNTIME_CACHE_KEY = 'nexum_api_runtime_url';
const RUNTIME_URL_TTL_MS = 30 * 1000;

let runtimeApiUrlPromise = null;
let runtimeApiUrlResolvedAt = 0;
const apiHealthCache = new Map();

const normalizeApiUrl = (value) => {
  const url = String(value || '').trim().replace(/\/+$/, '');
  return /^https?:\/\//i.test(url) ? url : '';
};

const getDefaultApiUrl = () => {
  if (typeof window === 'undefined') return 'http://127.0.0.1:5010';

  const { hostname } = window.location;
  const isLocalhost =
    hostname === 'localhost' ||
    hostname === '127.0.0.1' ||
    hostname === '::1' ||
    hostname === '';

  return isLocalhost ? 'http://127.0.0.1:5010' : PUBLIC_API_URL;
};

export const API_BASE_URL = normalizeApiUrl(process.env.REACT_APP_BACKEND_URL) || getDefaultApiUrl();
const API_URL = `${API_BASE_URL}/api`;

const collectRuntimeApiUrls = (config) => {
  const values = [
    config?.apiUrl,
    config?.api_url,
    config?.apiBaseUrl,
    config?.API_BASE_URL,
    config?.API_URL,
    config?.backendUrl,
    config?.url,
    ...(Array.isArray(config?.apiUrls) ? config.apiUrls : []),
    ...(Array.isArray(config?.api_urls) ? config.api_urls : []),
  ];

  return [...new Set(values.map(normalizeApiUrl).filter(Boolean))];
};

const canUseApiUrl = async (baseUrl, force = false) => {
  const normalized = normalizeApiUrl(baseUrl);
  if (!normalized) return false;

  if (!force && apiHealthCache.has(normalized)) {
    return apiHealthCache.get(normalized);
  }

  try {
    const controller = new AbortController();
    const timeout = window.setTimeout(() => controller.abort(), 4000);
    const response = await fetch(`${normalized}/health?t=${Date.now()}`, {
      method: 'GET',
      cache: 'no-store',
      headers: { Accept: 'text/plain, application/json' },
      signal: controller.signal,
    });
    window.clearTimeout(timeout);
    const healthy = response.ok;
    apiHealthCache.set(normalized, healthy);
    return healthy;
  } catch {
    apiHealthCache.set(normalized, false);
    return false;
  }
};

const isLocalApi = () => {
  if (typeof window === 'undefined') return true;
  const { hostname } = window.location;
  return hostname === 'localhost' || hostname === '127.0.0.1' || hostname === '::1' || hostname === '';
};

export const getRuntimeApiBaseUrl = async ({ force = false } = {}) => {
  if (isLocalApi()) return API_BASE_URL;
  if (force) {
    runtimeApiUrlPromise = null;
    runtimeApiUrlResolvedAt = 0;
    apiHealthCache.clear();
  }
  if (runtimeApiUrlPromise && Date.now() - runtimeApiUrlResolvedAt < RUNTIME_URL_TTL_MS) {
    return runtimeApiUrlPromise;
  }

  runtimeApiUrlPromise = (async () => {
    const cached = normalizeApiUrl(localStorage.getItem(RUNTIME_CACHE_KEY));
    const candidates = [];

    try {
      const response = await fetch(`${RUNTIME_API_CONFIG_URL}?t=${Date.now()}`, {
        cache: 'no-store',
        headers: { Accept: 'application/json' },
      });

      if (response.ok) {
        const config = await response.json();
        candidates.push(...collectRuntimeApiUrls(config));
      }
    } catch (error) {
      console.error('Falha ao carregar api-runtime.json publico.', error);
    }

    if (cached) candidates.push(cached);
    candidates.push(API_BASE_URL);
    candidates.push(PUBLIC_API_URL);

    for (const candidate of [...new Set(candidates.filter(Boolean))]) {
      if (await canUseApiUrl(candidate, force)) {
        localStorage.setItem(RUNTIME_CACHE_KEY, candidate);
        return candidate;
      }
    }

    localStorage.removeItem(RUNTIME_CACHE_KEY);
    throw new Error(`Nenhuma URL oficial da API respondeu em /health. URLs verificadas: ${[...new Set(candidates.filter(Boolean))].join(', ')}`);
  })();

  runtimeApiUrlResolvedAt = Date.now();
  return runtimeApiUrlPromise;
};

const normalizeRecord = (record) => {
  if (!record || typeof record !== 'object' || Array.isArray(record)) return record;

  return {
    ...record,
    preco_promocional: record.preco_promocional ?? record.precoPromocional,
    imagem_url: record.imagem_url ?? record.imagemUrl,
    categoria_id: record.categoria_id ?? record.categoriaId,
    subcategoria_id: record.subcategoria_id ?? record.subcategoriaId,
    categoria_pai_id: record.categoria_pai_id ?? record.categoriaPaiId,
    cpf: record.cpf ?? record.cpfCnpj ?? record.cpf_cnpj,
    documento: record.documento ?? record.cnpj ?? record.cpfCnpj ?? record.cpf_cnpj,
    numero_pedido: record.numero_pedido ?? record.numeroPedido,
    status_pagamento: record.status_pagamento ?? record.statusPagamento,
    meio_pagamento: record.meio_pagamento ?? record.meioPagamento,
    gateway_pagamento: record.gateway_pagamento ?? record.gatewayPagamento,
    gateway_transacao_id: record.gateway_transacao_id ?? record.gatewayTransacaoId,
    pix_qrcode: record.pix_qrcode ?? record.pixQrcode,
    payment_url: record.payment_url ?? record.paymentUrl,
    frete_valor: record.frete_valor ?? record.freteValor,
    frete_metodo: record.frete_metodo ?? record.freteMetodo,
    frete_transportadora: record.frete_transportadora ?? record.freteTransportadora,
    frete_prazo_dias: record.frete_prazo_dias ?? record.fretePrazoDias,
    frete_codigo_rastreio: record.frete_codigo_rastreio ?? record.freteCodigoRastreio,
    instrucao_pagamento: record.instrucao_pagamento ?? record.instrucaoPagamento,
    cliente_nome: record.cliente_nome ?? record.clienteNome,
    updated_at: record.updated_at ?? record.updatedAt,
    created_at: record.created_at ?? record.createdAt,
    configurada: record.configurada ?? record.Configurada,
    operacional: record.operacional ?? record.Operacional,
    detalhe: record.detalhe ?? record.Detalhe,
    pendencias: record.pendencias ?? record.Pendencias,
    verificado_em: record.verificado_em ?? record.verificadoEm ?? record.VerificadoEm,
    referencia: record.referencia ?? record.Referencia,
    ambiente: record.ambiente ?? record.Ambiente,
    prazo_dias: record.prazo_dias ?? record.prazoDias ?? record.PrazoDias,
    desconto_percentual: record.desconto_percentual ?? record.descontoPercentual,
    desconto_valor: record.desconto_valor ?? record.descontoValor,
    valor_minimo: record.valor_minimo ?? record.valorMinimo,
    pedidos_hoje: record.pedidos_hoje ?? record.pedidosHoje,
    total_clientes: record.total_clientes ?? record.totalClientes,
    faturamento_mes: record.faturamento_mes ?? record.faturamentoMes,
    leads_novos: record.leads_novos ?? record.leadsNovos,
    produtos_estoque_baixo: record.produtos_estoque_baixo ?? record.produtosEstoqueBaixo,
    ticket_medio: record.ticket_medio ?? record.ticketMedio,
  };
};

const normalizeData = (data) => {
  if (Array.isArray(data)) return data.map(normalizeRecord);
  return normalizeRecord(data);
};

export const unwrapApiData = (data) => {
  if (!data || typeof data !== 'object') return data;
  return normalizeData(data.dados ?? data.Dados ?? data.data ?? data.Data ?? data);
};

const api = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

api.interceptors.request.use(async (config) => {
  const runtimeApiBaseUrl = await getRuntimeApiBaseUrl();
  config.baseURL = `${runtimeApiBaseUrl}/api`;

  const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => ({
    ...response,
    data: unwrapApiData(response.data),
  }),
  async (error) => {
    const originalRequest = error.config;

    if (!error.response && originalRequest && !originalRequest._runtimeRetry) {
      originalRequest._runtimeRetry = true;
      const runtimeApiBaseUrl = await getRuntimeApiBaseUrl({ force: true });
      originalRequest.baseURL = `${runtimeApiBaseUrl}/api`;
      return api(originalRequest);
    }

    if (error.response?.status === HTTP_UNAUTHORIZED && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
        if (!refreshToken) {
          return Promise.reject(error);
        }

        const runtimeApiBaseUrl = await getRuntimeApiBaseUrl();
        const accessTokenAtual = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
        const response = await axios.post(`${runtimeApiBaseUrl}/api/auth/refresh`, {
          token: accessTokenAtual,
          access_token: accessTokenAtual,
          refresh_token: refreshToken,
          refreshToken,
        });

        const payload = unwrapApiData(response.data) || {};
        const accessToken = payload.access_token || payload.accessToken || payload.token || payload.Token;
        const nextRefreshToken = payload.refresh_token || payload.refreshToken || payload.RefreshToken || refreshToken;

        if (!accessToken) {
          return Promise.reject(error);
        }

        localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, nextRefreshToken);
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;

        return api(originalRequest);
      } catch (refreshError) {
        localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER);
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

const buildResourceApi = (resourcePath) => ({
  listar: (params) => api.get(resourcePath, { params }),
  getAll: (params) => api.get(resourcePath, { params }),
  obter: (id) => api.get(`${resourcePath}/${id}`),
  getById: (id) => api.get(`${resourcePath}/${id}`),
  criar: (data) => api.post(resourcePath, data),
  create: (data) => api.post(resourcePath, data),
  atualizar: (id, data) => api.put(`${resourcePath}/${id}`, data),
  update: (id, data) => api.put(`${resourcePath}/${id}`, data),
  excluir: (id) => api.delete(`${resourcePath}/${id}`),
  delete: (id) => api.delete(`${resourcePath}/${id}`),
});

export const lojaAPI = buildResourceApi('/lojas');

export const siteAPI = {
  obterConfiguracao: () => api.get('/site/configuracoes/publico'),
  getPublicConfig: () => api.get('/site/configuracoes/publico'),
  listarConfiguracoes: () => api.get('/site/configuracoes'),
  getAll: () => api.get('/site/configuracoes'),
  atualizarConfiguracao: (payload) => api.put('/site/configuracoes', payload?.itens ? payload : { itens: payload }),
  update: (itens) => api.put('/site/configuracoes', { itens }),
};

export const produtoAPI = {
  ...buildResourceApi('/produtos'),
  destaques: (limite = 5) => api.get('/produtos/destaques', { params: { limite } }),
  getDestaques: (limite = 5) => api.get('/produtos/destaques', { params: { limite } }),
  porCategoria: (categoriaId) => api.get('/produtos', { params: { categoriaId } }),
  precosPorLoja: (id) => api.get(`/produtos/${id}/precos-por-loja`),
  uploadImagem: (data) => api.post('/uploads/produtos/imagens', data),
};

export const categoriaAPI = buildResourceApi('/categorias');

export const pedidoAPI = {
  ...buildResourceApi('/pedidos'),
  criarCheckout: (data) => api.post('/pedidos', data),
  acompanhar: (params) => api.get('/pedidos/acompanhar', { params }),
  updateStatus: (id, status) => api.put(`/pedidos/${id}/status`, { novo_status: status, status }),
  atualizarStatus: (id, status) => api.put(`/pedidos/${id}/status`, { novo_status: status, status }),
  avancarFluxo: (id) => api.post(`/pedidos/${id}/fluxo-operacional`),
  updateLogistica: (id, data) => api.put(`/pedidos/${id}/logistica`, data),
};

export const clienteAPI = {
  ...buildResourceApi('/clientes'),
  verificarCadastro: (params) => api.get('/clientes/verificar', { params }),
  cadastrar: (data) => api.post('/clientes', data),
  confirmarEmail: (token) => api.get('/clientes/confirmar', { params: { token } }),
  confirmarCadastro: (token) => api.get('/clientes/confirmar', { params: { token } }),
  reenviarConfirmacao: (email) => api.post('/clientes/reenviar-confirmacao', { email }),
  getPortal: () => api.get('/clientes/portal/me'),
  criarEndereco: (data) => api.post('/clientes/portal/enderecos', data),
  atualizarEndereco: (id, data) => api.put(`/clientes/portal/enderecos/${id}`, data),
  definirEnderecoPrincipal: (id) => api.put(`/clientes/portal/enderecos/${id}/principal`),
  removerEndereco: (id) => api.delete(`/clientes/portal/enderecos/${id}`),
};

export const cupomAPI = {
  validar: (codigo) => api.get(`/cupons/${codigo}`),
};

export const fornecedorAPI = {
  ...buildResourceApi('/fornecedores'),
};

export const comprasAPI = {
  getPainel: () => api.get('/compras/painel'),
  registrarSolicitacao: (data) => api.post('/compras/solicitacoes', data),
  atualizarSolicitacaoStatus: (id, data) => api.patch(`/compras/solicitacoes/${id}/status`, data),
  registrarCotacao: (data) => api.post('/compras/cotacoes', data),
  criarPedido: (data) => api.post('/compras/pedidos', data),
  atualizarPedidoStatus: (id, data) => api.patch(`/compras/pedidos/${id}/status`, data),
  registrarEntrada: (id, data) => api.post(`/compras/pedidos/${id}/entradas`, data),
};

export const empresaGrupoAPI = {
  getAll: () => api.get('/erp/empresas'),
  create: (data) => api.post('/erp/empresas', data),
};

export const fiscalAPI = {
  getPedidos: () => api.get('/fiscal/pedidos'),
  updatePedidoStatus: (id, status) => api.put(`/fiscal/pedidos/${id}/status`, { novo_status: status, status }),
  getPdvConfiguracoes: () => api.get('/fiscal/pdv/configuracoes'),
  simularRoteamento: (data) => api.post('/fiscal/simular-roteamento', data),
  prepararEmissaoManual: (data) => api.post('/fiscal/preparar-emissao-manual', data),
  salvarRascunhoManual: (data) => api.post('/fiscal/rascunho-manual', data),
  getRascunhoManual: () => api.get('/fiscal/rascunho-manual'),
};

export const leadAPI = {
  getAll: (params) => api.get('/crm/leads', { params }),
  create: (data) => api.post('/crm/leads', data),
  updateStatus: (id, status, responsavel_id) =>
    api.put(`/crm/leads/${id}/status`, { novo_status: status, responsavel_id }),
};

export const dashboardAPI = {
  getResumo: () => api.get('/dashboard/resumo'),
  getCorporativoPainel: () => api.get('/gestao-corporativa/painel'),
  getDicionarioDados: () => api.get('/gestao-corporativa/dicionario-dados'),
  getCicloOperacional: () => api.get('/gestao-corporativa/ciclo-operacional'),
  getRelatorioVendas: (dataInicio, dataFim) =>
    api.get('/relatorios/vendas', {
      params: { data_inicio: dataInicio, data_fim: dataFim },
    }),
};

export const pdvAPI = {
  getCockpit: () => api.get('/pdv/cockpit'),
};

export const financeiroAPI = {
  getLancamentos: (tipo) => api.get('/financeiro/lancamentos', { params: { tipo } }),
  createLancamento: (data) => api.post('/financeiro/lancamentos', data),
  updateLancamentoStatus: (id, data) => api.patch(`/financeiro/lancamentos/${id}/status`, data),
  getFaturamento: (dataInicio, dataFim) =>
    api.get('/financeiro/faturamento', {
      params: { data_inicio: dataInicio, data_fim: dataFim },
    }),
};

export const integracoesAPI = {
  getStatus: () => api.get('/integracoes/status'),
  getDiagnostico: () => api.get('/integracoes/diagnostico'),
  getCredenciaisModelo: () => api.get('/integracoes/credenciais-modelo'),
  testar: (slug) => api.post(`/integracoes/testar/${slug}`),
};

export const assistenteAPI = {
  enviarMensagem: (data) => api.post('/assistentes/mensagem', data),
};

export const freteAPI = {
  cotar: (data) => api.post('/frete/cotar', data),
};

export const logisticaAPI = {
  rotear: (data) => api.post('/logistica/roteamento', data),
};

export default api;
