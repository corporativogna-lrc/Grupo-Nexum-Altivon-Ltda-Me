/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { Link } from 'react-router-dom';
import { User, MapPin, CreditCard } from 'lucide-react';
import { getPagamentoLabel } from '../utils/formatters';
import { fetchCepAddress, normalizeCep } from '../utils/validation';

function getLojaPrefix(loja) {
  const valorComparacao = [
    loja?.tipo,
    loja?.origem,
    loja?.segmento,
    loja?.nome,
    loja?.slug,
  ]
    .filter(Boolean)
    .join(' ')
    .toLowerCase();

  if (valorComparacao.includes('parceiro')) return 'Pr';
  if (valorComparacao.includes('dropship') || valorComparacao.includes('dropi') || valorComparacao.includes('cj')) return 'Ds';
  return 'Lj';
}

function formatLojaLabel(loja) {
  if (!loja) return '';
  const prefixo = getLojaPrefix(loja);
  return `${prefixo} ${loja.id} - ${loja.nome}`;
}

// Step 1: Dados Pessoais
export function StepDadosPessoais({ dadosCliente, setDadosCliente, exigeSenha = true, onNext }) {
  const senhaValida = !exigeSenha || (
    dadosCliente.senha &&
    dadosCliente.senha.length >= 8 &&
    dadosCliente.senha === dadosCliente.confirmarSenha
  );
  const isValid = dadosCliente.nome && dadosCliente.email && senhaValida;

  return (
    <div>
      <h2 className="mb-4 flex items-center text-2xl font-black text-white">
        <User className="mr-2 text-[#C9A227]" /> Dados Pessoais
      </h2>
      <div className="space-y-4">
        <input
          type="text"
          required
          placeholder="Nome Completo *"
          value={dadosCliente.nome}
          onChange={(e) => setDadosCliente({ ...dadosCliente, nome: e.target.value })}
          className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
          data-testid="checkout-nome"
        />
        <input
          type="email"
          required
          placeholder="Email *"
          value={dadosCliente.email}
          onChange={(e) => setDadosCliente({ ...dadosCliente, email: e.target.value })}
          className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
          data-testid="checkout-email"
        />
        <input
          type="text"
          placeholder="CPF"
          value={dadosCliente.cpf}
          onChange={(e) => setDadosCliente({ ...dadosCliente, cpf: e.target.value })}
          className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
        />
        <input
          type="tel"
          placeholder="Telefone"
          value={dadosCliente.telefone}
          onChange={(e) => setDadosCliente({ ...dadosCliente, telefone: e.target.value })}
          className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
        />
        {exigeSenha && (
          <div className="grid gap-4 md:grid-cols-2">
            <input
              type="password"
              required
              placeholder="Criar senha de acesso *"
              value={dadosCliente.senha}
              onChange={(e) => setDadosCliente({ ...dadosCliente, senha: e.target.value })}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
              data-testid="checkout-senha"
            />
            <input
              type="password"
              required
              placeholder="Confirmar senha *"
              value={dadosCliente.confirmarSenha}
              onChange={(e) => setDadosCliente({ ...dadosCliente, confirmarSenha: e.target.value })}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
              data-testid="checkout-confirmar-senha"
            />
          </div>
        )}
      </div>
      <button
        onClick={onNext}
        disabled={!isValid}
        className="mt-6 rounded-full bg-[#C9A227] px-6 py-3 font-black text-black transition hover:bg-[#FFD95A] disabled:opacity-50"
        data-testid="step1-next"
      >
        Próximo →
      </button>
    </div>
  );
}

