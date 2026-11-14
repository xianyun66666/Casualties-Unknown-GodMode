# AI-INSTALL.md —— 给 AI / 脚本用的安装说明

> 这份文件是**写给自动化代理（AI Agent、部署脚本）看的**，不是给人看的文学性说明。
> 目标是：**只读这一份，就能在全新机器上把 GodMode 装好、验证好、并在出问题时定位到原因。**
> 人类向的完整说明书见 [README.md](README.md)。

---

## 0. 机器可读摘要（先读这一段）

```yaml
product:        Casualties Unknown — GodMode (BepInEx 5 插件)
latest_version: 1.3.0
artifact:       GodMode.dll
artifact_size:  89600            # 字节，v1.3.0
artifact_md5:   B28C9F1D648F6ABF383AB65A8A72DE0E
game:           Casualties Unknown (官方版 / 早期 Demo 版都适用)
loader:         BepInEx 5.4.23.5  (x64, Unity Mono)
runtime:        .NET Framework 4.x / Unity 2022.3.62
dependencies:   无（只依赖 BepInEx 自带的 0Harmony 2.9.0.0）
install_type:   单文件复制（drop-in），无需改游戏文件
restart_needed: 是（BepInEx 只在启动时扫描 plugins 目录）
```

**一句话流程**：把 `GodMode.dll` 放进 `<游戏目录>\BepInEx\plugins\`，启动游戏，看日志出现
`[GodMode] 已加载 v1.3.0` 即成功。

---

## 1. 前置条件检查（按顺序，任何一步失败就停）

### 1.1 找到游戏目录

游戏根目录 = **包含 `CasualtiesUnknown.exe` 的那个目录**。常见位置：

| 版本 | 路径 |
| --- | --- |
| 官方版 | `C:\Users\Administrator\Desktop\CasualtiesUnknown` |
| Steam Demo | `E:\steam\steamapps\common\Casualties Unknown Demo` |

判定命令（Windows PowerShell）：

```powershell
$game = 'C:\Users\Administrator\Desktop\CasualtiesUnknown'   # 换成实际路径
Test-Path "$game\CasualtiesUnknown.exe"          # 必须 True
Test-Path "$game\CasualtiesUnknown_Data\Managed\Assembly-CSharp.dll"   # 必须 True
```

### 1.2 确认 BepInEx 5 已安装

```powershell
Test-Path "$game\BepInEx\core\BepInEx.dll"       # 必须 True
Test-Path "$game\winhttp.dll"                    # 必须 True（BepInEx 5 的注入器；x64 游戏用这个）
Test-Path "$game\BepInEx\plugins"                # 必须 True
```

- 版本必须是 **5.4.23.x（x64）**。用 6.x（IL2CPP 版）或 BeplnEx 4 都**不行**。
- 如果 `winhttp.dll` 缺失 → BepInEx 没装好，先装 BepInEx 5.4.23.5。

### 1.3 确认没有版本冲突

```powershell
Get-ChildItem "$game\BepInEx\plugins" -Recurse -Filter *.dll |
  Where-Object { $_.Name -match 'GodMode' } |
  Select-Object FullName, Length
```

- 只应该有一个 `GodMode.dll`。若同时存在 `GodMode3D.dll`、`CU3D\CU3D.dll` 之类
  **会抢相机的 3D 插件**，与本插件无直接冲突，但会造成画面异常，按需禁用。
- 若存在旧版 `GodMode.dll`（< 89600 字节），会被本次安装覆盖。

---

## 2. 安装（3 步，必须按序）

### 2.1 关闭游戏（强制要求）

```powershell
Get-Process -Name CasualtiesUnknown -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 800
```

> **为什么必须先关**：游戏运行时会**内存映射**插件 DLL，此时覆盖写入会失败，
> 报 `error CS0016`（如果你在编译）或 `拒绝访问`（如果在复制）。
> 注意：单纯用 `File.Open(...,'ReadWrite')` 测试可能**显示可写**，但实际映射仍会锁死 —— 不要信这个测试。

### 2.2 备份旧文件（幂等）

```powershell
$dst = "$game\BepInEx\plugins\GodMode.dll"
if (Test-Path $dst) {
    Copy-Item $dst "$dst.bak" -Force
}
```

### 2.3 复制 DLL → 校验

```powershell
Copy-Item '.\GodMode.dll' $dst -Force

