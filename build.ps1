# ============================================================================
#  GodMode —— 编译脚本
#
#  用 Windows 自带的 .NET Framework 编译器（csc.exe）把 GodMode.cs 编译成
#  GodMode.dll。不需要 Visual Studio、不需要 NuGet、不需要装 SDK。
#
#  用法：
#     pwsh -File .\build.ps1
#     pwsh -File .\build.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
#     pwsh -File .\build.ps1 -OutPath .\dist\GodMode.dll
#
#  游戏路径按这个顺序查找：
#     1. -GamePath 参数
#     2. 环境变量 $env:CASUALTIES_UNKNOWN_PATH
#     3. 从 Steam 库文件夹自动找（appid 4576510）
# ============================================================================
[CmdletBinding()]
param(
  # 游戏根目录（就是放着 CasualtiesUnknown.exe 的那个文件夹）
  [string]$GamePath = '',

  # GodMode.dll 输出到哪。默认：<游戏>\BepInEx\plugins\GodMode.dll
  # 游戏正在运行的时候请用这个参数 —— 插件 DLL 被进程映射住了，
  # 游戏开着的时候覆盖不了。
  [string]$OutPath = ''
)

$ErrorActionPreference = 'Stop'

$AppId   = '4576510'                       # 《Casualties: Unknown》的 Steam appid
$ExeName = 'CasualtiesUnknown.exe'

function Find-GamePath {
  # 1) 命令行显式给的
  if ($GamePath) {
    if (Test-Path (Join-Path $GamePath $ExeName)) { return (Resolve-Path $GamePath).Path }
    throw "-GamePath 给了，但里面没有 '$ExeName'：$GamePath"
  }
  # 2) 环境变量
  if ($env:CASUALTIES_UNKNOWN_PATH) {
    if (Test-Path (Join-Path $env:CASUALTIES_UNKNOWN_PATH $ExeName)) {
      return (Resolve-Path $env:CASUALTIES_UNKNOWN_PATH).Path
    }
    Write-Warning "CASUALTIES_UNKNOWN_PATH 有设置，但里面没有 '$ExeName' —— 忽略。"
  }
  # 3) 把每个 Steam 库（libraryfolders.vdf）都解析一遍，找这个 appid
  $steamRoots = @()
  foreach ($reg in @('HKCU:\Software\Valve\Steam', 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam')) {
    try {
      $p = (Get-ItemProperty -Path $reg -ErrorAction Stop).SteamPath
      if ($p) { $steamRoots += $p }
    } catch { }
  }
  $steamRoots += @('C:\Program Files (x86)\Steam', 'D:\Steam', 'E:\Steam')
  $steamRoots = $steamRoots | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

  foreach ($root in $steamRoots) {
    $vdf = Join-Path $root 'steamapps\libraryfolders.vdf'
    if (-not (Test-Path $vdf)) { continue }
    $text = Get-Content $vdf -Raw
    # 每一行 "path" 都是一个库根目录，逐个找我们的 appid
    $libs = [regex]::Matches($text, '"path"\s+"([^"]+)"') | ForEach-Object { $_.Groups[1].Value -replace '\\\\', '\' }
    $libs += (Join-Path $root 'steamapps')
    foreach ($lib in $libs) {
      $acf = Join-Path $lib "appmanifest_$AppId.acf"
      if (-not (Test-Path $acf)) { continue }
      $installdir = [regex]::Match((Get-Content $acf -Raw), '"installdir"\s+"([^"]+)"').Groups[1].Value
      if (-not $installdir) { continue }
      $candidate = Join-Path (Join-Path $lib 'common') $installdir
      if (Test-Path (Join-Path $candidate $ExeName)) { return (Resolve-Path $candidate).Path }
    }
  }
  throw @"
自动找不到游戏。
请显式指定，例如：
    .\build.ps1 -GamePath "E:\Steam\steamapps\common\Casualties Unknown Demo"
或者设一个永久环境变量：
    setx CASUALTIES_UNKNOWN_PATH "E:\Steam\steamapps\common\Casualties Unknown Demo"
"@
}

$Game = Find-GamePath
$Csc  = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$Src  = Join-Path $PSScriptRoot 'src\GodMode.cs'
if (-not (Test-Path $Src)) { $Src = Join-Path $PSScriptRoot 'GodMode.cs' }   # 扁平目录结构的兜底
$Core = Join-Path $Game 'BepInEx\core'
$Man  = Join-Path $Game 'CasualtiesUnknown_Data\Managed'
$Plug = Join-Path $Game 'BepInEx\plugins'

if ([string]::IsNullOrWhiteSpace($OutPath)) { $Out = Join-Path $Plug 'GodMode.dll' } else { $Out = $OutPath }
$OutDir = Split-Path $Out -Parent
if ($OutDir -and -not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir | Out-Null }

Write-Host '=== GodMode build ===' -ForegroundColor Cyan
Write-Host "game : $Game"
if (-not (Test-Path $Csc))  { throw "找不到编译器：$Csc" }
if (-not (Test-Path $Src))  { throw "找不到源码：$Src" }
if (-not (Test-Path $Plug)) { New-Item -ItemType Directory -Path $Plug | Out-Null }

# 必须引用 netstandard.dll：Unity 2022 的程序集把 System.Object 转发到了
# netstandard 2.1，老版 csc 不加这个引用会报 CS0012。
$refs = @(
  (Join-Path $Core 'BepInEx.dll'),
  (Join-Path $Core '0Harmony.dll'),
  (Join-Path $Man  'Assembly-CSharp.dll'),
  (Join-Path $Man  'netstandard.dll'),
  # TextMeshProUGUI 在 Unity.TextMeshPro.dll 里
  # （Image / CanvasScaler / GraphicRaycaster 在 UnityEngine.UI.dll，被下面的通配符匹配到）
  (Join-Path $Man  'Unity.TextMeshPro.dll')
)
# Unity 把引擎拆成了几十个模块（FontStyle 在 TextRenderingModule、Input 在
# InputLegacyModule……）—— 干脆全部引用，省得一个个猜。
$refs += (Get-ChildItem $Man -Filter 'UnityEngine*.dll' | ForEach-Object { $_.FullName })
foreach ($r in $refs) { if (-not (Test-Path $r)) { throw "缺少引用程序集：$r" } }
Write-Host ("references: " + $refs.Count)

# 路径里有空格（"Casualties Unknown Demo"），而 PowerShell 7 会把裸参数按空格
# 拆开 —— 于是报出一堆莫名其妙的 CS0006/CS2001。改成写一个 csc 响应文件，
# 这样完全没有引号歧义。
$rsp = Join-Path $PSScriptRoot 'build.rsp'
$rspLines = @('/target:library', '/langversion:5', '/optimize+', '/nologo', '/utf8output', ('/out:"' + $Out + '"'))
foreach ($r in $refs) { $rspLines += ('/reference:"' + $r + '"') }
$rspLines += ('"' + $Src + '"')
[System.IO.File]::WriteAllLines($rsp, $rspLines, (New-Object System.Text.UTF8Encoding($false)))

& $Csc "@$rsp"
if ($LASTEXITCODE -ne 0) { throw "编译失败，csc exit=$LASTEXITCODE（看 build.rsp）" }

Write-Host ''
Write-Host '[OK] 编译成功' -ForegroundColor Green
Get-Item $Out | Select-Object FullName, Length, LastWriteTime | Format-List
