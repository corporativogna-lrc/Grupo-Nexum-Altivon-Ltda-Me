/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useCart } from '../context/CartContext';
import { Link, useNavigate } from 'react-router-dom';
import { Trash2, Plus, Minus, ShoppingBag } from 'lucide-react';
import { useState } from 'react';
import api, { produtoAPI } from '../services/api';

export default function Carrinho() {
  const { cart, removeFromCart, updateQuantity, getTotal, clearCart } = useCart();
  const navigate = useNavigate();
  const [cupom, setCupom] = useState('');
  const [cupomAplicado, setCupomAplicado] = useState(null);
  const [error, setError] = useState('');
  const [validatingCart, setValidatingCart] = useState(false);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(price);
  };

  const validarCupom = async () => {
    if (!cupom) return;
    setError('');
    try {
      const response = await api.get(`/cupons/${cupom.toUpperCase()}`);
      setCupomAplicado(response.data);
    } catch (err) {
      const codigo = cupom.toUpperCase();
      const cuponsLocais = {
        NEXUM10: { codigo: 'NEXUM10', desconto_percentual: 10, valor_minimo: 500 },
        FRETEGRATIS: { codigo: 'FRETEGRATIS', desconto_valor: 89, valor_minimo: 1000 },
      };
      if (cuponsLocais[codigo]) {
        setCupomAplicado(cuponsLocais[codigo]);
        return;
      }

      setError(err.response?.data?.detail || err.response?.data?.mensagem || 'Cupom inválido');
      setCupomAplicado(null);
    }
  };

  const calcularDesconto = () => {
    if (!cupomAplicado) return 0;
    const subtotal = getTotal();
    if (cupomAplicado.valor_minimo && subtotal < cupomAplicado.valor_minimo) return 0;
    if (cupomAplicado.desconto_percentual) return subtotal * (cupomAplicado.desconto_percentual / 100);
    return cupomAplicado.desconto_valor || 0;
  };

  const subtotal = getTotal();
  const desconto = calcularDesconto();
  const total = subtotal - desconto;

  const handleCheckout = async () => {
    if (cart.length === 0) return;
    setError('');
    setValidatingCart(true);

    try {
      let houveAjuste = false;

      await Promise.all(cart.map(async (item) => {
        try {
          const response = await produtoAPI.getById(item.id);
          const produtoAtual = response.data?.dados || response.data?.data || response.data;
          const estoqueAtual = Number(produtoAtual?.estoque || 0);

          if (!produtoAtual || estoqueAtual <= 0) {
            removeFromCart(item.id);
            houveAjuste = true;
            return;
          }

          if (item.quantity > estoqueAtual) {
            updateQuantity(item.id, estoqueAtual);
            houveAjuste = true;
          }
        } catch {
          removeFromCart(item.id);
          houveAjuste = true;
        }
      }));

      if (houveAjuste) {
        setError('O carrinho foi atualizado porque um item não está mais liberado para venda.');
        return;
      }

      navigate('/checkout', { state: { cupomAplicado } });
    } finally {
      setValidatingCart(false);
    }
  };

  if (cart.length === 0) {
    return (
      <div className="min-h-screen bg-[#050505] py-12 text-white">
        <div className="max-w-3xl mx-auto px-4 text-center">
          <ShoppingBag className="mx-auto mb-4 text-[#C9A227]" size={64} />
          <h2 className="mb-4 text-3xl font-black text-white">Carrinho Vazio</h2>
          <p className="mb-8 text-[#A0A0A0]">Adicione produtos reais do catálogo para continuar</p>
          <Link to="/produtos" className="inline-block rounded-full bg-[#C9A227] px-8 py-3 font-black text-black transition hover:bg-[#FFD95A]">
            Continuar Comprando
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#050505] py-12 text-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="mb-8">
          <p className="text-sm font-black uppercase tracking-[0.22em] text-[#C9A227]">Finalização operacional</p>
          <h1 className="mt-2 text-4xl font-black text-white" data-testid="carrinho-title">Meu Carrinho</h1>
          <p className="mt-3 max-w-2xl text-[#A0A0A0]">Itens, quantidades, cupons e total são mantidos no fluxo real do pedido.</p>
        </div>

        <div className="grid lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 space-y-4">
            {cart.map((item) => {
              const price = item.preco_promocional || item.preco;
              return (
                <div key={item.id} className="flex items-center gap-4 rounded-2xl border border-[#2A2A2A] bg-[#111111] p-6 shadow-[0_24px_80px_rgba(0,0,0,0.35)]" data-testid={`cart-item-${item.id}`}>
                  <img src={item.imagem_url || item.imagem} alt={item.nome} className="h-24 w-24 rounded-xl object-cover" />
                  <div className="flex-1">
                    <h3 className="text-lg font-black text-white">{item.nome}</h3>
                    <p className="font-bold text-[#C9A227]">{formatPrice(price)}</p>
                  </div>
                  <div className="flex items-center overflow-hidden rounded-full border border-[#2A2A2A] bg-[#080808]">
                    <button onClick={() => updateQuantity(item.id, item.quantity - 1)} className="p-3 hover:bg-[#1A1A1A]">
                      <Minus size={16} />
                    </button>
                    <span className="w-12 text-center font-black">{item.quantity}</span>
                    <button onClick={() => updateQuantity(item.id, item.quantity + 1)} className="p-3 hover:bg-[#1A1A1A]">
                      <Plus size={16} />
                    </button>
                  </div>
                  <button onClick={() => removeFromCart(item.id)} className="rounded-full p-2 text-rose-400 hover:bg-rose-500/10">
                    <Trash2 size={20} />
                  </button>
                </div>
              );
            })}
            <button onClick={clearCart} className="text-rose-400 hover:underline">Limpar Carrinho</button>
          </div>

          <div className="h-fit rounded-2xl border border-[#2A2A2A] bg-[#111111] p-6 shadow-[0_24px_80px_rgba(0,0,0,0.35)]">
            <h2 className="mb-4 text-xl font-black text-white">Resumo do Pedido</h2>
            <div className="mb-4">
              <label className="mb-2 block text-sm font-bold text-[#D8D8D8]">Cupom de Desconto</label>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={cupom}
                  onChange={(e) => setCupom(e.target.value.toUpperCase())}
                  placeholder="Código do cupom"
                  className="flex-1 rounded-xl border border-[#2A2A2A] bg-[#080808] px-3 py-2 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
                  data-testid="cupom-input"
                />
                <button onClick={validarCupom} className="rounded-xl bg-[#C9A227] px-4 py-2 font-black text-black hover:bg-[#FFD95A]" data-testid="aplicar-cupom-btn">
                  Aplicar
                </button>
              </div>
              {error && <p className="mt-2 text-sm text-rose-400">{error}</p>}
              {cupomAplicado && <p className="mt-2 text-sm text-emerald-400">✓ Cupom {cupomAplicado.codigo} aplicado!</p>}
            </div>

            <div className="space-y-2 border-t border-[#2A2A2A] pt-4 text-[#D8D8D8]">
              <div className="flex justify-between"><span>Subtotal:</span><span>{formatPrice(subtotal)}</span></div>
              {desconto > 0 && <div className="flex justify-between text-emerald-400"><span>Desconto:</span><span>-{formatPrice(desconto)}</span></div>}
              <div className="flex justify-between border-t border-[#2A2A2A] pt-2 text-2xl font-black text-white">
                <span>Total:</span>
                <span className="text-[#C9A227]" data-testid="total-carrinho">{formatPrice(total)}</span>
              </div>
            </div>

            <button
              onClick={handleCheckout}
              disabled={validatingCart}
              className="mt-6 w-full rounded-full bg-[#C9A227] py-3 font-black text-black transition hover:bg-[#FFD95A] disabled:opacity-50"
              data-testid="checkout-btn"
            >
              {validatingCart ? 'Validando carrinho...' : 'Finalizar Compra'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
