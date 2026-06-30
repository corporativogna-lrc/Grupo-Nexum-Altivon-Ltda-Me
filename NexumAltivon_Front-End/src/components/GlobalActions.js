/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useLocation } from 'react-router-dom';
import { MessageCircleMore } from 'lucide-react';

const supportConfigs = {
  public: {
    href: 'mailto:corporativo.gna@gmail.com?subject=Yara%20-%20Atendimento%20de%20vendas&body=Ol%C3%A1%20Yara%2C%20preciso%20de%20ajuda%20com%20assuntos%20da%20empresa%2C%20produtos%20ou%20d%C3%BAvidas%20sobre%20a%20compra.',
    title: 'Yara online',
  },
  backoffice: {
    href: 'mailto:corporativo.gna@gmail.com?subject=Sophia%20-%20Apoio%20ERP&body=Ol%C3%A1%20Sophia%2C%20preciso%20de%20apoio%20interno%20na%20opera%C3%A7%C3%A3o%20do%20GenesisGest.Net.',
    title: 'Sophia online',
  },
};

export default function GlobalActions() {
  const location = useLocation();
  const backoffice = location.pathname.startsWith('/dashboard');
  const support = backoffice ? supportConfigs.backoffice : supportConfigs.public;

  return (
    <a
      href={support.href}
      className="fixed bottom-4 right-4 z-50 inline-flex h-14 w-14 items-center justify-center rounded-full border border-[#C9A227]/40 bg-[#111111]/95 text-[#E8D5A3] shadow-2xl shadow-black/40 backdrop-blur transition hover:border-[#E8D5A3] hover:text-white sm:bottom-5 sm:right-5"
      aria-label={support.title}
      title={support.title}
    >
      <span className="flex h-10 w-10 items-center justify-center rounded-full bg-[#C9A227] text-black">
        <MessageCircleMore size={18} />
      </span>
    </a>
  );
}
