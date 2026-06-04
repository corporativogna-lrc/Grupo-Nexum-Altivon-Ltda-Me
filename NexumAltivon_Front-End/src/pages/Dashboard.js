import { useEffect, useMemo, useState, useCallback } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { categoriaAPI, clienteAPI, dashboardAPI, fornecedorAPI, leadAPI, pedidoAPI, produtoAPI } from '../services/api';
import { fallbackCategories, fallbackLeads, fallbackPedidos, fallbackProducts, fallbackResumo } from '../data/mockStore';
import { formatDate, formatPrice, getLeadStatusClass, getPedidoStatusClass } from '../utils/formatters';
import {
  Activity,
  ArrowDownRight,
  ArrowUpRight,
  Bell,
  Boxes,
  Building2,
  ChevronRight,
  Cog,
  CreditCard,
  Database,
  FileText,
  Handshake,
  PackagePlus,
  LayoutDashboard,
  LogOut,
  PackageCheck,
  Save,
  Search,
  ShoppingBag,
  Sparkles,
  TrendingUp,
  UserRound,
  Users,
  WalletCards,
} from 'lucide-react';

const tabs = [
  { id: 'overview', label: 'Dashboard', icon: LayoutDashboard, section: 'Principal' },
  { id: 'pedidos', label: 'Pedidos', icon: ShoppingBag, section: 'Principal' },
  { id: 'cadastros', label: 'Produtos / Cadastros', icon: PackagePlus, section: 'Principal', badge: 'novo' },
  { id: 'crm', label: 'CRM', icon: Users, section: 'Marketing & CRM' },
];

const plannedModules = [
  { label: 'Lojas', icon: Building2, section: 'Gestão' },
  { label: 'Financeiro / Gateways', icon: WalletCards, section: 'Gestão' },
  { label: 'Fiscal', icon: FileText, section: 'Gestão' },
  { label: 'Logística', icon: Boxes, section: 'Gestão' },
  { label: 'Cupons', icon: CreditCard, section: 'Marketing & CRM' },
  { label: 'Marketing', icon: TrendingUp, section: 'Marketing & CRM' },
  { label: 'API / Marketplaces', icon: Database, section: 'Integrações' },
  { label: 'Dropshipping', icon: Handshake, section: 'Integrações' },
  { label: 'Configurações', icon: Cog, section: 'Sistema' },
];

const navSections = ['Principal', 'Gestão', 'Marketing & CRM', 'Integrações', 'Sistema'];

const fallbackClientes = [
  { id: 1, nome: 'Ana Carolina Silva', email: 'ana.silva@email.com', telefone: '(14) 99876-5432' },
  { id: 2, nome: 'Bruno Oliveira', email: 'bruno.oliveira@email.com', telefone: '(14) 99765-4321' },
  { id: 3, nome: 'Carla Mendes', email: 'carla.mendes@email.com', telefone: '(14) 99654-3210' },
];

const fallbackFornecedores = [
  { id: 1, nome: 'Chronos Imports', documento: '12.345.678/0001-90', email: 'comercial@chronosimports.com', telefone: '(11) 3030-1122', categoria: 'Relogios' },
  { id: 2, nome: 'Luxury Cases Brasil', documento: '98.765.432/0001-10', email: 'vendas@luxurycases.com', telefone: '(21) 4040-2211', categoria: 'Acessorios' },
];

const emptyProduto = {
  nome: '',
  descricao: '',
  preco: '',
  precoPromocional: '',
  imagemUrl: '',
  estoque: '',
  destaque: true,
  sku: '',
  categoriaId: 'classicos',
};

const emptyCliente = { nome: '', email: '', telefone: '', cpf: '' };
const emptyFornecedor = { nome: '', documento: '', email: '', telefone: '', categoria: 'Geral' };
const emptyLead = { nome: '', email: '', telefone: '', status: 'Novo', origem: 'Site', observacao: '' };
const pedidoStatusOptions = ['Pendente', 'Processando', 'Enviado', 'Entregue', 'Cancelado'];
const leadStatusOptions = ['Novo', 'Contato', 'Qualificado', 'Negociacao', 'Ganho', 'Perdido'];
const allowDemoData = process.env.NODE_ENV !== 'production';
const emptyResumo = {
  pedidos_hoje: 0,
  total_clientes: 0,
  faturamento_mes: 0,
  leads_novos: 0,
  produtos_estoque_baixo: 0,
  conversao: 0,
  ticket_medio: 0,
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

function StatCard({ title, value, detail, icon: Icon, trend, tone = 'slate' }) {
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
      <div className="mt-5 flex items-center gap-2 text-sm font-bold text-slate-500">
        {trend >= 0 ? <ArrowUpRight className="text-emerald-600" size={17} /> : <ArrowDownRight className="text-rose-600" size={17} />}
        <span className={trend >= 0 ? 'text-emerald-700' : 'text-rose-700'}>{Math.abs(trend)}%</span>
        <span>{detail}</span>
      </div>
    </div>
  );
}

