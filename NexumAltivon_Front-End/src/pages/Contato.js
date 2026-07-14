/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useState } from 'react';
import { Building, CheckCircle, Mail, MessageSquare, Phone, Send, User } from 'lucide-react';
import { leadAPI } from '../services/api';

const canais = [
  { label: 'Rodrigo', value: '+55 (14) 99673-1879', href: 'tel:+5514996731879', icon: Phone },
  { label: 'E-mail comercial', value: 'corporativo.gna@gmail.com', href: 'mailto:corporativo.gna@gmail.com', icon: Mail },
  { label: 'Atendimento', value: 'Segunda a sexta, 9h às 18h', href: null, icon: MessageSquare },
];

export default function Contato() {
  const [formData, setFormData] = useState({
    nome: '',
    email: '',
    telefone: '',
    empresa: '',
    mensagem: '',
  });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (event) => {
    setFormData((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError('');

    try {
      await leadAPI.create({
        ...formData,
        origem: 'Website - Formulário de Contato',
      });
      setSuccess(true);
      setFormData({ nome: '', email: '', telefone: '', empresa: '', mensagem: '' });
    } catch (err) {
      setError(err.response?.data?.detail || 'Erro ao enviar mensagem');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#2B2108_0,#050505_36%,#050505_100%)] px-4 py-14 text-white sm:px-6 lg:px-8">
        <section className="mx-auto max-w-xl rounded-[34px] border border-[#C9A227]/25 bg-[#111111] p-8 text-center shadow-2xl shadow-black/50" data-testid="success-message">
          <CheckCircle className="mx-auto text-[#C9A227]" size={64} />
          <p className="mt-6 text-xs font-black uppercase tracking-[0.26em] text-[#E8D5A3]">Contato registrado</p>
          <h1 className="mt-3 text-3xl font-black">Mensagem enviada.</h1>
          <p className="mt-4 text-sm leading-6 text-zinc-300">Recebemos sua solicitação e a equipe comercial dará sequência pelo canal informado.</p>
          <button
            type="button"
            onClick={() => setSuccess(false)}
            className="mt-8 inline-flex h-12 items-center justify-center rounded-full bg-[#C9A227] px-6 text-sm font-black text-black transition hover:bg-[#E8D5A3]"
          >
            Enviar nova mensagem
          </button>
        </section>
      </main>
    );
  }

  return (
    <main className="min-h-screen overflow-x-hidden bg-[radial-gradient(circle_at_top_left,#2B2108_0,#050505_36%,#050505_100%)] px-4 py-12 text-white sm:px-6 lg:px-8">
      <div className="mx-auto grid max-w-7xl gap-8 lg:grid-cols-[0.9fr_1.1fr]">
        <section className="rounded-[36px] border border-[#C9A227]/20 bg-[#111111] p-6 shadow-2xl shadow-black/50 sm:p-8">
          <p className="text-xs font-black uppercase tracking-[0.28em] text-[#E8D5A3]">Grupo Nexum Altivon</p>
          <h1 className="mt-4 text-4xl font-black leading-tight sm:text-5xl" data-testid="contato-title">
            Fale com a operação comercial.
          </h1>
          <p className="mt-5 max-w-2xl text-base leading-7 text-zinc-300">
            Canal direto para clientes, fornecedores, parceiros e assuntos institucionais. As mensagens alimentam o CRM do GenesisGest.Net.
          </p>

          <div className="mt-8 grid gap-3">
            {canais.map((canal) => {
              const Icon = canal.icon;
              const content = (
                <div className="flex min-w-0 items-center gap-4 rounded-3xl border border-white/10 bg-black/25 p-4 transition hover:border-[#C9A227]/60">
                  <span className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-[#C9A227] text-black">
                    <Icon size={19} />
                  </span>
                  <span className="min-w-0">
                    <span className="block text-xs font-black uppercase tracking-[0.18em] text-zinc-500">{canal.label}</span>
                    <span className="mt-1 block break-words text-sm font-bold text-zinc-100">{canal.value}</span>
                  </span>
                </div>
              );

              return canal.href ? (
                <a key={canal.label} href={canal.href}>
                  {content}
                </a>
              ) : (
                <div key={canal.label}>{content}</div>
              );
            })}
          </div>
        </section>

        <section className="rounded-[36px] border border-white/10 bg-[#111111] p-6 shadow-2xl shadow-black/50 sm:p-8">
          <div className="mb-6">
            <p className="text-xs font-black uppercase tracking-[0.24em] text-[#E8D5A3]">Contato oficial</p>
            <h2 className="mt-3 text-3xl font-black">Envie sua mensagem</h2>
          </div>

          {error && (
            <div className="mb-5 rounded-3xl border border-red-400/30 bg-red-500/10 px-4 py-3 text-sm font-bold text-red-100">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="grid gap-4">
            <FormInput icon={User} name="nome" value={formData.nome} onChange={handleChange} placeholder="Seu nome *" required testId="contato-nome" />
            <FormInput icon={Mail} type="email" name="email" value={formData.email} onChange={handleChange} placeholder="E-mail *" required testId="contato-email" />
            <FormInput icon={Phone} type="tel" name="telefone" value={formData.telefone} onChange={handleChange} placeholder="Telefone / WhatsApp" testId="contato-telefone" />
            <FormInput icon={Building} name="empresa" value={formData.empresa} onChange={handleChange} placeholder="Empresa" testId="contato-empresa" />

            <label className="relative block">
              <MessageSquare className="pointer-events-none absolute left-4 top-4 text-zinc-500" size={19} />
              <textarea
                name="mensagem"
                value={formData.mensagem}
                onChange={handleChange}
                required
                rows="5"
                placeholder="Sua mensagem *"
                className="w-full rounded-3xl border border-white/10 bg-black/30 px-12 py-4 text-sm font-semibold text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10"
                data-testid="contato-mensagem"
              />
            </label>

            <button
              type="submit"
              disabled={loading}
              className="inline-flex h-[52px] items-center justify-center gap-2 rounded-full bg-[#C9A227] px-6 py-4 text-sm font-black text-black transition hover:bg-[#E8D5A3] disabled:cursor-not-allowed disabled:opacity-50"
              data-testid="contato-submit"
            >
              <Send size={17} />
              {loading ? 'Enviando...' : 'Enviar mensagem'}
            </button>
          </form>
        </section>
      </div>
    </main>
  );
}

function FormInput({ icon: Icon, testId, ...props }) {
  return (
    <label className="relative block">
      <Icon className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-zinc-500" size={19} />
      <input
        {...props}
        className="h-[52px] w-full rounded-full border border-white/10 bg-black/30 px-12 py-4 text-sm font-semibold text-white outline-none placeholder:text-zinc-600 focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10"
        data-testid={testId}
      />
    </label>
  );
}
