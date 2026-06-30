/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { Link } from 'react-router-dom';
import { Mail, MapPin, Phone, ShieldCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { siteAPI, unwrapApiData } from '../services/api';

const fallbackLogo = '/imagens/homepage/Logo-2.png';
const resolveLogo = (logo) => {
  const value = String(logo || '').trim();
  return value && !value.includes('logo-grupo-nexum-altivon.svg') ? value : fallbackLogo;
};

const links = [
  { to: '/produtos', label: 'Catálogo' },
  { to: '/lojas', label: 'Lojas' },
  { to: '/contato', label: 'Contato' },
  { to: '/institucional', label: 'Institucional' },
  { to: '/politica-privacidade', label: 'Política de privacidade' },
  { to: '/politica-reembolso', label: 'Política de reembolso' },
];

export default function Footer() {
  const [branding, setBranding] = useState({
    siteName: 'Grupo Nexum Altivon',
    subtitle: 'Participações societárias',
    logo: fallbackLogo,
    email: 'corporativo.gna@gmail.com',
    telefone: '+55 (14) 99673-1879',
    telefoneSecundario: '+55 (14) 99634-8409',
  });

  useEffect(() => {
    let active = true;

    siteAPI
      .getPublicConfig()
      .then((response) => {
        const config = unwrapApiData(response.data) || {};
        if (!active) return;
        setBranding({
          siteName: config.siteNome || config.siteName || 'Grupo Nexum Altivon',
          subtitle: config.siteSubtitulo || config.siteSubtitle || 'Participações societárias',
          logo: resolveLogo(config.siteLogo),
          email: config.contactEmail || 'corporativo.gna@gmail.com',
          telefone: config.primaryPhone || '+55 (14) 99673-1879',
          telefoneSecundario: config.secondaryPhone || '+55 (14) 99634-8409',
        });
      })
      .catch(() => {});

    return () => {
      active = false;
    };
  }, []);

  return (
    <footer className="border-t border-[#2A2A2A] bg-[#0A0A0A] text-white">
      <div className="mx-auto grid max-w-7xl gap-8 px-4 py-10 sm:px-6 md:grid-cols-[1.5fr_1fr_1fr] lg:px-8">
        <div className="space-y-4">
          <Link to="/" className="inline-flex items-center gap-3" aria-label="Grupo Nexum Altivon">
            <img
              src={branding.logo}
              alt="Logotipo Grupo Nexum Altivon"
              className="h-11 w-11 rounded-xl bg-[#C9A227] object-contain p-1"
              onError={(event) => {
                event.currentTarget.src = fallbackLogo;
              }}
            />
            <div className="leading-tight">
              <p className="text-base font-black text-[#C9A227]">{branding.siteName}</p>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-zinc-500">{branding.subtitle}</p>
            </div>
          </Link>
          <p className="max-w-md text-sm leading-6 text-zinc-400">
            Grupo societário com 6 lojas especializadas, comprometido com a transformação do e-commerce brasileiro através de qualidade, inovação e preços acessíveis.
          </p>
        </div>

        <div>
          <p className="mb-3 text-sm font-black uppercase tracking-[0.14em] text-[#C9A227]">Navegação</p>
          <div className="space-y-2">
            {links.map((item) => (
              <Link key={item.to} to={item.to} className="block text-sm font-semibold text-zinc-300 transition hover:text-[#C9A227]">
                {item.label}
              </Link>
            ))}
          </div>
        </div>

        <div>
          <p className="mb-3 text-sm font-black uppercase tracking-[0.14em] text-[#C9A227]">Contato</p>
          <div className="space-y-3 text-sm font-semibold text-zinc-300">
            <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href={`mailto:${branding.email}`}>
              <Mail size={16} />
              {branding.email}
            </a>
            <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href={`tel:${branding.telefone.replace(/\D/g, '')}`}>
              <Phone size={16} />
              Rodrigo: {branding.telefone}
            </a>
            <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href={`tel:${branding.telefoneSecundario.replace(/\D/g, '')}`}>
              <Phone size={16} />
              Vinicius: {branding.telefoneSecundario}
            </a>
            <p className="flex items-center gap-2">
              <MapPin size={16} />
              Brasil
            </p>
            <p className="flex items-center gap-2">
              <ShieldCheck size={16} />
              Compra segura
            </p>
          </div>
        </div>
      </div>
      <div className="border-t border-[#2A2A2A] px-4 py-4 text-center text-xs font-semibold text-zinc-500">
        © 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.
      </div>
    </footer>
  );
}
