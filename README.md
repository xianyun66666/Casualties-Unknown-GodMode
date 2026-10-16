# GodMode —— 《Casualties: Unknown》上帝模式模组

一个 [BepInEx 5](https://github.com/BepInEx/BepInEx) 插件：无敌不死、上天入地飞行、技能拉满、清除全部负面状态、传送、全图照亮，外加一个可以**搜索并生成游戏里任何东西**的窗口（物品 / 生物 / 怪物 / 动物 / 植物 / 建筑 / 方块），而且**生成位置由你自己指定**。

**中文** · [English](README.en.md)

---

> ### ⚠️ 请先读这一段
> - **仅限单人游戏。** 这是个作弊模组，请不要在联机 / 合作模式里使用。
> - **由本模组引起的崩溃、异常、存档损坏，是模组的责任，不是游戏作者的。** 请不要把这些报给 Orsoniks。
> - 游戏还在 **beta**，而且**一定会更新**。游戏更新可能让任何模组失效，包括这个。见 [兼容性](#兼容性)。
> - 本仓库**不包含任何游戏文件**。这里只有源码，和一个在运行时给游戏打补丁的插件。你需要自己拥有这款游戏。

---

## 功能

| 按键 | 功能 | 说明 |
|:---:|---|---|
| `F1` | **上帝模式** | 不会死。血量、血液、氧气、饥饿、口渴、体力、体温、全肢体持续满值；流血、感染、骨折、脱臼、疼痛等持续清除。 |
| `F2` | **上天入地** | 自由飞行、穿墙。按住 `LeftShift` 加速 4 倍。靠近世界边界会被挡住，防止飞出去把游戏搞崩。 |
| `F3` | **技能拉满** | 力量 / 抗性 / 智力 直接拉满。 |
| `F4` | **清除负面状态** | 完全治疗，**并且**清掉全部负面状态。 |
| `F5` | **传送** | 传送到鼠标位置。 |
| `F6` | **全图照亮** | 整个地图点亮。 |
| `F7` | **初始装备包** | 发放开局装备。 |
| `F8` | **生成万物** | 打开生成窗口，见下。 |

所有按键都能改，所有功能都能单独关掉。

### 生成窗口（F8）

生成**游戏里的任何东西**。清单是运行时从游戏自己的注册表里读出来的 —— 所以游戏更新加了新物品、新怪物，**不用更新模组就会自动出现**。

- **大分类** —— 全部 / 物品 / 生物 / 植物 / 建筑 / 方块。物品再按游戏自己的物品分类分子类，用 `◀ ▶` 或 `Tab` 切换。
- **搜索** —— 按 `/` 直接打字。
- **生成位置** —— 三档：角色脚下 / 鼠标处 / **自己选的点**。选点模式下点世界里任意位置，屏幕上会有标记点实时显示世界坐标，还能用方向键微调（按住 `Shift` 走 0.25 格）。所有生成位置都会被夹进世界范围内。
- **批量生成** —— 「生成全部筛选结果」/ 随机 5 / 随机 20，按网格铺开、每帧生成几个。
- **真实名字** —— 显示名取自游戏自己的本地化表，你看到的是「天花板垂麦」而不是 `ceilingrye`。
- **物品耐久 / 自动拾取** 可以在生成前调。

完整操作说明见 [docs/USAGE.md](docs/USAGE.md)。

---

## 安装

**需要**

- 《Casualties: Unknown》（Steam appid `4576510`）
- [BepInEx 5.4.23.x **x64**](https://github.com/BepInEx/BepInEx/releases) —— 要 64 位版

**步骤**

1. **装 BepInEx。** 下载 `BepInEx_win_x64_5.4.23.*.zip`，把里面的东西解压到游戏根目录（Steam 里：右键游戏 → *管理* → *浏览本地文件*）。解压完游戏 exe 旁边应该有 `winhttp.dll` 和一个 `BepInEx` 文件夹。
2. **启动一次游戏再退出。** 这一步让 BepInEx 建好 `BepInEx\plugins\` 和 `BepInEx\config\`。
3. **装模组。** 把 `GodMode.dll` 放进：
   ```
   <游戏根目录>\BepInEx\plugins\
   ```
4. **进游戏。** 读档进到游戏世界里，按 `F8`，左上角应该出现窗口。

> **升级时注意**：覆盖 `GodMode.dll` 之前**必须先把游戏完全退出**。游戏运行时这个 DLL 被内存映射锁住，Windows 会拒绝复制并报「文件正被另一个进程使用」—— 这是正常的，不是下载坏了。

**Linux / Steam Deck（Proton）：** 在游戏的启动选项里加：
```
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

### 怎么确认装上了

打开 `<游戏根目录>\BepInEx\LogOutput.log`，搜 `GodMode`。正常应该是：

```
[Info   :BepInEx] Loading [GodMode 1.0.0]
[Info   :GodMode] [GodMode] 已挂钩: PlayerCamera.Update(每帧驱动)
...  （一共 12 行 已挂钩）
[Info   :GodMode] [GodMode] 已加载 v1.0.0
[Info   :GodMode] [GodMode] 看门狗已启动
[Info   :GodMode] [GodMode][生成器] 已载入：物品 284 + 实体 102 + 方块 36 = 422 种
[Info   :GodMode] [GodMode][界面] uGUI 窗口已创建，字体 = ...
```

- **一行 `[GodMode]` 都没有** → DLL 位置不对，或 BepInEx 没装好。
- **出现 `挂钩失败` / `找不到方法，跳过`** → 游戏更新导致方法签名变了，模组需要修。请带上日志[开一个 issue](../../issues)。

---

## 配置文件

第一次启动后自动生成在：

```
<游戏根目录>\BepInEx\config\casualtiesunknown.godmode.cfg
```

常用项：

```ini
[Keys]
GodMode = F1                 # 这里可以改键
FlyNoClip = F2
MaxSkills = F3
FullHeal = F4
TeleportToMouse = F5
Fullbright = F6
StarterKit = F7
ItemSpawner = F8

[Options]
StartWithGodMode = true      # 开局自动开上帝模式
StartWithFly = false
ShowHud = true               # 左上角状态字
Watchdog = true              # 看门狗：主线程卡死超 8 秒自动抢救
CrashGuard = true            # 崩溃防护：把跑到世界外的 NPC 拉回来
WorldMargin = 12             # 飞行时离世界边界留几格

# ---- 生成窗口（F8）----
SpawnPosMode = 2             # 0=角色脚下 1=鼠标处 2=自己选的点
SpawnCondition = 1           # 生成物品的耐久度 0~1
SpawnAutoPickup = true
SpawnBatchPerFrame = 15
SpawnMaxBatch = 800
PauseWhileOpen = true        # 开窗口时冻住游戏时间
DumpInventory = true         # 导出生成物清单，用来报分类错误
```

> 想报**分类错了**：保持 `DumpInventory = true`，打开一次窗口，然后把
> `<游戏根目录>\BepInEx\config\GodMode.生成物清单.log`（格式 `分类 | prefab名 | 身上的组件`）发过来。

---

## 从源码编译

需要装好游戏（编译器要引用它的程序集），以及 Windows PowerShell 5.1+ / PowerShell 7。不需要 SDK、不需要 NuGet、不需要 Visual Studio —— 用的是 Windows 自带的 C# 编译器。

```powershell
git clone https://github.com/xianyun66666/Casualties-Unknown-GodMode.git
cd <repo>
pwsh -File .\build.ps1
```

脚本会通过 Steam 注册表和 `libraryfolders.vdf` 自动找到游戏。也可以手动指定：

```powershell
pwsh -File .\build.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
# 或者设一次环境变量：
setx CASUALTIES_UNKNOWN_PATH "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
```

默认会直接写进 `BepInEx\plugins\`。**游戏开着的时候写不进去** —— 这时用暂存目录，等游戏关了再复制过去：

```powershell
pwsh -File .\build.ps1 -OutPath .\dist\GodMode.dll
```

编译目标刻意定在 **C# 5**（`/langversion:5`），因为用的是 .NET Framework 那个老的 `csc.exe`。所以：不能用 `?.`、不能用字符串内插、不能用表达式体成员、不能用 `nameof`。

---

## 兼容性

| | |
|---|---|
| 游戏 | 《Casualties: Unknown》（Steam appid `4576510`） |
| 实测版本 | build `24774057`（beta） |
| BepInEx | 5.4.23.4（5.4.23.x 应该都行） |
| Unity | 2022.3.62，Mono |
| 平台 | Windows x64（Linux / Proton 未测但理论上可用） |

**这个模组对版本敏感。** 它的原理是给游戏程序集里**指定名字的方法**打补丁，所以游戏更新一旦改了这些方法的名字或结构，个别功能就会失效。失效是「软」的：日志里写一行，那个功能停用，其它功能照常。真遇到了请带上 `LogOutput.log` 开 issue。

模组只**读取**游戏的本地化表和对象注册表，不重新分发任何游戏内容。

### 已知限制

- 分类是**根据对象身上挂了哪些组件推断**的，所以必然有个别条目分类不准。欢迎附上导出的清单来报错。
- 「方块」类是往世界里**砌**方块（那是一堵墙），不是掉落物。
- 生成窗口打开时会冻住 `Time.timeScale`（可配置），免得点界面的时候角色也跟着挥拳。

---

## 致谢

- **Orsoniks** —— 《Casualties: Unknown》。所有游戏内容都归他们。
- **[BepInEx](https://github.com/BepInEx/BepInEx)** 和 **[HarmonyX](https://github.com/BepInEx/HarmonyX)** —— 本模组赖以构建的框架。
- 界面的命中测试写法参考了社区其它模组使用的游戏控制台模式。

## 许可

[MIT](LICENSE) —— 代码随便用。游戏本身不在此许可范围内，也不在这里分发。
