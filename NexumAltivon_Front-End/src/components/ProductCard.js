import { Link } from 'react-router-dom';
import { Heart, ShoppingBag, Star } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { formatPrice } from '../utils/formatters';

export default function ProductCard({ product }) {
  const { addToCart } = useCart();
  const finalPrice = product.preco_promocional || product.preco;
  const hasDiscount = product.preco_promocional && product.preco_promocional < product.preco;
  const discount = hasDiscount ? Math.round(((product.preco - product.preco_promocional) / product.preco) * 100) : 0;
  const image = product.imagem_url || product.imagem || 'https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85';
  const isOut = Number(product.estoque) === 0;

  return (
    <article
      className="group overflow-hidden rounded-2xl border border-[#2A2A2A] bg-[#111111] shadow-[0_24px_80px_rgba(0,0,0,0.35)] transition duration-300 hover:-translate-y-1 hover:border-[#C9A227]/70 hover:shadow-[0_28px_90px_rgba(201,162,39,0.16)]"
      data-testid={`product-card-${product.id}`}
    >
      <div className="relative aspect-[4/5] overflow-hidden bg-[#080808]">
        <Link to={`/produto/${product.id}`} aria-label={product.nome}>
          <img
            src={image}
            alt={product.nome}
            className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
            loading="lazy"
          />
        </Link>
        <div className="absolute left-3 top-3 flex flex-wrap gap-2">
          {product.destaque && (
            <span className="rounded-full bg-[#C9A227] px-3 py-1 text-xs font-black uppercase tracking-wide text-black">
              Destaque
            </span>
          )}
          {hasDiscount && (
            <span className="rounded-full bg-rose-600 px-3 py-1 text-xs font-bold text-white">
              -{discount}%
            </span>
          )}
        </div>
        <button
          className="absolute right-3 top-3 inline-flex h-10 w-10 items-center justify-center rounded-full border border-white/10 bg-black/70 text-white shadow-sm backdrop-blur transition hover:border-[#C9A227] hover:text-[#C9A227]"
          aria-label="Favoritar produto"
          title="Favoritar"
        >
          <Heart size={18} />
        </button>
      </div>

      <div className="space-y-4 p-4">
        <div className="flex items-center justify-between gap-3 text-xs font-semibold text-[#A0A0A0]">
          <span>{product.sku || 'NA Premium'}</span>
          <span className="inline-flex items-center gap-1 text-[#C9A227]">
            <Star size={14} fill="currentColor" />
            {product.avaliacao || '4.8'}
          </span>
        </div>

        <div>
          <Link to={`/produto/${product.id}`}>
            <h3 className="line-clamp-2 min-h-[3.25rem] text-lg font-black leading-snug text-white transition hover:text-[#C9A227]" data-testid="product-name">
              {product.nome}
            </h3>
          </Link>
          <p className="mt-2 line-clamp-2 min-h-[2.5rem] text-sm leading-5 text-[#B8B8B8]">{product.descricao}</p>
        </div>

        <div className="flex items-end justify-between gap-4">
          <div>
            {hasDiscount && <p className="text-sm font-semibold text-[#777] line-through">{formatPrice(product.preco)}</p>}
            <p className="text-2xl font-black text-[#C9A227]" data-testid="product-price">{formatPrice(finalPrice)}</p>
            <p className="text-xs font-semibold text-emerald-400">ou 10x sem juros</p>
          </div>
          <button
            onClick={() => addToCart(product)}
            disabled={isOut}
            className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-[#C9A227] text-black shadow-sm transition hover:bg-[#FFD95A] disabled:cursor-not-allowed disabled:bg-[#2A2A2A] disabled:text-[#777]"
            data-testid="add-to-cart-btn"
            aria-label="Adicionar ao carrinho"
            title="Adicionar ao carrinho"
          >
            <ShoppingBag size={19} />
          </button>
        </div>

        <div className="h-5 text-xs font-bold">
          {isOut ? (
            <span className="text-rose-400">Fora de estoque</span>
          ) : product.estoque < 10 ? (
            <span className="text-orange-300">Ultimas {product.estoque} unidades</span>
          ) : (
            <span className="text-[#A0A0A0]">Pronta entrega</span>
          )}
        </div>
      </div>
    </article>
  );
}
