from __future__ import annotations

# GenesisGest.Net • v1.1.5.7243 • catálogo ilustrativo de homologação.

import html
import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ASSET_DIR = ROOT / "NexumAltivon_Front-End" / "public" / "imagens" / "catalogo"
SEED_FILE = ROOT / "NexumAltivon_Back-End" / "Database" / "seeds" / "20261030_produtos_homologacao.sql"

# Store, category slug, icon family, price basis, product names.
GROUPS = [
    ("grann-tur", "malas-viagem", "travel", 289.90, ["Mala rígida de viagem 20 polegadas", "Mala rígida de viagem 24 polegadas", "Mala rígida de viagem 28 polegadas"]),
    ("grann-tur", "malas-bordo", "travel", 219.90, ["Mala de bordo compacta com rodas", "Mala executiva de cabine com compartimento frontal"]),
    ("grann-tur", "mochilas-viagem", "travel", 129.90, ["Mochila urbana para viagem 20 litros", "Mochila de viagem expansível 35 litros", "Mochila cargueira para trilha 50 litros", "Mochila impermeável para uso diário"]),
    ("grann-tur", "necessaires", "travel", 49.90, ["Necessaire organizadora de viagem", "Necessaire impermeável com gancho"]),
    ("grann-tur", "organizadores-mala", "travel", 69.90, ["Kit organizador de mala com 6 peças", "Organizador para calçados de viagem"]),
    ("grann-tur", "frascos-viagem", "travel", 29.90, ["Kit de frascos reutilizáveis para viagem"]),
    ("grann-tur", "travesseiros-viagem", "travel", 79.90, ["Travesseiro de pescoço com apoio ajustável"]),
    ("grann-tur", "cadeados-bagagem", "travel", 39.90, ["Cadeado numérico para bagagem"]),
    ("grann-tur", "etiquetas-identificacao", "travel", 24.90, ["Kit de etiquetas identificadoras de bagagem"]),

    ("chronos", "relogios-classicos", "watch", 399.98, ["Relógio analógico clássico mostrador preto", "Relógio analógico clássico mostrador azul", "Relógio analógico feminino acabamento dourado", "Relógio analógico minimalista mostrador branco"]),
    ("chronos", "relogios-esportivos", "watch", 439.98, ["Relógio esportivo analógico com cronógrafo", "Relógio digital esportivo com pulseira ajustável", "Relógio analógico casual resistente à rotina"]),
    ("chronos", "relogios-minimalistas", "watch", 329.98, ["Relógio minimalista caixa fina prata", "Relógio minimalista com pulseira de couro"]),
    ("chronos", "smartwatches-android", "watch", 249.90, ["Smartwatch com notificações e monitoramento diário", "Smartwatch com chamadas Bluetooth", "Smartwatch com tela colorida e pulseira intercambiável"]),
    ("chronos", "relogios-fitness", "watch", 189.90, ["Relógio fitness com monitoramento de atividade"]),
    ("chronos", "pulseiras-inteligentes", "watch", 149.90, ["Pulseira inteligente com monitoramento diário"]),
    ("chronos", "pulseiras-relogio", "watch", 59.90, ["Pulseira de reposição em silicone 20 mm"]),
    ("chronos", "estojos-relogio", "watch", 89.90, ["Estojo individual para guardar relógio"]),

    ("moda-mim", "blusas-camisetas", "fashion", 79.90, ["Camiseta básica de algodão gola redonda", "Blusa feminina manga curta modelagem reta", "Camisa casual de manga longa com botões"]),
    ("moda-mim", "calcas-saias", "fashion", 159.90, ["Calça jeans reta de cintura média", "Calça casual feminina corte reto"]),
    ("moda-mim", "vestidos", "fashion", 179.90, ["Vestido midi casual com alças", "Vestido leve de manga curta", "Vestido social de corte clássico"]),
    ("moda-mim", "tenis-casuais", "fashion", 199.90, ["Tênis casual urbano com cadarço", "Tênis leve para caminhada diária", "Tênis casual de perfil baixo"]),
    ("moda-mim", "sandalias", "fashion", 99.90, ["Sandália feminina de tiras ajustáveis", "Sandália rasteira com fechamento lateral"]),
    ("moda-mim", "calcados-sociais", "fashion", 229.90, ["Sapato social masculino de amarrar"]),
    ("moda-mim", "bolsas", "fashion", 139.90, ["Bolsa transversal compacta com alça regulável", "Bolsa tote estruturada para uso diário"]),
    ("moda-mim", "cintos", "fashion", 69.90, ["Cinto casual com fivela metálica"]),
    ("moda-mim", "bijuterias-adornos", "fashion", 49.90, ["Kit de brincos geométricos leves"]),

    ("geracao-top", "teclados", "tech", 119.90, ["Teclado sem fio ABNT2 com receptor USB", "Teclado compacto Bluetooth para múltiplos dispositivos", "Teclado mecânico compacto com iluminação"]),
    ("geracao-top", "mouses", "tech", 69.90, ["Mouse óptico sem fio 1200 DPI", "Mouse Bluetooth ergonômico com ajuste de sensibilidade", "Mouse compacto para notebook com receptor USB"]),
    ("geracao-top", "armazenamento-digital", "tech", 249.90, ["SSD externo portátil USB 3.2 512 GB", "SSD externo portátil USB 3.2 1 TB"]),
    ("geracao-top", "fones-ouvido", "tech", 179.90, ["Fone Bluetooth intra-auricular com estojo", "Headphone sem fio com microfone integrado", "Fone com fio USB-C e controle de chamadas"]),
    ("geracao-top", "caixas-som", "tech", 229.90, ["Caixa de som Bluetooth portátil compacta", "Caixa de som estéreo para mesa com alimentação USB"]),
    ("geracao-top", "cabos-adaptadores", "tech", 39.90, ["Cabo USB-C reforçado para carregamento e dados"]),
    ("geracao-top", "carregadores", "tech", 89.90, ["Carregador USB-C Power Delivery 30 W", "Carregador de parede USB-C com duas portas"]),
    ("geracao-top", "baterias-portateis", "tech", 129.90, ["Bateria portátil USB-C 10000 mAh"]),

    ("estruturaline", "furadeiras-parafusadeiras", "tools", 329.90, ["Furadeira de impacto com mandril 3/8 polegadas", "Parafusadeira a bateria com controle de torque", "Furadeira a bateria compacta com carregador"]),
    ("estruturaline", "serras", "tools", 399.90, ["Serra circular manual para cortes retos", "Serra tico-tico com velocidade variável", "Serra mármore compacta para cortes guiados"]),
    ("estruturaline", "lixadeiras", "tools", 249.90, ["Lixadeira orbital compacta para acabamento", "Lixadeira de parede com coletor de pó"]),
    ("estruturaline", "chaves-bits", "tools", 59.90, ["Jogo de chaves e bits com estojo organizado", "Jogo de bits para parafusadeira com encaixe hexagonal"]),
    ("estruturaline", "alicates", "tools", 49.90, ["Alicate universal com cabo isolado", "Alicate de corte diagonal para manutenção"]),
    ("estruturaline", "medicao-nivelamento", "tools", 179.90, ["Trena a laser para medição interna"]),
    ("estruturaline", "equipamentos-protecao", "tools", 39.90, ["Óculos de proteção transparentes com hastes ajustáveis"]),
    ("estruturaline", "fixadores-ancoragens", "tools", 29.90, ["Kit de buchas e parafusos para fixação leve"]),
    ("estruturaline", "iluminacao-obra", "tools", 89.90, ["Refletor LED portátil para manutenção e obra"]),

    ("gran-fest", "baloes-arcos", "party", 59.90, ["Kit de arco desconstruído com balões sortidos", "Kit de balões metalizados para decoração", "Pacote de balões de látex para eventos"]),
    ("gran-fest", "paineis-fundos", "party", 149.90, ["Painel decorativo redondo para mesa de festa", "Fundo fotográfico temático para celebrações"]),
    ("gran-fest", "iluminacao-decorativa", "party", 69.90, ["Cordão de luz decorativa para ambiente interno", "Luminária LED decorativa para mesa de eventos"]),
    ("gran-fest", "pratos-bandejas", "party", 29.90, ["Kit de pratos descartáveis para festa", "Bandeja decorativa para servir doces"]),
    ("gran-fest", "copos-canudos", "party", 19.90, ["Kit de copos para festa com 20 unidades", "Canudos reutilizáveis com escova de limpeza"]),
    ("gran-fest", "talheres-guardanapos", "party", 24.90, ["Kit de talheres descartáveis para evento", "Guardanapos decorados pacote para festa"]),
    ("gran-fest", "kits-tematicos", "party", 79.90, ["Kit temático de decoração para aniversário", "Kit de mesa decorada para comemoração"]),
    ("gran-fest", "lembrancinhas", "party", 34.90, ["Caixa de lembrancinhas para convidados"]),
]

