/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useMemo, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { Bot, LoaderCircle, MessageCircleMore, Send, X } from 'lucide-react';
import { assistenteAPI } from '../services/api';

const assistantProfiles = Object.freeze({
  yara: {
    label: 'Yara',
    scope: 'Vendas',
    greeting: 'Sou a Yara. Posso ajudar com produtos, pedidos, compras, parcerias e atendimento comercial do Grupo Nexum Altivon.',
  },
  sophia: {
    label: 'Sophia',
    scope: 'Operação',
    greeting: 'Sou a Sophia. Posso ajudar com rotinas internas, ERP, PDV, financeiro, fiscal, estoque, logística e operação do GenesisGest.Net.',
  },
});

const getSessionId = () => {
  const key = 'nexum_ai_assistant_session';
  const current = localStorage.getItem(key);
  if (current) return current;

  const next = `site-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
  localStorage.setItem(key, next);
  return next;
};

const initialMessages = (assistant) => [
  {
    id: `${assistant}-welcome`,
    author: 'assistente',
    assistant,
    text: assistantProfiles[assistant].greeting,
  },
];

export default function GlobalActions() {
  const location = useLocation();
  const assistant = location.pathname.startsWith('/dashboard') ? 'sophia' : 'yara';
  const [open, setOpen] = useState(false);
  const [messagesByAssistant, setMessagesByAssistant] = useState(() => ({
    yara: initialMessages('yara'),
    sophia: initialMessages('sophia'),
  }));
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const messages = messagesByAssistant[assistant];
  const activeProfile = assistantProfiles[assistant];
  const historico = useMemo(
    () =>
      messages
        .filter((item) => item.author === 'usuario' || item.author === 'assistente')
        .slice(-8)
        .map((item) => ({ autor: item.author, texto: item.text })),
    [messages]
  );

  const sendMessage = async (event) => {
    event.preventDefault();
    const message = text.trim();
    if (!message || sending) return;

    const userMessage = {
      id: `user-${Date.now()}`,
      author: 'usuario',
      assistant,
      text: message,
    };
    setMessagesByAssistant((current) => ({
      ...current,
      [assistant]: [...current[assistant], userMessage],
    }));
    setText('');
    setSending(true);

    try {
      const send = assistant === 'sophia'
        ? assistenteAPI.enviarMensagemSophia
        : assistenteAPI.enviarMensagemYara;
      const response = await send({
        mensagem: message,
        sessaoId: getSessionId(),
        historico,
      });
      const answer = String(response.data?.mensagem || response.data?.Mensagem || response.data?.dados?.mensagem || response.data?.Dados?.Mensagem || '').trim();

      if (!answer) {
        throw new Error('A API da central de IA não retornou uma resposta válida.');
      }

      setMessagesByAssistant((current) => ({
        ...current,
        [assistant]: [
          ...current[assistant],
          {
            id: `assistant-${Date.now()}`,
            author: 'assistente',
            assistant,
            text: answer,
          },
        ],
      }));
    } catch (error) {
      const detail = String(
        error?.response?.data?.detail
        || error?.response?.data?.mensagem
        || error?.response?.data?.message
        || 'Não foi possível concluir o atendimento pela central de IA.'
      ).trim();
      setMessagesByAssistant((current) => ({
        ...current,
        [assistant]: [
          ...current[assistant],
          {
            id: `assistant-error-${Date.now()}`,
            author: 'assistente',
            assistant,
            text: detail,
          },
        ],
      }));
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="fixed bottom-4 right-4 z-50 sm:bottom-5 sm:right-5">
      {open && (
        <section className="mb-3 w-[calc(100vw-2rem)] max-w-[390px] overflow-hidden rounded-2xl border border-[#C9A227]/30 bg-[#080808]/97 text-white shadow-2xl shadow-black/50 backdrop-blur">
          <header className="flex items-center justify-between border-b border-white/10 px-4 py-3">
            <div className="flex items-center gap-3">
              <span className="flex h-9 w-9 items-center justify-center rounded-full bg-[#C9A227] text-black">
                <Bot size={18} />
              </span>
              <div>
                <p className="text-sm font-black uppercase tracking-[0.16em] text-[#E8D5A3]">{activeProfile.label}</p>
                <p className="text-xs font-semibold text-zinc-400">{activeProfile.scope}</p>
              </div>
            </div>
            <button
              type="button"
              onClick={() => setOpen(false)}
              className="inline-flex h-9 w-9 items-center justify-center rounded-full border border-white/10 text-zinc-300 transition hover:border-[#C9A227] hover:text-white"
              aria-label="Fechar atendimento"
            >
              <X size={16} />
            </button>
          </header>

          <div className="max-h-[360px] space-y-3 overflow-y-auto px-4 py-4">
            {messages.map((item) => (
              <div key={item.id} className={`flex ${item.author === 'usuario' ? 'justify-end' : 'justify-start'}`}>
                <p
                  className={`max-w-[84%] rounded-2xl px-4 py-3 text-sm leading-6 ${
                    item.author === 'usuario'
                      ? 'bg-[#C9A227] text-black'
                      : 'border border-white/10 bg-white/[0.04] text-zinc-100'
                  }`}
                >
                  {item.text}
                </p>
              </div>
            ))}
            {sending && (
              <div className="flex justify-start">
                <p className="inline-flex items-center gap-2 rounded-2xl border border-white/10 bg-white/[0.04] px-4 py-3 text-sm text-zinc-200">
                  <LoaderCircle size={15} className="animate-spin" />
                  Processando
                </p>
              </div>
            )}
          </div>

          <form onSubmit={sendMessage} className="flex gap-2 border-t border-white/10 p-3">
            <input
              value={text}
              onChange={(event) => setText(event.target.value)}
              maxLength={1200}
              aria-label={`Mensagem para ${activeProfile.label}`}
              className="min-w-0 flex-1 rounded-xl border border-white/10 bg-black px-3 py-3 text-sm text-white outline-none focus:border-[#C9A227]"
            />
            <button
              type="submit"
              disabled={sending || !text.trim()}
              className="inline-flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-[#C9A227] text-black transition hover:bg-[#E8D5A3] disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Enviar mensagem"
            >
              <Send size={17} />
            </button>
          </form>
        </section>
      )}

      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        className="inline-flex h-14 w-14 items-center justify-center rounded-full border border-[#C9A227]/40 bg-[#111111]/95 text-[#E8D5A3] shadow-2xl shadow-black/40 backdrop-blur transition hover:border-[#E8D5A3] hover:text-white"
        aria-label="Abrir atendimento por IA"
        title="Atendimento por IA"
      >
        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-[#C9A227] text-black">
          <MessageCircleMore size={18} />
        </span>
      </button>
    </div>
  );
}
