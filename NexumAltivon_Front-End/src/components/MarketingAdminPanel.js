/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useEffect, useMemo, useState } from 'react';
import { BarChart3, Layers3, Pencil, Plus, Save, Trash2, X } from 'lucide-react';
import { marketingAPI } from '../services/api';

const campaignTypes = ['Email', 'Sms', 'WhatsApp', 'MidiaPaga', 'Organica', 'Evento', 'Promocao'];
const campaignStatuses = ['Rascunho', 'Agendada', 'EmAndamento', 'Pausada', 'Concluida', 'Cancelada'];
const allowedTransitions = {
  Rascunho: ['Rascunho', 'Agendada', 'EmAndamento', 'Cancelada'],
  Agendada: ['Agendada', 'EmAndamento', 'Pausada', 'Cancelada'],
  EmAndamento: ['EmAndamento', 'Pausada', 'Concluida', 'Cancelada'],
  Pausada: ['Pausada', 'EmAndamento', 'Concluida', 'Cancelada'],
  Concluida: ['Concluida'],
  Cancelada: ['Cancelada'],
};

const emptySegment = {
  id: null, nome: '', descricao: '', cor: '#C88A00', prioridade: '0',
  ticketMedioMinimo: '', ticketMedioMaximo: '', frequenciaMinimaDias: '',
  frequenciaMaximaDias: '', ativo: true, rowVersion: null,
};

const emptyCampaign = {
  id: null, nome: '', descricao: '', tipo: 'Email', status: 'Rascunho', segmentoId: '',
  dataInicio: new Date().toISOString().slice(0, 10), dataFim: '', orcamento: '0', custoAtual: '0',
  alcance: '0', cliques: '0', leadsGerados: '0', oportunidadesGeradas: '0', vendasGeradas: '0',
  receitaGerada: '0', publicoAlvo: '', conteudo: '', rowVersion: null,
};

const errorMessage = (error) =>
  error?.response?.data?.mensagem || error?.response?.data?.detail || error?.message || 'Falha na operação de marketing.';

const optionalNumber = (value) => value === '' ? null : Number(value);
const dateValue = (value) => value ? String(value).slice(0, 10) : '';

const toSegmentForm = (item) => ({
  id: item.id,
  nome: item.nome || '',
  descricao: item.descricao || '',
  cor: item.cor || '#C88A00',
  prioridade: String(item.prioridade ?? 0),
  ticketMedioMinimo: item.ticketMedioMinimo == null ? '' : String(item.ticketMedioMinimo),
  ticketMedioMaximo: item.ticketMedioMaximo == null ? '' : String(item.ticketMedioMaximo),
  frequenciaMinimaDias: item.frequenciaMinimaDias == null ? '' : String(item.frequenciaMinimaDias),
  frequenciaMaximaDias: item.frequenciaMaximaDias == null ? '' : String(item.frequenciaMaximaDias),
  ativo: Boolean(item.ativo),
  rowVersion: item.rowVersion,
});

const toCampaignForm = (item) => ({
  id: item.id,
  nome: item.nome || '',
  descricao: item.descricao || '',
  tipo: item.tipo || 'Email',
  status: item.status || 'Rascunho',
  segmentoId: item.segmentoId || '',
  dataInicio: dateValue(item.dataInicio),
  dataFim: dateValue(item.dataFim),
  orcamento: String(item.orcamento ?? 0),
  custoAtual: String(item.custoAtual ?? 0),
  alcance: String(item.alcance ?? 0),
  cliques: String(item.cliques ?? 0),
  leadsGerados: String(item.leadsGerados ?? 0),
  oportunidadesGeradas: String(item.oportunidadesGeradas ?? 0),
  vendasGeradas: String(item.vendasGeradas ?? 0),
  receitaGerada: String(item.receitaGerada ?? 0),
  publicoAlvo: item.publicoAlvo || '',
  conteudo: item.conteudo || '',
  rowVersion: item.rowVersion,
});