PALETTES = {
    "grann-tur": ("#1E3A5F", "#D4AA37"),
    "chronos": ("#202833", "#D4AA37"),
    "moda-mim": ("#7B334B", "#E9BFC8"),
    "geracao-top": ("#174B3A", "#A7E4D0"),
    "estruturaline": ("#4A3728", "#E6A84A"),
    "gran-fest": ("#6B2D5C", "#E8B2D8"),
}

ICONS = {
    "travel": '<rect x="340" y="300" width="320" height="400" rx="52"/><path d="M430 300v-70h140v70M400 700v42m200-42v42" fill="none" stroke="{accent}" stroke-width="28" stroke-linecap="round"/>',
    "watch": '<rect x="420" y="200" width="160" height="600" rx="72" opacity=".65"/><circle cx="500" cy="500" r="190" fill="{accent}"/><circle cx="500" cy="500" r="145" fill="{base}"/><path d="M500 500V390m0 110 88 48" fill="none" stroke="{accent}" stroke-width="22" stroke-linecap="round"/>',
    "fashion": '<path d="M390 270 470 220h60l80 50 110 100-95 90-45-50v300H420V410l-45 50-95-90z"/>',
    "tech": '<rect x="260" y="290" width="480" height="320" rx="36"/><path d="M340 680h320M430 610v70m140-70v70" fill="none" stroke="{accent}" stroke-width="26" stroke-linecap="round"/><circle cx="500" cy="450" r="55" fill="{accent}"/>',
    "tools": '<path d="M320 270h210a70 70 0 0 1 70 70v86H470v-64H360v230h240v-54h160v104a70 70 0 0 1-70 70H320a70 70 0 0 1-70-70V340a70 70 0 0 1 70-70z"/><path d="M600 390h130v75H600zM690 465l95 195" fill="none" stroke="{accent}" stroke-width="45" stroke-linecap="round"/>',
    "party": '<path d="M500 260c-120 0-190 100-170 210 18 98 130 205 170 260 40-55 152-162 170-260 20-110-50-210-170-210z"/><path d="m500 730 35 65-35 40-35-40z" fill="{accent}"/><path d="M330 315c-80 0-125 68-112 142 12 68 74 112 112 155 38-43 100-87 112-155 13-74-32-142-112-142zM670 315c-80 0-125 68-112 142 12 68 74 112 112 155 38-43 100-87 112-155 13-74-32-142-112-142z" opacity=".55"/>',
}


