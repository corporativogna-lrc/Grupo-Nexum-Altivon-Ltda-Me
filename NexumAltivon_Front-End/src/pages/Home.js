import { Link } from 'react-router-dom';
import {
  ArrowRight,
  BadgeCheck,
  Building2,
  Check,
  Gift,
  Hammer,
  Headphones,
  Plane,
  ShieldCheck,
  Shirt,
  Smartphone,
  Store,
  Truck,
  Watch,
} from 'lucide-react';

const lojas = [
  {
    nome: 'Grann-Tur',
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
    nome: 'Gran-fest-festas',
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

export default function Home() {
  return (
    <main className="nexum-front-original bg-[#050505] text-[#f5f5f5]">
      <section id="home" className="relative min-h-[78vh] overflow-hidden">
        <div className="absolute inset-0">
          <img
            src="https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=1920&q=88"
            alt="Grupo Nexum Altivon"
            className="h-full w-full object-cover opacity-55"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-black via-black/55 to-black/70" />
        </div>

        <div className="relative mx-auto flex min-h-[78vh] max-w-7xl items-center px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-3xl">
            <p className="mb-5 inline-flex items-center rounded-full border border-[#C9A227]/40 bg-black/40 px-4 py-2 text-xs font-bold uppercase tracking-[0.25em] text-[#E8D5A3]">
              Grupo Nexum Altivon
            </p>
            <h1 className="font-serif text-5xl font-bold leading-[1.02] text-white sm:text-6xl lg:text-7xl" data-testid="hero-title">
              O Futuro do
              <span className="block text-[#C9A227]">E-Commerce</span>
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-zinc-200 sm:text-xl">
              Seis lojas, uma só essência: transformar a experiência de compra online com qualidade premium,
              atendimento diferenciado e preços que cabem no seu bolso.
            </p>
            <div className="mt-9 flex flex-col gap-3 sm:flex-row">
              <a
                href="#lojas"
                className="inline-flex items-center justify-center gap-2 rounded-full bg-[#C9A227] px-7 py-4 text-sm font-black uppercase tracking-wide text-black shadow-lg shadow-[#C9A227]/20 transition hover:bg-[#E8D5A3]"
                data-testid="hero-cta"
              >
                Conheça Nossas Lojas
                <ArrowRight size={18} />
              </a>
              <Link
                to="/produtos"
                className="inline-flex items-center justify-center rounded-full border border-white/35 px-7 py-4 text-sm font-black uppercase tracking-wide text-white transition hover:border-[#C9A227] hover:text-[#C9A227]"
              >
                Ver catálogo
              </Link>
            </div>
          </div>
        </div>
      </section>

      <section className="relative overflow-hidden bg-gradient-to-br from-[#0A0A0A] to-[#1A1A1A] px-4 py-20 text-center">
        <div className="absolute left-1/2 top-0 h-96 w-96 -translate-x-1/2 rounded-full bg-[#C9A227]/5 blur-3xl" />
        <div className="relative mx-auto max-w-4xl">
          <h2 className="font-serif text-4xl font-bold text-[#C9A227]">Uma Nova Era Começa</h2>
          <p className="mt-7 text-lg leading-8 text-zinc-100">
            A <strong className="text-[#C9A227]">Nexum Altivon</strong> está chegando para transformar e inovar o mercado digital brasileiro.
          </p>
          <p className="mt-4 text-lg leading-8 text-zinc-100">
            Nosso compromisso é claro: entregar <strong className="text-[#C9A227]">qualidade superior</strong>, atendimento que faz a diferença e
            <strong className="text-[#C9A227]"> preços acessíveis</strong> que respeitam o seu bolso.
          </p>
          <p className="mt-4 text-zinc-400">Seis marcas. Uma visão. Milhares de produtos escolhidos a dedo para você.</p>
          <div className="mx-auto mt-8 inline-flex rounded-full border border-[#C9A227]/35 bg-[#C9A227]/10 px-6 py-3 text-sm font-black uppercase tracking-[0.18em] text-[#E8D5A3]">
            www.nexumaltivon.com
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
                <div className="relative h-56 overflow-hidden">
                  <img src={loja.imagem} alt={loja.nome} className="h-full w-full object-cover transition duration-500 group-hover:scale-110" />
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
              {qualidade.map((item) => (
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
            {parceiros.map((item) => {
              const Icon = item.icon;
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
    </main>
  );
}
