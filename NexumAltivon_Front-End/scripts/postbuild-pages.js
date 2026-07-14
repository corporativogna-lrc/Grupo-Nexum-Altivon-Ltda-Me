/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

const fs = require('fs');
const path = require('path');

const buildDir = path.join(__dirname, '..', 'build');
const indexPath = path.join(buildDir, 'index.html');
const notFoundPath = path.join(buildDir, '404.html');
const codeHeader = `/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
`;
const htmlHeader = `<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->
`;

if (!fs.existsSync(indexPath)) {
  throw new Error(`index.html nao encontrado em ${indexPath}`);
}

const prependHeader = (filePath, header) => {
  const content = fs.readFileSync(filePath, 'utf8');
  if (!content.startsWith(header)) {
    fs.writeFileSync(filePath, `${header}${content}`, 'utf8');
  }
};

const sanitizeAssets = (directory) => {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const entryPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      sanitizeAssets(entryPath);
      continue;
    }

    if (entry.name.endsWith('.map')) {
      fs.unlinkSync(entryPath);
      continue;
    }

    if (entry.name.endsWith('.js')) {
      const currentContent = fs.readFileSync(entryPath, 'utf8');
      const content = (currentContent.startsWith(codeHeader)
        ? currentContent.slice(codeHeader.length)
        : currentContent)
        .replace(/\n?\/\/# sourceMappingURL=.*$/u, '');
      fs.writeFileSync(entryPath, `${codeHeader}${content}`, 'utf8');
      continue;
    }

    if (entry.name.endsWith('.css')) {
      const currentContent = fs.readFileSync(entryPath, 'utf8');
      const content = (currentContent.startsWith(codeHeader)
        ? currentContent.slice(codeHeader.length)
        : currentContent)
        .replace(/\n?\/\*# sourceMappingURL=.*?\*\/$/u, '');
      fs.writeFileSync(entryPath, `${codeHeader}${content}`, 'utf8');
      continue;
    }

    if (entry.name.endsWith('.LICENSE.txt')) {
      prependHeader(entryPath, codeHeader);
    }
  }
};

prependHeader(indexPath, htmlHeader);
sanitizeAssets(path.join(buildDir, 'static'));

const notFoundHtml = `${htmlHeader}<!doctype html>
<html lang="pt-BR">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Nexum Altivon</title>
    <script type="text/javascript">
      (function () {
        var location = window.location;
        var destination =
          location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') +
          '/?/' +
          location.pathname.slice(1).replace(/&/g, '~and~') +
          (location.search ? '&' + location.search.slice(1).replace(/&/g, '~and~') : '') +
          location.hash;

        location.replace(destination);
      })();
    </script>
  </head>
  <body>
    <noscript>Habilite JavaScript para acessar a Nexum Altivon.</noscript>
  </body>
</html>
`;

fs.writeFileSync(notFoundPath, notFoundHtml, 'utf8');
