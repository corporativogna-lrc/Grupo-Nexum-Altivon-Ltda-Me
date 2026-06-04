import { useState, useEffect, useCallback } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { lojaAPI, clienteAPI, pedidoAPI } from '../services/api';
import { fallbackCategories } from '../data/mockStore';
import {
  StepDadosPessoais,
  StepEndereco,
  StepPagamento,
  ResumoPedido,
  CheckoutStepper,
  CheckoutSuccess,
} from '../components/CheckoutSteps';

// Helper: calcula desconto baseado no cupom aplicado
function calcularDesconto(cupomAplicado, subtotal) {
  if (!cupomAplicado) return 0;
  if (cupomAplicado.valor_minimo && subtotal < cupomAplicado.valor_minimo) return 0;
  if (cupomAplicado.desconto_percentual) {
    return subtotal * (cupomAplicado.desconto_percentual / 100);
  }
  return cupomAplicado.desconto_valor || 0;
}

export default function Checkout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { cart, getTotal, clearCart } = useCart();
  const cupomAplicado = location.state?.cupomAplicado;

  const [lojas, setLojas] = useState([]);
  const [step, setStep] = useState(1);
  const [pedidoCriado, setPedidoCriado] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const [dadosCliente, setDadosCliente] = useState({
    nome: '', email: '', cpf: '', telefone: ''
  });

  const [endereco, setEndereco] = useState({
    cep: '', logradouro: '', numero: '', complemento: '',
    bairro: '', cidade: '', estado: ''
  });

  const [lojaId, setLojaId] = useState('');
  const [metodoPagamento, setMetodoPagamento] = useState('cartao');

  const loadLojas = useCallback(async () => {
    try {
      const response = await lojaAPI.getAll();
      const lojasApi = Array.isArray(response.data) ? response.data : [];
      setLojas(lojasApi);
      if (lojasApi.length > 0) setLojaId(lojasApi[0].id);
    } catch (err) {
      if (process.env.NODE_ENV === 'development') console.error(err);
      const fallbackLojas = fallbackCategories.map((categoria, index) => ({
        id: categoria.id,
        nome: categoria.nome,
        slug: categoria.id,
      }));
      setLojas(fallbackLojas);
      if (fallbackLojas.length > 0) setLojaId(fallbackLojas[0].id);
    }
  }, []);

  useEffect(() => {
    if (cart.length === 0 && !pedidoCriado) {
      navigate('/carrinho');
      return;
    }
    loadLojas();
  }, [cart.length, pedidoCriado, navigate, loadLojas]);

  const subtotal = getTotal();
  const desconto = calcularDesconto(cupomAplicado, subtotal);
  const total = subtotal - desconto;

  const finalizarPedido = async () => {
    setLoading(true);
    setError('');

    try {
      const clienteRes = await clienteAPI.create(dadosCliente);
      const clienteId = clienteRes.data.id;

      if (!clienteId) {
        throw new Error('Cliente nao confirmado pela API.');
      }

      const pedidoData = {
        cliente_id: clienteId,
        loja_id: lojaId,
        itens: cart.map(item => ({
          produto_id: item.id,
          quantidade: item.quantity
        })),
        cupom_codigo: cupomAplicado?.codigo,
        endereco_entrega: endereco
      };

      const pedidoRes = await pedidoAPI.create(pedidoData);
      setPedidoCriado(pedidoRes.data);

      clearCart();
      setStep(4);
    } catch (err) {
      const detail = err.response?.data?.detail;
      setError(detail || err.message || 'Erro ao processar pedido. Nenhum pedido foi registrado.');
    } finally {
      setLoading(false);
    }
  };

  // Success page
  if (step === 4 && pedidoCriado) {
    return <CheckoutSuccess pedido={pedidoCriado} onContinue={() => navigate('/')} />;
  }

  return (
    <div className="min-h-screen bg-[#050505] py-12 text-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="mb-8">
          <p className="text-sm font-black uppercase tracking-[0.22em] text-[#C9A227]">Checkout integrado</p>
          <h1 className="mt-2 text-4xl font-black text-white" data-testid="checkout-title">
            Finalizar Compra
          </h1>
          <p className="mt-3 max-w-3xl text-[#A0A0A0]">
            Cadastro do cliente, endereço, loja, forma de pagamento e criação do pedido seguem o fluxo real da API.
          </p>
        </div>

        {error && (
          <div className="mb-4 rounded-xl border border-rose-500/30 bg-rose-500/10 px-4 py-3 text-rose-200"
            data-testid="checkout-error">
            {error}
          </div>
        )}

        <CheckoutStepper step={step} />

        <div className="grid lg:grid-cols-3 gap-8">
          <div className="rounded-2xl border border-[#2A2A2A] bg-[#111111] p-6 shadow-[0_24px_80px_rgba(0,0,0,0.35)] lg:col-span-2">
            {step === 1 && (
              <StepDadosPessoais
                dadosCliente={dadosCliente}
                setDadosCliente={setDadosCliente}
                onNext={() => setStep(2)}
              />
            )}
            {step === 2 && (
              <StepEndereco
                endereco={endereco}
                setEndereco={setEndereco}
                onBack={() => setStep(1)}
                onNext={() => setStep(3)}
              />
            )}
            {step === 3 && (
              <StepPagamento
                lojas={lojas}
                lojaId={lojaId}
                setLojaId={setLojaId}
                metodoPagamento={metodoPagamento}
                setMetodoPagamento={setMetodoPagamento}
                onBack={() => setStep(2)}
                onConfirm={finalizarPedido}
                loading={loading}
              />
            )}
          </div>

          <ResumoPedido cart={cart} subtotal={subtotal} desconto={desconto} total={total} />
        </div>
      </div>
    </div>
  );
}
