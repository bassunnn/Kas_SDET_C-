param(
    [string]$LastName = "Фамилия",
    [string]$Initials = "И.О."
)

$archiveName = "${LastName}${Initials}_Тестовое задание_SDETC#.zip"
$outputPath = Join-Path $PSScriptRoot $archiveName

Write-Host "Подготовка архива решения: $archiveName" -ForegroundColor Cyan

# Удаляем предыдущий архив, если существовал
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Force
    Write-Host "Старый архив удален." -ForegroundColor Yellow
}

# Временная директория для чистого архивирования
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $tempDir | Out-Null

try {
    # Копируем проекты и файлы решения, исключая bin, obj, .vs, .git
    $excludeDirs = @('bin', 'obj', '.vs', '.git')
    
    $itemsToCopy = @(
        'UserManagement.sln',
        'test-cases.md',
        'README.md',
        'create-submission-archive.ps1',
        'UserManagement.API',
        'UserManagement.Tests'
    )

    foreach ($item in $itemsToCopy) {
        $sourcePath = Join-Path $PSScriptRoot $item
        if (Test-Path $sourcePath) {
            $destPath = Join-Path $tempDir $item
            if ((Get-Item $sourcePath) -is [System.IO.DirectoryInfo]) {
                Copy-Item -Path $sourcePath -Destination $destPath -Recurse -Force
                # Удаляем bin и obj из скопированной структуры
                foreach ($ex in $excludeDirs) {
                    Get-ChildItem -Path $destPath -Recurse -Directory -Filter $ex | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
                }
            } else {
                Copy-Item -Path $sourcePath -Destination $destPath -Force
            }
        }
    }

    # Создаем zip-архив
    Compress-Archive -Path "$tempDir\*" -DestinationPath $outputPath -Force
    Write-Host "Архив успешно создан: $outputPath" -ForegroundColor Green
    Write-Host "Размер архива: $((Get-Item $outputPath).Length / 1KB) KB" -ForegroundColor Green
    Write-Host "Ссылка для загрузки: https://box.kaspersky.com/u/d/53317deb628f4675bf83/" -ForegroundColor Cyan
}
finally {
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
