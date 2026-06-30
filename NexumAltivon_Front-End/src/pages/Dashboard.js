/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useEffect, useMemo, useState, useCallback } from 'react';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { categoriaAPI, clienteAPI, comprasAPI, dashboardAPI, empresaGrupoAPI, financeiroAPI, fiscalAPI, fornecedorAPI, integracoesAPI, leadAPI, pedidoAPI, produtoAPI, siteAPI, unwrapApiData } from '../services/api';
import { formatDate, formatPrice, getLeadStatusClass, getPagamentoLabel, getPedidoStatusClass } from '../utils/formatters';
import {
  Activity,
  ArrowDownRight,
  ArrowUpRight,
  Boxes,
  Building2,
  ChevronRight,
  Cog,
  CreditCard,
  Database,
  FileText,
  Globe2,
  Handshake,
  ImagePlus,
  PackagePlus,
  LayoutDashboard,
  LogOut,
  PackageCheck,
  Trash2,
  Save,
  Search,
  MessageCircleMore,
  ShoppingBag,
  Sparkles,
  Truck,
  TrendingUp,
  UserRound,
  Users,
  WalletCards,
} from 'lucide-react';

const tabs = [
  { id: 'overview', label: 'Dashboard', icon: LayoutDashboard, section: 'Principal' },
  { id: 'pedidos', label: 'Pedidos', icon: ShoppingBag, section: 'Principal' },
  { id: 'crm', label: 'CRM', icon: Users, section: 'Marketing & CRM' },
  { id: 'cadastros', label: 'Menu de Cadastros', icon: PackagePlus, section: 'Cadastros', badge: 'novo' },
  { id: 'cadastro-produtos', label: 'Produtos', icon: PackageCheck, section: 'Cadastros' },
  { id: 'cadastro-clientes', label: 'Clientes', icon: Users, section: 'Cadastros' },
  { id: 'cadastro-fornecedores', label: 'Fornecedores', icon: Building2, section: 'Cadastros' },
  { id: 'erp', label: 'ERP', icon: Database, section: 'Gestão', badge: 'desktop' },
  { id: 'erp-financeiro', label: 'Financeiro', icon: WalletCards, section: 'Gestão', badge: 'caixa' },
  { id: 'erp-empresas', label: 'Empresas do Grupo', icon: Building2, section: 'Gestão', badge: 'fiscal' },
  { id: 'erp-fiscal', label: 'Notas e Fiscal', icon: FileText, section: 'Gestão', badge: 'auto' },
  { id: 'erp-logistica', label: 'Logística', icon: Truck, section: 'Gestão', badge: 'estoque' },
  { id: 'erp-rh', label: 'RH', icon: UserRound, section: 'Gestão', badge: 'equipe' },
  { id: 'erp-compras', label: 'Compras', icon: ShoppingBag, section: 'Gestão', badge: 'fornec' },
  { id: 'erp-relatorios', label: 'Relatórios', icon: TrendingUp, section: 'Gestão', badge: 'kpi' },
  { id: 'integracoes', label: 'Integrações', icon: Globe2, section: 'Integrações', badge: 'paralelo' },
  { id: 'configuracoes-site', label: 'Site & Banners', icon: Cog, section: 'Sistema', badge: 'home' },
];

const cadastroTabs = [
  { id: 'produtos', label: 'Produtos', detail: 'Catálogo, estoque, preço e vitrine', icon: PackageCheck, action: 'Abrir tela de produtos' },
  { id: 'clientes', label: 'Clientes', detail: 'Identificação comercial e contatos', icon: Users, action: 'Abrir tela de clientes' },
  { id: 'fornecedores', label: 'Fornecedores', detail: 'Parceiros, dropshipping e logística', icon: Building2, action: 'Abrir tela de fornecedores' },
];

const cadastroHighlights = {
  produtos: [
    'Slug, SKU e nome são conferidos antes de salvar.',
    'Preço, estoque, categoria e imagem já alimentam a vitrine.',
    'A prévia lateral ajuda a revisar antes de gravar.',
  ],
  clientes: [
    'E-mail e CPF/CNPJ são conferidos antes de chegar ao banco.',
    'Se o cliente já existir, o cadastro não duplica.',
    'A base fica pronta para pedidos, CRM e cobrança.',
  ],
  fornecedores: [
    'Documento, e-mail e nome são conferidos para evitar duplicidade.',
    'Segmento ajuda a organizar dropshipping, compras e logística.',
    'A lista lateral mostra o que já está ativo na base real.',
  ],
};

const empresaGrupoHighlights = [
  'CNPJ e código interno são conferidos antes de salvar.',
  'O cadastro reúne fiscal, tributário, contato, emissão e prioridade estratégica.',
  'A base fica pronta para roteamento inteligente de NF-e entre empresas do grupo e parceiras.',
];

const erpDesktopLauncher = 'file:///C:/Users/Rodrigo%20Costa/Documents/Codex/2026-05-28/files-mentioned-by-the-user-nexumaltivon/NexumAltivon.com/ABRIR-ERP-DESKTOP.cmd';

const plannedModules = [
  { label: 'Lojas', icon: Building2, section: 'Gestão' },
  { label: 'Cupons', icon: CreditCard, section: 'Marketing & CRM' },
  { label: 'Marketing', icon: TrendingUp, section: 'Marketing & CRM' },
  { label: 'Configurações', icon: Cog, section: 'Sistema' },
];

const erpModules = [
  {
    title: 'Financeiro',
    tabId: 'erp-financeiro',
    status: 'Operacional assistido',
    icon: WalletCards,
    metrics: ['Fluxo de caixa', 'Contas a pagar', 'Contas a receber', 'DRE'],
    signal: 'Prioridade alta',
  },
  {
    title: 'Fiscal',
    tabId: 'erp-fiscal',
    status: 'Pronto para homologação',
    icon: FileText,
    metrics: ['NF-e entrada', 'NF-e saída', 'CFOP', 'Impostos'],
    signal: 'Motor tributário',
  },
  {
    title: 'Estoque e Logística',
    tabId: 'erp-logistica',
    status: 'Conectado ao pedido',
    icon: Boxes,
    metrics: ['Kardex', 'Inventário', 'Despacho', 'Rastreamento'],
    signal: 'Operação diária',
  },
  {
    title: 'Empresas e Parceiros',
    tabId: 'erp-empresas',
    status: 'Governança ERP',
    icon: Building2,
    metrics: ['Grupo societário', 'Parceiros', 'Contratos', 'Centros de custo'],
    signal: 'Multisocietário',
  },
  {
    title: 'Relatórios',
    tabId: 'erp-relatorios',
    status: 'Gestão executiva',
    icon: TrendingUp,
    metrics: ['Margens', 'Lucro líquido', 'Receita por loja', 'Auditoria'],
    signal: 'Decisão',
  },
  {
    title: 'RH / DP',
    tabId: 'erp-rh',
    status: 'Operação interna',
    icon: UserRound,
    metrics: ['Cargos', 'Equipe', 'Responsáveis', 'Folha'],
    signal: 'Mesa de gestão',
  },
  {
    title: 'Compras',
    tabId: 'erp-compras',
    status: 'Suprimentos e reposição',
    icon: ShoppingBag,
    metrics: ['Fornecedores', 'Solicitações', 'Reposição', 'Custos'],
    signal: 'Abastecimento',
  },
];

const erpWorkspaceNavItems = [
  { id: 'erp', label: 'Painel', short: 'painel', signal: 'Visão geral' },
  { id: 'erp-fiscal', label: 'Fiscal', short: 'nf-e', signal: 'Emissão' },
  { id: 'erp-financeiro', label: 'Financeiro', short: 'caixa', signal: 'Fluxo de caixa' },
  { id: 'erp-logistica', label: 'Logística', short: 'estoque', signal: 'Expedição' },
  { id: 'erp-empresas', label: 'Empresas', short: 'grupo', signal: 'Matriz / filiais' },
  { id: 'erp-compras', label: 'Compras', short: 'supr.', signal: 'Reposição' },
  { id: 'erp-relatorios', label: 'Relatórios', short: 'kpi', signal: 'Gestão' },
  { id: 'erp-rh', label: 'RH', short: 'equipe', signal: 'Equipe' },
];

const erpCockpitActions = [
  { id: 'erp-fiscal', label: 'Emissão fiscal', detail: 'Fila, emitente e automação NF-e' },
  { id: 'erp-financeiro', label: 'Caixa / Financeiro', detail: 'Fluxo de caixa, liquidação e conciliação' },
  { id: 'erp-logistica', label: 'Expedição / Estoque', detail: 'Separação, despacho e rastreio' },
  { id: 'erp-empresas', label: 'Cadastro societário', detail: 'Grupo, filiais e regras tributárias' },
  { id: 'erp-compras', label: 'Compras e custo', detail: 'Reposição, margem e fornecedores' },
];

const fallbackIntegracoes = [
  { nome: 'E-commerce e API', slug: 'ecommerce', status: 'Operacional', detalhe: 'Catálogo, clientes, pedidos, estoque e painel usam a API operacional.', configurada: true, ambiente: 'Produção' },
  { nome: 'Dropshipping', slug: 'dropshipping', status: 'Aguardando cadastros', detalhe: 'Roteamento depende dos fornecedores e produtos vinculados.', configurada: false, ambiente: 'Produção assistida' },
  { nome: 'Shopify', slug: 'shopify', status: 'Aguardando conexão', detalhe: 'Canal preparado para receber domínio da loja, token Admin API e webhooks.', configurada: false, ambiente: 'Staging privado' },
  { nome: 'CJ Dropshipping', slug: 'cjdropshipping', status: 'Aguardando conexão', detalhe: 'Canal preparado para receber endpoint, token e vínculo de catálogo.', configurada: false, ambiente: 'Staging privado' },
  { nome: 'Logística e Fretes', slug: 'logistica', status: 'Aguardando credenciais', detalhe: 'Frete está registrado no checkout; cotação e etiqueta dependem da transportadora.', configurada: false, ambiente: 'Sandbox' },
  { nome: 'Gateways de pagamento', slug: 'gateways', status: 'Aguardando credenciais', detalhe: 'Pedido registra método; cobrança real depende do token e webhook.', configurada: false, ambiente: 'Não configurado' },
  { nome: 'Certificado NF-e', slug: 'certificado', status: 'Aguardando configuração', detalhe: 'O emissor fiscal está pronto para A1 ou A3, mas depende do certificado da empresa.', configurada: false, ambiente: 'Servidor local' },
  { nome: 'Marketplaces', slug: 'marketplaces', status: 'Aguardando credenciais', detalhe: 'Sincronização de catálogo e pedidos depende da autorização externa.', configurada: false, ambiente: 'Integração externa' },
  { nome: 'Bancos e conciliação', slug: 'bancaria', status: 'Planejado', detalhe: 'Conciliação depende da definição do banco e convênio.', configurada: false, ambiente: 'Não configurado' },
];

const integrationGuides = {
  ecommerce: {
    description: 'Operação central de catálogo, clientes, pedidos, estoque e painel administrativo.',
    requirements: ['API respondendo continuamente', 'Banco de dados conectado', 'Domínio público estável'],
    nextTest: 'Criar um pedido controlado e conferir cliente, pedido e reserva de estoque.',
  },
  dropshipping: {
    description: 'Roteamento de produtos e pedidos para fornecedores parceiros.',
    requirements: ['Fornecedor ativo', 'Produto vinculado ao fornecedor', 'Regra de custo, prazo e comissão'],
    nextTest: 'Vincular um produto real a um fornecedor e simular o envio do pedido.',
  },
  shopify: {
    description: 'Canal privado para sincronizar catálogo, estoque e pedidos da Shopify com a operação Nexum.',
    requirements: ['Domínio da loja Shopify', 'Admin API Access Token', 'ApiVersion válida', 'Webhook de pedido/produto/estoque'],
    nextTest: 'Preencher StoreDomain e AdminApiAccessToken, depois validar o retorno de shop.json.',
  },
  cjdropshipping: {
    description: 'Canal privado para importar catálogo, roteamento de sourcing e pedidos com o CJ Dropshipping.',
    requirements: ['Endpoint contratado do CJ', 'AccessToken ou API key', 'Produtos vinculados ao canal', 'Regra de preço e prazo'],
    nextTest: 'Inserir endpoint/token reais e marcar os primeiros produtos aptos ao canal CJ.',
  },
  logistica: {
    description: 'Cotação de frete, seleção de serviço, etiqueta e rastreamento.',
    requirements: ['Token da transportadora', 'Endereço de origem', 'Peso e dimensões dos produtos'],
    nextTest: 'Executar uma cotação em sandbox e validar prazo e valor retornados.',
  },
  certificado: {
    description: 'Validação do certificado digital usado para emissão de NF-e em produção.',
    requirements: ['Certificado A1 com PFX e senha ou A3 com thumbprint', 'CNPJ da empresa emissora', 'Arquivo ou token acessível no servidor'],
    nextTest: 'Preencher os dados do certificado e validar o diagnóstico de prontidão.',
  },
  gateways: {
    description: 'Cobrança por Pix, boleto e cartão, com retorno automático do pagamento e contingência entre provedores.',
    requirements: ['Credencial do gateway principal', 'Credencial do gateway reserva', 'Webhook público seguro', 'Conta comercial homologada'],
    nextTest: 'Criar cobrança de baixo valor em sandbox e confirmar o webhook.',
  },
  mercadopago: {
    description: 'Gateway de pagamento para Pix, boleto e cartão por API oficial do Mercado Pago.',
    requirements: ['Access token oficial', 'Conta comercial homologada', 'Webhook público seguro'],
    nextTest: 'Validar credencial e criar uma cobrança controlada de baixo valor.',
  },
  marketplaces: {
    description: 'Sincronização de catálogo, estoque e pedidos de canais externos.',
    requirements: ['Aplicação cadastrada no marketplace', 'Autorização OAuth', 'Regras de preço e estoque'],
    nextTest: 'Autorizar uma conta de teste e importar um anúncio controlado.',
  },
  mercadolivre: {
    description: 'Canal de marketplace para sincronizar catálogo, estoque e pedidos autorizados do Mercado Livre.',
    requirements: ['Aplicação Mercado Livre', 'OAuth do vendedor', 'Access token/refresh token armazenados no servidor'],
    nextTest: 'Concluir OAuth do vendedor e chamar users/me para validar a autorização.',
  },
  melhorenvio: {
    description: 'Hub logístico para cotação, compra de frete, etiqueta e rastreamento pelo Melhor Envio.',
    requirements: ['Token Melhor Envio', 'Endereço de origem', 'Peso e dimensões reais dos produtos'],
    nextTest: 'Executar cotação em sandbox/produção e conferir prazo e valor retornados.',
  },
  bancaria: {
    description: 'Conciliação de recebimentos, tarifas e movimentações financeiras.',
    requirements: ['Banco e produto definidos', 'Convênio/API contratado', 'Credenciais armazenadas no servidor'],
    nextTest: 'Definir a primeira instituição e o formato de conciliação.',
  },
};

const navSections = ['Principal', 'Cadastros', 'Gestão', 'Marketing & CRM', 'Integrações', 'Sistema'];
const validTabIds = new Set(tabs.map((tab) => tab.id));
const validCadastroIds = new Set(cadastroTabs.map((tab) => tab.id));
const integrationCredentialCategoryMap = {
  gateways: ['gateway'],
  mercadopago: ['gateway'],
  logistica: ['logistica'],
  melhorenvio: ['logistica'],
  dropshipping: ['dropshipping'],
  shopify: ['dropshipping'],
  cjdropshipping: ['dropshipping'],
  marketplaces: ['marketplace'],
  mercadolivre: ['marketplace'],
  bancaria: ['bancaria'],
};

const isProdutoPublicavel = (produto) =>
  Boolean(
    produto &&
      produto.ativo !== false &&
      produto.nome &&
      produto.sku &&
      (produto.slug || produto.id) &&
      (produto.descricao_curta || produto.descricaoCurta || produto.descricao_longa || produto.descricaoLonga || produto.descricao) &&
      (produto.imagem_url || produto.imagemUrl || produto.imagem || produto.imagemPrincipal || produto.imagem_principal) &&
      Number(produto.preco) > 0 &&
      Number(produto.peso) > 0 &&
      Number(produto.altura) > 0 &&
      Number(produto.largura) > 0 &&
      Number(produto.comprimento) > 0,
  );

const fallbackClientes = [
  { id: 1, nome: 'Ana Carolina Silva', email: 'ana.silva@email.com', telefone: '(14) 99876-5432', cpf: '123.456.789-00' },
  { id: 2, nome: 'Bruno Oliveira', email: 'bruno.oliveira@email.com', telefone: '(14) 99765-4321', cpf: '234.567.890-11' },
  { id: 3, nome: 'Carla Mendes', email: 'carla.mendes@email.com', telefone: '(14) 99654-3210', cpf: '345.678.901-22' },
];

const fallbackFornecedores = [
  { id: 1, nome: 'Chronos Imports', documento: '12.345.678/0001-90', email: 'comercial@chronosimports.com', telefone: '(11) 3030-1122', categoria: 'Relogios' },
  { id: 2, nome: 'Luxury Cases Brasil', documento: '98.765.432/0001-10', email: 'vendas@luxurycases.com', telefone: '(21) 4040-2211', categoria: 'Acessorios' },
];

const fallbackEmpresasGrupo = [];

const emptyProduto = {
  id: '',
  nome: '',
  descricaoCurta: '',
  descricao: '',
  preco: '',
  precoPromocional: '',
  custo: '',
  peso: '',
  altura: '',
  largura: '',
  comprimento: '',
  imagemUrl: '',
  imagensGaleria: '',
  estoque: '',
  estoqueMinimo: '5',
  estoqueReservado: '0',
  destaque: true,
  ativo: true,
  sku: '',
  categoriaId: 'classicos',
  subcategoriaId: '',
  tipoProduto: 'Proprio',
  empresaAquisicaoCodigo: '',
  fornecedorId: '',
  marca: '',
  tags: '',
  seoTitulo: '',
  seoDescricao: '',
  seoKeywords: '',
};

const emptyCategoria = {
  id: '',
  nome: '',
  descricao: '',
  categoriaPaiId: '',
  ordem: '0',
};

const emptyCliente = { nome: '', email: '', telefone: '', cpf: '' };
const emptyFornecedor = { nome: '', documento: '', email: '', telefone: '', categoria: 'Geral' };
const emptyLead = { nome: '', email: '', telefone: '', status: 'Novo', origem: 'Site', observacao: '' };
const emptyCompraSolicitacao = {
  produtoId: '',
  produtoNome: '',
  quantidade: '1',
  origem: 'EstoqueFisico',
  finalidade: 'Reposicao/operacao',
  prioridade: 'Normal',
  observacoes: '',
};
const emptyCompraCotacao = {
  fornecedorId: '',
  produtoId: '',
  produtoNome: '',
  quantidade: '1',
  custoUnitario: '',
  origem: 'EstoqueFisico',
  finalidade: 'Reposicao/operacao',
  prioridade: 'Normal',
  prazoEntregaDias: '7',
  observacoes: '',
};
const emptyCompraPedido = {
  fornecedorId: '',
  solicitacaoId: '',
  produtoId: '',
  produtoNome: '',
  sku: '',
  quantidade: '1',
  custoUnitario: '',
  origem: 'EstoqueFisico',
  finalidade: 'Reposicao/operacao',
  dataPrevistaEntrega: '',
  dataVencimento: '',
  meioPagamento: 'A definir',
  observacoes: '',
};
const emptyCompraEntrada = {
  pedidoId: '',
  numeroDocumento: '',
  chaveNfeEntrada: '',
  tipoEntrada: 'EstoqueFisico',
  recebidoPor: 'Operacao',
  observacoes: '',
  itens: {},
};
const compraSolicitacaoStatusOptions = ['Aberta', 'Cotado', 'Aprovada', 'Atendida', 'Cancelada', 'Fechada'];
const compraPedidoStatusOptions = ['Aberto', 'Aprovado', 'RecebidoParcial', 'Recebido', 'Cancelado', 'Fechado'];
const emptyEmpresaGrupo = {
  tipoCadastro: 'GrupoSocietario',
  razaoSocial: '',
  nomeFantasia: '',
  cnpj: '',
  inscricaoEstadual: '',
  inscricaoMunicipal: '',
  matrizFilial: 'Matriz',
  codigoEmpresa: '',
  regimeTributario: 'Simples Nacional',
  crt: '1',
  cnaePrincipal: '',
  cnaesSecundarios: '',
  categoriaFiscal: '',
  subcategoriaFiscal: '',
  ncmPadrao: '',
  naturezaOperacaoPadrao: 'Venda de mercadoria',
  responsavelLegal: '',
  responsavelFiscal: '',
  emailFiscal: 'corporativo.gna@gmail.com',
  emailComercial: 'corporativo.gna@gmail.com',
  telefone: '',
  whatsapp: '',
  cep: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  cidade: '',
  estado: '',
  pais: 'Brasil',
  ambienteNfe: 'Homologacao',
  serieNfe: '1',
  serieNfce: '1',
  modeloDocumentoPdv: 'NFCe',
  ambienteNfce: 'Homologacao',
  proximaNfceNumero: '1',
  nfceCsc: '',
  nfceCscIdToken: '',
  pdvSerieSat: '',
  pdvImpressoraFiscal: '',
  pdvNomeCaixaPadrao: 'Caixa 01',
  pdvContingenciaOffline: true,
  proximaNfeNumero: '1',
  cfopPadraoInterno: '5102',
  cfopPadraoInterestadual: '6102',
  aliquotaIcmsInterna: '',
  aliquotaIcmsInterestadual: '',
  aliquotaPis: '',
  aliquotaCofins: '',
  aliquotaIss: '',
  aliquotaIpi: '',
  cargaTributariaPercentual: '',
  perfilTributacao: 'TributacaoAtual',
  usaStLegado: false,
  destacaIcmsStSeparado: false,
  custoOperacionalPercentual: '',
  margemMinimaPercentual: '',
  prioridadeFiscal: '100',
  permiteNfeEntrada: true,
  permiteNfeSaida: true,
  permiteDropshipping: false,
  permiteMarketplace: false,
  emitentePreferencial: false,
  ativa: true,
  beneficiosEstrategicos: '',
  contratoResumo: '',
  observacoes: '',
};
const emptySiteConfigForm = {
  site_nome: 'Grupo Nexum Altivon',
  site_email_contato: 'corporativo.gna@gmail.com',
  site_telefone: '(14) 99673-1879',
  site_telefone_secundario: '(14) 99634-8409',
  site_whatsapp: '5514996731879',
  site_whatsapp_secundario: '5514996348409',
  site_yara_email: 'corporativo.gna@gmail.com',
  site_logo: '/imagens/homepage/Logo-2.png',
  site_subtitulo: 'Participações societárias',
  site_institucional_url: '/institucional',
  site_politica_privacidade_url: '/politica-privacidade',
  site_politica_reembolso_url: '/politica-reembolso',
  home_intro_titulo: 'Uma Nova Era Começa',
  home_intro_texto_1: 'O Grupo Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.',
  home_intro_texto_2: 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.',
  home_intro_badge: 'nexumaltivon.com.br',
  home_footer_texto: 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.',
  home_quality_items: '["Curadoria rigorosa de fornecedores","Atendimento humano e especializado","Política de devolução simplificada","Preços justos e acessíveis"]',
  home_lojas_cards: '[{"nome":"Gran Tur","slug":"gran-tur","segmento":"Viagens & Turismo","descricao":"Mochilas, malas, acessórios de viagem e produtos para explorar o mundo com estilo e conforto.","imagem":"/imagens/homepage/loja-gran-tur.svg","icon":"Plane"},{"nome":"Chronos","slug":"chronos","segmento":"Relógios & Acessórios","descricao":"Relógios e acessórios para quem valoriza precisão, presença e elegância.","imagem":"/imagens/homepage/loja-chronos.svg","icon":"Watch"},{"nome":"Moda Mim","slug":"moda-mim","segmento":"Moda & Vestuário","descricao":"Roupas, calçados e acessórios para uma experiência de compra prática e atual.","imagem":"/imagens/homepage/loja-moda-mim.svg","icon":"Shirt"},{"nome":"Geração Top+","slug":"geracao-top","segmento":"Tecnologia & Gadgets","descricao":"Smartphones, eletrônicos, acessórios e tecnologia para rotina, trabalho e lazer.","imagem":"/imagens/homepage/loja-geracao-top.svg","icon":"Smartphone"},{"nome":"Estruturaline","slug":"estruturaline","segmento":"Construção & Estruturas","descricao":"Materiais, ferramentas e soluções para quem constrói com seriedade.","imagem":"/imagens/homepage/loja-estruturaline.svg","icon":"Hammer"},{"nome":"Gran Festas","slug":"gran-festas","segmento":"Festas & Eventos","descricao":"Decoração, utensílios e produtos para encontros, comemorações e eventos.","imagem":"/imagens/homepage/loja-gran-festas.svg","icon":"Gift"}]',
  home_partner_cards: '[{"title":"Parceiros de Vendas","text":"Lojas físicas ou online podem ampliar seus horizontes de venda com nossa infraestrutura comercial e operação integrada.","cta":"Quero Vender","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em ser parceiro de vendas do Grupo Nexum Altivon.","icon":"Store"}]',
  home_hero_slides: '[{"id":"ecommerce","badge":"Grupo Nexum Altivon","title":"O Futuro do","highlight":"E-Commerce","description":"Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.","image":"/imagens/homepage/banner-ecommerce.svg"},{"id":"marcas","badge":"6 marcas em expansão","title":"Uma operação,","highlight":"múltiplos mercados","description":"Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.","image":"/imagens/homepage/banner-marcas.svg"},{"id":"tecnologia","badge":"Experiência tecnológica","title":"Compra segura com","highlight":"atendimento humano","description":"Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.","image":"/imagens/homepage/banner-atendimento.svg"}]',
};
const siteConfigFieldMeta = [
  { key: 'site_nome', label: 'Nome do site', type: 'text', group: 'Geral', description: 'Nome público principal da operação.' },
  { key: 'site_email_contato', label: 'E-mail público', type: 'text', group: 'Geral', description: 'Destino principal das mensagens do site.' },
  { key: 'site_telefone', label: 'Telefone Rodrigo', type: 'text', group: 'Geral', description: 'Contato principal exibido na home.' },
  { key: 'site_telefone_secundario', label: 'Telefone Vinicios', type: 'text', group: 'Geral', description: 'Contato secundário exibido na home.' },
  { key: 'site_whatsapp', label: 'WhatsApp principal', type: 'text', group: 'Geral', description: 'Número usado em links do site.' },
  { key: 'site_whatsapp_secundario', label: 'WhatsApp secundário', type: 'text', group: 'Geral', description: 'Número usado em parceria/fornecedores.' },
  { key: 'site_yara_email', label: 'E-mail da Yara', type: 'text', group: 'Atendimento', description: 'Canal atual da Yara para atendimento comercial.' },
  { key: 'site_logo', label: 'Logo do site', type: 'text', group: 'Geral', description: 'URL pública ou caminho relativo do logo exibido na home.', imagePicker: true },
  { key: 'site_subtitulo', label: 'Subtítulo da marca', type: 'text', group: 'Geral', description: 'Texto discreto abaixo do nome público da empresa.' },
  { key: 'site_institucional_url', label: 'Link institucional', type: 'text', group: 'SiteHome', description: 'Destino do link institucional da home.' },
  { key: 'site_politica_privacidade_url', label: 'Link privacidade', type: 'text', group: 'SiteHome', description: 'Destino da política de privacidade.' },
  { key: 'site_politica_reembolso_url', label: 'Link reembolso', type: 'text', group: 'SiteHome', description: 'Destino da política de reembolso.' },
  { key: 'home_intro_titulo', label: 'Título institucional', type: 'text', group: 'SiteHome', description: 'Título principal do bloco institucional.' },
  { key: 'home_intro_texto_1', label: 'Texto institucional 1', type: 'textarea', group: 'SiteHome', description: 'Primeiro texto institucional da home.' },
  { key: 'home_intro_texto_2', label: 'Texto institucional 2', type: 'textarea', group: 'SiteHome', description: 'Segundo texto institucional da home.' },
  { key: 'home_intro_badge', label: 'Selo institucional', type: 'text', group: 'SiteHome', description: 'Texto do selo abaixo do bloco institucional.' },
  { key: 'home_footer_texto', label: 'Rodapé público', type: 'textarea', group: 'SiteHome', description: 'Mensagem institucional no rodapé da home.' },
  { key: 'home_quality_items', label: 'Itens de qualidade (JSON)', type: 'textarea', group: 'SiteHome', description: 'Array JSON de frases do bloco de qualidade.' },
  { key: 'home_lojas_cards', label: 'Lojas e imagens (JSON)', type: 'textarea', group: 'SiteHome', description: 'Array JSON com nome, slug, segmento, descrição, imagem e icon das lojas.', imagePicker: true },
  { key: 'home_partner_cards', label: 'Cards de parceria (JSON)', type: 'textarea', group: 'SiteHome', description: 'Array JSON com title, text, cta, href e icon.' },
  { key: 'home_hero_slides', label: 'Slides do banner (JSON)', type: 'textarea', group: 'SiteHome', description: 'Array JSON com id, badge, title, highlight, description e image.', imagePicker: true },
];

