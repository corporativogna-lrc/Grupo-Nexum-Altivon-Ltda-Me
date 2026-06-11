const fs = require('fs');
const path = require('path');

const buildDir = path.join(__dirname, '..', 'build');
const indexPath = path.join(buildDir, 'index.html');
const notFoundPath = path.join(buildDir, '404.html');

if (!fs.existsSync(indexPath)) {
  throw new Error(`index.html nao encontrado em ${indexPath}`);
}

const notFoundHtml = `<!doctype html>
<html lang="pt-BR">
  <head>
    <meta charset="utf-8" />
    <title>Nexum Altivon</title>
    <script type="text/javascript">
      (function () {
        var l = window.location;
        var newUrl =
          l.protocol + '//' + l.hostname + (l.port ? ':' + l.port : '') +
          '/?/' +
          l.pathname.slice(1).replace(/&/g, '~and~') +
          (l.search ? '&' + l.search.slice(1).replace(/&/g, '~and~') : '') +
          l.hash;

        l.replace(newUrl);
      })();
    </script>
  </head>
  <body></body>
</html>
`;

fs.copyFileSync(indexPath, path.join(buildDir, 'index.fallback.html'));
fs.writeFileSync(notFoundPath, notFoundHtml, 'utf8');
