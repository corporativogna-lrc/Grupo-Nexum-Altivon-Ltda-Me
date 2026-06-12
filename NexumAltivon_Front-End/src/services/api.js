import axios from 'axios';
import { HTTP_UNAUTHORIZED, STORAGE_KEYS } from '../constants';

const PUBLIC_API_URL = 'https://api.nexumaltivon.com';
const RUNTIME_API_CONFIG_URL = '/api-runtime.json';
const RUNTIME_CACHE_KEY = 'nexum_api_runtime_url';

let runtimeApiUrlPromise = null;
const apiHealthCache = new Map();

const getDefaultApiUrl = () => {
  if (typeof window === 'undefined') return 'http://localhost:5000';

  const { hostname } = window.location;
  const isLocalhost = hostname === 'localhost' || hostname === '127.0.0.1' || hostname === '';

  return isLocalhost ? 'http://localhost:5011' : PUBLIC_API_URL;
};

export const API_BASE_URL = process.env.REACT_APP_BACKEND_URL || getDefaultApiUrl();
const API_URL = `${API_BASE_URL}/api`;

const normalizeApiUrl = (value) => {
  const url = String(value || '').trim().replace(/\/+$/, '');
  return /^https?:\/\//i.test(url) ? url : '';
};

const canUseApiUrl = async (baseUrl) => {
  const normalized = normalizeApiUrl(baseUrl);
  if (!normalized) return false;

  if (apiHealthCache.has(normalized)) {
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
  return hostname === 'localhost' || hostname === '127.0.0.1' || hostname === '';
};

export const getRuntimeApiBaseUrl = async () => {
  if (process.env.REACT_APP_BACKEND_URL || isLocalApi()) return API_BASE_URL;
  if (runtimeApiUrlPromise) return runtimeApiUrlPromise;

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
        const runtimeUrl = normalizeApiUrl(config.apiUrl || config.api_url || config.url);
        if (runtimeUrl) {
          candidates.push(runtimeUrl);
        }
      }
    } catch {
      // Mantém a última ponte funcional em cache quando a configuração pública oscila.
    }

    if (cached) candidates.push(cached);
    candidates.push(API_BASE_URL);

    for (const candidate of [...new Set(candidates.filter(Boolean))]) {
      if (await canUseApiUrl(candidate)) {
        localStorage.setItem(RUNTIME_CACHE_KEY, candidate);
        return candidate;
      }
    }

    return cached || API_BASE_URL;
  })();

  return runtimeApiUrlPromise;
};

