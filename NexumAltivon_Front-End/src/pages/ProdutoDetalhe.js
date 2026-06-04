import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { produtoAPI } from '../services/api';
import { useCart } from '../context/CartContext';
import { fallbackProducts } from '../data/mockStore';
import { formatPrice } from '../utils/formatters';
import { ArrowLeft, Check, Minus, Plus, RefreshCw, Shield, ShoppingBag, Star, Truck } from 'lucide-react';

export default function ProdutoDetalhe() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { addToCart } = useCart();
  const [produto, setProduto] = useState(null);
  const [quantidade, setQuantidade] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadProduto = useCallback(async () => {
    try {
      const response = await produtoAPI.getById(id);
      setProduto(response.data);
    } catch (error) {
      if (process.env.NODE_ENV === 'development') console.error(error);
      setProduto(fallbackProducts.find((item) => String(item.id) === String(id)) || null);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadProduto();
  }, [loadProduto]);

  const handleAddToCart = () => {
    addToCart(produto, quantidade);
    navigate('/carrinho');
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
      </div>
    );
  }

  if (!produto) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600 mb-4">Produto não encontrado</p>
          <Link to="/produtos" className="text-amber-600 hover:underline">Voltar para produtos</Link>
        </div>
      </div>
    );
  }

  const finalPrice = produto.preco_promocional || produto.preco;
  const hasDiscount = produto.preco_promocional && produto.preco_promocional < produto.preco;
  const image = produto.imagem_url || produto.imagem || fallbackProducts[0].imagem_url;

  return (
    <div className="min-h-screen bg-[#f5f7fb] py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <Link to="/produtos" className="mb-6 inline-flex items-center gap-2 text-sm font-black text-slate-700 hover:text-slate-950">
          <ArrowLeft size={18} /> Voltar para produtos
        </Link>

        <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="grid lg:grid-cols-2">
            <div className="bg-slate-100">
              <img src={image} alt={produto.nome} className="h-full min-h-[520px] w-full object-cover" data-testid="produto-imagem" />
            </div>

            <div className="p-6 sm:p-10">
              {produto.destaque && (
                <span className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-sm font-black text-amber-800">
                  <Star size={15} fill="currentColor" />
                  Destaque
                </span>
              )}

              <h1 className="mt-5 text-4xl font-black leading-tight text-slate-950" data-testid="produto-nome">{produto.nome}</h1>
              <p className="mt-2 text-sm font-bold text-slate-500">SKU: {produto.sku || 'NA-PREMIUM'}</p>

              <div className="my-7">
                {hasDiscount && (
                  <p className="text-lg font-semibold text-slate-400 line-through">{formatPrice(produto.preco)}</p>
                )}
                <p className="text-5xl font-black text-slate-950" data-testid="produto-preco">{formatPrice(finalPrice)}</p>
                {hasDiscount && (
                  <p className="mt-2 font-bold text-emerald-700">
                    Economize {formatPrice(produto.preco - produto.preco_promocional)}!
                  </p>
                )}
                <p className="mt-2 text-sm font-semibold text-slate-500">Em até 10x sem juros ou com condição especial no PIX.</p>
              </div>

              <p className="mb-7 max-w-2xl leading-7 text-slate-600">{produto.descricao}</p>

              <div className="mb-7 flex flex-col gap-4 sm:flex-row sm:items-center">
                <div>
                  <label className="mb-2 block text-sm font-black text-slate-700">Quantidade</label>
                  <div className="inline-flex h-12 items-center overflow-hidden rounded-full border border-slate-200 bg-white">
                    <button onClick={() => setQuantidade(Math.max(1, quantidade - 1))} className="inline-flex h-12 w-12 items-center justify-center hover:bg-slate-100" aria-label="Diminuir quantidade">
                      <Minus size={16} />
                    </button>
                    <span className="w-12 text-center font-black">{quantidade}</span>
                    <button onClick={() => setQuantidade(quantidade + 1)} className="inline-flex h-12 w-12 items-center justify-center hover:bg-slate-100" aria-label="Aumentar quantidade">
                      <Plus size={16} />
                    </button>
                  </div>
                </div>
                <p className="text-sm font-bold text-slate-500">{produto.estoque} unidades disponíveis</p>
              </div>

              <button onClick={handleAddToCart} disabled={produto.estoque === 0}
                className="flex w-full items-center justify-center gap-2 rounded-full bg-amber-400 py-4 text-base font-black text-slate-950 shadow-sm transition hover:bg-amber-300 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400"
                data-testid="add-to-cart-detail">
                <ShoppingBag size={22} />
                <span>{produto.estoque === 0 ? 'Fora de Estoque' : 'Adicionar ao Carrinho'}</span>
              </button>

              <div className="mt-8 grid gap-3 border-t border-slate-200 pt-8 sm:grid-cols-3">
                <div className="rounded-lg bg-slate-50 p-4">
                  <Truck className="mb-3 text-slate-950" size={22} />
                  <p className="text-sm font-black text-slate-950">Frete seguro</p>
                  <p className="mt-1 text-xs text-slate-500">Rastreio completo</p>
                </div>
                <div className="rounded-lg bg-slate-50 p-4">
                  <Shield className="mb-3 text-slate-950" size={22} />
                  <p className="text-sm font-black text-slate-950">Garantia</p>
                  <p className="mt-1 text-xs text-slate-500">2 anos oficiais</p>
                </div>
                <div className="rounded-lg bg-slate-50 p-4">
                  <RefreshCw className="mb-3 text-slate-950" size={22} />
                  <p className="text-sm font-black text-slate-950">Troca fácil</p>
                  <p className="mt-1 text-xs text-slate-500">30 dias</p>
                </div>
              </div>

              <div className="mt-6 flex flex-wrap gap-3 text-sm font-bold text-slate-600">
                {['Certificado incluso', 'Embalagem premium', 'Compra protegida'].map((item) => (
                  <span key={item} className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-3 py-2 text-emerald-800">
                    <Check size={15} />
                    {item}
                  </span>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