export default function MarketingAdminPanel() {
  const [mode, setMode] = useState('campanhas');
  const [campaigns, setCampaigns] = useState([]);
  const [segments, setSegments] = useState([]);
  const [campaignForm, setCampaignForm] = useState(emptyCampaign);
  const [segmentForm, setSegmentForm] = useState(emptySegment);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const activeCampaigns = useMemo(() => campaigns.filter((item) => item.status === 'EmAndamento').length, [campaigns]);
  const generatedRevenue = useMemo(() => campaigns.reduce((sum, item) => sum + Number(item.receitaGerada || 0), 0), [campaigns]);

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      const [campaignResponse, segmentResponse] = await Promise.all([
        marketingAPI.listarCampanhas(),
        marketingAPI.listarSegmentos(),
      ]);
      setCampaigns(Array.isArray(campaignResponse.data) ? campaignResponse.data : []);
      setSegments(Array.isArray(segmentResponse.data) ? segmentResponse.data : []);
    } catch (requestError) {
      setError(errorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadData(); }, []);

  const resetMessages = () => { setMessage(''); setError(''); };
  const updateCampaign = (field, value) => setCampaignForm((current) => ({ ...current, [field]: value }));
  const updateSegment = (field, value) => setSegmentForm((current) => ({ ...current, [field]: value }));

  const saveSegment = async (event) => {
    event.preventDefault();
    setSaving(true);
    resetMessages();
    const payload = {
      nome: segmentForm.nome.trim(),
      descricao: segmentForm.descricao.trim() || null,
      cor: segmentForm.cor.toUpperCase(),
      prioridade: Number(segmentForm.prioridade),
      ticketMedioMinimo: optionalNumber(segmentForm.ticketMedioMinimo),
      ticketMedioMaximo: optionalNumber(segmentForm.ticketMedioMaximo),
      frequenciaMinimaDias: optionalNumber(segmentForm.frequenciaMinimaDias),
      frequenciaMaximaDias: optionalNumber(segmentForm.frequenciaMaximaDias),
      ativo: segmentForm.ativo,
      rowVersion: segmentForm.rowVersion,
    };
    try {
      const response = segmentForm.id
        ? await marketingAPI.atualizarSegmento(segmentForm.id, payload)
        : await marketingAPI.criarSegmento(payload);
      const persisted = response.data;
      if (!persisted?.id || !persisted.rowVersion || persisted.nome !== payload.nome) {
        throw new Error('A API não confirmou a persistência do segmento.');
      }
      setSegments((current) => current.some((item) => item.id === persisted.id)
        ? current.map((item) => item.id === persisted.id ? persisted : item)
        : [persisted, ...current]);
      setSegmentForm(toSegmentForm(persisted));
      setMessage(`Segmento ${persisted.nome} confirmado no banco com ID ${persisted.id}.`);
    } catch (requestError) {
      setError(errorMessage(requestError));
    } finally {
      setSaving(false);
    }
  };

  const saveCampaign = async (event) => {
    event.preventDefault();
    setSaving(true);
    resetMessages();
    const payload = {
      nome: campaignForm.nome.trim(),
      descricao: campaignForm.descricao.trim() || null,
      tipo: campaignForm.tipo,
      status: campaignForm.status,
      segmentoId: campaignForm.segmentoId || null,
      dataInicio: `${campaignForm.dataInicio}T00:00:00Z`,
      dataFim: campaignForm.dataFim ? `${campaignForm.dataFim}T23:59:59Z` : null,
      orcamento: Number(campaignForm.orcamento),
      custoAtual: Number(campaignForm.custoAtual),
      alcance: Number(campaignForm.alcance),
      cliques: Number(campaignForm.cliques),
      leadsGerados: Number(campaignForm.leadsGerados),
      oportunidadesGeradas: Number(campaignForm.oportunidadesGeradas),
      vendasGeradas: Number(campaignForm.vendasGeradas),
      receitaGerada: Number(campaignForm.receitaGerada),
      publicoAlvo: campaignForm.publicoAlvo.trim() || null,
      conteudo: campaignForm.conteudo.trim() || null,
      rowVersion: campaignForm.rowVersion,
    };
    try {
      const response = campaignForm.id
        ? await marketingAPI.atualizarCampanha(campaignForm.id, payload)
        : await marketingAPI.criarCampanha(payload);
      const persisted = response.data;
      if (!persisted?.id || !persisted.rowVersion || persisted.nome !== payload.nome) {
        throw new Error('A API não confirmou a persistência da campanha.');
      }
      setCampaigns((current) => current.some((item) => item.id === persisted.id)
        ? current.map((item) => item.id === persisted.id ? persisted : item)
        : [persisted, ...current]);
      setCampaignForm(toCampaignForm(persisted));
      setMessage(`Campanha ${persisted.nome} confirmada no banco com ID ${persisted.id}.`);
    } catch (requestError) {
      setError(errorMessage(requestError));
    } finally {
      setSaving(false);
    }
  };

  const removeSegment = async (item) => {
    if (!window.confirm(`Excluir o segmento ${item.nome}?`)) return;
    resetMessages();
    try {
      await marketingAPI.excluirSegmento(item.id);
      setSegments((current) => current.filter((segment) => segment.id !== item.id));
      if (segmentForm.id === item.id) setSegmentForm(emptySegment);
      setMessage(`Segmento ${item.nome} excluído no banco.`);
    } catch (requestError) { setError(errorMessage(requestError)); }
  };

  const removeCampaign = async (item) => {
    if (!window.confirm(`Excluir a campanha ${item.nome}?`)) return;
    resetMessages();
    try {
      await marketingAPI.excluirCampanha(item.id);
      setCampaigns((current) => current.filter((campaign) => campaign.id !== item.id));
      if (campaignForm.id === item.id) setCampaignForm(emptyCampaign);
      setMessage(`Campanha ${item.nome} excluída no banco.`);
    } catch (requestError) { setError(errorMessage(requestError)); }
  };

  return (
    <section className="space-y-6">
      <header className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6 shadow-sm xl:flex-row xl:items-center xl:justify-between">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.18em] text-amber-700">Comercial e CRM</p>
          <h2 className="mt-2 text-2xl font-black text-slate-950">Marketing operacional</h2>
          <p className="mt-1 text-sm text-slate-500">Campanhas, segmentação, investimento e resultados persistidos pela API oficial.</p>
        </div>
        <div className="grid grid-cols-3 gap-3 text-center">
          <Metric label="Campanhas" value={campaigns.length} />
          <Metric label="Em curso" value={activeCampaigns} />
          <Metric label="Receita" value={generatedRevenue.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })} />
        </div>
      </header>

      <div className="inline-flex rounded-lg border border-slate-200 bg-white p-1 shadow-sm">
        <ModeButton active={mode === 'campanhas'} onClick={() => { setMode('campanhas'); resetMessages(); }} icon={BarChart3} label="Campanhas" />
        <ModeButton active={mode === 'segmentos'} onClick={() => { setMode('segmentos'); resetMessages(); }} icon={Layers3} label="Segmentos" />
      </div>

      {(error || message) && <div className={`rounded-lg border px-5 py-4 text-sm font-bold ${error ? 'border-red-200 bg-red-50 text-red-800' : 'border-emerald-200 bg-emerald-50 text-emerald-800'}`}>{error || message}</div>}

      {mode === 'campanhas'
        ? <CampaignWorkspace form={campaignForm} setField={updateCampaign} records={campaigns} segments={segments} loading={loading} saving={saving} onSubmit={saveCampaign} onEdit={(item) => { setCampaignForm(toCampaignForm(item)); resetMessages(); }} onDelete={removeCampaign} onReset={() => { setCampaignForm(emptyCampaign); resetMessages(); }} />
        : <SegmentWorkspace form={segmentForm} setField={updateSegment} records={segments} loading={loading} saving={saving} onSubmit={saveSegment} onEdit={(item) => { setSegmentForm(toSegmentForm(item)); resetMessages(); }} onDelete={removeSegment} onReset={() => { setSegmentForm(emptySegment); resetMessages(); }} />}
    </section>
  );
}

