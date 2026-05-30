import { useCart } from '../context/CartContext';
import { Link, useNavigate } from 'react-router-dom';
import { Trash2, Plus, Minus, ShoppingBag } from 'lucide-react';
import { useState } from 'react';
import api from '../services/api';

export default function Carrinho() {
  const { cart, removeFromCart, updateQuantity, getTotal, clearCart } = useCart();
  const navigate = useNavigate();
  const [cupom, setCupom] = useState('');
  const [cupomAplicado, setCupomAplicado] = useState(null);
  const [error, setError] = useState('');

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

  const handleCheckout = () => {
    if (cart.length === 0) return;
    navigate('/checkout', { state: { cupomAplicado } });
  };

  if (cart.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="max-w-3xl mx-auto px-4 text-center">
          <ShoppingBag className="mx-auto text-gray-400 mb-4" size={64} />
          <h2 className="text-3xl font-bold text-gray-900 mb-4">Carrinho Vazio</h2>
          <p className="text-gray-600 mb-8">Adicione produtos ao seu carrinho para continuar</p>
          <Link to="/produtos" className="inline-block bg-amber-500 hover:bg-amber-600 text-white px-8 py-3 rounded-lg font-semibold transition">
            Continuar Comprando
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-8" data-testid="carrinho-title">Meu Carrinho</h1>

        <div className="grid lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 space-y-4">
            {cart.map((item) => {
              const price = item.preco_promocional || item.preco;
              return (
                <div key={item.id} className="bg-white p-6 rounded-lg shadow-md flex items-center gap-4" data-testid={`cart-item-${item.id}`}>
                  <img src={item.imagem_url} alt={item.nome} className="w-24 h-24 object-cover rounded" />
                  <div className="flex-1">
                    <h3 className="font-semibold text-lg">{item.nome}</h3>
                    <p className="text-amber-600 font-bold">{formatPrice(price)}</p>
                  </div>
                  <div className="flex items-center space-x-2">
                    <button onClick={() => updateQuantity(item.id, item.quantity - 1)} className="p-2 hover:bg-gray-100 rounded">
                      <Minus size={16} />
                    </button>
                    <span className="w-12 text-center font-semibold">{item.quantity}</span>
                    <button onClick={() => updateQuantity(item.id, item.quantity + 1)} className="p-2 hover:bg-gray-100 rounded">
                      <Plus size={16} />
                    </button>
                  </div>
                  <button onClick={() => removeFromCart(item.id)} className="text-red-500 hover:bg-red-50 p-2 rounded">
                    <Trash2 size={20} />
                  </button>
                </div>
              );
            })}
            <button onClick={clearCart} className="text-red-500 hover:underline">Limpar Carrinho</button>
          </div>

          <div className="bg-white p-6 rounded-lg shadow-md h-fit">
            <h2 className="text-xl font-bold mb-4">Resumo do Pedido</h2>
            <div className="mb-4">
              <label className="block text-sm font-semibold mb-2">Cupom de Desconto</label>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={cupom}
                  onChange={(e) => setCupom(e.target.value.toUpperCase())}
                  placeholder="Código do cupom"
                  className="flex-1 px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-amber-500"
                  data-testid="cupom-input"
                />
                <button onClick={validarCupom} className="bg-slate-900 hover:bg-slate-800 text-white px-4 py-2 rounded" data-testid="aplicar-cupom-btn">
                  Aplicar
                </button>
              </div>
              {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
              {cupomAplicado && <p className="text-green-600 text-sm mt-2">✓ Cupom {cupomAplicado.codigo} aplicado!</p>}
            </div>

            <div className="space-y-2 border-t pt-4">
              <div className="flex justify-between"><span>Subtotal:</span><span>{formatPrice(subtotal)}</span></div>
              {desconto > 0 && <div className="flex justify-between text-green-600"><span>Desconto:</span><span>-{formatPrice(desconto)}</span></div>}
              <div className="flex justify-between text-2xl font-bold border-t pt-2">
                <span>Total:</span>
                <span className="text-amber-600" data-testid="total-carrinho">{formatPrice(total)}</span>
              </div>
            </div>

            <button onClick={handleCheckout} className="w-full mt-6 bg-amber-500 hover:bg-amber-600 text-white py-3 rounded-lg font-semibold transition" data-testid="checkout-btn">
              Finalizar Compra
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