const toHomepageImagePath = (fileName = '') => {
  const safeName = String(fileName)
    .trim()
    .replace(/\\/g, '/')
    .split('/')
    .pop()
    .replace(/\s+/g, '-')
    .replace(/[^a-zA-Z0-9._-]/g, '');

  return safeName ? `/imagens/homepage/${safeName}` : '';
};

const resolveSiteLogoPreview = (logo) => {
  const value = String(logo || '').trim();
  return value && !value.includes('logo-grupo-nexum-altivon.svg') ? value : '/imagens/homepage/Logo-2.png';
};

const applyImagePathToSiteConfig = (fieldKey, currentValue, imagePath) => {
  if (!imagePath) return currentValue || '';
  if (fieldKey === 'site_logo') return imagePath;

  try {
    const parsed = JSON.parse(currentValue || '[]');
    if (Array.isArray(parsed) && parsed.length > 0) {
      const imageField = fieldKey === 'home_lojas_cards' ? 'imagem' : 'image';
      return JSON.stringify(
        parsed.map((item, index) => (index === 0 ? { ...item, [imageField]: imagePath } : item)),
        null,
        2
      );
    }
  } catch {
    return currentValue || '';
  }

  return currentValue || '';
};
const pedidoStatusOptions = ['Pendente', 'Pago', 'Em Separacao', 'Enviado', 'Entregue', 'Cancelado', 'Devolvido', 'Reembolsado'];
const pedidoFilaOptions = [
  { id: 'todos', label: 'Todos' },
  { id: 'pendente', label: 'Pendentes' },
  { id: 'pago', label: 'Pagos' },
  { id: 'separacao', label: 'Separacao' },
  { id: 'enviado', label: 'Enviados' },
  { id: 'sem-rastreio', label: 'Sem rastreio' },
];
const leadStatusOptions = ['Novo', 'Contato', 'Qualificado', 'Negociacao', 'Ganho', 'Perdido'];
const emptyResumo = {
  pedidos_hoje: 0,
  total_clientes: 0,
  faturamento_mes: 0,
  leads_novos: 0,
  produtos_estoque_baixo: 0,
  conversao: 0,
  ticket_medio: 0,
};

const emptyFinanceiroLancamento = {
  tipo: 'Receita',
  status: 'Pendente',
  categoria: '',
  descricao: '',
  valor: '',
  dataVencimento: '',
  dataPagamento: '',
  meioPagamento: '',
  contaBancaria: '',
  comprovanteUrl: '',
  observacoes: '',
  pedidoId: '',
};

const chart = [
  { label: 'Seg', value: 42 },
  { label: 'Ter', value: 68 },
  { label: 'Qua', value: 54 },
  { label: 'Qui', value: 83 },
  { label: 'Sex', value: 76 },
  { label: 'Sáb', value: 92 },
  { label: 'Dom', value: 61 },
];

const normalizeText = (value) => String(value ?? '').trim().toLowerCase();
const normalizeDocument = (value) => String(value ?? '').replace(/\D/g, '');
const galleryToArray = (value) =>
  String(value ?? '')
    .split(/\r?\n/)
    .map((item) => item.trim())
    .filter(Boolean);
const galleryToText = (items) => items.filter(Boolean).join('\n');
const parseJsonPreview = (value, fallback) => {
  if (!value) return fallback;

  try {
    const parsed = JSON.parse(value);
    return Array.isArray(parsed) ? parsed : fallback;
  } catch {
    return fallback;
  }
};
const normalizePreviewImage = (value) => String(value || '').trim();
const asArray = (data) => {
  const normalized = unwrapApiData(data);
  return Array.isArray(normalized) ? normalized : [];
};
const getFinanceiroValor = (item) => Number(item?.valor ?? item?.Valor ?? 0) || 0;
const getFinanceiroTipo = (item) => String(item?.tipo ?? item?.Tipo ?? '').trim();
const getFinanceiroStatus = (item) => String(item?.status ?? item?.Status ?? '').trim();
const getFinanceiroDescricao = (item) => item?.descricao ?? item?.Descricao ?? '-';
const getFinanceiroCategoria = (item) => item?.categoria ?? item?.Categoria ?? '-';
const getFinanceiroVencimento = (item) => item?.dataVencimento ?? item?.data_vencimento ?? item?.DataVencimento;
const getFinanceiroPagamento = (item) => item?.dataPagamento ?? item?.data_pagamento ?? item?.DataPagamento;
const getFinanceiroNumeroPedido = (item) => item?.numeroPedido ?? item?.numero_pedido ?? item?.NumeroPedido;
const getFinanceiroPedidoId = (item) => item?.pedidoId ?? item?.pedido_id ?? item?.PedidoId;

const getDashboardRouteState = (path = '') => {
  const segments = String(path || '').split('/').filter(Boolean);

  if (segments[0] === 'integracoes' && segments[1]) {
    return { activeTab: 'integracoes', activeCadastroTab: 'produtos', activeIntegration: segments[1] };
  }

  if (segments[0] === 'cadastros' && validCadastroIds.has(segments[1])) {
    return { activeTab: `cadastro-${segments[1]}`, activeCadastroTab: segments[1], activeIntegration: '' };
  }

  if (segments[0] === 'cadastros') {
    return { activeTab: 'cadastros', activeCadastroTab: 'produtos', activeIntegration: '' };
  }

  if (validTabIds.has(segments[0])) {
    const cadastroId = segments[0].replace('cadastro-', '');
    return {
      activeTab: segments[0],
      activeCadastroTab: validCadastroIds.has(cadastroId) ? cadastroId : 'produtos',
      activeIntegration: '',
    };
  }

  return { activeTab: 'overview', activeCadastroTab: 'produtos', activeIntegration: '' };
};

const getDashboardPath = (tabId, cadastroId = 'produtos') => {
  if (tabId === 'overview') return '/dashboard';
  if (tabId === 'cadastros') return '/dashboard/cadastros';
  if (tabId.startsWith('cadastro-')) return `/dashboard/cadastros/${cadastroId}`;
  return `/dashboard/${tabId}`;
};

const normalizarCodigoSku = (value, fallback = 'NEXUM') => {
  const normalized = String(value || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/gi, '')
    .toUpperCase();

  return (normalized || fallback).slice(0, 10);
};

const getProdutoPrefixoAquisicao = (tipoProduto, fornecedorId) => {
  const valorComparacao = `${tipoProduto ?? ''} ${fornecedorId ? 'fornecedor' : ''}`.toLowerCase();

  if (valorComparacao.includes('drop')) return 'DROP';
  if (valorComparacao.includes('marketplace') || valorComparacao.includes('afiliado')) return 'ECOM';
  if (fornecedorId) return 'DIST';
  return 'ECOM';
};

const normalizarSkuInterno = (form, empresasGrupo = []) => {
  const empresaSelecionada = empresasGrupo.find((empresa) => String(empresa.id ?? empresa.Id ?? '') === String(form.empresaAquisicaoCodigo || ''));
  const empresaCodigo = normalizarCodigoSku(
    empresaSelecionada?.codigoEmpresa
    || empresaSelecionada?.nomeFantasia
    || empresaSelecionada?.razaoSocial
    || form.empresaAquisicaoCodigo
    || 'NEXUM',
  );
  const prefixo = getProdutoPrefixoAquisicao(form.tipoProduto, form.fornecedorId);
  const bruto = String(form.sku || form.id || form.nome || '').trim();
  const semPrefixo = bruto.replace(/^[a-z0-9]{2,10}[\s\-_]+(ecom|drop|dist)[\s\-_]+/i, '');
  const corpo = semPrefixo
    .replace(/\D+/g, '');
  const sequencial = corpo ? corpo.slice(-6).padStart(6, '0') : Date.now().toString().slice(-6);

  return `${empresaCodigo}-${prefixo}-${sequencial}`;
};

const fileToDataUrl = (file) =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = () => reject(new Error('Falha ao ler imagem.'));
    reader.readAsDataURL(file);
  });

const getProdutoDuplicateMessage = (form, produtos, ignoreId = '') => {
  const sku = normalizeText(form.sku);
  const id = normalizeText(form.id);
  const nome = normalizeText(form.nome);
  const ignoreKey = normalizeText(ignoreId);

  if (sku) {
    const duplicate = produtos.find((produto) => normalizeText(produto.sku) === sku && normalizeText(produto.id) !== ignoreKey);
    if (duplicate) return `SKU já usado em ${duplicate.nome || 'outro produto'}.`;
  }

  if (id) {
    const duplicate = produtos.find((produto) => normalizeText(produto.id) === id && normalizeText(produto.id) !== ignoreKey);
    if (duplicate) return `Identificador público já usado em ${duplicate.nome || 'outro produto'}.`;
  }

  if (nome) {
    const duplicate = produtos.find((produto) => normalizeText(produto.nome) === nome && normalizeText(produto.id) !== ignoreKey);
    if (duplicate) return 'Produto com este nome já existe no catálogo.';
  }

  return '';
};

const getClienteDuplicateMessage = (form, clientes, ignoreId = '') => {
  const email = normalizeText(form.email);
  const documento = normalizeDocument(form.cpf);
  const ignoreKey = normalizeText(ignoreId);

  if (email) {
    const duplicate = clientes.find((cliente) => normalizeText(cliente.email) === email && normalizeText(cliente.id) !== ignoreKey);
    if (duplicate) return `Cliente já cadastrado com este e-mail: ${duplicate.nome || duplicate.email}.`;
  }

  if (documento) {
    const duplicate = clientes.find((cliente) => (normalizeDocument(cliente.cpf) === documento || normalizeDocument(cliente.cpfCnpj) === documento) && normalizeText(cliente.id) !== ignoreKey);
    if (duplicate) return `Cliente já cadastrado com este CPF/CNPJ: ${duplicate.nome || duplicate.email}.`;
  }

  return '';
};

const getFornecedorDuplicateMessage = (form, fornecedores, ignoreId = '') => {
  const documento = normalizeDocument(form.documento);
  const email = normalizeText(form.email);
  const nome = normalizeText(form.nome);
  const ignoreKey = normalizeText(ignoreId);

  if (documento) {
    const duplicate = fornecedores.find((fornecedor) => (normalizeDocument(fornecedor.documento) === documento || normalizeDocument(fornecedor.cnpj) === documento) && normalizeText(fornecedor.id) !== ignoreKey);
    if (duplicate) return `Fornecedor já cadastrado com este documento: ${duplicate.nome || duplicate.email}.`;
  }

  if (email) {
    const duplicate = fornecedores.find((fornecedor) => normalizeText(fornecedor.email) === email && normalizeText(fornecedor.id) !== ignoreKey);
    if (duplicate) return `Fornecedor já cadastrado com este e-mail: ${duplicate.nome || duplicate.email}.`;
  }

  if (nome) {
    const duplicate = fornecedores.find((fornecedor) => normalizeText(fornecedor.nome) === nome && normalizeText(fornecedor.id) !== ignoreKey);
    if (duplicate) return 'Fornecedor com este nome já existe na base.';
  }

  return '';
};

const getEmpresaGrupoDuplicateMessage = (form, empresas) => {
  const cnpj = normalizeDocument(form.cnpj);
  const codigoEmpresa = normalizeText(form.codigoEmpresa);
  const razaoSocial = normalizeText(form.razaoSocial);

  if (cnpj) {
    const duplicate = empresas.find((empresa) => normalizeDocument(empresa.cnpj) === cnpj);
    if (duplicate) return `Empresa já cadastrada com este CNPJ: ${duplicate.razaoSocial || duplicate.nomeFantasia || duplicate.cnpj}.`;
  }

  if (codigoEmpresa) {
    const duplicate = empresas.find((empresa) => normalizeText(empresa.codigoEmpresa) === codigoEmpresa);
    if (duplicate) return `Código interno já vinculado a ${duplicate.razaoSocial || duplicate.nomeFantasia || duplicate.cnpj}.`;
  }

  if (razaoSocial) {
    const duplicate = empresas.find((empresa) => normalizeText(empresa.razaoSocial) === razaoSocial);
    if (duplicate) return 'Já existe empresa com esta razão social na base fiscal.';
  }

  return '';
};