function CampaignWorkspace({ form, setField, records, segments, loading, saving, onSubmit, onEdit, onDelete, onReset }) {
  const statuses = form.id ? allowedTransitions[form.status] || [form.status] : ['Rascunho', 'Agendada'];
  return <div className="grid gap-6 2xl:grid-cols-[minmax(440px,0.95fr)_minmax(0,1.05fr)]">
    <form onSubmit={onSubmit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <FormHeader title="Campanha" id={form.id} onReset={onReset} />
      <div className="mt-5 grid gap-4 sm:grid-cols-2">
        <Input label="Nome" value={form.nome} onChange={(value) => setField('nome', value)} required />
        <Select label="Tipo" value={form.tipo} onChange={(value) => setField('tipo', value)} options={campaignTypes} />
        <Select label="Status" value={form.status} onChange={(value) => setField('status', value)} options={statuses} />
        <Select label="Segmento" value={form.segmentoId} onChange={(value) => setField('segmentoId', value)} options={[{ value: '', label: 'Sem segmento' }, ...segments.filter((item) => item.ativo).map((item) => ({ value: item.id, label: item.nome }))]} objects />
        <Input label="Início" type="date" value={form.dataInicio} onChange={(value) => setField('dataInicio', value)} required />
        <Input label="Término" type="date" value={form.dataFim} onChange={(value) => setField('dataFim', value)} />
        <Input label="Orçamento" type="number" min="0" step="0.01" value={form.orcamento} onChange={(value) => setField('orcamento', value)} required />
        <Input label="Custo realizado" type="number" min="0" step="0.01" value={form.custoAtual} onChange={(value) => setField('custoAtual', value)} required />
      </div>
      <TextArea label="Descrição" value={form.descricao} onChange={(value) => setField('descricao', value)} />
      <div className="mt-4 grid gap-4 sm:grid-cols-3">
        {['alcance', 'cliques', 'leadsGerados', 'oportunidadesGeradas', 'vendasGeradas'].map((field) => <Input key={field} label={metricLabel(field)} type="number" min="0" value={form[field]} onChange={(value) => setField(field, value)} required />)}
        <Input label="Receita gerada" type="number" min="0" step="0.01" value={form.receitaGerada} onChange={(value) => setField('receitaGerada', value)} required />
      </div>
      <TextArea label="Público-alvo" value={form.publicoAlvo} onChange={(value) => setField('publicoAlvo', value)} />
      <TextArea label="Conteúdo" value={form.conteudo} onChange={(value) => setField('conteudo', value)} />
      <SaveButton saving={saving} editing={Boolean(form.id)} />
    </form>
    <RecordList title="Campanhas no banco" loading={loading} empty="Nenhuma campanha cadastrada no banco oficial.">
      {records.map((item) => <article key={item.id} className="border-b border-slate-100 p-5 last:border-b-0">
        <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
          <div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><strong className="text-slate-950">{item.nome}</strong><Badge value={item.status} /></div><p className="mt-2 text-sm text-slate-600">{item.tipo} · {item.segmentoNome || 'Sem segmento'} · ROAS {Number(item.roas || 0).toFixed(2)}</p><p className="mt-1 text-xs text-slate-500">{dateValue(item.dataInicio)} a {dateValue(item.dataFim) || 'sem término'} · receita {Number(item.receitaGerada || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</p></div>
          <Actions onEdit={() => onEdit(item)} onDelete={() => onDelete(item)} />
        </div>
      </article>)}
    </RecordList>
  </div>;
}

function SegmentWorkspace({ form, setField, records, loading, saving, onSubmit, onEdit, onDelete, onReset }) {
  return <div className="grid gap-6 xl:grid-cols-[minmax(380px,0.8fr)_minmax(0,1.2fr)]">
    <form onSubmit={onSubmit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <FormHeader title="Segmento" id={form.id} onReset={onReset} />
      <div className="mt-5 grid gap-4 sm:grid-cols-2">
        <Input label="Nome" value={form.nome} onChange={(value) => setField('nome', value)} required />
        <Input label="Cor" type="color" value={form.cor} onChange={(value) => setField('cor', value)} required />
        <Input label="Prioridade" type="number" min="0" value={form.prioridade} onChange={(value) => setField('prioridade', value)} required />
        <Check label="Segmento ativo" checked={form.ativo} onChange={(value) => setField('ativo', value)} />
        <Input label="Ticket mínimo" type="number" min="0" step="0.01" value={form.ticketMedioMinimo} onChange={(value) => setField('ticketMedioMinimo', value)} />
        <Input label="Ticket máximo" type="number" min="0" step="0.01" value={form.ticketMedioMaximo} onChange={(value) => setField('ticketMedioMaximo', value)} />
        <Input label="Frequência mínima (dias)" type="number" min="0" value={form.frequenciaMinimaDias} onChange={(value) => setField('frequenciaMinimaDias', value)} />
        <Input label="Frequência máxima (dias)" type="number" min="0" value={form.frequenciaMaximaDias} onChange={(value) => setField('frequenciaMaximaDias', value)} />
      </div>
      <TextArea label="Descrição" value={form.descricao} onChange={(value) => setField('descricao', value)} />
      <SaveButton saving={saving} editing={Boolean(form.id)} />
    </form>
    <RecordList title="Segmentos no banco" loading={loading} empty="Nenhum segmento cadastrado no banco oficial.">
      {records.map((item) => <article key={item.id} className="flex flex-col gap-3 border-b border-slate-100 p-5 last:border-b-0 lg:flex-row lg:items-center lg:justify-between">
        <div><div className="flex items-center gap-3"><span className="h-4 w-4 rounded-full border border-slate-300" style={{ backgroundColor: item.cor }} /><strong className="text-slate-950">{item.nome}</strong><Badge value={item.ativo ? 'Ativo' : 'Inativo'} /></div><p className="mt-2 text-sm text-slate-600">Prioridade {item.prioridade} · ticket {item.ticketMedioMinimo ?? 0} a {item.ticketMedioMaximo ?? 'sem limite'}</p></div>
        <Actions onEdit={() => onEdit(item)} onDelete={() => onDelete(item)} />
      </article>)}
    </RecordList>
  </div>;
}

function metricLabel(field) { return ({ alcance: 'Alcance', cliques: 'Cliques', leadsGerados: 'Leads', oportunidadesGeradas: 'Oportunidades', vendasGeradas: 'Vendas' })[field]; }
function Metric({ label, value }) { return <div className="min-w-24 rounded-lg bg-slate-100 px-3 py-2"><p className="text-xs font-bold text-slate-500">{label}</p><p className="text-base font-black text-slate-950">{value}</p></div>; }
function ModeButton({ active, onClick, icon: Icon, label }) { return <button type="button" onClick={onClick} className={`inline-flex h-10 items-center gap-2 rounded-lg px-4 text-sm font-black ${active ? 'bg-slate-950 text-white' : 'text-slate-600 hover:bg-slate-100'}`}><Icon size={17} />{label}</button>; }
function FormHeader({ title, id, onReset }) { return <div className="flex items-start justify-between border-b border-slate-100 pb-4"><div><p className="text-xs font-black uppercase tracking-[0.16em] text-slate-500">{id ? `Registro ${id}` : 'Novo registro'}</p><h3 className="mt-1 text-lg font-black text-slate-950">{title}</h3></div>{id && <button type="button" onClick={onReset} title="Novo registro" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200 text-slate-600"><X size={18} /></button>}</div>; }
function Input({ label, onChange, ...props }) { return <label className="block text-sm font-bold text-slate-700">{label}<input {...props} onChange={(event) => onChange(event.target.value)} className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10" /></label>; }
function TextArea({ label, value, onChange }) { return <label className="mt-4 block text-sm font-bold text-slate-700">{label}<textarea value={value} onChange={(event) => onChange(event.target.value)} rows={3} className="mt-2 w-full resize-y rounded-lg border border-slate-200 bg-white px-3 py-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10" /></label>; }
function Select({ label, value, onChange, options, objects = false }) { return <label className="block text-sm font-bold text-slate-700">{label}<select value={value} onChange={(event) => onChange(event.target.value)} className="mt-2 h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold outline-none focus:border-slate-950 focus:ring-4 focus:ring-slate-950/10">{options.map((item) => { const option = objects ? item : { value: item, label: item }; return <option key={option.value} value={option.value}>{option.label}</option>; })}</select></label>; }
function Check({ label, checked, onChange }) { return <label className="mt-7 flex h-11 items-center gap-3 rounded-lg border border-slate-200 px-3 text-sm font-bold text-slate-700"><input type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} className="h-5 w-5 accent-slate-950" />{label}</label>; }
function SaveButton({ saving, editing }) { return <button disabled={saving} className="mt-6 inline-flex h-11 items-center gap-2 rounded-lg bg-slate-950 px-5 text-sm font-black text-white disabled:opacity-60"><Save size={17} />{saving ? 'Gravando...' : editing ? 'Atualizar registro' : 'Cadastrar registro'}</button>; }
function RecordList({ title, loading, empty, children }) { const count = Array.isArray(children) ? children.length : 0; return <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm"><div className="flex items-center justify-between border-b border-slate-200 px-6 py-5"><div><h3 className="text-lg font-black text-slate-950">{title}</h3><p className="mt-1 text-sm text-slate-500">Dados consultados na API oficial.</p></div><span className="inline-flex items-center gap-2 text-sm font-black text-slate-500"><Plus size={16} />{count}</span></div>{loading ? <p className="p-6 text-sm font-semibold text-slate-500">Consultando banco...</p> : count === 0 ? <p className="p-6 text-sm font-semibold text-slate-500">{empty}</p> : children}</div>; }
function Actions({ onEdit, onDelete }) { return <div className="flex shrink-0 gap-2"><button type="button" onClick={onEdit} title="Editar registro" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-slate-200 text-slate-700"><Pencil size={17} /></button><button type="button" onClick={onDelete} title="Excluir registro" className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-red-200 text-red-700"><Trash2 size={17} /></button></div>; }
function Badge({ value }) { const active = ['Ativo', 'EmAndamento', 'Agendada'].includes(value); return <span className={`rounded-full px-2.5 py-1 text-xs font-black ${active ? 'bg-emerald-50 text-emerald-800' : 'bg-slate-100 text-slate-600'}`}>{value}</span>; }
