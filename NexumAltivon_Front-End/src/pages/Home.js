/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  Building2,
  Check,
  ChevronLeft,
  ChevronRight,
  Gift,
  Hammer,
  Headphones,
  LoaderCircle,
  Mail,
  MessageCircleMore,
  Pause,
  Phone,
  Plane,
  Play,
  ShieldCheck,
  Shirt,
  Smartphone,
  Store,
  Truck,
  UserPlus,
  Watch,
} from 'lucide-react';
import ProductCard from '../components/ProductCard';
import { clienteAPI, produtoAPI, resolvePublicAssetUrl, siteAPI, unwrapApiData } from '../services/api';
import { buildWhatsAppLink, supportMessages } from '../utils/supportLinks';
import { isValidCpfCnpj } from '../utils/validation';
import { buildProfileThemeStyle } from '../utils/siteTheme';

const resolveLogo = (logo) => {
  const value = String(logo || '').trim();
  return resolvePublicAssetUrl(value);
};

const resolveMediaCollection = (items) => (
  Array.isArray(items)
    ? items.map((item) => ({
        ...item,
        image: resolvePublicAssetUrl(item?.image),
        imagem: resolvePublicAssetUrl(item?.imagem),
        logo: resolvePublicAssetUrl(item?.logo),
      }))
    : items
);

const emptyCadastro = {
  nome: '',
  email: '',
  telefone: '',
  cpf: '',
  senha: '',
  newsletter: true,
};

const qualidade = [
  'Curadoria rigorosa de fornecedores',
  'Atendimento humano e especializado',
  'Política de devolução simplificada',
  'Preços justos e acessíveis',
];

const partnerIconMap = {
  Store,
  Truck,
  Building2,
};

const storeIconMap = {
  Plane,
  Watch,
  Shirt,
  Smartphone,
  Hammer,
  Gift,
};

const normalizeText = (value) => String(value || '').trim().toLowerCase();
const normalizeDocument = (value) => String(value || '').replace(/\D/g, '');

const isProdutoPublicavel = (produto) =>
  Boolean(
    produto &&
      produto.ativo !== false &&
      produto.nome &&
      produto.sku &&
      (produto.slug || produto.id) &&
      (produto.descricao_curta || produto.descricaoCurta || produto.descricao_longa || produto.descricaoLonga || produto.descricao) &&
      (produto.imagem_url || produto.imagemUrl || produto.imagem || produto.imagemPrincipal || produto.imagem_principal) &&
      Number(produto.preco) > 0 &&
      Number(produto.peso) > 0 &&
      Number(produto.altura) > 0 &&
      Number(produto.largura) > 0 &&
      Number(produto.comprimento) > 0,
  );

const pickConfigValue = (config, keys, fallback = '') => {
  for (const key of keys) {
    const value = config?.[key];
    if (value !== undefined && value !== null && String(value).trim() !== '') {
      return value;
    }
  }

  return fallback;
};

