/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

import { useEffect, useState } from 'react';
import { ArrowLeft, ArrowRight, ExternalLink, LoaderCircle, Mail, MapPin, Phone } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import { resolvePublicAssetUrl, siteAPI } from '../services/api';
import { formatPrice } from '../utils/formatters';
import { buildProfileThemeStyle } from '../utils/siteTheme';

export default function ParceiroDetalhe() {
  const { slug } = useParams();
  const [partner, setPartner] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [imageFailed, setImageFailed] = useState(false);

  useEffect(() => {
    let active = true;
    siteAPI.obterParceiroPublico(slug)
      .then((response) => {
        if (!response.data?.id || !Array.isArray(response.data?.produtos)) throw new Error('A API não confirmou o perfil e o catálogo do parceiro.');
        if (active) setPartner(response.data);
      })
      .catch((loadError) => {
        if (active) setError(loadError?.response?.data?.mensagem || loadError?.response?.data?.detail || loadError.message || 'Parceiro não encontrado.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [slug]);

  if (loading) return <main className="flex min-h-screen items-center justify-center bg-[#050505] text-[#C9A227]"><LoaderCircle className="animate-spin" size={36} /></main>;
  if (error || !partner) return <main className="min-h-screen bg-[#050505] px-4 py-20 text-white"><div className="mx-auto max-w-3xl rounded-lg border border-red-400/30 bg-red-500/10 p-6 text-red-100">{error || 'Parceiro não encontrado.'}</div></main>;

  const isInternalAction = partner.href?.startsWith('/');
  const phoneHref = partner.telefonePublico ? `tel:${partner.telefonePublico.replace(/[^+\d]/g, '')}` : '';
  return (
    <main style={buildProfileThemeStyle(partner)} className="site-profile-theme min-h-screen">
      <section className="profile-surface relative flex min-h-[68vh] items-end overflow-hidden border-b border-white/10">
        {partner.image && !imageFailed && <img src={resolvePublicAssetUrl(partner.image)} alt={partner.title} className="absolute inset-0 h-full w-full object-cover" onError={() => setImageFailed(true)} />}
        {partner.image && imageFailed && <div className="absolute inset-0 flex items-center justify-center p-6 text-center font-bold text-red-200">O banner configurado para este perfil não respondeu.</div>}
        <div className="absolute inset-0 bg-gradient-to-t from-black via-black/65 to-black/10" />
        <div className="relative mx-auto w-full max-w-7xl px-4 pb-12 text-white sm:px-6 lg:px-8">
          <Link to="/parceiros" className="inline-flex items-center gap-2 text-sm font-black transition hover:opacity-80"><ArrowLeft size={17} /> Todos os parceiros</Link>
          {partner.logo && <img src={resolvePublicAssetUrl(partner.logo)} alt={`Logomarca ${partner.title}`} className="mt-8 h-28 max-w-xs rounded-lg border border-white/20 bg-black/60 object-contain p-3" />}
          <p className="mt-7 text-sm font-black uppercase tracking-[0.22em] opacity-80">{partner.segment} · {partner.tipo}</p>
          <h1 className="profile-primary mt-3 font-serif text-5xl font-bold sm:text-6xl">{partner.title}</h1>
          <p className="mt-5 max-w-3xl text-xl font-semibold leading-8">{partner.activity}</p>
        </div>
      </section>

      <section className="mx-auto grid max-w-7xl gap-10 px-4 py-14 sm:px-6 lg:grid-cols-[minmax(0,1fr)_320px] lg:px-8">
        <div>
          <h2 className="profile-primary font-serif text-3xl font-bold">Perfil comercial</h2>
          <p className="mt-6 whitespace-pre-line text-base leading-8 opacity-80">{partner.text}</p>
        </div>
        <aside className="border-l border-current/15 pl-6">
          <p className="text-xs font-black uppercase tracking-[0.16em] opacity-60">Dados autorizados para divulgação</p>
          <div className="mt-5 space-y-3 text-sm font-semibold">
            {partner.siteUrl && <a href={partner.siteUrl} target="_blank" rel="noreferrer" className="profile-primary flex items-center gap-2"><ExternalLink size={16} /> Site oficial</a>}
            {partner.emailPublico && <a href={`mailto:${partner.emailPublico}`} className="profile-primary flex items-center gap-2"><Mail size={16} /> {partner.emailPublico}</a>}
            {partner.telefonePublico && <a href={phoneHref} className="profile-primary flex items-center gap-2"><Phone size={16} /> {partner.telefonePublico}</a>}
            {partner.enderecoPublico && <p className="flex items-start gap-2"><MapPin className="profile-primary mt-0.5 shrink-0" size={16} /> {partner.enderecoPublico}</p>}
          </div>
          {partner.href && (isInternalAction ? (
            <Link to={partner.href} className="profile-primary-bg mt-6 inline-flex w-full items-center justify-center gap-2 rounded-lg px-5 py-3 text-sm font-black text-black">{partner.cta} <ArrowRight size={16} /></Link>
          ) : (
            <a href={partner.href} target="_blank" rel="noreferrer" className="profile-primary-bg mt-6 inline-flex w-full items-center justify-center gap-2 rounded-lg px-5 py-3 text-sm font-black text-black">{partner.cta} <ArrowRight size={16} /></a>
          ))}
        </aside>
      </section>

      <section className="border-t border-current/10 px-4 py-14 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-7xl">
          <h2 className="profile-primary font-serif text-3xl font-bold">Produtos oferecidos nesta parceria</h2>
          {partner.produtos.length === 0 ? (
            <p className="mt-6 rounded-lg border border-current/15 p-5 text-sm font-semibold opacity-70">Este perfil não possui produtos autorizados para divulgação no momento.</p>
          ) : (
            <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {partner.produtos.map((product) => {
                const finalPrice = product.precoPromocional && product.precoPromocional < product.preco ? product.precoPromocional : product.preco;
                return (
                  <article key={product.id} className="profile-surface overflow-hidden rounded-lg border border-current/10">
                    <Link to={`/produto/${product.id}`}>
                      <img src={resolvePublicAssetUrl(product.imagemUrl)} alt={product.nome} className="aspect-square w-full object-cover" loading="lazy" />
                      <div className="p-4">
                        <p className="text-xs font-bold opacity-60">{product.sku}</p>
                        <h3 className="mt-2 line-clamp-2 min-h-12 font-black">{product.nome}</h3>
                        <p className="profile-primary mt-4 text-xl font-black">{formatPrice(finalPrice)}</p>
                      </div>
                    </Link>
                  </article>
                );
              })}
            </div>
          )}
        </div>
      </section>
    </main>
  );
}
