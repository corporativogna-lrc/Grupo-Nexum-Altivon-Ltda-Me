/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useEffect, useMemo, useState } from 'react';
import { CheckCircle2, Pencil, Save, TicketPercent, Trash2, X } from 'lucide-react';
import { cupomAPI } from '../services/api';

const emptyForm = {
  id: null,
  codigo: '',
  tipo: 'Percentual',
  valor: '',
  valorMinimoPedido: '0',
  valorMaximoDesconto: '',
  quantidadeUsos: '',
  quantidadePorCliente: '1',
  validoDe: '',
  validoAte: '',
  primeiroCompraOnly: false,
  ativo: true,
};

const getErrorMessage = (error) =>
  error?.response?.data?.mensagem || error?.response?.data?.detail || error?.message || 'Falha ao operar o cadastro de cupons.';

const toDateInput = (value) => (value ? String(value).slice(0, 10) : '');

const toForm = (cupom) => ({
  id: cupom.id,
  codigo: cupom.codigo || '',
  tipo: cupom.tipo || 'Percentual',
  valor: String(cupom.valor ?? ''),
  valorMinimoPedido: String(cupom.valorMinimoPedido ?? 0),
  valorMaximoDesconto: cupom.valorMaximoDesconto == null ? '' : String(cupom.valorMaximoDesconto),
  quantidadeUsos: cupom.quantidadeUsos == null ? '' : String(cupom.quantidadeUsos),
  quantidadePorCliente: String(cupom.quantidadePorCliente ?? 1),
  validoDe: toDateInput(cupom.validoDe),
  validoAte: toDateInput(cupom.validoAte),
  primeiroCompraOnly: Boolean(cupom.primeiroCompraOnly),
  ativo: Boolean(cupom.ativo),
});

const formatDate = (value) => value ? new Intl.DateTimeFormat('pt-BR').format(new Date(value)) : 'Sem limite';

