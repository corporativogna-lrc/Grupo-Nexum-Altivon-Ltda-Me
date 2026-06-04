import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { produtoAPI } from '../services/api';
import ProductCard from '../components/ProductCard';
import { fallbackCategories, fallbackProducts } from '../data/mockStore';
import { ArrowRight, BadgeCheck, Gem, Headphones, ShieldCheck, Sparkles, Truck } from 'lucide-react';

const highlights = [
  { icon: ShieldCheck, title: 'Garantia oficial', text: '2 anos com assistência nacional' },
  { icon: Truck, title: 'Entrega premium', text: 'Envio segurado para todo o Brasil' },
  { icon: BadgeCheck, title: 'Curadoria real', text: 'Peças revisadas e certificadas' },
  { icon: Headphones, title: 'Concierge', text: 'Atendimento consultivo no WhatsApp' },
];

export default function Home() {
  const [produtosDestaque, setProdutosDestaque] = useState(fallbackProducts.slice(0, 3));
  const [loading, setLoading] = useState(true);

  const loadProdutosDestaque = useCallback(async () => {
    try {
      const response = await produtoAPI.getDestaques();
      if (Array.isArray(response.data) && response.data.length > 0) {
        setProdutosDestaque(response.data.slice(0, 6));
      }
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Erro ao carregar produtos:', error);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadProdutosDestaque();
  }, [loadProdutosDestaque]);

  return (
    <main className="bg-[#f5f7fb]">
      <section className="relative min-h-[620px] overflow-hidden bg-slate-950 text-white">
        <img
          src="https://images.unsplash.com/photo-1509048191080-d2984bad6ae5?auto=format&fit=crop&w=1800&q=88"
          alt="Coleção Nexum Altivon"
          className="absolute inset-0 h-full w-full object-cover opacity-55"
        />
        <div className="absolute inset-0 bg-slate-950/55" />
        <div className="relative mx-auto flex min-h-[620px] max-w-7xl flex-col justify-center px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-3xl">
            <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-2 text-sm font-bold text-amber-200 backdrop-blur">
              <Sparkles size={16} />
              Coleção executiva 2026
            </div>
            <h1 className="max-w-3xl text-5xl font-black leading-[1.02] tracking-normal sm:text-6xl lg:text-7xl" data-testid="hero-title">
              Nexum Altivon Store
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-100 sm:text-xl">
              Relógios premium, acessórios de alto padrão e uma jornada de compra pensada para quem escolhe por precisão, presença e confiança.
            </p>
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <Link
                to="/produtos"
                className="inline-flex items-center justify-center gap-2 rounded-full bg-amber-400 px-7 py-4 text-sm font-black uppercase tracking-wide text-slate-950 shadow-lg transition hover:bg-amber-300"
                data-testid="hero-cta"
              >
                Comprar agora
                <ArrowRight size={18} />
              </Link>
              <Link
                to="/lojas"
                className="inline-flex items-center justify-center rounded-full border border-white/40 px-7 py-4 text-sm font-black uppercase tracking-wide text-white transition hover:bg-white hover:text-slate-950"
              >
                Agendar atendimento
              </Link>
            </div>
          </div>
        </div>
      </section>

      <section className="-mt-14 relative z-10">
        <div className="mx-auto grid max-w-7xl grid-cols-1 gap-3 px-4 sm:grid-cols-2 sm:px-6 lg:grid-cols-4 lg:px-8">
          {highlights.map((item) => {
            const Icon = item.icon;
            return (
              <div key={item.title} className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
                <Icon className="mb-4 text-slate-950" size={24} />
                <h3 className="font-black text-slate-950">{item.title}</h3>
                <p className="mt-1 text-sm leading-5 text-slate-500">{item.text}</p>
              </div>
            );
          })}
        </div>
      </section>

      <section className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col justify-between gap-6 md:flex-row md:items-end">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.22em] text-amber-700">Compre por estilo</p>
              <h2 className="mt-3 text-3xl font-black text-slate-950 sm:text-4xl">Categorias em destaque</h2>
            </div>
            <Link to="/produtos" className="inline-flex items-center gap-2 text-sm font-black text-slate-950 hover:text-amber-700">
              Ver catálogo completo
              <ArrowRight size={17} />
            </Link>
          </div>

          <div className="mt-8 grid grid-cols-1 gap-4 md:grid-cols-4">
            {fallbackCategories.map((category, index) => (
              <Link
                key={category.id}
                to={`/produtos?categoria=${category.id}`}
                className={`group rounded-lg border border-slate-200 bg-white p-6 shadow-sm transition hover:-translate-y-1 hover:shadow-xl ${index === 0 ? 'md:col-span-2' : ''}`}
              >
                <Gem className="mb-10 text-amber-600" size={28} />
                <h3 className="text-2xl font-black text-slate-950">{category.nome}</h3>
                <p className="mt-2 max-w-sm text-sm leading-6 text-slate-500">{category.descricao}</p>
                <span className="mt-6 inline-flex items-center gap-2 text-sm font-black text-slate-950">
                  Explorar
                  <ArrowRight className="transition group-hover:translate-x-1" size={16} />
                </span>
              </Link>
            ))}
          </div>
        </div>
      </section>

      <section className="bg-white py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col justify-between gap-6 md:flex-row md:items-end">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.22em] text-amber-700">Seleção Nexum</p>
              <h2 className="mt-3 text-3xl font-black text-slate-950 sm:text-4xl" data-testid="featured-title">
                Produtos em alta
              </h2>
              <p className="mt-3 max-w-2xl text-slate-500">Os modelos com maior procura na semana, prontos para entrega e com parcelamento premium.</p>
            </div>
            <Link
              to="/produtos"
              className="inline-flex items-center justify-center gap-2 rounded-full bg-slate-950 px-6 py-3 text-sm font-black text-white transition hover:bg-slate-800"
            >
              Ver todos
              <ArrowRight size={17} />
            </Link>
          </div>

          {loading ? (
            <div className="mt-10 grid grid-cols-1 gap-6 md:grid-cols-3">
              {[1, 2, 3].map((item) => (
                <div key={item} className="h-[520px] animate-pulse rounded-lg bg-slate-100" />
              ))}
            </div>
          ) : (
            <div className="mt-10 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3" data-testid="products-grid">
              {produtosDestaque.map((produto) => (
                <ProductCard key={produto.id} product={produto} />
              ))}
            </div>
          )}
        </div>
      </section>
    </main>
  );
}
