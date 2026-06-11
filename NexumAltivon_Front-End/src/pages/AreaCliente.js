import { useEffect, useMemo, useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { AlertCircle, BadgeDollarSign, FileText, LifeBuoy, LoaderCircle, Receipt, ShoppingBag, Star, UserCircle2 } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { clienteAPI } from '../services/api';

const formatCurrency = (value) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(value || 0));

const formatDate = (value) =>
  value
    ? new Intl.DateTimeFormat('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      }).format(new Date(value))
    : '-';

export default function AreaCliente() {
  const { isAuthenticated, user, isAdmin } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [portal, setPortal] = useState(null);

  useEffect(() => {
    if (!isAuthenticated || isAdmin) {
      setLoading(false);
      return;
    }

    const loadPortal = async () => {
      try {
        setLoading(true);
        const response = await clienteAPI.getPortal();
        const payload = response.data?.dados || response.data?.Dados || response.data?.data || response.data;
        setPortal(payload);
      } catch (requestError) {
        setError(
          requestError.response?.data?.detail ||
            requestError.response?.data?.mensagem ||
            'Não foi possível carregar sua área do cliente agora.',
        );
      } finally {
        setLoading(false);
      }
    };

    loadPortal();
  }, [isAuthenticated, isAdmin]);

  const stats = useMemo(() => {
    const pedidos = portal?.pedidos || [];
    const totalCompras = pedidos.reduce((sum, item) => sum + Number(item.valorTotal || 0), 0);
    const pontos = Math.round(totalCompras / 10);
    const score = pedidos.length >= 5 ? 'Premium' : pedidos.length >= 2 ? 'Em evolução' : 'Inicial';
    const limiteFuturo = totalCompras >= 2000 ? 'Elegível para análise futura' : 'Em observação';

    return {
      totalCompras,
      pontos,
      score,
      limiteFuturo,
    };
  }, [portal]);

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (isAdmin) return <Navigate to="/dashboard" replace />;

  return (
    <main className="min-h-screen bg-[#050505] px-4 py-10 text-white sm:px-6 lg:px-8">
      <div className="mx-auto max-w-7xl space-y-8">
        <section className="rounded-[32px] border border-white/10 bg-gradient-to-br from-[#111111] to-[#1B1B1B] p-8 shadow-2xl">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.22em] text-[#E8D5A3]">Área do cliente Nexum Altivon</p>
              <h1 className="mt-3 text-4xl font-black">Olá, {portal?.nome || user?.nome || 'cliente'}.</h1>
              <p className="mt-3 max-w-2xl text-zinc-300">
                Aqui você acompanha pedidos, documentos, relacionamento e suporte. A Yara fica responsável pela recepção digital e pelo apoio inicial do seu atendimento.
              </p>
            </div>
            <div className="rounded-3xl border border-[#C9A227]/20 bg-black/30 p-5">
              <p className="text-xs font-black uppercase tracking-[0.18em] text-zinc-500">Cadastro principal</p>
              <p className="mt-2 text-lg font-bold">{portal?.email || user?.email}</p>
              <p className="mt-1 text-sm text-zinc-400">{portal?.telefone || 'Telefone em atualização'}</p>
              <p className="mt-4 text-xs font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Atendimento Yara</p>
              <a href="mailto:corporativo.gna@gmail.com" className="mt-2 inline-flex text-sm font-semibold text-white underline underline-offset-4">
                corporativo.gna@gmail.com
              </a>
            </div>
          </div>
        </section>

        {loading ? (
          <section className="flex min-h-[240px] items-center justify-center rounded-[32px] border border-white/10 bg-[#111111]">
            <div className="inline-flex items-center gap-3 text-zinc-300">
              <LoaderCircle className="animate-spin" size={20} />
              Carregando sua área do cliente...
            </div>
          </section>
        ) : error ? (
          <section className="rounded-[32px] border border-red-500/20 bg-red-500/10 p-6 text-red-100">
            <div className="inline-flex items-center gap-3">
              <AlertCircle size={20} />
              {error}
            </div>
          </section>
        ) : (
          <>
            <section className="grid gap-4 lg:grid-cols-4">
              {[
                { label: 'Pedidos registrados', value: portal?.pedidos?.length || 0, icon: ShoppingBag },
                { label: 'Total comprado', value: formatCurrency(stats.totalCompras), icon: BadgeDollarSign },
                { label: 'Pontuação', value: `${stats.pontos} pts`, icon: Star },
                { label: 'Score / limite', value: `${stats.score} • ${stats.limiteFuturo}`, icon: UserCircle2 },
              ].map((item) => {
                const Icon = item.icon;
                return (
                  <article key={item.label} className="rounded-[28px] border border-white/10 bg-[#111111] p-6">
                    <div className="inline-flex h-12 w-12 items-center justify-center rounded-2xl bg-[#C9A227] text-black">
                      <Icon size={20} />
                    </div>
                    <p className="mt-5 text-sm font-black uppercase tracking-[0.16em] text-zinc-500">{item.label}</p>
                    <p className="mt-3 text-xl font-black text-white">{item.value}</p>
                  </article>
                );
              })}
            </section>

            <section className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
              <article className="rounded-[32px] border border-white/10 bg-[#111111] p-6">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Histórico de compras</p>
                    <h2 className="mt-2 text-2xl font-black">Pedidos e status reais</h2>
                  </div>
                  <Link to="/produtos" className="rounded-full border border-white/10 px-4 py-2 text-sm font-bold text-zinc-100 transition hover:border-[#C9A227] hover:text-[#E8D5A3]">
                    Continuar comprando
                  </Link>
                </div>

                <div className="mt-6 overflow-hidden rounded-3xl border border-white/5">
                  <table className="min-w-full divide-y divide-white/5 text-sm">
                    <thead className="bg-black/30 text-zinc-400">
                      <tr>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Pedido</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Status</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Pagamento</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Total</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Data</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">
                      {(portal?.pedidos || []).map((pedido) => (
                        <tr key={pedido.id} className="bg-[#111111] text-zinc-200">
                          <td className="px-4 py-4 font-bold">{pedido.numeroPedido || `NX-${pedido.id}`}</td>
                          <td className="px-4 py-4">{pedido.status || '-'}</td>
                          <td className="px-4 py-4">{pedido.meioPagamento || '-'}</td>
                          <td className="px-4 py-4">{formatCurrency(pedido.valorTotal)}</td>
                          <td className="px-4 py-4">{formatDate(pedido.createdAt || pedido.dataCriacao)}</td>
                        </tr>
                      ))}
                      {(portal?.pedidos || []).length === 0 && (
                        <tr>
                          <td colSpan="5" className="px-4 py-8 text-center text-zinc-500">
                            Nenhum pedido registrado ainda.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </article>

              <div className="space-y-6">
                <article className="rounded-[32px] border border-white/10 bg-[#111111] p-6">
                  <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Central de mensagens</p>
                  <h2 className="mt-2 text-2xl font-black">Atendimento e suporte</h2>
                  <div className="mt-5 space-y-4">
                    <div className="rounded-3xl border border-white/5 bg-black/20 p-4">
                      <div className="inline-flex items-center gap-2 text-white">
                        <LifeBuoy size={18} />
                        <span className="font-bold">Yara • recepção digital</span>
                      </div>
                      <p className="mt-2 text-sm leading-6 text-zinc-300">Triagem inicial, dúvidas comerciais e encaminhamento das mensagens do site para corporativo.gna@gmail.com.</p>
                    </div>
                    <a href="mailto:corporativo.gna@gmail.com" className="inline-flex w-full items-center justify-center gap-2 rounded-2xl bg-[#C9A227] px-5 py-3 text-sm font-black uppercase tracking-[0.16em] text-black transition hover:bg-[#E8D5A3]">
                      <LifeBuoy size={18} />
                      Abrir atendimento
                    </a>
                  </div>
                </article>

                <article className="rounded-[32px] border border-white/10 bg-[#111111] p-6">
                  <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Documentos e relacionamento</p>
                  <h2 className="mt-2 text-2xl font-black">NFs, boletos e vínculo comercial</h2>
                  <div className="mt-5 space-y-3">
                    {(portal?.documentos || []).map((documento) => (
                      <div key={`${documento.tipo}-${documento.referencia}`} className="rounded-3xl border border-white/5 bg-black/20 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <div>
                            <p className="text-sm font-black text-white">{documento.tipo || 'Documento'}</p>
                            <p className="mt-1 text-sm text-zinc-400">{documento.referencia || 'Sem referência'}</p>
                          </div>
                          <div className="inline-flex items-center gap-2 text-zinc-300">
                            {documento.tipo?.toLowerCase().includes('boleto') ? <Receipt size={18} /> : <FileText size={18} />}
                            {documento.status || 'Disponível'}
                          </div>
                        </div>
                      </div>
                    ))}
                    {(portal?.documentos || []).length === 0 && (
                      <div className="rounded-3xl border border-dashed border-white/10 bg-black/20 p-4 text-sm text-zinc-400">
                        Assim que houver NF, boleto ou comprovante publicado, os arquivos aparecerão aqui para consulta e impressão.
                      </div>
                    )}
                  </div>
                </article>
              </div>
            </section>
          </>
        )}
      </div>
    </main>
  );
}