def product_options(store: str, category: str) -> dict[str, list[str]]:
    if store == "grann-tur":
        return {"Cor": ["Preto", "Azul-marinho", "Areia"]}
    if store == "chronos":
        return {"Acabamento": ["Prata", "Preto", "Dourado"], "Pulseira": ["Metal", "Couro", "Silicone"]}
    if store == "moda-mim":
        if category in {"tenis-casuais", "sandalias", "calcados-sociais"}:
            return {"Cor": ["Preto", "Branco", "Caramelo"], "Numeração": ["36", "38", "40", "42"]}
        if category in {"blusas-camisetas", "calcas-saias", "vestidos"}:
            return {"Cor": ["Preto", "Azul", "Off-white"], "Tamanho": ["P", "M", "G", "GG"]}
        return {"Cor": ["Preto", "Caramelo", "Vinho"]}
    if store == "geracao-top":
        return {"Conexão": ["USB", "Bluetooth", "USB e Bluetooth"], "Cor": ["Preto", "Branco", "Grafite"]}
    if store == "estruturaline":
        if category in {"furadeiras-parafusadeiras", "serras", "lixadeiras", "iluminacao-obra"}:
            return {"Tensão": ["127 V", "220 V"]}
        return {"Tamanho": ["Compacto", "Padrão", "Profissional"]}
    return {"Cor": ["Dourado", "Prata", "Colorido"], "Quantidade por kit": ["10 unidades", "20 unidades", "50 unidades"]}