export default function Dashboard() {
  const { user, isAuthenticated, loading: authLoading, logout } = useAuth();
  const [resumo, setResumo] = useState(allowDemoData ? fallbackResumo : emptyResumo);
  const [pedidos, setPedidos] = useState(allowDemoData ? fallbackPedidos : []);
  const [leads, setLeads] = useState(allowDemoData ? fallbackLeads : []);
  const [produtos, setProdutos] = useState(allowDemoData ? fallbackProducts : []);
  const [categorias, setCategorias] = useState(allowDemoData ? fallbackCategories : []);
  const [clientes, setClientes] = useState(allowDemoData ? fallbackClientes : []);
  const [fornecedores, setFornecedores] = useState(allowDemoData ? fallbackFornecedores : []);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');
  const [query, setQuery] = useState('');
  const [produtoForm, setProdutoForm] = useState(emptyProduto);
  const [clienteForm, setClienteForm] = useState(emptyCliente);
  const [fornecedorForm, setFornecedorForm] = useState(emptyFornecedor);
  const [leadForm, setLeadForm] = useState(emptyLead);
  const [formStatus, setFormStatus] = useState('');

  const loadData = useCallback(async () => {
    try {
      const [resumoRes, pedidosRes, leadsRes, produtosRes, categoriasRes, clientesRes, fornecedoresRes] = await Promise.all([
        dashboardAPI.getResumo(),
        pedidoAPI.getAll(),
        leadAPI.getAll(),
        produtoAPI.getAll(),
        categoriaAPI.getAll(),
        clienteAPI.getAll(),
        fornecedorAPI.getAll(),
      ]);
      if (resumoRes.data) setResumo({ ...fallbackResumo, ...resumoRes.data });
      if (Array.isArray(pedidosRes.data) && pedidosRes.data.length > 0) setPedidos(pedidosRes.data);
      if (Array.isArray(leadsRes.data) && leadsRes.data.length > 0) setLeads(leadsRes.data);
      if (Array.isArray(produtosRes.data) && produtosRes.data.length > 0) setProdutos(produtosRes.data);
      if (Array.isArray(categoriasRes.data) && categoriasRes.data.length > 0) setCategorias(categoriasRes.data);
      if (Array.isArray(clientesRes.data) && clientesRes.data.length > 0) setClientes(clientesRes.data);
      if (Array.isArray(fornecedoresRes.data) && fornecedoresRes.data.length > 0) setFornecedores(fornecedoresRes.data);
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

  const submitProduto = async (event) => {
    event.preventDefault();
    setFormStatus('');

    const payload = {
      ...produtoForm,
      preco: Number(produtoForm.preco),
      precoPromocional: produtoForm.precoPromocional ? Number(produtoForm.precoPromocional) : null,
      estoque: Number(produtoForm.estoque),
      categoriaId: produtoForm.categoriaId,
    };

    const response = await produtoAPI.create(payload);
    setProdutos((current) => [response.data, ...current]);
    setProdutoForm(emptyProduto);
    setFormStatus('Produto cadastrado e disponível no catálogo.');
  };

  const submitCliente = async (event) => {
    event.preventDefault();
    setFormStatus('');
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
    <main className="nexum-admin-shell min-h-screen bg-[#050505]">
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-72 border-r border-[#2A2A2A] bg-[#0A0A0A] text-white lg:block">
        <div className="flex h-full flex-col p-6">
          <Link to="/" className="flex items-center gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-[#C9A227] text-sm font-black text-black">NA</div>
            <div>
              <p className="font-serif text-lg font-black tracking-widest text-[#C9A227]">NEXUM ALTIVON</p>
              <p className="text-xs font-bold uppercase tracking-[0.18em] text-zinc-500">Painel Administrativo</p>
            </div>
          </Link>

          <nav className="mt-8 space-y-5 overflow-y-auto pb-6">
            {navSections.map((section) => {
              const activeItems = tabs.filter((tab) => tab.section === section);
              const futureItems = plannedModules.filter((module) => module.section === section);
              if (activeItems.length === 0 && futureItems.length === 0) return null;

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
                          onClick={() => setActiveTab(tab.id)}
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
                    {futureItems.map((item) => {
                      const Icon = item.icon;
                      return (
                        <button
                          key={item.label}
                          type="button"
                          className="flex w-full cursor-not-allowed items-center gap-3 border-l-4 border-transparent px-3 py-3 text-left text-sm font-semibold text-zinc-700"
                          title="Módulo em estruturação"
                        >
                          <Icon size={18} />
                          <span>{item.label}</span>
                          <span className="ml-auto rounded-full border border-zinc-700 px-2 py-0.5 text-[0.62rem] uppercase text-zinc-600">breve</span>
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
            <div className="mt-4 h-2 overflow-hidden rounded-full bg-white/10">
              <div className="h-full w-[72%] rounded-full bg-[#C9A227]" />
            </div>
            <p className="mt-3 text-xs font-semibold text-zinc-400">Base operacional conectada à API e ao banco real.</p>
          </div>
        </div>
      </aside>

      <section className="lg:pl-72">
        <header className="sticky top-0 z-30 border-b border-[#2A2A2A] bg-[#0A0A0A]/95 text-white backdrop-blur-xl">
          <div className="flex min-h-[76px] flex-col gap-4 px-4 py-4 sm:px-6 xl:flex-row xl:items-center xl:justify-between xl:px-8">
            <div>
              <p className="text-sm font-bold text-zinc-500"><span className="text-[#C9A227]">{tabs.find((tab) => tab.id === activeTab)?.label || 'Dashboard'}</span> / Gestão</p>
              <h1 className="text-2xl font-black text-white" data-testid="dashboard-title">Painel Administrativo</h1>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="relative">
                <Search className="absolute left-3 top-3 text-zinc-500" size={18} />
                <input
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="Buscar pedidos, clientes, produtos ou leads"
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
                  onClick={() => setActiveTab(tab.id)}
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
              {activeTab === 'overview' && (
                <div className="space-y-6">
                  <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
                    <StatCard title="Pedidos hoje" value={resumo.pedidos_hoje} detail="vs. ontem" trend={12} icon={ShoppingBag} tone="slate" />
                    <StatCard title="Clientes" value={resumo.total_clientes} detail="base ativa" trend={8} icon={Users} tone="emerald" />
                    <StatCard title="Faturamento" value={formatPrice(resumo.faturamento_mes)} detail="no mês" trend={18} icon={TrendingUp} tone="amber" />
                    <StatCard title="Leads novos" value={resumo.leads_novos} detail="em qualificação" trend={-3} icon={UserRound} tone="indigo" />
                  </div>

                  <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                      <div>
                        <h2 className="text-xl font-black text-slate-950">Arquitetura operacional integrada</h2>
                        <p className="mt-1 text-sm text-slate-500">
                          E-commerce, dropshipping, logística, gateways de pagamento e módulos empresariais conectados pelo canal de API.
                        </p>
                      </div>
                      <span className="rounded-full bg-emerald-50 px-4 py-2 text-sm font-black text-emerald-800">API em produção temporária</span>
                    </div>
                    <div className="mt-6 grid gap-3 md:grid-cols-2 xl:grid-cols-5">
                      {[
                        { label: 'E-commerce', status: 'Operante', icon: ShoppingBag },
                        { label: 'Cadastros reais', status: 'Em uso', icon: PackagePlus },
                        { label: 'Dropshipping', status: 'Estruturando', icon: Handshake },
                        { label: 'Logística', status: 'Estruturando', icon: Boxes },
                        { label: 'Gateways/API', status: 'Estruturando', icon: Database },
                      ].map((item) => {
                        const Icon = item.icon;
                        return (
                          <div key={item.label} className="rounded-lg border border-slate-100 bg-slate-50 p-4">
                            <Icon className="text-amber-600" size={22} />
                            <p className="mt-3 text-sm font-black text-slate-950">{item.label}</p>
                            <p className="mt-1 text-xs font-bold uppercase tracking-wide text-slate-500">{item.status}</p>
                          </div>
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
                      <button onClick={() => setActiveTab('pedidos')} className="inline-flex items-center gap-1 text-sm font-black text-slate-950">
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
                    <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                      <div>
                        <h2 className="text-xl font-black text-slate-950">Cadastros operacionais</h2>
                        <p className="mt-1 text-sm text-slate-500">Produtos, clientes e fornecedores prontos para uso no painel.</p>
                      </div>
                      {formStatus && <span className="rounded-full bg-emerald-50 px-4 py-2 text-sm font-black text-emerald-800">{formStatus}</span>}
                    </div>
                  </div>

                  <div className="grid gap-6 xl:grid-cols-[minmax(0,1.3fr)_minmax(340px,0.7fr)]">
                    <form onSubmit={submitProduto} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
                      <h3 className="text-lg font-black text-slate-950">Cadastro de produto</h3>
                      <div className="mt-5 grid gap-4 md:grid-cols-2">
                        <Field label="Nome" value={produtoForm.nome} onChange={(value) => setProdutoForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="SKU" value={produtoForm.sku} onChange={(value) => setProdutoForm((form) => ({ ...form, sku: value }))} />
                        <Field label="Preco" type="number" value={produtoForm.preco} onChange={(value) => setProdutoForm((form) => ({ ...form, preco: value }))} required />
                        <Field label="Preco promocional" type="number" value={produtoForm.precoPromocional} onChange={(value) => setProdutoForm((form) => ({ ...form, precoPromocional: value }))} />
                        <Field label="Estoque" type="number" value={produtoForm.estoque} onChange={(value) => setProdutoForm((form) => ({ ...form, estoque: value }))} required />
                        <label className="block text-sm font-bold text-slate-700">
                          Categoria
                          <select
                            value={produtoForm.categoriaId}
                            onChange={(event) => setProdutoForm((form) => ({ ...form, categoriaId: event.target.value }))}
                            className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                          >
                            {categorias.map((categoria) => (
                              <option key={categoria.id} value={categoria.id}>{categoria.nome}</option>
                            ))}
                          </select>
                        </label>
                        <label className="block text-sm font-bold text-slate-700 md:col-span-2">
                          Descricao
                          <textarea
                            value={produtoForm.descricao}
                            onChange={(event) => setProdutoForm((form) => ({ ...form, descricao: event.target.value }))}
                            className="mt-2 min-h-24 w-full rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10"
                          />
                        </label>
                        <Field label="Imagem URL" value={produtoForm.imagemUrl} onChange={(value) => setProdutoForm((form) => ({ ...form, imagemUrl: value }))} className="md:col-span-2" />
                        <label className="flex items-center gap-3 rounded-lg border border-slate-200 px-3 py-3 text-sm font-bold text-slate-700">
                          <input
                            type="checkbox"
                            checked={produtoForm.destaque}
                            onChange={(event) => setProdutoForm((form) => ({ ...form, destaque: event.target.checked }))}
                            className="h-5 w-5 accent-slate-950"
                          />
                          Produto em destaque
                        </label>
                      </div>
                      <button className="mt-5 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white">
                        <Save size={17} />
                        Salvar produto
                      </button>
                    </form>

                    <div className="space-y-6">
                      <SimpleForm title="Cadastro de cliente" onSubmit={submitCliente} buttonLabel="Salvar cliente">
                        <Field label="Nome" value={clienteForm.nome} onChange={(value) => setClienteForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="Email" type="email" value={clienteForm.email} onChange={(value) => setClienteForm((form) => ({ ...form, email: value }))} required />
                        <Field label="Telefone" value={clienteForm.telefone} onChange={(value) => setClienteForm((form) => ({ ...form, telefone: value }))} />
                        <Field label="CPF/CNPJ" value={clienteForm.cpf} onChange={(value) => setClienteForm((form) => ({ ...form, cpf: value }))} />
                      </SimpleForm>

                      <SimpleForm title="Cadastro de fornecedor" onSubmit={submitFornecedor} buttonLabel="Salvar fornecedor">
                        <Field label="Nome" value={fornecedorForm.nome} onChange={(value) => setFornecedorForm((form) => ({ ...form, nome: value }))} required />
                        <Field label="Documento" value={fornecedorForm.documento} onChange={(value) => setFornecedorForm((form) => ({ ...form, documento: value }))} />
                        <Field label="Email" type="email" value={fornecedorForm.email} onChange={(value) => setFornecedorForm((form) => ({ ...form, email: value }))} />
                        <Field label="Telefone" value={fornecedorForm.telefone} onChange={(value) => setFornecedorForm((form) => ({ ...form, telefone: value }))} />
                        <Field label="Categoria" value={fornecedorForm.categoria} onChange={(value) => setFornecedorForm((form) => ({ ...form, categoria: value }))} />
                      </SimpleForm>
                    </div>
                  </div>

                  <div className="grid gap-6 xl:grid-cols-3">
                    <CompactList title="Produtos cadastrados" items={produtos} fields={['nome', 'sku', 'estoque']} />
                    <CompactList title="Clientes cadastrados" items={clientes} fields={['nome', 'email', 'telefone']} />
                    <CompactList title="Fornecedores cadastrados" items={fornecedores} fields={['nome', 'categoria', 'telefone']} />
                  </div>
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
            </>
          )}
        </div>
      </section>
    </main>
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

function SimpleForm({ title, onSubmit, buttonLabel, children }) {
  return (
    <form onSubmit={onSubmit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h3 className="text-lg font-black text-slate-950">{title}</h3>
      <div className="mt-5 grid gap-4">{children}</div>
      <button className="mt-5 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white">
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
      <table className="w-full min-w-[720px]">
        <thead className="bg-slate-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Pedido</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Total</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Status</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Data</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Ação</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {pedidos.length === 0 ? (
            <tr><td colSpan="5" className="px-6 py-8 text-center text-slate-500">Nenhum pedido encontrado</td></tr>
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
