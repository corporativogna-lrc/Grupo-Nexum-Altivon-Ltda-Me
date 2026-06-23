# Modelo seguro de configuração local da API Nexum Altivon.
# Copie este arquivo para Y:\NexumAltivon_API_24H\config\api.env.ps1 no servidor.
# Preencha os valores reais somente no servidor. Não envie este arquivo preenchido para o Git.

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5012"

$env:ConnectionStrings__DefaultConnection = "server=192.168.1.72;port=3309;database=nexum_altivon;user=nexum_app;password=COLOQUE_A_SENHA_REAL_AQUI;SslMode=none;AllowPublicKeyRetrieval=true;"
$env:ConnectionStrings__GenesisConnection = "server=192.168.1.72;port=3309;database=genesis_bd;user=nexum_app;password=COLOQUE_A_SENHA_REAL_AQUI;SslMode=none;AllowPublicKeyRetrieval=true;"

$env:JwtSettings__SecretKey = "COLOQUE_UMA_CHAVE_FORTE_COM_MAIS_DE_32_CARACTERES"
$env:JwtSettings__Issuer = "NexumAltivon.API"
$env:JwtSettings__Audience = "NexumAltivon.Front"

$env:AdminUser__Email = "admin@nexumaltivon.com"
$env:AdminUser__Password = "COLOQUE_A_SENHA_ADMIN_REAL_AQUI"
$env:AdminUser__Name = "Administrador Nexum"
$env:AdminUser__Role = "Gerente"
