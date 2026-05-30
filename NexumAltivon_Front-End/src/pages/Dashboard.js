import { useEffect, useMemo, useState, useCallback } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { dashboardAPI, leadAPI, pedidoAPI } from '../services/api';
import { fallbackLeads, fallbackPedidos, fallbackResumo } from '../data/mockStore';
import { formatDate, formatPrice, getLeadStatusClass, getPedidoStatusClass } from '../utils/formatters';
import {
  Activity,
  ArrowDownRight,
  ArrowUpRight,
  Bell,
  Boxes,
  ChevronRight,
  CreditCard,
  LayoutDashboard,
  LogOut,
  PackageCheck,
  Search,
  ShoppingBag,
  Sparkles,
  TrendingUp,
  UserRound,
  Users,
} from 'lucide-react';

const tabs = [
  { id: 'overview', label: 'Visão geral', icon: LayoutDashboard },
  { id: 'pedidos', label: 'Pedidos', icon: ShoppingBag },
  { id: 'crm', label: 'CRM', icon: Users },
];

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
  const [resumo, setResumo] = useState(fallbackResumo);
  const [pedidos, setPedidos] = useState(fallbackPedidos);
  const [leads, setLeads] = useState(fallbackLeads);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');
  const [query, setQuery] = useState('');

  const loadData = useCallback(async () => {
    try {
      const [resumoRes, pedidosRes, leadsRes] = await Promise.all([
        dashboardAPI.getResumo(),
        pedidoAPI.getAll(),
        leadAPI.getAll(),
      ]);
      if (resumoRes.data) setResumo({ ...fallbackResumo, ...resumoRes.data });
      if (Array.isArray(pedidosRes.data) && pedidosRes.data.length > 0) setPedidos(pedidosRes.data);
      if (Array.isArray(leadsRes.data) && leadsRes.data.length > 0) setLeads(leadsRes.data);
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
      <div className="flex min-h-screen items-center justify-center bg-[#eef2f7]">
        <div className="h-12 w-12 animate-spin rounded-full border-4 border-slate-200 border-t-slate-950" />
      </div>
    );
  }

  if (!isAuthenticated) return <Navigate to="/login" />;

  return (
    <main className="min-h-screen bg-[#eef2f7]">
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-72 border-r border-slate-200 bg-slate-950 text-white lg:block">
        <div className="flex h-full flex-col p-6">
          <Link to="/" className="flex items-center gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-amber-400 text-sm font-black text-slate-950">NA</div>
            <div>
              <p className="font-black">Nexum Altivon</p>
              <p className="text-xs font-bold uppercase tracking-[0.18em] text-slate-400">Control Center</p>
            </div>
          </Link>

          <nav className="mt-10 space-y-2">
            {tabs.map((tab) => {
              const Icon = tab.icon;
              const active = activeTab === tab.id;
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex w-full items-center gap-3 rounded-lg px-4 py-3 text-left text-sm font-black transition ${
                    active ? 'bg-white text-slate-950' : 'text-slate-300 hover:bg-white/10 hover:text-white'
                  }`}
                  data-testid={`tab-${tab.id}`}
                >
                  <Icon size={18} />
                  {tab.label}
                </button>
              );
            })}
          </nav>

          <div className="mt-auto rounded-lg border border-white/10 bg-white/5 p-4">
            <div className="flex items-center gap-2 text-amber-200">
              <Sparkles size={16} />
              <p className="text-xs font-black uppercase tracking-[0.18em]">Meta do mês</p>
            </div>
            <p className="mt-3 text-2xl font-black">{formatPrice(resumo.faturamento_mes)}</p>
            <div className="mt-4 h-2 overflow-hidden rounded-full bg-white/10">
              <div className="h-full w-[72%] rounded-full bg-amber-400" />
            </div>
            <p className="mt-3 text-xs font-semibold text-slate-300">72% da meta comercial concluída.</p>
          </div>
        </div>
      </aside>

      <section className="lg:pl-72">
        <header className="sticky top-0 z-30 border-b border-slate-200 bg-white/92 backdrop-blur-xl">
          <div className="flex min-h-[76px] flex-col gap-4 px-4 py-4 sm:px-6 xl:flex-row xl:items-center xl:justify-between xl:px-8">
            <div>
              <p className="text-sm font-bold text-slate-500">Olá, {user?.nome || 'gestor'}.</p>
              <h1 className="text-2xl font-black text-slate-950" data-testid="dashboard-title">Painel de controle</h1>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="relative">
                <Search className="absolute left-3 top-3 text-slate-400" size={18} />
                <input
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="Buscar pedidos, clientes ou leads"
                  className="h-11 w-full rounded-full border border-slate-200 bg-white pl-10 pr-4 text-sm font-semibold outline-none transition focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10 sm:w-80"
                />
              </div>
              <button className="inline-flex h-11 w-11 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-700" aria-label="Notificações" title="Notificações">
                <Bell size={19} />
              </button>
              <button
                onClick={logout}
                className="inline-flex h-11 items-center justify-center gap-2 rounded-full bg-slate-950 px-5 text-sm font-black text-white"
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
                    <OrdersTable pedidos={pedidos.slice(0, 5)} />
                  </section>
                </div>
              )}

              {activeTab === 'pedidos' && (
                <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
                  <div className="border-b border-slate-200 px-6 py-5">
                    <h2 className="text-xl font-black text-slate-950">Gestão de pedidos</h2>
                    <p className="mt-1 text-sm text-slate-500">{filteredPedidos.length} pedidos encontrados.</p>
                  </div>
                  <OrdersTable pedidos={filteredPedidos} />
                </section>
              )}

              {activeTab === 'crm' && (
                <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
                  <div className="border-b border-slate-200 px-6 py-5">
                    <h2 className="text-xl font-black text-slate-950">Pipeline CRM</h2>
                    <p className="mt-1 text-sm text-slate-500">{filteredLeads.length} oportunidades em acompanhamento.</p>
                  </div>
                  <LeadsTable leads={filteredLeads} />
                </section>
              )}
            </>
          )}
        </div>
      </section>
    </main>
  );
}

function OrdersTable({ pedidos }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px]">
        <thead className="bg-slate-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Pedido</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Total</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Status</th>
            <th className="px-6 py-3 text-left text-xs font-black uppercase tracking-wide text-slate-500">Data</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {pedidos.length === 0 ? (
            <tr><td colSpan="4" className="px-6 py-8 text-center text-slate-500">Nenhum pedido encontrado</td></tr>
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
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

function LeadsTable({ leads }) {
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
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {leads.length === 0 ? (
            <tr><td colSpan="5" className="px-6 py-8 text-center text-slate-500">Nenhum lead encontrado</td></tr>
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
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
