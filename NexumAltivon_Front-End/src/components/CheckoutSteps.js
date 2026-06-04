import { User, MapPin, CreditCard } from 'lucide-react';
import { getPagamentoLabel } from '../utils/formatters';

// Step 1: Dados Pessoais
export function StepDadosPessoais({ dadosCliente, setDadosCliente, onNext }) {
  const isValid = dadosCliente.nome && dadosCliente.email;

  return (
    <div>
      <h2 className="text-2xl font-bold mb-4 flex items-center">
        <User className="mr-2 text-amber-600" /> Dados Pessoais
      </h2>
      <div className="space-y-4">
        <input
          type="text"
          required
          placeholder="Nome Completo *"
          value={dadosCliente.nome}
          onChange={(e) => setDadosCliente({ ...dadosCliente, nome: e.target.value })}
          className="w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
          data-testid="checkout-nome"
        />
        <input
          type="email"
          required
          placeholder="Email *"
          value={dadosCliente.email}
          onChange={(e) => setDadosCliente({ ...dadosCliente, email: e.target.value })}
          className="w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
          data-testid="checkout-email"
        />
        <input
          type="text"
          placeholder="CPF"
          value={dadosCliente.cpf}
          onChange={(e) => setDadosCliente({ ...dadosCliente, cpf: e.target.value })}
          className="w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
        />
        <input
          type="tel"
          placeholder="Telefone"
          value={dadosCliente.telefone}
          onChange={(e) => setDadosCliente({ ...dadosCliente, telefone: e.target.value })}
          className="w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
        />
      </div>
      <button
        onClick={onNext}
        disabled={!isValid}
        className="mt-6 bg-amber-500 hover:bg-amber-600 text-white px-6 py-3 rounded-lg font-semibold transition disabled:opacity-50"
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
      <h2 className="text-2xl font-bold mb-4 flex items-center">
        <MapPin className="mr-2 text-amber-600" /> Endereço de Entrega
      </h2>
      <div className="grid grid-cols-2 gap-4">
        <input type="text" required placeholder="CEP *" value={endereco.cep}
          onChange={(e) => setEndereco({ ...endereco, cep: e.target.value })}
          className="px-4 py-3 border rounded-lg" />
        <input type="text" required placeholder="Logradouro *" value={endereco.logradouro}
          onChange={(e) => setEndereco({ ...endereco, logradouro: e.target.value })}
          className="px-4 py-3 border rounded-lg" />
        <input type="text" required placeholder="Número *" value={endereco.numero}
          onChange={(e) => setEndereco({ ...endereco, numero: e.target.value })}
          className="px-4 py-3 border rounded-lg" />
        <input type="text" placeholder="Complemento" value={endereco.complemento}
          onChange={(e) => setEndereco({ ...endereco, complemento: e.target.value })}
          className="px-4 py-3 border rounded-lg" />
        <input type="text" required placeholder="Bairro *" value={endereco.bairro}
          onChange={(e) => setEndereco({ ...endereco, bairro: e.target.value })}
          className="px-4 py-3 border rounded-lg" />
        <input type="text" required placeholder="Cidade *" value={endereco.cidade}
          onChange={(e) => setEndereco({ ...endereco, cidade: e.target.value })}
          className="px-4 py-3 border rounded-lg" />
        <input type="text" required placeholder="Estado *" value={endereco.estado}
          onChange={(e) => setEndereco({ ...endereco, estado: e.target.value })}
          className="px-4 py-3 border rounded-lg col-span-2" />
      </div>
      <div className="flex gap-3 mt-6">
        <button onClick={onBack} className="bg-gray-300 hover:bg-gray-400 text-gray-800 px-6 py-3 rounded-lg font-semibold">
          ← Voltar
        </button>
        <button onClick={onNext} disabled={!isValid}
          className="bg-amber-500 hover:bg-amber-600 text-white px-6 py-3 rounded-lg font-semibold transition disabled:opacity-50">
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
      <h2 className="text-2xl font-bold mb-4 flex items-center">
        <CreditCard className="mr-2 text-amber-600" /> Pagamento
      </h2>

      <div className="mb-4">
        <label className="block text-sm font-semibold mb-2">Loja</label>
        <select value={lojaId} onChange={(e) => setLojaId(e.target.value)}
          className="w-full px-4 py-3 border rounded-lg">
          {lojas.map((loja) => (
            <option key={loja.id} value={loja.id}>{loja.nome}</option>
          ))}
        </select>
      </div>

      <div className="space-y-3 mb-6">
        <label className="block font-semibold mb-2">Método de Pagamento</label>
        {metodos.map((metodo) => (
          <label key={metodo} className="flex items-center p-4 border rounded-lg cursor-pointer hover:bg-gray-50">
            <input
              type="radio"
              name="pagamento"
              value={metodo}
              checked={metodoPagamento === metodo}
              onChange={(e) => setMetodoPagamento(e.target.value)}
              className="mr-3"
            />
            <span className="capitalize font-semibold">{getPagamentoLabel(metodo)}</span>
          </label>
        ))}
      </div>

      <div className="flex gap-3">
        <button onClick={onBack} className="bg-gray-300 hover:bg-gray-400 text-gray-800 px-6 py-3 rounded-lg font-semibold">
          ← Voltar
        </button>
        <button onClick={onConfirm} disabled={loading}
          className="flex-1 bg-amber-500 hover:bg-amber-600 text-white px-6 py-3 rounded-lg font-semibold transition disabled:opacity-50"
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
    <div className="bg-white p-6 rounded-lg shadow-md h-fit">
      <h3 className="text-xl font-bold mb-4">Resumo</h3>
      <div className="space-y-2 mb-4">
        {cart.map((item) => (
          <div key={item.id} className="flex justify-between text-sm">
            <span>{item.nome} x {item.quantity}</span>
            <span>{formatPrice((item.preco_promocional || item.preco) * item.quantity)}</span>
          </div>
        ))}
      </div>
      <div className="border-t pt-4 space-y-2">
        <div className="flex justify-between"><span>Subtotal:</span><span>{formatPrice(subtotal)}</span></div>
        {desconto > 0 && (
          <div className="flex justify-between text-green-600">
            <span>Desconto:</span><span>-{formatPrice(desconto)}</span>
          </div>
        )}
        <div className="flex justify-between text-xl font-bold border-t pt-2">
          <span>Total:</span>
          <span className="text-amber-600">{formatPrice(total)}</span>
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
            step >= s ? 'bg-amber-500 text-white' : 'bg-gray-300 text-gray-600'
          }`}>
            {s}
          </div>
          {s < 3 && <div className={`flex-1 h-1 mx-2 ${step > s ? 'bg-amber-500' : 'bg-gray-300'}`}></div>}
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
    <div className="min-h-screen bg-gray-50 py-12 flex items-center justify-center">
      <div className="max-w-2xl bg-white p-8 rounded-lg shadow-md text-center" data-testid="pedido-sucesso">
        <div className="text-green-500 text-7xl mb-4">✓</div>
        <h2 className="text-3xl font-bold text-gray-900 mb-2">Pedido Realizado!</h2>
        <p className="text-gray-600 mb-6">Seu pedido foi processado com sucesso.</p>

        <div className="bg-gray-50 p-6 rounded-lg mb-6 text-left">
          <p className="text-sm text-gray-600">Número do Pedido:</p>
          <p className="text-2xl font-mono font-bold text-amber-600" data-testid="numero-pedido">
            {pedido.numero_pedido}
          </p>
          <p className="mt-4 text-sm text-gray-600">Total:</p>
          <p className="text-2xl font-bold">{formatPrice(pedido.total)}</p>
        </div>

        <button onClick={onContinue}
          className="bg-amber-500 hover:bg-amber-600 text-white px-8 py-3 rounded-lg font-semibold">
          Voltar para a Loja
        </button>
      </div>
    </div>
  );
}
