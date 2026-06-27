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
  Phone,
  Plane,
  ShieldCheck,
  Shirt,
  Smartphone,
  Store,
  Truck,
  UserPlus,
  Watch,
} from 'lucide-react';
import ProductCard from '../components/ProductCard';
import { clienteAPI, produtoAPI, siteAPI, unwrapApiData } from '../services/api';
import { isValidCpfCnpj } from '../utils/validation';

const heroSlides = [
  {
    id: 'ecommerce',
    badge: 'Grupo Nexum Altivon',
    title: 'O Futuro do',
    highlight: 'E-Commerce',
    description:
      'Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.',
    image: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=1920&q=88',
  },
  {
    id: 'marcas',
    badge: '6 marcas em expansão',
    title: 'Uma operação,',
    highlight: 'múltiplos mercados',
    description:
      'Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.',
    image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1920&q=88',
  },
  {
    id: 'tecnologia',
    badge: 'Experiência tecnológica',
    title: 'Compra segura com',
    highlight: 'atendimento humano',
    description:
      'Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.',
    image: 'https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=1920&q=88',
  },
  {
    id: 'audio',
    badge: 'Qualidade premium',
    title: 'Produtos escolhidos',
    highlight: 'a dedo para você',
    description:
      'Curadoria, confiança e posicionamento visual forte para acelerar as vendas com identidade própria.',
    image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=1920&q=88',
  },
  {
    id: 'luxo',
    badge: 'Nexum Altivon',
    title: 'Presença digital com',
    highlight: 'força de marca',
    description:
      'Uma vitrine mais elegante, mais viva e preparada para receber clientes, parceiros e operações reais.',
    image: 'https://images.unsplash.com/photo-1546868871-af0c7a6b6f7f?auto=format&fit=crop&w=1920&q=88',
  },
  {
    id: 'mobile',
    badge: 'Operação em evolução',
    title: 'Pronto para',
    highlight: 'escalar o negócio',
    description:
      'Estrutura pensada para integrar e-commerce, dropshipping, logística, gateways e relacionamento com o cliente.',
    image: 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=1920&q=88',
  },
];

const emptyCadastro = {
  nome: '',
  email: '',
  telefone: '',
  cpf: '',
  senha: '',
  newsletter: true,
};

const lojas = [
  {
    nome: 'Gran Tur',
    segmento: 'Viagens & Turismo',
    descricao: 'Mochilas, malas, acessórios de viagem e tudo o que você precisa para explorar o mundo com estilo e conforto.',
    imagem: 'https://images.unsplash.com/photo-1488646953014-85cb44e25828?auto=format&fit=crop&w=900&q=85',
    icon: Plane,
  },
  {
    nome: 'Chronos',
    segmento: 'Relógios & Acessórios',
    descricao: 'Relógios que marcam mais que horas — marcam estilo. Do clássico ao moderno, peças para quem valoriza precisão e elegância.',
    imagem: 'https://images.unsplash.com/photo-1524592094714-0f0654e20314?auto=format&fit=crop&w=900&q=85',
    icon: Watch,
  },
  {
    nome: 'Moda Mim',
    segmento: 'Moda & Vestuário',
    descricao: 'Tendências que vestem a sua personalidade. Roupas, calçados e acessórios para quem não abre mão de estar no topo.',
    imagem: 'https://images.unsplash.com/photo-1445205170230-46b8b7b1d0a6?auto=format&fit=crop&w=900&q=85',
    icon: Shirt,
  },
  {
    nome: 'Geração Top+',
    segmento: 'Tecnologia & Gadgets',
    descricao: 'Tecnologia de ponta ao alcance de todos. Smartphones, gadgets, eletrônicos e inovações que conectam você ao futuro.',
    imagem: 'https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=900&q=85',
    icon: Smartphone,
  },
  {
    nome: 'Estruturaline',
    segmento: 'Construção & Estruturas',
    descricao: 'Ferramentas, materiais de construção e equipamentos profissionais. Solidez para quem constrói com seriedade.',
    imagem: 'https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=900&q=85',
    icon: Hammer,
  },
  {
    nome: 'Gran Festas',
    segmento: 'Festas & Eventos',
    descricao: 'Decorações, utensílios e tudo para tornar sua festa inesquecível, dos pequenos encontros aos grandes eventos.',
    imagem: 'https://images.unsplash.com/photo-1530103862676-de3c9a59aa38?auto=format&fit=crop&w=900&q=85',
    icon: Gift,
  },
];