// Step 2: Endereço
export function StepEndereco({ endereco, setEndereco, onBack, onNext }) {
  const isValid =
    endereco.cep &&
    endereco.logradouro &&
    endereco.numero &&
    endereco.bairro &&
    endereco.cidade &&
    endereco.estado;

  const handleCepBlur = async () => {
    const cep = normalizeCep(endereco.cep);
    if (cep.length !== 8) return;

    try {
      const autoEndereco = await fetchCepAddress(cep);
      if (!autoEndereco) return;

      setEndereco((current) => ({
        ...current,
        ...autoEndereco,
        cep: autoEndereco.cep || cep,
      }));
    } catch {
      // Mantém o preenchimento manual se a consulta externa falhar.
    }
  };

  return (
    <div>
      <h2 className="mb-4 flex items-center text-2xl font-black text-white">
        <MapPin className="mr-2 text-[#C9A227]" /> Endereço de Entrega
      </h2>
      <div className="grid grid-cols-2 gap-4">
        <input type="text" required placeholder="CEP *" value={endereco.cep}
          onChange={(e) => setEndereco({ ...endereco, cep: e.target.value })}
          onBlur={handleCepBlur}
          className="rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
        <input type="text" required placeholder="Logradouro *" value={endereco.logradouro}
          onChange={(e) => setEndereco({ ...endereco, logradouro: e.target.value })}
          className="rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
        <input type="text" required placeholder="Número *" value={endereco.numero}
          onChange={(e) => setEndereco({ ...endereco, numero: e.target.value })}
          className="rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
        <input type="text" placeholder="Complemento" value={endereco.complemento}
          onChange={(e) => setEndereco({ ...endereco, complemento: e.target.value })}
          className="rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
        <input type="text" required placeholder="Bairro *" value={endereco.bairro}
          onChange={(e) => setEndereco({ ...endereco, bairro: e.target.value })}
          className="rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
        <input type="text" required placeholder="Cidade *" value={endereco.cidade}
          onChange={(e) => setEndereco({ ...endereco, cidade: e.target.value })}
          className="rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
        <input type="text" required placeholder="Estado *" value={endereco.estado}
          onChange={(e) => setEndereco({ ...endereco, estado: e.target.value })}
          className="col-span-2 rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white placeholder:text-[#777]" />
      </div>
      <div className="flex gap-3 mt-6">
        <button onClick={onBack} className="rounded-full border border-[#2A2A2A] bg-[#080808] px-6 py-3 font-bold text-[#D8D8D8] hover:border-[#C9A227]">
          ← Voltar
        </button>
        <button onClick={onNext} disabled={!isValid}
          className="rounded-full bg-[#C9A227] px-6 py-3 font-black text-black transition hover:bg-[#FFD95A] disabled:opacity-50">
          Próximo →
        </button>
      </div>
    </div>
  );
}