const mapPublicSiteConfig = (config) => {
  if (!config || typeof config !== 'object' || Array.isArray(config)) return null;

  return {
    siteName: pickConfigValue(config, ['siteNome', 'SiteNome', 'site_name', 'siteName'], 'Grupo Nexum Altivon'),
    siteUrl: pickConfigValue(config, ['siteUrl', 'SiteUrl', 'site_url'], 'https://nexumaltivon.com.br'),
    contactEmail: pickConfigValue(config, ['contactEmail', 'ContactEmail', 'site_email_contato', 'siteEmailContato'], 'corporativo.gna@gmail.com'),
    primaryPhone: pickConfigValue(config, ['primaryPhone', 'PrimaryPhone', 'site_telefone', 'siteTelefone'], '(14) 99673-1879'),
    secondaryPhone: pickConfigValue(config, ['secondaryPhone', 'SecondaryPhone', 'site_telefone_secundario', 'siteTelefoneSecundario'], '(14) 99673-1879'),
    primaryWhatsapp: pickConfigValue(config, ['primaryWhatsapp', 'PrimaryWhatsapp', 'site_whatsapp', 'siteWhatsapp'], '5514996731879'),
    secondaryWhatsapp: pickConfigValue(config, ['secondaryWhatsapp', 'SecondaryWhatsapp', 'site_whatsapp_secundario', 'siteWhatsappSecundario'], '5514996731879'),
    yaraEmail: pickConfigValue(config, ['yaraEmail', 'YaraEmail', 'site_yara_email', 'siteYaraEmail'], 'corporativo.gna@gmail.com'),
    siteLogo: resolveLogo(pickConfigValue(config, ['siteLogo', 'SiteLogo', 'site_logo', 'siteLogoUrl'], '')),
    institutionalUrl: pickConfigValue(config, ['institutionalUrl', 'InstitutionalUrl', 'site_institucional_url', 'siteInstitucionalUrl'], '/institucional'),
    privacyUrl: pickConfigValue(config, ['privacyUrl', 'PrivacyUrl', 'site_politica_privacidade_url', 'sitePoliticaPrivacidadeUrl'], '/politica-privacidade'),
    refundUrl: pickConfigValue(config, ['refundUrl', 'RefundUrl', 'site_politica_reembolso_url', 'sitePoliticaReembolsoUrl'], '/politica-reembolso'),
    heroIntervalSeconds: Number(pickConfigValue(config, ['heroIntervalSeconds', 'HeroIntervalSeconds'], 7)),
    partnerRotationSeconds: Number(pickConfigValue(config, ['partnerRotationSeconds', 'PartnerRotationSeconds'], 8)),
    heroSlides: resolveMediaCollection(pickConfigValue(config, ['heroSlides', 'HeroSlides'], [])),
    storeCards: resolveMediaCollection(pickConfigValue(config, ['storeCards', 'StoreCards', 'home_lojas_cards', 'homeLojasCards'], [])),
    introTitle: pickConfigValue(config, ['introTitle', 'IntroTitle', 'home_intro_titulo', 'homeIntroTitulo'], 'Uma Nova Era Começa'),
    introText1: pickConfigValue(config, ['introText1', 'IntroText1', 'home_intro_texto_1', 'homeIntroTexto1'], 'O Grupo Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.'),
    introText2: pickConfigValue(config, ['introText2', 'IntroText2', 'home_intro_texto_2', 'homeIntroTexto2'], 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.'),
    introBadge: pickConfigValue(config, ['introBadge', 'IntroBadge', 'home_intro_badge', 'homeIntroBadge'], 'nexumaltivon.com.br'),
    qualityItems: pickConfigValue(config, ['qualityItems', 'QualityItems', 'home_quality_items', 'homeQualityItems'], []),
    partnerCards: resolveMediaCollection(pickConfigValue(config, ['partnerCards', 'PartnerCards'], [])),
    footerText: pickConfigValue(config, ['footerText', 'FooterText', 'home_footer_texto', 'homeFooterTexto'], 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.'),
  };
};
export default function Home() {
  const navigate = useNavigate();
  const [currentSlide, setCurrentSlide] = useState(0);
  const [siteConfig, setSiteConfig] = useState(null);
  const [siteConfigLoading, setSiteConfigLoading] = useState(true);
  const [siteConfigError, setSiteConfigError] = useState('');
  const [siteMediaError, setSiteMediaError] = useState('');
  const [isCarouselPaused, setIsCarouselPaused] = useState(false);
  const [partnerPage, setPartnerPage] = useState(0);
  const [failedSlideImages, setFailedSlideImages] = useState(() => new Set());
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [loadingProducts, setLoadingProducts] = useState(true);
  const [catalogSearch, setCatalogSearch] = useState('');
  const [cadastroForm, setCadastroForm] = useState(emptyCadastro);
  const [cadastroStatus, setCadastroStatus] = useState({ tone: '', message: '' });
  const [loadingCadastro, setLoadingCadastro] = useState(false);

  const displaySlides = Array.isArray(siteConfig?.heroSlides) ? siteConfig.heroSlides : [];
  const activeSlide = displaySlides[currentSlide] || displaySlides[0] || null;
  const displayStores = Array.isArray(siteConfig?.storeCards) ? siteConfig.storeCards : [];
  const qualityItems = Array.isArray(siteConfig?.qualityItems) && siteConfig.qualityItems.length > 0 ? siteConfig.qualityItems : qualidade;
  const partnerCards = Array.isArray(siteConfig?.partnerCards) ? siteConfig.partnerCards : [];
  const partnerPageCount = Math.max(1, Math.ceil(partnerCards.length / 4));
  const visiblePartnerCards = partnerCards.slice(partnerPage * 4, partnerPage * 4 + 4);
  const introTitle = siteConfig?.introTitle || 'Uma Nova Era Começa';
  const introText1 =
    siteConfig?.introText1 || 'O Grupo Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.';
  const introText2 =
    siteConfig?.introText2 || 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.';
  const introBadge = siteConfig?.introBadge || 'nexumaltivon.com.br';
  const primaryPhone = siteConfig?.primaryPhone || '+55 (14) 99673-1879';
  const secondaryPhone = siteConfig?.secondaryPhone || '+55 (14) 99673-1879';
  const publicContactEmail = siteConfig?.contactEmail || 'corporativo.gna@gmail.com';
  const yaraInstantHref = buildWhatsAppLink(siteConfig?.primaryWhatsapp, supportMessages.yaraSales);
  const siteLogo = siteConfig?.siteLogo || '';
  const institutionalUrl = siteConfig?.institutionalUrl || '/institucional';
  const privacyUrl = siteConfig?.privacyUrl || '/politica-privacidade';
  const refundUrl = siteConfig?.refundUrl || '/politica-reembolso';

  useEffect(() => {
    let active = true;

    siteAPI
      .getPublicConfig()
      .then((response) => {
        const config = unwrapApiData(response.data);
        const mappedConfig = config ? mapPublicSiteConfig(config) : null;
        if (!mappedConfig || !Array.isArray(mappedConfig.heroSlides) || mappedConfig.heroSlides.length === 0) {
          throw new Error('A API não retornou banners ativos para a home.');
        }
        if (active) {
          setSiteConfig(mappedConfig);
          setSiteConfigError('');
        }
      })
      .catch((error) => {
        if (active) {
          setSiteConfig(null);
          setSiteConfigError(error?.message || 'A configuração pública não respondeu.');
        }
      })
      .finally(() => {
        if (active) setSiteConfigLoading(false);
      });

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    let active = true;

    const loadFeaturedProducts = async () => {
      setLoadingProducts(true);

      try {
        const destaquesRes = await produtoAPI.getDestaques(4);
        let produtos = unwrapApiData(destaquesRes.data);

        if (!Array.isArray(produtos) || produtos.length === 0) {
          const todosRes = await produtoAPI.getAll({ pagina: 1, itensPorPagina: 4 });
          produtos = unwrapApiData(todosRes.data);
        }

        if (active) {
          const publicaveis = Array.isArray(produtos)
            ? produtos.filter(isProdutoPublicavel).slice(0, 4)
            : [];
          setFeaturedProducts(publicaveis);
        }
      } catch {
        if (active) {
          setFeaturedProducts([]);
        }
      } finally {
        if (active) {
          setLoadingProducts(false);
        }
      }
    };

    loadFeaturedProducts();

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (currentSlide >= displaySlides.length) {
      setCurrentSlide(0);
    }
  }, [currentSlide, displaySlides.length]);

  useEffect(() => {
    if (isCarouselPaused || displaySlides.length < 2) return undefined;

    const seconds = Math.min(30, Math.max(3, Number(siteConfig?.heroIntervalSeconds) || 7));
    const timer = window.setInterval(() => {
      setCurrentSlide((slide) => (slide + 1) % displaySlides.length);
    }, seconds * 1000);

    return () => window.clearInterval(timer);
  }, [displaySlides.length, isCarouselPaused, siteConfig?.heroIntervalSeconds]);

  useEffect(() => {
    if (partnerPage >= partnerPageCount) setPartnerPage(0);
  }, [partnerPage, partnerPageCount]);

  useEffect(() => {
    if (partnerPageCount < 2) return undefined;
    const seconds = Math.min(60, Math.max(5, Number(siteConfig?.partnerRotationSeconds) || 8));
    const timer = window.setInterval(() => setPartnerPage((current) => (current + 1) % partnerPageCount), seconds * 1000);
    return () => window.clearInterval(timer);
  }, [partnerPageCount, siteConfig?.partnerRotationSeconds]);

  const cadastroDuplicadoLocal = useMemo(() => {
    if (!cadastroStatus.message || cadastroStatus.tone !== 'info') return false;
    return true;
  }, [cadastroStatus]);

  const changeSlide = (direction) => {
      setCurrentSlide((slide) => {
        if (direction === 'prev') {
        return slide === 0 ? displaySlides.length - 1 : slide - 1;
      }

      return (slide + 1) % displaySlides.length;
    });
  };

  const handleCadastroChange = (field, value) => {
    setCadastroForm((current) => ({ ...current, [field]: value }));
    if (cadastroStatus.message) {
      setCadastroStatus({ tone: '', message: '' });
    }
  };

  const handleCadastroSubmit = async (event) => {
    event.preventDefault();

    const payload = {
      nome: cadastroForm.nome.trim(),
      email: cadastroForm.email.trim(),
      telefone: cadastroForm.telefone.trim(),
      cpf: cadastroForm.cpf.trim(),
      senha: cadastroForm.senha,
      newsletter: Boolean(cadastroForm.newsletter),
    };

      if (!payload.nome || !payload.email || !payload.senha) {
        setCadastroStatus({ tone: 'error', message: 'Preencha nome, e-mail e uma senha para concluir o auto cadastro e liberar a área do cliente.' });
        return;
      }

    if (payload.cpf && !isValidCpfCnpj(payload.cpf)) {
      setCadastroStatus({ tone: 'error', message: 'CPF/CNPJ inválido. Corrija o documento antes de continuar.' });
      return;
    }

    setLoadingCadastro(true);
    setCadastroStatus({ tone: '', message: '' });

    try {
      const email = normalizeText(payload.email);
      const cpf = normalizeDocument(payload.cpf);
      try {
        const verificacao = await clienteAPI.verificarCadastro({ email, cpf });

        if (verificacao.data?.existe) {
          const nomeExistente = verificacao.data?.cliente?.nome || payload.nome;
          setCadastroStatus({
            tone: 'info',
            message: `Já existe um cadastro para ${nomeExistente}. Não vamos duplicar seus dados; você pode seguir comprando com esse mesmo registro.`,
          });
          return;
        }
      } catch (verificationError) {
        setCadastroStatus({
          tone: 'error',
          message:
            verificationError?.response?.data?.mensagem ||
            verificationError?.response?.data?.detail ||
            'Não foi possível confirmar se o cadastro já existe. Nenhum dado foi gravado; tente novamente quando a API estiver disponível.',
        });
        return;
      }

      const cadastroResponse = await clienteAPI.create(payload);
      const mensagemCadastro =
        cadastroResponse.data?.mensagem ||
        cadastroResponse.data?.Mensagem ||
        'Cadastro realizado com sucesso. Verifique seu e-mail para confirmar o acesso.';

      setCadastroForm(emptyCadastro);
      setCadastroStatus({
        tone: 'success',
        message: mensagemCadastro,
      });
    } catch (error) {
      const detail =
        error.response?.data?.detail ||
        error.response?.data?.mensagem ||
        error.message ||
        'Não foi possível concluir seu cadastro agora.';

      setCadastroStatus({ tone: 'error', message: detail });
    } finally {
      setLoadingCadastro(false);
    }
  };

  const handleCatalogSearch = (event) => {
    event.preventDefault();
    const query = catalogSearch.trim();
    navigate(query ? `/produtos?busca=${encodeURIComponent(query)}` : '/produtos');
  };

  return (
    <main className="nexum-front-original bg-[#050505] text-[#f5f5f5]">
      <section id="home" className="relative min-h-[84vh] overflow-hidden">
        {displaySlides.map((slide, index) => {
          const slideKey = String(slide.id || index);
          const imageFailed = failedSlideImages.has(slideKey);
          return (
          <div
            key={slideKey}
            className={`absolute inset-0 transition-opacity duration-1000 ${index === currentSlide ? 'opacity-100' : 'pointer-events-none opacity-0'}`}
          >
            {!imageFailed && slide.image ? (
              <img
                src={resolvePublicAssetUrl(slide.image)}
                alt={slide.imageAlt || slide.highlight || 'Banner institucional'}
                className="h-full w-full object-cover"
                onError={() => {
                  setFailedSlideImages((current) => new Set(current).add(slideKey));
                  setSiteMediaError(`A imagem configurada para o slide ${index + 1} não respondeu.`);
                }}
              />
            ) : (
              <div className="flex h-full w-full items-end bg-zinc-950 p-6 text-sm font-bold text-amber-200">
                Imagem do slide {index + 1} indisponível
              </div>
            )}
            <div className="absolute inset-0 bg-gradient-to-r from-black via-black/55 to-black/75" />
          </div>
          );
        })}

        <div className="relative mx-auto flex min-h-[84vh] max-w-7xl items-center px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-3xl">
            {(siteConfigLoading || siteConfigError || siteMediaError) && (
              <div className="mb-5 border border-amber-400/60 bg-black/80 px-4 py-3 text-sm font-bold text-amber-100" role="alert">
                {siteConfigLoading ? 'Carregando a configuração pública do portal...' : (siteConfigError ? `Configuração pública indisponível: ${siteConfigError}` : siteMediaError)}
              </div>
            )}
            {activeSlide && (
              <>
                <p className="mb-5 inline-flex items-center rounded-full border border-[#C9A227]/40 bg-black/40 px-4 py-2 text-xs font-bold uppercase tracking-[0.25em] text-[#E8D5A3]">
                  {activeSlide.badge}
                </p>
                <h1 className="font-serif text-5xl font-bold leading-[1.02] text-white sm:text-6xl lg:text-7xl" data-testid="hero-title">
                  {activeSlide.title}
                  <span className="block text-[#C9A227]">{activeSlide.highlight}</span>
                </h1>
              </>
            )}
            <div className="mt-6 flex items-start gap-4">
              {siteLogo && (
                <img
                  src={siteLogo}
                  alt="Logotipo Grupo Nexum Altivon"
                  className="hidden h-16 w-16 rounded-2xl border border-[#C9A227]/30 bg-black/60 object-contain p-2 sm:block"
                  onError={(event) => {
                    setSiteMediaError('A logomarca configurada não respondeu.');
                    event.currentTarget.hidden = true;
                  }}
                />
              )}
              {activeSlide && <p className="max-w-2xl text-lg leading-8 text-zinc-200 sm:text-xl">{activeSlide.description}</p>}
            </div>

            <form onSubmit={handleCatalogSearch} className="mt-8 flex max-w-2xl gap-3 rounded-2xl border border-white/10 bg-black/40 p-3 backdrop-blur">
              <input
                type="search"
                value={catalogSearch}
                onChange={(event) => setCatalogSearch(event.target.value)}
                placeholder="Buscar por nome, SKU ou categoria"
                className="min-w-0 flex-1 rounded-xl border border-white/10 bg-black/40 px-4 py-3 text-sm text-white outline-none placeholder:text-zinc-500 focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
                aria-label="Buscar produtos no catálogo"
              />
              <button
                type="submit"
                className="rounded-xl bg-[#C9A227] px-5 py-3 text-sm font-black text-black transition hover:bg-[#E8D5A3]"
              >
                Buscar
              </button>
            </form>

            <div className="mt-9 flex flex-col gap-3 sm:flex-row">
              <a
                href="#lojas"
                className="inline-flex items-center justify-center gap-2 rounded-full bg-[#C9A227] px-7 py-4 text-sm font-black uppercase tracking-wide text-black shadow-lg shadow-[#C9A227]/20 transition hover:bg-[#E8D5A3]"
                data-testid="hero-cta"
              >
                Conheça Nossas Lojas
                <ArrowRight size={18} />
              </a>
              <a
                href={institutionalUrl}
                className="inline-flex items-center justify-center gap-2 rounded-full border border-white/35 px-7 py-4 text-sm font-black uppercase tracking-wide text-white transition hover:border-[#C9A227] hover:text-[#C9A227]"
              >
                Institucional
                <Building2 size={18} />
              </a>
            </div>

            {displaySlides.length > 1 && (
              <div className="mt-8 flex flex-wrap items-center gap-3">
                {displaySlides.map((slide, index) => (
                  <button
                    key={slide.id}
                    type="button"
                    aria-label={`Exibir banner ${index + 1}`}
                    onClick={() => setCurrentSlide(index)}
                    className={`h-3 rounded-full transition-all ${index === currentSlide ? 'w-10 bg-[#C9A227]' : 'w-3 bg-white/40 hover:bg-white/70'}`}
                  />
                ))}
                <button
                  type="button"
                  onClick={() => setIsCarouselPaused((current) => !current)}
                  title={isCarouselPaused ? 'Retomar troca automática' : 'Pausar troca automática'}
                  aria-label={isCarouselPaused ? 'Retomar troca automática dos banners' : 'Pausar troca automática dos banners'}
                  className="ml-2 inline-flex h-9 w-9 items-center justify-center rounded-full border border-white/30 bg-black/40 text-white transition hover:border-[#C9A227] hover:text-[#C9A227]"
                >
                  {isCarouselPaused ? <Play size={15} /> : <Pause size={15} />}
                </button>
              </div>
            )}
          </div>
        </div>

        {displaySlides.length > 1 && <div className="pointer-events-none absolute inset-y-0 left-0 right-0 mx-auto hidden max-w-7xl items-center justify-between px-4 sm:flex sm:px-6 lg:px-8">
          <button
            type="button"
            onClick={() => changeSlide('prev')}
            className="pointer-events-auto inline-flex h-12 w-12 items-center justify-center rounded-full border border-white/15 bg-black/35 text-white transition hover:border-[#C9A227] hover:text-[#C9A227]"
            aria-label="Banner anterior"
          >
            <ChevronLeft size={22} />
          </button>
          <button
            type="button"
            onClick={() => changeSlide('next')}
            className="pointer-events-auto inline-flex h-12 w-12 items-center justify-center rounded-full border border-white/15 bg-black/35 text-white transition hover:border-[#C9A227] hover:text-[#C9A227]"
            aria-label="Próximo banner"
          >
            <ChevronRight size={22} />
          </button>
        </div>}
      </section>

      <section className="relative overflow-hidden bg-gradient-to-br from-[#0A0A0A] to-[#1A1A1A] px-4 py-20 text-center">
        <div className="absolute left-1/2 top-0 h-96 w-96 -translate-x-1/2 rounded-full bg-[#C9A227]/5 blur-3xl" />
        <div className="relative mx-auto max-w-4xl">
          <h2 className="font-serif text-4xl font-bold text-[#C9A227]">{introTitle}</h2>
          <p className="mt-7 text-lg leading-8 text-zinc-100">
            {introText1}
          </p>
          <p className="mt-4 text-lg leading-8 text-zinc-100">
            {introText2}
          </p>
          <p className="mt-4 text-zinc-400">Seis marcas. Uma visão. Milhares de produtos escolhidos a dedo para você.</p>
          <div className="mx-auto mt-8 inline-flex rounded-full border border-[#C9A227]/35 bg-[#C9A227]/10 px-6 py-3 text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">
            {introBadge}
          </div>
        </div>
      </section>

      <section id="produtos-destaque" className="relative overflow-hidden border-y border-[#2A2A2A] bg-[#080808] px-4 py-20">
        <div className="absolute -left-24 top-16 h-72 w-72 rounded-full bg-[#C9A227]/10 blur-3xl" />
        <div className="absolute -right-28 bottom-0 h-80 w-80 rounded-full bg-emerald-500/5 blur-3xl" />
        <div className="relative mx-auto max-w-7xl sm:px-6 lg:px-8">
          <div className="mb-10 flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.25em] text-[#E8D5A3]">Vitrine em operação</p>
              <h2 className="mt-4 font-serif text-4xl font-bold leading-tight text-[#C9A227]">Produtos disponíveis para compra</h2>
              <p className="mt-4 max-w-2xl text-zinc-400">
                Itens carregados diretamente da operação comercial, com estoque, preço e checkout conectados ao fluxo interno de vendas.
              </p>
            </div>
            <Link
              to="/produtos"
              className="inline-flex items-center justify-center gap-2 rounded-full border border-[#C9A227]/40 px-6 py-3 text-sm font-black uppercase tracking-wide text-[#E8D5A3] transition hover:border-[#E8D5A3] hover:text-white"
            >
              Ver catálogo completo
              <ArrowRight size={17} />
            </Link>
          </div>

          {loadingProducts && (
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
              {[1, 2, 3, 4].map((item) => (
                <div key={item} className="h-[430px] animate-pulse rounded-2xl border border-[#2A2A2A] bg-[#111111]" />
              ))}
            </div>
          )}

          {!loadingProducts && featuredProducts.length > 0 && (
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4" data-testid="home-featured-products">
              {featuredProducts.map((produto) => (
                <ProductCard key={produto.id} product={produto} />
              ))}
            </div>
          )}

          {!loadingProducts && featuredProducts.length === 0 && (
            <div className="rounded-[28px] border border-[#C9A227]/25 bg-[#111111] p-8 text-center shadow-2xl shadow-black/30">
              <p className="text-xl font-black text-white">Produtos temporariamente indisponíveis na Home</p>
              <p className="mx-auto mt-3 max-w-2xl text-sm leading-6 text-zinc-400">
                A vitrine exibe somente produtos completos e liberados para venda. Use o catálogo completo como rota de consulta enquanto a curadoria é atualizada.
              </p>
              <Link to="/produtos" className="mt-6 inline-flex rounded-full bg-[#C9A227] px-6 py-3 text-sm font-black text-black">
                Abrir catálogo
              </Link>
            </div>
          )}
        </div>
      </section>

      <section id="cadastro" className="bg-[#101010] px-4 py-20">
        <div className="mx-auto grid max-w-7xl gap-10 sm:px-6 lg:grid-cols-[1.1fr_0.9fr] lg:items-start lg:px-8">
          <div>
            <p className="text-sm font-black uppercase tracking-[0.25em] text-[#E8D5A3]">Cadastro do cliente</p>
            <h2 className="mt-4 font-serif text-4xl font-bold leading-tight text-[#C9A227]">Seu cadastro direto na home, sem duplicidade</h2>
            <p className="mt-6 max-w-2xl leading-8 text-zinc-300">
              Agora o próprio cliente já consegue se registrar por aqui. Antes de salvar, o sistema verifica e-mail e CPF/CNPJ para evitar cadastro duplicado.
            </p>
            <a
              href={yaraInstantHref}
              className="mt-6 inline-flex h-12 w-12 items-center justify-center rounded-full border border-[#C9A227]/30 bg-[#C9A227]/10 text-[#E8D5A3] transition hover:border-[#E8D5A3] hover:text-white"
              aria-label="Atendimento Yara"
              title="Atendimento Yara"
            >
              <MessageCircleMore size={18} />
            </a>

            <div className="mt-8 grid gap-4">
              {[
                'Verificação prévia de e-mail e documento antes do envio.',
                'Mesmo cadastro reutilizado no checkout e nos fluxos comerciais.',
                'Base preparada para relacionamento, histórico, NFs e futuras áreas do cliente.',
              ].map((item) => (
                <div key={item} className="flex items-start gap-3 rounded-xl border border-[#2A2A2A] bg-black/30 px-4 py-4 text-sm font-semibold text-zinc-100">
                  <BadgeCheck className="mt-0.5 text-[#C9A227]" size={18} />
                  <span>{item}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="rounded-[28px] border border-[#C9A227]/20 bg-[#171717] p-6 shadow-2xl shadow-black/30 sm:p-8">
            <div className="mb-6 flex items-center justify-between">
              <div>
                <p className="text-sm font-black uppercase tracking-[0.22em] text-[#E8D5A3]">Auto cadastro</p>
                <h3 className="mt-2 text-2xl font-black text-white">Quero começar meu relacionamento</h3>
              </div>
              <div className="hidden h-14 w-14 items-center justify-center rounded-2xl bg-[#C9A227] text-black sm:flex">
                <UserPlus size={26} />
              </div>
            </div>

            <form className="grid gap-4" onSubmit={handleCadastroSubmit}>
              <label className="grid gap-2 text-sm font-semibold text-zinc-200">
                Nome completo / Razão social
                <input
                  type="text"
                  value={cadastroForm.nome}
                  onChange={(event) => handleCadastroChange('nome', event.target.value)}
                  className="rounded-2xl border border-white/10 bg-black/30 px-4 py-3 text-white outline-none transition focus:border-[#C9A227]"
                  placeholder="Digite seu nome"
                  required
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-zinc-200">
                E-mail principal
                <div className="flex items-center gap-3 rounded-2xl border border-white/10 bg-black/30 px-4 py-3 focus-within:border-[#C9A227]">
                  <Mail size={18} className="text-[#C9A227]" />
                  <input
                    type="email"
                    value={cadastroForm.email}
                    onChange={(event) => handleCadastroChange('email', event.target.value)}
                    className="w-full bg-transparent text-white outline-none"
                    placeholder="voce@empresa.com"
                    required
                  />
                </div>
              </label>

              <div className="grid gap-4 md:grid-cols-2">
                <label className="grid gap-2 text-sm font-semibold text-zinc-200">
                  Telefone / WhatsApp
                  <div className="flex items-center gap-3 rounded-2xl border border-white/10 bg-black/30 px-4 py-3 focus-within:border-[#C9A227]">
                    <Phone size={18} className="text-[#C9A227]" />
                    <input
                      type="text"
                      value={cadastroForm.telefone}
                      onChange={(event) => handleCadastroChange('telefone', event.target.value)}
                      className="w-full bg-transparent text-white outline-none"
                      placeholder="(14) 99999-9999"
                    />
                  </div>
                </label>

                <label className="grid gap-2 text-sm font-semibold text-zinc-200">
                  CPF / CNPJ
                  <input
                    type="text"
                    value={cadastroForm.cpf}
                    onChange={(event) => handleCadastroChange('cpf', event.target.value)}
                    className="rounded-2xl border border-white/10 bg-black/30 px-4 py-3 text-white outline-none transition focus:border-[#C9A227]"
                    placeholder="Somente para evitar duplicidade"
                  />
                </label>
              </div>

              <label className="grid gap-2 text-sm font-semibold text-zinc-200">
                Senha de acesso
                <input
                  type="password"
                  value={cadastroForm.senha}
                  onChange={(event) => handleCadastroChange('senha', event.target.value)}
                  className="rounded-2xl border border-white/10 bg-black/30 px-4 py-3 text-white outline-none transition focus:border-[#C9A227]"
                  placeholder="Crie uma senha para sua área do cliente"
                  required
                />
              </label>

              <label className="flex items-start gap-3 rounded-2xl border border-white/10 bg-black/20 px-4 py-3 text-sm font-semibold text-zinc-200">
                <input
                  type="checkbox"
                  checked={cadastroForm.newsletter}
                  onChange={(event) => handleCadastroChange('newsletter', event.target.checked)}
                  className="mt-1 h-4 w-4 rounded border-white/20 bg-transparent text-[#C9A227] focus:ring-[#C9A227]"
                />
                <span>Quero receber novidades, campanhas e comunicações comerciais do Grupo Nexum Altivon.</span>
              </label>

              {cadastroStatus.message && (
                <div
                  className={`rounded-2xl border px-4 py-3 text-sm font-semibold ${
                    cadastroStatus.tone === 'success'
                      ? 'border-emerald-400/30 bg-emerald-500/10 text-emerald-200'
                      : cadastroStatus.tone === 'info'
                        ? 'border-amber-400/30 bg-amber-500/10 text-amber-100'
                        : 'border-red-400/30 bg-red-500/10 text-red-200'
                  }`}
                >
                  {cadastroStatus.message}
                </div>
              )}

              <button
                type="submit"
                disabled={loadingCadastro}
                className="inline-flex items-center justify-center gap-3 rounded-2xl bg-[#C9A227] px-6 py-4 text-sm font-black uppercase tracking-[0.18em] text-black transition hover:bg-[#E8D5A3] disabled:cursor-not-allowed disabled:opacity-70"
              >
                {loadingCadastro ? <LoaderCircle className="animate-spin" size={18} /> : <UserPlus size={18} />}
                {loadingCadastro ? 'Validando cadastro...' : 'Cadastrar agora'}
              </button>

              <p className="text-xs leading-6 text-zinc-500">
                {cadastroDuplicadoLocal
                  ? 'O sistema encontrou um cadastro compatível e evitou a duplicidade antes de qualquer novo registro.'
                  : 'Usamos seus dados apenas para relacionamento comercial, compras e atendimentos do Grupo Nexum Altivon.'}
              </p>
            </form>
          </div>
        </div>
      </section>

      <section id="lojas" className="mx-auto max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
        <div className="mx-auto mb-14 max-w-3xl text-center">
          <h2 className="font-serif text-4xl font-bold text-[#C9A227]">Nossas Lojas</h2>
          <div className="mx-auto mt-5 h-1 w-24 rounded-full bg-[#C9A227]" />
          <p className="mt-5 text-zinc-400">
            6 lojas especializadas, 6 universos de possibilidades. Cada uma pensada para oferecer a melhor experiência em seu segmento.
          </p>
        </div>

        {displayStores.length === 0 && (
          <div className="rounded-lg border border-amber-400/30 bg-amber-500/10 p-5 text-sm font-bold text-amber-100">Nenhuma loja está publicada no momento.</div>
        )}
        <div className="grid gap-8 md:grid-cols-2 xl:grid-cols-3">
          {displayStores.map((loja) => {
            const Icon = typeof loja.icon === 'string' ? storeIconMap[loja.icon] || Store : loja.icon || Store;
            return (
              <article
                key={loja.id || loja.slug}
                style={buildProfileThemeStyle(loja)}
                className="site-profile-theme group overflow-hidden rounded-lg border border-[#2A2A2A] transition duration-300 hover:-translate-y-2 hover:shadow-2xl"
              >
                <Link to={`/lojas/${loja.slug}`} className="block">
                  <div className="profile-surface relative h-56 overflow-hidden">
                    {loja.imagem && <img src={loja.imagem} alt={loja.nome} className="absolute inset-0 h-full w-full object-cover transition duration-500 group-hover:scale-110" onError={() => setSiteMediaError(`A imagem configurada para ${loja.nome} não respondeu.`)} />}
                    <div className="absolute inset-0 bg-gradient-to-t from-[#1A1A1A] to-transparent" />
                    <div className="profile-primary absolute left-5 top-5 flex h-12 w-12 items-center justify-center rounded-full bg-black/70">
                      <Icon size={24} />
                    </div>
                  </div>
                  <div className="p-6">
                    <h3 className="profile-primary font-serif text-2xl font-bold">{loja.nome}</h3>
                    <p className="mt-3 text-sm font-semibold text-zinc-200">{loja.atividade}</p>
                    <p className="mt-3 min-h-24 text-sm leading-6 text-zinc-400">{loja.descricao}</p>
                    <span className="mt-5 inline-flex rounded-full bg-[#0A0A0A] px-4 py-2 text-xs font-bold uppercase tracking-wide text-[#E8D5A3]">{loja.segmento}</span>
                  </div>
                </Link>
              </article>
            );
          })}
        </div>
      </section>

      <section id="qualidade" className="bg-[#1A1A1A] px-4 py-20">
        <div className="mx-auto grid max-w-7xl gap-12 sm:px-6 lg:grid-cols-2 lg:items-center lg:px-8">
          <div>
            <p className="text-sm font-black uppercase tracking-[0.25em] text-[#E8D5A3]">Qualidade premium</p>
            <h2 className="mt-4 font-serif text-4xl font-bold leading-tight text-[#C9A227]">Compromisso Inabalável com a Qualidade</h2>
            <p className="mt-6 leading-8 text-zinc-300">
              No Grupo Nexum Altivon, qualidade não é apenas uma promessa — é a base de tudo o que fazemos. Cada produto passa por seleção,
              curadoria e análise para atender aos padrões do mercado.
            </p>
            <div className="mt-8 grid gap-3">
              {qualityItems.map((item) => (
                <div key={item} className="flex items-center gap-3 rounded-lg border border-[#2A2A2A] bg-[#0A0A0A] px-4 py-3 text-sm font-semibold text-zinc-100">
                  <Check className="text-[#C9A227]" size={18} />
                  {item}
                </div>
              ))}
            </div>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            {[
              { icon: ShieldCheck, title: 'Compra segura', text: 'Ambiente preparado para operação confiável.' },
              { icon: BadgeCheck, title: 'Curadoria real', text: 'Fornecedores e produtos com seleção cuidadosa.' },
              { icon: Headphones, title: 'Atendimento humano', text: 'Acompanhamento próximo com foco no cliente.' },
              { icon: Truck, title: 'Logística em evolução', text: 'Estrutura preparada para escalar entregas.' },
            ].map((item) => {
              const Icon = item.icon;
              return (
                <div key={item.title} className="rounded-xl border border-[#2A2A2A] bg-[#0A0A0A] p-6">
                  <Icon className="text-[#C9A227]" size={28} />
                  <h3 className="mt-5 text-lg font-black text-white">{item.title}</h3>
                  <p className="mt-2 text-sm leading-6 text-zinc-400">{item.text}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      <section id="parceiros" className="bg-gradient-to-b from-[#050505] to-[#0A0A0A] px-4 py-24 text-center">
        <div className="mx-auto max-w-7xl sm:px-6 lg:px-8">
          <h2 className="font-serif text-4xl font-bold text-[#C9A227]">Seja Nosso Parceiro</h2>
          <p className="mx-auto mt-5 max-w-3xl text-zinc-400">
            O Grupo Nexum Altivon está construindo uma rede forte de parcerias para venda, fornecimento e dropshipping.
          </p>
          {partnerCards.length === 0 && (
            <div className="mt-10 rounded-lg border border-amber-400/30 bg-amber-500/10 p-5 text-sm font-bold text-amber-100">Nenhum parceiro está publicado no momento.</div>
          )}
          <div className="mt-12 grid gap-6 md:grid-cols-2">
            {visiblePartnerCards.map((item) => {
              const Icon = typeof item.icon === 'string' ? partnerIconMap[item.icon] || Building2 : item.icon;
              return (
                <article key={item.id || item.slug} style={buildProfileThemeStyle(item)} className="site-profile-theme overflow-hidden rounded-lg border border-[#2A2A2A] text-left transition">
                  <Link to={`/parceiros/${item.slug}`} className="block">
                    <div className="profile-surface relative aspect-[16/9] overflow-hidden">
                      {item.image ? <img src={item.image} alt={item.title} className="h-full w-full object-cover" onError={() => setSiteMediaError(`A imagem configurada para ${item.title} não respondeu.`)} /> : <div className="flex h-full items-center justify-center"><Icon className="profile-primary" size={54} /></div>}
                    </div>
                  </Link>
                  <div className="p-8">
                  <div className="profile-primary-bg flex h-14 w-14 items-center justify-center rounded-full text-black">
                    <Icon size={26} />
                  </div>
                  <h3 className="mt-6 text-xl font-black text-white">{item.title}</h3>
                  <p className="mt-3 text-sm font-semibold text-zinc-200">{item.activity}</p>
                  <p className="mt-4 min-h-28 text-sm leading-7 text-zinc-400">{item.text}</p>
                  <Link to={`/parceiros/${item.slug}`} className="profile-primary-bg mt-6 inline-flex items-center gap-2 rounded-full px-5 py-3 text-sm font-black text-black transition">Ver perfil <ArrowRight size={16} /></Link>
                  </div>
                </article>
              );
            })}
          </div>
        </div>
      </section>

      <section className="border-t border-white/5 bg-[#050505] px-4 py-12">
        <div className="mx-auto flex max-w-7xl flex-col items-start justify-between gap-4 sm:px-6 md:flex-row md:items-center lg:px-8">
          <div>
            <div className="flex items-center gap-3">
              {siteLogo && (
                <img
                  src={siteLogo}
                  alt="Logotipo Grupo Nexum Altivon"
                  className="h-12 w-12 rounded-xl border border-[#C9A227]/30 bg-black object-contain p-1.5"
                  onError={(event) => {
                    setSiteMediaError('A logomarca configurada não respondeu.');
                    event.currentTarget.hidden = true;
                  }}
                />
              )}
              <p className="text-xs font-black uppercase tracking-[0.25em] text-[#E8D5A3]">Grupo Nexum Altivon</p>
            </div>
            <p className="mt-3 text-sm leading-7 text-zinc-400">{siteConfig?.footerText || 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.'}</p>
            <p className="mt-2 text-xs font-semibold uppercase tracking-[0.14em] text-zinc-500">
              Atendimento: {primaryPhone} · {secondaryPhone} · {publicContactEmail}
            </p>
            <div className="mt-4 flex flex-wrap gap-3 text-xs font-bold uppercase tracking-[0.12em] text-zinc-400">
              <a href={institutionalUrl} className="transition hover:text-[#C9A227]">Institucional</a>
              <a href={privacyUrl} className="transition hover:text-[#C9A227]">Política de privacidade</a>
              <a href={refundUrl} className="transition hover:text-[#C9A227]">Política de reembolso</a>
            </div>
          </div>
          {partnerPageCount > 1 && (
            <div className="mt-8 flex items-center justify-center gap-4">
              <button type="button" onClick={() => setPartnerPage((current) => (current - 1 + partnerPageCount) % partnerPageCount)} className="flex h-10 w-10 items-center justify-center rounded-full border border-white/20" title="Parceiros anteriores"><ChevronLeft size={18} /></button>
              <span className="text-sm font-black">{partnerPage + 1} de {partnerPageCount}</span>
              <button type="button" onClick={() => setPartnerPage((current) => (current + 1) % partnerPageCount)} className="flex h-10 w-10 items-center justify-center rounded-full border border-white/20" title="Próximos parceiros"><ChevronRight size={18} /></button>
            </div>
          )}
          <div className="flex flex-wrap gap-3">
            <a
              href="#home"
              className="inline-flex items-center gap-2 rounded-full border border-white/10 px-4 py-3 text-sm font-bold text-zinc-200 transition hover:border-[#C9A227] hover:text-[#C9A227]"
            >
              <ArrowLeft size={16} />
              Voltar ao topo
            </a>
            <Link
              to="/produtos"
              className="inline-flex items-center gap-2 rounded-full bg-[#C9A227] px-5 py-3 text-sm font-black text-black transition hover:bg-[#E8D5A3]"
            >
              Explorar catálogo
              <ArrowRight size={16} />
            </Link>
          </div>
        </div>
      </section>

    </main>
  );
}