const qualidade = [
  'Curadoria rigorosa de fornecedores',
  'Atendimento humano e especializado',
  'Política de devolução simplificada',
  'Preços justos e acessíveis',
];

const parceiros = [
  {
    title: 'Parceiros de Vendas',
    text: 'Lojas físicas ou online podem ampliar seus horizontes de venda com nossa infraestrutura comercial e operação integrada.',
    cta: 'Quero Vender',
    href: 'https://wa.me/5514996731879?text=Olá! Tenho interesse em ser parceiro de vendas do Grupo Nexum Altivon.',
    icon: Store,
  },
  {
    title: 'Fornecedores & Distribuidores',
    text: 'Distribuidores e fabricantes encontram um canal de venda em crescimento, com visão de volume, relacionamento e longo prazo.',
    cta: 'Quero Fornecer',
    href: 'https://wa.me/5514996348409?text=Olá! Sou fornecedor/distribuidor e tenho interesse em parceria com o Grupo Nexum Altivon.',
    icon: Truck,
  },
  {
    title: 'Dropshipping',
    text: 'Integre seu catálogo às nossas lojas ou utilize nossa infraestrutura para conectar produtos, logística e novos canais.',
    cta: 'Quero Fazer Dropship',
    href: 'https://wa.me/5514996731879?text=Olá! Tenho interesse em parceria de dropshipping com o Grupo Nexum Altivon.',
    icon: Building2,
  },
];

