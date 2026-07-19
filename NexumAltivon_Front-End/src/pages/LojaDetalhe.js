/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

import { useEffect, useState } from 'react';
import { ArrowLeft, ArrowRight, LoaderCircle } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import { resolvePublicAssetUrl, siteAPI } from '../services/api';
import { buildProfileThemeStyle } from '../utils/siteTheme';

export default function LojaDetalhe() {
  const { slug } = useParams();
  const [store, setStore] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [imageFailed, setImageFailed] = useState(false);

  useEffect(() => {
    let active = true;
    setLoading(true);
    siteAPI.obterLojaPublica(slug)
      .then((response) => {
        if (!response.data?.id) throw new Error('A API não confirmou o perfil da loja.');
        if (active) setStore(response.data);
      })
      .catch((loadError) => {
        if (active) setError(loadError?.response?.data?.mensagem || loadError?.response?.data?.detail || loadError.message || 'Loja não encontrada.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [slug]);

  if (loading) return <main className="flex min-h-screen items-center justify-center bg-[#050505] text-[#C9A227]"><LoaderCircle className="animate-spin" size={36} /></main>;
  if (error || !store) return <main className="min-h-screen bg-[#050505] px-4 py-20 text-white"><div className="mx-auto max-w-3xl rounded-lg border border-red-400/30 bg-red-500/10 p-6 text-red-100">{error || 'Loja não encontrada.'}</div></main>;

  const actionUrl = store.ctaUrl || `/produtos?loja=${encodeURIComponent(store.lojaId || '')}`;
  return (
    <main style={buildProfileThemeStyle(store)} className="site-profile-theme min-h-screen">
      <section className="profile-surface relative flex min-h-[68vh] items-end overflow-hidden border-b border-white/10">
        {store.imagem && !imageFailed && <img src={resolvePublicAssetUrl(store.imagem)} alt={store.nome} className="absolute inset-0 h-full w-full object-cover" onError={() => setImageFailed(true)} />}
        {store.imagem && imageFailed && <div className="absolute inset-0 flex items-center justify-center px-6 text-center font-bold text-red-200">O banner configurado para esta loja não respondeu.</div>}
        <div className="absolute inset-0 bg-gradient-to-t from-black via-black/65 to-black/10" />
        <div className="relative mx-auto w-full max-w-7xl px-4 pb-12 sm:px-6 lg:px-8">
          <Link to="/lojas" className="inline-flex items-center gap-2 text-sm font-black text-zinc-200 transition hover:text-[#C9A227]"><ArrowLeft size={17} /> Todas as lojas</Link>
          {store.logo && <img src={resolvePublicAssetUrl(store.logo)} alt={`Logomarca ${store.nome}`} className="mt-8 h-28 max-w-xs rounded-lg border border-white/20 bg-black/60 object-contain p-3" />}
          <p className="mt-7 text-sm font-black uppercase tracking-[0.22em] text-[#E8D5A3]">{store.segmento}</p>
          <h1 className="profile-primary mt-3 font-serif text-5xl font-bold sm:text-6xl">{store.nome}</h1>
          <p className="mt-5 max-w-3xl text-xl font-semibold leading-8 text-white">{store.atividade}</p>
        </div>
      </section>
      <section className="mx-auto grid max-w-7xl gap-10 px-4 py-14 sm:px-6 lg:grid-cols-[minmax(0,1fr)_280px] lg:px-8">
        <div><h2 className="profile-primary font-serif text-3xl font-bold">Sobre a loja</h2><p className="mt-6 whitespace-pre-line text-base leading-8 opacity-80">{store.descricao}</p></div>
        <div className="border-l border-white/10 pl-6">
          <p className="text-xs font-black uppercase tracking-[0.16em] text-zinc-500">Atendimento comercial</p>
          {actionUrl.startsWith('/') ? (
            <Link to={actionUrl} className="profile-primary-bg mt-5 inline-flex w-full items-center justify-center gap-2 rounded-lg px-5 py-3 text-sm font-black text-black">{store.ctaTexto || 'Ver produtos'} <ArrowRight size={16} /></Link>
          ) : (
            <a href={actionUrl} target="_blank" rel="noreferrer" className="profile-primary-bg mt-5 inline-flex w-full items-center justify-center gap-2 rounded-lg px-5 py-3 text-sm font-black text-black">{store.ctaTexto || 'Entrar em contato'} <ArrowRight size={16} /></a>
          )}
        </div>
      </section>
    </main>
  );
}