def slugify(value: str) -> str:
    value = value.lower().replace("ç", "c")
    value = re.sub(r"[áàâãä]", "a", value)
    value = re.sub(r"[éèêë]", "e", value)
    value = re.sub(r"[íìîï]", "i", value)
    value = re.sub(r"[óòôõö]", "o", value)
    value = re.sub(r"[úùûü]", "u", value)
    return re.sub(r"[^a-z0-9]+", "-", value).strip("-")


def sql(value: str) -> str:
    return "'" + value.replace("\\", "\\\\").replace("'", "''") + "'"


def main() -> None:
    ASSET_DIR.mkdir(parents=True, exist_ok=True)
    rows: list[tuple[str, ...]] = []
    for store, category, icon, base_price, names in GROUPS:
        for ordinal, name in enumerate(names, 1):
            slug = slugify(name)
            sku = f"HOM-{store[:3].upper()}-{len(rows)+1:03d}"
            image = f"https://nexumaltivon.com.br/imagens/catalogo/{sku.lower()}.svg"
            price = round(base_price * (1 + ((ordinal - 1) % 4) * 0.075), 2)
            palette = PALETTES[store]
            icon_markup = ICONS[icon].format(base=palette[0], accent=palette[1])
            title_lines = [name[i:i+26] for i in range(0, min(len(name), 78), 26)]
            title_svg = "".join(
                f'<text x="500" y="{790 + i*42}" text-anchor="middle" class="label">{html.escape(line)}</text>'
                for i, line in enumerate(title_lines[:2])
            )
            svg = f'''<svg xmlns="http://www.w3.org/2000/svg" width="1000" height="1000" viewBox="0 0 1000 1000" role="img" aria-labelledby="title desc">
<title id="title">{html.escape(name)} — imagem ilustrativa</title><desc id="desc">Ilustração vetorial para homologação do catálogo.</desc>
<defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop stop-color="{palette[0]}"/><stop offset="1" stop-color="#111827"/></linearGradient><filter id="shadow"><feDropShadow dx="0" dy="24" stdDeviation="22" flood-opacity=".24"/></filter></defs>
<rect width="1000" height="1000" rx="54" fill="url(#bg)"/><circle cx="500" cy="470" r="310" fill="#fff" opacity=".07"/>
<g fill="{palette[1]}" filter="url(#shadow)">{icon_markup}</g><text x="500" y="105" text-anchor="middle" class="brand" fill="#fff">GRUPO NEXUM ALTIVON</text>
<rect x="300" y="125" width="400" height="4" rx="2" fill="{palette[1]}" opacity=".85"/>{title_svg}
<rect x="290" y="890" width="420" height="54" rx="27" fill="{palette[1]}"/><text x="500" y="926" text-anchor="middle" class="badge">IMAGEM ILUSTRATIVA • HOMOLOGAÇÃO</text>
<style>.brand{{font:700 25px Arial,sans-serif;letter-spacing:4px}}.label{{font:700 25px Arial,sans-serif;fill:#fff}}.badge{{font:700 15px Arial,sans-serif;fill:#151515;letter-spacing:1px}}</style></svg>'''
            (ASSET_DIR / f"{sku.lower()}.svg").write_text(svg, encoding="utf-8")
            short = f"{name}. Item de catálogo para validar busca, carrinho, pedidos e PDV. Imagem ilustrativa de homologação."
            detail = (f"{name}. Cadastro de homologação para exercitar a apresentação do catálogo, seleção do produto, "
                      f"cálculo do carrinho, reserva de estoque e integração com pedidos e ponto de venda. "
                      f"A referência visual é uma ilustração vetorial, não uma fotografia do produto comercial. "
                      f"Preço e especificações são referências de homologação e devem ser confirmados antes de uso comercial. "
                      f"Opções representadas para validação do seletor: variações de cor e configuração conforme a família do item.")
            tags = json.dumps(
                {"homologacao": True, "opcoes": product_options(store, category)},
                ensure_ascii=False,
                separators=(",", ":"),
            )
            # Weight and dimensions allow the existing public-catalog contract; stock is explicitly seeded for test checkout/POS.
            rows.append((sku, store, category, name, slug, short, detail, f"{price:.2f}", image, tags))

    if len(rows) != 100:
        raise SystemExit(f"Esperados 100 produtos; gerados {len(rows)}")
    if any(len(row[9]) > 255 for row in rows):
        raise SystemExit("Metadados de opcoes excedem o limite de tags VARCHAR(255)")

    category_seed = (ROOT / "NexumAltivon_Back-End" / "Database" / "seeds" / "20261030_categorias_seis_lojas.sql").read_text(encoding="utf-8")
    available_categories = set(re.findall(r"^\('([^']+)','[^']+','([^']+)'", category_seed, re.MULTILINE))
    missing_categories = sorted({(row[1], row[2]) for row in rows} - available_categories)
    if missing_categories:
        raise SystemExit(f"Categorias ausentes na seed: {missing_categories}")

    values = ",\n".join(
        "(" + ",".join(sql(part) for part in row) + ")" for row in rows
    )
    SEED_FILE.parent.mkdir(parents=True, exist_ok=True)
    SEED_FILE.write_text(f'''-- GenesisGest.Net v1.1.5.7243 • catalogo de homologacao para site, pedidos e PDV.
-- Todas as imagens sao vetoriais ilustrativas; estoque inicial: 5 unidades por SKU.
-- Precos sao referencias para homologacao e nao confirmam fornecedor, custo ou preco comercial.
-- Referencias de varejo consultadas em 30/10/2026:
-- https://www.decathlon.com.br/colecao/ofertas/mochilas
-- https://loja.chillibeans.com.br/relogios/chillibeans
-- https://www.lojasrenner.com.br/lista/promocoes/feminino/camisas
-- https://www.kalunga.com.br/depto/informatica/teclado-e-mouse-wireless/kit-teclado-e-mouse-sem-fio/1/2081/3326
-- https://www.leroymerlin.com.br/marceneiro
-- https://www.magazineluiza.com.br/busca/arco%2Bbaloes%2Bfesta/
-- Aplicar depois das seeds de lojas e categorias, em janela controlada.
START TRANSACTION;
CREATE TEMPORARY TABLE _nexum_catalogo_homologacao (
    sku VARCHAR(50) COLLATE utf8mb4_unicode_ci PRIMARY KEY,
    loja_slug VARCHAR(50) COLLATE utf8mb4_unicode_ci NOT NULL,
    categoria_slug VARCHAR(100) COLLATE utf8mb4_general_ci NOT NULL,
    nome VARCHAR(200) NOT NULL, slug VARCHAR(200) NOT NULL, descricao_curta VARCHAR(500) NOT NULL,
    descricao_longa LONGTEXT NOT NULL, preco DECIMAL(10,2) NOT NULL, imagem_principal VARCHAR(255) NOT NULL,
    tags VARCHAR(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
INSERT INTO _nexum_catalogo_homologacao VALUES
{values};
INSERT INTO produtos
    (loja_id,categoria_id,sku,nome,slug,descricao_curta,descricao_longa,preco,custo,peso,altura,largura,comprimento,
     imagem_principal,imagens_galeria,estoque_minimo,estoque_atual,estoque_reservado,tipo_produto,marca,tags,
     seo_titulo,seo_descricao,seo_keywords,destaque,ativo,created_at,updated_at)
SELECT l.id,c.id,s.sku,s.nome,s.slug,s.descricao_curta,s.descricao_longa,s.preco,0.00,0.500,10.00,15.00,20.00,
       s.imagem_principal,NULL,1,5,0,'Proprio','Nexum Altivon',s.tags,s.nome,s.descricao_curta,
       CONCAT(s.loja_slug,',homologacao,ilustrativo'),0,1,UTC_TIMESTAMP(),UTC_TIMESTAMP()
FROM _nexum_catalogo_homologacao s
JOIN lojas l ON l.slug=s.loja_slug AND l.ativa=1
JOIN categorias c ON c.loja_id=l.id AND c.slug=s.categoria_slug AND c.ativa=1
WHERE NOT EXISTS (SELECT 1 FROM produtos p WHERE p.sku=s.sku);
COMMIT;
DROP TEMPORARY TABLE _nexum_catalogo_homologacao;
SELECT COUNT(*) AS produtos_catalogo_homologacao FROM produtos WHERE tags LIKE '%homologacao%';
''', encoding="utf-8")
    print(f"Produtos: {len(rows)}; imagens SVG: {len(rows)}; SQL: {SEED_FILE}")


if __name__ == "__main__":
    main()