function StatCard({ title, value, detail, icon: Icon, trend, tone = 'slate', onClick }) {
  const toneClass = {
    slate: 'bg-slate-950 text-white',
    amber: 'bg-amber-400 text-slate-950',
    emerald: 'bg-emerald-600 text-white',
    indigo: 'bg-indigo-600 text-white',
  }[tone];

  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-lg border border-slate-200 bg-white p-5 text-left shadow-sm transition ${onClick ? 'cursor-pointer hover:-translate-y-0.5 hover:border-[#C9A227] hover:shadow-md' : ''}`}
      data-testid={`dashboard-stat-${title}`}
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">{title}</p>
          <p className="mt-3 text-2xl font-black text-slate-950 sm:text-3xl">{value}</p>
        </div>
        <div className={`flex h-10 w-10 items-center justify-center rounded-lg sm:h-11 sm:w-11 ${toneClass}`}>
          <Icon size={20} className="sm:h-[22px] sm:w-[22px]" />
        </div>
      </div>
      <div className="mt-5 flex items-center gap-2 text-sm font-bold text-slate-500">
        {trend >= 0 ? <ArrowUpRight className="text-emerald-600" size={17} /> : <ArrowDownRight className="text-rose-600" size={17} />}
        <span className={trend >= 0 ? 'text-emerald-700' : 'text-rose-700'}>{Math.abs(trend)}%</span>
        <span>{detail}</span>
      </div>
    </button>
  );
}

function StatMiniCard({ label, value, onClick }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`erp-mini-card rounded-lg border border-slate-200 bg-white p-5 text-left shadow-sm transition ${onClick ? 'cursor-pointer hover:-translate-y-0.5 hover:border-[#C9A227] hover:shadow-md' : ''}`}
    >
      <p className="text-xs font-black uppercase tracking-[0.16em] text-slate-500">{label}</p>
      <p className="mt-3 text-2xl font-black text-slate-950 sm:text-3xl">{value}</p>
    </button>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();
  const params = useParams();
  const { user, isAuthenticated, loading: authLoading, logout } = useAuth();
  const routeState = useMemo(() => getDashboardRouteState(params['*']), [params]);
  const sophiaHref = 'mailto:corporativo.gna@gmail.com?subject=Sophia%20-%20Apoio%20ERP&body=Ol%C3%A1%20Sophia%2C%20preciso%20de%20apoio%20interno%20na%20opera%C3%A7%C3%A3o%20do%20GenesisGest.Net.';
  const [resumo, setResumo] = useState(emptyResumo);
  const [pedidos, setPedidos] = useState([]);
  const [leads, setLeads] = useState([]);
  const [produtos, setProdutos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [fornecedores, setFornecedores] = useState([]);
  const [empresasGrupo, setEmpresasGrupo] = useState([]);
  const [financeiroLancamentos, setFinanceiroLancamentos] = useState([]);
  const [produtoEditingId, setProdutoEditingId] = useState('');
  const [clienteEditingId, setClienteEditingId] = useState('');
  const [fornecedorEditingId, setFornecedorEditingId] = useState('');
  const [fiscalPedidos, setFiscalPedidos] = useState([]);
  const [comprasPainel, setComprasPainel] = useState(null);
  const [integracoes, setIntegracoes] = useState(fallbackIntegracoes);
  const [credenciaisModelo, setCredenciaisModelo] = useState([]);
  const [siteConfigItems, setSiteConfigItems] = useState([]);
  const [siteConfigForm, setSiteConfigForm] = useState(emptySiteConfigForm);
  const [loading, setLoading] = useState(true);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [activeTab, setActiveTab] = useState(routeState.activeTab);
  const [activeCadastroTab, setActiveCadastroTab] = useState(routeState.activeCadastroTab);
  const [activeIntegration, setActiveIntegration] = useState(routeState.activeIntegration);
  const [query, setQuery] = useState('');
  const [pedidoFila, setPedidoFila] = useState('todos');
  const [produtoForm, setProdutoForm] = useState(emptyProduto);
  const [categoriaForm, setCategoriaForm] = useState(emptyCategoria);
  const [clienteForm, setClienteForm] = useState(emptyCliente);
  const [fornecedorForm, setFornecedorForm] = useState(emptyFornecedor);
  const [leadForm, setLeadForm] = useState(emptyLead);
  const [compraSolicitacaoForm, setCompraSolicitacaoForm] = useState(emptyCompraSolicitacao);
  const [compraCotacaoForm, setCompraCotacaoForm] = useState(emptyCompraCotacao);
  const [compraPedidoForm, setCompraPedidoForm] = useState(emptyCompraPedido);
  const [compraEntradaForm, setCompraEntradaForm] = useState(emptyCompraEntrada);
  const [empresaGrupoForm, setEmpresaGrupoForm] = useState(emptyEmpresaGrupo);
  const [financeiroLancamentoForm, setFinanceiroLancamentoForm] = useState(emptyFinanceiroLancamento);
  const [financeiroFiltro, setFinanceiroFiltro] = useState('todos');
  const [formStatus, setFormStatus] = useState('');
  const [isFullscreen, setIsFullscreen] = useState(false);
  const isErpWorkspace = activeTab === 'erp' || activeTab.startsWith('erp-');
  const sitePreviewLogo = resolveSiteLogoPreview(siteConfigForm.site_logo);
  const previewSlides = parseJsonPreview(siteConfigForm.home_hero_slides, [{
    id: 'preview',
    badge: siteConfigForm.home_intro_badge || 'Preview do banner',
    title: siteConfigForm.home_intro_titulo || 'Título institucional',
    highlight: 'Visual ao vivo',
    description: siteConfigForm.home_intro_texto_1 || 'A prévia mostra como o banner ficará na home antes de salvar no banco.',
    image: siteConfigForm.site_logo || '/imagens/homepage/Logo-2.png',
  }]);
  const previewSlide = previewSlides[0] || {
    badge: siteConfigForm.home_intro_badge || 'Preview do banner',
    title: siteConfigForm.home_intro_titulo || 'Título institucional',
    highlight: 'Visual ao vivo',
    description: siteConfigForm.home_intro_texto_1 || 'A prévia mostra como o banner ficará na home antes de salvar no banco.',
    image: siteConfigForm.site_logo || '/imagens/homepage/Logo-2.png',
  };
  const previewQualityItems = parseJsonPreview(siteConfigForm.home_quality_items, []);
  const previewPartnerCards = parseJsonPreview(siteConfigForm.home_partner_cards, []);

  const handleSiteConfigImageSelect = (fieldKey, file) => {
    const imagePath = toHomepageImagePath(file?.name);
    if (!imagePath) return;

    setSiteConfigForm((current) => ({
      ...current,
      [fieldKey]: applyImagePathToSiteConfig(fieldKey, current[fieldKey], imagePath),
    }));
    setFormStatus(`Caminho aplicado: ${imagePath}. Mantenha o arquivo publicado em public/imagens/homepage.`);
  };

  useEffect(() => {
    const syncFullscreen = () => setIsFullscreen(Boolean(document.fullscreenElement));
    syncFullscreen();
    document.addEventListener('fullscreenchange', syncFullscreen);
    return () => document.removeEventListener('fullscreenchange', syncFullscreen);
  }, []);

  const toggleFullscreen = async () => {
    try {
      if (document.fullscreenElement) {
        await document.exitFullscreen();
      } else {
        await document.documentElement.requestFullscreen();
      }
    } catch {
      setFormStatus('Nao foi possivel alternar para tela cheia neste navegador.');
    }
  };

  const loadData = useCallback(async () => {
    try {
      const [resumoRes, pedidosRes, leadsRes, produtosRes, categoriasRes, clientesRes, fornecedoresRes, empresasGrupoRes, fiscalPedidosRes, comprasPainelRes, integracoesRes, credenciaisRes, financeiroLancamentosRes, siteConfigRes] = await Promise.all([
        dashboardAPI.getResumo(),
        pedidoAPI.getAll(),
        leadAPI.getAll(),
        produtoAPI.getAll(),
        categoriaAPI.getAll(),
        clienteAPI.getAll(),
        fornecedorAPI.getAll(),
        empresaGrupoAPI.getAll().catch(() => ({ data: [] })),
        fiscalAPI.getPedidos().catch(() => ({ data: [] })),
        comprasAPI.getPainel().catch(() => ({ data: null })),
        integracoesAPI.getDiagnostico()
          .catch(() => integracoesAPI.getStatus())
          .catch(() => ({ data: [] })),
        integracoesAPI.getCredenciaisModelo().catch(() => ({ data: [] })),
        financeiroAPI.getLancamentos().catch(() => ({ data: [] })),
        siteAPI.getAll().catch(() => ({ data: [] })),
      ]);
      if (resumoRes.data) setResumo({ ...emptyResumo, ...resumoRes.data });
      const pedidosData = asArray(pedidosRes.data);
      const leadsData = asArray(leadsRes.data);
      if (pedidosData.length > 0) setPedidos(pedidosData);
      if (leadsData.length > 0) setLeads(leadsData);
      const produtosData = asArray(produtosRes.data).filter(isProdutoPublicavel);
      const categoriasData = asArray(categoriasRes.data);
      const clientesData = asArray(clientesRes.data);
      const fornecedoresData = asArray(fornecedoresRes.data);
      const empresasGrupoData = asArray(empresasGrupoRes.data);
      const fiscalPedidosData = asArray(fiscalPedidosRes.data);
      const comprasPainelData = unwrapApiData(comprasPainelRes.data);
      const financeiroLancamentosData = asArray(financeiroLancamentosRes.data);
      if (produtosData.length > 0) setProdutos(produtosData);
      if (categoriasData.length > 0) setCategorias(categoriasData);
      if (clientesData.length > 0) setClientes(clientesData);
      if (fornecedoresData.length > 0) setFornecedores(fornecedoresData);
      if (empresasGrupoData.length > 0) setEmpresasGrupo(empresasGrupoData);
      if (fiscalPedidosData.length > 0) setFiscalPedidos(fiscalPedidosData);
      if (comprasPainelData && typeof comprasPainelData === 'object') setComprasPainel(comprasPainelData);
      setFinanceiroLancamentos(financeiroLancamentosData);
      const integracoesData = asArray(integracoesRes.data);
      const credenciaisData = asArray(credenciaisRes.data);
      const siteConfigData = asArray(siteConfigRes.data);
      if (integracoesData.length > 0) setIntegracoes(integracoesData);
      if (credenciaisData.length > 0) setCredenciaisModelo(credenciaisData);
      if (siteConfigData.length > 0) {
        setSiteConfigItems(siteConfigData);
        setSiteConfigForm((current) =>
          siteConfigData.reduce((acc, item) => ({ ...acc, [item.chave]: item.valor ?? '' }), { ...current }),
        );
      }
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Erro:', error);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      loadData();
    }
  }, [isAuthenticated, loadData]);

  useEffect(() => {
    setActiveTab(routeState.activeTab);
    setActiveCadastroTab(routeState.activeCadastroTab);
    setActiveIntegration(routeState.activeIntegration);
    setFormStatus('');
  }, [routeState.activeCadastroTab, routeState.activeIntegration, routeState.activeTab]);

  const categoriasRaiz = useMemo(
    () => categorias.filter((categoria) => !(categoria.categoria_pai_id || categoria.categoriaPaiId)),
    [categorias],
  );

  const subcategoriasDisponiveis = useMemo(() => {
    if (!produtoForm.categoriaId) return [];
    return categorias.filter((categoria) => (categoria.categoria_pai_id || categoria.categoriaPaiId) === produtoForm.categoriaId);
  }, [categorias, produtoForm.categoriaId]);

  const categoriasHierarquicas = useMemo(() => (
    categorias.map((categoria) => ({
      ...categoria,
      label: (categoria.caminho || categoria.Caminho || categoria.nome || categoria.Nome || '').trim() || 'Sem nome',
    }))
  ), [categorias]);

  const financeiroResumo = useMemo(() => {
    const pendentes = financeiroLancamentos.filter((item) => getFinanceiroStatus(item).toLowerCase() === 'pendente');
    const pagos = financeiroLancamentos.filter((item) => getFinanceiroStatus(item).toLowerCase() === 'pago');
    const receitasAbertas = pendentes.filter((item) => getFinanceiroTipo(item).toLowerCase() === 'receita');
    const despesasAbertas = pendentes.filter((item) => getFinanceiroTipo(item).toLowerCase() === 'despesa');
    const contasAReceber = receitasAbertas.reduce((total, item) => total + getFinanceiroValor(item), 0);
    const contasAPagar = despesasAbertas.reduce((total, item) => total + getFinanceiroValor(item), 0);

    return {
      contasAReceber,
      contasAPagar,
      saldoAberto: contasAReceber - contasAPagar,
      pagos: pagos.reduce((total, item) => total + getFinanceiroValor(item), 0),
      pendentes: pendentes.length,
    };
  }, [financeiroLancamentos]);

  const financeiroLancamentosFiltrados = useMemo(() => {
    if (financeiroFiltro === 'todos') return financeiroLancamentos;

    return financeiroLancamentos.filter((item) => getFinanceiroStatus(item).toLowerCase() === financeiroFiltro);
  }, [financeiroFiltro, financeiroLancamentos]);

  const submitCategoria = async (event) => {
    event.preventDefault();
    setFormStatus('');

    const slugExistente = categorias.some((categoria) => String(categoria.id).toLowerCase() === String(categoriaForm.id).trim().toLowerCase());
    if (categoriaForm.id && slugExistente) {
      setFormStatus('Já existe uma categoria com este slug.');
      return;
    }

    const nomeExistente = categorias.some((categoria) => String(categoria.nome).trim().toLowerCase() === String(categoriaForm.nome).trim().toLowerCase());
    if (nomeExistente) {
      setFormStatus('Já existe uma categoria com este nome.');
      return;
    }

    const payload = {
      ...categoriaForm,
      ordem: categoriaForm.ordem ? Number(categoriaForm.ordem) : 0,
      categoriaPaiId: categoriaForm.categoriaPaiId || null,
    };

    const response = await categoriaAPI.create(payload);
    setCategorias((current) => [...current, response.data]);
    setCategoriaForm(emptyCategoria);
    setFormStatus('Categoria/subcategoria cadastrada e pronta para uso no catálogo.');
  };

  const submitProduto = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const requiredProductFields = [
      produtoForm.nome,
      produtoForm.descricao,
      produtoForm.imagemUrl,
      produtoForm.peso,
      produtoForm.altura,
      produtoForm.largura,
      produtoForm.comprimento,
    ];
    if (requiredProductFields.some((value) => !String(value ?? '').trim())
        || [produtoForm.peso, produtoForm.altura, produtoForm.largura, produtoForm.comprimento]
          .some((value) => Number(value) <= 0)) {
      setFormStatus('Preencha nome, descrição, imagem, peso, altura, largura e comprimento com valores válidos.');
      return;
    }

    const duplicateMessage = getProdutoDuplicateMessage(produtoForm, produtos, produtoEditingId);
    if (duplicateMessage) {
      setFormStatus(duplicateMessage);
      return;
    }

    const payload = {
      ...produtoForm,
      sku: normalizarSkuInterno(produtoForm, empresasGrupo),
      preco: Number(produtoForm.preco),
      precoPromocional: produtoForm.precoPromocional ? Number(produtoForm.precoPromocional) : null,
      custo: produtoForm.custo ? Number(produtoForm.custo) : null,
      peso: produtoForm.peso ? Number(produtoForm.peso) : null,
      altura: produtoForm.altura ? Number(produtoForm.altura) : null,
      largura: produtoForm.largura ? Number(produtoForm.largura) : null,
      comprimento: produtoForm.comprimento ? Number(produtoForm.comprimento) : null,
      estoque: Number(produtoForm.estoque),
      estoqueMinimo: produtoForm.estoqueMinimo ? Number(produtoForm.estoqueMinimo) : null,
      estoqueReservado: produtoForm.estoqueReservado ? Number(produtoForm.estoqueReservado) : null,
      categoriaId: produtoForm.categoriaId,
      subcategoriaId: produtoForm.subcategoriaId || null,
      fornecedorId: produtoForm.fornecedorId ? Number(produtoForm.fornecedorId) : null,
    };

    const response = produtoEditingId
      ? await produtoAPI.update(produtoEditingId, payload)
      : await produtoAPI.create(payload);
    setProdutos((current) => {
      const respostaId = String(response.data.id ?? response.data.slug ?? response.data.Slug ?? produtoEditingId);
      const semDuplicidade = current.filter((produto) => String(produto.id ?? produto.slug ?? produto.Slug ?? '') !== respostaId);
      return isProdutoPublicavel(response.data) ? [response.data, ...semDuplicidade] : semDuplicidade;
    });
    setProdutoForm(emptyProduto);
    setProdutoEditingId('');
    setFormStatus(isProdutoPublicavel(response.data)
      ? (produtoEditingId ? 'Produto atualizado e pronto para o catálogo.' : 'Produto cadastrado e disponível no catálogo.')
      : 'Produto salvo, mas ainda não atende os requisitos para aparecer no catálogo.');
  };

  const uploadProdutoImagem = async (file) => {
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      setFormStatus('Selecione um arquivo de imagem válido.');
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      setFormStatus('Imagem muito grande. Use arquivo de até 2MB.');
      return;
    }

    setUploadingImage(true);
    setFormStatus('');
    try {
      const dataUrl = await fileToDataUrl(file);
      const response = await produtoAPI.uploadImagem({
        fileName: file.name,
        contentType: file.type,
        dataUrl,
      });
      const url = response.data?.url || response.data?.Url;
      if (!url) throw new Error('URL da imagem não retornada.');
      setProdutoForm((form) => ({ ...form, imagemUrl: url }));
      setFormStatus('Imagem enviada e vinculada ao produto.');
    } catch (error) {
      setFormStatus(error.response?.data?.detail || error.message || 'Não foi possível enviar a imagem.');
    } finally {
      setUploadingImage(false);
    }
  };

  const uploadProdutoGaleria = async (files) => {
    if (!files?.length) return;

    const selectedFiles = Array.from(files);
    const invalidFile = selectedFiles.find((file) => !file.type.startsWith('image/'));
    if (invalidFile) {
      setFormStatus(`Arquivo inválido na galeria: ${invalidFile.name}.`);
      return;
    }

    const oversizedFile = selectedFiles.find((file) => file.size > 2 * 1024 * 1024);
    if (oversizedFile) {
      setFormStatus(`Arquivo muito grande na galeria: ${oversizedFile.name}. Use até 2MB por imagem.`);
      return;
    }

    setUploadingImage(true);
    setFormStatus('');

    try {
      const uploadedUrls = [];

      for (const file of selectedFiles) {
        const dataUrl = await fileToDataUrl(file);
        const response = await produtoAPI.uploadImagem({
          fileName: file.name,
          contentType: file.type,
          dataUrl,
        });

        const url = response.data?.url || response.data?.Url;
        if (!url) {
          throw new Error(`A API não retornou URL para ${file.name}.`);
        }

        uploadedUrls.push(url);
      }

      setProdutoForm((form) => {
        const currentGallery = galleryToArray(form.imagensGaleria);
        return { ...form, imagensGaleria: galleryToText([...currentGallery, ...uploadedUrls]) };
      });
      setFormStatus(`${uploadedUrls.length} imagem(ns) adicionada(s) à galeria do produto.`);
    } catch (error) {
      setFormStatus(error.response?.data?.detail || error.message || 'Não foi possível enviar a galeria do produto.');
    } finally {
      setUploadingImage(false);
    }
  };

  const submitCliente = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const duplicateMessage = getClienteDuplicateMessage(clienteForm, clientes, clienteEditingId);
    if (duplicateMessage) {
      setFormStatus(duplicateMessage);
      return;
    }

    const response = clienteEditingId
      ? await clienteAPI.update(clienteEditingId, {
        nome: clienteForm.nome,
        email: clienteForm.email,
        cpfCnpj: clienteForm.cpf || null,
        telefone: clienteForm.telefone || null,
        whatsapp: clienteForm.telefone || null,
        newsletter: true,
        vip: false,
        status: 'Ativo',
      })
      : await clienteAPI.create(clienteForm);
    setClientes((current) => {
      const semDuplicidade = current.filter((cliente) => String(cliente.id) !== String(response.data.id ?? clienteEditingId));
      return [response.data, ...semDuplicidade];
    });
    setClienteForm(emptyCliente);
    setClienteEditingId('');
    setFormStatus(clienteEditingId ? 'Cliente atualizado no painel.' : 'Cliente cadastrado no painel.');
  };

  const submitFornecedor = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const duplicateMessage = getFornecedorDuplicateMessage(fornecedorForm, fornecedores, fornecedorEditingId);
    if (duplicateMessage) {
      setFormStatus(duplicateMessage);
      return;
    }

    const response = fornecedorEditingId
      ? await fornecedorAPI.update(fornecedorEditingId, fornecedorForm)
      : await fornecedorAPI.create(fornecedorForm);
    setFornecedores((current) => {
      const semDuplicidade = current.filter((fornecedor) => String(fornecedor.id) !== String(response.data.id ?? fornecedorEditingId));
      return [response.data, ...semDuplicidade];
    });
    setFornecedorForm(emptyFornecedor);
    setFornecedorEditingId('');
    setFormStatus(fornecedorEditingId ? 'Fornecedor atualizado no painel.' : 'Fornecedor cadastrado no painel.');
  };

  const refreshComprasPainel = async () => {
    const response = await comprasAPI.getPainel();
    setComprasPainel(response.data);
    return response.data;
  };

  const getCompraProdutoSelecionado = (produtoId) =>
    (comprasPainel?.produtosReposicao || []).find((produto) => String(produto.produtoId ?? produto.id ?? '') === String(produtoId ?? ''))
    || produtos.find((produto) => String(produto.produtoId ?? produto.id ?? '') === String(produtoId ?? ''));

  const getCompraProdutoNome = (produto) => produto?.produtoNome || produto?.nome || produto?.Nome || '';

  const getCompraProdutoSku = (produto) => produto?.sku || produto?.Sku || '';

  const submitCompraSolicitacao = async (event) => {
    event.preventDefault();
    setFormStatus('');

    if (Number(compraSolicitacaoForm.quantidade) <= 0) {
      setFormStatus('Informe uma quantidade válida para abrir a solicitação.');
      return;
    }

    const produtoSelecionado = getCompraProdutoSelecionado(compraSolicitacaoForm.produtoId);
    if (!produtoSelecionado && !String(compraSolicitacaoForm.produtoNome || '').trim()) {
      setFormStatus('Selecione um produto ou informe a descrição do item solicitado.');
      return;
    }

    const response = await comprasAPI.registrarSolicitacao({
      produtoId: compraSolicitacaoForm.produtoId ? Number(compraSolicitacaoForm.produtoId) : null,
      produtoNome: getCompraProdutoNome(produtoSelecionado) || compraSolicitacaoForm.produtoNome || null,
      quantidade: Number(compraSolicitacaoForm.quantidade),
      origem: compraSolicitacaoForm.origem,
      finalidade: compraSolicitacaoForm.finalidade,
      prioridade: compraSolicitacaoForm.prioridade,
      observacoes: compraSolicitacaoForm.observacoes || null,
    });

    setComprasPainel(response.data);
    setCompraSolicitacaoForm(emptyCompraSolicitacao);
    setFormStatus('Solicitação de compra aberta e disponível para cotação.');
  };

  const submitCompraCotacao = async (event) => {
    event.preventDefault();
    setFormStatus('');

    if (!compraCotacaoForm.fornecedorId || Number(compraCotacaoForm.quantidade) <= 0 || Number(compraCotacaoForm.custoUnitario) <= 0) {
      setFormStatus('Informe fornecedor, quantidade e custo unitário para registrar a cotação.');
      return;
    }

    const produtoSelecionado = getCompraProdutoSelecionado(compraCotacaoForm.produtoId);
    if (!produtoSelecionado && !String(compraCotacaoForm.produtoNome || '').trim()) {
      setFormStatus('Selecione um produto ou informe a descrição do item cotado.');
      return;
    }

    const response = await comprasAPI.registrarCotacao({
      fornecedorId: Number(compraCotacaoForm.fornecedorId),
      produtoId: compraCotacaoForm.produtoId ? Number(compraCotacaoForm.produtoId) : null,
      produtoNome: getCompraProdutoNome(produtoSelecionado) || compraCotacaoForm.produtoNome || null,
      quantidade: Number(compraCotacaoForm.quantidade),
      custoUnitario: Number(compraCotacaoForm.custoUnitario),
      origem: compraCotacaoForm.origem,
      finalidade: compraCotacaoForm.finalidade,
      prioridade: compraCotacaoForm.prioridade,
      prazoEntregaDias: compraCotacaoForm.prazoEntregaDias ? Number(compraCotacaoForm.prazoEntregaDias) : null,
      observacoes: compraCotacaoForm.observacoes || null,
    });

    setComprasPainel(response.data);
    setCompraCotacaoForm(emptyCompraCotacao);
    setFormStatus('Cotação registrada e refletida no painel de compras.');
  };

  const submitCompraPedido = async (event) => {
    event.preventDefault();
    setFormStatus('');

    if (!compraPedidoForm.fornecedorId || Number(compraPedidoForm.quantidade) <= 0 || Number(compraPedidoForm.custoUnitario) <= 0) {
      setFormStatus('Informe fornecedor, quantidade e custo unitário para gerar o pedido de compra.');
      return;
    }

    const produtoSelecionado = getCompraProdutoSelecionado(compraPedidoForm.produtoId);
    if (!produtoSelecionado && !String(compraPedidoForm.produtoNome || '').trim()) {
      setFormStatus('Selecione um produto ou informe a descrição do item comprado.');
      return;
    }

    const response = await comprasAPI.criarPedido({
      fornecedorId: Number(compraPedidoForm.fornecedorId),
      solicitacaoId: compraPedidoForm.solicitacaoId ? Number(compraPedidoForm.solicitacaoId) : null,
      origem: compraPedidoForm.origem,
      finalidade: compraPedidoForm.finalidade,
      dataPrevistaEntrega: compraPedidoForm.dataPrevistaEntrega || null,
      dataVencimento: compraPedidoForm.dataVencimento || null,
      meioPagamento: compraPedidoForm.meioPagamento || null,
      observacoes: compraPedidoForm.observacoes || null,
      itens: [{
        produtoId: compraPedidoForm.produtoId ? Number(compraPedidoForm.produtoId) : null,
        produtoNome: getCompraProdutoNome(produtoSelecionado) || compraPedidoForm.produtoNome || null,
        sku: getCompraProdutoSku(produtoSelecionado) || compraPedidoForm.sku || null,
        quantidade: Number(compraPedidoForm.quantidade),
        custoUnitario: Number(compraPedidoForm.custoUnitario),
      }],
    });

    setComprasPainel(response.data);
    setCompraPedidoForm(emptyCompraPedido);
    setFormStatus('Pedido de compra gerado com conta a pagar e visão empresarial atualizada.');
  };

  const submitCompraEntrada = async (event) => {
    event.preventDefault();
    setFormStatus('');

    if (!compraEntradaForm.pedidoId) {
      setFormStatus('Selecione um pedido de compra para registrar a entrada.');
      return;
    }

    const itensEntrada = (selectedCompraEntradaPedido?.itens || [])
      .map((item) => ({
        itemId: Number(item.id),
        quantidadeRecebida: Number(compraEntradaForm.itens?.[String(item.id)] ?? item.quantidadePendente ?? 0),
      }))
      .filter((item) => item.itemId > 0 && item.quantidadeRecebida > 0);

    const response = await comprasAPI.registrarEntrada(Number(compraEntradaForm.pedidoId), {
      numeroDocumento: compraEntradaForm.numeroDocumento || null,
      chaveNfeEntrada: compraEntradaForm.chaveNfeEntrada || null,
      tipoEntrada: compraEntradaForm.tipoEntrada,
      recebidoPor: compraEntradaForm.recebidoPor || 'Operacao',
      observacoes: compraEntradaForm.observacoes || null,
      itens: itensEntrada.length > 0 ? itensEntrada : null,
    });

    setComprasPainel(response.data);
    setCompraEntradaForm(emptyCompraEntrada);
    await refreshComprasPainel();
    setFormStatus('Entrada registrada, estoque atualizado e fiscal sinalizado.');
  };

  const updateCompraSolicitacaoStatus = async (solicitacao, status) => {
    try {
      const observacoes = ['Cancelada', 'Fechada'].includes(status)
        ? window.prompt('Observação para registrar no histórico da solicitação:', '') || ''
        : '';
      const response = await comprasAPI.atualizarSolicitacaoStatus(solicitacao.id, { status, observacoes });
      setComprasPainel(response.data);
      await refreshComprasPainel();
      setFormStatus(`Solicitação ${solicitacao.id} atualizada para ${status}.`);
    } catch (error) {
      setFormStatus(error?.response?.data?.mensagem || 'Não foi possível atualizar a solicitação de compra.');
    }
  };

  const updateCompraPedidoStatus = async (pedido, status) => {
    try {
      const observacoes = ['Cancelado', 'Fechado'].includes(status)
        ? window.prompt('Observação para registrar no histórico do pedido de compra:', '') || ''
        : '';
      const response = await comprasAPI.atualizarPedidoStatus(pedido.id, { status, observacoes });
      setComprasPainel(response.data);
      await refreshComprasPainel();
      setFormStatus(`Pedido ${pedido.numero || pedido.id} atualizado para ${status}.`);
    } catch (error) {
      setFormStatus(error?.response?.data?.mensagem || 'Não foi possível atualizar o pedido de compra.');
    }
  };

  const submitFinanceiroLancamento = async (event) => {
    event.preventDefault();
    setFormStatus('');

    const valor = Number(financeiroLancamentoForm.valor || 0);
    if (valor <= 0) {
      setFormStatus('Informe um valor financeiro maior que zero.');
      return;
    }

    const payload = {
      ...financeiroLancamentoForm,
      valor,
      pedidoId: financeiroLancamentoForm.pedidoId ? Number(financeiroLancamentoForm.pedidoId) : null,
      dataVencimento: financeiroLancamentoForm.dataVencimento || null,
      dataPagamento: financeiroLancamentoForm.dataPagamento || null,
      categoria: financeiroLancamentoForm.categoria || null,
      descricao: financeiroLancamentoForm.descricao || null,
      meioPagamento: financeiroLancamentoForm.meioPagamento || null,
      contaBancaria: financeiroLancamentoForm.contaBancaria || null,
      comprovanteUrl: financeiroLancamentoForm.comprovanteUrl || null,
      observacoes: financeiroLancamentoForm.observacoes || null,
    };

    await financeiroAPI.createLancamento(payload);
    setFinanceiroLancamentoForm(emptyFinanceiroLancamento);
    setFormStatus('Lançamento financeiro registrado e carteira atualizada.');
    await loadData();
  };

  const atualizarFinanceiroLancamentoStatus = async (id, status) => {
    await financeiroAPI.updateLancamentoStatus(id, {
      status,
      dataPagamento: status === 'Pago' ? new Date().toISOString() : null,
      meioPagamento: status === 'Pago' ? 'Baixa manual' : null,
      contaBancaria: status === 'Pago' ? 'Conta operacional' : null,
      comprovanteUrl: null,
      observacoes: `Atualização pelo painel financeiro em ${new Date().toLocaleString('pt-BR')}.`,
    });
    setFormStatus(`Lançamento financeiro atualizado para ${status}.`);
    await loadData();
  };

  const submitLead = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const response = await leadAPI.create(leadForm);
    setLeads((current) => [response.data, ...current]);
    setLeadForm(emptyLead);
    setFormStatus('Lead cadastrado no CRM.');
  };

  const submitSiteConfiguracoes = async (event) => {
    event.preventDefault();
    setFormStatus('');

    const payload = siteConfigFieldMeta.map((field) => {
      const existing = siteConfigItems.find((item) => item.chave === field.key);
      const value = siteConfigForm[field.key] ?? '';
      return {
        chave: field.key,
        valor: value,
        tipo: field.type === 'textarea' && String(value).trim().startsWith('[') ? 'JSON' : existing?.tipo || 'Texto',
        descricao: existing?.descricao || field.description,
        grupo: existing?.grupo || field.group,
        editavel: existing?.editavel ?? true,
      };
    });

    await siteAPI.update(payload);

    setSiteConfigItems(payload.map((item, index) => ({
      id: siteConfigItems.find((config) => config.chave === item.chave)?.id || index + 1,
      chave: item.chave,
      valor: item.valor,
      tipo: item.tipo,
      descricao: item.descricao,
      grupo: item.grupo,
      editavel: item.editavel,
      updatedAt: new Date().toISOString(),
    })));
    setFormStatus('Configurações da home, banners e contatos salvas no banco com sucesso.');
  };

  const submitEmpresaGrupo = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const duplicateMessage = getEmpresaGrupoDuplicateMessage(empresaGrupoForm, empresasGrupo);
    if (duplicateMessage) {
      setFormStatus(duplicateMessage);
      return;
    }

    const payload = {
      ...empresaGrupoForm,
      proximaNfceNumero: empresaGrupoForm.proximaNfceNumero ? Number(empresaGrupoForm.proximaNfceNumero) : null,
      proximaNfeNumero: empresaGrupoForm.proximaNfeNumero ? Number(empresaGrupoForm.proximaNfeNumero) : null,
      aliquotaIcmsInterna: empresaGrupoForm.aliquotaIcmsInterna ? Number(empresaGrupoForm.aliquotaIcmsInterna) : null,
      aliquotaIcmsInterestadual: empresaGrupoForm.aliquotaIcmsInterestadual ? Number(empresaGrupoForm.aliquotaIcmsInterestadual) : null,
      aliquotaPis: empresaGrupoForm.aliquotaPis ? Number(empresaGrupoForm.aliquotaPis) : null,
      aliquotaCofins: empresaGrupoForm.aliquotaCofins ? Number(empresaGrupoForm.aliquotaCofins) : null,
      aliquotaIss: empresaGrupoForm.aliquotaIss ? Number(empresaGrupoForm.aliquotaIss) : null,
      aliquotaIpi: empresaGrupoForm.aliquotaIpi ? Number(empresaGrupoForm.aliquotaIpi) : null,
      cargaTributariaPercentual: empresaGrupoForm.cargaTributariaPercentual ? Number(empresaGrupoForm.cargaTributariaPercentual) : null,
      custoOperacionalPercentual: empresaGrupoForm.custoOperacionalPercentual ? Number(empresaGrupoForm.custoOperacionalPercentual) : null,
      margemMinimaPercentual: empresaGrupoForm.margemMinimaPercentual ? Number(empresaGrupoForm.margemMinimaPercentual) : null,
      prioridadeFiscal: empresaGrupoForm.prioridadeFiscal ? Number(empresaGrupoForm.prioridadeFiscal) : 100,
    };

    const response = await empresaGrupoAPI.create(payload);
    setEmpresasGrupo((current) => [response.data, ...current]);
    setEmpresaGrupoForm(emptyEmpresaGrupo);
    setFormStatus('Empresa fiscal cadastrada no ERP.');
  };

  const updateLeadStatus = async (id, status) => {
    const response = await leadAPI.updateStatus(id, status);
    setLeads((current) => current.map((lead) => (lead.id === id ? response.data : lead)));
  };

  const updatePedidoStatus = async (id, status) => {
    const response = await pedidoAPI.updateStatus(id, status);
    setPedidos((current) => current.map((pedido) => (pedido.id === id ? response.data : pedido)));
  };

  const updatePedidoLogistica = async (pedido) => {
    const transportadora = window.prompt('Transportadora ou forma de entrega:', pedido.frete_transportadora || 'Nexum Altivon');
    if (transportadora === null) return;

    const rastreio = window.prompt('Codigo de rastreio, se houver:', pedido.frete_codigo_rastreio || '');
    if (rastreio === null) return;

    const prazo = window.prompt('Prazo em dias:', String(pedido.frete_prazo_dias || 3));
    if (prazo === null) return;

    const response = await pedidoAPI.updateLogistica(pedido.id, {
      frete_metodo: pedido.frete_metodo || 'manual',
      frete_transportadora: transportadora,
      frete_codigo_rastreio: rastreio,
      frete_prazo_dias: Number(prazo) || pedido.frete_prazo_dias || 3,
    });

    setPedidos((current) => current.map((item) => (item.id === pedido.id ? response.data : item)));
  };

  const pedidosPorFila = useMemo(() => {
    if (pedidoFila === 'todos') return pedidos;

    return pedidos.filter((pedido) => {
      const status = String(pedido.status || '').toLowerCase();
      if (pedidoFila === 'sem-rastreio') {
        return status.includes('enviado') && !pedido.frete_codigo_rastreio;
      }
      if (pedidoFila === 'separacao') {
        return status.includes('separ');
      }
      return status.includes(pedidoFila);
    });
  }, [pedidoFila, pedidos]);

  const filteredPedidos = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return pedidosPorFila;
    return pedidosPorFila.filter((pedido) =>
      [pedido.numero_pedido, pedido.status, pedido.total, pedido.frete_codigo_rastreio, pedido.frete_transportadora].some((value) => String(value || '').toLowerCase().includes(term))
    );
  }, [pedidosPorFila, query]);

  const filteredLeads = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return leads;
    return leads.filter((lead) =>
      [lead.nome, lead.email, lead.telefone, lead.status].some((value) => String(value || '').toLowerCase().includes(term))
    );
  }, [leads, query]);

  const produtoDuplicateMessage = useMemo(() => getProdutoDuplicateMessage(produtoForm, produtos, produtoEditingId), [produtoForm, produtos, produtoEditingId]);
  const clienteDuplicateMessage = useMemo(() => getClienteDuplicateMessage(clienteForm, clientes, clienteEditingId), [clienteForm, clientes, clienteEditingId]);
  const fornecedorDuplicateMessage = useMemo(() => getFornecedorDuplicateMessage(fornecedorForm, fornecedores, fornecedorEditingId), [fornecedorForm, fornecedores, fornecedorEditingId]);
  const empresaGrupoDuplicateMessage = useMemo(() => getEmpresaGrupoDuplicateMessage(empresaGrupoForm, empresasGrupo), [empresaGrupoForm, empresasGrupo]);
  const compraFornecedorOptions = useMemo(() => {
    const fornecedoresMap = new Map();
    [...(comprasPainel?.fornecedores || []), ...fornecedores].forEach((fornecedor) => {
      const id = fornecedor?.id ?? fornecedor?.Id;
      const nome = fornecedor?.nome || fornecedor?.Nome || fornecedor?.razaoSocial || fornecedor?.razao_social || fornecedor?.RazaoSocial;
      if (id && nome) fornecedoresMap.set(String(id), nome);
    });
    return Array.from(fornecedoresMap, ([value, label]) => ({ value, label }));
  }, [comprasPainel, fornecedores]);
  const compraProdutoOptions = useMemo(() => {
    const produtosMap = new Map();
    [...(comprasPainel?.produtosReposicao || []), ...produtos].forEach((produto) => {
      const id = produto?.produtoId ?? produto?.id ?? produto?.Id;
      const nome = produto?.produtoNome || produto?.nome || produto?.Nome;
      if (!id || Number.isNaN(Number(id)) || !nome) return;
      const sku = produto?.sku || produto?.Sku || 'sem SKU';
      const estoque = produto?.estoqueAtual ?? produto?.estoque_atual ?? produto?.estoque ?? produto?.EstoqueAtual ?? 0;
      produtosMap.set(String(id), {
        value: String(id),
        label: `${nome} · ${sku} · estoque ${estoque}`,
        produto,
      });
    });
    return Array.from(produtosMap.values());
  }, [comprasPainel, produtos]);
  const compraPedidoEntradaOptions = useMemo(() => (comprasPainel?.pedidos || [])
    .filter((pedido) => !['recebido', 'cancelado'].includes(String(pedido.status || '').toLowerCase()))
    .map((pedido) => ({
      value: String(pedido.id),
      label: `${pedido.numero || `Pedido ${pedido.id}`} · ${pedido.fornecedorNome || 'Fornecedor'} · ${formatPrice(pedido.valorTotal || 0)}`,
      pedido,
    })), [comprasPainel]);
  const selectedCompraEntradaPedido = useMemo(
    () => compraPedidoEntradaOptions.find((item) => item.value === compraEntradaForm.pedidoId)?.pedido || null,
    [compraEntradaForm.pedidoId, compraPedidoEntradaOptions]
  );
  const compraSolicitacaoOptions = useMemo(() => (comprasPainel?.solicitacoes || [])
    .filter((solicitacao) => !['cancelada', 'atendida', 'fechada'].includes(String(solicitacao.status || '').toLowerCase()))
    .map((solicitacao) => ({
      value: String(solicitacao.id),
      label: `${solicitacao.produtoNome || `Solicitação ${solicitacao.id}`} · ${solicitacao.quantidade || 0} un · ${solicitacao.origem || 'Origem'}`,
      solicitacao,
    })), [comprasPainel]);
  const selectedCadastro = cadastroTabs.find((item) => item.id === activeCadastroTab) || cadastroTabs[0];
  const cadastroCounts = {
    produtos: produtos.length,
    clientes: clientes.length,
    fornecedores: fornecedores.length,
  };

  const openCadastro = (id) => {
    navigate(getDashboardPath(`cadastro-${id}`, id));
  };

  const openMainTab = (id) => {
    const cadastroId = id.startsWith('cadastro-') ? id.replace('cadastro-', '') : activeCadastroTab;
    navigate(getDashboardPath(id, cadastroId));
  };

  const openIntegration = (slug) => {
    navigate(`/dashboard/integracoes/${slug}`);
  };

  const startEditProduto = (produto) => {
    setProdutoEditingId(String(produto.id ?? produto.slug ?? produto.Slug ?? '').trim());
    setProdutoForm({
      ...emptyProduto,
      id: produto.id ?? produto.slug ?? produto.Slug ?? '',
      nome: produto.nome ?? produto.Nome ?? '',
      descricaoCurta: produto.descricao_curta ?? produto.descricaoCurta ?? produto.DescricaoCurta ?? '',
      descricao: produto.descricao_longa ?? produto.descricaoLonga ?? produto.descricao ?? produto.DescricaoLonga ?? '',
      preco: String(produto.preco ?? produto.Preco ?? ''),
      precoPromocional: String(produto.preco_promocional ?? produto.precoPromocional ?? produto.PrecoPromocional ?? ''),
      custo: String(produto.custo ?? produto.Custo ?? ''),
      peso: String(produto.peso ?? produto.Peso ?? ''),
      altura: String(produto.altura ?? produto.Altura ?? ''),
      largura: String(produto.largura ?? produto.Largura ?? ''),
      comprimento: String(produto.comprimento ?? produto.Comprimento ?? ''),
      imagemUrl: produto.imagemUrl ?? produto.imagem_url ?? produto.imagem_principal ?? produto.imagemPrincipal ?? produto.ImagemPrincipal ?? '',
      imagensGaleria: produto.imagensGaleria ?? produto.imagens_galeria ?? produto.ImagensGaleria ?? '',
      estoque: String(produto.estoque ?? produto.estoque_atual ?? produto.EstoqueAtual ?? ''),
      estoqueMinimo: String(produto.estoqueMinimo ?? produto.estoque_minimo ?? produto.EstoqueMinimo ?? ''),
      estoqueReservado: String(produto.estoqueReservado ?? produto.estoque_reservado ?? produto.EstoqueReservado ?? ''),
      destaque: Boolean(produto.destaque ?? produto.Destaque),
      ativo: Boolean(produto.ativo ?? produto.Ativo ?? true),
      sku: produto.sku ?? produto.Sku ?? '',
      categoriaId: String(produto.categoria_id ?? produto.categoriaId ?? produto.CategoriaId ?? 'classicos'),
      subcategoriaId: String(produto.subcategoria_id ?? produto.subcategoriaId ?? produto.SubcategoriaId ?? ''),
      tipoProduto: produto.tipo_produto ?? produto.tipoProduto ?? produto.TipoProduto ?? 'Proprio',
      empresaAquisicaoCodigo: produto.empresaAquisicaoCodigo ?? produto.EmpresaAquisicaoCodigo ?? '',
      fornecedorId: String(produto.fornecedor_id ?? produto.fornecedorId ?? produto.FornecedorId ?? ''),
      marca: produto.marca ?? produto.Marca ?? '',
      tags: produto.tags ?? produto.Tags ?? '',
      seoTitulo: produto.seo_titulo ?? produto.seoTitulo ?? produto.SeoTitulo ?? '',
      seoDescricao: produto.seo_descricao ?? produto.seoDescricao ?? produto.SeoDescricao ?? '',
      seoKeywords: produto.seo_keywords ?? produto.seoKeywords ?? produto.SeoKeywords ?? '',
    });
    setFormStatus(`Editando produto ${produto.nome ?? produto.Nome ?? ''}.`);
    setActiveTab('cadastro-produtos');
    setActiveCadastroTab('produtos');
  };

  const startEditCliente = (cliente) => {
    setClienteEditingId(String(cliente.id ?? cliente.Id ?? '').trim());
    setClienteForm({
      nome: cliente.nome ?? cliente.Nome ?? '',
      email: cliente.email ?? cliente.Email ?? '',
      telefone: cliente.telefone ?? cliente.Telefone ?? '',
      cpf: cliente.cpf ?? cliente.cpfCnpj ?? cliente.CpfCnpj ?? '',
    });
    setFormStatus(`Editando cliente ${cliente.nome ?? cliente.Nome ?? ''}.`);
    setActiveTab('cadastro-clientes');
    setActiveCadastroTab('clientes');
  };

  const startEditFornecedor = (fornecedor) => {
    setFornecedorEditingId(String(fornecedor.id ?? fornecedor.Id ?? '').trim());
    setFornecedorForm({
      nome: fornecedor.nome ?? fornecedor.Nome ?? '',
      documento: fornecedor.documento ?? fornecedor.cnpj ?? fornecedor.Cnpj ?? '',
      email: fornecedor.email ?? fornecedor.Email ?? '',
      telefone: fornecedor.telefone ?? fornecedor.Telefone ?? '',
      categoria: fornecedor.categoria ?? fornecedor.segmento ?? fornecedor.Categoria ?? 'Geral',
    });
    setFormStatus(`Editando fornecedor ${fornecedor.nome ?? fornecedor.Nome ?? ''}.`);
    setActiveTab('cadastro-fornecedores');
    setActiveCadastroTab('fornecedores');
  };

  const statusCounts = useMemo(() => {
    return pedidos.reduce((acc, pedido) => {
      acc[pedido.status] = (acc[pedido.status] || 0) + 1;
      return acc;
    }, {});
  }, [pedidos]);

  if (authLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#050505]">
        <div className="h-12 w-12 animate-spin rounded-full border-4 border-slate-200 border-t-slate-950" />
      </div>
    );
  }

  if (!isAuthenticated) return <Navigate to="/login" />;

  return (
    <main className={`nexum-admin-shell min-h-screen bg-[#050505] ${isErpWorkspace ? 'erp-desktop-shell' : ''}`}>
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-80 border-r border-[#2A2A2A] bg-[#0A0A0A] text-white lg:block">
        <div className="flex h-full flex-col p-5">
          <Link to="/" className="flex items-center gap-3 rounded-2xl border border-white/5 bg-white/5 px-4 py-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-[#C9A227] text-sm font-black text-black shadow-[0_12px_35px_rgba(201,162,39,0.25)]">NA</div>
            <div>
              <p className="font-black tracking-[0.18em] text-[#C9A227]">GENESISGEST.NET</p>
              <p className="text-[0.66rem] font-bold uppercase tracking-[0.22em] text-zinc-500">Workstation administrativa local</p>
            </div>
          </Link>

          <div className="mt-5 rounded-2xl border border-white/6 bg-black/30 px-4 py-3 text-xs font-semibold text-zinc-400">
            <div className="flex items-center justify-between">
              <span className="uppercase tracking-[0.18em] text-zinc-500">Console</span>
              <span className="rounded-full bg-emerald-500/10 px-2 py-1 text-[0.62rem] font-black uppercase text-emerald-300">local</span>
            </div>
            <p className="mt-2 text-zinc-300">API 192.168.1.72:5012 · BD 3309 · modo operacional</p>
          </div>

          <nav className="mt-5 space-y-4 overflow-y-auto pb-6 pr-1">
            {navSections.map((section) => {
              const activeItems = tabs.filter((tab) => tab.section === section);
              const futureItems = plannedModules.filter((module) => module.section === section);
              if (activeItems.length === 0 && futureItems.length === 0) return null;

              return (
                <div key={section}>
                  <p className="px-3 pb-2 text-[0.62rem] font-black uppercase tracking-[0.26em] text-zinc-600">{section}</p>
                  <div className="space-y-1">
                    {activeItems.map((tab) => {
                      const Icon = tab.icon;
                      const active = activeTab === tab.id;
                      return (
                        <button
                          key={tab.id}
                          onClick={() => openMainTab(tab.id)}
                          className={`flex w-full items-center gap-3 border-l-4 px-3 py-3 text-left text-[0.88rem] font-bold transition ${
                            active ? 'border-[#C9A227] bg-[#C9A227]/12 text-[#C9A227] shadow-[inset_0_0_0_1px_rgba(201,162,39,0.12)]' : 'border-transparent text-zinc-400 hover:border-[#C9A227]/60 hover:bg-white/5 hover:text-[#E8D5A3]'
                          }`}
                          data-testid={`tab-${tab.id}`}
                        >
                          <Icon size={17} />
                          <span>{tab.label}</span>
                          {tab.badge && <span className="ml-auto rounded-full bg-emerald-600/15 px-2 py-0.5 text-[0.6rem] uppercase text-emerald-300 ring-1 ring-emerald-500/20">{tab.badge}</span>}
                        </button>
                      );
                    })}
                    {futureItems.map((item) => {
                      const Icon = item.icon;
                      return (
                        <button
                          key={item.label}
                          type="button"
                          className="flex w-full cursor-not-allowed items-center gap-3 border-l-4 border-transparent px-3 py-3 text-left text-[0.88rem] font-semibold text-zinc-700"
                          title="Módulo em estruturação"
                        >
                          <Icon size={17} />
                          <span>{item.label}</span>
                          <span className="ml-auto rounded-full border border-zinc-700 px-2 py-0.5 text-[0.6rem] uppercase text-zinc-600">{item.track || 'breve'}</span>
                        </button>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </nav>

          <div className="mt-auto rounded-2xl border border-white/10 bg-white/[0.04] p-4">
            <div className="flex items-center gap-2 text-[#E8D5A3]">
              <Sparkles size={16} />
              <p className="text-xs font-black uppercase tracking-[0.18em]">Operação real</p>
            </div>
            <p className="mt-3 text-[1.8rem] font-black tracking-tight">{formatPrice(resumo.faturamento_mes)}</p>
            <div className="mt-4 h-2 overflow-hidden rounded-full bg-white/10">
              <div className="h-full w-[72%] rounded-full bg-gradient-to-r from-[#B88E1B] to-[#FFD95A]" />
            </div>
            <p className="mt-3 text-xs font-semibold text-zinc-400">Base operacional conectada à API e ao banco real.</p>
          </div>
        </div>
      </aside>

      <section className="lg:pl-80">
        <header className="sticky top-0 z-30 border-b border-[#2A2A2A] bg-[#0A0A0A]/96 text-white backdrop-blur-xl">
          <div className="flex min-h-[84px] flex-col gap-4 px-4 py-4 sm:px-6 xl:flex-row xl:items-center xl:justify-between xl:px-8">
            <div>
              <p className="text-[0.72rem] font-black uppercase tracking-[0.24em] text-zinc-500"><span className="text-[#C9A227]">{tabs.find((tab) => tab.id === activeTab)?.label || 'Dashboard'}</span> / Operação local</p>
              <h1 className="mt-1 text-[2rem] font-black tracking-tight text-white" data-testid="dashboard-title">GenesisGest.Net</h1>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="rounded-2xl border border-[#2A2A2A] bg-[#111111] px-4 py-2 text-right shadow-[inset_0_1px_0_rgba(255,255,255,0.03)]">
                <p className="text-[0.68rem] font-black uppercase tracking-[0.18em] text-zinc-500">Usuário</p>
                <p className="text-sm font-bold text-white">{user?.nome || user?.email || 'Equipe Nexum'}</p>
              </div>
              <a
                href={sophiaHref}
                className="inline-flex h-11 w-11 items-center justify-center rounded-full border border-[#2A2A2A] bg-[#111111] text-[#E8D5A3] hover:border-[#C9A227] hover:text-white"
                title="Chamar Sophia"
              >
                <MessageCircleMore size={18} />
              </a>
              <button
                type="button"
                onClick={toggleFullscreen}
                className="inline-flex h-11 items-center justify-center gap-2 rounded-full border border-[#2A2A2A] bg-[#111111] px-5 text-sm font-black text-zinc-200 hover:border-[#C9A227] hover:text-white"
                title={isFullscreen ? 'Sair da tela cheia' : 'Ativar tela cheia'}
              >
                {isFullscreen ? 'Janela' : 'Tela cheia'}
              </button>
              <div className="relative">
                <Search className="absolute left-3 top-3 text-zinc-500" size={18} />
                <input
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="Buscar pedidos, clientes, produtos ou leads"
                  className="h-11 w-full rounded-full border border-[#2A2A2A] bg-[#050505] pl-10 pr-4 text-sm font-semibold text-white outline-none focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10 sm:w-80"
                />
              </div>
              <button
                onClick={logout}
                className="inline-flex h-11 items-center justify-center gap-2 rounded-full bg-[#C9A227] px-5 text-sm font-black text-black shadow-[0_16px_35px_rgba(201,162,39,0.18)] hover:bg-[#FFD95A]"
              >
                <LogOut size={17} />
                Sair
              </button>
            </div>
          </div>

          <div className="flex gap-2 overflow-x-auto px-4 pb-4 sm:px-6 lg:hidden">
            {tabs.map((tab) => {
              const Icon = tab.icon;
              const active = activeTab === tab.id;
              return (
                <button
                  key={tab.id}
                  onClick={() => openMainTab(tab.id)}
                  className={`inline-flex shrink-0 items-center gap-2 rounded-full px-4 py-2 text-sm font-black ${
                    active ? 'bg-slate-950 text-white' : 'bg-slate-100 text-slate-700'
                  }`}
                >
                  <Icon size={16} />
                  {tab.label}
                </button>
              );
            })}
          </div>
        </header>

        <div className="px-4 py-8 sm:px-6 xl:px-8">
          {loading ? (
            <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
              {[1, 2, 3, 4].map((item) => (
                <div key={item} className="h-40 animate-pulse rounded-2xl bg-white/5" />
              ))}
            </div>
          ) : (
            <>
              {activeTab === 'overview' && (
                <div className="space-y-6">
                  <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
                    <StatCard title="Pedidos hoje" value={resumo.pedidos_hoje} detail="vs. ontem" trend={12} icon={ShoppingBag} tone="slate" onClick={() => openMainTab('pedidos')} />
                    <StatCard title="Clientes" value={resumo.total_clientes} detail="base ativa" trend={8} icon={Users} tone="emerald" onClick={() => openMainTab('cadastro-clientes')} />
                    <StatCard title="Faturamento" value={formatPrice(resumo.faturamento_mes)} detail="no mês" trend={18} icon={TrendingUp} tone="amber" onClick={() => openMainTab('erp-financeiro')} />
                    <StatCard title="Leads novos" value={resumo.leads_novos} detail="em qualificação" trend={-3} icon={UserRound} tone="indigo" onClick={() => openMainTab('crm')} />
                  </div>

                  <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                      <div>
                        <h2 className="text-xl font-black text-slate-950">Arquitetura operacional integrada</h2>
                        <p className="mt-1 text-sm text-slate-500">
                          E-commerce, dropshipping, logística, gateways de pagamento e módulos empresariais conectados pelo canal de API.
                        </p>
                      </div>
                        <span className="rounded-full bg-emerald-50 px-4 py-2 text-sm font-black text-emerald-800">Operação local validada</span>
                      </div>
                    <div className="mt-6 grid gap-3 md:grid-cols-2 xl:grid-cols-5">
                      {[
                        { label: 'E-commerce', status: 'Operante', icon: ShoppingBag },
                        { label: 'Cadastros reais', status: 'Em uso', icon: PackagePlus },
                        { label: 'Dropshipping', status: 'Paralelo', icon: Handshake },
                        { label: 'Logística', status: 'Paralelo', icon: Boxes },
                        { label: 'Gateways/API', status: 'Paralelo', icon: Database },
                      ].map((item) => {
                        const Icon = item.icon;
                        const targetTab = item.label === 'Cadastros reais'
                          ? 'cadastro-produtos'
                          : item.label === 'Dropshipping'
                            ? 'integracoes'
                            : item.label === 'Logística'
                              ? 'erp-logistica'
                              : item.label === 'Gateways/API'
                                ? 'integracoes'
                                : 'overview';
                        return (
                          <button key={item.label} type="button" onClick={() => openMainTab(targetTab)} className="rounded-lg border border-slate-100 bg-slate-50 p-4 text-left transition hover:-translate-y-0.5 hover:border-[#C9A227] hover:shadow-md">
                            <Icon className="text-amber-600" size={22} />
                            <p className="mt-3 text-sm font-black text-slate-950">{item.label}</p>
                            <p className="mt-1 text-xs font-bold uppercase tracking-wide text-slate-500">{item.status}</p>
                          </button>
                        );
                      })}
                    </div>
                  </section>

                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.5fr)_minmax(360px,1fr)]">
                    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                        <div>
                          <h2 className="text-xl font-black text-slate-950">Performance de vendas</h2>
                          <p className="mt-1 text-sm text-slate-500">Receita por dia da semana e tendência operacional.</p>
                        </div>
                        <span className="rounded-full bg-emerald-50 px-4 py-2 text-sm font-black text-emerald-800">Conversão {resumo.conversao}%</span>
                      </div>
                      <div className="mt-8 flex h-72 items-end gap-3">
                        {chart.map((item) => (
                          <div key={item.label} className="flex h-full flex-1 flex-col justify-end gap-3">
                            <div className="relative flex flex-1 items-end rounded-lg bg-slate-100">
                              <div className="w-full rounded-lg bg-slate-950" style={{ height: `${item.value}%` }} />
                            </div>
                            <p className="text-center text-xs font-black text-slate-500">{item.label}</p>
                          </div>
                        ))}
                      </div>
                    </section>

                    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex items-center justify-between">
                        <div>
                          <h2 className="text-xl font-black text-slate-950">Operação agora</h2>
                          <p className="mt-1 text-sm text-slate-500">Sinais que pedem atenção.</p>
                        </div>
                        <Activity className="text-slate-950" size={24} />
                      </div>
                      <div className="mt-6 space-y-3">
                        {[
                          { label: 'Estoque baixo', value: resumo.produtos_estoque_baixo, icon: Boxes, tone: 'text-orange-700 bg-orange-50' },
                          { label: 'Ticket médio', value: formatPrice(resumo.ticket_medio), icon: CreditCard, tone: 'text-emerald-700 bg-emerald-50' },
                          { label: 'Pedidos em processamento', value: statusCounts.Processando || statusCounts.Pendente || 0, icon: PackageCheck, tone: 'text-indigo-700 bg-indigo-50' },
                        ].map((item) => {
                          const Icon = item.icon;
                          return (
                            <div key={item.label} className="flex items-center justify-between rounded-lg border border-slate-100 p-4">
                              <div className="flex items-center gap-3">
                                <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${item.tone}`}>
                                  <Icon size={19} />
                                </div>
                                <p className="font-black text-slate-950">{item.label}</p>
                              </div>
                              <p className="font-black text-slate-950">{item.value}</p>
                            </div>
                          );
                        })}
                      </div>
                    </section>
                  </div>

                  <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
                    <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
                      <div>
                        <h2 className="text-xl font-black text-slate-950">Pedidos recentes</h2>
                        <p className="mt-1 text-sm text-slate-500">Últimas movimentações do e-commerce.</p>
                      </div>
                      <button onClick={() => openMainTab('pedidos')} className="inline-flex items-center gap-1 text-sm font-black text-slate-950">
                        Ver lista
                        <ChevronRight size={16} />
                      </button>
                    </div>
                    <OrdersTable pedidos={pedidos.slice(0, 5)} onStatusChange={updatePedidoStatus} onLogisticaChange={updatePedidoLogistica} />
                  </section>
                </div>
              )}

              {activeTab === 'cadastros' && (
                <section className="space-y-6">
                  <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                      <div>
                        <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Cadastros operacionais</p>
                        <h2 className="mt-2 text-2xl font-black text-slate-950">Menu principal de cadastros</h2>
                        <p className="mt-1 text-sm text-slate-500">Escolha uma área para abrir uma tela própria de trabalho.</p>
                      </div>
                      {formStatus && <span className="rounded-full bg-emerald-50 px-4 py-2 text-sm font-black text-emerald-800">{formStatus}</span>}
                    </div>
                  </div>

                  <div className="grid gap-4 lg:grid-cols-3">
                    {cadastroTabs.map((item) => {
                      const Icon = item.icon;
                      const active = activeCadastroTab === item.id;
                      return (
                        <button
                          key={item.id}
                          type="button"
                          onClick={() => openCadastro(item.id)}
                          className={`rounded-lg border p-5 text-left shadow-sm transition ${
                            active
                              ? 'border-slate-950 bg-slate-950 text-white'
                              : 'border-slate-200 bg-white text-slate-950 hover:border-[#C9A227]'
                          }`}
                        >
                          <div className="flex items-center justify-between gap-4">
                            <div className={`flex h-11 w-11 items-center justify-center rounded-lg ${active ? 'bg-[#C9A227] text-black' : 'bg-slate-100 text-slate-700'}`}>
                              <Icon size={22} />
                            </div>
                            <ChevronRight className={active ? 'text-[#C9A227]' : 'text-slate-400'} size={20} />
                          </div>
                          <h3 className="mt-4 text-lg font-black">{item.label}</h3>
                          <p className={`mt-1 text-sm font-semibold ${active ? 'text-slate-300' : 'text-slate-500'}`}>{item.detail}</p>
                          <p className={`mt-4 text-xs font-black uppercase tracking-[0.14em] ${active ? 'text-[#C9A227]' : 'text-slate-400'}`}>
                            {cadastroCounts[item.id]} registros · {item.action}
                          </p>
                        </button>
                      );
                    })}
                  </div>
                </section>
              )}

              {activeTab.startsWith('cadastro-') && (
                <section className="space-y-6">
                  <CadastroWorkspaceHeader
                    cadastro={selectedCadastro}
                    count={cadastroCounts[activeCadastroTab]}
                    highlights={cadastroHighlights[activeCadastroTab]}
                  />

                  {activeCadastroTab === 'produtos' && (
                    <div className="grid gap-6 xl:grid-cols-[minmax(0,1.35fr)_minmax(340px,0.65fr)]">
                      <form onSubmit={submitProduto} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                        <h3 className="text-lg font-black text-slate-950">{produtoEditingId ? 'Editar produto cadastrado' : 'Cadastro detalhado de produto'}</h3>
                        <p className="mt-1 text-sm font-semibold text-slate-500">Informações principais para catálogo, estoque e vitrine pública.</p>
                        {produtoDuplicateMessage && <DuplicateAlert message={produtoDuplicateMessage} />}
                        <div className="mt-5 grid gap-4 md:grid-cols-2">
                          <Field label="Slug público / ID do produto" value={produtoForm.id} onChange={(value) => setProdutoForm((form) => ({ ...form, id: value }))} />
                          <Field label="Nome" value={produtoForm.nome} onChange={(value) => setProdutoForm((form) => ({ ...form, nome: value }))} required />
                          <label className="block text-sm font-bold text-slate-700">
                            Empresa compradora para o código
                            <select
                              value={produtoForm.empresaAquisicaoCodigo}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, empresaAquisicaoCodigo: event.target.value }))}
                              className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                            >
                              <option value="">Nexum Altivon / padrão</option>
                              {empresasGrupo.map((empresa) => (
                                <option key={empresa.id} value={empresa.id}>
                                  {empresa.codigoEmpresa || empresa.nomeFantasia || empresa.razaoSocial}
                                </option>
                              ))}
                            </select>
                          </label>
                          <SelectField label="Formato de aquisição" value={produtoForm.tipoProduto} onChange={(value) => setProdutoForm((form) => ({ ...form, tipoProduto: value }))} options={['Proprio', 'Dropshipping', 'Marketplace', 'Afiliado']} />
                          <Field label="Sequência / código interno" value={produtoForm.sku} onChange={(value) => setProdutoForm((form) => ({ ...form, sku: value }))} />
                          <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm font-black text-slate-700">
                            Código final: {normalizarSkuInterno(produtoForm, empresasGrupo)}
                          </div>
                          <Field label="Preço" type="number" value={produtoForm.preco} onChange={(value) => setProdutoForm((form) => ({ ...form, preco: value }))} required />
                          <Field label="Preço promocional" type="number" value={produtoForm.precoPromocional} onChange={(value) => setProdutoForm((form) => ({ ...form, precoPromocional: value }))} />
                          <Field label="Custo interno" type="number" value={produtoForm.custo} onChange={(value) => setProdutoForm((form) => ({ ...form, custo: value }))} />
                          <Field label="Estoque inicial" type="number" value={produtoForm.estoque} onChange={(value) => setProdutoForm((form) => ({ ...form, estoque: value }))} required />
                          <Field label="Estoque mínimo" type="number" value={produtoForm.estoqueMinimo} onChange={(value) => setProdutoForm((form) => ({ ...form, estoqueMinimo: value }))} />
                          <Field label="Estoque reservado" type="number" value={produtoForm.estoqueReservado} onChange={(value) => setProdutoForm((form) => ({ ...form, estoqueReservado: value }))} />
                          <label className="block text-sm font-bold text-slate-700">
                            Categoria
                            <select
                              value={produtoForm.categoriaId}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, categoriaId: event.target.value, subcategoriaId: '' }))}
                              className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                            >
                              {categoriasRaiz.map((categoria) => (
                                <option key={categoria.id} value={categoria.id}>{categoria.nome}</option>
                              ))}
                            </select>
                          </label>
                          <label className="block text-sm font-bold text-slate-700">
                            Subcategoria
                            <select
                              value={produtoForm.subcategoriaId}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, subcategoriaId: event.target.value }))}
                              className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                            >
                              <option value="">Usar apenas a categoria principal</option>
                              {subcategoriasDisponiveis.map((categoria) => (
                                <option key={categoria.id} value={categoria.id}>{categoria.nome}</option>
                              ))}
                            </select>
                          </label>
                          <label className="block text-sm font-bold text-slate-700">
                            Fornecedor principal
                            <select
                              value={produtoForm.fornecedorId}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, fornecedorId: event.target.value }))}
                              className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                            >
                              <option value="">Não vinculado</option>
                              {fornecedores.map((fornecedor) => (
                                <option key={fornecedor.id} value={fornecedor.id}>{fornecedor.nome}</option>
                              ))}
                            </select>
                          </label>
                          <Field label="Marca" value={produtoForm.marca} onChange={(value) => setProdutoForm((form) => ({ ...form, marca: value }))} />
                          <Field label="Tags / palavras-chave" value={produtoForm.tags} onChange={(value) => setProdutoForm((form) => ({ ...form, tags: value }))} />
                          <Field label="Peso (kg)" type="number" value={produtoForm.peso} onChange={(value) => setProdutoForm((form) => ({ ...form, peso: value }))} />
                          <Field label="Altura (cm)" type="number" value={produtoForm.altura} onChange={(value) => setProdutoForm((form) => ({ ...form, altura: value }))} />
                          <Field label="Largura (cm)" type="number" value={produtoForm.largura} onChange={(value) => setProdutoForm((form) => ({ ...form, largura: value }))} />
                          <Field label="Comprimento (cm)" type="number" value={produtoForm.comprimento} onChange={(value) => setProdutoForm((form) => ({ ...form, comprimento: value }))} />
                          <Field label="Descrição curta" value={produtoForm.descricaoCurta} onChange={(value) => setProdutoForm((form) => ({ ...form, descricaoCurta: value }))} />
                          <label className="block text-sm font-bold text-slate-700 md:col-span-2">
                            Descrição comercial
                            <textarea
                              value={produtoForm.descricao}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, descricao: event.target.value }))}
                              className="mt-2 min-h-28 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                            />
                          </label>
                          <ImageUrlField
                            value={produtoForm.imagemUrl}
                            onChange={(value) => setProdutoForm((form) => ({ ...form, imagemUrl: value }))}
                            onUpload={uploadProdutoImagem}
                            uploading={uploadingImage}
                            className="md:col-span-2"
                          />
                          <ImageGalleryField
                            value={produtoForm.imagensGaleria}
                            onChange={(value) => setProdutoForm((form) => ({ ...form, imagensGaleria: value }))}
                            onUpload={uploadProdutoGaleria}
                            uploading={uploadingImage}
                            className="md:col-span-2"
                          />
                          <Field label="SEO título" value={produtoForm.seoTitulo} onChange={(value) => setProdutoForm((form) => ({ ...form, seoTitulo: value }))} />
                          <Field label="SEO palavras-chave" value={produtoForm.seoKeywords} onChange={(value) => setProdutoForm((form) => ({ ...form, seoKeywords: value }))} />
                          <label className="block text-sm font-bold text-slate-700 md:col-span-2">
                            SEO descrição
                            <textarea
                              value={produtoForm.seoDescricao}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, seoDescricao: event.target.value }))}
                              className="mt-2 min-h-24 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                            />
                          </label>
                          <label className="flex items-center gap-3 rounded-lg border border-slate-200 px-3 py-3 text-sm font-bold text-slate-700">
                            <input
                              type="checkbox"
                              checked={produtoForm.destaque}
                              onChange={(event) => setProdutoForm((form) => ({ ...form, destaque: event.target.checked }))}
                              className="h-5 w-5 accent-slate-950"
                            />
                            Produto em destaque na vitrine
                          </label>
                        </div>
                        <div className="mt-5 flex flex-wrap gap-3">
                          <button disabled={Boolean(produtoDuplicateMessage)} className="inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:cursor-not-allowed disabled:bg-slate-300">
                            <Save size={17} />
                            {produtoEditingId ? 'Atualizar produto' : 'Salvar produto'}
                          </button>
                          {produtoEditingId && (
                            <button
                              type="button"
                              onClick={() => {
                                setProdutoForm(emptyProduto);
                                setProdutoEditingId('');
                                setFormStatus('Edição de produto cancelada.');
                              }}
                              className="inline-flex h-11 items-center gap-2 rounded-lg border border-slate-200 bg-white px-5 text-sm font-black text-slate-700"
                            >
                              Cancelar edição
                            </button>
                          )}
                        </div>
                      </form>
                      <div className="space-y-6">
                        <ProductPreview produto={produtoForm} categorias={categorias} />
                        <form onSubmit={submitCategoria} className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
                          <h4 className="text-base font-black text-slate-950">Categorias e subcategorias</h4>
                          <p className="mt-1 text-sm font-semibold text-slate-500">Monte a hierarquia do catálogo antes de liberar o cadastro em massa.</p>
                          <div className="mt-4 grid gap-3">
                            <Field label="Slug / código" value={categoriaForm.id} onChange={(value) => setCategoriaForm((form) => ({ ...form, id: value }))} />
                            <Field label="Nome da categoria" value={categoriaForm.nome} onChange={(value) => setCategoriaForm((form) => ({ ...form, nome: value }))} required />
                            <Field label="Descrição" value={categoriaForm.descricao} onChange={(value) => setCategoriaForm((form) => ({ ...form, descricao: value }))} />
                            <label className="block text-sm font-bold text-slate-700">
                              Categoria pai
                              <select
                                value={categoriaForm.categoriaPaiId}
                                onChange={(event) => setCategoriaForm((form) => ({ ...form, categoriaPaiId: event.target.value }))}
                                className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                              >
                                <option value="">Categoria principal</option>
                                {categoriasRaiz.map((categoria) => (
                                  <option key={categoria.id} value={categoria.id}>{categoria.nome}</option>
                                ))}
                              </select>
                            </label>
                            <Field label="Ordem" type="number" value={categoriaForm.ordem} onChange={(value) => setCategoriaForm((form) => ({ ...form, ordem: value }))} />
                          </div>
                          <button className="mt-4 inline-flex h-11 items-center gap-2 rounded-lg bg-[#C9A227] px-5 text-sm font-black text-slate-950">
                            <Save size={17} />
                            Salvar categoria
                          </button>
                        </form>
                        <CompactList title="Hierarquia do catálogo" items={categoriasHierarquicas} fields={['label', 'descricao']} />
                        <CompactList title="Produtos cadastrados" items={produtos} fields={['nome', 'sku', 'estoque']} onEdit={startEditProduto} />
                      </div>
                    </div>
                  )}

                  {activeCadastroTab === 'clientes' && (
                    <div className="grid gap-6 xl:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
                      <SimpleForm title={clienteEditingId ? 'Editar cliente cadastrado' : 'Cadastro detalhado de cliente'} subtitle="Evita duplicidade por e-mail/CPF e reaproveita registros existentes." onSubmit={submitCliente} buttonLabel={clienteEditingId ? 'Atualizar cliente' : 'Salvar cliente'} alertMessage={clienteDuplicateMessage} disabled={Boolean(clienteDuplicateMessage)}>
                        <Field label="Nome completo / Razão social" value={clienteForm.nome} onChange={(value) => setClienteForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="Email principal" type="email" value={clienteForm.email} onChange={(value) => setClienteForm((form) => ({ ...form, email: value }))} required />
                        <Field label="Telefone / WhatsApp" value={clienteForm.telefone} onChange={(value) => setClienteForm((form) => ({ ...form, telefone: value }))} />
                        <Field label="CPF/CNPJ" value={clienteForm.cpf} onChange={(value) => setClienteForm((form) => ({ ...form, cpf: value }))} />
                      </SimpleForm>
                      <div className="space-y-6">
                        <CadastroInsightPanel title="Controle de clientes" checks={cadastroHighlights.clientes} />
                        <CompactList title="Clientes cadastrados" items={clientes} fields={['nome', 'email', 'telefone', 'cpf']} onEdit={startEditCliente} />
                      </div>
                    </div>
                  )}

                  {activeCadastroTab === 'fornecedores' && (
                    <div className="grid gap-6 xl:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
                      <SimpleForm title={fornecedorEditingId ? 'Editar fornecedor cadastrado' : 'Cadastro detalhado de fornecedor'} subtitle="Base para compras, dropshipping, logística e integrações futuras." onSubmit={submitFornecedor} buttonLabel={fornecedorEditingId ? 'Atualizar fornecedor' : 'Salvar fornecedor'} alertMessage={fornecedorDuplicateMessage} disabled={Boolean(fornecedorDuplicateMessage)}>
                        <Field label="Nome / Razão social" value={fornecedorForm.nome} onChange={(value) => setFornecedorForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="Documento CNPJ/CPF" value={fornecedorForm.documento} onChange={(value) => setFornecedorForm((form) => ({ ...form, documento: value }))} />
                        <Field label="Email comercial" type="email" value={fornecedorForm.email} onChange={(value) => setFornecedorForm((form) => ({ ...form, email: value }))} />
                        <Field label="Telefone / WhatsApp" value={fornecedorForm.telefone} onChange={(value) => setFornecedorForm((form) => ({ ...form, telefone: value }))} />
                        <Field label="Categoria / Segmento" value={fornecedorForm.categoria} onChange={(value) => setFornecedorForm((form) => ({ ...form, categoria: value }))} />
                      </SimpleForm>
                      <div className="space-y-6">
                        <CadastroInsightPanel title="Controle de fornecedores" checks={cadastroHighlights.fornecedores} />
                        <CompactList title="Fornecedores cadastrados" items={fornecedores} fields={['nome', 'documento', 'email', 'categoria']} onEdit={startEditFornecedor} />
                      </div>
                    </div>
                  )}
                </section>
              )}
              {activeTab === 'pedidos' && (
                <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
                  <div className="border-b border-slate-200 px-6 py-5">
                    <h2 className="text-xl font-black text-slate-950">Gestão de pedidos</h2>
                    <p className="mt-1 text-sm text-slate-500">{filteredPedidos.length} pedidos encontrados.</p>
                    <div className="mt-4 flex flex-wrap gap-2">
                      {pedidoFilaOptions.map((option) => (
                        <button
                          key={option.id}
                          type="button"
                          onClick={() => setPedidoFila(option.id)}
                          className={`rounded-full border px-4 py-2 text-xs font-black uppercase tracking-[0.12em] transition ${
                            pedidoFila === option.id
                              ? 'border-slate-950 bg-slate-950 text-white'
                              : 'border-slate-200 bg-slate-50 text-slate-600 hover:border-[#C9A227] hover:text-[#8E6A12]'
                          }`}
                        >
                          {option.label}
                        </button>
                      ))}
                    </div>
                  </div>
                  <OrdersTable pedidos={filteredPedidos} onStatusChange={updatePedidoStatus} onLogisticaChange={updatePedidoLogistica} />
                </section>
              )}

              {activeTab === 'crm' && (
                <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
                  <div className="border-b border-slate-200 px-6 py-5">
                    <h2 className="text-xl font-black text-slate-950">Pipeline CRM</h2>
                    <p className="mt-1 text-sm text-slate-500">{filteredLeads.length} oportunidades em acompanhamento.</p>
                  </div>
                  <div className="grid gap-6 p-6 xl:grid-cols-[360px_minmax(0,1fr)]">
                    <SimpleForm title="Novo lead CRM" onSubmit={submitLead} buttonLabel="Salvar lead">
                      <Field label="Nome" value={leadForm.nome} onChange={(value) => setLeadForm((form) => ({ ...form, nome: value }))} required />
                      <Field label="Email" type="email" value={leadForm.email} onChange={(value) => setLeadForm((form) => ({ ...form, email: value }))} required />
                      <Field label="Telefone" value={leadForm.telefone} onChange={(value) => setLeadForm((form) => ({ ...form, telefone: value }))} />
                      <label className="block text-sm font-bold text-slate-700">
                        Status
                        <select
                          value={leadForm.status}
                          onChange={(event) => setLeadForm((form) => ({ ...form, status: event.target.value }))}
                          className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                        >
                          {leadStatusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
                        </select>
                      </label>
                      <Field label="Origem" value={leadForm.origem} onChange={(value) => setLeadForm((form) => ({ ...form, origem: value }))} />
                    </SimpleForm>
                    <LeadsTable leads={filteredLeads} onStatusChange={updateLeadStatus} />
                  </div>
                </section>
              )}

              {activeTab === 'erp' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <div className="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
                    <ErpCockpitMenu activeTab={activeTab} onNavigate={openMainTab} />
                    <div className="space-y-6">
                      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                          <div>
                            <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Gestão absoluta</p>
                            <h2 className="mt-2 text-2xl font-black text-slate-950">GenesisGest.Net</h2>
                            <p className="mt-1 max-w-3xl text-sm text-slate-500">
                              Central empresarial para financeiro, fiscal, logística, estoque, empresas do grupo e parceiros estratégicos.
                            </p>
                          </div>
                          <span className="rounded-full bg-slate-950 px-4 py-2 text-sm font-black text-[#C9A227]">Painel + Desktop</span>
                        </div>
                      </div>

                      <div className="rounded-lg border border-[#C9A227]/30 bg-[#C9A227]/10 p-6 shadow-sm">
                        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                          <div>
                            <p className="text-xs font-black uppercase tracking-[0.2em] text-[#8B6B12]">Acesso direto</p>
                            <h3 className="mt-2 text-xl font-black text-slate-950">ERP Desktop para operação local</h3>
                            <p className="mt-1 max-w-3xl text-sm text-slate-600">
                              Baixe ou execute o atalho do Windows para abrir o painel desktop do GenesisGest.Net com o portal e a API já apontados para o ambiente local.
                            </p>
                          </div>
                          <a
                            href={erpDesktopLauncher}
                            download="ABRIR-ERP-DESKTOP.cmd"
                            className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-black text-white transition hover:bg-slate-800"
                          >
                            Abrir ERP Desktop
                          </a>
                        </div>
                        <p className="mt-4 text-xs font-semibold text-slate-500">
                          Se o navegador baixar o arquivo, execute <span className="font-black text-slate-700">ABRIR-ERP-DESKTOP.cmd</span> no Windows.
                        </p>
                      </div>

                      <div className="grid gap-4 lg:grid-cols-2 xl:grid-cols-3">
                        {erpModules.map((module) => {
                          const Icon = module.icon;
                          return (
                            <button
                              type="button"
                              key={module.title}
                              onClick={() => openMainTab(module.tabId)}
                              className="rounded-lg border border-slate-200 bg-white p-6 text-left shadow-sm transition hover:border-[#C9A227] hover:shadow-md"
                            >
                              <div className="flex items-start justify-between gap-4">
                                <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-slate-950 text-[#C9A227]">
                                  <Icon size={23} />
                                </div>
                                <span className="rounded-full bg-amber-50 px-3 py-1 text-xs font-black uppercase tracking-wide text-amber-900">
                                  {module.signal}
                                </span>
                              </div>
                              <h3 className="mt-5 text-xl font-black text-slate-950">{module.title}</h3>
                              <p className="mt-2 text-sm font-bold text-slate-500">{module.status}</p>
                              <div className="mt-5 grid gap-2">
                                {module.metrics.map((metric) => (
                                  <div key={metric} className="flex items-center gap-3 rounded-lg border border-slate-100 bg-slate-50 px-3 py-2">
                                    <span className="h-2 w-2 rounded-full bg-[#C9A227]" />
                                    <span className="text-sm font-bold text-slate-700">{metric}</span>
                                  </div>
                                ))}
                              </div>
                              <div className="mt-5 inline-flex items-center gap-2 text-sm font-black text-slate-950">
                                Abrir módulo
                                <ChevronRight size={16} />
                              </div>
                            </button>
                          );
                        })}
                      </div>

                      <section className="rounded-lg border border-slate-200 bg-slate-950 p-6 text-white shadow-sm">
                        <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px] lg:items-center">
                          <div>
                            <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Roteamento fiscal inteligente</p>
                            <h3 className="mt-2 text-2xl font-black">Motor de decisão por empresa emitente</h3>
                            <p className="mt-3 max-w-3xl text-sm font-semibold leading-6 text-slate-300">
                              A análise compara empresa do grupo, regime tributário, CFOP, origem/destino, custo fiscal estimado e margem operacional para recomendar a emissão com menor ônus e maior lucratividade.
                            </p>
                          </div>
                          <div className="rounded-lg border border-white/10 bg-white/5 p-5">
                            <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-400">Prontidão</p>
                            <p className="mt-2 text-3xl font-black text-[#C9A227]">Staging</p>
                            <p className="mt-2 text-sm font-bold text-slate-300">Estrutura pronta para credenciais, homologação fiscal e regras finais do contador.</p>
                          </div>
                        </div>
                      </section>
                    </div>
                  </div>
                </section>
              )}

              {activeTab === 'erp-empresas' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <div className="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
                    <ErpCockpitMenu activeTab={activeTab} onNavigate={openMainTab} />
                    <div className="space-y-6">
                      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                        <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Cadastro fiscal mestre</p>
                        <h2 className="mt-2 text-2xl font-black text-slate-950">Empresas do grupo e parceiras</h2>
                        <p className="mt-1 max-w-3xl text-sm text-slate-500">
                          Cadastre as empresas com o máximo de detalhes fiscais, tributários e operacionais para suportar a decisão automática de emissão.
                        </p>
                      </div>

                      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_380px]">
                        <SimpleForm
                          title="Nova empresa fiscal"
                          subtitle="Este cadastro alimenta o motor tributário, relatórios e a futura automação de NF-e."
                          onSubmit={submitEmpresaGrupo}
                          buttonLabel="Salvar empresa fiscal"
                          alertMessage={formStatus || empresaGrupoDuplicateMessage}
                        >
                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Base societária</p>
                        <div className="mt-4 grid gap-4 md:grid-cols-2">
                          <SelectField label="Tipo de cadastro" value={empresaGrupoForm.tipoCadastro} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, tipoCadastro: value }))} options={['GrupoSocietario', 'ParceiraEstrategica']} />
                          <SelectField label="Matriz / filial" value={empresaGrupoForm.matrizFilial} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, matrizFilial: value }))} options={['Matriz', 'Filial', 'Unidade']} />
                          <Field label="Razão social" value={empresaGrupoForm.razaoSocial} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, razaoSocial: value }))} required />
                          <Field label="Nome fantasia" value={empresaGrupoForm.nomeFantasia} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, nomeFantasia: value }))} />
                          <Field label="CNPJ" value={empresaGrupoForm.cnpj} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cnpj: value }))} required />
                          <Field label="Código interno" value={empresaGrupoForm.codigoEmpresa} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, codigoEmpresa: value }))} />
                        </div>
                      </div>

                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Fiscal e contábil</p>
                        <div className="mt-4 grid gap-4 md:grid-cols-2">
                          <Field label="Inscrição estadual" value={empresaGrupoForm.inscricaoEstadual} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, inscricaoEstadual: value }))} />
                          <Field label="Inscrição municipal" value={empresaGrupoForm.inscricaoMunicipal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, inscricaoMunicipal: value }))} />
                          <SelectField label="Regime tributário" value={empresaGrupoForm.regimeTributario} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, regimeTributario: value }))} options={['Simples Nacional', 'Lucro Presumido', 'Lucro Real']} />
                          <Field label="CRT" value={empresaGrupoForm.crt} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, crt: value }))} />
                          <Field label="CNAE principal" value={empresaGrupoForm.cnaePrincipal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cnaePrincipal: value }))} />
                          <Field label="NCM padrão" value={empresaGrupoForm.ncmPadrao} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, ncmPadrao: value }))} />
                          <Field label="Categoria fiscal" value={empresaGrupoForm.categoriaFiscal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, categoriaFiscal: value }))} />
                          <Field label="Subcategoria fiscal" value={empresaGrupoForm.subcategoriaFiscal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, subcategoriaFiscal: value }))} />
                          <Field label="CFOP interno" value={empresaGrupoForm.cfopPadraoInterno} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cfopPadraoInterno: value }))} />
                          <Field label="CFOP interestadual" value={empresaGrupoForm.cfopPadraoInterestadual} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cfopPadraoInterestadual: value }))} />
                        </div>
                        <TextAreaField label="CNAEs secundários" value={empresaGrupoForm.cnaesSecundarios} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cnaesSecundarios: value }))} />
                        <TextAreaField label="Natureza padrão de operação" value={empresaGrupoForm.naturezaOperacaoPadrao} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, naturezaOperacaoPadrao: value }))} rows={3} />
                      </div>

                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Contato e emissão</p>
                        <div className="mt-4 grid gap-4 md:grid-cols-2">
                          <Field label="Responsável legal" value={empresaGrupoForm.responsavelLegal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, responsavelLegal: value }))} />
                          <Field label="Responsável fiscal" value={empresaGrupoForm.responsavelFiscal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, responsavelFiscal: value }))} />
                          <Field label="E-mail fiscal" type="email" value={empresaGrupoForm.emailFiscal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, emailFiscal: value }))} />
                          <Field label="E-mail comercial" type="email" value={empresaGrupoForm.emailComercial} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, emailComercial: value }))} />
                          <Field label="Telefone" value={empresaGrupoForm.telefone} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, telefone: value }))} />
                          <Field label="WhatsApp" value={empresaGrupoForm.whatsapp} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, whatsapp: value }))} />
                          <SelectField label="Ambiente NF-e" value={empresaGrupoForm.ambienteNfe} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, ambienteNfe: value }))} options={['Homologacao', 'Producao']} />
                          <Field label="Série NF-e" value={empresaGrupoForm.serieNfe} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, serieNfe: value }))} />
                        </div>
                      </div>

                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Endereço fiscal e operacional</p>
                        <div className="mt-4 grid gap-4 md:grid-cols-2">
                          <Field label="CEP" value={empresaGrupoForm.cep} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cep: value }))} />
                          <Field label="Logradouro" value={empresaGrupoForm.logradouro} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, logradouro: value }))} />
                          <Field label="Número" value={empresaGrupoForm.numero} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, numero: value }))} />
                          <Field label="Complemento" value={empresaGrupoForm.complemento} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, complemento: value }))} />
                          <Field label="Bairro" value={empresaGrupoForm.bairro} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, bairro: value }))} />
                          <Field label="Cidade" value={empresaGrupoForm.cidade} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cidade: value }))} />
                          <Field label="UF" value={empresaGrupoForm.estado} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, estado: value }))} />
                          <Field label="País" value={empresaGrupoForm.pais} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, pais: value }))} />
                        </div>
                      </div>

                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">PDV e cupom fiscal</p>
                        <div className="mt-4 grid gap-4 md:grid-cols-2">
                          <SelectField label="Documento fiscal do PDV" value={empresaGrupoForm.modeloDocumentoPdv} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, modeloDocumentoPdv: value }))} options={['NFCe', 'SAT', 'MFe', 'ECF']} />
                          <SelectField label="Ambiente NFC-e" value={empresaGrupoForm.ambienteNfce} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, ambienteNfce: value }))} options={['Homologacao', 'Producao']} />
                          <Field label="Série NFC-e" value={empresaGrupoForm.serieNfce} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, serieNfce: value }))} />
                          <Field label="Próximo número NFC-e" type="number" value={empresaGrupoForm.proximaNfceNumero} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, proximaNfceNumero: value }))} />
                          <Field label="CSC NFC-e" value={empresaGrupoForm.nfceCsc} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, nfceCsc: value }))} />
                          <Field label="ID Token CSC" value={empresaGrupoForm.nfceCscIdToken} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, nfceCscIdToken: value }))} />
                          <Field label="Série SAT / MFe" value={empresaGrupoForm.pdvSerieSat} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, pdvSerieSat: value }))} />
                          <Field label="Impressora fiscal" value={empresaGrupoForm.pdvImpressoraFiscal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, pdvImpressoraFiscal: value }))} />
                          <Field label="Nome padrão do caixa" value={empresaGrupoForm.pdvNomeCaixaPadrao} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, pdvNomeCaixaPadrao: value }))} />
                        </div>
                        <div className="mt-4 grid gap-3 md:grid-cols-2">
                          <ToggleField label="Contingência offline no PDV" checked={empresaGrupoForm.pdvContingenciaOffline} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, pdvContingenciaOffline: checked }))} />
                          <ToggleField label="Habilitar emissão fiscal no caixa" checked={empresaGrupoForm.permiteNfeSaida} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, permiteNfeSaida: checked }))} />
                        </div>
                      </div>

                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                        <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Custo e decisão automática</p>
                        <div className="mt-4 grid gap-4 md:grid-cols-2">
                          <Field label="Próxima NF-e" type="number" value={empresaGrupoForm.proximaNfeNumero} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, proximaNfeNumero: value }))} />
                          <Field label="Prioridade fiscal" type="number" value={empresaGrupoForm.prioridadeFiscal} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, prioridadeFiscal: value }))} />
                          <SelectField label="Perfil de tributação" value={empresaGrupoForm.perfilTributacao} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, perfilTributacao: value }))} options={['TributacaoAtual', 'LegadoST', 'Hibrido']} />
                          <Field label="ICMS interna (%)" type="number" value={empresaGrupoForm.aliquotaIcmsInterna} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, aliquotaIcmsInterna: value }))} />
                          <Field label="ICMS interestadual (%)" type="number" value={empresaGrupoForm.aliquotaIcmsInterestadual} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, aliquotaIcmsInterestadual: value }))} />
                          <Field label="PIS (%)" type="number" value={empresaGrupoForm.aliquotaPis} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, aliquotaPis: value }))} />
                          <Field label="COFINS (%)" type="number" value={empresaGrupoForm.aliquotaCofins} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, aliquotaCofins: value }))} />
                          <Field label="ISS (%)" type="number" value={empresaGrupoForm.aliquotaIss} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, aliquotaIss: value }))} />
                          <Field label="IPI (%)" type="number" value={empresaGrupoForm.aliquotaIpi} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, aliquotaIpi: value }))} />
                          <Field label="Carga tributária (%)" type="number" value={empresaGrupoForm.cargaTributariaPercentual} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, cargaTributariaPercentual: value }))} />
                          <Field label="Custo operacional (%)" type="number" value={empresaGrupoForm.custoOperacionalPercentual} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, custoOperacionalPercentual: value }))} />
                          <Field label="Margem mínima (%)" type="number" value={empresaGrupoForm.margemMinimaPercentual} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, margemMinimaPercentual: value }))} />
                        </div>
                        <div className="mt-4 grid gap-3 md:grid-cols-2">
                          <ToggleField label="NF-e entrada" checked={empresaGrupoForm.permiteNfeEntrada} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, permiteNfeEntrada: checked }))} />
                          <ToggleField label="NF-e saída" checked={empresaGrupoForm.permiteNfeSaida} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, permiteNfeSaida: checked }))} />
                          <ToggleField label="Usa ST legado" checked={empresaGrupoForm.usaStLegado} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, usaStLegado: checked }))} />
                          <ToggleField label="Destaca ICMS-ST separado" checked={empresaGrupoForm.destacaIcmsStSeparado} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, destacaIcmsStSeparado: checked }))} />
                          <ToggleField label="Pode dropshipping" checked={empresaGrupoForm.permiteDropshipping} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, permiteDropshipping: checked }))} />
                          <ToggleField label="Pode marketplace" checked={empresaGrupoForm.permiteMarketplace} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, permiteMarketplace: checked }))} />
                          <ToggleField label="Emitente preferencial" checked={empresaGrupoForm.emitentePreferencial} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, emitentePreferencial: checked }))} />
                          <ToggleField label="Empresa ativa" checked={empresaGrupoForm.ativa} onChange={(checked) => setEmpresaGrupoForm((form) => ({ ...form, ativa: checked }))} />
                        </div>
                      </div>

                      <TextAreaField label="Benefícios estratégicos" value={empresaGrupoForm.beneficiosEstrategicos} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, beneficiosEstrategicos: value }))} />
                      <TextAreaField label="Resumo contratual" value={empresaGrupoForm.contratoResumo} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, contratoResumo: value }))} />
                      <TextAreaField label="Observações fiscais e operacionais" value={empresaGrupoForm.observacoes} onChange={(value) => setEmpresaGrupoForm((form) => ({ ...form, observacoes: value }))} />
                        </SimpleForm>

                        <div className="space-y-6">
                          <CadastroInsightPanel title="Proteções do cadastro fiscal" checks={empresaGrupoHighlights} />
                          <CompactList title="Empresas já mapeadas" items={empresasGrupo} fields={['razaoSocial', 'cnpj', 'regimeTributario', 'cidade']} />
                        </div>
                      </div>
                    </div>
                  </div>
                </section>
              )}

              {activeTab === 'erp-fiscal' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <div className="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
                    <ErpCockpitMenu activeTab={activeTab} onNavigate={openMainTab} />
                    <div className="space-y-6">
                      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                        <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Fiscal operacional</p>
                        <h2 className="mt-2 text-2xl font-black text-slate-950">Notas e automação fiscal</h2>
                        <p className="mt-1 max-w-3xl text-sm text-slate-500">
                          A ERP agora expõe a fila fiscal dos pedidos, com empresa emitente sugerida, ambiente, CFOP e resumo da automação preparada no ato da compra.
                        </p>
                      </div>

                      <div className="grid gap-4 md:grid-cols-4">
                        <StatMiniCard label="Pendências fiscais" value={fiscalPedidos.filter((item) => item.statusNfe === 'Pendente').length} />
                        <StatMiniCard label="Autorizadas" value={fiscalPedidos.filter((item) => item.statusNfe === 'Autorizada').length} />
                        <StatMiniCard label="Empresas emitentes" value={new Set(fiscalPedidos.map((item) => item.codigoEmpresaEmitente).filter(Boolean)).size} />
                        <StatMiniCard label="Pedidos na fila" value={fiscalPedidos.length} />
                      </div>

                      <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
                        <div className="border-b border-slate-200 px-6 py-5">
                          <h3 className="text-lg font-black text-slate-950">Fila fiscal dos pedidos</h3>
                          <p className="mt-1 text-sm text-slate-500">Pré-emissão automática gerada a partir do checkout e do roteamento fiscal.</p>
                        </div>
                        <div className="overflow-x-auto">
                          <table className="w-full min-w-[1100px]">
                        <thead className="bg-slate-50">
                          <tr>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Pedido</th>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Emitente</th>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Documento</th>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">CFOP</th>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Status</th>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Total</th>
                            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Resumo</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                          {fiscalPedidos.length === 0 ? (
                            <tr>
                              <td colSpan="7" className="px-6 py-8 text-center text-slate-500">Nenhum registro fiscal gerado ainda.</td>
                            </tr>
                          ) : fiscalPedidos.map((item) => (
                            <tr key={item.id} className="hover:bg-slate-50">
                              <td className="px-6 py-4 font-mono text-sm font-black text-slate-950">#{item.pedidoId}</td>
                              <td className="px-6 py-4">
                                <p className="font-black text-slate-950">{item.empresaEmitente || '-'}</p>
                                <p className="mt-1 text-xs font-semibold text-slate-500">{item.codigoEmpresaEmitente || '-'}</p>
                              </td>
                              <td className="px-6 py-4 text-sm font-semibold text-slate-600">
                                <p>{item.modeloDocumento || 'NFe'}</p>
                                <p className="mt-1 text-xs text-slate-400">{item.ambienteDocumento || '-'}</p>
                              </td>
                              <td className="px-6 py-4 text-sm font-black text-slate-700">{item.cfop || '-'}</td>
                              <td className="px-6 py-4">
                                <span className="rounded-full bg-amber-50 px-3 py-1 text-xs font-black text-amber-900">
                                  {item.statusNfe} · {item.statusAutomacao || 'Aguardando'}
                                </span>
                              </td>
                              <td className="px-6 py-4 text-sm font-black text-slate-950">{formatPrice(item.valorTotal || 0)}</td>
                              <td className="px-6 py-4 text-sm font-semibold text-slate-500">{item.resumoRoteamento || '-'}</td>
                            </tr>
                          ))}
                        </tbody>
                          </table>
                        </div>
                      </section>
                    </div>
                  </div>
                </section>
              )}

              {activeTab === 'erp-financeiro' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <ErpModuleHero
                    eyebrow="Financeiro operacional"
                    title="Fluxo de caixa, contas e conciliação"
                    description="Receitas, despesas, baixas, contas a receber e contas a pagar operando direto pela API e pelo banco."
                  />
                  <ModuleActionGrid
                    title="Acessos do financeiro"
                    actions={[
                      { label: 'Pedidos para faturar', detail: 'Conferir pedidos aprovados e separar recebimento.', onClick: () => openMainTab('pedidos') },
                      { label: 'Clientes e cobrança', detail: 'Abrir a base de clientes para relacionamento e cobrança.', onClick: () => openMainTab('cadastro-clientes') },
                      { label: 'Notas e impostos', detail: 'Ir direto para o módulo fiscal da ERP.', onClick: () => openMainTab('erp-fiscal') },
                      { label: 'Integrações de gateway', detail: 'Validar meios de pagamento e webhooks.', onClick: () => openMainTab('integracoes') },
                    ]}
                  />
                  <div className="grid gap-4 md:grid-cols-4">
                    <StatMiniCard label="Contas a receber" value={formatPrice(financeiroResumo.contasAReceber)} />
                    <StatMiniCard label="Contas a pagar" value={formatPrice(financeiroResumo.contasAPagar)} />
                    <StatMiniCard label="Saldo aberto" value={formatPrice(financeiroResumo.saldoAberto)} />
                    <StatMiniCard label="Pendências" value={financeiroResumo.pendentes} />
                  </div>
                  <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
                    <SimpleForm
                      title="Novo lançamento financeiro"
                      subtitle="Registre receita, despesa, taxa, estorno ou transferência com vínculo opcional ao pedido."
                      onSubmit={submitFinanceiroLancamento}
                      buttonLabel="Registrar lançamento"
                    >
                      <div className="grid gap-4 sm:grid-cols-2">
                        <SelectField
                          label="Tipo"
                          value={financeiroLancamentoForm.tipo}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, tipo: value }))}
                          options={['Receita', 'Despesa', 'Transferencia', 'Estorno', 'Taxa']}
                        />
                        <SelectField
                          label="Status"
                          value={financeiroLancamentoForm.status}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, status: value }))}
                          options={['Pendente', 'Pago', 'Atrasado', 'Cancelado', 'Estornado']}
                        />
                        <Field
                          label="Categoria"
                          value={financeiroLancamentoForm.categoria}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, categoria: value }))}
                        />
                        <Field
                          label="Valor"
                          type="number"
                          required
                          value={financeiroLancamentoForm.valor}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, valor: value }))}
                        />
                        <Field
                          label="Vencimento"
                          type="date"
                          value={financeiroLancamentoForm.dataVencimento}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, dataVencimento: value }))}
                        />
                        <Field
                          label="Pedido vinculado"
                          type="number"
                          value={financeiroLancamentoForm.pedidoId}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, pedidoId: value }))}
                        />
                        <Field
                          label="Meio de pagamento"
                          value={financeiroLancamentoForm.meioPagamento}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, meioPagamento: value }))}
                        />
                        <Field
                          label="Conta bancária"
                          value={financeiroLancamentoForm.contaBancaria}
                          onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, contaBancaria: value }))}
                        />
                      </div>
                      <Field
                        label="Descrição"
                        value={financeiroLancamentoForm.descricao}
                        onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, descricao: value }))}
                      />
                      <TextAreaField
                        label="Observações"
                        rows={3}
                        value={financeiroLancamentoForm.observacoes}
                        onChange={(value) => setFinanceiroLancamentoForm((current) => ({ ...current, observacoes: value }))}
                      />
                    </SimpleForm>

                    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                        <div>
                          <h3 className="text-lg font-black text-slate-950">Carteira financeira</h3>
                          <p className="mt-1 text-sm font-semibold text-slate-500">
                            {financeiroLancamentosFiltrados.length} lançamentos em acompanhamento.
                          </p>
                        </div>
                        <select
                          value={financeiroFiltro}
                          onChange={(event) => setFinanceiroFiltro(event.target.value)}
                          className="h-11 rounded-lg border border-slate-200 bg-white px-3 text-sm font-black text-slate-700 outline-none focus:border-slate-950"
                        >
                          <option value="todos">Todos</option>
                          <option value="pendente">Pendentes</option>
                          <option value="pago">Pagos</option>
                          <option value="atrasado">Atrasados</option>
                          <option value="cancelado">Cancelados</option>
                          <option value="estornado">Estornados</option>
                        </select>
                      </div>

                      <div className="mt-5 grid gap-3">
                        {financeiroLancamentosFiltrados.slice(0, 12).map((item) => {
                          const id = item.id ?? item.Id;
                          const status = getFinanceiroStatus(item);
                          const tipo = getFinanceiroTipo(item);
                          const statusLower = status.toLowerCase();

                          return (
                            <article key={id} className="rounded-lg border border-slate-200 bg-slate-50 p-4">
                              <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                                <div>
                                  <div className="flex flex-wrap items-center gap-2">
                                    <span className="rounded-full bg-slate-950 px-3 py-1 text-[11px] font-black uppercase tracking-[0.14em] text-white">
                                      {tipo || 'Financeiro'}
                                    </span>
                                    <span className="rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-[11px] font-black uppercase tracking-[0.14em] text-amber-900">
                                      {status || 'Pendente'}
                                    </span>
                                  </div>
                                  <p className="mt-3 text-base font-black text-slate-950">{getFinanceiroDescricao(item)}</p>
                                  <p className="mt-1 text-sm font-semibold text-slate-500">
                                    {getFinanceiroCategoria(item)} · Pedido {getFinanceiroNumeroPedido(item) || getFinanceiroPedidoId(item) || 'sem vínculo'}
                                  </p>
                                  <p className="mt-2 text-xs font-semibold text-slate-400">
                                    Vencimento: {formatDate(getFinanceiroVencimento(item))} · Pagamento: {formatDate(getFinanceiroPagamento(item))}
                                  </p>
                                </div>
                                <div className="text-left lg:text-right">
                                  <p className="text-2xl font-black text-slate-950">{formatPrice(getFinanceiroValor(item))}</p>
                                  <div className="mt-3 flex flex-wrap gap-2 lg:justify-end">
                                    {statusLower !== 'pago' && statusLower !== 'cancelado' && (
                                      <button
                                        type="button"
                                        onClick={() => atualizarFinanceiroLancamentoStatus(id, 'Pago')}
                                        className="h-9 rounded-lg bg-emerald-600 px-3 text-xs font-black uppercase tracking-[0.12em] text-white transition hover:bg-emerald-700"
                                      >
                                        Baixar
                                      </button>
                                    )}
                                    {statusLower !== 'cancelado' && statusLower !== 'pago' && (
                                      <button
                                        type="button"
                                        onClick={() => atualizarFinanceiroLancamentoStatus(id, 'Cancelado')}
                                        className="h-9 rounded-lg border border-slate-200 bg-white px-3 text-xs font-black uppercase tracking-[0.12em] text-slate-700 transition hover:border-rose-300 hover:text-rose-700"
                                      >
                                        Cancelar
                                      </button>
                                    )}
                                  </div>
                                </div>
                              </div>
                            </article>
                          );
                        })}

                        {financeiroLancamentosFiltrados.length === 0 && (
                          <div className="rounded-lg border border-dashed border-slate-200 bg-slate-50 px-4 py-8 text-center text-sm font-bold text-slate-500">
                            Nenhum lançamento financeiro encontrado para este filtro.
                          </div>
                        )}
                      </div>
                    </section>
                  </div>

                  <div className="grid gap-6 xl:grid-cols-2">
                    <ErpChecklistCard
                      title="Controles financeiros ativos"
                      items={[
                        'Lançamento manual operacional com tipo, status, vencimento e vínculo com pedido.',
                        'Baixa de contas a receber e contas a pagar pelo painel administrativo.',
                        'Separação entre receitas, despesas, taxas, estornos e transferências.',
                        'Base aberta para conciliação com gateway, banco, DRE e relatórios contábeis.',
                      ]}
                    />
                    <ErpChecklistCard
                      title="Próxima amarração do checklist"
                      items={[
                        'Ligar pagamentos reais dos gateways às baixas automáticas.',
                        'Fechar integração fiscal para nota, boleto, comprovante e cobrança.',
                        'Conectar logística para custo de frete e baixa por entrega.',
                        'Abrir relatórios gerenciais por empresa, loja, canal e centro de custo.',
                      ]}
                    />
                  </div>
                </section>
              )}

              {activeTab === 'erp-logistica' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <ErpModuleHero
                    eyebrow="Logística e estoque"
                    title="Despacho, rastreamento e posição operacional"
                    description="Tela dedicada para separar estoque, despacho, rastreamento e estrutura de frete sem deixar a operação escondida."
                  />
                  <ModuleActionGrid
                    title="Acessos da logística"
                    actions={[
                      { label: 'Produtos e estoque', detail: 'Abrir o cadastro com peso, altura, largura e estoque.', onClick: () => openMainTab('cadastro-produtos') },
                      { label: 'Pedidos em separação', detail: 'Ir para os pedidos e acompanhar expedição.', onClick: () => openMainTab('pedidos') },
                      { label: 'Fornecedores e dropship', detail: 'Conferir parceiros logísticos e fornecedores.', onClick: () => openMainTab('cadastro-fornecedores') },
                      { label: 'Integrações de frete', detail: 'Acessar conectores externos de logística.', onClick: () => openMainTab('integracoes') },
                    ]}
                  />
                  <div className="grid gap-4 md:grid-cols-4">
                    <StatMiniCard label="Pedidos na operação" value={pedidos.length} />
                    <StatMiniCard label="Produtos ativos" value={produtos.length} />
                    <StatMiniCard label="Fornecedores" value={fornecedores.length} />
                    <StatMiniCard label="Integrações logísticas" value={integracoes.filter((item) => ['logistica', 'melhorenvio'].includes(item.slug)).length} />
                  </div>
                  <div className="grid gap-6 xl:grid-cols-2">
                    <ErpChecklistCard
                      title="Frentes logísticas"
                      items={[
                        'Rastreamento detalhado por evento.',
                        'Despacho por transportadora e retirada local.',
                        'Peso, volume e cubagem ligados ao cadastro do produto.',
                        'Base pronta para etiqueta, CT-e e expedição.',
                      ]}
                    />
                    <CompactList title="Produtos com foco logístico" items={produtos} fields={['nome', 'estoque', 'peso', 'altura']} />
                  </div>
                </section>
              )}

              {activeTab === 'erp-rh' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <ErpModuleHero
                    eyebrow="RH e diretoria operacional"
                    title="Equipe, cargos e supervisão da rotina"
                    description="Tela separada para estruturar RH/DP, cargos, responsáveis e apoio operacional do GenesisGest.Net."
                  />
                  <ModuleActionGrid
                    title="Acessos do RH"
                    actions={[
                      { label: 'Empresas do grupo', detail: 'Ver empresas, centros de custo e emitentes.', onClick: () => openMainTab('erp-empresas') },
                      { label: 'Compras e suprimentos', detail: 'Acessar o módulo separado de compras.', onClick: () => openMainTab('erp-compras') },
                      { label: 'Relatórios gerenciais', detail: 'Abrir indicadores executivos da operação.', onClick: () => openMainTab('erp-relatorios') },
                    ]}
                  />
                  <div className="grid gap-6 xl:grid-cols-2">
                    <ErpChecklistCard
                      title="Estrutura prevista"
                      items={[
                        'Cadastro de cargos e salários.',
                        'Vinculação de responsáveis por setor.',
                        'Base para folha e custos por centro de resultado.',
                        'Governança para diretoria e operação diária.',
                      ]}
                    />
                    <ErpChecklistCard
                      title="Ajuda operacional"
                      items={[
                        'Botão direto para suporte operacional quando necessário.',
                        'Leitura de indicadores e alertas de operação.',
                        'Apoio na navegação entre módulos críticos.',
                        'Preparação para rotinas executivas futuras.',
                      ]}
                    />
                  </div>
                </section>
              )}

              {activeTab === 'erp-compras' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <ErpModuleHero
                    eyebrow="Compras e suprimentos"
                    title="Aquisição, entradas e reposição conectadas ao estoque"
                    description="Compras diretas, dropshipping, parcerias e encomendas alimentam estoque, fiscal, financeiro e visão empresarial pelo banco."
                  />
                  <ModuleActionGrid
                    title="Acessos de compras"
                    actions={[
                      { label: 'Fornecedores', detail: 'Abrir a tela separada de fornecedores.', onClick: () => openMainTab('cadastro-fornecedores') },
                      { label: 'Produtos e custo', detail: 'Ir para o cadastro de produtos e revisar margens.', onClick: () => openMainTab('cadastro-produtos') },
                      { label: 'Empresas do grupo', detail: 'Relacionar emitente, estoque e centro de custo.', onClick: () => openMainTab('erp-empresas') },
                      { label: 'Logística e estoque', detail: 'Conferir necessidade de reposição e despacho.', onClick: () => openMainTab('erp-logistica') },
                    ]}
                  />
                  <div className="grid gap-4 md:grid-cols-4">
                    <StatMiniCard label="Solicitações abertas" value={comprasPainel?.kpis?.solicitacoesAbertas ?? 0} />
                    <StatMiniCard label="Pedidos de compra" value={comprasPainel?.kpis?.pedidosAbertos ?? 0} />
                    <StatMiniCard label="Entradas no mês" value={comprasPainel?.kpis?.entradasMes ?? 0} />
                    <StatMiniCard label="Valor em aberto" value={formatPrice(comprasPainel?.kpis?.valorComprasAbertas ?? 0)} />
                  </div>
                  {(comprasPainel?.alertas || []).length > 0 && (
                    <section className="rounded-2xl border border-amber-200 bg-amber-50 p-5">
                      <p className="text-xs font-black uppercase tracking-[0.2em] text-amber-700">Alertas de suprimentos</p>
                      <div className="mt-3 grid gap-2">
                        {comprasPainel.alertas.map((alerta) => (
                          <p key={alerta} className="rounded-xl bg-white px-4 py-3 text-sm font-bold text-slate-700 shadow-sm">{alerta}</p>
                        ))}
                      </div>
                    </section>
                  )}
                  <div className="grid gap-6 xl:grid-cols-2 2xl:grid-cols-4">
                    <SimpleForm
                      title="Abrir solicitação"
                      subtitle="Ponto inicial da aquisição: necessidade, origem, prioridade e finalidade."
                      onSubmit={submitCompraSolicitacao}
                      buttonLabel="Salvar solicitação"
                    >
                      <OptionSelectField
                        label="Produto do estoque"
                        value={compraSolicitacaoForm.produtoId}
                        onChange={(value) => {
                          const option = compraProdutoOptions.find((item) => item.value === value);
                          const produto = option?.produto;
                          setCompraSolicitacaoForm((current) => ({
                            ...current,
                            produtoId: value,
                            produtoNome: getCompraProdutoNome(produto) || current.produtoNome,
                          }));
                        }}
                        options={compraProdutoOptions}
                        placeholder="Selecione ou descreva abaixo"
                      />
                      <Field label="Item livre, parceria ou encomenda" value={compraSolicitacaoForm.produtoNome} onChange={(value) => setCompraSolicitacaoForm((current) => ({ ...current, produtoNome: value }))} />
                      <Field label="Quantidade solicitada" type="number" value={compraSolicitacaoForm.quantidade} onChange={(value) => setCompraSolicitacaoForm((current) => ({ ...current, quantidade: value }))} />
                      <div className="grid gap-4 md:grid-cols-2">
                        <SelectField label="Origem" value={compraSolicitacaoForm.origem} onChange={(value) => setCompraSolicitacaoForm((current) => ({ ...current, origem: value }))} options={['EstoqueFisico', 'Dropshipping', 'FornecedorDireto', 'Parceria', 'Encomenda']} />
                        <SelectField label="Prioridade" value={compraSolicitacaoForm.prioridade} onChange={(value) => setCompraSolicitacaoForm((current) => ({ ...current, prioridade: value }))} options={['Baixa', 'Normal', 'Alta', 'Urgente']} />
                      </div>
                      <Field label="Finalidade" value={compraSolicitacaoForm.finalidade} onChange={(value) => setCompraSolicitacaoForm((current) => ({ ...current, finalidade: value }))} />
                      <TextAreaField label="Observações da necessidade" rows={3} value={compraSolicitacaoForm.observacoes} onChange={(value) => setCompraSolicitacaoForm((current) => ({ ...current, observacoes: value }))} />
                    </SimpleForm>

                    <SimpleForm
                      title="Registrar cotação"
                      subtitle="Primeira etapa de compra: fornecedor, item, custo, origem e finalidade."
                      onSubmit={submitCompraCotacao}
                      buttonLabel="Salvar cotação"
                      disabled={compraFornecedorOptions.length === 0}
                      alertMessage={compraFornecedorOptions.length === 0 ? 'Cadastre ao menos um fornecedor real antes de cotar compras.' : ''}
                    >
                      <OptionSelectField
                        label="Fornecedor"
                        value={compraCotacaoForm.fornecedorId}
                        onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, fornecedorId: value }))}
                        options={compraFornecedorOptions}
                      />
                      <OptionSelectField
                        label="Produto do estoque"
                        value={compraCotacaoForm.produtoId}
                        onChange={(value) => {
                          const option = compraProdutoOptions.find((item) => item.value === value);
                          const produto = option?.produto;
                          setCompraCotacaoForm((current) => ({
                            ...current,
                            produtoId: value,
                            produtoNome: getCompraProdutoNome(produto) || current.produtoNome,
                            custoUnitario: produto?.custoAtual || produto?.custo || produto?.Custo ? String(produto.custoAtual ?? produto.custo ?? produto.Custo) : current.custoUnitario,
                          }));
                        }}
                        options={compraProdutoOptions}
                        placeholder="Selecione ou descreva abaixo"
                      />
                      <Field label="Item livre, parceria ou encomenda" value={compraCotacaoForm.produtoNome} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, produtoNome: value }))} />
                      <div className="grid gap-4 md:grid-cols-2">
                        <Field label="Quantidade" type="number" value={compraCotacaoForm.quantidade} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, quantidade: value }))} />
                        <Field label="Custo unitário" type="number" value={compraCotacaoForm.custoUnitario} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, custoUnitario: value }))} />
                      </div>
                      <div className="grid gap-4 md:grid-cols-2">
                        <SelectField label="Origem" value={compraCotacaoForm.origem} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, origem: value }))} options={['EstoqueFisico', 'Dropshipping', 'FornecedorDireto', 'Parceria', 'Encomenda']} />
                        <SelectField label="Prioridade" value={compraCotacaoForm.prioridade} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, prioridade: value }))} options={['Baixa', 'Normal', 'Alta', 'Urgente']} />
                      </div>
                      <Field label="Finalidade" value={compraCotacaoForm.finalidade} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, finalidade: value }))} />
                      <Field label="Prazo previsto em dias" type="number" value={compraCotacaoForm.prazoEntregaDias} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, prazoEntregaDias: value }))} />
                      <TextAreaField label="Observações da cotação" rows={3} value={compraCotacaoForm.observacoes} onChange={(value) => setCompraCotacaoForm((current) => ({ ...current, observacoes: value }))} />
                    </SimpleForm>

                    <SimpleForm
                      title="Gerar pedido de compra"
                      subtitle="Cria pedido, conta a pagar e vínculo para entrada posterior."
                      onSubmit={submitCompraPedido}
                      buttonLabel="Criar pedido"
                      disabled={compraFornecedorOptions.length === 0}
                      alertMessage={compraFornecedorOptions.length === 0 ? 'Cadastre ao menos um fornecedor real antes de abrir pedido.' : ''}
                    >
                      <OptionSelectField
                        label="Fornecedor"
                        value={compraPedidoForm.fornecedorId}
                        onChange={(value) => setCompraPedidoForm((current) => ({ ...current, fornecedorId: value }))}
                        options={compraFornecedorOptions}
                      />
                      <OptionSelectField
                        label="Solicitação vinculada"
                        value={compraPedidoForm.solicitacaoId}
                        onChange={(value) => {
                          const option = compraSolicitacaoOptions.find((item) => item.value === value);
                          const solicitacao = option?.solicitacao;
                          setCompraPedidoForm((current) => ({
                            ...current,
                            solicitacaoId: value,
                            produtoId: solicitacao?.produtoId ? String(solicitacao.produtoId) : current.produtoId,
                            produtoNome: solicitacao?.produtoNome || current.produtoNome,
                            quantidade: solicitacao?.quantidade ? String(solicitacao.quantidade) : current.quantidade,
                            origem: solicitacao?.origem || current.origem,
                            finalidade: solicitacao?.finalidade || current.finalidade,
                          }));
                        }}
                        options={compraSolicitacaoOptions}
                        placeholder="Opcional: vincular cotação/solicitação"
                      />
                      <OptionSelectField
                        label="Produto do estoque"
                        value={compraPedidoForm.produtoId}
                        onChange={(value) => {
                          const option = compraProdutoOptions.find((item) => item.value === value);
                          const produto = option?.produto;
                          setCompraPedidoForm((current) => ({
                            ...current,
                            produtoId: value,
                            produtoNome: getCompraProdutoNome(produto) || current.produtoNome,
                            sku: getCompraProdutoSku(produto) || current.sku,
                            custoUnitario: produto?.custoAtual || produto?.custo || produto?.Custo ? String(produto.custoAtual ?? produto.custo ?? produto.Custo) : current.custoUnitario,
                          }));
                        }}
                        options={compraProdutoOptions}
                        placeholder="Selecione ou descreva abaixo"
                      />
                      <Field label="Item livre, parceria ou encomenda" value={compraPedidoForm.produtoNome} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, produtoNome: value }))} />
                      <Field label="SKU ou referência" value={compraPedidoForm.sku} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, sku: value }))} />
                      <div className="grid gap-4 md:grid-cols-2">
                        <Field label="Quantidade" type="number" value={compraPedidoForm.quantidade} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, quantidade: value }))} />
                        <Field label="Custo unitário" type="number" value={compraPedidoForm.custoUnitario} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, custoUnitario: value }))} />
                      </div>
                      <div className="grid gap-4 md:grid-cols-2">
                        <SelectField label="Origem" value={compraPedidoForm.origem} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, origem: value }))} options={['EstoqueFisico', 'Dropshipping', 'FornecedorDireto', 'Parceria', 'Encomenda']} />
                        <Field label="Meio de pagamento" value={compraPedidoForm.meioPagamento} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, meioPagamento: value }))} />
                      </div>
                      <Field label="Finalidade" value={compraPedidoForm.finalidade} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, finalidade: value }))} />
                      <div className="grid gap-4 md:grid-cols-2">
                        <Field label="Entrega prevista" type="date" value={compraPedidoForm.dataPrevistaEntrega} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, dataPrevistaEntrega: value }))} />
                        <Field label="Vencimento financeiro" type="date" value={compraPedidoForm.dataVencimento} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, dataVencimento: value }))} />
                      </div>
                      <TextAreaField label="Observações do pedido" rows={3} value={compraPedidoForm.observacoes} onChange={(value) => setCompraPedidoForm((current) => ({ ...current, observacoes: value }))} />
                    </SimpleForm>

                    <SimpleForm
                      title="Entrada de mercadoria"
                      subtitle="Conclui o recebimento, atualiza estoque e marca a conferência fiscal."
                      onSubmit={submitCompraEntrada}
                      buttonLabel="Registrar entrada"
                      disabled={compraPedidoEntradaOptions.length === 0}
                      alertMessage={compraPedidoEntradaOptions.length === 0 ? 'Abra um pedido de compra antes de lançar entrada de mercadoria.' : ''}
                    >
                      <OptionSelectField
                        label="Pedido de compra"
                        value={compraEntradaForm.pedidoId}
                        onChange={(value) => {
                          const pedido = compraPedidoEntradaOptions.find((item) => item.value === value)?.pedido;
                          const itens = {};
                          (pedido?.itens || []).forEach((item) => {
                            itens[String(item.id)] = String(item.quantidadePendente ?? Math.max(0, (item.quantidade || 0) - (item.quantidadeRecebida || 0)));
                          });
                          setCompraEntradaForm((current) => ({ ...current, pedidoId: value, itens }));
                        }}
                        options={compraPedidoEntradaOptions}
                      />
                      {(selectedCompraEntradaPedido?.itens || []).length > 0 && (
                        <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                          <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Itens para recebimento</p>
                          <div className="mt-3 grid gap-3">
                            {selectedCompraEntradaPedido.itens.map((item) => (
                              <div key={item.id} className="grid gap-2 rounded-xl bg-white p-3 shadow-sm md:grid-cols-[minmax(0,1fr)_120px]">
                                <div>
                                  <p className="text-sm font-black text-slate-950">{item.produtoNome}</p>
                                  <p className="mt-1 text-xs font-semibold text-slate-500">
                                    Pedido: {item.quantidade} · Recebido: {item.quantidadeRecebida || 0} · Pendente: {item.quantidadePendente || 0}
                                  </p>
                                </div>
                                <Field
                                  label="Receber"
                                  type="number"
                                  value={compraEntradaForm.itens?.[String(item.id)] ?? ''}
                                  onChange={(value) => setCompraEntradaForm((current) => ({
                                    ...current,
                                    itens: { ...(current.itens || {}), [String(item.id)]: value },
                                  }))}
                                />
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                      <SelectField label="Tipo de entrada" value={compraEntradaForm.tipoEntrada} onChange={(value) => setCompraEntradaForm((current) => ({ ...current, tipoEntrada: value }))} options={['EstoqueFisico', 'Dropshipping', 'FornecedorDireto', 'Parceria', 'Encomenda']} />
                      <Field label="Documento de entrada" value={compraEntradaForm.numeroDocumento} onChange={(value) => setCompraEntradaForm((current) => ({ ...current, numeroDocumento: value }))} />
                      <Field label="Chave NF-e de entrada" value={compraEntradaForm.chaveNfeEntrada} onChange={(value) => setCompraEntradaForm((current) => ({ ...current, chaveNfeEntrada: value }))} />
                      <Field label="Recebido por" value={compraEntradaForm.recebidoPor} onChange={(value) => setCompraEntradaForm((current) => ({ ...current, recebidoPor: value }))} />
                      <TextAreaField label="Observações do recebimento" rows={3} value={compraEntradaForm.observacoes} onChange={(value) => setCompraEntradaForm((current) => ({ ...current, observacoes: value }))} />
                    </SimpleForm>
                  </div>
                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
                    <CompactList
                      title="Solicitações e cotações"
                      items={comprasPainel?.solicitacoes || []}
                      fields={[
                        { key: 'produtoNome', label: 'Item' },
                        { key: 'quantidade', label: 'Qtd.' },
                        { key: 'finalidade', label: 'Finalidade' },
                        { key: 'origem', label: 'Origem' },
                        { key: 'status', label: 'Status' },
                        { key: 'prioridade', label: 'Prioridade' },
                        { key: 'createdAt', label: 'Criado em', type: 'date' },
                      ]}
                      renderActions={(solicitacao) => (
                        <select
                          value={solicitacao.status || 'Aberta'}
                          onChange={(event) => updateCompraSolicitacaoStatus(solicitacao, event.target.value)}
                          className="rounded-full border border-slate-200 bg-white px-3 py-1 text-[11px] font-black text-slate-700"
                        >
                          {compraSolicitacaoStatusOptions.map((status) => (
                            <option key={status} value={status}>{status}</option>
                          ))}
                        </select>
                      )}
                    />
                    <CompactList
                      title="Pedidos de compra em acompanhamento"
                      items={comprasPainel?.pedidos || []}
                      fields={[
                        { key: 'numero', label: 'Pedido' },
                        { key: 'fornecedorNome', label: 'Fornecedor' },
                        { key: 'origem', label: 'Origem' },
                        { key: 'finalidade', label: 'Finalidade' },
                        { key: 'status', label: 'Status' },
                        { key: 'statusFiscal', label: 'Fiscal' },
                        { key: 'valorTotal', label: 'Valor', type: 'money' },
                        { key: 'dataPrevistaEntrega', label: 'Entrega', type: 'date' },
                        { key: 'createdAt', label: 'Criado em', type: 'date' },
                      ]}
                      renderActions={(pedido) => (
                        <select
                          value={pedido.status || 'Aberto'}
                          onChange={(event) => updateCompraPedidoStatus(pedido, event.target.value)}
                          className="rounded-full border border-slate-200 bg-white px-3 py-1 text-[11px] font-black text-slate-700"
                        >
                          {compraPedidoStatusOptions.map((status) => (
                            <option key={status} value={status}>{status}</option>
                          ))}
                        </select>
                      )}
                    />
                  </div>
                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
                    <CompactList
                      title="Produtos para reposição ou dropshipping"
                      items={comprasPainel?.produtosReposicao || []}
                      fields={[
                        { key: 'produtoNome', label: 'Produto' },
                        { key: 'sku', label: 'SKU' },
                        { key: 'tipoProduto', label: 'Tipo' },
                        { key: 'estoqueAtual', label: 'Estoque' },
                        { key: 'estoqueMinimo', label: 'Mínimo' },
                        { key: 'fornecedorId', label: 'Fornecedor' },
                        { key: 'custoAtual', label: 'Custo', type: 'money' },
                      ]}
                    />
                    <CompactList
                      title="Entradas de mercadoria"
                      items={comprasPainel?.entradas || []}
                      fields={[
                        { key: 'pedidoNumero', label: 'Pedido' },
                        { key: 'fornecedorNome', label: 'Fornecedor' },
                        { key: 'numeroDocumento', label: 'Documento' },
                        { key: 'chaveNfeEntrada', label: 'NF-e' },
                        { key: 'tipoEntrada', label: 'Tipo' },
                        { key: 'statusFiscal', label: 'Fiscal' },
                        { key: 'valorTotal', label: 'Valor', type: 'money' },
                        { key: 'createdAt', label: 'Criado em', type: 'date' },
                      ]}
                    />
                    <ErpChecklistCard
                      title="Fluxo operacional aplicado"
                      items={[
                        'Cotação registra fornecedor, origem, finalidade, quantidade e custo.',
                        'Pedido de compra gera conta a pagar no financeiro e no Genesis quando disponível.',
                        'Entrada atualiza estoque físico e cria movimento rastreável.',
                        'Documento fiscal ou NF-e de entrada fica sinalizado para conferência.',
                      ]}
                    />
                  </div>
                </section>
              )}

              {activeTab === 'erp-relatorios' && (
                <section className="erp-desktop-surface space-y-6">
                  <ErpWorkspaceNav activeTab={activeTab} onNavigate={openMainTab} />
                  <ErpModuleHero
                    eyebrow="Relatórios executivos"
                    title="KPIs, gráficos e controle gerencial"
                    description="Tela dedicada para consolidar indicadores da operação e abrir caminho para relatórios profundos do grupo."
                  />
                  <ModuleActionGrid
                    title="Acessos de análise"
                    actions={[
                      { label: 'Financeiro', detail: 'Cruzar receita, liquidação e caixa.', onClick: () => openMainTab('erp-financeiro') },
                      { label: 'Fiscal', detail: 'Auditar emissão, impostos e roteamento.', onClick: () => openMainTab('erp-fiscal') },
                      { label: 'CRM', detail: 'Analisar leads, clientes e relacionamento.', onClick: () => openMainTab('crm') },
                      { label: 'Empresas do grupo', detail: 'Comparar resultado por empresa emitente.', onClick: () => openMainTab('erp-empresas') },
                    ]}
                  />
                  <div className="grid gap-4 md:grid-cols-4">
                    <StatMiniCard label="Clientes" value={clientes.length} />
                    <StatMiniCard label="Leads" value={leads.length} />
                    <StatMiniCard label="Empresas do grupo" value={empresasGrupo.length} />
                    <StatMiniCard label="Pedidos" value={pedidos.length} />
                  </div>
                  <div className="grid gap-6 xl:grid-cols-2">
                    <ErpChecklistCard
                      title="Relatórios previstos"
                      items={[
                        'Margem por empresa emitente.',
                        'Receita por loja e canal.',
                        'Tributação e custo operacional por pedido.',
                        'Auditoria de cadastro, pedido, cliente e fornecedor.',
                      ]}
                    />
                    <ErpChecklistCard
                      title="Visão de diretoria"
                      items={[
                        'Indicadores prontos para apresentação.',
                        'Leitura executiva de produtividade comercial.',
                        'Base para painéis financeiros e fiscais.',
                        'Expansão futura para chao de fábrica e PDV premium.',
                      ]}
                    />
                  </div>
                </section>
              )}

              {activeTab === 'configuracoes-site' && (
                <section className="space-y-6">
                  <ErpModuleHero
                    eyebrow="Portal de vendas"
                    title="Configurações públicas da home"
                    description="Tudo o que aparece na vitrine principal deve sair do banco: banners, contatos, Yara, textos institucionais e cards de parceria."
                  />
                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.05fr)_minmax(320px,0.95fr)]">
                    <form onSubmit={submitSiteConfiguracoes} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex items-start justify-between gap-4">
                        <div>
                          <h3 className="text-lg font-black text-slate-950">Editor da home comercial</h3>
                          <p className="mt-1 text-sm text-slate-500">Salvar aqui atualiza o banco e prepara a home para banners, anúncios e contatos reais.</p>
                        </div>
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black uppercase tracking-[0.16em] text-slate-600">
                          {siteConfigItems.length || siteConfigFieldMeta.length} chaves
                        </span>
                      </div>
                      <div className="mt-6 grid gap-4">
                        {siteConfigFieldMeta.map((field) => (
                          <label key={field.key} className="block text-sm font-bold text-slate-700">
                            {field.label}
                            {field.type === 'textarea' ? (
                              <textarea
                                value={siteConfigForm[field.key] ?? ''}
                                onChange={(event) => setSiteConfigForm((current) => ({ ...current, [field.key]: event.target.value }))}
                                className="mt-2 min-h-28 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                              />
                            ) : (
                              <input
                                type="text"
                                value={siteConfigForm[field.key] ?? ''}
                                onChange={(event) => setSiteConfigForm((current) => ({ ...current, [field.key]: event.target.value }))}
                                className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                              />
                            )}
                            <span className="mt-2 block text-xs font-semibold text-slate-400">{field.description}</span>
                            {field.imagePicker && (
                              <span className="mt-3 flex flex-wrap items-center gap-3">
                                <span className="inline-flex cursor-pointer items-center gap-2 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-black text-slate-700 transition hover:border-slate-950 hover:bg-white">
                                  <ImagePlus size={15} />
                                  Selecionar imagem
                                  <input
                                    type="file"
                                    accept="image/*,.svg"
                                    className="sr-only"
                                    onChange={(event) => {
                                      handleSiteConfigImageSelect(field.key, event.target.files?.[0]);
                                      event.target.value = '';
                                    }}
                                  />
                                </span>
                                <span className="text-xs font-semibold text-slate-400">
                                  Arquivos devem ficar em public/imagens/homepage.
                                </span>
                              </span>
                            )}
                          </label>
                        ))}
                      </div>
                      <button className="mt-5 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white">
                        <Save size={17} />
                        Salvar configuração pública
                      </button>
                    </form>
                    <div className="space-y-6">
                      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-slate-950 text-white shadow-sm">
                        <div className="border-b border-white/10 px-5 py-4">
                          <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Espelho da home</p>
                          <p className="mt-1 text-sm text-slate-300">Prévia condensada do que o cliente verá na home pública.</p>
                        </div>

                        <div className="bg-[#050505]">
                          <div className="flex items-center justify-between gap-3 border-b border-white/10 px-5 py-3">
                            <div className="flex items-center gap-3">
                              <img src={normalizePreviewImage(sitePreviewLogo)} alt="Preview do logo" className="h-11 w-11 rounded-2xl object-cover" />
                              <div>
                                <p className="text-xs font-black uppercase tracking-[0.2em] text-[#E8D5A3]">
                                  {siteConfigForm.site_nome || 'Grupo Nexum Altivon'}
                                </p>
                                <p className="text-[11px] text-slate-400">Configuração salva no banco</p>
                              </div>
                            </div>
                            <div className="hidden items-center gap-2 rounded-full border border-white/10 bg-white/5 px-3 py-1 md:flex">
                              {['Início', 'Catálogo', 'Lojas'].map((item) => (
                                <span key={item} className="rounded-full px-3 py-1 text-[11px] font-black uppercase tracking-[0.14em] text-slate-200">
                                  {item}
                                </span>
                              ))}
                            </div>
                          </div>

                          <div className="relative min-h-64 overflow-hidden">
                            {previewSlide.image ? (
                              <img
                                src={normalizePreviewImage(previewSlide.image)}
                                alt="Preview do banner"
                                className="absolute inset-0 h-full w-full object-cover opacity-30"
                              />
                            ) : null}
                            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,_rgba(201,162,39,0.16),_transparent_40%),linear-gradient(135deg,#0b0b0b,#1a1a1a)]" />
                            <div className="relative grid gap-4 px-5 py-6 md:grid-cols-[1fr_240px] md:items-center">
                              <div>
                                <p className="inline-flex rounded-full border border-[#C9A227]/30 bg-[#C9A227]/10 px-3 py-1 text-[11px] font-black uppercase tracking-[0.2em] text-[#E8D5A3]">
                                  {previewSlide.badge}
                                </p>
                                <h4 className="mt-4 text-3xl font-black leading-tight text-white">
                                  {previewSlide.title} <span className="block text-[#C9A227]">{previewSlide.highlight}</span>
                                </h4>
                                <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-300">{previewSlide.description}</p>
                                <div className="mt-5 flex flex-wrap gap-2">
                                  {(previewQualityItems.length > 0 ? previewQualityItems : ['Qualidade premium']).slice(0, 4).map((item) => (
                                    <span key={item} className="inline-flex rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-semibold text-slate-200">
                                      {item}
                                    </span>
                                  ))}
                                </div>
                              </div>

                              <div className="rounded-2xl border border-white/10 bg-black/35 p-4 backdrop-blur">
                                <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Chamada principal</p>
                                <p className="mt-3 text-sm font-semibold leading-6 text-slate-200">
                                  {siteConfigForm.home_intro_texto_1 || 'A home vai refletir o texto institucional definido no painel.'}
                                </p>
                                <p className="mt-2 text-xs text-slate-400">
                                  WhatsApp: {siteConfigForm.site_whatsapp || '5514996731879'}
                                </p>
                              </div>
                            </div>
                          </div>

                          <div className="grid gap-0 border-t border-white/10 md:grid-cols-[1.1fr_0.9fr]">
                            <div className="px-5 py-5">
                              <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Bloco institucional</p>
                              <h5 className="mt-3 text-xl font-black text-white">{siteConfigForm.home_intro_titulo || 'Uma Nova Era Começa'}</h5>
                              <p className="mt-3 text-sm leading-6 text-slate-300">
                                {siteConfigForm.home_intro_texto_2 || 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.'}
                              </p>
                              <div className="mt-4 inline-flex rounded-full border border-[#C9A227]/30 bg-[#C9A227]/10 px-4 py-2 text-xs font-black uppercase tracking-[0.18em] text-[#E8D5A3]">
                                {siteConfigForm.home_intro_badge || 'nexumaltivon.com.br'}
                              </div>
                            </div>

                            <div className="border-t border-white/10 px-5 py-5 md:border-l md:border-t-0">
                              <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-400">Parcerias e qualidade</p>
                              <div className="mt-3 space-y-2">
                                {(previewPartnerCards.length > 0 ? previewPartnerCards : [{ title: 'Parceiros de Vendas', text: 'Prévia carregada do banco.' }]).slice(0, 2).map((item) => (
                                  <div key={item.title} className="rounded-xl border border-white/10 bg-white/5 p-3">
                                    <p className="text-sm font-black text-white">{item.title}</p>
                                    <p className="mt-1 text-xs leading-5 text-slate-300 line-clamp-3">{item.text}</p>
                                  </div>
                                ))}
                              </div>
                            </div>
                          </div>

                          <div className="border-t border-white/10 px-5 py-4 text-xs font-semibold text-slate-400">
                            Rodapé: {siteConfigForm.home_footer_texto || 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.'}
                          </div>
                        </div>
                      </section>

                      <ErpChecklistCard
                        title="O que já vinha pronto no banco"
                        items={[
                          'A tabela configuracoes_sistema já existia mas não estava servindo a home.',
                          'Havia divergência de porta entre 3309 e 3306 na configuração da API.',
                          'Banners, contatos e blocos institucionais estavam presos no front e não no banco.',
                          'Agora a trilha comercial volta para o caminho certo: vitrine configurável sem mexer em código.',
                        ]}
                      />
                      <CompactList title="Chaves carregadas do banco" items={siteConfigItems} fields={['chave', 'grupo', 'tipo']} />
                    </div>
                  </div>
                </section>
              )}

              {activeTab === 'integracoes' && (
                <section className="space-y-6">
                  <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                      <div>
                        <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Trilha paralela</p>
                        <h2 className="mt-2 text-2xl font-black text-slate-950">Integrações operacionais</h2>
                        <p className="mt-1 max-w-3xl text-sm text-slate-500">
                          Base para dropshipping, logística, gateways e marketplaces sem travar os cadastros e vendas principais.
                        </p>
                      </div>
                      <span className="rounded-full bg-amber-50 px-4 py-2 text-sm font-black text-amber-900">Preparado para credenciais reais</span>
                    </div>
                  </div>

                  {activeIntegration ? (
                    <IntegrationWorkspace
                      integracao={integracoes.find((item) => item.slug === activeIntegration)}
                      guide={integrationGuides[activeIntegration]}
                      credenciaisModelo={credenciaisModelo}
                      onBack={() => navigate('/dashboard/integracoes')}
                    />
                  ) : (
                    <div className="grid gap-4 lg:grid-cols-2">
                      {integracoes.map((integracao) => (
                        <IntegrationCard
                          key={integracao.slug || integracao.nome}
                          integracao={integracao}
                          onOpen={() => openIntegration(integracao.slug)}
                        />
                      ))}
                    </div>
                  )}

                  {!activeIntegration && <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                    <h3 className="text-lg font-black text-slate-950">Sequência segura de ativação</h3>
                    <div className="mt-5 grid gap-3 md:grid-cols-4">
                      {[
                        'Cadastrar credenciais reais',
                        'Testar sandbox quando existir',
                        'Validar pedido real pequeno',
                        'Liberar venda assistida',
                      ].map((item, index) => (
                        <div key={item} className="rounded-lg border border-slate-100 bg-slate-50 p-4">
                          <p className="text-2xl font-black text-[#C9A227]">0{index + 1}</p>
                          <p className="mt-2 text-sm font-black text-slate-950">{item}</p>
                        </div>
                      ))}
                    </div>
                  </section>}
                </section>
              )}
            </>
          )}
        </div>
      </section>
    </main>
  );
}

function CadastroWorkspaceHeader({ cadastro, count, highlights = [] }) {
  const Icon = cadastro.icon;

  return (
    <section className="overflow-hidden rounded-lg border border-slate-200 bg-slate-950 text-white shadow-sm">
      <div className="grid gap-0 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="p-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div className="flex items-start gap-4">
              <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-lg bg-[#C9A227] text-black">
                <Icon size={26} />
              </div>
              <div>
                <p className="text-xs font-black uppercase tracking-[0.22em] text-[#C9A227]">Tela dedicada</p>
                <h3 className="mt-2 text-2xl font-black">{cadastro.label}</h3>
                <p className="mt-2 max-w-2xl text-sm font-semibold text-slate-300">{cadastro.detail}</p>
              </div>
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 px-4 py-3 text-right">
              <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-400">Registros</p>
              <p className="mt-1 text-3xl font-black text-[#C9A227]">{count}</p>
            </div>
          </div>
        </div>
        <div className="border-t border-white/10 bg-white/5 p-6 lg:border-l lg:border-t-0">
          <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Proteções desta tela</p>
          <div className="mt-4 space-y-3">
            {highlights.map((item) => (
              <div key={item} className="flex gap-3 text-sm font-semibold text-slate-200">
                <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-[#C9A227]" />
                <span>{item}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}

function CadastroInsightPanel({ title, checks }) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Antes de salvar</p>
      <h3 className="mt-2 text-lg font-black text-slate-950">{title}</h3>
      <div className="mt-5 space-y-3">
        {checks.map((item) => (
          <div key={item} className="flex gap-3 rounded-lg border border-slate-100 bg-slate-50 p-3 text-sm font-bold text-slate-700">
            <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-emerald-500" />
            <span>{item}</span>
          </div>
        ))}
      </div>
    </section>
  );
}

function ErpModuleHero({ eyebrow, title, description }) {
  return (
    <section className="erp-module-card rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">{eyebrow}</p>
      <h2 className="mt-2 text-2xl font-black text-slate-950">{title}</h2>
      <p className="mt-1 max-w-3xl text-sm text-slate-500">{description}</p>
    </section>
  );
}

function ErpChecklistCard({ title, items }) {
  return (
    <section className="erp-module-card rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h3 className="text-lg font-black text-slate-950">{title}</h3>
      <div className="mt-5 space-y-3">
        {items.map((item) => (
          <div key={item} className="flex gap-3 rounded-lg border border-slate-100 bg-slate-50 p-3 text-sm font-bold text-slate-700">
            <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-[#C9A227]" />
            <span>{item}</span>
          </div>
        ))}
      </div>
    </section>
  );
}

function ModuleActionGrid({ title, actions }) {
  return (
    <section className="erp-module-card rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <div className="flex items-center justify-between gap-4">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Operação direta</p>
          <h3 className="mt-2 text-lg font-black text-slate-950">{title}</h3>
        </div>
        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black uppercase tracking-[0.16em] text-slate-600">Acesso rápido</span>
      </div>
      <div className="mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {actions.map((action) => (
          <button
            key={action.label}
            type="button"
            onClick={action.onClick}
            className="group rounded-lg border border-slate-200 bg-slate-50 p-4 text-left transition hover:-translate-y-0.5 hover:border-[#C9A227] hover:bg-white"
          >
            <p className="text-sm font-black text-slate-950">{action.label}</p>
            <p className="mt-2 text-sm text-slate-500">{action.detail}</p>
            <span className="mt-4 inline-flex items-center gap-2 text-sm font-black text-[#C97A00]">
              Abrir agora
              <ChevronRight size={16} />
            </span>
          </button>
        ))}
      </div>
    </section>
  );
}

function ErpWorkspaceNav({ activeTab, onNavigate }) {
  return (
    <section className="erp-module-card rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.22em] text-[#C9A227]">Atalhos internos do ERP</p>
          <p className="mt-1 text-sm font-semibold text-slate-500">Navegação rápida entre os blocos operacionais do desktop corporativo.</p>
        </div>
        <div className="rounded-full border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-black uppercase tracking-[0.16em] text-slate-500">
          {erpWorkspaceNavItems.find((item) => item.id === activeTab)?.signal || 'Operação local'}
        </div>
      </div>
      <div className="mt-4 flex flex-wrap gap-2">
        {erpWorkspaceNavItems.map((item) => {
          const active = activeTab === item.id;
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onNavigate(item.id)}
              className={`inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-black transition ${
                active
                  ? 'bg-slate-950 text-[#C9A227] shadow-[0_14px_30px_rgba(0,0,0,0.16)]'
                  : 'border border-slate-200 bg-white text-slate-700 hover:border-[#C9A227] hover:text-slate-950'
              }`}
            >
              <span className={`rounded-full px-2 py-1 text-[0.62rem] uppercase tracking-[0.18em] ${active ? 'bg-[#C9A227]/15 text-[#C9A227]' : 'bg-slate-100 text-slate-500'}`}>
                {item.short}
              </span>
              {item.label}
            </button>
          );
        })}
      </div>
    </section>
  );
}

function ErpCockpitMenu({ activeTab, onNavigate }) {
  return (
    <aside className="erp-module-card rounded-lg border border-slate-200 bg-white p-4 shadow-sm xl:sticky xl:top-24">
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.22em] text-[#C9A227]">Cockpit técnico</p>
          <p className="mt-1 text-sm font-semibold text-slate-500">Menu interno de operação do ERP/PDV.</p>
        </div>
        <span className="rounded-full bg-[#C9A227]/10 px-3 py-1 text-[0.62rem] font-black uppercase tracking-[0.18em] text-[#C9A227]">
          online local
        </span>
      </div>

      <div className="mt-4 space-y-2">
        {erpCockpitActions.map((item) => {
          const active = activeTab === item.id;
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onNavigate(item.id)}
              className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                active
                  ? 'border-[#C9A227] bg-[#C9A227]/10 shadow-[0_12px_25px_rgba(201,162,39,0.10)]'
                  : 'border-slate-200 bg-slate-50 hover:border-[#C9A227] hover:bg-white'
              }`}
            >
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm font-black text-slate-950">{item.label}</p>
                  <p className="mt-1 text-xs font-semibold text-slate-500">{item.detail}</p>
                </div>
                <span className={`rounded-full px-2 py-1 text-[0.62rem] font-black uppercase tracking-[0.18em] ${active ? 'bg-[#C9A227]/15 text-[#8B6B12]' : 'bg-slate-100 text-slate-500'}`}>
                  abrir
                </span>
              </div>
            </button>
          );
        })}
      </div>

      <div className="mt-4 rounded-xl border border-slate-200 bg-slate-950 p-4 text-white">
        <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Status do motor</p>
        <p className="mt-2 text-sm font-semibold text-slate-300">
          API local apontada para o ambiente de operação, pronta para emissão, caixa e expedição.
        </p>
      </div>
    </aside>
  );
}

function ProductPreview({ produto, categorias }) {
  const categoriaSelecionada = produto.subcategoriaId || produto.categoriaId;
  const categoria = categorias.find((item) => item.id === categoriaSelecionada)?.caminho
    || categorias.find((item) => item.id === categoriaSelecionada)?.nome
    || categorias.find((item) => item.id === produto.categoriaId)?.nome
    || 'Sem categoria';
  const image = produto.imagemUrl || '';
  const price = Number(produto.preco || 0);
  const promotional = Number(produto.precoPromocional || 0);

  return (
    <section className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="aspect-[16/10] bg-slate-100">
        {image ? (
          <img src={image} alt={produto.nome || 'Prévia do produto'} className="h-full w-full object-cover" />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-slate-100 to-slate-200">
            <p className="text-xs font-black uppercase tracking-[0.24em] text-slate-400">Imagem não cadastrada</p>
          </div>
        )}
      </div>
      <div className="p-5">
        <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Prévia da vitrine</p>
        <h3 className="mt-2 text-xl font-black text-slate-950">{produto.nome || 'Nome do produto'}</h3>
        <p className="mt-2 line-clamp-3 text-sm font-semibold text-slate-500">{produto.descricao || 'Descrição comercial do produto aparecerá aqui para revisão rápida.'}</p>
        <div className="mt-4 flex flex-wrap gap-2">
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-700">{categoria}</span>
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-700">SKU {produto.sku || '-'}</span>
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-700">Estoque {produto.estoque || 0}</span>
        </div>
        <div className="mt-5 flex items-end gap-3">
          {promotional > 0 && <p className="text-sm font-bold text-slate-400 line-through">{formatPrice(price)}</p>}
          <p className="text-3xl font-black text-slate-950">{formatPrice(promotional || price)}</p>
        </div>
      </div>
    </section>
  );
}

function ImageUrlField({ value, onChange, onUpload, uploading, className = '' }) {
  return (
    <div className={`rounded-lg border border-slate-200 bg-slate-50 p-4 ${className}`}>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end">
        <Field
          label="Imagem principal URL"
          value={value}
          onChange={onChange}
          className="flex-1"
        />
        <label className="inline-flex h-11 cursor-pointer items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-4 text-sm font-black text-slate-800 hover:border-[#C9A227]">
          <ImagePlus size={17} />
          {uploading ? 'Enviando...' : 'Enviar imagem'}
          <input
            type="file"
            accept="image/png,image/jpeg,image/webp,image/gif"
            className="hidden"
            disabled={uploading}
            onChange={(event) => onUpload(event.target.files?.[0])}
          />
        </label>
      </div>
      <p className="mt-3 text-xs font-semibold text-slate-500">
        Use uma URL pública HTTPS ou envie uma imagem de até 2MB. O sistema salva a imagem na API e grava o link no produto.
      </p>
    </div>
  );
}

function IntegrationCard({ integracao, onOpen }) {
  const iconMap = {
    ecommerce: ShoppingBag,
    dropshipping: Handshake,
    shopify: Handshake,
    cjdropshipping: Handshake,
    logistica: Truck,
    melhorenvio: Truck,
    gateways: WalletCards,
    mercadopago: WalletCards,
    marketplaces: Database,
    mercadolivre: Database,
    bancaria: CreditCard,
  };
  const Icon = iconMap[integracao.slug] || Globe2;
  const configured = integracao.configurada ?? integracao.Configurada;

  return (
    <button type="button" onClick={onOpen} className="rounded-lg border border-slate-200 bg-white p-6 text-left shadow-sm transition hover:border-[#C9A227] hover:shadow-md">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-start gap-4">
          <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-slate-950 text-[#C9A227]">
            <Icon size={23} />
          </div>
          <div>
            <h3 className="text-lg font-black text-slate-950">{integracao.nome}</h3>
            <p className="mt-2 text-sm font-semibold text-slate-500">{integracao.detalhe}</p>
          </div>
        </div>
        <span className={`rounded-full px-3 py-1 text-xs font-black uppercase tracking-wide ${configured ? 'bg-emerald-50 text-emerald-800' : 'bg-amber-50 text-amber-900'}`}>
          {integracao.status}
        </span>
      </div>
      <div className="mt-5 flex items-center justify-between border-t border-slate-100 pt-4">
        <span className="text-xs font-black uppercase tracking-[0.14em] text-slate-400">{integracao.ambiente || integracao.Ambiente || 'Não configurado'}</span>
        <span className="inline-flex items-center gap-1 text-sm font-black text-slate-950">Abrir módulo <ChevronRight size={16} /></span>
      </div>
    </button>
  );
}

function IntegrationWorkspace({ integracao, guide, credenciaisModelo = [], onBack }) {
  const [testing, setTesting] = useState(false);
  const [testResult, setTestResult] = useState(null);

  if (!integracao || !guide) {
    return (
      <section className="rounded-lg border border-amber-200 bg-amber-50 p-6 text-amber-950">
        <p className="font-black">Módulo de integração não encontrado.</p>
        <button type="button" onClick={onBack} className="mt-4 font-black underline">Voltar para integrações</button>
      </section>
    );
  }

  const configured = integracao.configurada ?? integracao.Configurada;
  const operational = integracao.operacional ?? integracao.Operacional ?? false;
  const ambiente = integracao.ambiente || integracao.Ambiente || 'Não configurado';
  const pendencias = testResult?.pendencias || integracao.pendencias || [];
  const referencia = testResult?.referencia || integracao.referencia;
  const detalhe = testResult?.detalhe || integracao.detalhe;
  const status = testResult?.status || integracao.status;
  const credentialCategories = integrationCredentialCategoryMap[integracao.slug] || [];
  const relevantCredentials = credenciaisModelo.filter((item) => credentialCategories.includes((item.categoria || item.Categoria || '').toLowerCase()));

  const runTest = async () => {
    setTesting(true);
    setTestResult(null);
    try {
      const response = await integracoesAPI.testar(integracao.slug);
      setTestResult(response.data);
    } catch (error) {
      setTestResult({
        status: 'Erro no teste',
        operacional: false,
        detalhe: error.response?.data?.detail || error.message || 'Não foi possível testar esta integração.',
        pendencias: ['Confira se a API pública está respondendo e tente novamente.'],
      });
    } finally {
      setTesting(false);
    }
  };

  return (
    <section className="space-y-6">
      <button type="button" onClick={onBack} className="inline-flex items-center gap-2 text-sm font-black text-slate-700">
        ← Voltar para integrações
      </button>
      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_minmax(320px,0.8fr)]">
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Módulo operacional</p>
              <h3 className="mt-2 text-3xl font-black text-slate-950">{integracao.nome}</h3>
              <p className="mt-3 max-w-3xl text-sm font-semibold leading-6 text-slate-500">{guide.description}</p>
            </div>
            <div className="flex flex-col items-start gap-3 sm:items-end">
              <span className={`rounded-full px-4 py-2 text-sm font-black ${operational ? 'bg-emerald-50 text-emerald-800' : configured ? 'bg-blue-50 text-blue-800' : 'bg-amber-50 text-amber-900'}`}>
                {status}
              </span>
              <button
                type="button"
                onClick={runTest}
                disabled={testing}
                className="rounded-lg bg-slate-950 px-4 py-2 text-sm font-black text-white transition hover:bg-[#C9A227] hover:text-slate-950 disabled:cursor-wait disabled:opacity-60"
              >
                {testing ? 'Testando...' : 'Testar conexão real'}
              </button>
            </div>
          </div>
          <div className="mt-6 rounded-lg border border-slate-100 bg-slate-50 p-5">
            <p className="text-xs font-black uppercase tracking-[0.16em] text-slate-400">Leitura real da API</p>
            <p className="mt-2 font-bold text-slate-800">{detalhe}</p>
            {referencia && <p className="mt-2 text-xs font-black uppercase tracking-[0.12em] text-slate-400">Referência: {referencia}</p>}
          </div>
          {pendencias.length > 0 && (
            <div className="mt-6 rounded-lg border border-amber-200 bg-amber-50 p-5">
              <h4 className="font-black text-amber-950">Pendências para ficar 100%</h4>
              <div className="mt-3 space-y-2">
                {pendencias.map((pendencia) => (
                  <p key={pendencia} className="text-sm font-bold text-amber-900">• {pendencia}</p>
                ))}
              </div>
            </div>
          )}
          <div className="mt-6">
            <h4 className="font-black text-slate-950">Requisitos para ativação</h4>
            <div className="mt-3 space-y-3">
              {guide.requirements.map((requirement) => (
                <div key={requirement} className="flex items-center gap-3 rounded-lg border border-slate-100 p-4">
                  <div className={`h-3 w-3 rounded-full ${configured ? 'bg-emerald-500' : 'bg-amber-400'}`} />
                  <span className="text-sm font-bold text-slate-700">{requirement}</span>
                </div>
              ))}
            </div>
          </div>
          {relevantCredentials.length > 0 && (
            <div className="mt-6">
              <h4 className="font-black text-slate-950">Credenciais já preparadas no sistema</h4>
              <div className="mt-3 grid gap-3">
                {relevantCredentials.map((credencial) => {
                  const provider = credencial.provedor || credencial.Provedor;
                  const key = credencial.chave || credencial.Chave;
                  const usage = credencial.uso || credencial.Uso;
                  const required = credencial.obrigatoria ?? credencial.Obrigatoria;

                  return (
                    <div key={`${provider}-${key}`} className="rounded-lg border border-slate-100 bg-slate-50 p-4">
                      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                        <div>
                          <p className="text-sm font-black text-slate-950">{provider}</p>
                          <p className="mt-1 break-all text-xs font-black uppercase tracking-[0.12em] text-slate-500">{key}</p>
                          <p className="mt-2 text-sm font-semibold leading-6 text-slate-600">{usage}</p>
                        </div>
                        <span className={`rounded-full px-3 py-1 text-xs font-black uppercase tracking-wide ${required ? 'bg-red-50 text-red-800' : 'bg-blue-50 text-blue-800'}`}>
                          {required ? 'Obrigatória' : 'Estrutural'}
                        </span>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
        <aside className="space-y-4">
          <div className="rounded-lg border border-slate-200 bg-slate-950 p-6 text-white shadow-sm">
            <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Ambiente</p>
            <p className="mt-2 text-2xl font-black">{ambiente}</p>
          </div>
          <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
            <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-400">Próximo teste controlado</p>
            <p className="mt-3 text-sm font-bold leading-6 text-slate-800">{guide.nextTest}</p>
          </div>
          <div className="rounded-lg border border-amber-200 bg-amber-50 p-5 text-sm font-bold leading-6 text-amber-950">
            Credenciais nunca são gravadas no navegador. Elas devem permanecer no servidor e fora do GitHub.
          </div>
        </aside>
      </div>
    </section>
  );
}

function Field({ label, value, onChange, type = 'text', required = false, className = '' }) {
  return (
    <label className={`block text-sm font-bold text-slate-700 ${className}`}>
      {label}
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        required={required}
        min={type === 'number' ? '0' : undefined}
        step={type === 'number' ? '0.01' : undefined}
        className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
      />
    </label>
  );
}

function DuplicateAlert({ message }) {
  return (
    <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm font-black text-amber-900">
      {message}
    </div>
  );
}

function SimpleForm({ title, subtitle, onSubmit, buttonLabel, children, alertMessage = '', disabled = false }) {
  return (
    <form onSubmit={onSubmit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h3 className="text-lg font-black text-slate-950">{title}</h3>
      {subtitle && <p className="mt-1 text-sm font-semibold text-slate-500">{subtitle}</p>}
      {alertMessage && <DuplicateAlert message={alertMessage} />}
      <div className="mt-5 grid gap-4">{children}</div>
      <button disabled={disabled} className="mt-5 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:cursor-not-allowed disabled:bg-slate-300">
        <Save size={17} />
        {buttonLabel}
      </button>
    </form>
  );
}

function getCompactFieldConfig(field) {
  return typeof field === 'string' ? { key: field, label: field, type: 'text' } : field;
}

function formatCompactValue(value, type) {
  if (value === null || value === undefined || value === '') return '-';
  if (type === 'money') return formatPrice(Number(value) || 0);
  if (type === 'date') return formatDate(value);
  return String(value);
}

function CompactList({ title, items, fields, onEdit, renderActions }) {
  const primaryField = getCompactFieldConfig(fields[0]);
  const detailFields = fields.slice(1).map(getCompactFieldConfig);

  return (
    <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-4 py-4 sm:px-5">
        <h3 className="text-base font-black text-slate-950 sm:text-lg">{title}</h3>
        <p className="mt-1 text-xs text-slate-500 sm:text-sm">{items.length} registros</p>
      </div>
      <div className="max-h-72 divide-y divide-slate-100 overflow-auto sm:max-h-96">
        {items.slice(0, 12).map((item, index) => (
          <div key={item.id || item.produtoId || item.numero || item.sku || item.email || index} className="px-3 py-2 sm:px-5 sm:py-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="text-xs font-black text-slate-950 sm:text-base">{formatCompactValue(item[primaryField.key], primaryField.type)}</p>
                <p className="mt-1 text-[11px] font-semibold leading-4 text-slate-500 sm:text-sm">
                  {detailFields.map((field) => `${field.label}: ${formatCompactValue(item[field.key], field.type)}`).join(' · ')}
                </p>
              </div>
              <div className="flex shrink-0 flex-wrap justify-end gap-2">
                {onEdit && (
                  <button
                    type="button"
                    onClick={() => onEdit(item)}
                    className="inline-flex items-center gap-1 rounded-full border border-slate-200 bg-slate-50 px-3 py-1 text-[11px] font-black uppercase tracking-[0.12em] text-slate-700 hover:border-[#C9A227] hover:text-[#8E6A12]"
                  >
                    Editar
                  </button>
                )}
                {renderActions?.(item)}
              </div>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

function OrdersTable({ pedidos, onStatusChange, onLogisticaChange }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[920px]">
        <thead className="bg-slate-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Pedido</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Total</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Status</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Pagamento / Frete</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Data</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Ação</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {pedidos.length === 0 ? (
            <tr><td colSpan="6" className="px-6 py-8 text-center text-slate-500">Nenhum pedido encontrado</td></tr>
          ) : (
            pedidos.map((pedido) => (
              <tr key={pedido.id} className="hover:bg-slate-50" data-testid={`pedido-${pedido.id}`}>
                <td className="px-6 py-4 font-mono text-sm font-black text-slate-950">{pedido.numero_pedido}</td>
                <td className="px-6 py-4 font-black text-slate-950">{formatPrice(pedido.total)}</td>
                <td className="px-6 py-4">
                  <span className={`rounded-full px-3 py-1 text-xs font-black ${getPedidoStatusClass(pedido.status)}`}>
                    {pedido.status}
                  </span>
                </td>
                <td className="px-6 py-4 text-sm">
                  <p className="font-black text-slate-950">{pedido.status_pagamento || 'Aguardando pagamento'}</p>
                  <p className="mt-1 font-semibold text-slate-500">
                    {getPagamentoLabel(pedido.meio_pagamento || '')} · {pedido.gateway_pagamento || 'Gateway pendente'}
                  </p>
                  <p className="mt-1 text-xs font-semibold text-slate-400">
                    {pedido.frete_metodo || 'Frete a combinar'} · {pedido.frete_transportadora || 'Nexum Altivon'}
                  </p>
                  <p className="mt-1 text-xs font-black text-slate-500">
                    Rastreio: {pedido.frete_codigo_rastreio || 'pendente'}
                  </p>
                </td>
                <td className="px-6 py-4 text-sm font-semibold text-slate-500">{formatDate(pedido.created_at)}</td>
                <td className="px-6 py-4">
                  <select
                    value={pedido.status}
                    onChange={(event) => onStatusChange?.(pedido.id, event.target.value)}
                    className="h-9 rounded-lg border border-slate-200 bg-white px-3 text-xs font-black text-slate-700 outline-none focus:border-slate-950"
                  >
                    {pedidoStatusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
                  </select>
                  <button
                    type="button"
                    onClick={() => onLogisticaChange?.(pedido)}
                    className="mt-2 h-9 rounded-lg border border-slate-200 bg-slate-950 px-3 text-xs font-black text-white transition hover:bg-[#C9A227] hover:text-black"
                  >
                    Logistica
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

function LeadsTable({ leads, onStatusChange }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[820px]">
        <thead className="bg-slate-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Lead</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Email</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Telefone</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Status</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Entrada</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Ação</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {leads.length === 0 ? (
            <tr><td colSpan="6" className="px-6 py-8 text-center text-slate-500">Nenhum lead encontrado</td></tr>
          ) : (
            leads.map((lead) => (
              <tr key={lead.id} className="hover:bg-slate-50" data-testid={`lead-${lead.id}`}>
                <td className="px-6 py-4 font-black text-slate-950">{lead.nome}</td>
                <td className="px-6 py-4 text-sm font-semibold text-slate-600">{lead.email}</td>
                <td className="px-6 py-4 text-sm font-semibold text-slate-600">{lead.telefone || '-'}</td>
                <td className="px-6 py-4">
                  <span className={`rounded-full px-3 py-1 text-xs font-black ${getLeadStatusClass(lead.status)}`}>
                    {lead.status}
                  </span>
                </td>
                <td className="px-6 py-4 text-sm font-semibold text-slate-500">{formatDate(lead.created_at)}</td>
                <td className="px-6 py-4">
                  <select
                    value={lead.status}
                    onChange={(event) => onStatusChange?.(lead.id, event.target.value)}
                    className="h-9 rounded-lg border border-slate-200 bg-white px-3 text-xs font-black text-slate-700 outline-none focus:border-slate-950"
                  >
                    {leadStatusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
                  </select>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

function ImageGalleryField({ value, onChange, onUpload, uploading, className = '' }) {
  const items = galleryToArray(value);

  const removeItem = (targetUrl) => {
    onChange(galleryToText(items.filter((item) => item !== targetUrl)));
  };

  return (
    <div className={`rounded-lg border border-slate-200 bg-slate-50 p-4 ${className}`}>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div className="flex-1">
          <p className="text-sm font-bold text-slate-700">Galeria de imagens</p>
          <p className="mt-2 text-xs font-semibold text-slate-500">
            Você pode colar URLs ou enviar várias imagens de uma vez. O sistema monta a galeria automaticamente.
          </p>
        </div>
        <label className="inline-flex h-11 cursor-pointer items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-4 text-sm font-black text-slate-800 hover:border-[#C9A227]">
          <ImagePlus size={17} />
          {uploading ? 'Enviando galeria...' : 'Adicionar imagens'}
          <input
            type="file"
            accept="image/png,image/jpeg,image/webp,image/gif"
            multiple
            className="hidden"
            disabled={uploading}
            onChange={(event) => onUpload(event.target.files)}
          />
        </label>
      </div>

      <textarea
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-4 min-h-24 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
        placeholder="Uma URL por linha"
      />

      {items.length > 0 && (
        <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {items.map((item) => (
            <div key={item} className="overflow-hidden rounded-lg border border-slate-200 bg-white">
              <div className="aspect-square bg-slate-100">
                <img src={item} alt="Galeria do produto" className="h-full w-full object-cover" />
              </div>
              <div className="flex items-center justify-between gap-2 px-3 py-2">
                <p className="truncate text-xs font-semibold text-slate-500">{item}</p>
                <button
                  type="button"
                  onClick={() => removeItem(item)}
                  className="inline-flex h-8 w-8 items-center justify-center rounded-full text-slate-500 transition hover:bg-rose-50 hover:text-rose-600"
                  title="Remover imagem"
                >
                  <Trash2 size={15} />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function SelectField({ label, value, onChange, options, className = '' }) {
  return (
    <label className={`block text-sm font-bold text-slate-700 ${className}`}>
      {label}
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
      >
        {options.map((option) => (
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
    </label>
  );
}

function OptionSelectField({ label, value, onChange, options, placeholder = 'Selecione', className = '' }) {
  return (
    <label className={`block text-sm font-bold text-slate-700 ${className}`}>
      {label}
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>{option.label}</option>
        ))}
      </select>
    </label>
  );
}

function TextAreaField({ label, value, onChange, rows = 4, className = '' }) {
  return (
    <label className={`block text-sm font-bold text-slate-700 ${className}`}>
      {label}
      <textarea
        value={value}
        rows={rows}
        onChange={(event) => onChange(event.target.value)}
        className="mt-2 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
      />
    </label>
  );
}

function ToggleField({ label, checked, onChange, className = '' }) {
  return (
    <label className={`flex items-center justify-between gap-4 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 ${className}`}>
      <span className="text-sm font-black text-slate-700">{label}</span>
      <button
        type="button"
        onClick={() => onChange(!checked)}
        className={`inline-flex h-7 w-14 items-center rounded-full px-1 transition ${checked ? 'bg-emerald-500' : 'bg-slate-300'}`}
      >
        <span className={`h-5 w-5 rounded-full bg-white shadow transition ${checked ? 'translate-x-7' : 'translate-x-0'}`} />
      </button>
    </label>
  );
}

