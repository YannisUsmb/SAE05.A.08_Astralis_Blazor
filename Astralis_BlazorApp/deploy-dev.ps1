Write-Host "🚀 1. Compilation du code..." -ForegroundColor Cyan
dotnet publish -c Release -o published

Write-Host "📦 2. Construction de l'image Docker..." -ForegroundColor Cyan
docker build -t dockerregistryastralis.azurecr.io/blazor-app:v1 .

Write-Host "☁️ 3. Envoi vers Azure..." -ForegroundColor Cyan
docker push dockerregistryastralis.azurecr.io/blazor-app:v1

Write-Host "✅ Terminé ! Le site va se mettre à jour." -ForegroundColor Green