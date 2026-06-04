import { Link } from 'react-router-dom';
import { Mail, MapPin, Phone, ShieldCheck } from 'lucide-react';

const links = [
  { to: '/produtos', label: 'Catálogo' },
  { to: '/lojas', label: 'Lojas' },
  { to: '/contato', label: 'Contato' },
];

export default function Footer() {
  return (
    <footer className="border-t border-[#2A2A2A] bg-[#0A0A0A] text-white">
      <div className="mx-auto grid max-w-7xl gap-8 px-4 py-10 sm:px-6 md:grid-cols-[1.5fr_1fr_1fr] lg:px-8">
        <div className="space-y-4">
          <Link to="/" className="inline-flex items-center gap-3" aria-label="Nexum Altivon">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-[#C9A227] text-xs font-black tracking-wide text-black">
              NA
            </div>
            <div className="leading-tight">
              <p className="text-base font-black text-[#C9A227]">Nexum Altivon</p>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-zinc-500">Grupo Commerce</p>
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
            <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href="mailto:corporativo.gna@gmail.com">
              <Mail size={16} />
              corporativo.gna@gmail.com
            </a>
            <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href="tel:+5514996731879">
              <Phone size={16} />
              Rodrigo: +55 (14) 99673-1879
            </a>
            <a className="flex items-center gap-2 transition hover:text-[#C9A227]" href="tel:+5514996348409">
              <Phone size={16} />
              Vinicius: +55 (14) 99634-8409
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
