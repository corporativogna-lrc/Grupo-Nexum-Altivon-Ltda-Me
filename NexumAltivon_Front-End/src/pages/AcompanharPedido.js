/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { AlertCircle, CheckCircle2, LoaderCircle, PackageSearch, Truck } from 'lucide-react';
import { pedidoAPI } from '../services/api';
import { getPagamentoLabel } from '../utils/formatters';

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

export default function AcompanharPedido() {
  const [searchParams] = useSearchParams();
  const [numero, setNumero] = useState(searchParams.get('pedido') || '');
  const [identificador, setIdentificador] = useState(searchParams.get('email') || searchParams.get('documento') || '');
  const [pedido, setPedido] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setPedido(null);
    setLoading(true);

    try {
      const documento = identificador.replace(/\D/g, '');
      const params = {
        numero: numero.trim(),
        email: identificador.includes('@') ? identificador.trim() : undefined,
        documento: documento || undefined,
      };
      const response = await pedidoAPI.acompanhar(params);
      setPedido(response.data);
    } catch (requestError) {
      setError(
        requestError.response?.data?.detail ||
          requestError.response?.data?.mensagem ||
          'Nao localizamos este pedido com os dados informados.',
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen bg-[#050505] px-4 py-10 text-white sm:px-6 lg:px-8">
      <div className="mx-auto grid max-w-6xl gap-8 lg:grid-cols-[0.95fr_1.05fr] lg:items-start">
        <section className="rounded-[32px] border border-white/10 bg-gradient-to-br from-[#151515] to-[#080808] p-8 shadow-2xl">
          <div className="inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-[#C9A227] text-black">
            <PackageSearch size={26} />
          </div>
          <p className="mt-6 text-sm font-black uppercase tracking-[0.2em] text-[#E8D5A3]">Acompanhar pedido</p>
          <h1 className="mt-3 text-4xl font-black">Consulte sua compra sem depender do login.</h1>
          <p className="mt-4 leading-7 text-zinc-300">
            Informe o numero do pedido e o e-mail ou documento usado na compra. A consulta mostra apenas o pedido correspondente aos seus dados.
          </p>

          <form className="mt-8 space-y-4" onSubmit={handleSubmit}>
            <div>
              <label className="mb-2 block text-sm font-black text-zinc-200">Numero do pedido</label>
              <input
                value={numero}
                onChange={(event) => setNumero(event.target.value)}
                required
                className="w-full rounded-2xl border border-white/10 bg-black/40 px-4 py-3 font-mono text-white outline-none transition focus:border-[#C9A227]"
                placeholder="Ex: NX26062304132918"
                data-testid="acompanhar-numero"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-black text-zinc-200">E-mail ou CPF/CNPJ</label>
              <input
                value={identificador}
                onChange={(event) => setIdentificador(event.target.value)}
                required
                className="w-full rounded-2xl border border-white/10 bg-black/40 px-4 py-3 text-white outline-none transition focus:border-[#C9A227]"
                placeholder="cliente@email.com ou documento"
                data-testid="acompanhar-identificador"
              />
            </div>

            {error && (
              <div className="flex items-start gap-3 rounded-2xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-100">
                <AlertCircle className="mt-0.5 shrink-0" size={18} />
                <span>{error}</span>
              </div>
            )}

            <button
              type="submit"
              disabled={loading}
              className="inline-flex w-full items-center justify-center gap-2 rounded-full bg-[#C9A227] px-6 py-3 font-black text-black transition hover:bg-[#E8D5A3] disabled:opacity-60"
              data-testid="acompanhar-submit"
            >
              {loading ? <LoaderCircle className="animate-spin" size={18} /> : <PackageSearch size={18} />}
              {loading ? 'Consultando...' : 'Consultar pedido'}
            </button>
          </form>
        </section>

        <section className="rounded-[32px] border border-white/10 bg-[#111111] p-8 shadow-2xl">
          {pedido ? (
            <div data-testid="acompanhar-resultado">
              <div className="flex flex-col gap-4 border-b border-white/10 pb-6 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <p className="text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Pedido encontrado</p>
                  <h2 className="mt-2 font-mono text-2xl font-black text-white">{pedido.numero_pedido || pedido.numeroPedido}</h2>
                  <p className="mt-2 text-zinc-400">Cliente: {pedido.cliente_nome || pedido.clienteNome}</p>
                </div>
                <span className="inline-flex items-center gap-2 rounded-full border border-emerald-400/30 bg-emerald-400/10 px-4 py-2 text-sm font-black text-emerald-200">
                  <CheckCircle2 size={16} />
                  {pedido.status || 'Registrado'}
                </span>
              </div>

              <div className="mt-6 grid gap-4 sm:grid-cols-2">
                <InfoCard label="Total" value={formatCurrency(pedido.total)} />
                <InfoCard label="Pagamento" value={pedido.status_pagamento || pedido.statusPagamento || 'Aguardando'} detail={getPagamentoLabel(pedido.meio_pagamento || pedido.meioPagamento || '')} />
                <InfoCard label="Criado em" value={formatDate(pedido.created_at || pedido.createdAt)} />
                <InfoCard label="Atualizado em" value={formatDate(pedido.updated_at || pedido.updatedAt)} />
              </div>

              <div className="mt-6 rounded-3xl border border-white/10 bg-black/30 p-5">
                <div className="inline-flex items-center gap-2 text-white">
                  <Truck size={19} className="text-[#C9A227]" />
                  <span className="font-black">Entrega e logistica</span>
                </div>
                <div className="mt-4 grid gap-3 text-sm text-zinc-300 sm:grid-cols-2">
                  <p><span className="font-black text-zinc-100">Metodo:</span> {pedido.frete_metodo || pedido.freteMetodo || 'A combinar'}</p>
                  <p><span className="font-black text-zinc-100">Transportadora:</span> {pedido.frete_transportadora || pedido.freteTransportadora || 'Nexum Altivon'}</p>
                  <p><span className="font-black text-zinc-100">Prazo:</span> {(pedido.frete_prazo_dias || pedido.fretePrazoDias || 0) > 0 ? `${pedido.frete_prazo_dias || pedido.fretePrazoDias} dias` : 'Em definicao'}</p>
                  <p><span className="font-black text-zinc-100">Rastreio:</span> {pedido.frete_codigo_rastreio || pedido.freteCodigoRastreio || 'Ainda nao informado'}</p>
                </div>
              </div>

              {pedido.instrucao_pagamento && (
                <p className="mt-6 rounded-2xl border border-[#C9A227]/30 bg-[#C9A227]/10 p-4 text-sm font-semibold text-[#F7E7A6]">
                  {pedido.instrucao_pagamento}
                </p>
              )}
            </div>
          ) : (
            <div className="flex min-h-[420px] flex-col items-center justify-center text-center">
              <div className="inline-flex h-20 w-20 items-center justify-center rounded-3xl border border-white/10 bg-black/30 text-[#C9A227]">
                <PackageSearch size={34} />
              </div>
              <h2 className="mt-6 text-2xl font-black">Aguardando consulta</h2>
              <p className="mt-3 max-w-md text-zinc-400">
                Esta area ajuda o atendimento e o cliente a conferir pedido, pagamento e entrega enquanto as integracoes externas ficam em espera.
              </p>
            </div>
          )}
        </section>
      </div>
    </main>
  );
}

function InfoCard({ label, value, detail }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-black/25 p-4">
      <p className="text-xs font-black uppercase tracking-[0.16em] text-zinc-500">{label}</p>
      <p className="mt-2 text-lg font-black text-white">{value || '-'}</p>
      {detail && <p className="mt-1 text-sm text-zinc-400">{detail}</p>}
    </div>
  );
}
