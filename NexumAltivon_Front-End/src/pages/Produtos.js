import { useEffect, useMemo, useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { categoriaAPI, produtoAPI, unwrapApiData } from '../services/api';
import ProductCard from '../components/ProductCard';
import { ArrowDownUp, Filter, Search, SlidersHorizontal, X } from 'lucide-react';

const sortOptions = {
  destaque: 'Destaques',
  menor_preco: 'Menor preço',
  maior_preco: 'Maior preço',
  nome: 'Nome',
};

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

export default function Produtos() {
  const [params, setParams] = useSearchParams();
  const [produtos, setProdutos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState(params.get('busca') || '');
  const [selectedCategory, setSelectedCategory] = useState(params.get('categoria') || '');
  const [sortBy, setSortBy] = useState('destaque');
  const [inStockOnly, setInStockOnly] = useState(true);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [produtosRes, categoriasRes] = await Promise.all([
        produtoAPI.getAll({
          pagina: 1,
          itensPorPagina: 60,
          busca: searchTerm.trim() || undefined,
          categoriaId: selectedCategory || undefined,
        }),
        categoriaAPI.getAll(),
      ]);
      const produtosData = unwrapApiData(produtosRes.data);
      const categoriasData = unwrapApiData(categoriasRes.data);
      setProdutos(Array.isArray(produtosData) ? produtosData.filter(isProdutoPublicavel) : []);
      setCategorias(Array.isArray(categoriasData) ? categoriasData : []);
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Erro:', error);
      }
      setProdutos([]);
      setCategorias([]);
    } finally {
      setLoading(false);
    }
  }, [searchTerm, selectedCategory]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  useEffect(() => {
    const nextParams = {};
    if (selectedCategory) nextParams.categoria = selectedCategory;
    if (searchTerm.trim()) nextParams.busca = searchTerm.trim();
    setParams(nextParams);
  }, [selectedCategory, searchTerm, setParams]);

  const filteredProdutos = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase();
    const filtered = produtos.filter((produto) => {
      if (!isProdutoPublicavel(produto)) return false;
      const matchesSearch =
        !normalizedSearch ||
        produto.nome?.toLowerCase().includes(normalizedSearch) ||
        produto.descricao?.toLowerCase().includes(normalizedSearch) ||
        produto.sku?.toLowerCase().includes(normalizedSearch);
      const matchesStock = !inStockOnly || Number(produto.estoque) > 0;
      const matchesCategory = !selectedCategory || String(produto.categoria_id || produto.categoria?.id) === String(selectedCategory);
      return matchesSearch && matchesStock && matchesCategory;
    });

    return [...filtered].sort((a, b) => {
      const priceA = a.preco_promocional || a.preco || 0;
      const priceB = b.preco_promocional || b.preco || 0;
      if (sortBy === 'menor_preco') return priceA - priceB;
      if (sortBy === 'maior_preco') return priceB - priceA;
      if (sortBy === 'nome') return (a.nome || '').localeCompare(b.nome || '');
      return Number(Boolean(b.destaque)) - Number(Boolean(a.destaque));
    });
  }, [inStockOnly, produtos, searchTerm, selectedCategory, sortBy]);

  const clearFilters = () => {
    setSearchTerm('');
    setSelectedCategory('');
    setSortBy('destaque');
    setInStockOnly(true);
  };

  return (
    <main className="min-h-screen bg-[#050505] text-white">
      <section className="border-b border-[#2A2A2A] bg-[radial-gradient(circle_at_top_left,rgba(201,162,39,0.18),transparent_32%),linear-gradient(135deg,#050505,#101010)]">
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
          <div className="flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.22em] text-[#C9A227]">Catálogo integrado</p>
              <h1 className="mt-3 text-4xl font-black text-white sm:text-5xl" data-testid="produtos-title">
                Produtos Nexum Altivon
              </h1>
              <p className="mt-4 max-w-2xl text-[#B8B8B8]">
                Catálogo conectado ao sistema: produtos, estoque, preço promocional e categorias entram pelo mesmo fluxo da API operacional.
              </p>
            </div>
            <div className="grid grid-cols-3 gap-3 rounded-2xl border border-[#2A2A2A] bg-black/40 p-3 text-center backdrop-blur">
              <div>
                <p className="text-2xl font-black text-[#C9A227]">{filteredProdutos.length}</p>
                <p className="text-xs font-bold uppercase text-[#A0A0A0]">produtos</p>
              </div>
              <div>
                <p className="text-2xl font-black text-[#C9A227]">{categorias.length}</p>
                <p className="text-xs font-bold uppercase text-[#A0A0A0]">categorias</p>
              </div>
              <div>
                <p className="text-2xl font-black text-[#C9A227]">API</p>
                <p className="text-xs font-bold uppercase text-[#A0A0A0]">integrada</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="mx-auto grid max-w-7xl gap-8 px-4 py-10 sm:px-6 lg:grid-cols-[280px_minmax(0,1fr)] lg:px-8">
        <aside className="h-fit rounded-2xl border border-[#2A2A2A] bg-[#111111] p-5 shadow-[0_24px_80px_rgba(0,0,0,0.35)] lg:sticky lg:top-28">
          <div className="mb-5 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <SlidersHorizontal size={18} />
              <h2 className="font-black text-white">Filtros</h2>
            </div>
            <button onClick={clearFilters} className="inline-flex items-center gap-1 text-xs font-black text-[#A0A0A0] hover:text-[#C9A227]">
              <X size={14} />
              Limpar
            </button>
          </div>

          <label className="block text-sm font-bold text-[#D8D8D8]" htmlFor="search-input">Buscar</label>
          <div className="relative mt-2">
            <Search className="absolute left-3 top-3 text-[#777]" size={18} />
            <input
              id="search-input"
              type="text"
              placeholder="Modelo, SKU ou estilo"
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] py-3 pl-10 pr-3 text-sm text-white outline-none transition placeholder:text-[#777] focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10"
              data-testid="search-input"
            />
          </div>

          <div className="mt-6">
            <label className="mb-2 flex items-center gap-2 text-sm font-bold text-[#D8D8D8]" htmlFor="category-filter">
              <Filter size={16} />
              Coleção
            </label>
            <select
              id="category-filter"
              value={selectedCategory}
              onChange={(event) => setSelectedCategory(event.target.value)}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-3 py-3 text-sm text-white outline-none transition focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10"
              data-testid="category-filter"
            >
              <option value="">Todas as coleções</option>
              {categorias.map((cat) => (
                <option key={cat.id} value={cat.id}>{cat.nome}</option>
              ))}
            </select>
          </div>

          <div className="mt-6">
            <label className="mb-2 flex items-center gap-2 text-sm font-bold text-[#D8D8D8]" htmlFor="sort-by">
              <ArrowDownUp size={16} />
              Ordenar
            </label>
            <select
              id="sort-by"
              value={sortBy}
              onChange={(event) => setSortBy(event.target.value)}
              className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] px-3 py-3 text-sm text-white outline-none transition focus:border-[#C9A227] focus:ring-4 focus:ring-[#C9A227]/10"
            >
              {Object.entries(sortOptions).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </div>

          <label className="mt-6 flex items-center justify-between rounded-xl border border-[#2A2A2A] bg-[#080808] px-3 py-3 text-sm font-bold text-[#D8D8D8]">
            <span>Somente pronta entrega</span>
            <input
              type="checkbox"
              checked={inStockOnly}
              onChange={(event) => setInStockOnly(event.target.checked)}
              className="h-5 w-5 accent-[#C9A227]"
            />
          </label>
        </aside>

        <div>
          {loading && (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
              {[1, 2, 3, 4, 5, 6].map((item) => (
                <div key={item} className="h-[520px] animate-pulse rounded-2xl border border-[#2A2A2A] bg-[#111111]" />
              ))}
            </div>
          )}

          {!loading && filteredProdutos.length === 0 && (
            <div className="rounded-2xl border border-[#2A2A2A] bg-[#111111] px-6 py-16 text-center shadow-[0_24px_80px_rgba(0,0,0,0.35)]">
              <p className="text-xl font-black text-white">Nenhum produto cadastrado</p>
              <p className="mt-2 text-[#A0A0A0]">Quando o banco devolver itens reais, eles aparecem aqui. Se quiser, ajuste os filtros para buscar outra categoria.</p>
              <button onClick={clearFilters} className="mt-6 rounded-full bg-[#C9A227] px-5 py-3 text-sm font-black text-black">
                Limpar filtros
              </button>
            </div>
          )}

          {!loading && filteredProdutos.length > 0 && (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3" data-testid="produtos-grid">
              {filteredProdutos.map((produto) => (
                <ProductCard key={produto.id} product={produto} />
              ))}
            </div>
          )}
        </div>
      </section>
    </main>
  );
}