$h = Get-FileHash $dst -Algorithm MD5
$h.Hash                                        # v1.3.0 应为 B28C9F1D648F6ABF383AB65A8A72DE0E
(Get-Item $dst).Length                         # v1.3.0 应为 89600
```

哈希或大小不符 → 文件损坏或被其它进程替换，重新复制。

> **不要**把 DLL 放进 `BepInEx\core\` 或游戏根目录，只放 `BepInEx\plugins\`。

---

## 3. 配置（可选，首次启动游戏后自动生成）

路径：

```
<游戏目录>\BepInEx\config\casualtiesunknown.godmode.cfg
```

```ini
[Keys]
Menu = F1                 # 统一主菜单
GodMode = F2
FlyNoClip = F3
MaxSkills = F4
FullHeal = F5
TeleportToMouse = F6
Fullbright = F7
StarterKit = F8
ItemSpawner = F9
OneHitKill = F10

[Options]
ConfigVersion = 1.3.0     # 由插件维护；不要手改，改了会被重写键位
ShowHud = true            # 左上角状态提示
DebugLog = true
StartWithGodMode = false
StartWithFly = false
WorldMargin = 12          # 飞行限位（防飞出世界崩溃）
CrashGuard = true         # 跑到世界外的 NPC 自动拉回
MaxSkillLevel = 30
Watchdog = true
OneHitMultiplier = 100000
SpawnPosMode = 2
SpawnCondition = 1
SpawnAutoPickup = true
SpawnBatchPerFrame = 15
SpawnMaxBatch = 800
PauseWhileOpen = true
DumpInventory = true
```

**关键规则**

1. cfg **只在游戏启动时读取**。改了 cfg 必须重启游戏。
2. 键位名用 Unity `KeyCode` 写法：`F1`、`G`、`Keypad1`、`LeftAlt`、`BackQuote`。
3. `ConfigVersion` 不是 `1.3.0` 时，插件会**自动把十个键位重写为 F1~F10 默认值**
   （这是从旧版升级的迁移逻辑）。想保留自定义键位，迁移完成后别再改版本号。
4. 想恢复默认：**删掉 cfg 再启动游戏**。

---

## 4. 验证（这一步不可跳过）

### 4.1 启动游戏

```powershell
Start-Process "$game\CasualtiesUnknown.exe"
Start-Sleep -Seconds 25        # 世界生成较慢，等久一点
```

### 4.2 读日志（唯一的事实来源）

```powershell
$log = "$game\BepInEx\LogOutput.log"
$fs  = [IO.File]::Open($log,'Open','Read','ReadWrite')   # 游戏占用时普通读会失败，必须这样开
$sr  = New-Object IO.StreamReader($fs); $text = $sr.ReadToEnd(); $sr.Close(); $fs.Close()
$text -split "`n" | Select-String -Pattern 'Loading \[GodMode|已加载 v1\.3\.0|已挂钩'
```

**成功标准**（必须全部出现）：

```text
[Info   :   BepInEx] Loading [GodMode 1.3.0]
[Info   :   GodMode] [GodMode] 已加载 v1.3.0（统一菜单 + 物品说明 + 图标兜底）
[Info   :   GodMode] [GodMode] 已挂钩: ...   ← 应有 12~15 行不同的钩子
```

钩子清单（名称会随版本略有增减，但下面这些是核心）：

```text
Body.HandleBody(每帧身体模拟)
Body.HandleCirculation(血液循环/脑死亡)
Limb.ImpactDamage(受击入口)
Body.TryLastStand(濒死挣扎)
Body.get_alive(生死判定)
TraderScript.OnWillRenderObject(越界防护)
PlayerCamera.HandleCameraPosition / ConsoleScript.Update / RenderPipelineManager.beginCameraRendering
```

### 4.3 功能验证（无人值守可做的部分）

```powershell
# 心跳行证明主循环在跑（每 5 秒一行）
$text -split "`n" | Select-String -Pattern '\[GodMode\]\[心跳\]' | Select-Object -Last 1
```

出现 `帧=N … 上帝=False 飞行=False` 且 N 在增长 → **插件已正常工作**。

按键功能需要真实按键输入，自动化环境下可用 `keybd_event` 注入，但见 §6 注意事项。

---

## 5. 卸载

```powershell
Get-Process -Name CasualtiesUnknown -ErrorAction SilentlyContinue | Stop-Process -Force
Remove-Item "$game\BepInEx\plugins\GodMode.dll" -Force
Remove-Item "$game\BepInEx\config\casualtiesunknown.godmode.cfg" -Force   # 可选，保留则下次装回来还是这套配置
```

卸载不影响存档（存档在 `%LOCALAPPDATA%Low\Orsoniks\CasualtiesUnknown\`）。

---

## 6. 故障排查表（症状 → 原因 → 处理）

| 症状 | 根因 | 处理 |
| --- | --- | --- |
| 日志里**完全没有** `Loading [GodMode` | DLL 不在 `BepInEx\plugins\`；或 BepInEx 未生效 | 检查 §1.2；确认 `winhttp.dll` 存在；确认是 BepInEx **5**（x64） |
| 日志有 `Loading [GodMode` 但随后一堆 `TypeLoadException` / `MissingMethodException` | 游戏版本与插件不匹配（本插件针对 Assembly-CSharp 的公开成员编译） | 换对应版本的 DLL；上报时附上 `Assembly-CSharp.dll` 版本 |
| 复制 DLL 报「拒绝访问」 | 游戏还开着（DLL 被内存映射） | `Stop-Process -Name CasualtiesUnknown -Force` 后再复制 |
| 装了但按键没反应 | 游戏窗口没焦点；或按键被别的程序吃掉 | 先让游戏窗口获得焦点；看日志有没有 `[GodMode][按键] 收到 KeyCode …` 行 —— **有这行说明键收到了，是功能逻辑问题；没这行说明按键根本没到游戏** |
| 按一次键功能「开→关」各触发一次 | v1.3.0 之前的老 bug（Tick 每帧被调用两次） | 升级到 v1.3.0（已用 `Time.frameCount` 帧门修掉） |
| 生成器窗口打开后角色还在动 | 老版本没有 `PauseWhileOpen` | 升级到 v1.3.0，并确认 `PauseWhileOpen = true` |
| 游戏卡死 / 主线程 100% | 原版 bug：`TraderScript.OnWillRenderObject` 用世界坐标索引 `fluid[,]` 无边界检查，NPC 走出世界就每帧抛 `IndexOutOfRangeException` | 保持 `CrashGuard = true`（本插件已挂前缀拦截并自动把 NPC 拉回世界内）；同时 `WorldMargin` 别设太小 |
| 飞太远后崩溃 | 同上，飞出世界边界触发 | `WorldMargin ≥ 8`；插件已做飞行限位并提示「已到世界边界」 |
| 界面全中文但显示成方块/乱码 | 找不到游戏字体 | 日志会打印 `字体 = …`；正常情况下会用 `Retro Gaming SDF` |

---

## 7. 版本与发布

| 版本 | 说明 | 资产 |
| --- | --- | --- |
| **v1.3.0** | 最新正式版：统一主菜单（F1）+ F1~F10 满键位 + 物品原文说明 + 图标兜底 + 拖出可见性修复 | `GodMode-v1.3.0.dll`，89600 字节 |
| **v1.2.0** | 稳定留存版（无统一菜单，旧 F1~F9 键位） | `未知伤亡-上帝模式-v1.2.0.zip`，114328 字节 |

下载地址：<https://github.com/xianyun66666/Casualties-Unknown-GodMode/releases>

**AI 选择规则**：
- 要「最新功能、统一菜单、物品说明」→ 用 **v1.3.0**（单 DLL，直接复制）。
- 要「保守稳定、不想动键位、不介意旧键位」→ 用 **v1.2.0**（zip 内含 DLL + 说明书）。

---

## 8. 给代理的硬约束（别再犯）

1. **不要在游戏运行时覆盖 DLL** —— 一定先 `Stop-Process`。
2. **不要用「文件是否可写」判断能否覆盖** —— 内存映射锁不影响这种探测，会误判。
3. **判断状态一律以 `BepInEx\LogOutput.log` 为准**，不要靠截图猜。截图只能证明「画面长什么样」。
4. **日志文件被游戏占用**，直接 `Get-Content` 可能抛「另一个进程正在使用此文件」，必须用 §4.2 的 `FileShare` 方式读，或先拷一份。
5. **不要往用户正在操作的游戏窗口注入按键** —— 会和用户真实按键冲突，导致状态错乱（历史事故：注入的 F1 把用户的上帝模式关掉了）。
6. **改键位后必须重启游戏** —— cfg 只在启动时读一次。
7. 本项目的界面、日志、文档**全部是中文**，不要翻译成英文再写回文件。

---

## 9. 从源码构建（仅在需要时）

```powershell
# 编译器：.NET Framework 自带 csc（不是 dotnet build）
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' `
    "@D:\AI_Tools\deepseek harness\GodModeMod\build-official.rsp"
```

- `build-official.rsp` 里已经写好了全部 `/reference:`（73 个程序集，含 `netstandard.dll` 和所有 `UnityEngine*.dll`）。
- 缺失 `netstandard.dll` 会报 `CS0012: 类型"System.Object"在未引用的程序集中定义`。
- 语言版本锁 **C# 5**（`/langversion:5`），不要用 `var` 之外的现代语法糖。
- 输出直接覆盖 `BepInEx\plugins\GodMode.dll`，所以**编译前同样必须先关游戏**。
