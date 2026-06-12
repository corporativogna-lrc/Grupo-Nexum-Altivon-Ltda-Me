// Helper functions for UI styling and formatting

export const formatPrice = (price) => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(price || 0);
};

export const formatDate = (dateString) => {
  if (!dateString) return '-';
  return new Date(dateString).toLocaleDateString('pt-BR');
};

// Pedido status styling
const PEDIDO_STATUS_STYLES = {
  Entregue: 'bg-green-100 text-green-800',
  Enviado: 'bg-blue-100 text-blue-800',
  Cancelado: 'bg-red-100 text-red-800',
  Pendente: 'bg-yellow-100 text-yellow-800',
  Processando: 'bg-yellow-100 text-yellow-800',
};

export const getPedidoStatusClass = (status) => {
  return PEDIDO_STATUS_STYLES[status] || 'bg-gray-100 text-gray-800';
};

// Lead status styling
const LEAD_STATUS_STYLES = {
  Novo: 'bg-purple-100 text-purple-800',
  Contato: 'bg-blue-100 text-blue-800',
  Qualificado: 'bg-cyan-100 text-cyan-800',
  Negociacao: 'bg-amber-100 text-amber-800',
  Ganho: 'bg-green-100 text-green-800',
  Perdido: 'bg-red-100 text-red-800',
};

export const getLeadStatusClass = (status) => {
  return LEAD_STATUS_STYLES[status] || 'bg-gray-100 text-gray-800';
};

// Metodo de pagamento labels
const PAGAMENTO_LABELS = {
  cartao: 'Cartão de Crédito',
  pix: 'PIX',
  boleto: 'Boleto',
  debito: 'Cartão de Débito',
  deposito: 'Depósito / Transferência',
};

export const getPagamentoLabel = (metodo) => {
  return PAGAMENTO_LABELS[metodo] || metodo;
};
