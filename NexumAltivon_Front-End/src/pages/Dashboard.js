/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */
import { useEffect, useMemo, useState, useCallback } from 'react';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { categoriaAPI, clienteAPI, dashboardAPI, empresaGrupoAPI, fiscalAPI, fornecedorAPI, integracoesAPI, leadAPI, pedidoAPI, produtoAPI, siteAPI } from '../services/api';
import { formatDate, formatPrice, getLeadStatusClass, getPagamentoLabel, getPedidoStatusClass } from '../utils/formatters';
import CupomAdminPanel from '../components/CupomAdminPanel';
import AccessAuditPanel from '../components/AccessAuditPanel';
import MarketingAdminPanel from '../components/MarketingAdminPanel';
import DropshippingAdminPanel from '../components/DropshippingAdminPanel';
import SiteMediaLibrary from '../components/SiteMediaLibrary';
import SitePublicProfilesEditor from '../components/SitePublicProfilesEditor';
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import {
  Activity,
  Bell,
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
  Megaphone,
  LogOut,
  PackageCheck,
  Trash2,
  Save,
  Search,
  ShoppingBag,
  Sparkles,
  Truck,
  TrendingUp,
  TicketPercent,
  UserRound,
  UserCog,
  Users,
  WalletCards,
  FileSearch,
} from 'lucide-react';