const normalizeRecord = (record) => {
  if (!record || typeof record !== 'object' || Array.isArray(record)) return record;

  return {
    ...record,
    preco_promocional: record.preco_promocional ?? record.precoPromocional,
    imagem_url: record.imagem_url ?? record.imagemUrl,
    categoria_id: record.categoria_id ?? record.categoriaId,
    cpf: record.cpf ?? record.cpfCnpj ?? record.cpf_cnpj,
    documento: record.documento ?? record.cnpj ?? record.cpfCnpj ?? record.cpf_cnpj,
    numero_pedido: record.numero_pedido ?? record.numeroPedido,
    status_pagamento: record.status_pagamento ?? record.statusPagamento,
    meio_pagamento: record.meio_pagamento ?? record.meioPagamento,
    gateway_pagamento: record.gateway_pagamento ?? record.gatewayPagamento,
    gateway_transacao_id: record.gateway_transacao_id ?? record.gatewayTransacaoId,
    frete_valor: record.frete_valor ?? record.freteValor,
    frete_metodo: record.frete_metodo ?? record.freteMetodo,
    frete_transportadora: record.frete_transportadora ?? record.freteTransportadora,
    frete_prazo_dias: record.frete_prazo_dias ?? record.fretePrazoDias,
    instrucao_pagamento: record.instrucao_pagamento ?? record.instrucaoPagamento,
    configurada: record.configurada ?? record.Configurada,
    operacional: record.operacional ?? record.Operacional,
    detalhe: record.detalhe ?? record.Detalhe,
    pendencias: record.pendencias ?? record.Pendencias,
    verificado_em: record.verificado_em ?? record.verificadoEm ?? record.VerificadoEm,
    referencia: record.referencia ?? record.Referencia,
    ambiente: record.ambiente ?? record.Ambiente,
    prazo_dias: record.prazo_dias ?? record.prazoDias ?? record.PrazoDias,
    created_at: record.created_at ?? record.createdAt,
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
  return normalizeData(data.dados ?? data.Dados ?? data.data ?? data);
};

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use(async (config) => {
  const runtimeApiBaseUrl = await getRuntimeApiBaseUrl();
  config.baseURL = `${runtimeApiBaseUrl}/api`;

  const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token refresh
api.interceptors.response.use(
  (response) => ({
    ...response,
    data: unwrapApiData(response.data),
  }),
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === HTTP_UNAUTHORIZED && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
        const runtimeApiBaseUrl = await getRuntimeApiBaseUrl();
        const response = await axios.post(`${runtimeApiBaseUrl}/api/auth/refresh`, {
          refresh_token: refreshToken,
        });

        const { access_token } = response.data;
        localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, access_token);
        originalRequest.headers.Authorization = `Bearer ${access_token}`;

        return api(originalRequest);
      } catch (refreshError) {
        localStorage.clear();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

// API Methods
export const lojaAPI = {
  getAll: () => api.get('/lojas'),
  getById: (id) => api.get(`/lojas/${id}`),
};

export const siteAPI = {
  getPublicConfig: () => api.get('/site/configuracoes/publico'),
  getAll: () => api.get('/site/configuracoes'),
  update: (itens) => api.put('/site/configuracoes', { itens }),
};

export const produtoAPI = {
  getAll: (params) => api.get('/produtos', { params }),
  getDestaques: () => api.get('/produtos/destaques'),
  getById: (id) => api.get(`/produtos/${id}`),
  create: (data) => api.post('/produtos', data),
  update: (id, data) => api.put(`/produtos/${id}`, data),
  uploadImagem: (data) => api.post('/uploads/produtos/imagens', data),
};

export const categoriaAPI = {
  getAll: () => api.get('/categorias'),
  create: (data) => api.post('/categorias', data),
};

export const pedidoAPI = {
  getAll: (params) => api.get('/pedidos', { params }),
  getById: (id) => api.get(`/pedidos/${id}`),
  create: (data) => api.post('/pedidos', data),
  updateStatus: (id, status) => api.put(`/pedidos/${id}/status`, { novo_status: status }),
};

export const clienteAPI = {
  getAll: () => api.get('/clientes'),
  verificarCadastro: (params) => api.get('/clientes/verificar', { params }),
  create: (data) => api.post('/clientes', data),
  getPortal: () => api.get('/clientes/portal/me'),
};

export const fornecedorAPI = {
  getAll: () => api.get('/fornecedores'),
  create: (data) => api.post('/fornecedores', data),
};

export const empresaGrupoAPI = {
  getAll: () => api.get('/erp/empresas'),
  create: (data) => api.post('/erp/empresas', data),
};

export const fiscalAPI = {
  getPedidos: () => api.get('/fiscal/pedidos'),
  getPdvConfiguracoes: () => api.get('/fiscal/pdv/configuracoes'),
  simularRoteamento: (data) => api.post('/fiscal/simular-roteamento', data),
};

export const leadAPI = {
  getAll: (params) => api.get('/crm/leads', { params }),
  create: (data) => api.post('/crm/leads', data),
  updateStatus: (id, status, responsavel_id) =>
    api.put(`/crm/leads/${id}/status`, { novo_status: status, responsavel_id }),
};

export const dashboardAPI = {
  getResumo: () => api.get('/dashboard/resumo'),
  getRelatorioVendas: (dataInicio, dataFim) =>
    api.get('/relatorios/vendas', {
      params: { data_inicio: dataInicio, data_fim: dataFim },
    }),
};

export const financeiroAPI = {
  getLancamentos: (tipo) => api.get('/financeiro/lancamentos', { params: { tipo } }),
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

export const freteAPI = {
  cotar: (data) => api.post('/frete/cotar', data),
};

export default api;

