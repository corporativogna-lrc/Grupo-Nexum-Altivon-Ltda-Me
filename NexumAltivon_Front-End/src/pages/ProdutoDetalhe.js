/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { produtoAPI } from '../services/api';
import { useCart } from '../context/CartContext';
import { formatPrice } from '../utils/formatters';
import { ArrowLeft, Check, Minus, Plus, RefreshCw, Shield, ShoppingBag, Star, Truck } from 'lucide-react';

export default function ProdutoDetalhe() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { addToCart } = useCart();
  const [produto, setProduto] = useState(null);
  const [sugeridos, setSugeridos] = useState([]);
  const [quantidade, setQuantidade] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadProduto = useCallback(async () => {
    try {
      const response = await produtoAPI.getById(id);
      const payload = response.data?.dados || response.data?.data || response.data;
      setProduto(payload);

      if (payload?.categoria_id) {
        try {
          const relatedResponse = await produtoAPI.getAll({ categoria_id: payload.categoria_id });
          const relatedPayload = relatedResponse.data?.dados || relatedResponse.data?.data || relatedResponse.data;
          const relatedList = Array.isArray(relatedPayload) ? relatedPayload : [];
          setSugeridos(
            relatedList
              .filter((item) => String(item.id) !== String(payload.id))
              .filter((item) => item.ativo !== false)
              .slice(0, 4),
          );
        } catch {
          setSugeridos([]);
        }
      } else {
        setSugeridos([]);
      }
    } catch (error) {
      if (process.env.NODE_ENV === 'development') console.error(error);
      setProduto(null);
      setSugeridos([]);
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
      <div className="flex min-h-screen items-center justify-center bg-[#050505]">
        <div className="h-12 w-12 animate-spin rounded-full border-b-2 border-[#C9A227]"></div>
      </div>
    );
  }

  if (!produto) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#050505] text-white">
        <div className="text-center">
          <p className="mb-4 text-[#B8B8B8]">Produto não encontrado</p>
          <Link to="/produtos" className="text-[#C9A227] hover:underline">Voltar para produtos</Link>
        </div>
      </div>
    );
  }

  const finalPrice = produto.preco_promocional || produto.preco;
  const hasDiscount = produto.preco_promocional && produto.preco_promocional < produto.preco;
  const image = produto.imagem_url || produto.imagem || '';
  const dimensions = [
    produto.peso ? `${produto.peso} kg` : null,
    produto.altura ? `${produto.altura} cm de altura` : null,
    produto.largura ? `${produto.largura} cm de largura` : null,
    produto.comprimento ? `${produto.comprimento} cm de comprimento` : null,
  ].filter(Boolean);

  return (
    <div className="min-h-screen bg-[#050505] py-8 text-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <Link to="/produtos" className="mb-6 inline-flex items-center gap-2 text-sm font-black text-[#A0A0A0] hover:text-[#C9A227]">
          <ArrowLeft size={18} /> Voltar para produtos
        </Link>

        <div className="overflow-hidden rounded-3xl border border-[#2A2A2A] bg-[#111111] shadow-[0_28px_100px_rgba(0,0,0,0.42)]">
          <div className="grid lg:grid-cols-2">
            <div className="bg-[#080808]">
              {image ? (
                <img src={image} alt={produto.nome} className="h-full min-h-[520px] w-full object-cover" data-testid="produto-imagem" />
              ) : (
                <div className="flex min-h-[520px] items-center justify-center bg-[radial-gradient(circle_at_top,_rgba(201,162,39,0.14),_transparent_52%),linear-gradient(180deg,#111,#080808)] text-center">
                  <div>
                    <p className="text-sm font-black uppercase tracking-[0.22em] text-[#C9A227]">Imagem não cadastrada</p>
                    <p className="mt-3 text-sm text-[#A0A0A0]">Este produto está no banco, mas ainda não possui foto vinculada.</p>
                  </div>
                </div>
              )}
            </div>

            <div className="p-6 sm:p-10">
              {produto.destaque && (
                <span className="inline-flex items-center gap-2 rounded-full bg-[#C9A227] px-3 py-1 text-sm font-black text-black">
                  <Star size={15} fill="currentColor" />
                  Destaque
                </span>
              )}

              <h1 className="mt-5 text-4xl font-black leading-tight text-white" data-testid="produto-nome">{produto.nome}</h1>
              <p className="mt-2 text-sm font-bold text-[#A0A0A0]">SKU: {produto.sku || 'NA-PREMIUM'}</p>

              <div className="my-7">
                {hasDiscount && (
                  <p className="text-lg font-semibold text-[#777] line-through">{formatPrice(produto.preco)}</p>
                )}
                <p className="text-5xl font-black text-[#C9A227]" data-testid="produto-preco">{formatPrice(finalPrice)}</p>
                {hasDiscount && (
                  <p className="mt-2 font-bold text-emerald-400">
                    Economize {formatPrice(produto.preco - produto.preco_promocional)}!
                  </p>
                )}
                <p className="mt-2 text-sm font-semibold text-[#A0A0A0]">Em até 10x sem juros ou com condição especial no PIX.</p>
              </div>

              <p className="mb-7 max-w-2xl leading-7 text-[#C8C8C8]">{produto.descricao}</p>

              <div className="mb-7 grid gap-3 rounded-2xl border border-[#2A2A2A] bg-[#080808] p-4 sm:grid-cols-2">
                <div>
                  <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">SKU</p>
                  <p className="mt-1 font-bold text-white">{produto.sku || 'NA'}</p>
                </div>
                <div>
                  <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">Estoque</p>
                  <p className="mt-1 font-bold text-white">{produto.estoque} unidades</p>
                </div>
                <div>
                  <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">Categoria</p>
                  <p className="mt-1 font-bold text-white">{produto.categoria?.nome || produto.categoria_nome || 'Sem categoria'}</p>
                </div>
                <div>
                  <p className="text-xs font-black uppercase tracking-[0.18em] text-[#777]">Dimensões</p>
                  <p className="mt-1 font-bold text-white">{dimensions.length > 0 ? dimensions.join(' · ') : 'Não informadas'}</p>
                </div>
              </div>

              <div className="mb-7 flex flex-col gap-4 sm:flex-row sm:items-center">
                <div>
                  <label className="mb-2 block text-sm font-black text-[#D8D8D8]">Quantidade</label>
                  <div className="inline-flex h-12 items-center overflow-hidden rounded-full border border-[#2A2A2A] bg-[#080808]">
                    <button onClick={() => setQuantidade(Math.max(1, quantidade - 1))} className="inline-flex h-12 w-12 items-center justify-center hover:bg-[#1A1A1A]" aria-label="Diminuir quantidade">
                      <Minus size={16} />
                    </button>
                    <span className="w-12 text-center font-black">{quantidade}</span>
                    <button onClick={() => setQuantidade(quantidade + 1)} className="inline-flex h-12 w-12 items-center justify-center hover:bg-[#1A1A1A]" aria-label="Aumentar quantidade">
                      <Plus size={16} />
                    </button>
                  </div>
                </div>
                <p className="text-sm font-bold text-[#A0A0A0]">{produto.estoque} unidades disponíveis</p>
              </div>

              <button onClick={handleAddToCart} disabled={produto.estoque === 0}
                className="flex w-full items-center justify-center gap-2 rounded-full bg-[#C9A227] py-4 text-base font-black text-black shadow-sm transition hover:bg-[#FFD95A] disabled:cursor-not-allowed disabled:bg-[#2A2A2A] disabled:text-[#777]"
                data-testid="add-to-cart-detail">
                <ShoppingBag size={22} />
                <span>{produto.estoque === 0 ? 'Fora de Estoque' : 'Adicionar ao Carrinho'}</span>
              </button>

              <div className="mt-8 grid gap-3 border-t border-[#2A2A2A] pt-8 sm:grid-cols-3">
                <div className="rounded-2xl border border-[#2A2A2A] bg-[#080808] p-4">
                  <Truck className="mb-3 text-[#C9A227]" size={22} />
                  <p className="text-sm font-black text-white">Frete seguro</p>
                  <p className="mt-1 text-xs text-[#A0A0A0]">Rastreio completo</p>
                </div>
                <div className="rounded-2xl border border-[#2A2A2A] bg-[#080808] p-4">
                  <Shield className="mb-3 text-[#C9A227]" size={22} />
                  <p className="text-sm font-black text-white">Garantia</p>
                  <p className="mt-1 text-xs text-[#A0A0A0]">2 anos oficiais</p>
                </div>
                <div className="rounded-2xl border border-[#2A2A2A] bg-[#080808] p-4">
                  <RefreshCw className="mb-3 text-[#C9A227]" size={22} />
                  <p className="text-sm font-black text-white">Troca fácil</p>
                  <p className="mt-1 text-xs text-[#A0A0A0]">30 dias</p>
                </div>
              </div>

              <div className="mt-6 flex flex-wrap gap-3 text-sm font-bold">
                {['Certificado incluso', 'Embalagem premium', 'Compra protegida'].map((item) => (
                  <span key={item} className="inline-flex items-center gap-2 rounded-full border border-emerald-500/20 bg-emerald-500/10 px-3 py-2 text-emerald-300">
                    <Check size={15} />
                    {item}
                  </span>
                ))}
              </div>

              {sugeridos.length > 0 && (
                <div className="mt-10 border-t border-[#2A2A2A] pt-8">
                  <h2 className="text-xl font-black text-white">Sugestões semelhantes</h2>
                  <div className="mt-4 grid gap-4 sm:grid-cols-2">
                    {sugeridos.map((item) => (
                      <Link
                        key={item.id}
                        to={`/produto/${item.id}`}
                        className="group rounded-2xl border border-[#2A2A2A] bg-[#080808] p-4 transition hover:border-[#C9A227]"
                      >
                        <div className="aspect-[4/3] overflow-hidden rounded-xl bg-[#111111]">
                          <img
                            src={item.imagem_url || item.imagem || ''}
                            alt={item.nome}
                            className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
                          />
                        </div>
                        <p className="mt-3 text-sm font-black text-white">{item.nome}</p>
                        <p className="mt-1 text-sm text-[#C9C9C9]">{formatPrice(item.preco_promocional || item.preco || 0)}</p>
                      </Link>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