const tabs = [
  { id: 'overview', label: 'Dashboard', icon: LayoutDashboard, section: 'Principal' },
  { id: 'pedidos', label: 'Pedidos', icon: ShoppingBag, section: 'Principal' },
  { id: 'crm', label: 'CRM', icon: Users, section: 'Marketing & CRM' },
  { id: 'marketing', label: 'Marketing', icon: Megaphone, section: 'Marketing & CRM' },
  { id: 'cupons', label: 'Cupons', icon: TicketPercent, section: 'Marketing & CRM' },
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
  { id: 'usuarios', label: 'Usuários', icon: UserCog, section: 'Sistema' },
  { id: 'auditoria', label: 'Auditoria', icon: FileSearch, section: 'Sistema' },
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

const integrationGuides = {
  ecommerce: {
    description: 'Operação central de catálogo, clientes, pedidos, estoque e painel administrativo.',
    requirements: ['API respondendo continuamente', 'Banco de dados conectado', 'Domínio público estável'],
    nextTest: 'Criar um pedido controlado e conferir cliente, pedido e reserva de estoque.',
  },
  dropshipping: {
    description: 'Roteamento de produtos e pedidos para fornecedores parceiros.',
    requirements: ['Fornecedor ativo', 'Produto vinculado ao fornecedor', 'Regra de custo, prazo e comissão'],
    nextTest: 'Vincular um produto real a um fornecedor e executar o roteamento do pedido.',
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
const emptyFiscalManualForm = {
  empresaEmissora: '',
  cnpjEmissor: '',
  clienteDestinatario: '',
  documentoDestinatario: '',
  naturezaOperacao: 'Venda de mercadoria',
  cfop: '5102',
  subtotal: '0',
  frete: '0',
  impostosEstimados: '0',
  margemMinima: '10',
  observacoes: '',
  tipoOperacao: 'VendaInterna',
  estadoOrigem: 'SP',
  estadoDestino: 'SP',
  categoriaFiscal: '',
  subcategoriaFiscal: '',
  exigeMarketplace: false,
  exigeDropshipping: false,
  requerSaidaNfe: true,
  requerEntradaNfe: false,
};
const emptySiteConfigForm = {
  site_nome: 'Grupo Nexum Altivon',
  site_url: 'https://www.nexumaltivon.com.br',
  site_logo: '/assets/logo-2.jpg',
  site_email_contato: 'corporativo.gna@gmail.com',
  site_telefone: '(14) 99673-1879',
  site_telefone_secundario: '(14) 99673-1879',
  site_whatsapp: '5514996731879',
  site_whatsapp_secundario: '5514996731879',
  site_yara_email: 'corporativo.gna@gmail.com',
  site_sophia_email: 'corporativo.gna@gmail.com',
  site_cor_primaria: '#C9A227',
  site_cor_secundaria: '#0A0A0A',
  site_cor_fundo: '#F5F7FB',
  site_cor_superficie: '#FFFFFF',
  site_cor_texto: '#0F172A',
  site_cor_texto_suave: '#64748B',
  home_intro_titulo: 'Uma Nova Era Começa',
  home_intro_texto_1: 'A Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.',
  home_intro_texto_2: 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.',
  home_intro_badge: 'www.nexumaltivon.com.br',
  home_footer_texto: 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.',
  home_hero_interval_seconds: '7',
  site_partner_rotation_seconds: '8',
  home_quality_items: '["Curadoria rigorosa de fornecedores","Atendimento humano e especializado","Política de devolução simplificada","Preços justos e acessíveis"]',
  home_hero_slides: '[{"id":"ecommerce","badge":"Grupo Nexum Altivon","title":"O Futuro do","highlight":"E-Commerce","description":"Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.","image":"/imagens/homepage/banner-ecommerce.svg","imageAlt":"Operação integrada do Grupo Nexum Altivon","active":true,"order":0}]',
};
const siteConfigJsonFields = new Set(['home_quality_items', 'home_hero_slides']);
const siteConfigFieldMeta = [
  { key: 'site_nome', label: 'Nome do site', type: 'text', group: 'Geral', description: 'Nome público principal da operação.' },
  { key: 'site_url', label: 'URL pública', type: 'text', group: 'Geral', description: 'Endereço principal exibido nos links institucionais.' },
  { key: 'site_logo', label: 'Logo do site', type: 'text', group: 'Geral', description: 'URL ou caminho do logo exibido na home e no rodapé.' },
  { key: 'site_email_contato', label: 'E-mail público', type: 'text', group: 'Geral', description: 'Destino principal das mensagens do site.' },
  { key: 'site_telefone', label: 'Telefone Rodrigo', type: 'text', group: 'Geral', description: 'Contato principal exibido na home.' },
  { key: 'site_telefone_secundario', label: 'Telefone secundário', type: 'text', group: 'Geral', description: 'Contato secundário exibido na home.' },
  { key: 'site_whatsapp', label: 'WhatsApp principal', type: 'text', group: 'Geral', description: 'Número usado em links do site.' },
  { key: 'site_whatsapp_secundario', label: 'WhatsApp secundário', type: 'text', group: 'Geral', description: 'Número usado em parceria/fornecedores.' },
  { key: 'site_yara_email', label: 'E-mail da Yara', type: 'text', group: 'Atendimento', description: 'Canal atual da Yara para atendimento comercial.' },
  { key: 'site_sophia_email', label: 'E-mail da Sophia', type: 'text', group: 'Atendimento', description: 'Canal da Sophia para apoio administrativo interno.' },
  { key: 'site_cor_primaria', label: 'Cor principal', type: 'color', group: 'Aparencia', description: 'Ações, links e destaques do portal.' },
  { key: 'site_cor_secundaria', label: 'Cor secundária', type: 'color', group: 'Aparencia', description: 'Cabeçalho, navegação e áreas de contraste.' },
  { key: 'site_cor_fundo', label: 'Cor de fundo', type: 'color', group: 'Aparencia', description: 'Fundo geral das páginas públicas.' },
  { key: 'site_cor_superficie', label: 'Cor de superfície', type: 'color', group: 'Aparencia', description: 'Cards, painéis e blocos de conteúdo.' },
  { key: 'site_cor_texto', label: 'Cor do texto', type: 'color', group: 'Aparencia', description: 'Textos principais das páginas públicas.' },
  { key: 'site_cor_texto_suave', label: 'Cor do texto secundário', type: 'color', group: 'Aparencia', description: 'Descrições e informações auxiliares.' },
  { key: 'home_intro_titulo', label: 'Título institucional', type: 'text', group: 'SiteHome', description: 'Título principal do bloco institucional.' },
  { key: 'home_intro_texto_1', label: 'Texto institucional 1', type: 'textarea', group: 'SiteHome', description: 'Primeiro texto institucional da home.' },
  { key: 'home_intro_texto_2', label: 'Texto institucional 2', type: 'textarea', group: 'SiteHome', description: 'Segundo texto institucional da home.' },
  { key: 'home_intro_badge', label: 'Selo institucional', type: 'text', group: 'SiteHome', description: 'Texto do selo abaixo do bloco institucional.' },
  { key: 'home_footer_texto', label: 'Rodapé público', type: 'textarea', group: 'SiteHome', description: 'Mensagem institucional no rodapé da home.' },
  { key: 'home_hero_interval_seconds', label: 'Troca automática dos banners', type: 'number', min: 3, max: 30, group: 'SiteHome', description: 'Intervalo entre 3 e 30 segundos para avançar os slides ativos.' },
  { key: 'site_partner_rotation_seconds', label: 'Rodízio de parceiros', type: 'number', min: 5, max: 60, group: 'SiteHome', description: 'Intervalo entre 5 e 60 segundos para alternar grupos de até quatro parceiros.' },
  { key: 'home_quality_items', label: 'Itens de qualidade (JSON)', type: 'textarea', group: 'SiteHome', description: 'Array JSON de frases do bloco de qualidade.' },
  { key: 'home_hero_slides', label: 'Slides do banner', type: 'managed', group: 'SiteHome', description: 'Slides administrados pelo editor visual e persistidos como dados estruturados.' },
];
const siteConfigFieldByKey = Object.fromEntries(siteConfigFieldMeta.map((field) => [field.key, field]));
const siteConfigSections = [
  {
    id: 'identidade',
    title: 'Identidade e contatos',
    description: 'Marca principal, endereço público, logo e canais de atendimento que aparecem na operação.',
    fields: ['site_nome', 'site_url', 'site_logo', 'site_email_contato', 'site_telefone', 'site_telefone_secundario', 'site_whatsapp', 'site_whatsapp_secundario', 'site_yara_email', 'site_sophia_email'],
  },
  {
    id: 'aparencia',
    title: 'Cores do portal',
    description: 'Paleta global aplicada ao frontend público e compartilhada com o controle administrativo do Desktop.',
    fields: ['site_cor_primaria', 'site_cor_secundaria', 'site_cor_fundo', 'site_cor_superficie', 'site_cor_texto', 'site_cor_texto_suave'],
  },
  {
    id: 'home',
    title: 'Textos institucionais da home',
    description: 'Textos do banner institucional, do bloco de apresentação e do rodapé público.',
    fields: ['home_intro_titulo', 'home_intro_badge', 'home_intro_texto_1', 'home_intro_texto_2', 'home_footer_texto', 'home_hero_interval_seconds', 'site_partner_rotation_seconds'],
  },
  {
    id: 'home-complementos',
    title: 'Blocos complementares',
    description: 'Conteúdo estruturado dos diferenciais exibidos abaixo do banner.',
    fields: ['home_quality_items'],
  },
];
const pedidoStatusOptions = ['Pendente', 'Processando', 'Enviado', 'Entregue', 'Cancelado'];
const leadStatusOptions = ['Novo', 'Contato', 'Qualificado', 'Negociacao', 'Ganho', 'Perdido'];
const emptyResumo = {
  pedidos_hoje: 0,
  total_clientes: 0,
  faturamento_mes: 0,
  leads_novos: 0,
  produtos_estoque_baixo: 0,
  conversao: 0,
  ticket_medio: 0,
  periodo_dias: 7,
  periodo_inicio: null,
  periodo_fim: null,
  atualizado_em: null,
  vendas_periodo: [],
};
const salesPeriodOptions = [7, 30, 90];
const salesDateFormatter = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit' });
const salesWeekdayFormatter = new Intl.DateTimeFormat('pt-BR', { weekday: 'short' });

const normalizeText = (value) => String(value ?? '').trim().toLowerCase();
const normalizeDocument = (value) => String(value ?? '').replace(/\D/g, '');
const getApiErrorMessage = (error, defaultMessage) =>
  error.response?.data?.detail ||
  error.response?.data?.mensagem ||
  error.message ||
  defaultMessage;

const safeJsonParse = (value, defaultValue) => {
  try {
    return JSON.parse(String(value ?? ''));
  } catch {
    return defaultValue;
  }
};
const formatSiteConfigJsonForForm = (value, defaultValue) => {
  if (typeof value !== 'string' || !value.trim()) {
    return defaultValue;
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
};
const getJsonArrayCount = (value) => {
  try {
    const parsed = JSON.parse(String(value ?? '[]'));
    return Array.isArray(parsed) ? parsed.length : 0;
  } catch {
    return 0;
  }
};
const normalizeSiteConfigValue = (key, value) => {
  const rawValue = String(value ?? '').trim();

  if (!siteConfigJsonFields.has(key)) {
    return value;
  }

  if (!rawValue) {
    return '[]';
  }

  const parsed = JSON.parse(rawValue);
  return JSON.stringify(parsed);
};
const buildSiteConfigFormFromItems = (items, baseForm = emptySiteConfigForm) =>
  items.reduce((acc, item) => {
    if (!item?.chave) return acc;

    return {
      ...acc,
      [item.chave]: siteConfigJsonFields.has(item.chave)
        ? formatSiteConfigJsonForForm(item.valor, emptySiteConfigForm[item.chave] || '[]')
        : item.valor ?? '',
    };
  }, { ...baseForm });
const getFiscalDraftSavedAt = (draft) =>
  draft?.updatedAt || draft?.UpdatedAt || draft?.salvoEm || draft?.SalvoEm || draft?.salvo_em || '';
const galleryToArray = (value) =>
  String(value ?? '')
    .split(/\r?\n/)
    .map((item) => item.trim())
    .filter(Boolean);
const galleryToText = (items) => items.filter(Boolean).join('\n');

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

const fileToDataUrl = (file) =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = () => reject(new Error('Falha ao ler imagem.'));
    reader.readAsDataURL(file);
  });

const getProdutoDuplicateMessage = (form, produtos) => {
  const sku = normalizeText(form.sku);
  const id = normalizeText(form.id);
  const nome = normalizeText(form.nome);

  if (sku) {
    const duplicate = produtos.find((produto) => normalizeText(produto.sku) === sku);
    if (duplicate) return `SKU já usado em ${duplicate.nome || 'outro produto'}.`;
  }

  if (id) {
    const duplicate = produtos.find((produto) => normalizeText(produto.id) === id);
    if (duplicate) return `Identificador público já usado em ${duplicate.nome || 'outro produto'}.`;
  }

  if (nome) {
    const duplicate = produtos.find((produto) => normalizeText(produto.nome) === nome);
    if (duplicate) return 'Produto com este nome já existe no catálogo.';
  }

  return '';
};

const getClienteDuplicateMessage = (form, clientes) => {
  const email = normalizeText(form.email);
  const documento = normalizeDocument(form.cpf);

  if (email) {
    const duplicate = clientes.find((cliente) => normalizeText(cliente.email) === email);
    if (duplicate) return `Cliente já cadastrado com este e-mail: ${duplicate.nome || duplicate.email}.`;
  }

  if (documento) {
    const duplicate = clientes.find((cliente) => normalizeDocument(cliente.cpf) === documento || normalizeDocument(cliente.cpfCnpj) === documento);
    if (duplicate) return `Cliente já cadastrado com este CPF/CNPJ: ${duplicate.nome || duplicate.email}.`;
  }

  return '';
};

const getFornecedorDuplicateMessage = (form, fornecedores) => {
  const documento = normalizeDocument(form.documento);
  const email = normalizeText(form.email);
  const nome = normalizeText(form.nome);

  if (documento) {
    const duplicate = fornecedores.find((fornecedor) => normalizeDocument(fornecedor.documento) === documento || normalizeDocument(fornecedor.cnpj) === documento);
    if (duplicate) return `Fornecedor já cadastrado com este documento: ${duplicate.nome || duplicate.email}.`;
  }

  if (email) {
    const duplicate = fornecedores.find((fornecedor) => normalizeText(fornecedor.email) === email);
    if (duplicate) return `Fornecedor já cadastrado com este e-mail: ${duplicate.nome || duplicate.email}.`;
  }

  if (nome) {
    const duplicate = fornecedores.find((fornecedor) => normalizeText(fornecedor.nome) === nome);
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

function StatCard({ title, value, detail, icon: Icon, tone = 'slate' }) {
  const toneClass = {
    slate: 'bg-slate-950 text-white',
    amber: 'bg-amber-400 text-slate-950',
    emerald: 'bg-emerald-600 text-white',
    indigo: 'bg-indigo-600 text-white',
  }[tone];

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm" data-testid={`dashboard-stat-${title}`}>
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">{title}</p>
          <p className="mt-3 text-3xl font-black text-slate-950">{value}</p>
        </div>
        <div className={`flex h-11 w-11 items-center justify-center rounded-lg ${toneClass}`}>
          <Icon size={22} />
        </div>
      </div>
      <p className="mt-5 text-sm font-bold text-slate-500">{detail}</p>
    </div>
  );
}

function StatMiniCard({ label, value }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-xs font-black uppercase tracking-[0.16em] text-slate-500">{label}</p>
      <p className="mt-3 text-3xl font-black text-slate-950">{value}</p>
    </div>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();
  const params = useParams();
  const { user, isAuthenticated, loading: authLoading, logout } = useAuth();
  const routeState = useMemo(() => getDashboardRouteState(params['*']), [params]);
  const [resumo, setResumo] = useState(emptyResumo);
  const [pedidos, setPedidos] = useState([]);
  const [leads, setLeads] = useState([]);
  const [produtos, setProdutos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [fornecedores, setFornecedores] = useState([]);
  const [empresasGrupo, setEmpresasGrupo] = useState([]);
  const [fiscalPedidos, setFiscalPedidos] = useState([]);
  const [integracoes, setIntegracoes] = useState([]);
  const [credenciaisModelo, setCredenciaisModelo] = useState([]);
  const [siteConfigItems, setSiteConfigItems] = useState([]);
  const [siteConfigForm, setSiteConfigForm] = useState(emptySiteConfigForm);
  const [fiscalManualForm, setFiscalManualForm] = useState(emptyFiscalManualForm);
  const [fiscalManualResumo, setFiscalManualResumo] = useState(null);
  const [fiscalManualRascunho, setFiscalManualRascunho] = useState('');
  const [loading, setLoading] = useState(true);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [activeTab, setActiveTab] = useState(routeState.activeTab);
  const [activeCadastroTab, setActiveCadastroTab] = useState(routeState.activeCadastroTab);
  const [activeIntegration, setActiveIntegration] = useState(routeState.activeIntegration);
  const [query, setQuery] = useState('');
  const [produtoForm, setProdutoForm] = useState(emptyProduto);
  const [categoriaForm, setCategoriaForm] = useState(emptyCategoria);
  const [clienteForm, setClienteForm] = useState(emptyCliente);
  const [fornecedorForm, setFornecedorForm] = useState(emptyFornecedor);
  const [leadForm, setLeadForm] = useState(emptyLead);
  const [empresaGrupoForm, setEmpresaGrupoForm] = useState(emptyEmpresaGrupo);
  const [formStatus, setFormStatus] = useState('');
  const [siteConfigStatus, setSiteConfigStatus] = useState({ type: '', message: '' });
  const [siteConfigSaving, setSiteConfigSaving] = useState(false);
  const [loadError, setLoadError] = useState('');
  const [salesPeriodDays, setSalesPeriodDays] = useState(7);
  const [salesPeriodLoading, setSalesPeriodLoading] = useState(false);
  const [salesPeriodError, setSalesPeriodError] = useState('');
  const sophiaEmail = siteConfigForm.site_sophia_email || siteConfigForm.site_email_contato || 'corporativo.gna@gmail.com';
  const sophiaMailTo = `mailto:${encodeURIComponent(sophiaEmail)}?subject=Sophia%20-%20Apoio%20administrativo&body=Ol%C3%A1%20Sophia%2C%20preciso%20de%20apoio%20administrativo%20por%20mensagem%20instant%C3%A2nea%20para%20d%C3%BAvidas%20internas%20e%20solu%C3%A7%C3%B5es%20operacionais.`;

  const loadData = useCallback(async () => {
    try {
      setLoadError('');
      const [resumoRes, pedidosRes, leadsRes, produtosRes, categoriasRes, clientesRes, fornecedoresRes, empresasGrupoRes, fiscalPedidosRes, integracoesRes, credenciaisRes, siteConfigRes, fiscalDraftRes] = await Promise.all([
        dashboardAPI.getResumo(),
        pedidoAPI.getAll(),
        leadAPI.getAll(),
        produtoAPI.getAll(),
        categoriaAPI.getAll(),
        clienteAPI.getAll(),
        fornecedorAPI.getAll(),
        empresaGrupoAPI.getAll(),
        fiscalAPI.getPedidos(),
        integracoesAPI.getDiagnostico(),
        integracoesAPI.getCredenciaisModelo(),
        siteAPI.getAll(),
        fiscalAPI.getRascunhoManual(),
      ]);
      if (!resumoRes.data || !Array.isArray(resumoRes.data.vendas_periodo)) {
        throw new Error('A API nao retornou a serie operacional de vendas do cockpit.');
      }
      setResumo(resumoRes.data);
      setSalesPeriodDays(resumoRes.data.periodo_dias);
      setPedidos(Array.isArray(pedidosRes.data) ? pedidosRes.data : []);
      setLeads(Array.isArray(leadsRes.data) ? leadsRes.data : []);
      setProdutos(Array.isArray(produtosRes.data) ? produtosRes.data : []);
      setCategorias(Array.isArray(categoriasRes.data) ? categoriasRes.data : []);
      setClientes(Array.isArray(clientesRes.data) ? clientesRes.data : []);
      setFornecedores(Array.isArray(fornecedoresRes.data) ? fornecedoresRes.data : []);
      setEmpresasGrupo(Array.isArray(empresasGrupoRes.data) ? empresasGrupoRes.data : []);
      setFiscalPedidos(Array.isArray(fiscalPedidosRes.data) ? fiscalPedidosRes.data : []);
      setIntegracoes(Array.isArray(integracoesRes.data) ? integracoesRes.data : []);
      setCredenciaisModelo(Array.isArray(credenciaisRes.data) ? credenciaisRes.data : []);
      if (Array.isArray(siteConfigRes.data) && siteConfigRes.data.length > 0) {
        setSiteConfigItems(siteConfigRes.data);
        setSiteConfigForm((current) => buildSiteConfigFormFromItems(siteConfigRes.data, current));
      }
      const fiscalDraft = fiscalDraftRes?.data ?? {};
      const fiscalDraftPayload = safeJsonParse(fiscalDraft.valor || fiscalDraft.Valor || fiscalDraft.Valor || fiscalDraft.payload, null);
      if (fiscalDraftPayload && typeof fiscalDraftPayload === 'object' && !Array.isArray(fiscalDraftPayload)) {
        setFiscalManualRascunho(getFiscalDraftSavedAt(fiscalDraft));
        setFiscalManualForm((current) => ({
          ...current,
          empresaEmissora: fiscalDraftPayload.empresaEmissora ?? fiscalDraftPayload.EmpresaEmissora ?? current.empresaEmissora,
          cnpjEmissor: fiscalDraftPayload.cnpjEmissor ?? fiscalDraftPayload.CnpjEmissor ?? current.cnpjEmissor,
          clienteDestinatario: fiscalDraftPayload.clienteDestinatario ?? fiscalDraftPayload.ClienteDestinatario ?? current.clienteDestinatario,
          documentoDestinatario: fiscalDraftPayload.documentoDestinatario ?? fiscalDraftPayload.DocumentoDestinatario ?? current.documentoDestinatario,
          naturezaOperacao: fiscalDraftPayload.naturezaOperacao ?? fiscalDraftPayload.NaturezaOperacao ?? current.naturezaOperacao,
          cfop: fiscalDraftPayload.cfop ?? fiscalDraftPayload.Cfop ?? current.cfop,
          subtotal: String(fiscalDraftPayload.subtotal ?? fiscalDraftPayload.Subtotal ?? current.subtotal),
          frete: String(fiscalDraftPayload.frete ?? fiscalDraftPayload.Frete ?? current.frete),
          impostosEstimados: String(fiscalDraftPayload.impostosEstimados ?? fiscalDraftPayload.ImpostosEstimados ?? current.impostosEstimados),
          margemMinima: String(fiscalDraftPayload.margemMinima ?? fiscalDraftPayload.MargemMinima ?? current.margemMinima),
          observacoes: fiscalDraftPayload.observacoes ?? fiscalDraftPayload.Observacoes ?? current.observacoes,
          tipoOperacao: fiscalDraftPayload.tipoOperacao ?? fiscalDraftPayload.TipoOperacao ?? current.tipoOperacao,
          estadoOrigem: fiscalDraftPayload.estadoOrigem ?? fiscalDraftPayload.EstadoOrigem ?? current.estadoOrigem,
          estadoDestino: fiscalDraftPayload.estadoDestino ?? fiscalDraftPayload.EstadoDestino ?? current.estadoDestino,
          categoriaFiscal: fiscalDraftPayload.categoriaFiscal ?? fiscalDraftPayload.CategoriaFiscal ?? current.categoriaFiscal,
          subcategoriaFiscal: fiscalDraftPayload.subcategoriaFiscal ?? fiscalDraftPayload.SubcategoriaFiscal ?? current.subcategoriaFiscal,
          exigeMarketplace: Boolean(fiscalDraftPayload.exigeMarketplace ?? fiscalDraftPayload.ExigeMarketplace ?? current.exigeMarketplace),
          exigeDropshipping: Boolean(fiscalDraftPayload.exigeDropshipping ?? fiscalDraftPayload.ExigeDropshipping ?? current.exigeDropshipping),
          requerSaidaNfe: Boolean(fiscalDraftPayload.requerSaidaNfe ?? fiscalDraftPayload.RequerSaidaNfe ?? current.requerSaidaNfe),
          requerEntradaNfe: Boolean(fiscalDraftPayload.requerEntradaNfe ?? fiscalDraftPayload.RequerEntradaNfe ?? current.requerEntradaNfe),
        }));
      }
    } catch (error) {
      const message = getApiErrorMessage(error, 'Não foi possível carregar o painel administrativo.');
      setLoadError(message);
      console.error('Falha ao carregar painel administrativo:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  const changeSalesPeriod = async (days) => {
    if (salesPeriodLoading || days === salesPeriodDays) return;

    setSalesPeriodLoading(true);
    setSalesPeriodError('');
    try {
      const response = await dashboardAPI.getResumo(days);
      if (!response.data || !Array.isArray(response.data.vendas_periodo) || response.data.periodo_dias !== days) {
        throw new Error('A API nao confirmou o periodo solicitado para o cockpit.');
      }

      setResumo(response.data);
      setSalesPeriodDays(days);
    } catch (error) {
      setSalesPeriodError(getApiErrorMessage(error, 'Nao foi possivel atualizar o periodo de vendas.'));
    } finally {
      setSalesPeriodLoading(false);
    }
  };

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
    const produtoDuplicateMessage = getProdutoDuplicateMessage(produtoForm, produtos);
    if (produtoDuplicateMessage) {
      setFormStatus(produtoDuplicateMessage);
      return;
    }

    const payload = {
      ...produtoForm,
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

    const response = await produtoAPI.create(payload);
    setProdutos((current) => [response.data, ...current]);
    setProdutoForm(emptyProduto);
    setFormStatus('Produto cadastrado e disponível no catálogo.');
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
    const clienteDuplicateMessage = getClienteDuplicateMessage(clienteForm, clientes);
    if (clienteDuplicateMessage) {
      setFormStatus(clienteDuplicateMessage);
      return;
    }

    const response = await clienteAPI.create(clienteForm);
    setClientes((current) => {
      const semDuplicidade = current.filter((cliente) => cliente.id !== response.data.id);
      return [response.data, ...semDuplicidade];
    });
    setClienteForm(emptyCliente);
    setFormStatus('Cliente cadastrado no painel.');
  };

  const submitFornecedor = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const fornecedorDuplicateMessage = getFornecedorDuplicateMessage(fornecedorForm, fornecedores);
    if (fornecedorDuplicateMessage) {
      setFormStatus(fornecedorDuplicateMessage);
      return;
    }

    const response = await fornecedorAPI.create(fornecedorForm);
    setFornecedores((current) => [response.data, ...current]);
    setFornecedorForm(emptyFornecedor);
    setFormStatus('Fornecedor cadastrado no painel.');
  };

  const submitLead = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const response = await leadAPI.create(leadForm);
    setLeads((current) => [response.data, ...current]);
    setLeadForm(emptyLead);
    setFormStatus('Lead cadastrado no CRM.');
  };

  const persistSiteConfiguracoes = async (overrides = {}) => {
    if (siteConfigSaving) {
      throw new Error('Já existe uma gravação de configuração pública em andamento.');
    }

    const sourceForm = { ...siteConfigForm, ...overrides };
    setSiteConfigStatus({ type: '', message: '' });
    setSiteConfigSaving(true);

    let payload;

    try {
      payload = siteConfigFieldMeta.map((field) => {
        const existing = siteConfigItems.find((item) => item.chave === field.key);
        const value = normalizeSiteConfigValue(field.key, sourceForm[field.key] ?? '');
        return {
          chave: field.key,
          valor: value,
          tipo: siteConfigJsonFields.has(field.key) ? 'JSON' : (field.type === 'number' ? 'Numero' : field.type === 'color' ? 'Cor' : existing?.tipo || 'Texto'),
          descricao: existing?.descricao || field.description,
          grupo: existing?.grupo || field.group,
          editavel: existing?.editavel ?? true,
        };
      });
    } catch (error) {
      setSiteConfigStatus({ type: 'error', message: 'Os campos em JSON precisam estar válidos antes de salvar.' });
      setSiteConfigSaving(false);
      throw error;
    }

    try {
      const persistedResponse = await siteAPI.update(payload);
      const persistedItems = persistedResponse.data;

      if (!Array.isArray(persistedItems)) {
        throw new Error('A API não confirmou as configurações persistidas no banco.');
      }

      const persistedByKey = new Map(persistedItems.map((item) => [String(item.chave || '').toLowerCase(), item]));
      const unconfirmedItem = payload.find((item) => {
        const persisted = persistedByKey.get(item.chave.toLowerCase());
        return !persisted || String(persisted.valor ?? '') !== String(item.valor ?? '').trim();
      });

      if (unconfirmedItem) {
        throw new Error(`A API não confirmou no banco o valor da configuração ${unconfirmedItem.chave}.`);
      }

      setSiteConfigItems(persistedItems);
      setSiteConfigForm((current) => buildSiteConfigFormFromItems(persistedItems, current));
      setSiteConfigStatus({
        type: 'success',
        message: `${payload.length} configurações da home, banners e contatos foram gravadas e relidas do banco.`,
      });
      return persistedItems;
    } catch (error) {
      setSiteConfigStatus({
        type: 'error',
        message: error.response?.data?.detail || error.response?.data?.mensagem || error.message || 'Não foi possível salvar as configurações do site.',
      });
      throw error;
    } finally {
      setSiteConfigSaving(false);
    }
  };

  const submitSiteConfiguracoes = async (event) => {
    event.preventDefault();
    try {
      await persistSiteConfiguracoes();
    } catch {
      // O estado visível da própria tela já contém o erro operacional retornado pela API.
    }
  };

  const submitEmpresaGrupo = async (event) => {
    event.preventDefault();
    setFormStatus('');
    const empresaDuplicateMessage = getEmpresaGrupoDuplicateMessage(empresaGrupoForm, empresasGrupo);
    if (empresaDuplicateMessage) {
      setFormStatus(empresaDuplicateMessage);
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

  const submitFiscalManualRascunho = async () => {
    setFormStatus('');
    const payload = {
      empresaEmissora: fiscalManualForm.empresaEmissora.trim(),
      cnpjEmissor: fiscalManualForm.cnpjEmissor.trim(),
      clienteDestinatario: fiscalManualForm.clienteDestinatario.trim(),
      documentoDestinatario: fiscalManualForm.documentoDestinatario.trim(),
      naturezaOperacao: fiscalManualForm.naturezaOperacao.trim(),
      cfop: fiscalManualForm.cfop.trim(),
      subtotal: Number(fiscalManualForm.subtotal || 0),
      frete: Number(fiscalManualForm.frete || 0),
      impostosEstimados: Number(fiscalManualForm.impostosEstimados || 0),
      margemMinima: Number(fiscalManualForm.margemMinima || 0),
      observacoes: fiscalManualForm.observacoes.trim(),
      tipoOperacao: fiscalManualForm.tipoOperacao,
      estadoOrigem: fiscalManualForm.estadoOrigem.trim().toUpperCase(),
      estadoDestino: fiscalManualForm.estadoDestino.trim().toUpperCase(),
      categoriaFiscal: fiscalManualForm.categoriaFiscal.trim() || null,
      subcategoriaFiscal: fiscalManualForm.subcategoriaFiscal.trim() || null,
      exigeMarketplace: Boolean(fiscalManualForm.exigeMarketplace),
      exigeDropshipping: Boolean(fiscalManualForm.exigeDropshipping),
      requerSaidaNfe: Boolean(fiscalManualForm.requerSaidaNfe),
      requerEntradaNfe: Boolean(fiscalManualForm.requerEntradaNfe),
    };

    try {
      const response = await fiscalAPI.salvarRascunhoManual(payload);
      let savedAt = getFiscalDraftSavedAt(response.data);

      if (!savedAt) {
        const persistedResponse = await fiscalAPI.getRascunhoManual();
        savedAt = getFiscalDraftSavedAt(persistedResponse.data);
      }

      if (!savedAt) {
        throw new Error('A API não confirmou a data do rascunho persistido no banco.');
      }

      setFiscalManualRascunho(savedAt);
      setFormStatus('Rascunho fiscal manual confirmado no banco.');
    } catch (error) {
      setFormStatus(error.response?.data?.detail || error.response?.data?.mensagem || error.message || 'Não foi possível salvar o rascunho fiscal manual.');
    }
  };

  const prepararFiscalManual = async () => {
    setFormStatus('');
    setFiscalManualResumo(null);

    const payload = {
      empresaEmissora: fiscalManualForm.empresaEmissora.trim(),
      cnpjEmissor: fiscalManualForm.cnpjEmissor.trim(),
      clienteDestinatario: fiscalManualForm.clienteDestinatario.trim(),
      documentoDestinatario: fiscalManualForm.documentoDestinatario.trim(),
      naturezaOperacao: fiscalManualForm.naturezaOperacao.trim(),
      cfop: fiscalManualForm.cfop.trim(),
      subtotal: Number(fiscalManualForm.subtotal || 0),
      frete: Number(fiscalManualForm.frete || 0),
      impostosEstimados: Number(fiscalManualForm.impostosEstimados || 0),
      margemMinima: Number(fiscalManualForm.margemMinima || 0),
      observacoes: fiscalManualForm.observacoes.trim(),
      tipoOperacao: fiscalManualForm.tipoOperacao,
      estadoOrigem: fiscalManualForm.estadoOrigem.trim().toUpperCase(),
      estadoDestino: fiscalManualForm.estadoDestino.trim().toUpperCase(),
      categoriaFiscal: fiscalManualForm.categoriaFiscal.trim() || null,
      subcategoriaFiscal: fiscalManualForm.subcategoriaFiscal.trim() || null,
      exigeMarketplace: Boolean(fiscalManualForm.exigeMarketplace),
      exigeDropshipping: Boolean(fiscalManualForm.exigeDropshipping),
      requerSaidaNfe: Boolean(fiscalManualForm.requerSaidaNfe),
      requerEntradaNfe: Boolean(fiscalManualForm.requerEntradaNfe),
    };

    const response = await fiscalAPI.prepararEmissaoManual(payload);
    const resumo = response.data?.dados ?? response.data?.Dados ?? response.data?.data ?? response.data;
    setFiscalManualResumo(resumo);
    setFormStatus('Pré-análise fiscal concluída. Revise as pendências antes de emitir.');
  };

  const updateLeadStatus = async (id, status) => {
    const response = await leadAPI.updateStatus(id, status);
    setLeads((current) => current.map((lead) => (lead.id === id ? response.data : lead)));
  };

  const updatePedidoStatus = async (id, status) => {
    const response = await pedidoAPI.updateStatus(id, status);
    setPedidos((current) => current.map((pedido) => (pedido.id === id ? response.data : pedido)));
  };

  const filteredPedidos = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return pedidos;
    return pedidos.filter((pedido) =>
      [pedido.numero_pedido, pedido.status, pedido.total].some((value) => String(value || '').toLowerCase().includes(term))
    );
  }, [pedidos, query]);

  const filteredLeads = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return leads;
    return leads.filter((lead) =>
      [lead.nome, lead.email, lead.telefone, lead.status].some((value) => String(value || '').toLowerCase().includes(term))
    );
  }, [leads, query]);

  const produtoDuplicateMessage = useMemo(() => getProdutoDuplicateMessage(produtoForm, produtos), [produtoForm, produtos]);
  const clienteDuplicateMessage = useMemo(() => getClienteDuplicateMessage(clienteForm, clientes), [clienteForm, clientes]);
  const fornecedorDuplicateMessage = useMemo(() => getFornecedorDuplicateMessage(fornecedorForm, fornecedores), [fornecedorForm, fornecedores]);
  const empresaGrupoDuplicateMessage = useMemo(() => getEmpresaGrupoDuplicateMessage(empresaGrupoForm, empresasGrupo), [empresaGrupoForm, empresasGrupo]);
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

  const statusCounts = useMemo(() => {
    return pedidos.reduce((acc, pedido) => {
      acc[pedido.status] = (acc[pedido.status] || 0) + 1;
      return acc;
    }, {});
  }, [pedidos]);
  const salesSeries = useMemo(() => (
    resumo.vendas_periodo.map((item) => {
      const date = new Date(`${item.data}T12:00:00`);
      const dateLabel = salesDateFormatter.format(date);
      return {
        data: item.data,
        label: salesPeriodDays === 7 ? `${salesWeekdayFormatter.format(date)} ${dateLabel}` : dateLabel,
        receita: Number(item.receita_confirmada),
        pedidos: Number(item.pedidos_confirmados),
      };
    })
  ), [resumo.vendas_periodo, salesPeriodDays]);
  const salesTotals = useMemo(() => salesSeries.reduce(
    (total, item) => ({ receita: total.receita + item.receita, pedidos: total.pedidos + item.pedidos }),
    { receita: 0, pedidos: 0 },
  ), [salesSeries]);
  const salesChartTickInterval = salesPeriodDays === 7 ? 0 : salesPeriodDays === 30 ? 4 : 14;

  if (authLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#050505]">
        <div className="h-12 w-12 animate-spin rounded-full border-4 border-slate-200 border-t-slate-950" />
      </div>
    );
  }

  if (!isAuthenticated) return <Navigate to="/login" />;

  return (
    <main className="nexum-admin-shell min-h-screen bg-[#050505]">
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-72 border-r border-[#2A2A2A] bg-[#0A0A0A] text-white lg:block">
        <div className="flex h-full flex-col p-6">
          <Link to="/" className="flex items-center gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-[#C9A227] text-sm font-black text-black">NA</div>
            <div>
              <p className="font-serif text-lg font-black tracking-widest text-[#C9A227]">GENESISGEST.NET</p>
              <p className="text-xs font-bold uppercase tracking-[0.18em] text-zinc-500">Painel administrativo Nexum Altivon</p>
            </div>
          </Link>

          <nav className="mt-8 space-y-5 overflow-y-auto pb-6">
            {navSections.map((section) => {
              const activeItems = tabs.filter((tab) => tab.section === section);
              if (activeItems.length === 0) return null;

              return (
                <div key={section}>
                  <p className="px-3 pb-2 text-[0.65rem] font-black uppercase tracking-[0.22em] text-zinc-600">{section}</p>
                  <div className="space-y-1">
                    {activeItems.map((tab) => {
                      const Icon = tab.icon;
                      const active = activeTab === tab.id;
                      return (
                        <button
                          key={tab.id}
                          onClick={() => openMainTab(tab.id)}
                          className={`flex w-full items-center gap-3 border-l-4 px-3 py-3 text-left text-sm font-bold transition ${
                            active ? 'border-[#C9A227] bg-[#C9A227]/10 text-[#C9A227]' : 'border-transparent text-zinc-400 hover:border-[#C9A227]/60 hover:bg-white/5 hover:text-[#E8D5A3]'
                          }`}
                          data-testid={`tab-${tab.id}`}
                        >
                          <Icon size={18} />
                          <span>{tab.label}</span>
                          {tab.badge && <span className="ml-auto rounded-full bg-emerald-600 px-2 py-0.5 text-[0.62rem] uppercase text-white">{tab.badge}</span>}
                        </button>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </nav>

          <div className="mt-auto rounded-lg border border-white/10 bg-white/5 p-4">
            <div className="flex items-center gap-2 text-[#E8D5A3]">
              <Sparkles size={16} />
              <p className="text-xs font-black uppercase tracking-[0.18em]">Operação real</p>
            </div>
            <p className="mt-3 text-2xl font-black">{formatPrice(resumo.faturamento_mes)}</p>
            <p className="mt-3 text-xs font-semibold text-zinc-400">Receita aprovada no mês corrente.</p>
            {resumo.atualizado_em && (
              <p className="mt-2 text-[0.68rem] font-bold uppercase text-zinc-600">
                Atualizado {new Date(resumo.atualizado_em).toLocaleString('pt-BR')}
              </p>
            )}
          </div>
        </div>
      </aside>

      <section className="lg:pl-72">
        <header className="sticky top-0 z-30 border-b border-[#2A2A2A] bg-[#0A0A0A]/95 text-white backdrop-blur-xl">
          <div className="flex min-h-[76px] flex-col gap-4 px-4 py-4 sm:px-6 xl:flex-row xl:items-center xl:justify-between xl:px-8">
            <div>
              <p className="text-sm font-bold text-zinc-500"><span className="text-[#C9A227]">{tabs.find((tab) => tab.id === activeTab)?.label || 'Dashboard'}</span> / Gestão</p>
              <h1 className="text-2xl font-black text-white" data-testid="dashboard-title">GenesisGest.Net</h1>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="rounded-full border border-[#2A2A2A] bg-[#111111] px-4 py-2 text-right">
                <p className="text-[0.68rem] font-black uppercase tracking-[0.18em] text-zinc-500">Operando como</p>
                <p className="text-sm font-bold text-white">{user?.nome || user?.email || 'Equipe Nexum'}</p>
              </div>
              <a
                href={sophiaMailTo}
                className="inline-flex h-11 items-center justify-center gap-2 rounded-full border border-[#2A2A2A] bg-[#111111] px-5 text-sm font-black text-[#E8D5A3] transition hover:border-[#C9A227] hover:text-white"
              >
                Chamar Sophia
              </a>
              <div className="relative">
                <Search className="absolute left-3 top-3 text-zinc-500" size={18} />
                <input
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  aria-label="Buscar pedidos, clientes, produtos ou leads"
                  className="h-11 w-full rounded-full border border-[#2A2A2A] bg-[#050505] pl-10 pr-4 text-sm font-semibold text-white outline-none transition focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10 sm:w-80"
                />
              </div>
              <button className="inline-flex h-11 w-11 items-center justify-center rounded-full border border-[#2A2A2A] bg-[#1A1A1A] text-zinc-300" aria-label="Notificações" title="Notificações">
                <Bell size={19} />
              </button>
              <button
                onClick={logout}
                className="inline-flex h-11 items-center justify-center gap-2 rounded-full bg-[#C9A227] px-5 text-sm font-black text-black"
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
                <div key={item} className="h-40 animate-pulse rounded-lg bg-white" />
              ))}
            </div>
          ) : (
            <>
              {loadError && (
                <div className="mb-6 rounded-lg border border-red-200 bg-red-50 px-5 py-4 text-sm font-bold text-red-800">
                  {loadError}
                </div>
              )}
              {activeTab === 'overview' && (
                <div className="space-y-6">
                  <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
                    <StatCard title="Pedidos hoje" value={resumo.pedidos_hoje} detail="Recebidos no dia operacional" icon={ShoppingBag} tone="slate" />
                    <StatCard title="Clientes" value={resumo.total_clientes} detail="Cadastros visíveis para o tenant" icon={Users} tone="emerald" />
                    <StatCard title="Faturamento" value={formatPrice(resumo.faturamento_mes)} detail="Pagamentos aprovados no mês" icon={TrendingUp} tone="amber" />
                    <StatCard title="Leads novos" value={resumo.leads_novos} detail="Aguardando qualificação" icon={UserRound} tone="indigo" />
                  </div>

                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.5fr)_minmax(360px,1fr)]">
                    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                        <div>
                          <h2 className="text-xl font-black text-slate-950">Performance de vendas</h2>
                          <p className="mt-1 text-sm text-slate-500">Receita com pagamento aprovado, consolidada no fuso de São Paulo.</p>
                        </div>
                        <div className="inline-flex self-start rounded-lg border border-slate-200 bg-slate-100 p-1" aria-label="Período do gráfico de vendas">
                          {salesPeriodOptions.map((days) => (
                            <button
                              key={days}
                              type="button"
                              onClick={() => changeSalesPeriod(days)}
                              disabled={salesPeriodLoading}
                              aria-pressed={salesPeriodDays === days}
                              className={`min-w-14 rounded-md px-3 py-2 text-xs font-black transition disabled:cursor-wait disabled:opacity-60 ${
                                salesPeriodDays === days ? 'bg-slate-950 text-white' : 'text-slate-600 hover:bg-white'
                              }`}
                            >
                              {days}d
                            </button>
                          ))}
                        </div>
                      </div>
                      <div className="mt-5 flex flex-wrap gap-3">
                        <span className="rounded-md bg-emerald-50 px-3 py-2 text-sm font-black text-emerald-800">
                          {formatPrice(salesTotals.receita)} confirmados
                        </span>
                        <span className="rounded-md bg-slate-100 px-3 py-2 text-sm font-black text-slate-700">
                          {salesTotals.pedidos} pedidos pagos
                        </span>
                        <span className="rounded-md bg-amber-50 px-3 py-2 text-sm font-black text-amber-800">
                          Conversão CRM {resumo.conversao}%
                        </span>
                      </div>
                      {salesPeriodError && (
                        <p className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm font-bold text-red-800">{salesPeriodError}</p>
                      )}
                      <div className="mt-6 h-72" aria-busy={salesPeriodLoading}>
                        {salesTotals.pedidos === 0 ? (
                          <div className="flex h-full flex-col items-center justify-center rounded-lg border border-dashed border-slate-300 bg-slate-50 px-6 text-center">
                            <TrendingUp className="text-slate-400" size={30} />
                            <p className="mt-3 font-black text-slate-800">Nenhuma venda com pagamento aprovado no período</p>
                            <p className="mt-1 text-sm text-slate-500">Pedidos pendentes ou recusados não são contabilizados como faturamento.</p>
                          </div>
                        ) : (
                          <ResponsiveContainer width="100%" height="100%">
                            <AreaChart data={salesSeries} margin={{ top: 12, right: 12, left: 4, bottom: 0 }}>
                              <CartesianGrid stroke="#E2E8F0" strokeDasharray="4 4" vertical={false} />
                              <XAxis dataKey="label" interval={salesChartTickInterval} tick={{ fill: '#64748B', fontSize: 11, fontWeight: 700 }} tickLine={false} axisLine={false} />
                              <YAxis tickFormatter={(value) => `R$ ${Number(value).toLocaleString('pt-BR')}`} tick={{ fill: '#64748B', fontSize: 11, fontWeight: 700 }} tickLine={false} axisLine={false} width={78} />
                              <Tooltip
                                formatter={(value) => [formatPrice(value), 'Receita aprovada']}
                                labelFormatter={(label) => `Data ${label}`}
                                contentStyle={{ border: '1px solid #CBD5E1', borderRadius: 8, fontWeight: 700 }}
                              />
                              <Area type="monotone" dataKey="receita" stroke="#0F172A" strokeWidth={3} fill="#C9A227" fillOpacity={0.28} activeDot={{ r: 5, fill: '#C9A227', stroke: '#0F172A', strokeWidth: 2 }} />
                            </AreaChart>
                          </ResponsiveContainer>
                        )}
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
                          { label: 'Pedidos em processamento', value: (statusCounts.Processando || 0) + (statusCounts.Pendente || 0), icon: PackageCheck, tone: 'text-indigo-700 bg-indigo-50' },
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
                    <OrdersTable pedidos={pedidos.slice(0, 5)} onStatusChange={updatePedidoStatus} />
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
                        <h3 className="text-lg font-black text-slate-950">Cadastro detalhado de produto</h3>
                        <p className="mt-1 text-sm font-semibold text-slate-500">Informações principais para catálogo, estoque e vitrine pública.</p>
                        {produtoDuplicateMessage && <DuplicateAlert message={produtoDuplicateMessage} />}
                        <div className="mt-5 grid gap-4 md:grid-cols-2">
                          <Field label="Slug público / ID do produto" value={produtoForm.id} onChange={(value) => setProdutoForm((form) => ({ ...form, id: value }))} />
                          <Field label="Nome" value={produtoForm.nome} onChange={(value) => setProdutoForm((form) => ({ ...form, nome: value }))} required />
                          <Field label="SKU / Código interno" value={produtoForm.sku} onChange={(value) => setProdutoForm((form) => ({ ...form, sku: value }))} />
                          <SelectField label="Tipo do produto" value={produtoForm.tipoProduto} onChange={(value) => setProdutoForm((form) => ({ ...form, tipoProduto: value }))} options={['Proprio', 'Dropshipping', 'Marketplace', 'Afiliado']} />
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
                        <button disabled={Boolean(produtoDuplicateMessage)} className="mt-5 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:cursor-not-allowed disabled:bg-slate-300">
                          <Save size={17} />
                          Salvar produto
                        </button>
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
                        <CompactList title="Produtos cadastrados" items={produtos} fields={['nome', 'sku', 'estoque']} />
                      </div>
                    </div>
                  )}

                  {activeCadastroTab === 'clientes' && (
                    <div className="grid gap-6 xl:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
                      <SimpleForm title="Cadastro detalhado de cliente" subtitle="Evita duplicidade por e-mail/CPF e reaproveita registros existentes." onSubmit={submitCliente} buttonLabel="Salvar cliente" alertMessage={clienteDuplicateMessage} disabled={Boolean(clienteDuplicateMessage)}>
                        <Field label="Nome completo / Razão social" value={clienteForm.nome} onChange={(value) => setClienteForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="Email principal" type="email" value={clienteForm.email} onChange={(value) => setClienteForm((form) => ({ ...form, email: value }))} required />
                        <Field label="Telefone / WhatsApp" value={clienteForm.telefone} onChange={(value) => setClienteForm((form) => ({ ...form, telefone: value }))} />
                        <Field label="CPF/CNPJ" value={clienteForm.cpf} onChange={(value) => setClienteForm((form) => ({ ...form, cpf: value }))} />
                      </SimpleForm>
                      <div className="space-y-6">
                        <CadastroInsightPanel title="Controle de clientes" checks={cadastroHighlights.clientes} />
                        <CompactList title="Clientes cadastrados" items={clientes} fields={['nome', 'email', 'telefone', 'cpf']} />
                      </div>
                    </div>
                  )}

                  {activeCadastroTab === 'fornecedores' && (
                    <div className="grid gap-6 xl:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
                      <SimpleForm title="Cadastro detalhado de fornecedor" subtitle="Base para compras, dropshipping, logística e integrações futuras." onSubmit={submitFornecedor} buttonLabel="Salvar fornecedor" alertMessage={fornecedorDuplicateMessage} disabled={Boolean(fornecedorDuplicateMessage)}>
                        <Field label="Nome / Razão social" value={fornecedorForm.nome} onChange={(value) => setFornecedorForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="Documento CNPJ/CPF" value={fornecedorForm.documento} onChange={(value) => setFornecedorForm((form) => ({ ...form, documento: value }))} />
                        <Field label="Email comercial" type="email" value={fornecedorForm.email} onChange={(value) => setFornecedorForm((form) => ({ ...form, email: value }))} />
                        <Field label="Telefone / WhatsApp" value={fornecedorForm.telefone} onChange={(value) => setFornecedorForm((form) => ({ ...form, telefone: value }))} />
                        <Field label="Categoria / Segmento" value={fornecedorForm.categoria} onChange={(value) => setFornecedorForm((form) => ({ ...form, categoria: value }))} />
                      </SimpleForm>
                      <div className="space-y-6">
                        <CadastroInsightPanel title="Controle de fornecedores" checks={cadastroHighlights.fornecedores} />
                        <CompactList title="Fornecedores cadastrados" items={fornecedores} fields={['nome', 'documento', 'email', 'categoria']} />
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
                  </div>
                  <OrdersTable pedidos={filteredPedidos} onStatusChange={updatePedidoStatus} />
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

              {activeTab === 'cupons' && <CupomAdminPanel />}

              {activeTab === 'marketing' && <MarketingAdminPanel />}

              {activeTab === 'usuarios' && <AccessAuditPanel mode="usuarios" />}

              {activeTab === 'auditoria' && <AccessAuditPanel mode="auditoria" />}

              {activeTab === 'erp' && (
                <section className="space-y-6">
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
                </section>
              )}

              {activeTab === 'erp-empresas' && (
                <section className="space-y-6">
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
                </section>
              )}

              {activeTab === 'erp-fiscal' && (
                <section className="space-y-6">
                  <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                    <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">Fiscal operacional</p>
                    <h2 className="mt-2 text-2xl font-black text-slate-950">Notas e automação fiscal</h2>
                    <p className="mt-1 max-w-3xl text-sm text-slate-500">
                      A ERP agora expõe a fila fiscal dos pedidos, com empresa emitente sugerida, ambiente, CFOP e resumo da automação preparada no ato da compra.
                    </p>
                  </div>

                  <section className="grid gap-6 xl:grid-cols-[minmax(0,1.08fr)_minmax(320px,0.92fr)]">
                    <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                        <div>
                          <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Emissão manual</p>
                          <h3 className="mt-2 text-lg font-black text-slate-950">Rascunho e pré-análise antes da NF-e</h3>
                          <p className="mt-1 text-sm text-slate-500">
                            Preencha os dados abaixo, salve o rascunho no banco e rode a checagem fiscal para validar empresa, certificado e margem.
                          </p>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          <button
                            type="button"
                            onClick={submitFiscalManualRascunho}
                            className="inline-flex h-11 items-center gap-2 rounded-lg border border-slate-200 bg-slate-50 px-4 text-sm font-black text-slate-800 transition hover:border-slate-950 hover:bg-white"
                          >
                            <Save size={16} />
                            Salvar rascunho
                          </button>
                          <button
                            type="button"
                            onClick={prepararFiscalManual}
                            className="inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-4 text-sm font-black text-white transition hover:bg-[#C9A227] hover:text-slate-950"
                          >
                            <FileText size={16} />
                            Preparar emissão
                          </button>
                        </div>
                      </div>

                      <div className="mt-6 grid gap-4 md:grid-cols-2">
                        <Field label="Empresa emissora" value={fiscalManualForm.empresaEmissora} onChange={(value) => setFiscalManualForm((form) => ({ ...form, empresaEmissora: value }))} />
                        <Field label="CNPJ do emissor" value={fiscalManualForm.cnpjEmissor} onChange={(value) => setFiscalManualForm((form) => ({ ...form, cnpjEmissor: value }))} />
                        <Field label="Cliente destinatário" value={fiscalManualForm.clienteDestinatario} onChange={(value) => setFiscalManualForm((form) => ({ ...form, clienteDestinatario: value }))} />
                        <Field label="Documento do destinatário" value={fiscalManualForm.documentoDestinatario} onChange={(value) => setFiscalManualForm((form) => ({ ...form, documentoDestinatario: value }))} />
                        <Field label="Natureza da operação" value={fiscalManualForm.naturezaOperacao} onChange={(value) => setFiscalManualForm((form) => ({ ...form, naturezaOperacao: value }))} className="md:col-span-2" />
                        <Field label="CFOP" value={fiscalManualForm.cfop} onChange={(value) => setFiscalManualForm((form) => ({ ...form, cfop: value }))} />
                        <SelectField label="Tipo de operação" value={fiscalManualForm.tipoOperacao} onChange={(value) => setFiscalManualForm((form) => ({ ...form, tipoOperacao: value }))} options={['VendaInterna', 'VendaInterestadual']} />
                        <Field label="Estado de origem" value={fiscalManualForm.estadoOrigem} onChange={(value) => setFiscalManualForm((form) => ({ ...form, estadoOrigem: value }))} />
                        <Field label="Estado de destino" value={fiscalManualForm.estadoDestino} onChange={(value) => setFiscalManualForm((form) => ({ ...form, estadoDestino: value }))} />
                        <Field label="Subtotal" type="number" value={fiscalManualForm.subtotal} onChange={(value) => setFiscalManualForm((form) => ({ ...form, subtotal: value }))} />
                        <Field label="Frete" type="number" value={fiscalManualForm.frete} onChange={(value) => setFiscalManualForm((form) => ({ ...form, frete: value }))} />
                        <Field label="Impostos estimados" type="number" value={fiscalManualForm.impostosEstimados} onChange={(value) => setFiscalManualForm((form) => ({ ...form, impostosEstimados: value }))} />
                        <Field label="Margem mínima (%)" type="number" value={fiscalManualForm.margemMinima} onChange={(value) => setFiscalManualForm((form) => ({ ...form, margemMinima: value }))} />
                        <Field label="Categoria fiscal" value={fiscalManualForm.categoriaFiscal} onChange={(value) => setFiscalManualForm((form) => ({ ...form, categoriaFiscal: value }))} />
                        <Field label="Subcategoria fiscal" value={fiscalManualForm.subcategoriaFiscal} onChange={(value) => setFiscalManualForm((form) => ({ ...form, subcategoriaFiscal: value }))} />
                        <TextAreaField label="Observações" value={fiscalManualForm.observacoes} onChange={(value) => setFiscalManualForm((form) => ({ ...form, observacoes: value }))} className="md:col-span-2" rows={3} />
                        <ToggleField label="Exige marketplace" checked={fiscalManualForm.exigeMarketplace} onChange={(checked) => setFiscalManualForm((form) => ({ ...form, exigeMarketplace: checked }))} />
                        <ToggleField label="Exige dropshipping" checked={fiscalManualForm.exigeDropshipping} onChange={(checked) => setFiscalManualForm((form) => ({ ...form, exigeDropshipping: checked }))} />
                        <ToggleField label="Requer saída NF-e" checked={fiscalManualForm.requerSaidaNfe} onChange={(checked) => setFiscalManualForm((form) => ({ ...form, requerSaidaNfe: checked }))} />
                        <ToggleField label="Requer entrada NF-e" checked={fiscalManualForm.requerEntradaNfe} onChange={(checked) => setFiscalManualForm((form) => ({ ...form, requerEntradaNfe: checked }))} />
                      </div>

                      {fiscalManualRascunho && (
                        <p className="mt-4 text-xs font-semibold text-slate-500">
                          Último rascunho salvo em {String(fiscalManualRascunho).replace('T', ' ').replace('Z', '')}
                        </p>
                      )}
                    </div>

                    <div className="space-y-6">
                      <ErpChecklistCard
                        title="Checklist fiscal antes da emissão"
                        items={[
                          'Certificado digital operacional e válido.',
                          'Empresa emitente compatível com o estado e com a operação.',
                          'Margem estimada acima do mínimo definido.',
                          'CFOP e natureza de operação conferidos antes do envio.',
                        ]}
                      />

                      {fiscalManualResumo ? (
                        <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                          <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Pré-análise fiscal</p>
                          <h3 className="mt-2 text-lg font-black text-slate-950">Resultado da validação</h3>
                          <div className="mt-4 space-y-3 rounded-2xl border border-slate-100 bg-slate-50 p-4">
                            <p className="text-sm font-semibold text-slate-700"><strong>Status:</strong> {fiscalManualResumo.certificadoStatus || '-'}</p>
                            <p className="text-sm font-semibold text-slate-700"><strong>Certificado:</strong> {fiscalManualResumo.certificadoOperacional ? 'Operacional' : 'Pendente'}</p>
                            <p className="text-sm font-semibold text-slate-700"><strong>Emitente sugerido:</strong> {fiscalManualResumo.razaoSocialSelecionada || fiscalManualResumo.codigoEmpresaSelecionada || '-'}</p>
                            <p className="text-sm font-semibold text-slate-700"><strong>Resumo do roteamento:</strong> {fiscalManualResumo.roteamentoResumo || '-'}</p>
                          </div>
                          <div className="mt-4">
                            <h4 className="text-sm font-black uppercase tracking-[0.16em] text-slate-500">Pendências</h4>
                            <div className="mt-3 space-y-2">
                              {(fiscalManualResumo.pendencias || []).length === 0 ? (
                                <p className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-800">
                                  Nenhuma pendência crítica encontrada.
                                </p>
                              ) : (
                                fiscalManualResumo.pendencias.map((pendencia) => (
                                  <p key={pendencia} className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm font-bold text-amber-900">
                                    {pendencia}
                                  </p>
                                ))
                              )}
                            </div>
                          </div>
                          {fiscalManualResumo.geradoEm && (
                            <p className="mt-4 text-xs font-semibold text-slate-500">
                              Gerado em {formatDate(fiscalManualResumo.geradoEm)}
                            </p>
                          )}
                        </section>
                      ) : (
                        <ErpChecklistCard
                          title="Como usar a emissão manual"
                          items={[
                            'Preencha os dados do destinatário e da operação.',
                            'Salve o rascunho para não perder informação.',
                            'Execute a preparação para ver empresa, certificado e margem.',
                            'Só depois siga para a emissão definitiva.',
                          ]}
                        />
                      )}
                    </div>
                  </section>

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
                </section>
              )}

              {activeTab === 'erp-financeiro' && (
                <section className="space-y-6">
                  <ErpModuleHero
                    eyebrow="Financeiro operacional"
                    title="Fluxo de caixa, contas e conciliação"
                    description="Tela dedicada para a retaguarda financeira com visão de entradas, saídas, recebimentos e base para DRE."
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
                    <StatMiniCard label="Faturamento do mês" value={formatPrice(resumo.faturamento_mes || 0)} />
                    <StatMiniCard label="Ticket médio" value={formatPrice(resumo.ticket_medio || 0)} />
                    <StatMiniCard label="Pedidos hoje" value={resumo.pedidos_hoje || 0} />
                    <StatMiniCard label="Conversão" value={`${resumo.conversao || 0}%`} />
                  </div>
                  <div className="grid gap-6 xl:grid-cols-2">
                    <ErpChecklistCard
                      title="Rotina financeira"
                      items={[
                        'Baixa automática por webhook de pagamento.',
                        'Fluxo de caixa por pedido, loja e canal.',
                        'Contas a pagar/receber com conciliação assistida.',
                        'Preparação para DRE e balancete em tempo real.',
                      ]}
                    />
                    <ErpChecklistCard
                      title="Acessos rápidos"
                      items={[
                        'Conferir pedidos aprovados antes do faturamento.',
                        'Relacionar gateway, taxa e liquidação.',
                        'Separar receita operacional e custos logísticos.',
                        'Base pronta para relatórios contábeis detalhados.',
                      ]}
                    />
                  </div>
                </section>
              )}

              {activeTab === 'erp-logistica' && (
                <section className="space-y-6">
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
                <section className="space-y-6">
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
                <section className="space-y-6">
                  <ErpModuleHero
                    eyebrow="Compras e suprimentos"
                    title="Fornecedores, reposição e estratégia de custo"
                    description="Tela dedicada para compras, reposição e tomada de decisão entre fornecedores, dropshipping e parceiros."
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
                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
                    <CompactList title="Fornecedores mapeados" items={fornecedores} fields={['nome', 'categoria', 'email', 'telefone']} />
                    <ErpChecklistCard
                      title="Critérios de compra"
                      items={[
                        'Menor custo com maior margem líquida.',
                        'Comparação entre fornecedor próprio, parceiro e drop.',
                        'Relação direta com estoque mínimo e reservado.',
                        'Base pronta para aprovação de compras e contratos.',
                      ]}
                    />
                  </div>
                </section>
              )}

              {activeTab === 'erp-relatorios' && (
                <section className="space-y-6">
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
                    description="Identidade, banners, contatos e textos persistidos no banco com imagens públicas servidas pela API oficial."
                  />
                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.08fr)_minmax(340px,0.92fr)]">
                    <form onSubmit={submitSiteConfiguracoes} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <div className="flex items-start justify-between gap-4">
                        <div>
                          <h3 className="text-lg font-black text-slate-950">Editor da home comercial</h3>
                          <p className="mt-1 text-sm text-slate-500">Salvar aqui atualiza a tabela <code className="rounded bg-slate-100 px-1.5 py-0.5">configuracoes_sistema</code> e prepara a home para uso real.</p>
                        </div>
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black uppercase tracking-[0.16em] text-slate-600">
                          {siteConfigItems.length || siteConfigFieldMeta.length} chaves
                        </span>
                      </div>
                      <div className="mt-6 space-y-6">
                        {siteConfigSections.map((section) => (
                          <section key={section.id} className="rounded-2xl border border-slate-200 bg-slate-50/50 p-4">
                            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                              <div>
                                <h4 className="text-sm font-black uppercase tracking-[0.16em] text-slate-900">{section.title}</h4>
                                <p className="mt-1 text-xs font-semibold leading-5 text-slate-500">{section.description}</p>
                              </div>
                              <span className="rounded-full bg-white px-3 py-1 text-[11px] font-black uppercase tracking-[0.18em] text-slate-500">
                                {section.fields.length} campos
                              </span>
                            </div>
                            <div className="mt-4 grid gap-4 md:grid-cols-2">
                              {section.fields.map((key) => {
                                const field = siteConfigFieldByKey[key];
                                const value = siteConfigForm[key] ?? '';
                                const isJson = siteConfigJsonFields.has(key);
                                const isWide = key === 'home_footer_texto' || key === 'home_intro_texto_1' || key === 'home_intro_texto_2' || isJson;

                                return (
                                  <label key={key} className={`block text-sm font-bold text-slate-700 ${isWide ? 'md:col-span-2' : ''}`}>
                                    <div className="flex items-center justify-between gap-3">
                                      <span>{field?.label || key}</span>
                                      {isJson && (
                                        <span className="rounded-full bg-slate-900 px-2.5 py-1 text-[10px] font-black uppercase tracking-[0.16em] text-white">
                                          JSON
                                        </span>
                                      )}
                                    </div>
                                    {key === 'site_logo' && (
                                      <div className="mt-2 flex items-center gap-3 rounded-xl border border-slate-200 bg-white p-3">
                                        <div className="flex h-14 w-14 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-slate-100 bg-slate-50">
                                          <img
                                            src={value || '/assets/logo-2.jpg'}
                                            alt="Prévia do logo"
                                            className="h-full w-full object-contain p-1"
                                          />
                                        </div>
                                        <div className="min-w-0">
                                          <p className="text-xs font-black uppercase tracking-[0.14em] text-slate-500">Prévia do logo</p>
                                          <p className="mt-1 truncate text-xs font-semibold text-slate-400">{value || '/assets/logo-2.jpg'}</p>
                                        </div>
                                      </div>
                                    )}
                                    {isJson ? (
                                      <textarea
                                        value={value}
                                        onChange={(event) => setSiteConfigForm((current) => ({ ...current, [key]: event.target.value }))}
                                        className="mt-2 min-h-36 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 font-mono text-xs font-semibold leading-6 outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                                      />
                                    ) : (
                                      <input
                                        type={field?.type === 'number' ? 'number' : field?.type === 'color' ? 'color' : 'text'}
                                        min={field?.type === 'number' ? field?.min : undefined}
                                        max={field?.type === 'number' ? field?.max : undefined}
                                        value={value}
                                        onChange={(event) => setSiteConfigForm((current) => ({ ...current, [key]: event.target.value }))}
                                        className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                                      />
                                    )}
                                    <span className="mt-2 block text-xs font-semibold text-slate-400">{field?.description}</span>
                                    {isJson && (
                                      <span className="mt-1 block text-[11px] font-medium text-slate-500">
                                        Cada JSON fica salvo em uma chave própria na tabela `configuracoes_sistema`.
                                      </span>
                                    )}
                                  </label>
                                );
                              })}
                            </div>
                          </section>
                        ))}
                      </div>
                      {siteConfigStatus.message && (
                        <div
                          className={`mt-6 rounded-lg border px-4 py-3 text-sm font-bold ${siteConfigStatus.type === 'success' ? 'border-emerald-200 bg-emerald-50 text-emerald-800' : 'border-red-200 bg-red-50 text-red-800'}`}
                          role={siteConfigStatus.type === 'error' ? 'alert' : 'status'}
                          aria-live="polite"
                        >
                          {siteConfigStatus.message}
                        </div>
                      )}
                      <button
                        type="submit"
                        disabled={siteConfigSaving}
                        className="mt-6 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white transition disabled:cursor-wait disabled:opacity-60"
                      >
                        <Save size={17} />
                        {siteConfigSaving ? 'Salvando e confirmando no banco...' : 'Salvar configuração pública'}
                      </button>
                    </form>
                    <div className="space-y-6">
                      <SiteMediaLibrary
                        logoUrl={siteConfigForm.site_logo}
                        heroSlidesJson={siteConfigForm.home_hero_slides}
                        onLogoChange={(url) => setSiteConfigForm((current) => ({ ...current, site_logo: url }))}
                        onHeroSlidesChange={(value) => setSiteConfigForm((current) => ({ ...current, home_hero_slides: value }))}
                        onSaveHeroSlides={(value) => persistSiteConfiguracoes({ home_hero_slides: value })}
                        savingHeroSlides={siteConfigSaving}
                      />
                      <SitePublicProfilesEditor />
                      <section className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
                        <div className="border-b border-slate-100 px-6 py-5">
                          <p className="text-xs font-black uppercase tracking-[0.18em] text-[#C9A227]">Prévia da home</p>
                          <h3 className="mt-2 text-lg font-black text-slate-950">O que vai aparecer para o cliente</h3>
                        </div>
                        <div className="space-y-4 p-6">
                          <div className="flex items-center gap-3 rounded-2xl border border-slate-100 bg-slate-50 p-4">
                            <div className="flex h-16 w-16 items-center justify-center overflow-hidden rounded-2xl border border-slate-200 bg-white">
                              <img
                                src={siteConfigForm.site_logo || '/assets/logo-2.jpg'}
                                alt="Logo do site"
                                className="h-full w-full object-contain p-2"
                              />
                            </div>
                            <div className="min-w-0">
                              <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">{siteConfigForm.site_nome || 'Grupo Nexum Altivon'}</p>
                              <p className="mt-1 truncate text-sm font-semibold text-slate-600">{siteConfigForm.site_url || 'https://www.nexumaltivon.com.br'}</p>
                            </div>
                          </div>
                          <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                            <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Banner</p>
                            <p className="mt-2 text-sm font-semibold text-slate-700">{siteConfigForm.home_intro_titulo || 'Uma Nova Era Começa'}</p>
                            <p className="mt-1 text-sm text-slate-500">{siteConfigForm.home_intro_badge || 'www.nexumaltivon.com.br'}</p>
                          </div>
                          <div className="grid gap-3 sm:grid-cols-2">
                            <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                              <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Slides</p>
                              <p className="mt-2 text-3xl font-black text-slate-950">{getJsonArrayCount(siteConfigForm.home_hero_slides)}</p>
                            </div>
                            <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                              <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Qualidade</p>
                              <p className="mt-2 text-3xl font-black text-slate-950">{getJsonArrayCount(siteConfigForm.home_quality_items)}</p>
                            </div>
                          </div>
                        </div>
                      </section>
                      <ErpChecklistCard
                        title="Persistência operacional do portal"
                        items={[
                          'Configurações públicas são gravadas e relidas da tabela configuracoes_sistema.',
                          'Metadados das imagens são persistidos na tabela site_midias com tenant, concorrência e exclusão lógica.',
                          'Arquivos validados são gravados no wwwroot oficial e servidos pelo mesmo processo da API na porta 5010.',
                          'Logo e banners só são publicados depois do salvamento confirmado pela API.',
                        ]}
                      />
                      <CompactList title="Chaves carregadas do banco" items={siteConfigItems} fields={['chave', 'grupo', 'tipo', 'updatedAt']} />
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
    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <p className="text-xs font-black uppercase tracking-[0.2em] text-[#C9A227]">{eyebrow}</p>
      <h2 className="mt-2 text-2xl font-black text-slate-950">{title}</h2>
      <p className="mt-1 max-w-3xl text-sm text-slate-500">{description}</p>
    </section>
  );
}

function ErpChecklistCard({ title, items }) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
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
    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
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

function ProductPreview({ produto, categorias }) {
  const categoriaSelecionada = produto.subcategoriaId || produto.categoriaId;
  const categoria = categorias.find((item) => item.id === categoriaSelecionada)?.caminho
    || categorias.find((item) => item.id === categoriaSelecionada)?.nome
    || categorias.find((item) => item.id === produto.categoriaId)?.nome
    || 'Sem categoria';
  const image = produto.imagemUrl || 'https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85';
  const price = Number(produto.preco || 0);
  const promotional = Number(produto.precoPromocional || 0);

  return (
    <section className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="aspect-[16/10] bg-slate-100">
        <img src={image} alt={produto.nome || 'Prévia do produto'} className="h-full w-full object-cover" />
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
  const [marketplaceSyncing, setMarketplaceSyncing] = useState(false);
  const [marketplaceResult, setMarketplaceResult] = useState(null);
  const [marketplaceForm, setMarketplaceForm] = useState({ canal: 'mercadolivre', direcao: 'pull', entidade: 'pedidos', limite: '50' });

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
  const isMarketplaceWorkspace = integracao.slug === 'mercadolivre' || integracao.slug === 'marketplaces';
  const isDropshippingWorkspace = ['dropshipping', 'shopify', 'cjdropshipping'].includes(integracao.slug);

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
          {isDropshippingWorkspace && <DropshippingAdminPanel />}
          {isMarketplaceWorkspace && (
            <form onSubmit={runMarketplaceSync} className="mt-6 rounded-lg border border-slate-200 bg-slate-50 p-5">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <p className="text-xs font-black uppercase tracking-[0.16em] text-slate-500">Operação externa controlada</p>
                  <h4 className="mt-1 text-lg font-black text-slate-950">Sincronizar marketplace</h4>
                  <p className="mt-1 text-sm font-semibold text-slate-600">A API só confirma sucesso depois da resposta aceita pelo seller externo.</p>
                </div>
                <button disabled={marketplaceSyncing} className="inline-flex h-11 items-center justify-center rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:opacity-60">
                  {marketplaceSyncing ? 'Executando...' : 'Executar sincronização'}
                </button>
              </div>
              <div className="mt-5 grid gap-4 md:grid-cols-4">
                <SelectField label="Canal" value={marketplaceForm.canal} onChange={(value) => setMarketplaceForm((current) => ({ ...current, canal: value }))} options={['mercadolivre', 'b2w', 'via']} />
                <SelectField label="Direção" value={marketplaceForm.direcao} onChange={(value) => setMarketplaceForm((current) => ({ ...current, direcao: value }))} options={['pull', 'push', 'bidirecional']} />
                <SelectField label="Entidade" value={marketplaceForm.entidade} onChange={(value) => setMarketplaceForm((current) => ({ ...current, entidade: value }))} options={['pedidos', 'produtos', 'catalogo', 'todos']} />
                <Field label="Limite" type="number" value={marketplaceForm.limite} onChange={(value) => setMarketplaceForm((current) => ({ ...current, limite: value }))} required />
              </div>
              {marketplaceResult && (
                <div className={`mt-5 rounded-lg border p-4 ${marketplaceResult.apiSuccess ? 'border-emerald-200 bg-emerald-50 text-emerald-950' : 'border-red-200 bg-red-50 text-red-950'}`}>
                  <div className="flex flex-wrap items-center gap-3">
                    <strong>HTTP {marketplaceResult.httpStatus}</strong>
                    <span>{marketplaceResult.canal || marketplaceForm.canal}</span>
                    <span>{marketplaceResult.operacional ? 'Operação externa confirmada' : 'Operação não confirmada'}</span>
                  </div>
                  {marketplaceResult.message && <p className="mt-2 text-sm font-semibold">{marketplaceResult.message}</p>}
                  <div className="mt-3 grid gap-2 text-sm font-bold sm:grid-cols-3">
                    <span>Locais elegíveis: {marketplaceResult.produtosLocaisElegiveis ?? 0}</span>
                    <span>Externos lidos: {marketplaceResult.registrosExternosLidos ?? 0}</span>
                    <span>Externos enviados: {marketplaceResult.produtosExternosEnviados ?? 0}</span>
                  </div>
                  {Array.isArray(marketplaceResult.pendencias) && marketplaceResult.pendencias.length > 0 && (
                    <div className="mt-3 space-y-1">{marketplaceResult.pendencias.map((item) => <p key={item} className="text-sm font-semibold">{item}</p>)}</div>
                  )}
                </div>
              )}
            </form>
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

function CompactList({ title, items, fields }) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-5 py-4">
        <h3 className="font-black text-slate-950">{title}</h3>
        <p className="mt-1 text-sm text-slate-500">{items.length} registros</p>
      </div>
      <div className="max-h-96 divide-y divide-slate-100 overflow-auto">
        {items.slice(0, 12).map((item) => (
          <div key={item.id || item.sku || item.email} className="px-5 py-4">
            <p className="font-black text-slate-950">{item[fields[0]] || '-'}</p>
            <p className="mt-1 text-sm font-semibold text-slate-500">{fields.slice(1).map((field) => item[field] || '-').join(' · ')}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

function OrdersTable({ pedidos, onStatusChange }) {
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

  const runMarketplaceSync = async (event) => {
    event.preventDefault();
    setMarketplaceSyncing(true);
    setMarketplaceResult(null);
    try {
      const response = await integracoesAPI.sincronizarMarketplace(marketplaceForm.canal, {
        direcao: marketplaceForm.direcao,
        entidade: marketplaceForm.entidade,
        limite: Number(marketplaceForm.limite),
        produto_ids: null,
        categoria_externa: null,
        forcar: false,
      });
      setMarketplaceResult({ ...response.data, httpStatus: response.status, apiSuccess: true });
    } catch (error) {
      const envelope = error.response?.data || {};
      const result = envelope.dados || envelope.Dados || {};
      setMarketplaceResult({
        ...result,
        apiSuccess: false,
        httpStatus: error.response?.status || 0,
        message: envelope.mensagem || error.message || 'Sincronização não executada.',
        pendencias: result.pendencias || envelope.erros || ['A API não confirmou operação externa.'],
      });
    } finally {
      setMarketplaceSyncing(false);
    }
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
        aria-label="URLs da galeria, uma por linha"
        className="mt-4 min-h-24 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
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

