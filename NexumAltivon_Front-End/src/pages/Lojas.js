/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import { Building2, Gift, Hammer, Mail, MapPin, Phone, Plane, Shirt, Smartphone, Store, Watch } from 'lucide-react';
import { lojaAPI, siteAPI, unwrapApiData } from '../services/api';

const defaultStoreCards = [
  {
    nome: 'Gran Tur',
    segmento: 'Viagens & Turismo',
    descricao: 'Mochilas, malas, acessórios de viagem e produtos para explorar o mundo com estilo e conforto.',
    imagem: '/imagens/homepage/loja-gran-tur.svg',
    icon: 'Plane',
  },
  {
    nome: 'Chronos',
    segmento: 'Relógios & Acessórios',
    descricao: 'Relógios e acessórios para quem valoriza precisão, presença e elegância.',
    imagem: '/imagens/homepage/loja-chronos.svg',
    icon: 'Watch',
  },
  {
    nome: 'Moda Mim',
    segmento: 'Moda & Vestuário',
    descricao: 'Roupas, calçados e acessórios para uma experiência de compra prática e atual.',
    imagem: '/imagens/homepage/loja-moda-mim.svg',
    icon: 'Shirt',
  },
  {
    nome: 'Geração Top+',
    segmento: 'Tecnologia & Gadgets',
    descricao: 'Smartphones, eletrônicos, acessórios e tecnologia para rotina, trabalho e lazer.',
    imagem: '/imagens/homepage/loja-geracao-top.svg',
    icon: 'Smartphone',
  },
  {
    nome: 'Estruturaline',
    segmento: 'Construção & Estruturas',
    descricao: 'Materiais, ferramentas e soluções para quem constrói com seriedade.',
    imagem: '/imagens/homepage/loja-estruturaline.svg',
    icon: 'Hammer',
  },
  {
    nome: 'Gran Festas',
    segmento: 'Festas & Eventos',
    descricao: 'Decoração, utensílios e produtos para encontros, comemorações e eventos.',
    imagem: '/imagens/homepage/loja-gran-festas.svg',
    icon: 'Gift',
  },
];

const storeIconMap = {
  Plane,
  Watch,
  Shirt,
  Smartphone,
  Hammer,
  Gift,
  Store,
};

const normalizeKey = (value) =>
  String(value || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '');

const getConfigForStore = (storeConfigs, loja) => {
  const candidates = [
    loja.slug,
    loja.nome,
    loja.Nome,
    loja.segmento,
    loja.Segmento,
  ].map(normalizeKey);

  return storeConfigs.find((config) => candidates.includes(normalizeKey(config.slug || config.nome || config.title))) || null;
};

