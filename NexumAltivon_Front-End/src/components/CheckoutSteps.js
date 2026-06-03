import { User, MapPin, CreditCard } from 'lucide-react';
import { getPagamentoLabel } from '../utils/formatters';

// Step 1: Dados Pessoais
export function StepDadosPessoais({ dadosCliente, setDadosCliente, onNext }) {
  const isValid = dadosCliente.nome && dadosCliente.email;

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

  return (
    <div>
      <h2 className="mb-4 flex items-center text-2xl font-black text-white">
        <MapPin className="mr-2 text-[#C9A227]" /> Endereço de Entrega
      </h2>
      <div className="grid grid-cols-2 gap-4">
        <input type="text" required placeholder="CEP *" value={endereco.cep}
          onChange={(e) => setEndereco({ ...endereco, cep: e.target.value })}
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
export function StepPagamento({ lojas, lojaId, setLojaId, metodoPagamento, setMetodoPagamento, onBack, onConfirm, loading }) {
  const metodos = ['cartao', 'pix', 'boleto'];

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
            <option key={loja.id} value={loja.id}>{loja.nome}</option>
          ))}
        </select>
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
              Integração via API
            </span>
          </label>
        ))}
      </div>

      <div className="flex gap-3">
        <button onClick={onBack} className="rounded-full border border-[#2A2A2A] bg-[#080808] px-6 py-3 font-bold text-[#D8D8D8] hover:border-[#C9A227]">
          ← Voltar
        </button>
        <button onClick={onConfirm} disabled={loading}
          className="flex-1 rounded-full bg-[#C9A227] px-6 py-3 font-black text-black transition hover:bg-[#FFD95A] disabled:opacity-50"
          data-testid="finalizar-pedido-btn">
          {loading ? 'Processando...' : 'Confirmar Pedido'}
        </button>
      </div>
    </div>
  );
}

// Resumo do pedido
export function ResumoPedido({ cart, subtotal, desconto, total }) {
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
export function CheckoutSuccess({ pedido, onContinue }) {
  const formatPrice = (price) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(price);

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#050505] py-12 text-white">
      <div className="max-w-2xl rounded-3xl border border-[#2A2A2A] bg-[#111111] p-8 text-center shadow-[0_28px_100px_rgba(0,0,0,0.42)]" data-testid="pedido-sucesso">
        <div className="mb-4 text-7xl text-emerald-400">✓</div>
        <h2 className="mb-2 text-3xl font-black text-white">Pedido Realizado!</h2>
        <p className="mb-6 text-[#A0A0A0]">Seu pedido foi processado com sucesso.</p>

        <div className="mb-6 rounded-2xl border border-[#2A2A2A] bg-[#080808] p-6 text-left">
          <p className="text-sm text-[#A0A0A0]">Número do Pedido:</p>
          <p className="font-mono text-2xl font-black text-[#C9A227]" data-testid="numero-pedido">
            {pedido.numero_pedido}
          </p>
          <p className="mt-4 text-sm text-[#A0A0A0]">Total:</p>
          <p className="text-2xl font-bold">{formatPrice(pedido.total)}</p>
        </div>

        <button onClick={onContinue}
          className="rounded-full bg-[#C9A227] px-8 py-3 font-black text-black hover:bg-[#FFD95A]">
          Voltar para a Loja
        </button>
      </div>
    </div>
  );
}