export default function CupomAdminPanel() {
  const [cupons, setCupons] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const ativos = useMemo(() => cupons.filter((cupom) => cupom.ativo).length, [cupons]);
  const usos = useMemo(() => cupons.reduce((total, cupom) => total + Number(cupom.usosAtuais || 0), 0), [cupons]);

  const loadCupons = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await cupomAPI.listarAdministracao();
      setCupons(Array.isArray(response.data) ? response.data : []);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCupons();
  }, []);

  const updateField = (field, value) => setForm((current) => ({ ...current, [field]: value }));

  const resetForm = () => {
    setForm(emptyForm);
    setMessage('');
    setError('');
  };

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setMessage('');
    setError('');

    const payload = {
      codigo: form.codigo.trim().toUpperCase(),
      tipo: form.tipo,
      valor: form.tipo === 'FreteGratis' ? 0 : Number(form.valor),
      valorMinimoPedido: Number(form.valorMinimoPedido || 0),
      valorMaximoDesconto: form.valorMaximoDesconto === '' ? null : Number(form.valorMaximoDesconto),
      quantidadeUsos: form.quantidadeUsos === '' ? null : Number(form.quantidadeUsos),
      quantidadePorCliente: Number(form.quantidadePorCliente || 0),
      validoDe: form.validoDe ? `${form.validoDe}T00:00:00Z` : null,
      validoAte: form.validoAte ? `${form.validoAte}T23:59:59Z` : null,
      primeiroCompraOnly: form.primeiroCompraOnly,
      ativo: form.ativo,
    };

    try {
      const response = form.id
        ? await cupomAPI.atualizar(form.id, payload)
        : await cupomAPI.criar(payload);
      const persisted = response.data;
      if (!persisted?.id || persisted.codigo !== payload.codigo) {
        throw new Error('A API nao confirmou o registro persistido do cupom.');
      }

      setCupons((current) => {
        const exists = current.some((cupom) => cupom.id === persisted.id);
        return exists
          ? current.map((cupom) => cupom.id === persisted.id ? persisted : cupom)
          : [persisted, ...current];
      });
      setForm(toForm(persisted));
      setMessage(`Cupom ${persisted.codigo} confirmado no banco com ID ${persisted.id}.`);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  };

  const deactivate = async (cupom) => {
    if (!window.confirm(`Desativar o cupom ${cupom.codigo}?`)) return;
    setMessage('');
    setError('');
    try {
      const response = await cupomAPI.desativar(cupom.id);
      const persisted = response.data;
      if (!persisted?.id || persisted.ativo !== false) {
        throw new Error('A API nao confirmou a desativacao do cupom.');
      }
      setCupons((current) => current.map((item) => item.id === persisted.id ? persisted : item));
      if (form.id === persisted.id) setForm(toForm(persisted));
      setMessage(`Cupom ${persisted.codigo} desativado no banco.`);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  return (
    <section className="space-y-6">
      <div className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6 shadow-sm lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.18em] text-amber-700">Comercial e marketing</p>
          <h2 className="mt-2 text-2xl font-black text-slate-950">Cupons persistidos</h2>
          <p className="mt-1 text-sm text-slate-500">Cadastro, vigência, limites de uso e desativação aplicados pela API oficial.</p>
        </div>
        <div className="grid grid-cols-3 gap-3 text-center">
          <Metric label="Cadastrados" value={cupons.length} />
          <Metric label="Ativos" value={ativos} />
          <Metric label="Usos" value={usos} />
        </div>
      </div>

      {(error || message) && (
        <div className={`rounded-lg border px-5 py-4 text-sm font-bold ${error ? 'border-red-200 bg-red-50 text-red-800' : 'border-emerald-200 bg-emerald-50 text-emerald-800'}`}>
          {error || message}
        </div>
      )}

      <div className="grid gap-6 xl:grid-cols-[minmax(360px,0.8fr)_minmax(0,1.2fr)]">
        <form onSubmit={submit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between gap-4 border-b border-slate-100 pb-4">
            <div>
              <p className="text-xs font-black uppercase tracking-[0.16em] text-slate-500">{form.id ? `Registro ${form.id}` : 'Novo registro'}</p>
              <h3 className="mt-1 text-lg font-black text-slate-950">Configuração do cupom</h3>
            </div>
            {form.id && <button type="button" onClick={resetForm} title="Novo cupom" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200 text-slate-600"><X size={18} /></button>}
          </div>

          <div className="mt-5 grid gap-4 sm:grid-cols-2">
            <Input label="Código" value={form.codigo} onChange={(value) => updateField('codigo', value.toUpperCase())} required />
            <Select label="Tipo" value={form.tipo} onChange={(value) => updateField('tipo', value)} options={['Percentual', 'ValorFixo', 'FreteGratis']} />
            <Input label={form.tipo === 'Percentual' ? 'Percentual' : 'Valor'} type="number" step="0.01" min="0" value={form.tipo === 'FreteGratis' ? '0' : form.valor} onChange={(value) => updateField('valor', value)} disabled={form.tipo === 'FreteGratis'} required />
            <Input label="Pedido mínimo" type="number" step="0.01" min="0" value={form.valorMinimoPedido} onChange={(value) => updateField('valorMinimoPedido', value)} required />
            <Input label="Desconto máximo" type="number" step="0.01" min="0.01" value={form.valorMaximoDesconto} onChange={(value) => updateField('valorMaximoDesconto', value)} />
            <Input label="Total de usos" type="number" min="1" value={form.quantidadeUsos} onChange={(value) => updateField('quantidadeUsos', value)} />
            <Input label="Usos por cliente" type="number" min="0" value={form.quantidadePorCliente} onChange={(value) => updateField('quantidadePorCliente', value)} required />
            <Input label="Válido de" type="date" value={form.validoDe} onChange={(value) => updateField('validoDe', value)} />
            <Input label="Válido até" type="date" value={form.validoAte} onChange={(value) => updateField('validoAte', value)} />
          </div>

          <div className="mt-5 grid gap-3 sm:grid-cols-2">
            <Check label="Somente primeira compra" checked={form.primeiroCompraOnly} onChange={(checked) => updateField('primeiroCompraOnly', checked)} />
            <Check label="Ativo para uso" checked={form.ativo} onChange={(checked) => updateField('ativo', checked)} />
          </div>

          <button disabled={saving} className="mt-6 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:opacity-60">
            <Save size={17} />
            {saving ? 'Gravando...' : form.id ? 'Atualizar cupom' : 'Cadastrar cupom'}
          </button>
        </form>

        <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-5">
            <h3 className="text-lg font-black text-slate-950">Registros do banco</h3>
            <p className="mt-1 text-sm text-slate-500">A lista é carregada de <code>/api/admin/cupons</code>.</p>
          </div>
          {loading ? (
            <p className="p-6 text-sm font-semibold text-slate-500">Consultando cupons...</p>
          ) : cupons.length === 0 ? (
            <p className="p-6 text-sm font-semibold text-slate-500">Nenhum cupom cadastrado no banco oficial.</p>
          ) : (
            <div className="divide-y divide-slate-100">
              {cupons.map((cupom) => (
                <article key={cupom.id} className="flex flex-col gap-4 p-5 lg:flex-row lg:items-center lg:justify-between">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <TicketPercent size={18} className="text-amber-700" />
                      <span className="font-mono text-base font-black text-slate-950">{cupom.codigo}</span>
                      <span className={`rounded-full px-2.5 py-1 text-xs font-black ${cupom.ativo ? 'bg-emerald-50 text-emerald-800' : 'bg-slate-100 text-slate-600'}`}>{cupom.ativo ? 'Ativo' : 'Inativo'}</span>
                    </div>
                    <p className="mt-2 text-sm font-semibold text-slate-600">
                      {cupom.tipo} · valor {cupom.valor} · usos {cupom.usosAtuais}/{cupom.quantidadeUsos ?? 'sem limite'}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">Vigência: {formatDate(cupom.validoDe)} até {formatDate(cupom.validoAte)}</p>
                  </div>
                  <div className="flex gap-2">
                    <button type="button" onClick={() => { setForm(toForm(cupom)); setMessage(''); setError(''); }} title="Editar cupom" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200 text-slate-700"><Pencil size={17} /></button>
                    <button type="button" onClick={() => deactivate(cupom)} disabled={!cupom.ativo} title="Desativar cupom" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-red-200 text-red-700 disabled:cursor-not-allowed disabled:opacity-35"><Trash2 size={17} /></button>
                  </div>
                </article>
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  );
}

function Metric({ label, value }) {
  return <div className="min-w-20 rounded-lg bg-slate-100 px-3 py-2"><p className="text-xs font-bold text-slate-500">{label}</p><p className="text-lg font-black text-slate-950">{value}</p></div>;
}

function Input({ label, onChange, ...props }) {
  return <label className="block text-sm font-bold text-slate-700">{label}<input {...props} onChange={(event) => onChange(event.target.value)} className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10 disabled:bg-slate-100" /></label>;
}

function Select({ label, value, onChange, options }) {
  return <label className="block text-sm font-bold text-slate-700">{label}<select value={value} onChange={(event) => onChange(event.target.value)} className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10">{options.map((option) => <option key={option} value={option}>{option}</option>)}</select></label>;
}

function Check({ label, checked, onChange }) {
  return <label className="flex items-center gap-3 rounded-lg border border-slate-200 px-3 py-3 text-sm font-bold text-slate-700"><input type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} className="h-5 w-5 accent-slate-950" />{checked ? <CheckCircle2 size={18} className="text-emerald-600" /> : null}{label}</label>;
}
