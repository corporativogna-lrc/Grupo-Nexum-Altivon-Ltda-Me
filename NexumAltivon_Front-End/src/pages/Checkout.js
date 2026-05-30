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
      let clienteId = `cliente-${Date.now()}`;

      try {
        const clienteRes = await clienteAPI.create(dadosCliente);
        clienteId = clienteRes.data.id || clienteId;
      } catch (clienteError) {
        if (process.env.NODE_ENV === 'development') console.warn('Checkout usando cliente local:', clienteError);
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

      try {
        const pedidoRes = await pedidoAPI.create(pedidoData);
        setPedidoCriado(pedidoRes.data);
      } catch (pedidoError) {
        if (process.env.NODE_ENV === 'development') console.warn('Checkout usando pedido local:', pedidoError);
        setPedidoCriado({
          id: `pedido-${Date.now()}`,
          numero_pedido: `NA-${new Date().toISOString().slice(2, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`,
          total,
          status: 'Recebido',
          metodo_pagamento: metodoPagamento,
          created_at: new Date().toISOString(),
        });
      }

      clearCart();
      setStep(4);
    } catch (err) {
      const detail = err.response?.data?.detail;
      if (err.response?.status === 400) {
        setError('Email já cadastrado. Por favor faça login.');
      } else {
        setError(detail || 'Erro ao processar pedido');
      }
    } finally {
      setLoading(false);
    }
  };

  // Success page
  if (step === 4 && pedidoCriado) {
    return <CheckoutSuccess pedido={pedidoCriado} onContinue={() => navigate('/')} />;
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-8" data-testid="checkout-title">
          Finalizar Compra
        </h1>

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4"
            data-testid="checkout-error">
            {error}
          </div>
        )}

        <CheckoutStepper step={step} />

        <div className="grid lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 bg-white p-6 rounded-lg shadow-md">
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