// Step 3: Pagamento
export function StepPagamento({
  lojas,
  lojaId,
  setLojaId,
  metodoPagamento,
  setMetodoPagamento,
  parcelas,
  setParcelas,
  dadosCartao,
  setDadosCartao,
  freteOptions = [],
  freteSelecionado,
  setFreteSelecionado,
  onBack,
  onConfirm,
  loading,
}) {
  const metodos = ['cartao', 'pix', 'boleto', 'debito', 'deposito'];
  const formatPrice = (price) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(price);
  const cartaoValido =
    metodoPagamento !== 'cartao' ||
    Boolean(
      dadosCartao?.numero &&
        dadosCartao?.nomeTitular &&
        dadosCartao?.validade &&
        dadosCartao?.cvv &&
        dadosCartao?.cpfTitular,
    );

  return (
    <div>
      <h2 className="mb-4 flex items-center text-2xl font-black text-white">
        <CreditCard className="mr-2 text-[#C9A227]" /> Pagamento
      </h2>

      <div className="mb-4">
        <label className="mb-2 block text-sm font-bold text-[#D8D8D8]">Loja</label>
        <select value={lojaId} onChange={(e) => setLojaId(e.target.value)}
          className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white">
          {lojas.map((loja) => (
            <option key={loja.id} value={loja.id}>{formatLojaLabel(loja)}</option>
          ))}
        </select>
      </div>

      <div className="mb-6 space-y-3">
        <label className="mb-2 block font-bold text-[#D8D8D8]">Frete e logística</label>
        {freteOptions.map((frete) => (
          <label key={frete.id} className="flex cursor-pointer items-center rounded-xl border border-[#2A2A2A] bg-[#080808] p-4 hover:border-[#C9A227]">
            <input
              type="radio"
              name="frete"
              value={frete.id}
              checked={freteSelecionado === frete.id}
              onChange={(e) => setFreteSelecionado(e.target.value)}
              className="mr-3 accent-[#C9A227]"
            />
            <span>
              <span className="block font-bold text-white">{frete.nome}</span>
              <span className="text-xs font-semibold text-[#A0A0A0]">{frete.transportadora} · {frete.prazo === 0 ? 'combinar prazo' : `${frete.prazo} dias úteis`}</span>
            </span>
            <span className="ml-auto rounded-full border border-[#2A2A2A] px-3 py-1 text-xs font-black text-[#C9A227]">
              {frete.valor === 0 ? 'Grátis' : formatPrice(frete.valor)}
            </span>
          </label>
        ))}
      </div>

      <div className="space-y-3 mb-6">
        <label className="mb-2 block font-bold text-[#D8D8D8]">Método de Pagamento</label>
        {metodos.map((metodo) => (
          <label key={metodo} className="flex cursor-pointer items-center rounded-xl border border-[#2A2A2A] bg-[#080808] p-4 hover:border-[#C9A227]">
            <input
              type="radio"
              name="pagamento"
              value={metodo}
              checked={metodoPagamento === metodo}
              onChange={(e) => setMetodoPagamento(e.target.value)}
              className="mr-3 accent-[#C9A227]"
            />
            <span className="font-bold capitalize text-white">{getPagamentoLabel(metodo)}</span>
            <span className="ml-auto rounded-full border border-[#2A2A2A] px-3 py-1 text-xs font-bold text-[#A0A0A0]">
              {metodo === 'pix' ? 'Confirmação automática' : metodo === 'boleto' ? 'Baixa por compensação' : 'Registrado no pedido'}
            </span>
          </label>
        ))}
      </div>

      {metodoPagamento === 'cartao' && (
        <div className="mb-6">
          <label className="mb-2 block font-bold text-[#D8D8D8]">Parcelamento no cartão</label>
          <select
            value={parcelas}
            onChange={(e) => setParcelas(Number(e.target.value))}
            className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-4 py-3 text-white"
          >
            {Array.from({ length: 24 }, (_, index) => index + 1).map((item) => (
              <option key={item} value={item}>
                {item}x {item === 1 ? 'à vista' : 'no cartão'}
              </option>
            ))}
          </select>
          <p className="mt-2 text-xs font-semibold text-[#A0A0A0]">
            O pedido já grava as parcelas para o financeiro e para a integração real do gateway.
          </p>

          <div className="mt-4 space-y-3 rounded-2xl border border-[#2A2A2A] bg-[#080808] p-4">
            <p className="text-sm font-black uppercase tracking-[0.14em] text-[#C9A227]">Dados do cartão</p>
            <input
              type="text"
              inputMode="numeric"
              placeholder="Número do cartão"
              value={dadosCartao.numero}
              onChange={(e) => setDadosCartao({ ...dadosCartao, numero: e.target.value })}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#111111] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
            />
            <input
              type="text"
              placeholder="Nome do titular"
              value={dadosCartao.nomeTitular}
              onChange={(e) => setDadosCartao({ ...dadosCartao, nomeTitular: e.target.value })}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#111111] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
            />
            <div className="grid gap-3 md:grid-cols-2">
              <input
                type="text"
                placeholder="Validade MM/AA"
                value={dadosCartao.validade}
                onChange={(e) => setDadosCartao({ ...dadosCartao, validade: e.target.value })}
                className="w-full rounded-xl border border-[#2A2A2A] bg-[#111111] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
              />
              <input
                type="password"
                inputMode="numeric"
                placeholder="CVV"
                value={dadosCartao.cvv}
                onChange={(e) => setDadosCartao({ ...dadosCartao, cvv: e.target.value })}
                className="w-full rounded-xl border border-[#2A2A2A] bg-[#111111] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
              />
            </div>
            <input
              type="text"
              placeholder="CPF do titular"
              value={dadosCartao.cpfTitular}
              onChange={(e) => setDadosCartao({ ...dadosCartao, cpfTitular: e.target.value })}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#111111] px-4 py-3 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
            />
          </div>
        </div>
      )}

      <div className="flex gap-3">
        <button onClick={onBack} className="rounded-full border border-[#2A2A2A] bg-[#080808] px-6 py-3 font-bold text-[#D8D8D8] hover:border-[#C9A227]">
          ← Voltar
        </button>
        <button onClick={onConfirm} disabled={loading || !cartaoValido}
          className="flex-1 rounded-full bg-[#C9A227] px-6 py-3 font-black text-black transition hover:bg-[#FFD95A] disabled:opacity-50"
          data-testid="finalizar-pedido-btn">
          {loading ? 'Processando...' : 'Confirmar Pedido'}
        </button>
      </div>
    </div>
  );
}

