# GodMode —— 《Casualties: Unknown》上帝模式模组

一个 [BepInEx 5](https://github.com/BepInEx/BepInEx) 插件：无敌不死、上天入地飞行、技能拉满、清除全部负面状态（**含药物过量与戒断**，断掉的手脚也能治好到能开枪）、传送、全图照亮、**一击必杀**，外加一个可以**搜索并生成游戏里任何东西**的窗口（物品 / 生物 / 怪物 / 动物 / 植物 / 建筑 / 方块）—— **分类细分到能直接找到**、**生成位置由你自己指定**、**按键在窗口里就能改**、**开窗口时角色不会乱动**。

**中文** · [English](README.en.md)

**⬇️ [下载最新版](https://github.com/xianyun66666/Casualties-Unknown-GodMode/releases/latest)**（压缩包：插件 + 图文安装说明 + 使用手册 + 源码） · [安装步骤](#安装)

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
| `F4` | **清除负面状态** | 完全治疗，**并且**清掉全部负面状态 —— 流血、感染、骨折、脱臼、疼痛、**药物过量 / 戒断 / 兴奋剂 / 咖啡因**，连被砍掉的手脚都会重新接回去（不然治好了也拿不起枪）。 |
| `F5` | **传送** | 传送到鼠标位置。 |
| `F6` | **全图照亮** | 整个地图点亮。 |
| `F7` | **初始装备包** | 发放开局装备。 |
| `F8` | **生成万物** | 打开生成窗口，见下。 |
| `F9` | **一击必杀** | 打一下就秒掉任何东西：生物 / 怪物 / 植物 / 建筑 / 方块。 |

所有按键都能改 —— **在窗口里点「键位设置」就能改**（见下），也可以手改配置文件。所有功能都能单独关掉。

### 一击必杀（F9）

按 `F9` 打开，然后随便打一下就秒：

| 打的东西 | 结果 |
|---|---|
| 生物 / 怪物 / 蜘蛛 / 商人 | 一下死 |
| 植物（垂麦、仙人掌、发光菇…） | 一下碎 |
| 建筑 / 设施（门、工作台、充电站…） | 一下碎 |
| 方块（岩石、矿脉、沙砾…） | 一下碎 |

近战、枪械、空手都算 —— 只要伤害是你按「攻击键」打出去的。

实现上只挂了一个 Harmony 前缀，挂在 `Body.Attack(AttackInfo atk, int slot)` 上。这个方法
是玩家一切攻击的唯一收口（`PlayerCamera.HandleAttacks` → `Body.UseItemInHand` → `Body.Attack`），
它内部按命中物分三条路：方块走 `WorldGeneration.DamageBlock(atk.structuralDamage)`，建筑走
`BuildingEntity.health -= atk.structuralDamage`，生物走 `BuildingEntity.health -= atk.damage`。
所以前缀里把两个伤害数乘一个倍率（默认 `100000`）就够了 —— **一个钩子覆盖方块 / 建筑 / 生物**。

- **只对你生效**：NPC 互殴、野兽咬野兽仍是原版数值，世界不会自己崩坏。
- 倍率可在配置里调，**但别调过一百万**：伤害数字走 `(int)` 转换，太大会溢出成负数。

### 键位设置

生成窗口第一排有 **「键位设置」**，第二排有 **「重置键位」**。

- 左键点一行 → 那一行变成 `◀ 请按下新的按键…` → 按一下新键 → 立刻生效并写回 cfg。
- 9 个功能全都能改：上帝模式 / 上天入地 / 技能拉满 / 清除负面状态 / 传送到鼠标 / 全亮 / 初始装备包 / 生成万物 / 一击必杀。
- **键位撞了不会拦你，但会明确标出来**：那一行变红、末尾写 `⚠ 与「XXX」冲突`。撞车的两个功能会同时触发（Unity 本来就是这样），再点那行换一个就好。
- **改键时会临时屏蔽功能热键**，否则你把「上帝模式」改成 `F1` 的那一瞬间就会顺手开关一次。
- 插件不碰游戏自带的键位系统（`Settings` / `KeyBinds` / `SettingKeybind`），用自己的 cfg 存，不动你的存档。

### 生成窗口（F8）

生成**游戏里的任何东西**。清单是运行时从游戏自己的注册表里读出来的 —— 所以游戏更新加了新物品、新怪物，**不用更新模组就会自动出现**。

- **先选大类，再看分类格子** —— 第一排是 全部 / 物品 / 生物 / 植物 / 建筑 / 方块 / 其它。点一个分组就直接进「分类总览」，每个格子是一个具体分类并标着件数，点哪格就只留那类东西；第二排的分类标签随时点回总览。
- **细到能用的程度** —— 物品分 弹药 / 穿戴（护甲·服装）/ 武器（枪械·近战）/ 医疗（包扎·药品·药物）/ 食物 / 饮料 / 容器 / 垃圾 / 工具 / 材料；生物分 怪物 / 商人 / 动物 / NPC；植物分 蘑菇 / 树木 / 藤蔓 / 草丛 / 作物 / 花藻；建筑分 门墙 / 工作台 / 电力 / 存储 / 医疗 / 防御 / 家具 / 水晶；方块按材质分七类。分类以组件类型为先、名字关键词兜底 —— 不会再把沙玫瑰当建筑、把显现体当装饰水晶。
- **搜索** —— 按 `/` 直接打字。**一旦搜索就无视分组和分类**，整个清单一起搜，所以在「物品」里搜怪物名也能搜到。
- **开窗口时角色不会乱动** —— 打开窗口会冻结玩家操作（移动 / 瞄准 / 攻击一起挡）。想边开菜单边走路，把 `FreezeInputWhileOpen` 设成 `false`。
- **生成位置** —— 三档：角色脚下 / 鼠标处 / **自己选的点**。选点模式下点世界里任意位置，屏幕上会有标记点实时显示世界坐标，还能用方向键微调（按住 `Shift` 走 0.25 格）。所有生成位置都会被夹进世界范围内。
- **批量生成** —— 「生成全部筛选结果」/ 随机 5 / 随机 20，按网格铺开、每帧生成几个。
- **真实名字** —— 显示名取自游戏自己的本地化表，你看到的是「天花板垂麦」而不是 `ceilingrye`。
- **物品耐久 / 自动拾取** 可以在生成前调。

完整操作说明见 [docs/USAGE.md](docs/USAGE.md)。

---

## 安装

**下载**

不想自己编译，就下打包好的：[**Releases → 最新版**](https://github.com/xianyun66666/Casualties-Unknown-GodMode/releases/latest) —— 压缩包里有 `GodMode.dll`、图文安装说明、完整使用手册和源码。想自己编译见[从源码编译](#从源码编译)。

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
[Info   :BepInEx] Loading [GodMode 1.2.0]
[Info   :GodMode] [GodMode] 已挂钩: PlayerCamera.Update(每帧驱动)
...  （一共 17 行 已挂钩）
[Info   :GodMode] [GodMode] 已加载 v1.2.0
[Info   :GodMode] [GodMode] 看门狗已启动
[Info   :GodMode] [GodMode][生成器] 已载入：物品 284 + 实体 … + 方块 … = … 种
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
GodMode = F1                 # 这里可以手改键，也可以在窗口里点「键位设置」改
FlyNoClip = F2
MaxSkills = F3
FullHeal = F4
TeleportToMouse = F5
Fullbright = F6
StarterKit = F7
ItemSpawner = F8
OneHitKill = F9

[Options]
StartWithGodMode = true      # 开局自动开上帝模式
StartWithFly = false
ShowHud = true               # 左上角状态字
Watchdog = true              # 看门狗：主线程卡死超 8 秒自动抢救
CrashGuard = true            # 崩溃防护：把跑到世界外的 NPC 拉回来
WorldMargin = 12             # 飞行时离世界边界留几格
OneHitMultiplier = 100000    # 一击必杀倍率（别超过 1000000，飘字会溢出成负数）

# ---- 生成窗口（F8）----
SpawnPosMode = 2             # 0=角色脚下 1=鼠标处 2=自己选的点
SpawnCondition = 1           # 生成物品的耐久度 0~1
SpawnAutoPickup = true
SpawnBatchPerFrame = 15
SpawnMaxBatch = 800
PauseWhileOpen = true        # 开窗口时冻住游戏时间
FreezeInputWhileOpen = true  # 开窗口时冻结玩家操作（移动/瞄准/攻击），想边开边走就设 false
DumpInventory = true         # 导出生成物清单，用来报分类错误
```

> 想报**分类错了**：保持 `DumpInventory = true`，打开一次窗口，然后把
> `<游戏根目录>\BepInEx\config\GodMode.生成物清单.log`（格式 `分类 | id | 显示名 | 附带信息`）发过来。

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

[MIT](LICENSE) —— 代码随便用，保留版权声明即可，作者不负责。
（想看大白话版：[许可证中文说明](LICENSE-中文说明.md)。游戏本身不在此许可范围内，也不在这里分发。）
