import axios from 'axios';
import { HTTP_UNAUTHORIZED, STORAGE_KEYS } from '../constants';

const getDefaultApiUrl = () => {
  if (typeof window === 'undefined') return 'http://localhost:5000';

  const { hostname } = window.location;
  const isLocalhost = hostname === 'localhost' || hostname === '127.0.0.1' || hostname === '';

  return isLocalhost ? 'http://localhost:5010' : 'https://api.nexumaltivon.com';
};

export const API_BASE_URL = process.env.REACT_APP_BACKEND_URL || getDefaultApiUrl();
const API_URL = `${API_BASE_URL}/api`;

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
api.interceptors.request.use((config) => {
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
        const response = await axios.post(`${API_URL}/auth/refresh`, {
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

export const produtoAPI = {
  getAll: (params) => api.get('/produtos', { params }),
  getDestaques: () => api.get('/produtos/destaques'),
  getById: (id) => api.get(`/produtos/${id}`),
  create: (data) => api.post('/produtos', data),
  update: (id, data) => api.put(`/produtos/${id}`, data),
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
  create: (data) => api.post('/clientes', data),
};

export const fornecedorAPI = {
  getAll: () => api.get('/fornecedores'),
  create: (data) => api.post('/fornecedores', data),
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

export default api;