const partnerIconMap = {
  Store,
  Truck,
  Building2,
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
    secondaryPhone: pickConfigValue(config, ['secondaryPhone', 'SecondaryPhone', 'site_telefone_secundario', 'siteTelefoneSecundario'], '(14) 99634-8409'),
    primaryWhatsapp: pickConfigValue(config, ['primaryWhatsapp', 'PrimaryWhatsapp', 'site_whatsapp', 'siteWhatsapp'], '5514996731879'),
    secondaryWhatsapp: pickConfigValue(config, ['secondaryWhatsapp', 'SecondaryWhatsapp', 'site_whatsapp_secundario', 'siteWhatsappSecundario'], '5514996348409'),
    yaraEmail: pickConfigValue(config, ['yaraEmail', 'YaraEmail', 'site_yara_email', 'siteYaraEmail'], 'corporativo.gna@gmail.com'),
    siteLogo: pickConfigValue(config, ['siteLogo', 'SiteLogo', 'site_logo', 'siteLogoUrl'], '/assets/logo-2.jpg'),
    heroSlides: pickConfigValue(config, ['heroSlides', 'HeroSlides'], []),
    introTitle: pickConfigValue(config, ['introTitle', 'IntroTitle', 'home_intro_titulo', 'homeIntroTitulo'], 'Uma Nova Era Começa'),
    introText1: pickConfigValue(config, ['introText1', 'IntroText1', 'home_intro_texto_1', 'homeIntroTexto1'], 'A Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.'),
    introText2: pickConfigValue(config, ['introText2', 'IntroText2', 'home_intro_texto_2', 'homeIntroTexto2'], 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.'),
    introBadge: pickConfigValue(config, ['introBadge', 'IntroBadge', 'home_intro_badge', 'homeIntroBadge'], 'nexumaltivon.com.br'),
    qualityItems: pickConfigValue(config, ['qualityItems', 'QualityItems', 'home_quality_items', 'homeQualityItems'], []),
    partnerCards: pickConfigValue(config, ['partnerCards', 'PartnerCards', 'home_partner_cards', 'homePartnerCards'], []),
    footerText: pickConfigValue(config, ['footerText', 'FooterText', 'home_footer_texto', 'homeFooterTexto'], 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.'),
  };
};
export default function Home() {
  const navigate = useNavigate();
  const [currentSlide, setCurrentSlide] = useState(0);
  const [siteConfig, setSiteConfig] = useState(null);
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [loadingProducts, setLoadingProducts] = useState(true);
  const [catalogSearch, setCatalogSearch] = useState('');
  const [cadastroForm, setCadastroForm] = useState(emptyCadastro);
  const [cadastroStatus, setCadastroStatus] = useState({ tone: '', message: '' });
  const [loadingCadastro, setLoadingCadastro] = useState(false);

  const displaySlides = Array.isArray(siteConfig?.heroSlides) && siteConfig.heroSlides.length > 0 ? siteConfig.heroSlides : heroSlides;
  const activeSlide = displaySlides[currentSlide] || displaySlides[0] || heroSlides[0];
  const qualityItems = Array.isArray(siteConfig?.qualityItems) && siteConfig.qualityItems.length > 0 ? siteConfig.qualityItems : qualidade;
  const partnerCards = Array.isArray(siteConfig?.partnerCards) && siteConfig.partnerCards.length > 0 ? siteConfig.partnerCards : parceiros;
  const introTitle = siteConfig?.introTitle || 'Uma Nova Era Começa';
  const introText1 =
    siteConfig?.introText1 || 'A Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.';
  const introText2 =
    siteConfig?.introText2 || 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.';
  const introBadge = siteConfig?.introBadge || 'nexumaltivon.com.br';
  const primaryPhone = siteConfig?.primaryPhone || '+55 (14) 99673-1879';
  const secondaryPhone = siteConfig?.secondaryPhone || '+55 (14) 99634-8409';
  const publicContactEmail = siteConfig?.contactEmail || 'corporativo.gna@gmail.com';
  const yaraEmail = siteConfig?.yaraEmail || publicContactEmail;
  const yaraMailTo = `mailto:${encodeURIComponent(yaraEmail)}?subject=Yara%20-%20Atendimento%20de%20vendas&body=Ol%C3%A1%20Yara%2C%20preciso%20de%20ajuda%20com%20assuntos%20da%20empresa%2C%20produtos%20ou%20d%C3%BAvidas%20sobre%20a%20compra.`;

  useEffect(() => {
    const interval = window.setInterval(() => {
      setCurrentSlide((slide) => (slide + 1) % displaySlides.length);
    }, 5000);

    return () => window.clearInterval(interval);
  }, [displaySlides.length]);

  useEffect(() => {
    let active = true;

    siteAPI
      .getPublicConfig()
      .then((response) => {
        if (active && response.data) {
          setSiteConfig(mapPublicSiteConfig(response.data));
        }
      })
      .catch(() => {});

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    let active = true;

    const loadFeaturedProducts = async () => {
      setLoadingProducts(true);

      try {
        const destaquesRes = await produtoAPI.getDestaques();
        let produtos = unwrapApiData(destaquesRes.data);

        if (!Array.isArray(produtos) || produtos.length === 0) {
          const todosRes = await produtoAPI.getAll({});
          produtos = unwrapApiData(todosRes.data);
        }

        if (active) {
          const publicaveis = Array.isArray(produtos)
            ? produtos.filter(isProdutoPublicavel).slice(0, 5)
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
      } catch {
        setCadastroStatus({
          tone: 'warning',
          message: 'Verificação de cadastro indisponível no momento. Vamos seguir com o registro para não travar seu acesso.',
        });
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
        {displaySlides.map((slide, index) => (
          <div
            key={slide.id}
            className={`absolute inset-0 transition-opacity duration-1000 ${index === currentSlide ? 'opacity-100' : 'pointer-events-none opacity-0'}`}
          >
            <img src={slide.image} alt={slide.highlight} className="h-full w-full object-cover" />
            <div className="absolute inset-0 bg-gradient-to-r from-black via-black/55 to-black/75" />
          </div>
        ))}

        <div className="relative mx-auto flex min-h-[84vh] max-w-7xl items-center px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-3xl">
            <p className="mb-5 inline-flex items-center rounded-full border border-[#C9A227]/40 bg-black/40 px-4 py-2 text-xs font-bold uppercase tracking-[0.25em] text-[#E8D5A3]">
              {activeSlide.badge}
            </p>
            <h1 className="font-serif text-5xl font-bold leading-[1.02] text-white sm:text-6xl lg:text-7xl" data-testid="hero-title">
              {activeSlide.title}
              <span className="block text-[#C9A227]">{activeSlide.highlight}</span>
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-zinc-200 sm:text-xl">{activeSlide.description}</p>

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
                href="#cadastro"
                className="inline-flex items-center justify-center gap-2 rounded-full border border-white/35 px-7 py-4 text-sm font-black uppercase tracking-wide text-white transition hover:border-[#C9A227] hover:text-[#C9A227]"
              >
                Fazer meu cadastro
                <UserPlus size={18} />
              </a>
            </div>

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
            </div>
          </div>
        </div>

        <div className="pointer-events-none absolute inset-y-0 left-0 right-0 mx-auto hidden max-w-7xl items-center justify-between px-4 sm:flex sm:px-6 lg:px-8">
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
        </div>
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
                Itens carregados diretamente da API operacional, com estoque, preço e checkout conectados ao fluxo interno de vendas.
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
                A vitrine está preparada para exibir os produtos assim que a API responder. Use o catálogo completo como rota de consulta enquanto isso.
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
              href={yaraMailTo}
              className="mt-6 inline-flex items-center gap-2 rounded-full border border-[#C9A227]/30 bg-[#C9A227]/10 px-4 py-2 text-xs font-black uppercase tracking-[0.18em] text-[#E8D5A3] transition hover:border-[#E8D5A3] hover:text-white"
            >
              <MessageCircleMore size={16} />
              Falar com Yara agora
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

        <div className="grid gap-8 md:grid-cols-2 xl:grid-cols-3">
          {lojas.map((loja) => {
            const Icon = loja.icon;
            return (
              <article
                key={loja.nome}
                className="group overflow-hidden rounded-xl border border-[#2A2A2A] bg-[#1A1A1A] transition duration-300 hover:-translate-y-2 hover:border-[#C9A227] hover:shadow-2xl hover:shadow-[#C9A227]/10"
              >
                <div className="relative h-56 overflow-hidden bg-[#111111]">
                  <div
                    className="absolute inset-0 bg-cover bg-center transition duration-500 group-hover:scale-110"
                    style={{ backgroundImage: `url(${loja.imagem})` }}
                    aria-hidden="true"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-[#1A1A1A] to-transparent" />
                  <div className="absolute left-5 top-5 flex h-12 w-12 items-center justify-center rounded-full bg-black/70 text-[#C9A227]">
                    <Icon size={24} />
                  </div>
                </div>
                <div className="p-6">
                  <h3 className="font-serif text-2xl font-bold text-[#C9A227]">{loja.nome}</h3>
                  <p className="mt-3 min-h-24 text-sm leading-6 text-zinc-400">{loja.descricao}</p>
                  <span className="mt-5 inline-flex rounded-full bg-[#0A0A0A] px-4 py-2 text-xs font-bold uppercase tracking-wide text-[#E8D5A3]">
                    {loja.segmento}
                  </span>
                </div>
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
          <div className="mt-12 grid gap-6 lg:grid-cols-3">
            {partnerCards.map((item) => {
              const Icon = typeof item.icon === 'string' ? partnerIconMap[item.icon] || Building2 : item.icon;
              return (
                <article key={item.title} className="rounded-xl border border-[#2A2A2A] bg-[#1A1A1A] p-8 text-left transition hover:border-[#C9A227]">
                  <div className="flex h-14 w-14 items-center justify-center rounded-full bg-[#C9A227] text-black">
                    <Icon size={26} />
                  </div>
                  <h3 className="mt-6 text-xl font-black text-white">{item.title}</h3>
                  <p className="mt-4 min-h-28 text-sm leading-7 text-zinc-400">{item.text}</p>
                  <a
                    href={item.href}
                    target="_blank"
                    rel="noreferrer"
                    className="mt-6 inline-flex items-center gap-2 rounded-full bg-[#C9A227] px-5 py-3 text-sm font-black text-black transition hover:bg-[#E8D5A3]"
                  >
                    {item.cta}
                    <ArrowRight size={16} />
                  </a>
                </article>
              );
            })}
          </div>
        </div>
      </section>

      <section className="border-t border-white/5 bg-[#050505] px-4 py-12">
        <div className="mx-auto flex max-w-7xl flex-col items-start justify-between gap-4 sm:px-6 md:flex-row md:items-center lg:px-8">
          <div>
            <p className="text-xs font-black uppercase tracking-[0.25em] text-[#E8D5A3]">Grupo Nexum Altivon</p>
            <p className="mt-3 text-sm leading-7 text-zinc-400">{siteConfig?.footerText || 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.'}</p>
            <p className="mt-2 text-xs font-semibold uppercase tracking-[0.14em] text-zinc-500">
              Rodrigo: {primaryPhone} · Vinicios: {secondaryPhone} · {publicContactEmail}
            </p>
          </div>
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
