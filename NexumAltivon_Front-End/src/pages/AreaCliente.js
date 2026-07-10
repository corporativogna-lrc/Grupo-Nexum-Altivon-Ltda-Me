/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { AlertCircle, BadgeDollarSign, CheckCircle2, FileText, LifeBuoy, LoaderCircle, MapPin, Pencil, Plus, Receipt, Save, ShoppingBag, Star, Trash2, Truck, UserCircle2, X } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { clienteAPI } from '../services/api';
import { buildWhatsAppLink, supportMessages, YARA_WHATSAPP } from '../utils/supportLinks';
import { fetchCepAddress, normalizeCep } from '../utils/validation';

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

const yaraInstantHref = buildWhatsAppLink(YARA_WHATSAPP, supportMessages.yaraCustomer);

const getPedidoTotal = (pedido) => pedido.valorTotal ?? pedido.total ?? 0;
const getPedidoData = (pedido) => pedido.createdAt || pedido.dataCriacao || pedido.dataPedido;
const getPedidoPagamento = (pedido) => pedido.meioPagamento || pedido.pagamento || pedido.statusPagamento || '-';
const getPedidoRastreio = (pedido) => pedido.codigoRastreio || pedido.freteCodigoRastreio || pedido.rastreio;
const getPedidoTransportadora = (pedido) => pedido.transportadora || pedido.freteTransportadora || 'Nexum Altivon';
const enderecoVazio = {
  apelido: '',
  tipo: 'Entrega',
  cep: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  cidade: '',
  estado: '',
  pais: 'Brasil',
  padrao: false,
};

