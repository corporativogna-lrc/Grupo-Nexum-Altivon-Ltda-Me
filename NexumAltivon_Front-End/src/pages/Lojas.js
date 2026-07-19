/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

import { useEffect, useState } from 'react';
import { ArrowRight, Building2, Gift, Hammer, LoaderCircle, Plane, Shirt, Smartphone, Store, Watch } from 'lucide-react';
import { Link } from 'react-router-dom';
import { resolvePublicAssetUrl, siteAPI } from '../services/api';
import { buildProfileThemeStyle } from '../utils/siteTheme';

const storeIconMap = { Plane, Watch, Shirt, Smartphone, Hammer, Gift, Store };

const getErrorMessage = (error) =>
  error?.response?.data?.detail
  || error?.response?.data?.mensagem
  || error?.message
  || 'Não foi possível carregar as lojas publicadas.';

export default function Lojas() {
  const [stores, setStores] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [failedImages, setFailedImages] = useState(() => new Set());

  useEffect(() => {
    let active = true;
    siteAPI.listarLojasPublicas()
      .then((response) => {
        if (!Array.isArray(response.data)) throw new Error('A API retornou lojas em formato inválido.');
        if (active) {
          setStores(response.data);
          setError('');
        }
      })
      .catch((loadError) => {
        if (active) {
          setStores([]);
          setError(getErrorMessage(loadError));
        }
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => { active = false; };
  }, []);

  return (
    <main className="min-h-screen bg-[#050505] text-white">
      <section className="border-b border-white/10 px-4 py-14 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-7xl">
          <p className="text-sm font-black uppercase tracking-[0.24em] text-[#E8D5A3]">Grupo Nexum Altivon</p>
          <h1 className="mt-4 font-serif text-5xl font-bold text-[#C9A227]" data-testid="lojas-title">Nossas Lojas</h1>
          <p className="mt-5 max-w-2xl text-lg leading-8 text-zinc-300">Marcas do grupo com identidade, atividade e canais comerciais administrados individualmente.</p>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6 lg:px-8">
        {loading && <div className="flex min-h-72 items-center justify-center"><LoaderCircle className="animate-spin text-[#C9A227]" size={34} /></div>}
        {!loading && error && <div className="rounded-lg border border-red-400/30 bg-red-500/10 p-5 text-sm font-bold text-red-100" role="alert">{error}</div>}
        {!loading && !error && stores.length === 0 && (
          <div className="rounded-lg border border-amber-400/30 bg-amber-500/10 p-5 text-sm font-bold text-amber-100">Nenhuma loja está publicada no momento.</div>
        )}
        {!loading && !error && stores.length > 0 && (
          <div className="grid gap-7 md:grid-cols-2 lg:grid-cols-3" data-testid="lojas-grid">
            {stores.map((store) => {
              const Icon = storeIconMap[store.icon] || Building2;
              const imageUrl = store.imagem ? resolvePublicAssetUrl(store.imagem) : '';
              return (
                <article key={store.id} style={buildProfileThemeStyle(store)} className="site-profile-theme overflow-hidden rounded-lg border border-white/10 shadow-2xl shadow-black/25 transition hover:-translate-y-1">
                  <Link to={`/lojas/${store.slug}`} className="block">
                    <div className="profile-surface relative aspect-[16/9] overflow-hidden">
                      {imageUrl && !failedImages.has(store.id) && (
                        <img
                          src={imageUrl}
                          alt={store.nome}
                          className="h-full w-full object-cover transition duration-500 hover:scale-105"
                          onError={() => setFailedImages((current) => new Set(current).add(store.id))}
                        />
                      )}
                      {!imageUrl && <div className="flex h-full items-center justify-center"><Icon className="profile-primary" size={54} /></div>}
                      {failedImages.has(store.id) && <div className="flex h-full items-center justify-center px-6 text-center text-sm font-bold text-red-200">A imagem configurada para esta loja não respondeu.</div>}
                      <div className="absolute inset-x-0 bottom-0 h-24 bg-gradient-to-t from-[#111111] to-transparent" />
                      <div className="profile-primary absolute left-5 top-5 flex h-12 w-12 items-center justify-center rounded-full bg-black/80"><Icon size={24} /></div>
                    </div>
                    <div className="p-6">
                      <h2 className="profile-primary font-serif text-2xl font-bold">{store.nome}</h2>
                      <p className="mt-2 text-xs font-black uppercase tracking-[0.16em] text-[#E8D5A3]">{store.segmento}</p>
                      <p className="mt-4 text-sm font-semibold text-zinc-200">{store.atividade}</p>
                      <p className="mt-3 line-clamp-3 text-sm leading-7 text-zinc-400">{store.descricao}</p>
                      <span className="profile-primary mt-6 inline-flex items-center gap-2 text-sm font-black">Conhecer loja <ArrowRight size={16} /></span>
                    </div>
                  </Link>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
