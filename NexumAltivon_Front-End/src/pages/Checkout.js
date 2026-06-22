import { useState, useEffect, useCallback } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { lojaAPI, clienteAPI, freteAPI, pedidoAPI, unwrapApiData } from '../services/api';
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

function mapGatewayLabel(metodoPagamento) {
  switch (metodoPagamento) {
    case 'pix':
      return 'PIX';
    case 'boleto':
      return 'Boleto';
    case 'debito':
      return 'Debito';
    case 'deposito':
      return 'Deposito';
    default:
      return 'Cartao';
  }
}

export default function Checkout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { cart, getTotal, clearCart } = useCart();
  const { isAuthenticated, isAdmin, user } = useAuth();
  const cupomAplicado = location.state?.cupomAplicado;

  const [lojas, setLojas] = useState([]);
  const [step, setStep] = useState(1);
  const [pedidoCriado, setPedidoCriado] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [checkoutInfo, setCheckoutInfo] = useState('');
  const [clientePortal, setClientePortal] = useState(null);

  const [dadosCliente, setDadosCliente] = useState({
    nome: '', email: '', cpf: '', telefone: ''
  });

  const [endereco, setEndereco] = useState({
    cep: '', logradouro: '', numero: '', complemento: '',
    bairro: '', cidade: '', estado: ''
  });

  const [lojaId, setLojaId] = useState('');
  const [metodoPagamento, setMetodoPagamento] = useState('cartao');
  const [parcelas, setParcelas] = useState(1);
  const [dadosCartao, setDadosCartao] = useState({
    numero: '',
    nomeTitular: '',
    validade: '',
    cvv: '',
    cpfTitular: '',
  });
  const [freteSelecionado, setFreteSelecionado] = useState('padrao');
  const [freteOptions, setFreteOptions] = useState([
    { id: 'retirada', nome: 'Retirada / combinar entrega', transportadora: 'Nexum Altivon', prazo: 0, valor: 0 },
    { id: 'padrao', nome: 'Entrega padrão', transportadora: 'Tabela local', prazo: 7, valor: 29.9 },
    { id: 'expresso', nome: 'Entrega expressa', transportadora: 'Tabela local', prazo: 3, valor: 49.9 },
  ]);

  const loadLojas = useCallback(async () => {
    try {
      const response = await lojaAPI.getAll();
      const lojasApi = Array.isArray(unwrapApiData(response.data)) ? unwrapApiData(response.data) : [];
      setLojas(lojasApi);
      if (lojasApi.length > 0) setLojaId(String(lojasApi[0].id));
    } catch (err) {
      if (process.env.NODE_ENV === 'development') console.error(err);
      const fallbackLojas = fallbackCategories.map((categoria, index) => ({
        id: categoria.id,
        nome: categoria.nome,
        slug: categoria.id,
      }));
      setLojas(fallbackLojas);
      if (fallbackLojas.length > 0) setLojaId(String(fallbackLojas[0].id));
    }
  }, []);

  useEffect(() => {
    if (cart.length === 0 && !pedidoCriado) {
      navigate('/carrinho');
      return;
    }
    loadLojas();
  }, [cart.length, pedidoCriado, navigate, loadLojas]);

  useEffect(() => {
    if (!isAuthenticated || isAdmin) {
      setClientePortal(null);
      setDadosCliente((current) => ({
        ...current,
        nome: current.nome || user?.nome || '',
        email: current.email || user?.email || '',
      }));
      return;
    }

    let cancelled = false;

    const hydrateLoggedCustomer = async () => {
      try {
        const response = await clienteAPI.getPortal();
        const portal = response.data?.dados || response.data?.Dados || response.data?.data || response.data;
        if (cancelled || !portal) return;

        setClientePortal(portal);
        setDadosCliente((current) => ({
          ...current,
          nome: portal.nome || current.nome || user?.nome || '',
          email: portal.email || current.email || user?.email || '',
          cpf: portal.documento || current.cpf || '',
          telefone: portal.telefone || current.telefone || '',
        }));
        setCheckoutInfo('Cadastro do cliente logado carregado automaticamente para agilizar o checkout.');
      } catch {
        if (cancelled) return;
        setClientePortal(null);
        setDadosCliente((current) => ({
          ...current,
          nome: current.nome || user?.nome || '',
          email: current.email || user?.email || '',
        }));
      }
    };

    hydrateLoggedCustomer();

    return () => {
      cancelled = true;
    };
  }, [isAuthenticated, isAdmin, user]);

  const subtotal = getTotal();
  const desconto = calcularDesconto(cupomAplicado, subtotal);
  const frete = freteOptions.find((item) => item.id === freteSelecionado) || freteOptions[1];
  const total = subtotal + frete.valor - desconto;

  useEffect(() => {
    const cepDestino = endereco.cep?.replace(/\D/g, '');
    if (!cepDestino || cepDestino.length < 8 || cart.length === 0) return;

    let cancelled = false;
    const timer = setTimeout(async () => {
      try {
        const response = await freteAPI.cotar({
          cep_destino: cepDestino,
          itens: cart.map((item) => ({
            sku: item.sku || item.id,
            quantidade: item.quantity,
            valor_unitario: item.preco_promocional || item.preco || item.price || 0,
            peso_kg: item.peso_kg || 0.5,
            altura_cm: item.altura_cm || 8,
            largura_cm: item.largura_cm || 16,
            comprimento_cm: item.comprimento_cm || 24,
          })),
        });

        const cotacoesData = unwrapApiData(response.data);
        const cotacoes = Array.isArray(cotacoesData) ? cotacoesData : [];
        if (!cancelled && cotacoes.length > 0) {
          const mapped = [
            { id: 'retirada', nome: 'Retirada / combinar entrega', transportadora: 'Nexum Altivon', prazo: 0, valor: 0 },
            ...cotacoes.map((cotacao) => ({
              id: cotacao.codigo,
              nome: cotacao.nome,
              transportadora: cotacao.transportadora,
              prazo: cotacao.prazoDias ?? cotacao.prazo_dias,
              valor: cotacao.valor,
            })),
          ];
          setFreteOptions(mapped);
          if (!mapped.some((option) => option.id === freteSelecionado)) {
            setFreteSelecionado(mapped[1]?.id || 'retirada');
          }
        }
      } catch {
        // O checkout não deve cair por oscilação externa: mantém a tabela local.
      }
    }, 450);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [cart, endereco.cep, freteSelecionado]);

  const finalizarPedido = async () => {
    setLoading(true);
    setError('');
    const previousCheckoutInfo = checkoutInfo;
    setCheckoutInfo('');

    try {
      let clienteId = clientePortal?.id;

      if (clienteId) {
        setCheckoutInfo('Pedido vinculado diretamente ao cadastro do cliente logado.');
      } else {
        try {
          const clienteVerificacao = await clienteAPI.verificarCadastro({
            email: dadosCliente.email,
            cpf: dadosCliente.cpf,
          });

          clienteId = clienteVerificacao.data?.cliente?.id;
          if (clienteId) {
            setCheckoutInfo('Cliente já existente reaproveitado para não duplicar cadastro.');
          }
        } catch {
          setCheckoutInfo('Verificação de cadastro indisponível. Seguindo direto para o cadastro do cliente.');
        }

        if (!clienteId) {
          const clienteRes = await clienteAPI.create(dadosCliente);
          clienteId = clienteRes.data?.id ?? clienteRes.data?.dados?.id ?? clienteRes.data?.cliente?.id;
          setCheckoutInfo('Novo cliente registrado e vinculado ao pedido.');
        }
      }

      if (!clienteId) {
        throw new Error('Cliente nao confirmado pela API.');
      }

      const pedidoData = {
        cliente_id: clienteId,
        loja_id: lojaId,
        itens: cart.map(item => ({
          produtoId: String(item.slug || item.sku || item.id),
          quantidade: item.quantity
        })),
        cupom_codigo: cupomAplicado?.codigo,
        endereco_entrega: endereco,
        metodo_pagamento: metodoPagamento,
        parcelas: metodoPagamento === 'cartao' ? parcelas : 1,
        gateway_pagamento: mapGatewayLabel(metodoPagamento),
        dados_cartao: metodoPagamento === 'cartao' ? dadosCartao : undefined,
        frete_valor: frete.valor,
        frete_metodo: frete.id,
        frete_transportadora: frete.transportadora,
        frete_prazo_dias: frete.prazo
      };

      const pedidoRes = await pedidoAPI.create({
        ...pedidoData,
        loja_id: String(lojaId),
      });
      setPedidoCriado(pedidoRes.data);

      clearCart();
      setStep(4);
    } catch (err) {
      const detail = err.response?.data?.detail;
      setError(detail || err.message || 'Erro ao processar pedido. Nenhum pedido foi registrado.');
      setCheckoutInfo(previousCheckoutInfo);
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

        {checkoutInfo && (
          <div className="mb-4 rounded-xl border border-emerald-500/30 bg-emerald-500/10 px-4 py-3 text-emerald-200">
            {checkoutInfo}
          </div>
        )}

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
                parcelas={parcelas}
                setParcelas={setParcelas}
                dadosCartao={dadosCartao}
                setDadosCartao={setDadosCartao}
                freteOptions={freteOptions}
                freteSelecionado={freteSelecionado}
                setFreteSelecionado={setFreteSelecionado}
                onBack={() => setStep(2)}
                onConfirm={finalizarPedido}
                loading={loading}
              />
            )}
          </div>

          <ResumoPedido cart={cart} subtotal={subtotal} desconto={desconto} frete={frete} total={total} />
        </div>
      </div>
    </div>
  );
}