export default function AreaCliente() {
  const { isAuthenticated, user, isAdmin } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [portal, setPortal] = useState(null);
  const [enderecoForm, setEnderecoForm] = useState(enderecoVazio);
  const [enderecoEditandoId, setEnderecoEditandoId] = useState(null);
  const [enderecoFeedback, setEnderecoFeedback] = useState('');
  const [savingEndereco, setSavingEndereco] = useState(false);

  const loadPortal = useCallback(async () => {
    if (!isAuthenticated || isAdmin) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError('');
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
  }, [isAuthenticated, isAdmin]);

  useEffect(() => {
    loadPortal();
  }, [loadPortal]);

  const resetEnderecoForm = () => {
    setEnderecoForm(enderecoVazio);
    setEnderecoEditandoId(null);
    setEnderecoFeedback('');
  };

  const editarEndereco = (endereco) => {
    setEnderecoEditandoId(endereco.id);
    setEnderecoFeedback('');
    setEnderecoForm({
      apelido: endereco.apelido || '',
      tipo: endereco.tipo || 'Entrega',
      cep: endereco.cep || '',
      logradouro: endereco.logradouro || '',
      numero: endereco.numero || '',
      complemento: endereco.complemento || '',
      bairro: endereco.bairro || '',
      cidade: endereco.cidade || '',
      estado: endereco.estado || '',
      pais: endereco.pais || 'Brasil',
      padrao: Boolean(endereco.padrao),
    });
  };

  const handleCepBlur = async () => {
    const cep = normalizeCep(enderecoForm.cep);
    if (cep.length !== 8) return;

    const autoEndereco = await fetchCepAddress(cep);
    if (!autoEndereco) return;

    setEnderecoForm((current) => ({
      ...current,
      ...autoEndereco,
      cep: autoEndereco.cep || cep,
    }));
  };

  const salvarEndereco = async (event) => {
    event.preventDefault();
    setEnderecoFeedback('');
    const cep = normalizeCep(enderecoForm.cep);
    if (cep.length !== 8 || !enderecoForm.logradouro || !enderecoForm.numero || !enderecoForm.bairro || !enderecoForm.cidade || !enderecoForm.estado) {
      setEnderecoFeedback('Preencha CEP, endereço, número, bairro, cidade e estado.');
      return;
    }

    try {
      setSavingEndereco(true);
      const payload = { ...enderecoForm, cep, estado: enderecoForm.estado.toUpperCase().slice(0, 2) };
      if (enderecoEditandoId) {
        await clienteAPI.atualizarEndereco(enderecoEditandoId, payload);
      } else {
        await clienteAPI.criarEndereco(payload);
      }
      resetEnderecoForm();
      await loadPortal();
      setEnderecoFeedback('Endereço salvo.');
    } catch (requestError) {
      setEnderecoFeedback(requestError.response?.data?.mensagem || 'Não foi possível salvar o endereço.');
    } finally {
      setSavingEndereco(false);
    }
  };

  const definirEnderecoPrincipal = async (id) => {
    try {
      setSavingEndereco(true);
      await clienteAPI.definirEnderecoPrincipal(id);
      await loadPortal();
      setEnderecoFeedback('Endereço principal atualizado.');
    } catch {
      setEnderecoFeedback('Não foi possível marcar o endereço principal.');
    } finally {
      setSavingEndereco(false);
    }
  };

  const removerEndereco = async (id) => {
    if (!window.confirm('Remover este endereço?')) return;

    try {
      setSavingEndereco(true);
      await clienteAPI.removerEndereco(id);
      if (enderecoEditandoId === id) resetEnderecoForm();
      await loadPortal();
      setEnderecoFeedback('Endereço removido.');
    } catch {
      setEnderecoFeedback('Não foi possível remover o endereço.');
    } finally {
      setSavingEndereco(false);
    }
  };

  const stats = useMemo(() => {
    const pedidos = portal?.pedidos || [];
    const totalCompras = pedidos.reduce((sum, item) => sum + Number(getPedidoTotal(item)), 0);
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

  const statusCadastro = String(portal?.statusCadastro || portal?.status || '').toLowerCase();
  const cadastroConfirmado = statusCadastro === 'ativo';
  const confirmadoEm = portal?.confirmadoEm || portal?.confirmado_em;

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (isAdmin) return <Navigate to="/dashboard" replace />;

  return (
    <main className="min-h-screen overflow-x-hidden bg-[radial-gradient(circle_at_top_left,#241B07_0,#050505_34%,#050505_100%)] px-3 py-6 text-white sm:px-6 lg:px-8">
      <div className="mx-auto w-full max-w-7xl space-y-6 sm:space-y-8">
        <section className="w-full max-w-full overflow-hidden rounded-[28px] border border-[#C9A227]/20 bg-gradient-to-br from-[#141414] to-[#080808] p-5 shadow-2xl shadow-black/40 sm:rounded-[32px] sm:p-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.22em] text-[#E8D5A3]">Área do cliente Grupo Nexum Altivon</p>
              <h1 className="mt-3 break-words text-3xl font-black sm:text-4xl">Olá, {portal?.nome || user?.nome || 'cliente'}.</h1>
              <p className="mt-3 max-w-2xl text-zinc-300">
                Aqui você acompanha pedidos, documentos, relacionamento e suporte em um canal direto com a operação comercial.
              </p>
              <div className={`mt-5 inline-flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-black ${cadastroConfirmado ? 'border-emerald-400/30 bg-emerald-400/10 text-emerald-200' : 'border-amber-400/30 bg-amber-400/10 text-amber-200'}`}>
                {cadastroConfirmado ? <CheckCircle2 size={16} /> : <AlertCircle size={16} />}
                {cadastroConfirmado
                  ? `Cadastro confirmado${confirmadoEm ? ` em ${formatDate(confirmadoEm)}` : ''}`
                  : 'Cadastro pendente de confirmação'}
              </div>
            </div>
            <div className="w-full min-w-0 rounded-3xl border border-[#C9A227]/20 bg-black/40 p-5 lg:max-w-sm">
              <p className="text-xs font-black uppercase tracking-[0.18em] text-zinc-500">Cadastro principal</p>
              <p className="mt-2 break-words text-lg font-bold">{portal?.email || user?.email}</p>
              <p className="mt-1 text-sm text-zinc-400">{portal?.telefone || 'Telefone em atualização'}</p>
              <a
                href={yaraInstantHref}
                className="mt-4 inline-flex h-11 w-11 items-center justify-center rounded-full border border-[#C9A227]/30 text-[#E8D5A3] transition hover:border-[#E8D5A3] hover:text-white"
                aria-label="Atendimento Yara"
                title="Atendimento Yara"
              >
                <LifeBuoy size={18} />
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
            <p className="mt-3 text-sm text-red-50/90">
              Se você acabou de se cadastrar, confira o e-mail de confirmação antes de tentar entrar novamente.
            </p>
          </section>
        ) : (
          <>
            <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {[
                { label: 'Pedidos registrados', value: portal?.pedidos?.length || 0, icon: ShoppingBag },
                { label: 'Total comprado', value: formatCurrency(stats.totalCompras), icon: BadgeDollarSign },
                { label: 'Pontuação', value: `${stats.pontos} pts`, icon: Star },
                { label: 'Score / limite', value: `${stats.score} • ${stats.limiteFuturo}`, icon: UserCircle2 },
              ].map((item) => {
                const Icon = item.icon;
                return (
                  <article key={item.label} className="min-w-0 rounded-[28px] border border-white/10 bg-[#111111] p-5 sm:p-6">
                    <div className="inline-flex h-12 w-12 items-center justify-center rounded-2xl bg-[#C9A227] text-black">
                      <Icon size={20} />
                    </div>
                    <p className="mt-5 text-sm font-black uppercase tracking-[0.16em] text-zinc-500">{item.label}</p>
                    <p className="mt-3 break-words text-xl font-black text-white">{item.value}</p>
                  </article>
                );
              })}
            </section>

            <section className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
              <article className="min-w-0 rounded-[28px] border border-white/10 bg-[#111111] p-5 sm:rounded-[32px] sm:p-6">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Histórico de compras</p>
                    <h2 className="mt-2 text-2xl font-black">Pedidos e status reais</h2>
                  </div>
                  <Link to="/produtos" className="inline-flex w-fit rounded-full border border-white/10 px-4 py-2 text-sm font-bold text-zinc-100 transition hover:border-[#C9A227] hover:text-[#E8D5A3]">
                    Continuar comprando
                  </Link>
                </div>

                <div className="mt-6 grid gap-3 md:hidden">
                  {(portal?.pedidos || []).map((pedido) => {
                    const rastreio = getPedidoRastreio(pedido);
                    return (
                      <article key={pedido.id} className="rounded-3xl border border-white/10 bg-black/25 p-4">
                        <div className="flex items-start justify-between gap-3">
                          <div className="min-w-0">
                            <p className="break-words text-sm font-black text-white">{pedido.numeroPedido || `NX-${pedido.id}`}</p>
                            <p className="mt-1 text-xs font-bold uppercase tracking-[0.14em] text-[#E8D5A3]">{pedido.status || '-'}</p>
                          </div>
                          <span className="shrink-0 text-sm font-black text-white">{formatCurrency(getPedidoTotal(pedido))}</span>
                        </div>
                        <div className="mt-4 grid gap-2 text-sm text-zinc-300">
                          <p><span className="font-bold text-zinc-100">Pagamento:</span> {getPedidoPagamento(pedido)}</p>
                          <p><span className="font-bold text-zinc-100">Entrega:</span> {getPedidoTransportadora(pedido)}</p>
                          <p><span className="font-bold text-zinc-100">Rastreio:</span> {rastreio || 'em preparação'}</p>
                          <p><span className="font-bold text-zinc-100">Data:</span> {formatDate(getPedidoData(pedido))}</p>
                        </div>
                      </article>
                    );
                  })}
                  {(portal?.pedidos || []).length === 0 && (
                    <div className="rounded-3xl border border-dashed border-white/10 bg-black/20 p-5 text-center text-sm text-zinc-500">
                      Nenhum pedido registrado ainda.
                    </div>
                  )}
                </div>

                <div className="mt-6 hidden max-w-full overflow-x-auto rounded-3xl border border-white/5 md:block">
                  <table className="min-w-[860px] divide-y divide-white/5 text-sm">
                    <thead className="bg-black/30 text-zinc-400">
                      <tr>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Pedido</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Status</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Pagamento</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Entrega</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Total</th>
                        <th className="px-4 py-3 text-left font-black uppercase tracking-[0.14em]">Data</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">
                      {(portal?.pedidos || []).map((pedido) => {
                        const rastreio = getPedidoRastreio(pedido);
                        return (
                          <tr key={pedido.id} className="bg-[#111111] text-zinc-200">
                            <td className="px-4 py-4 font-bold">{pedido.numeroPedido || `NX-${pedido.id}`}</td>
                            <td className="px-4 py-4">{pedido.status || '-'}</td>
                            <td className="px-4 py-4">{getPedidoPagamento(pedido)}</td>
                            <td className="px-4 py-4">
                              <div className="inline-flex items-start gap-2">
                                <Truck className="mt-0.5 shrink-0 text-[#C9A227]" size={16} />
                                <div>
                                  <p className="font-bold text-zinc-100">{getPedidoTransportadora(pedido)}</p>
                                  <p className="mt-1 text-xs text-zinc-400">
                                    {rastreio ? `Rastreio: ${rastreio}` : 'Rastreio em preparação'}
                                  </p>
                                </div>
                              </div>
                            </td>
                            <td className="px-4 py-4">{formatCurrency(getPedidoTotal(pedido))}</td>
                            <td className="px-4 py-4">{formatDate(getPedidoData(pedido))}</td>
                          </tr>
                        );
                      })}
                      {(portal?.pedidos || []).length === 0 && (
                        <tr>
                          <td colSpan="6" className="px-4 py-8 text-center text-zinc-500">
                            Nenhum pedido registrado ainda.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </article>

              <div className="min-w-0 space-y-6">
                <article className="min-w-0 rounded-[28px] border border-white/10 bg-[#111111] p-5 sm:rounded-[32px] sm:p-6">
                  <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Central de mensagens</p>
                  <h2 className="mt-2 text-2xl font-black">Atendimento e suporte</h2>
                  <div className="mt-5 space-y-4">
                    <div className="rounded-3xl border border-white/5 bg-black/20 p-4">
                      <div className="inline-flex items-center gap-2 text-white">
                        <LifeBuoy size={18} />
                        <span className="font-bold">Atendimento comercial</span>
                      </div>
                      <p className="mt-2 text-sm leading-6 text-zinc-300">Contato direto para dúvidas comerciais, apoio ao pedido e acompanhamento do relacionamento com o cliente.</p>
                    </div>
                    <a href={yaraInstantHref} className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-[#C9A227] text-black transition hover:bg-[#E8D5A3]" aria-label="Atendimento Yara" title="Atendimento Yara">
                      <LifeBuoy size={18} />
                    </a>
                  </div>
                </article>

                <article className="min-w-0 rounded-[28px] border border-white/10 bg-[#111111] p-5 sm:rounded-[32px] sm:p-6">
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

                <article className="min-w-0 rounded-[28px] border border-white/10 bg-[#111111] p-5 sm:rounded-[32px] sm:p-6">
                  <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Endereços</p>
                  <h2 className="mt-2 text-2xl font-black">Principal e auxiliares</h2>

                  <form onSubmit={salvarEndereco} className="mt-5 min-w-0 space-y-3 rounded-3xl border border-white/5 bg-black/20 p-4">
                    <div className="grid min-w-0 gap-3 sm:grid-cols-2">
                      <input
                        type="text"
                        placeholder="Apelido"
                        value={enderecoForm.apelido}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, apelido: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227]"
                      />
                      <select
                        value={enderecoForm.tipo}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, tipo: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none focus:border-[#C9A227]"
                      >
                        <option value="Entrega">Entrega</option>
                        <option value="Cobranca">Cobrança</option>
                        <option value="Ambos">Ambos</option>
                      </select>
                      <input
                        type="text"
                        placeholder="CEP"
                        value={enderecoForm.cep}
                        onBlur={handleCepBlur}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, cep: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227]"
                      />
                      <input
                        type="text"
                        placeholder="Número"
                        value={enderecoForm.numero}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, numero: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227]"
                      />
                      <input
                        type="text"
                        placeholder="Logradouro"
                        value={enderecoForm.logradouro}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, logradouro: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227] sm:col-span-2"
                      />
                      <input
                        type="text"
                        placeholder="Complemento"
                        value={enderecoForm.complemento}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, complemento: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227] sm:col-span-2"
                      />
                      <input
                        type="text"
                        placeholder="Bairro"
                        value={enderecoForm.bairro}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, bairro: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227]"
                      />
                      <input
                        type="text"
                        placeholder="Cidade"
                        value={enderecoForm.cidade}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, cidade: event.target.value }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227]"
                      />
                      <input
                        type="text"
                        placeholder="Estado"
                        value={enderecoForm.estado}
                        onChange={(event) => setEnderecoForm((current) => ({ ...current, estado: event.target.value.toUpperCase().slice(0, 2) }))}
                        className="rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227]"
                      />
                      <label className="inline-flex items-center gap-2 rounded-2xl border border-white/10 bg-[#0B0B0B] px-4 py-3 text-sm font-bold text-zinc-200">
                        <input
                          type="checkbox"
                          checked={enderecoForm.padrao}
                          onChange={(event) => setEnderecoForm((current) => ({ ...current, padrao: event.target.checked }))}
                          className="accent-[#C9A227]"
                        />
                        Principal
                      </label>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="submit"
                        disabled={savingEndereco}
                        className="inline-flex items-center gap-2 rounded-full bg-[#C9A227] px-4 py-2 text-sm font-black text-black transition hover:bg-[#E8D5A3] disabled:opacity-50"
                      >
                        {enderecoEditandoId ? <Save size={16} /> : <Plus size={16} />}
                        {enderecoEditandoId ? 'Salvar endereço' : 'Adicionar endereço'}
                      </button>
                      {enderecoEditandoId && (
                        <button
                          type="button"
                          onClick={resetEnderecoForm}
                          className="inline-flex items-center gap-2 rounded-full border border-white/10 px-4 py-2 text-sm font-bold text-zinc-200 transition hover:border-[#C9A227]"
                        >
                          <X size={16} />
                          Cancelar
                        </button>
                      )}
                    </div>
                    {enderecoFeedback && <p className="text-sm font-bold text-[#E8D5A3]">{enderecoFeedback}</p>}
                  </form>

                  <div className="mt-5 space-y-3">
                    {(portal?.enderecos || []).map((endereco) => (
                      <div key={endereco.id} className="min-w-0 rounded-3xl border border-white/5 bg-black/20 p-4">
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                          <div className="min-w-0">
                            <div className="inline-flex items-center gap-2">
                              <MapPin size={16} className="text-[#C9A227]" />
                              <p className="text-sm font-black text-white">
                                {endereco.apelido || 'Endereço'}
                                {endereco.padrao ? ' • Principal' : ''}
                              </p>
                            </div>
                            <p className="mt-1 break-words text-sm text-zinc-400">
                              {endereco.logradouro}, {endereco.numero}
                              {endereco.complemento ? ` - ${endereco.complemento}` : ''}
                            </p>
                            <p className="mt-1 break-words text-xs text-zinc-500">
                              {endereco.bairro || 'Bairro não informado'} • {endereco.cidade || 'Cidade não informada'} / {endereco.estado || '--'} • {endereco.cep}
                            </p>
                          </div>
                          <div className="flex shrink-0 flex-wrap gap-2 sm:justify-end">
                            <span className="rounded-full border border-[#C9A227]/20 px-3 py-1 text-xs font-black text-[#E8D5A3]">
                              {endereco.tipo || 'Entrega'}
                            </span>
                            {!endereco.padrao && (
                              <button
                                type="button"
                                onClick={() => definirEnderecoPrincipal(endereco.id)}
                                disabled={savingEndereco}
                                className="rounded-full border border-white/10 p-2 text-zinc-300 transition hover:border-[#C9A227] hover:text-[#E8D5A3]"
                                title="Definir como principal"
                              >
                                <Star size={15} />
                              </button>
                            )}
                            <button
                              type="button"
                              onClick={() => editarEndereco(endereco)}
                              disabled={savingEndereco}
                              className="rounded-full border border-white/10 p-2 text-zinc-300 transition hover:border-[#C9A227] hover:text-[#E8D5A3]"
                              title="Editar endereço"
                            >
                              <Pencil size={15} />
                            </button>
                            <button
                              type="button"
                              onClick={() => removerEndereco(endereco.id)}
                              disabled={savingEndereco}
                              className="rounded-full border border-white/10 p-2 text-zinc-300 transition hover:border-red-400 hover:text-red-200"
                              title="Remover endereço"
                            >
                              <Trash2 size={15} />
                            </button>
                          </div>
                        </div>
                      </div>
                    ))}
                    {(portal?.enderecos || []).length === 0 && (
                      <div className="rounded-3xl border border-dashed border-white/10 bg-black/20 p-4 text-sm text-zinc-400">
                        Nenhum endereço cadastrado ainda. O checkout continua aceitando endereço novo no momento da compra.
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
