/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

import { useEffect, useMemo, useState } from 'react';
import { ArrowRight, Building2, ChevronLeft, ChevronRight, LoaderCircle, ShoppingBag, ShoppingCart, Store, Truck } from 'lucide-react';
import { Link } from 'react-router-dom';
import { resolvePublicAssetUrl, siteAPI } from '../services/api';
import { buildProfileThemeStyle } from '../utils/siteTheme';

const filters = [
  { value: 'Todos', label: 'Todos' },
  { value: 'ParceiroVenda', label: 'Vendas' },
  { value: 'ParceiroCompra', label: 'Compras' },
  { value: 'Fornecedor', label: 'Fornecedores' },
  { value: 'Marketplace', label: 'Marketplaces' },
];

const iconMap = { Building2, ShoppingBag, ShoppingCart, Store, Truck };
const pageSize = 4;

export default function Parceiros() {
  const [partners, setPartners] = useState([]);
  const [filter, setFilter] = useState('Todos');
  const [page, setPage] = useState(0);
  const [rotationSeconds, setRotationSeconds] = useState(8);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [failedImages, setFailedImages] = useState(() => new Set());

  useEffect(() => {
    let active = true;
    Promise.all([siteAPI.listarParceirosPublicos(), siteAPI.getPublicConfig()])
      .then(([partnersResponse, configResponse]) => {
        if (!Array.isArray(partnersResponse.data)) throw new Error('A API retornou parceiros em formato inválido.');
        if (active) {
          setPartners(partnersResponse.data);
          setRotationSeconds(Math.min(60, Math.max(5, Number(configResponse.data?.partnerRotationSeconds) || 8)));
        }
      })
      .catch((loadError) => {
        if (active) setError(loadError?.response?.data?.detail || loadError?.response?.data?.mensagem || loadError.message || 'Não foi possível carregar os parceiros.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  const filteredPartners = useMemo(
    () => filter === 'Todos' ? partners : partners.filter((partner) => partner.tipo === filter),
    [filter, partners],
  );
  const pageCount = Math.max(1, Math.ceil(filteredPartners.length / pageSize));
  const visiblePartners = useMemo(
    () => filteredPartners.slice(page * pageSize, page * pageSize + pageSize),
    [filteredPartners, page],
  );

  useEffect(() => { setPage(0); }, [filter]);
  useEffect(() => {
    if (page >= pageCount) setPage(0);
  }, [page, pageCount]);
  useEffect(() => {
    if (pageCount < 2) return undefined;
    const timer = window.setInterval(() => setPage((current) => (current + 1) % pageCount), rotationSeconds * 1000);
    return () => window.clearInterval(timer);
  }, [pageCount, rotationSeconds]);

  return (
    <main className="min-h-screen bg-[#050505] text-white">
      <section className="border-b border-white/10 px-4 py-14 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-7xl">
          <p className="text-sm font-black uppercase tracking-[0.24em] text-[#E8D5A3]">Rede comercial</p>
          <h1 className="mt-4 font-serif text-5xl font-bold text-[#C9A227]">Parceiros</h1>
          <p className="mt-5 max-w-3xl text-lg leading-8 text-zinc-300">Fornecedores, parceiros de compra e venda e marketplaces oficialmente publicados pelo Grupo Nexum Altivon.</p>
          <div className="mt-8 flex flex-wrap gap-2" role="tablist" aria-label="Filtrar parceiros">
            {filters.map((item) => (
              <button key={item.value} type="button" onClick={() => setFilter(item.value)} className={`rounded-full px-4 py-2 text-sm font-black transition ${filter === item.value ? 'bg-[#C9A227] text-black' : 'border border-white/15 text-zinc-300 hover:border-[#C9A227]'}`}>{item.label}</button>
            ))}
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6 lg:px-8">
        {loading && <div className="flex min-h-72 items-center justify-center"><LoaderCircle className="animate-spin text-[#C9A227]" size={34} /></div>}
        {!loading && error && <div className="rounded-lg border border-red-400/30 bg-red-500/10 p-5 text-sm font-bold text-red-100" role="alert">{error}</div>}
        {!loading && !error && filteredPartners.length === 0 && <div className="rounded-lg border border-amber-400/30 bg-amber-500/10 p-5 text-sm font-bold text-amber-100">Nenhum perfil desta categoria está publicado no momento.</div>}
        {!loading && !error && filteredPartners.length > 0 && (
          <>
            <div className="grid gap-7 md:grid-cols-2" data-testid="partner-rotation-grid">
              {visiblePartners.map((partner) => {
                const Icon = iconMap[partner.icon] || Building2;
                const hasImage = Boolean(partner.image) && !failedImages.has(partner.id);
                return (
                  <article key={partner.id} style={buildProfileThemeStyle(partner)} className="site-profile-theme overflow-hidden rounded-lg border border-white/10 shadow-2xl shadow-black/25 transition hover:-translate-y-1">
                    <Link to={`/parceiros/${partner.slug}`}>
                      <div className="profile-surface relative aspect-[16/9] overflow-hidden">
                        {hasImage && <img src={resolvePublicAssetUrl(partner.image)} alt={partner.title} className="h-full w-full object-cover" onError={() => setFailedImages((current) => new Set(current).add(partner.id))} />}
                        {!hasImage && <div className="flex h-full items-center justify-center"><Icon className="profile-primary" size={54} /></div>}
                        <span className="profile-primary absolute left-4 top-4 flex h-11 w-11 items-center justify-center rounded-full bg-black/80"><Icon size={22} /></span>
                      </div>
                      <div className="p-6">
                        <p className="text-xs font-black uppercase tracking-[0.16em] opacity-70">{partner.segment}</p>
                        <h2 className="mt-3 text-2xl font-black">{partner.title}</h2>
                        <p className="mt-3 text-sm font-semibold opacity-90">{partner.activity}</p>
                        <p className="mt-3 line-clamp-3 text-sm leading-7 opacity-70">{partner.text}</p>
                        <span className="profile-primary mt-6 inline-flex items-center gap-2 text-sm font-black">Ver perfil <ArrowRight size={16} /></span>
                      </div>
                    </Link>
                  </article>
                );
              })}
            </div>
            {pageCount > 1 && (
              <div className="mt-8 flex items-center justify-center gap-4" aria-label="Navegação do rodízio de parceiros">
                <button type="button" onClick={() => setPage((current) => (current - 1 + pageCount) % pageCount)} className="flex h-10 w-10 items-center justify-center rounded-full border border-white/20" title="Parceiros anteriores"><ChevronLeft size={18} /></button>
                <span className="text-sm font-black">{page + 1} de {pageCount}</span>
                <button type="button" onClick={() => setPage((current) => (current + 1) % pageCount)} className="flex h-10 w-10 items-center justify-center rounded-full border border-white/20" title="Próximos parceiros"><ChevronRight size={18} /></button>
              </div>
            )}
          </>
        )}
      </section>
    </main>
  );
}
