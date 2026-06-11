@echo off
setlocal

echo ==================================================
echo Nexum Altivon - Checagem de Producao
echo ==================================================
echo Data/hora:
echo %DATE% %TIME%
echo.

echo [1/5] DNS do site principal
nslookup www.nexumaltivon.com 1.1.1.1
echo.

echo [2/5] DNS da API
nslookup api.nexumaltivon.com 1.1.1.1
echo.

echo [3/5] HTTP site principal
curl -I https://www.nexumaltivon.com/
echo.

echo [4/5] HTTP login/painel
curl -I https://www.nexumaltivon.com/login
echo.

echo [5/5] HTTP API
curl -I https://api.nexumaltivon.com/
echo.

echo ==================================================
echo Fim da checagem
echo ==================================================