// Resumo do pedido
export function ResumoPedido({ cart, subtotal, desconto, frete, total }) {
  const formatPrice = (price) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(price);

  return (
    <div className="h-fit rounded-2xl border border-[#2A2A2A] bg-[#111111] p-6 shadow-[0_24px_80px_rgba(0,0,0,0.35)]">
      <h3 className="mb-4 text-xl font-black text-white">Resumo</h3>
      <div className="space-y-2 mb-4">
        {cart.map((item) => (
          <div key={item.id} className="flex justify-between gap-4 text-sm text-[#D8D8D8]">
            <span>{item.nome} x {item.quantity}</span>
            <span>{formatPrice((item.preco_promocional || item.preco) * item.quantity)}</span>
          </div>
        ))}
      </div>
      <div className="space-y-2 border-t border-[#2A2A2A] pt-4 text-[#D8D8D8]">
        <div className="flex justify-between"><span>Subtotal:</span><span>{formatPrice(subtotal)}</span></div>
        {desconto > 0 && (
          <div className="flex justify-between text-emerald-400">
            <span>Desconto:</span><span>-{formatPrice(desconto)}</span>
          </div>
        )}
        {frete && (
          <div className="flex justify-between">
            <span>Frete:</span><span>{frete.valor === 0 ? 'Grátis' : formatPrice(frete.valor)}</span>
          </div>
        )}
        <div className="flex justify-between border-t border-[#2A2A2A] pt-2 text-xl font-black text-white">
          <span>Total:</span>
          <span className="text-[#C9A227]">{formatPrice(total)}</span>
        </div>
      </div>
    </div>
  );
}

// Stepper indicator
export function CheckoutStepper({ step }) {
  return (
    <div className="flex items-center mb-8">
      {[1, 2, 3].map((s) => (
        <div key={s} className="flex items-center flex-1">
          <div className={`w-10 h-10 rounded-full flex items-center justify-center font-bold ${
            step >= s ? 'bg-[#C9A227] text-black' : 'bg-[#2A2A2A] text-[#A0A0A0]'
          }`}>
            {s}
          </div>
          {s < 3 && <div className={`mx-2 h-1 flex-1 ${step > s ? 'bg-[#C9A227]' : 'bg-[#2A2A2A]'}`}></div>}
        </div>
      ))}
    </div>
  );
}