export default function Lojas() {
  const [lojas, setLojas] = useState([]);
  const [storeConfigs, setStoreConfigs] = useState(defaultStoreCards);
  const [loading, setLoading] = useState(true);

  const loadLojas = useCallback(async () => {
    try {
      setLoading(true);
      const [lojasResponse, siteResponse] = await Promise.all([
        lojaAPI.getAll(),
        siteAPI.getPublicConfig(),
      ]);

      const lojasData = unwrapApiData(lojasResponse.data);
      const siteConfig = unwrapApiData(siteResponse.data) || {};
      setLojas(Array.isArray(lojasData) && lojasData.length > 0 ? lojasData : defaultStoreCards);
      setStoreConfigs(Array.isArray(siteConfig.storeCards) && siteConfig.storeCards.length > 0 ? siteConfig.storeCards : defaultStoreCards);
    } catch {
      setLojas(defaultStoreCards);
      setStoreConfigs(defaultStoreCards);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadLojas();
  }, [loadLojas]);

  const lojasVisuais = useMemo(
    () =>
      lojas.map((loja) => {
        const config = getConfigForStore(storeConfigs, loja) || {};
        return {
          ...loja,
          nome: loja.nome || loja.Nome || config.nome || config.title || 'Loja Grupo Nexum Altivon',
          segmento: loja.segmento || loja.Segmento || config.segmento || '',
          descricao: loja.descricao || loja.Descricao || config.descricao || '',
          imagem: config.imagem || config.image || '/imagens/homepage/banner-ecommerce.svg',
          icon: config.icon || 'Store',
          endereco: loja.endereco || loja.Endereco,
          cidade: loja.cidade || loja.Cidade,
          estado: loja.estado || loja.Estado,
          telefone: loja.telefone || loja.Telefone,
          email: loja.email || loja.Email,
        };
      }),
    [lojas, storeConfigs],
  );

  return (
    <main className="min-h-screen bg-[#050505] text-white">
      <section className="relative overflow-hidden border-b border-[#2A2A2A] px-4 py-16 sm:px-6 lg:px-8">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,#C9A22722,transparent_42%)]" />
        <div className="relative mx-auto max-w-7xl">
          <p className="text-sm font-black uppercase tracking-[0.24em] text-[#E8D5A3]">Grupo Nexum Altivon</p>
          <h1 className="mt-4 font-serif text-5xl font-bold text-[#C9A227]" data-testid="lojas-title">Nossas Lojas</h1>
          <p className="mt-5 max-w-2xl text-lg leading-8 text-zinc-300">
            Seis marcas em operação integrada, com imagem, posicionamento e conteúdo administráveis pelo painel de configuração da home.
          </p>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6 lg:px-8">
        {loading ? (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {[1, 2, 3, 4, 5, 6].map((item) => (
              <div key={item} className="h-[420px] animate-pulse rounded-[28px] border border-[#2A2A2A] bg-[#111111]" />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-7 md:grid-cols-2 lg:grid-cols-3" data-testid="lojas-grid">
            {lojasVisuais.map((loja, index) => {
              const Icon = storeIconMap[loja.icon] || Building2;
              return (
                <article key={loja.id || loja.nome || index} className="group overflow-hidden rounded-[28px] border border-[#2A2A2A] bg-[#111111] shadow-2xl shadow-black/25 transition hover:-translate-y-1 hover:border-[#C9A227]/70" data-testid={`loja-${loja.id || index}`}>
                  <div className="relative h-56 overflow-hidden bg-[#0A0A0A]">
                    <img src={loja.imagem} alt={loja.nome} className="h-full w-full object-cover transition duration-500 group-hover:scale-105" />
                    <div className="absolute inset-0 bg-gradient-to-t from-[#111111] via-transparent to-transparent" />
                    <div className="absolute left-5 top-5 flex h-12 w-12 items-center justify-center rounded-full bg-black/75 text-[#C9A227]">
                      <Icon size={24} />
                    </div>
                  </div>

                  <div className="p-6">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h2 className="font-serif text-2xl font-bold text-[#C9A227]">{loja.nome}</h2>
                        <p className="mt-2 text-xs font-black uppercase tracking-[0.16em] text-[#E8D5A3]">{loja.segmento || 'Operação comercial'}</p>
                      </div>
                      <Building2 className="shrink-0 text-zinc-600" size={22} />
                    </div>

                    <p className="mt-5 min-h-24 text-sm leading-7 text-zinc-300">{loja.descricao || 'Loja integrada ao Grupo Nexum Altivon.'}</p>

                    <div className="mt-6 space-y-3 border-t border-white/10 pt-5 text-sm text-zinc-300">
                      {(loja.endereco || loja.cidade || loja.estado) && (
                        <div className="flex items-start gap-2">
                          <MapPin className="mt-0.5 shrink-0 text-[#C9A227]" size={17} />
                          <div>
                            {loja.endereco && <p>{loja.endereco}</p>}
                            {(loja.cidade || loja.estado) && <p className="text-zinc-500">{loja.cidade}{loja.cidade && loja.estado ? '/' : ''}{loja.estado}</p>}
                          </div>
                        </div>
                      )}

                      {loja.telefone && (
                        <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href={`tel:${String(loja.telefone).replace(/\D/g, '')}`}>
                          <Phone size={17} />
                          {loja.telefone}
                        </a>
                      )}

                      {loja.email && (
                        <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href={`mailto:${loja.email}`}>
                          <Mail size={17} />
                          {loja.email}
                        </a>
                      )}
                    </div>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