// Success page
export function CheckoutSuccess({ pedido, clienteEmail, onContinue }) {
  const formatPrice = (price) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(price);
  const possuiAlertaOperacional = Boolean(pedido.alerta_operacional);
  const acompanharPedidoUrl = `/acompanhar-pedido?pedido=${encodeURIComponent(pedido.numero_pedido || '')}${
    clienteEmail ? `&email=${encodeURIComponent(clienteEmail)}` : ''
  }`;

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#050505] py-12 text-white">
      <div className="max-w-2xl rounded-3xl border border-[#2A2A2A] bg-[#111111] p-8 text-center shadow-[0_28px_100px_rgba(0,0,0,0.42)]" data-testid="pedido-sucesso">
        <div className={`mb-4 text-7xl ${possuiAlertaOperacional ? 'text-amber-400' : 'text-emerald-400'}`}>
          {possuiAlertaOperacional ? '!' : '✓'}
        </div>
        <h2 className="mb-2 text-3xl font-black text-white">
          {possuiAlertaOperacional ? 'Pedido registrado com pendência' : 'Pedido realizado'}
        </h2>
        <p className="mb-6 text-[#A0A0A0]">
          {possuiAlertaOperacional ? 'A compra foi registrada e requer acompanhamento operacional.' : 'Seu pedido foi processado.'}
        </p>

        {possuiAlertaOperacional && (
          <p className="mb-6 rounded-2xl border border-amber-400/35 bg-amber-400/10 p-4 text-left text-sm font-bold text-amber-100">
            {pedido.alerta_operacional}
          </p>
        )}

        <div className="mb-6 rounded-2xl border border-[#2A2A2A] bg-[#080808] p-6 text-left">
          <p className="text-sm text-[#A0A0A0]">Número do Pedido:</p>
          <p className="font-mono text-2xl font-black text-[#C9A227]" data-testid="numero-pedido">
            {pedido.numero_pedido}
          </p>
          <p className="mt-4 text-sm text-[#A0A0A0]">Total:</p>
          <p className="text-2xl font-bold">{formatPrice(pedido.total)}</p>
          <div className="mt-5 grid gap-3 rounded-2xl border border-[#2A2A2A] bg-black/40 p-4 text-sm text-[#D8D8D8] sm:grid-cols-2">
            <div>
              <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">Pagamento</p>
              <p className="mt-1 font-bold text-white">{pedido.status_pagamento || 'Aguardando pagamento'}</p>
              <p className="text-[#A0A0A0]">
                {getPagamentoLabel(pedido.meio_pagamento || '')}
                {pedido.parcelas > 1 ? ` · ${pedido.parcelas}x` : ''}
              </p>
            </div>
            <div>
              <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">Entrega</p>
              <p className="mt-1 font-bold text-white">{pedido.frete_metodo || 'Entrega a combinar'}</p>
              <p className="text-[#A0A0A0]">{pedido.frete_transportadora || 'Nexum Altivon'}</p>
            </div>
            {pedido.status_fiscal && (
              <div>
                <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">Fiscal</p>
                <p className="mt-1 font-bold text-white">{pedido.status_fiscal}</p>
              </div>
            )}
          </div>
          {pedido.instrucao_pagamento && (
            <p className="mt-4 rounded-xl border border-[#C9A227]/30 bg-[#C9A227]/10 p-3 text-sm font-semibold text-[#F7E7A6]">
              {pedido.instrucao_pagamento}
            </p>
          )}
          {pedido.pix_qrcode && (
            <div className="mt-4 rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-4 text-left text-sm text-emerald-100">
              <p className="font-black uppercase tracking-[0.14em]">PIX Copia e Cola</p>
              <textarea
                readOnly
                value={pedido.pix_qrcode}
                className="mt-3 min-h-[120px] w-full rounded-xl border border-emerald-500/20 bg-black/30 p-3 font-mono text-xs text-emerald-50"
              />
            </div>
          )}
          {pedido.payment_url && (
            <div className="mt-4 rounded-xl border border-sky-500/30 bg-sky-500/10 p-4 text-left text-sm text-sky-100">
              <p className="font-black uppercase tracking-[0.14em]">Link de pagamento</p>
              <a
                href={pedido.payment_url}
                target="_blank"
                rel="noreferrer"
                className="mt-3 inline-flex rounded-full bg-sky-400 px-4 py-2 font-black text-slate-950 transition hover:bg-sky-300"
              >
                Abrir cobrança
              </a>
            </div>
          )}
        </div>

        <div className="flex flex-col justify-center gap-3 sm:flex-row">
          <Link
            to={acompanharPedidoUrl}
            className="rounded-full border border-[#C9A227]/40 px-8 py-3 font-black text-[#E8D5A3] transition hover:border-[#E8D5A3] hover:text-white"
          >
            Acompanhar Pedido
          </Link>
          <button onClick={onContinue}
            className="rounded-full bg-[#C9A227] px-8 py-3 font-black text-black hover:bg-[#FFD95A]">
            Voltar para a Loja
          </button>
        </div>
      </div>
    </div>
  );
}
