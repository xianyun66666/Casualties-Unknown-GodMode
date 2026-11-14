# Casualties Unknown — 上帝模式 God Mode  v1.3.0

一个 BepInEx 5 插件：让角色「上天入地，无所不能」，并且**顺手修掉几个会让游戏卡死的原版 bug**。

---

## 一、已经装好了

插件已编译并放进游戏插件目录，**不需要你再做任何安装操作**：

```
C:\Users\Administrator\Desktop\CasualtiesUnknown\BepInEx\plugins\GodMode.dll
```

启动游戏即自动加载（BepInEx 5.4.23.5 已就位）。官方版与 Demo 版通用。

---

## 二、按键功能

| 按键 | 功能 | 说明 |
|------|------|------|
| **F1** | **统一主菜单 开/关** | 右侧弹出一个面板，**9 个功能全排在里面，点按钮就能用**，右边实时显示「开 / 关」。不想记键位就只按 F1。 |
| **F2** | **上帝模式 开/关** | 无敌。脑健康/血量/血氧/血压/饥饿/口渴/耐力/精力全部锁满，无出血、无疼痛、无感染、无休克，四肢皮肤与肌肉健康锁满，骨折/脱臼/断肢自动复原。屏幕左上角常驻 `★ 上帝模式 ON`。 |
| **F3** | **上天入地 开/关** | 原版 noclip 飞行 + 穿墙。用 **WASD 自由飞**，按住 **Shift** 加速 4 倍。**会被世界边界挡住**（飞出去会让游戏崩溃，见第五节）。 |
| **F4** | 技能拉满 | 力量/抗性/智力 设为 **30 级**（可在配置里改，安全上限 35）。 |
| **F5** | **清除全部负面状态** | 一次搞定：伤势 + **情绪/精神/中毒/寒冷/肮脏/超重/失眠…** 屏幕上所有「坏图标」全部清空（见第六节）。不需要开上帝模式。 |
| **F6** | 传送到鼠标 | 把角色瞬移到鼠标指针所在位置。 |
| **F7** | 全亮 开/关 | 游戏原版 fullbright，黑暗里也能看清。 |
| **F8** | 领取初始装备包 | 游戏原版 starterkit。 |
| **F9** | **生成万物 开/关** | 弹出**创造模式图标网格**（见第七节）：一格一个图标，**按住图标拖到世界里松手就生成在那儿**。物品 / 生物 / 怪物 / 植物 / 建筑 / 方块全都有，分好类；**鼠标放上去就能看到游戏原文的物品说明**。 |
| **F10** | **一击必杀 开/关** | 打一下就秒掉任何东西：**生物 / 怪物 / 植物 / 建筑 / 方块** 全是一下没（见第八节）。 |

> 上面 **F1~F10 十个键位全部用满，没有空键**。每个键**都能自己改**，而且**就在窗口里改**，不用去翻配置文件 —— 按 **F9** 打开窗口，点右上角的 **「键位设置」**（见第九节）。

**上帝模式与飞行是两个独立开关**，可以只开飞行不开无敌（想自由探索但保留难度），也可以只开无敌不开飞行。

### ★ v1.3.0 新增：统一主菜单

按 **F1** 右边弹出一个面板，**9 个功能全在里面**：

```
上帝模式 · 统一菜单
点按钮 = 使用功能    F1 关闭
F2   上帝模式        关
F3   上天入地        关
F4   技能拉满      点一次
F5   清除负面状态  点一次
F6   传送到鼠标    点一次
F7   全亮            关
F8   初始装备包    点一次
F9   生成万物        关
F10  一击必杀        关
F1   关闭菜单         >
```

点一行就用一次该功能，右边那列实时跟着变（开/关）。**不想记键位的话，平时只按 F1 就够了。**

### ★ v1.3.0 新增：每个物品都带游戏原文说明

生成器网格下方多了一条**说明栏**：鼠标放到哪一格，就显示那件东西的**游戏自带说明**。

- 物品 —— `ItemInfo.description` 原文，例如「叶片餐」→「一份还不错的食物，由爽脆的植物纤维和简单的馅料制成。保质期不长且不含脂肪，但非常饱腹。」
- 实体 —— 显示它是什么对象、身上挂了哪些组件、属于哪个分类
- 方块 —— 显示方块 id、分类、硬度、是否金属/湿滑/高温

### ★ v1.3.0 新增：图标兜底 + 拖出来就能看见

- **幸存者 / 员工 / 守卫**这类类人 NPC 根节点上根本没有 SpriteRenderer（身体是运行时按肢体拼的），以前取不到图就是一片空白格子。现在取不到就**按分类画剪影**（人形 / 叶子 / 房子 / 方块 / 问号），保证每格都有图标。
- 拖出来的东西会做一次**可见性修复**：把它自己和所有子物体里被关掉的渲染器重新打开，不会出现「拖出来什么都没有」。

### ★ v1.3.0 更新：小箭头 · 回车生成 · 生成即满弹 · 无限弹药

**右上角常驻一个小箭头 `>` / `<`**：点它 = 开关主菜单，**按住它就能拖到任意位置**；菜单面板本身也能拖标题栏移动。

生成器里**不再点一下就生成**了（容易误触），改成三种：

| 操作 | 结果 |
| --- | --- |
| 左键点图标 | 只选中，不生成 |
| 选中/悬停 + **回车** | 生成在**鼠标位置** |
| 按住图标拖到世界里松手 | 生成在**鼠标落点**（位移要超过 20px 才算拖） |

**枪 / 弹匣 / 子弹盒拖出来就是满的**：`hasMag=true`、`roundsInMag=magCapacity`、`safe=false`，弹药类 `rounds=maxRounds`。
主菜单新增第 10 项 **无限弹药**：开了以后每 0.25 秒把你自己的枪和弹药补满（NPC 的不动）。

**类人 NPC 补图**：`guard`（幸存者/员工/守卫）根节点没有 SpriteRenderer，身体是 `GuardScript.myBody` / `armSprite` 指向的子渲染器，生成后由 `FixHumanoid()` 打开它们、`sprite` 为空时补 `bodyStaySprite`/`armsNormal`，不再是空白一片。

---

## 三、配置文件

首次启动游戏后会自动生成：

```
E:\steam\steamapps\common\Casualties Unknown Demo\BepInEx\config\casualtiesunknown.godmode.cfg
```

```ini
[Keys]
Menu = F1                 # 统一主菜单
GodMode = F2              # 上帝模式
FlyNoClip = F3            # 上天入地
MaxSkills = F4
FullHeal = F5             # 清除全部负面状态(伤势+情绪+中毒…)
TeleportToMouse = F6
Fullbright = F7
StarterKit = F8
ItemSpawner = F9          # 生成万物窗口
OneHitKill = F10          # 一击必杀

[Options]
ConfigVersion = 1.3.0     # 模组自己维护，改了会被重写键位
StartWithGodMode = false  # 改成 true = 一进游戏就自动无敌
StartWithFly = false      # 改成 true = 一进游戏就自动能飞
ShowHud = true            # 左上角状态提示
DebugLog = true           # 写诊断日志(心跳/按键扫描)
WorldMargin = 12          # 飞行限位：离世界边界留多少格
CrashGuard = true         # 崩溃防护：修正跑到世界外的 NPC
MaxSkillLevel = 30        # F4 设成几级（安全上限 35）
SkillGuard = true         # 技能防护：阻止技能等级/经验溢出导致的卡死
Watchdog = true           # 看门狗：主线程卡死超过8秒时强制抢救
ClearMoodAndMind = true   # F5 是否连情绪/精神类(抑郁、精神空洞)一起清除
OneHitMultiplier = 100000 # 一击必杀的伤害倍率（别调过百万，飘字会溢出成负数）

# ---- 生成万物窗口（F9）----
SpawnPosMode = 2          # 生成位置：0=角色脚下 1=鼠标处 2=自己选的点（推荐 2）
SpawnCondition = 1        # 生成的物品耐久度 0~1
SpawnAutoPickup = true    # 生成在附近(2格内)时自动捡起
SpawnBatchPerFrame = 15   # 每帧生成几个（太大可能卡顿）
SpawnMaxBatch = 800       # 一次「生成全部」最多生成几个
PauseWhileOpen = true     # 打开生成器时冻住游戏时间，避免点界面时角色也挥拳/用道具
DumpInventory = true      # 导出「生成物清单.log」到 BepInEx\config，用来核对分类
```

按键名用 Unity 的 KeyCode 写法：`F1`、`G`、`Keypad1`、`LeftAlt`、`BackQuote` 等。

> 不过**你基本不用手改这里**：按键在游戏里就能改（F8 → 「键位设置」），改完它会自己写回这个文件。

---

## 四、它是怎么实现的（技术说明）

逆向 `Assembly-CSharp.dll` 得到的关键事实，全部经 IL 验证：

- **生死判定**：`Body.alive` 属性的实现就是 `brainHealth > 0`（只有 getter，没有 setter）。所以只要脑健康不为 0，角色就不会死。
- **每帧身体模拟**：`Body.HandleBody(Painkillers)` 是身体总模拟。`Body.HandleCirculation` 负责血液循环与脑死亡。
- **受击入口**：`Limb.ImpactDamage(float, Vector2)`。
- **满值常量**：直接取自 `Body..ctor` / `Limb..ctor`：`brainHealth/bloodVolume/bloodOxygen/hunger/thirst/stamina/energy/consciousness = 100`、`bloodPressure = 120`、`heartRate = 70`、`temperature = 37`、`Limb.skinHealth/muscleHealth = 100`。
- **上天入地**：原版 `noclip` 命令的 IL 逻辑是「`noClip` 取反 → 把当前位置存进 `noClipPos` → `body.rb.simulated = false`」，之后由 `ConsoleScript.Update` 每帧执行 `noClipPos += body.moveDir * deltaTime * 20 * (按住Shift?4:1)`。而 `moveDir` 由 `PlayerCamera.HandleInput` 以引用方式写入，与是否落地无关 —— 所以空中照样能操控方向。

插件用 Harmony 挂了 **12 个钩子**：

### 玩家无敌（6 个）

| 挂钩目标 | 方式 | 作用 |
|---------|------|------|
| `Body.HandleBody` | 前置 + 后置 | 模拟前后各恢复一次满状态，让内部死亡判定永远看到健康值 |
| `Body.HandleCirculation` | 前置 | 同上，堵住脑死亡路径 |
| `Limb.ImpactDamage` | 前置返回 false | 玩家肢体**直接免疫伤害**，连断肢都不会发生 |
| `Limb.Dismember` | 前置返回 false | **v1.2.1 新增**：上帝模式开着时不让玩家肢体被砍断（NPC 照常，正常玩法不受影响） |
| `Body.TryLastStand` | 前置返回 false | 屏蔽濒死挣扎 |
| `Body.get_alive` | 后置 | 对玩家强制返回 true，彻底堵死死亡分支 |

### 每帧驱动（2 个挂钩）

| 挂钩目标 | 方式 | 作用 |
|---------|------|------|
| `ConsoleScript.Update` | 后置 | **主驱动**：每帧跑一次插件的逻辑（按键、恢复、限位、界面刷新与鼠标命中测试） |
| `PlayerCamera.Update` | 后置 | 备用驱动，防止控制台不存在时逻辑停摆 |

> **⚠ 为什么插件自己的 `Update` 不能用**：这个游戏 + BepInEx 的组合下，**插件自己那个组件的 `Update` 和 `OnGUI` 永远不会被 Unity 调用**。心跳日志里「驱动(插件Update/…)=**0**/…」跑了近两万帧都是 0，这是硬证据。最早表现为「按 F1 完全没反应」，改成挂在游戏的 `ConsoleScript.Update` / `PlayerCamera.Update` 上绕开。
>
> 中间还试过用 `new GameObject` + `AddComponent` 自建一个组件来承载 `Update`/`OnGUI`，但日志证明**自建组件的 `Update` 同样是 0**，所以 `OnGUI` 也不触发 —— IMGUI 全部失效。最终方案见第七节：界面改用 **uGUI**（Canvas 渲染不依赖组件 Update），交互放进上面这两个挂钩里手工做命中测试。

### 修原版 bug（5 个，见下一节）

| 挂钩目标 | 方式 | 作用 |
|---------|------|------|
| `TraderScript.OnWillRenderObject` | 前置返回 false | 交易商越界时跳过，避免渲染阶段刷异常卡死 |
| `Skills.GetExperienceForLevel` | 前置 + 后置 | 经验曲线浮点溢出防护 |
| `Skills.UpdateExpBoundaries` | 后置 | 经验上限被算成负数时修正 |
| `Skills.CheckForLevelUp` | 前置 | 等级越界时拉回安全值，堵死死循环 |
| `Skills.AddExp` | 前置 | 经验值越界时压回安全值（这是唯一的入口，堵住它就堵死了全部路径） |

**加上看门狗线程**：主线程连续 8 秒没有任何帧推进，就判定为卡死，从后台线程强行把技能数值拉回安全值并写一行诊断日志到 `BepInEx\config\GodMode.卡死诊断.log`。

所有无敌挂钩只对**玩家自己**生效（判定 `== PlayerCamera.main.body`），不会影响 NPC 和动物。

---

## 五、顺手修掉的三个原版 bug（重要）

### bug 1：交易商跑到世界外 → 渲染管线每帧抛异常

`TraderScript.OnWillRenderObject` 里这行：

```csharp
Vector2Int v = WorldGeneration.world.WorldToBlockPos(transform.position + Vector3.up);
FluidManager.main.fluid[v.x, v.y] = 0;   // ← 没有边界检查
```

而游戏自己的 `FluidManager.SetLiquid` 是带 `Math.Clamp(x, 0, world.width - 1)` 的 —— 说明游戏知道要夹紧，只有这条路径漏写了。这个函数是在**渲染剔除阶段**（`ScriptableRenderContext.Internal_Cull`）被回调的，在 native→托管→native 边界抛托管异常会把整条渲染管线卡住，主线程随后 100% 空转。

**修法**：越界的商人直接跳过这个方法（不执行就不会抛异常），同时登记起来，回到正常的 Update 上下文里把它**连目标点 `desiredPos` 一起**拉回世界内。

### bug 2：技能等级 ≥ 42 时经验曲线浮点溢出 → 无限循环卡死

`Skills.GetExperienceForLevel(level)` 是个 O(level) 循环，步长按 `×1.18`（20 级后 ×1.15，30 级后 ×2）增长：

| 等级 | 所需总经验 |
|---|---|
| 30 | 54,632 |
| 40 | 7.61e8 |
| **42** | **5.60e9 ← 首次超过 int.MaxValue** |
| 100 | 7.91e34 |
| **109** | **Infinity** |

`Mathf.RoundToInt(溢出值)` 会返回负数，于是 `Skills.CheckForLevelUp` 里的

```csharp
while (xp >= (float)xpToLevel) { level++; UpdateExpBoundaries(); }
```

**条件永远成立 → 主线程死循环 → 游戏卡死**（而且每轮还要跑一次 O(level) 的循环，越转越慢）。

任何一次获得经验都会触发它：`Body.Attack`（打 NPC）、`WorldGeneration.IncreaseDepthByLayer`（下一层）、`Recipe.TryMake`（合成）、`Body.Jump`、`Body.HandleBody`……

**修法**：等级钳到 35、经验钳到 1e8、`GetExperienceForLevel` 返回非正数时改成 `int.MaxValue`。

### bug 3（其实是 bug 2 的触发器）：模组自己的 F3 写错了字段

`Skills` 里的 `minSTR/maxSTR` **不是等级上下限，而是「本级到下一级所需的经验值」**。最早的 F3 直接 `STR = maxSTR`（把 60 点经验当成 60 级），一按就把等级推到溢出区。现在改为设置真实等级并重算经验边界。

---

## 六、清除全部负面状态（F4）

按 **F4**，屏幕上**所有「坏图标」一次全部消失** —— 不只是伤势，连情绪、精神、中毒、寒冷、肮脏、超重、失眠这些「不影响战斗但看着烦」的状态也一起清掉。

**具体清掉什么：**

| 类别 | 清掉的状态 | 底层字段 |
| --- | --- | --- |
| 伤势 | 骨折、脱臼、断肢、出血、感染、疼痛、弹片、中风 | `Limb.broken/dislocated/dismembered/bleedAmount/infected/infectionAmount/pain/shrapnel/strokeAffected` |
| **药物** | **过量（`overdose1..4`）、戒断（`withdrawal1..4`）、药物过量（`drugoverdose`）、兴奋（`stimulated`）、咖啡因（`energized`）、专注、濒死恐惧** | **`Painkillers.opiateAmount/opiateTolerance/opiateReception/antagonistAmount/actualOpiateReception`**、`overdoseIndex`、`onHardStimulants`、`caffeinated`、`stimulantMultiplier`、`focusedLevel`、`horrifiedLevel` |
| 内脏 | 内出血、血胸、败血症、休克、肺栓塞、心室颤动、药物过量 | `internalBleeding/hemothorax/septicShock/shock/hasPulmonaryEmbolism/fibrillationProgress/overdoseIndex` |
| 循环 | 脑健康、血量、血氧、血压、心率、呼吸、意识 | `brainHealth/bloodVolume/bloodOxygen/bloodPressure/heartRate/respiratoryRate/consciousness` |
| **情绪** | **抑郁、忧郁、悲惨**（`depression1..4` / `gloomy` / `miserable`） | `happiness`（+100）、`opiateHappiness`、`antidepressantHappiness`、`horrifiedLevel` |
| 精神 | 精神空洞（`hollow`）、脑损伤 | `mindWipe.active = false`、`strokeAmount = 0` |
| 中毒 | 中毒（`venom1..4`）、疾病（`sick1..4`）、辐射病 | `venomCurrent`、`venomTotal`、`sicknessAmount`、`radiationSickness` |
| 环境 | 寒冷/炎热（`cold1..4`/`hot1..4`）、潮湿（`wet1..4`）、肮脏（`dirty1..2`） | `temperature = 37`、`tempDiffFromNormal`、`wetness`、`snowAmount`、`dirtyness` |
| 生理 | 饥饿、口渴、疲倦、超重/偏瘦、失眠、听力损伤、爪子损伤、免疫低下 | `hunger`、`thirst`、`energy`、`weightOffset`、`badSleepAmount`、`hearingLoss`、`clawHealth`、`immunity` |
| 外观 | 毁容（`rippedjaw`）、独眼/失明（`rippedeye`/`rippedeyes`） | `disfigured`、`eyeGone`、`bothEyesGone` |

**保留不动的（这些是「好图标」）：** 快乐（`happy1..4`）、精力充沛（`energized`）、高免疫（`highimmunity`）、肾上腺素（`adrenaline`/`fightorflight`）、角质促进（`keratinbooster`）。

**★ 断掉的手脚会重新接回去（v1.2.0 修，v1.2.1 补好了后遗症）：**

`Limb.Dismember()` 砍肢体的时候不只是把 `dismembered` 置 `true`，还会
`gameObject.SetActive(false)` —— **整个肢体对象被停用了**。以前的 F4 只把标志位改回来、
健康值拉满，却没重新启用那个对象，于是手臂在引擎层面根本不存在：
治好了也举不起枪、近战也软绵绵。现在 F4 会一并 `SetActive(true)`，并把
`broken` / `dislocated` / `splinted`、断骨与脱臼计时器一起清掉。

另外，**上帝模式开着的时候，玩家的肢体根本不会被砍断** —— v1.2.1 加了第 17 个钩子
`Limb.Dismember`，上帝模式期间直接跳过这个方法（NPC 照常被砍断，正常玩法不受影响）。
与其事后补救，不如不让它发生。

**★ 按 F4 之后那根白条（v1.2.1 修）／跳得比平时高（v1.2.2 修）／角色自己走（v1.2.3 修）：**

这两个都是 v1.2.0 的「清药物 + 接回断肢」留下的副作用 —— **是两个互不相干的原因**：

- **白条**不是特效，是游戏画骨折的那根骨头精灵。它叫 `BrokenBone`，由
  `Limb.MakeBoneSprite()` 造出来、挂在肢体下面，**只有 `Limb.MendBone()` 会删它**，
  而 `MendBone()` 第一行就是 `if (!broken) return;`。v1.2.0 直接写 `l.broken = false`，
  等于把记录抹了、骨头还留在身上；又因为断肢时整条肢体被停用过，骨头一直藏在停用的
  物体里，所以直到接回肢体那一刻才突然冒出来。现在改成「先让 `MendBone()` 删精灵、
  再清标志位」，并额外按名字扫一遍肢体子物体兜底。（v1.2.1）
- **跳得比平时高**的真凶是**一个写错的基线值**，跟断肢物理没关系。
  v1.2.0 的「清药物」里写了 `b.stimulantMultiplier = 1f`，看起来像「复位成 1」，
  但这个字段的**中性值是 0**：

  ```csharp
  // Body::HandleVariableUpdates —— 游戏自己每帧把它往 0 拉，每秒只拉 0.02
  stimulantMultiplier = Mathf.MoveTowards(stimulantMultiplier, 0f, Time.deltaTime * 0.02f);

  // Body::get_legSpeedMult —— 移动速度乘的是 (1 + stimulantMultiplier)
  ... * (1 + stimulantMultiplier) * ...

  // Body::get_actualMaxSpeed —— 最高速度（跳跃高度也按它算）
  Mathf.Min(maxSpeed * (...) * (1 + stimulantMultiplier), ...)
  ```

  所以写 `1f` 等于给角色灌了满剂量兴奋剂：**移动速度 ×2**、最高速度 ×2、**跳得高一倍**，
  而且每秒只衰减 0.02、要 **50 秒**才自己归零。v1.2.2 已改成 `0f`。
  顺带一提，`stimulantMultiplier` **不驱动任何状态图标**（「兴奋」图标读的是
  `onHardStimulants`，「咖啡因」读的是 `caffeinated`），所以清零不会漏掉图标。
- **角色自己走**是另一个原因，v1.2.2 并没有修好它。真凶是 v1.2.0 还写了一句
  `SetF(b, "slowdownAmount", 0f)` —— `slowdownAmount` 是**地面摩擦系数**：

  ```csharp
  // Body::FixedUpdate —— 松开按键时，唯一让角色停下来的地方
  if (grounded) rb.velocity = new Vector2(rb.velocity.x * (1 - slowdownAmount * get_legSpeedMult()), rb.velocity.y);
  ```

  全程序集里只有两处会写它：**Body prefab 上序列化的原始值**（正常玩法的摩擦全靠它，
  `Body` 的构造函数根本没碰过这个字段），以及 `CrystalKinetic::Touched` 里硬编码的
  `slowdownAmount = 0.5f`。**没有任何代码会把它恢复原值。**
  所以一旦写成 `0`，系数就变成 `1 - 0 = 1`：每个物理步都不减速，角色会永远保持上一次
  的速度 —— 松开按键继续滑、一整天都在滑，而且再按几次 F4 也好不了（0 写 0 是幂等的）。
  表现和「双倍速度」很像，所以前两版都误判了。v1.2.3 直接**不再碰这个字段**。
- 还有一处默默失效：`temporarySlowdown` 是 **float**，以前按布尔写 `SetF(..., false)`
  属于类型不匹配、静默失败，什么也没清掉。v1.2.3 改成写 `0f`。
- 另外，接回断肢时**仍然**做了物理复位作为预防：`Limb.Dismember()` 从头到尾没动过
  `rb`（Rigidbody2D）和 `joint`（HingeJoint2D），只是停用了物体，所以一 `SetActive(true)`
  那个刚体就会带着陈旧位姿砸回物理世界。顺序是 `simulated = false` 冻结 →
  贴回铰链另一端（`joint.connectedBody`）→ 速度与角速度清零 → `simulated = true` 恢复
  → 顺手把身体刚体也刹一脚。（v1.2.1 加的保险，留着了）

**★ 药物的后遗症也清了（v1.2.0 修）：**

「药物过量」「戒断反应」「兴奋」「咖啡因」这几类图标的数据源**根本不在 `Body` 上**，
而在 `Painkillers` 组件里 —— `overdose1..4` 与 `withdrawal1..4` 直接由
`Painkillers.actualOpiateReception` 的数值决定（>50 / >20 / >5 是过量，
<-5 / -15 / -25 / -34 是戒断）。以前这些一个都没清，所以按完 F4 图标还在、过一会儿
又开始掉血手抖。现在五个量全部清零，`onHardStimulants` / `caffeinated` /
`stimulantMultiplier` / `overdoseIndex` / `focusedLevel` 也一并复位。

**想保留情绪状态的话：** 配置里把 `ClearMoodAndMind` 改成 `false`，F4 就只清身体上的负面状态（伤势/中毒/寒冷/肮脏/超重/失眠…），抑郁、忧郁、精神空洞这些留着不动。

**★ 为什么按一下图标就立刻消失：**

状态图标由 `MoodleManager` 管理。模组先按上表把底层字段压回健康值，然后手动调一次游戏的 `MoodleManager.UpdateMoodles()` —— 这个方法开头就是 `UpdatePrevMoodles() → ClearMoodles() → AddAllMoodles()`，等于「把图标全部推倒重画一遍」。不调它的话要等游戏自己 0.5 秒一次的定时刷新（`updateTime = 0.5f`）。

**★ 它是怎么知道有哪些状态的：**

反编译 `MoodleManager.UpdateMoodles()` 得到的完整列表 —— 游戏把**每一个**图标都写成「读某个 `Body` 字段 → 判断阈值 → `AddMoodle(强度, 图标名, 名称, 描述, 是否危急, 是否仅破损时)`」，所以照着字段清就一定对得上，不会有漏网的图标。

**日志里会看到：**

```
[GodMode] F4 -> 已清除全部负面状态（治疗 + 情绪 + 中毒…）
```

> 当前版本不打这行日志，左上角会显示 `已清除全部负面状态（治疗 + 情绪 + 中毒…）`。
> 如果按了 F4 还有坏图标没消失，把图标的样子（或截图）发我，我按 `UpdateMoodles` 再对一遍。

---

## 七、生成万物（F8）：物品 / 生物 / 怪物 / 植物 / 建筑 / 方块

按 **F8** 打开窗口，可以生成游戏里的**任何东西** —— 不只是物品，还有 NPC、怪物、动物、植物、建筑设施、方块。窗口在屏幕左上角（默认位置），带半透明深色背景。

### ★ v2.0.0：创造模式的图标网格

打开就是**一格一格的图标**，9 列 × 5 行 = **45 个一页**，和 Minecraft 创造模式背包一样：

- **按住图标往世界里拖 → 松手就生成在鼠标落点那个格子上**（拖动时有半透明幽灵图标跟着鼠标）
- 在面板上松手 = 生成在当前放置点（老行为不变）
- 左键单击图标 = 直接生成
- 滚轮 / PgUp / PgDn 翻页（**只在鼠标位于面板内时生效**）

图标不是画出来的，是真从游戏里取的：

| 类别 | 图标来源 |
| --- | --- |
| 方块 | `WorldGeneration.tiles[方块id]` 的 `Tile.sprite`（游戏自己的 tileset） |
| 物品 / 实体 | prefab 上 `SpriteRenderer.sprite` |
| 取不到 | 只显示名字 |

> 这游戏**没有任何图标注册表** —— `ItemInfo` 里连 sprite 字段都没有，所以只能这样硬取。

界面这轮也一起重做了：面板/格子/幽灵图标都是**运行时逐像素生成的圆角九宫格 Sprite**（不依赖任何美术资源），
深色半透明底 + 顶部蓝色强调条，悬停蓝边、选中绿边。

一开窗直接是「全部」网格，不再先弹一层分类总览；第一排的大分类按钮之间直接切网格。
**分类总览**和**键位设置**仍然是原来的行列表 —— 想看分类总览就点第二排中间那个分类标签。

### ★ 分类：先选大类，再看分类格子（v1.2.0 重做）

游戏自己只给物品分了 8 个粗桶 —— 光 `custom` 一个桶就塞了几十件，所以以前那条约等于没法用。
现在改成两级：

**① 第一排选分组**：全部 / 物品 / 生物 / 植物 / 建筑 / 方块 / 其它。

**② 点完分组直接进「分类总览」**：列表变成格子，每格是一个具体分类，右边标着件数，
点哪格就只留那类东西。第二排的分类标签在普通列表里显示成
`▼ 物品/医疗（点这里选分类）`，**点它随时回总览**，`Backspace` 也能来回切。

细分到什么程度：

| 分组 | 细分分类 |
| --- | --- |
| 物品 | 弹药 / 穿戴（护甲·服装）/ 武器（枪械·近战·其它）/ 医疗（包扎·药品·药物）/ 食物 / 饮料 / 容器 / 垃圾 / 工具 / 材料 |
| 生物 | 怪物 / 商人 / 动物 / NPC |
| 植物 | 蘑菇 / 树木 / 藤蔓 / 草丛 / 作物 / 花藻 |
| 建筑 | 门墙 / 工作台 / 电力 / 存储 / 医疗 / 防御 / 家具 / 水晶 / 设施 |
| 方块 | 按材质分七类（空气 / 液体 / 矿物 / 岩土 / 有机 / 建材 / 其它） |
| 其它 | 尸体 / 可交互 / 杂项 |

**分类判据以组件类型为先，名字关键词只做兜底** —— 尸体走 `CorpseScript`、
水晶怪走 `CrystalEnemy`、装饰水晶走 `CrystalBehaviour`、商人走 `TraderScript`、
蜘蛛系走 `SpiderHandler` / `SpiderHandlerTBE`。所以不会再把「沙玫瑰」当建筑、
把「显现体」当装饰水晶、把「医疗站」当居民。

### ★ 搜索：不再被分组卡住（v1.2.0 修）

按 `/` 打字。**一旦输入了关键词，分组和分类筛选就全部失效**，整个清单一起搜 ——
以前在「物品」分组里搜一个怪物名永远搜不到，这就是「菜单里很多东西找不到」的主因。
清单本身也补全了：实体除了 `Item` / `BuildingEntity`，现在也认 `UsableObject`
（家具、终端等一大堆可交互物件）；方块的探测容错从 256 放宽到 512。

### ★ 开窗口时角色不会乱动（v1.2.0 修）

以前只把 `Time.timeScale` 冻成 0，但玩家的输入是在 `PlayerCamera.HandleInput()` 里读的，
跟时间缩放完全无关 —— 所以点界面时人照样走、照样挥拳、照样开枪。
现在给 `HandleInput` 挂了前缀，窗口开着时整个方法直接跳过（移动 / 瞄准 / 攻击一起挡），
另外每秒清一次 `body.moveDir` 兜底。

想边开菜单边走路，把配置里的 `FreezeInputWhileOpen` 改成 `false`。

### ★ 生成位置

第三排第一个按钮就是**生成位置**，点一下循环切换三档：

| 模式 | 按钮显示 | 生成到哪 |
|---|---|---|
| 0 | `位置: 角色脚下` | 生成在角色站的地方 |
| 1 | `位置: 鼠标处` | 生成在鼠标指的世界位置（鼠标在界面上时会自动夹回世界范围） |
| **2** | **`位置: 自己选的点`** | **生成在你手动指定的那一点**（默认模式） |

**怎么「自己选一个地方」：**

1. 点第三排的 **`选点`** 按钮（按钮会变成橙色的 `选点中…`）；
2. 鼠标在世界里移动 —— 屏幕上会跟着出现一个**标记点**，旁边写着它当前的世界坐标；
3. **左键点一下**就定下来了。提示会显示 `生成点已设为 X, Y（点列表里的东西就会生成在这里）`；
4. 之后点列表里任何一行，东西就生成在那个点上。

微调与取消：

| 操作 | 效果 |
|---|---|
| `←` `→` `↑` `↓` | 生成点按 1 格移动 |
| `Shift` + 方向键 | 按 0.25 格精细移动 |
| `Esc` 或 **鼠标右键** | 取消选点（生成点保留在最后的位置） |

标记点的样子：一个**黑框黄点**，如果正在选点则变成**红点**。它只在「自己选的点」模式下显示。

> **为什么会多出这个功能**：原来只能「鼠标处 / 角色脚下」二选一。而点窗口里的列表时鼠标必然停在界面上，换算出来的世界坐标容易跑到世界外面去 —— 这既不方便，也正是上一版生成后卡死的元凶（见下面「本版修了什么」）。
> 现在**所有生成位置都会被夹回世界范围内**，生成到地图外已经不可能了。

### 分类

打开后第一排是**大分类**按钮，点一下就切过去（当前分类会变绿）：

| 大分类 | 里面有什么 |
|---|---|
| **全部** | 所有东西混在一起（按名字排序） |
| **物品** | 游戏里的一切可捡物品（武器、食物、药品、工具、垃圾…），再按游戏自己的分类分小类（小类名已翻成中文，如 `物品/医疗`） |
| **生物** | 商人、动物、怪物、蜘蛛…（细分为 `生物/商人`、`生物/动物`、`生物/怪物`）。`显现体`（水晶怪）也在这里 |
| **植物** | 树、蘑菇、藤蔓、苔藓、垂麦、水芦、沙玫瑰…之类的植物实体 |
| **建筑** | 建筑与设施（炮塔、地雷、医疗站、救生舱、终端…），另有 `建筑/水晶`（水晶簇，是装饰矿物不是怪） |
| **方块** | 直接往世界里放的方块（岩石、泥土、矿石…）。**注意这个是「砌墙」，不是掉落物** —— 生成点在哪就砌在哪 |

第二排是**子分类**：`◀` `▶` 切换，中间显示当前子分类名，`子分类=全部` 按钮一键回全部。

### 鼠标操作

| 操作 | 效果 |
|---|---|
| 左键点某一行 | **直接生成**那一行的东西 |
| 滚轮上/下 | 翻页 |
| 第一排按钮 | 切换大分类 / 搜索 / 清空筛选 / **键位设置** / 关闭 |
| 第二排按钮 | 子分类 ◀▶ / 子分类=全部 / **重置键位** / 上一页 / 下一页 |
| 第三排按钮 | **位置（切换三档）** / **选点** / 耐久 ∓10% / 自动拾取 / 生成全部 / 随机5 / 随机20 / 停止 |
| 选点模式下左键点世界 | 把生成点定在那里 |

### 键盘操作（不用鼠标也行）

| 按键 | 效果 |
|---|---|
| `↑` `↓` | 移动选中行（高亮的那行）※**选点状态下**方向键改为微调生成点，要移选中行先按 `Esc` 退出选点 |
| `回车` | 生成选中的那一个 |
| `PageUp` `PageDown` | 翻页 |
| `Home` `End` | 跳到第一页 / 最后一页 |
| `Tab` | 切换到下一个子分类 |
| `/` | 进入/退出输入状态；进入后直接打字搜索，`退格` 删除 |
| `Esc` | 退出输入状态；再按一次关窗口 |
| `F8` | 开关窗口 |

> 第一排的 **「键位设置」** 会把窗口切换成键位页（见第九节），点里面的 **「重置键位」** 可以一键恢复 F1~F9。

**窗口里显示什么：** 标题栏显示「全部 N 种 / 当前筛选 M 件 / 第 x/y 页」；下面一行是当前分组、子分类（本分组共几类）、搜索词、**放置位置（含坐标）**、耐久、自动拾取；再往下是 16 行列表，每行「[分类] 名字 (内部id)」，鼠标悬停的那行变蓝、选中的那行变绿。底部是操作提示。世界里另有一个**生成点标记**（黑框黄点 / 选点时红点）。

### ★ 清单是从哪来的（为什么游戏更新会自动多出新东西）

清单**不是写死在模组里的**，三个来源全部运行时从游戏里读：

1. **物品** —— 游戏自己的物品注册表 `Item.GlobalItems`（游戏启动时 `Item.SetupItems()` 填的字典），键就是生成时用的资源名，显示名和分类都是游戏本地化的。你装的其它模组往注册表里加了物品，也会一起出现。
2. **生物 / 怪物 / 植物 / 建筑** —— `Resources.LoadAll<GameObject>("")` 里**带 `Item` 或 `BuildingEntity` 组件的 prefab**，id 就是 prefab 名。这跟**游戏自带控制台 `spawn` 命令的自动补全完全是同一套来源**（`ConsoleScript.RegisterSpawnEntities()` 就是这么枚举的），所以凡是游戏自己能 spawn 出来的东西，这里都有。
3. **方块** —— 方块**不是 prefab**，是世界数组里的一个 `UInt16` 编号。游戏没有暴露方块表，但 `WorldGeneration.GetBlockInfo(ushort)` 对合法编号会返回带本地化名字的 `BlockInfo`，所以模组用「从 0 往上递增探测、返回 null 就算到头」的办法把方块全枚举出来。

于是：**游戏更新后新增的物品 / 怪物 / 植物 / 方块 → 自动出现在列表里；删掉的 → 自动消失。** 不需要改一行代码，也不需要我重新发版本。

### 名字是从哪来的

列表里的名字**全部是游戏自己的中文名**，来源是游戏的本地化表 `Locale`：

- **物品** —— `ItemInfo.fullName`（游戏在 `Item.SetupItems()` 里用 `Locale.GetItem(id)` 填好的）；
- **生物 / 植物 / 建筑** —— `Locale.GetBuilding(id)`。

> **上一版为什么名字是空的**：`BuildingEntity.fullName` 这个字段**在 prefab 上是空字符串** —— 它只在东西被实例化出来之后、`Start()` 里才被 `Locale.GetBuilding(id)` 填上。上一版直接读 prefab，所以拿到空白，日志里那些 `已生成： (10) (ceilingrye)` 的括号数字其实是**生命值**（`fullNameDisplay` 的实现就是 `fullName + "(" + 血量 + ")"`），名字整个是空的。
> 现在建清单时直接查 `Locale` 表，顺带把物品的子分类键（`medical`/`food`/`tool`…）也翻成中文。

如果某个东西的本地化表里没有条目，`Locale` 会原样返回 id —— 这时列表里就显示 id 本身（至少你知道它叫什么）。

### 分类是怎么判定的

判定依据是**这个东西身上挂了哪些组件**（按组件类型名匹配，不硬引用类型）：

| 判据 | 归到 |
|---|---|
| `CrystalEnemy` | 生物/怪物 |
| `TraderScript` | 生物/商人 |
| `SpiderHandler` / `SpiderHandlerTBE` / `CaveTickSpawner` / `ElderThornbackBehaviour` | 生物/怪物 |
| `GrabberPlant` / `BananaPlantSlip` | 植物 |
| `CorpseScript` | 其它/尸体 |
| `BuildingEntity.animal = true` | 生物/动物 |
| 有 `Body` 组件 | 生物/怪物 |
| 本身就是 `Item` | 物品 |
| 名字命中植物关键词（tree / mushroom / rye / reed / rose / cactus / cap / vine / moss …） | 植物 |
| `CrystalBehaviour` | 建筑/水晶 |
| 其余 `BuildingEntity` | 建筑/设施 |

> 已修掉的误判：天花板垂麦/水芦/沙玫瑰/炮果草/寒霜草/热蕨/仙人掌/弹力菇这些植物原来被塞进了「建筑/设施」（关键词表缺词）；医疗站/充电站因为身上有 `Talker` 被当成了「居民」（`Talker` 已不再作为判据）；水晶簇原来被当成「水晶怪」（它们只是装饰矿物）；`reinforcedrope` 这种本身是物品的被归到了「其它」。

因为还是按组件猜的，**分类可能仍有个别不准**。所以模组每次打开窗口都会顺手导出一份清单到：

```
E:\steam\steamapps\common\Casualties Unknown Demo\BepInEx\config\GodMode.生成物清单.log
```

格式是 `分类 | prefab名 | 身上的组件`。哪一类分错了，把这份文件发我，我照着改规则就行（也可以在配置里把 `DumpInventory` 关掉，不再导出）。

> 早期版本因为路径拼接的 bug，这份文件实际掉在 `BepInEx\` 根目录下，现已修正到 `BepInEx\config\`。

### 生成用的是哪套代码

- **prefab 类**（物品/生物/植物/建筑）：先调用游戏自己的 `Utils.Create(id, pos, rot)`（实现就是 `Object.Instantiate(Resources.Load(id), pos, rot)`）—— 和游戏自带 `spawn` 命令**完全同一条路径**；如果 `Resources` 里找不到这个名字，就退回**直接 Instantiate 模组手里那份 prefab 引用**，所以连不在 `Resources` 根目录、但被游戏某个字段引用着的 prefab 也能生成。
- 生成后如果是 `Item`：调 `Item.SetCondition(耐久)`；距离玩家小于 2 格且开了自动拾取就调 `Body.AutoPickUpItem(item)`。
- **方块**：`WorldGeneration.world.SetBlock(world.WorldToBlockPos(生成点), 编号)`。`SetBlock` 内部自带边界钳制（`Mathf.Clamp`），所以往地图外放也不会崩。
- **所有生成位置都先过一遍 `ClampToWorld()`**（夹在世界边界内 6 格），所以无论哪个模式都不会把东西生成到地图外去。

批量生成走队列，每帧最多生成 `SpawnBatchPerFrame` 个（默认 15），按 12 列网格铺开，避免一口气生成几百个把游戏卡住。

### ★ 本版修了什么

| 问题 | 原因 | 修法 |
|---|---|---|
| **生成几个东西后游戏卡死** | 原来「鼠标处」模式下直接拿鼠标屏幕坐标换算世界坐标。而你点列表里那一行时**鼠标必然停在左上角的窗口上**，换算出来的坐标跑到世界外面，实体被生成到地图外，之后每帧的区块/流体查询就把游戏拖死了。 | 三档位置模式 + **选点**；所有位置统一 `ClampToWorld()` 夹进世界内；另外加了「窗口关了却还冻着时间」的自愈解冻；每次生成都写一行带坐标的日志，万一再出问题能直接看到生成在哪。 |
| **生成的东西没名字** | `BuildingEntity.fullName` 在 prefab 上是**空的**，只有实例化后的 `Start()` 才会填。原来直接读 prefab，所以名字是空白。 | 建清单时直接查游戏的本地化表 `Locale`（`GetBuilding` / `GetOther` / `GetItem`）。 |
| **有些东西分类错了** | 植物关键词表缺词；`Talker` 被当成「居民」的判据，结果医疗站/充电站被归进生物；水晶簇被当成水晶怪。 | 见上面「分类是怎么判定的」的判据表，已重排。 |
| **`生成物清单.log` 跑到 `BepInEx\` 根目录** | `Paths.ConfigPath` 末尾带反斜杠，拼路径时多剥了一层。 | 改用 `Path.Combine(Paths.ConfigPath, 文件名)`，现在正确落在 `BepInEx\config\`。 |

### ★ 为什么界面是 uGUI 而不是普通窗口

这不是为了好看，是**必须这么做**。实测证据：

- 心跳日志里 `驱动(插件Update/相机补丁/控制台补丁/自建HUD)` 中，**`插件Update` 和 `自建HUD` 全程恒为 0** —— BepInEx 插件自身的组件、以及模组自己 `new GameObject` + `AddComponent` 出来的组件，**Update / OnGUI 都不被 Unity 调用**；
- 用反编译工具全量扫描：游戏本体 `Assembly-CSharp.dll` 里声明 `OnGUI` 的方法数 = **0**，现成能正常工作的 QoL 模组里也是 **0** —— 没有任何 OnGUI 宿主可以挂。

所以 IMGUI（`GUI.Window` / `GUILayout`）在这个游戏里根本画不出东西（最早那版窗口只弹出了一行游戏原生提示文字，就是「有文字没界面」的原因）。

现在用的是 **uGUI（Canvas + TextMeshPro）+ 手工命中测试**：

- Canvas 的渲染由 Unity 的 Canvas 系统驱动，**不依赖我们组件的 Update**，所以一定能显示；
- 鼠标/键盘交互放在**已经被证实每帧都在跑**的 `ConsoleScript.Update` 后置补丁里，用 `RectTransformUtility.RectangleContainsScreenPoint` 自己做命中测试（这套办法抄的是 QoL 的 `QoLMenuButton`）；
- 中文字体直接借用游戏自己的 TMP 字体（从 `PlayerCamera.alertText.font` 取），所以中文能正常显示；
- 窗口打开时会强制显示鼠标指针，关闭时恢复原状；打开期间默认把 `Time.timeScale` 冻成 0，免得点界面的时候角色也跟着挥拳（配置 `PauseWhileOpen` 可关）。

左上角的黄色状态字（`★ 上帝模式 ON` / `★ 上天入地 ON` / `★ 生成器`）也是 uGUI 画的。

**日志里会看到：**

```
[GodMode][生成器] 已载入：物品 NNN + 实体 NN + 方块 NN = NNN 种，分类 N 个（全部/物品/医疗/食物/…/生物/商人/生物/动物/生物/怪物/植物/建筑/设施/建筑/水晶/方块）
[GodMode][界面] uGUI 窗口已创建，字体 = XXXX
```

生成东西的时候还会各写一行（带坐标，方便对账）：

```
[GodMode][生成器] 已生成 ceilingrye（天花板垂麦） @ 12,-4  分类=植物
[GodMode][生成器] 开始批量生成 100 个，基准点 12,-4（位置模式=自己选的点）
```

如果窗口那行是 `创建 uGUI 失败` 或字体显示「未找到游戏字体」，把日志发我。如果实体数和方块数是 0，说明当时还没进游戏世界（要先读档进图再按 F8）。

---

## 八、一击必杀（F9）— 打一下，什么都碎

按 **F9** 打开，左上角会出现 `★ 一击必杀 ON   [F9 关]`。然后**随便打一下就秒**：

| 你打的东西 | 结果 |
|-----------|------|
| 生物 / 怪物 / 蜘蛛 / 商人 | 一下死 |
| 植物（垂麦、仙人掌、发光菇…） | 一下碎 |
| 建筑 / 设施（门、工作台、充电站…） | 一下碎 |
| 方块（岩石、矿脉、沙砾…） | 一下碎 |

近战武器、枪械、空手都算 —— 只要是你按「攻击键」打出去的伤害。

### 它是怎么做到的

游戏里玩家**所有攻击**都收口到同一个方法：

```csharp
public bool Body.Attack(AttackInfo atk, int slot)
```

它内部按你打中的东西分三种走法（IL 实测）：

- **打中方块** → `WorldGeneration.DamageBlock(..., atk.structuralDamage, ...)`
- **打中建筑** → `BuildingEntity.health -= atk.structuralDamage`
- **打中生物** → `BuildingEntity.health -= atk.damage`

所以插件只挂了一个前缀，在进这个方法之前把两个伤害数放大：

```csharp
atk.damage *= 倍率;            // 默认 100000
atk.structuralDamage *= 倍率;
```

**一个钩子同时覆盖方块、建筑、生物**，不需要分别去改三个地方。而且 `atk` 是临时对象，不会污染存档。

### 几个刻意的设计

- **只对你生效**：判定 `__instance == PlayerCamera.main.body`。NPC 互相打、野兽咬野兽，伤害还是原版数值，世界不会自己崩坏。
- **前缀返回 void**，让原方法照常执行。返回 `false` 跳过原方法就变成「打不出去」了，那不对。
- **倍率上限一百万**：伤害飘字走的是 `(int)` 转换，调到千万级会溢出成负数，屏幕上会飘出奇怪的红字。
- 想改倍率：配置文件 `[Options] OneHitMultiplier`。

---

## 九、键位设置 —— 就在窗口里改，冲突能看见

按 **F8** 打开生成万物窗口，点第一排右边的 **「键位设置」**，窗口就会切换成键位页：

```
  上帝模式          →   [F1]
  上天入地          →   [F2]
  技能拉满          →   [F3]
  清除负面状态      →   [F4]
  传送到鼠标        →   [F5]
  全亮              →   [F6]
  初始装备包        →   [F7]
  生成万物窗口      →   [F8]
  一击必杀          →   [F9]
```

**怎么改**：左键点一行 → 那一行变成 `◀ 请按下新的按键…（Esc 取消）` → 按一下你想用的键 → 立刻生效并写进配置文件。

| 操作 | 效果 |
|------|------|
| 左键点一行 | 选中，等按键 |
| 按任意键 | 绑定成这个键 |
| **Esc** | 取消本次改键 |
| 「重置键位」按钮 | 9 个键全部恢复 F1~F9 |
| 「返回生成器」按钮 / Esc | 回到生成列表 |
| **F8** | 关窗口（下次打开自动回到生成列表，不会卡在键位页） |

### 键位撞车了怎么办

用户说「冲突可以改」，所以插件**不阻止**你绑重复的键，但会**明确告诉你**：

- 那一行**底色变红**，末尾显示 `⚠ 与「XXX」冲突`
- 顶部提示条也会写一句

这样你能立刻看出是哪个键撞了，再点那一行换一个就行。撞车的两个功能会**同时触发**（Unity 的 `Input.GetKeyDown` 本来就是这样），不是 bug。

### 两个细节

- **改键时功能热键会被屏蔽**。不然你把「上帝模式」改成 F1 的那一瞬间，就会顺手开关一次上帝模式 —— 那是很坑的体验。
- 写回的文件就是 `BepInEx\config\casualtiesunknown.godmode.cfg`，和第三节里那份是同一个。所以两种改法（窗口里改 / 手改 cfg）随便你用哪种。

### 为什么不用游戏自带的键位系统

游戏里确实有一套 `Settings` / `KeyBinds` / `SettingKeybind`（`PlayerCamera.HandleAttacks()` 就是用 `KeyBinds.GetBind("attack")` 读攻击键的）。但那是**游戏自己的存档系统**，动它等于改存档、还可能和游戏后续版本冲突。插件自己用 BepInEx 的 cfg 存，互不干扰。

---

## 十、重新编译（改代码后）

```powershell
& 'G:\deepseek harness\GodModeMod\build.ps1'
```

游戏正在运行时 DLL 被内存映射，写不进去，用暂存目录：

```powershell
& 'G:\deepseek harness\GodModeMod\build.ps1' -OutPath 'G:\deepseek harness\GodModeMod\stage\GodMode.dll'
```

源码：`G:\deepseek harness\GodModeMod\GodMode.cs`

编译用的是 Windows 自带的 `.NET Framework 4.0 csc.exe`，**因此源码必须保持 C# 5 语法**（不能用 `?.`、字符串插值 `$""`、表达式体成员、`nameof`）。

脚本内部把参数写进 `build.rsp` 再用 `csc @build.rsp` 调用 —— 因为游戏路径里有空格（`Casualties Unknown Demo`），PowerShell 7 直接传参会把路径拆开，报一堆莫名其妙的 `CS0006/CS2001`。

---

## 十一、卸载

删掉这个文件即可，不留任何痕迹：

```
E:\steam\steamapps\common\Casualties Unknown Demo\BepInEx\plugins\GodMode.dll
```

---

## 十二、出问题时

看日志：

```
E:\steam\steamapps\common\Casualties Unknown Demo\BepInEx\LogOutput.log
```

搜 `[GodMode]`。正常启动应该看到 17 行 `已挂钩:`：

```
[Info   :GodMode] [GodMode] 已挂钩: Body.HandleBody(每帧身体模拟)
[Info   :GodMode] [GodMode] 已挂钩: Body.HandleCirculation(血液循环/脑死亡)
[Info   :GodMode] [GodMode] 已挂钩: Limb.ImpactDamage(受击入口)
[Info   :GodMode] [GodMode] 已挂钩: Body.TryLastStand(濒死挣扎)
[Info   :GodMode] [GodMode] 已挂钩: Body.get_alive(生死判定)
[Info   :GodMode] [GodMode] 已挂钩: PlayerCamera.Update(每帧驱动)
[Info   :GodMode] [GodMode] 已挂钩: ConsoleScript.Update(备用驱动)
[Info   :GodMode] [GodMode] 已挂钩: TraderScript.OnWillRenderObject(越界防护)
[Info   :GodMode] [GodMode] 已挂钩: Skills.GetExperienceForLevel(经验溢出防护)
[Info   :GodMode] [GodMode] 已挂钩: Skills.UpdateExpBoundaries(上限修正)
[Info   :GodMode] [GodMode] 已挂钩: Skills.CheckForLevelUp(等级钳制)
[Info   :GodMode] [GodMode] 已挂钩: Skills.AddExp(经验值钳制)
[Info   :GodMode] [GodMode] 已挂钩: Body.Attack(一击必杀)
[Info   :GodMode] [GodMode] 已挂钩: PlayerCamera.HandleInput(菜单时冻结操作)
[Info   :GodMode] [GodMode] 已挂钩: Limb.Dismember(上帝模式下不被砍断)
...  （一共 17 行 已挂钩）
[Info   :GodMode] [GodMode] 已加载 v1.2.0
[Info   :GodMode] [GodMode] 看门狗已启动：主线程卡死超过 8 秒会自动抢救
```

改键时日志里会多一行：

```
[Info   :GodMode] [GodMode][键位] 上帝模式 (GodMode) = G
```

按 F4 之后会多出几行运动诊断（前后各一份，之后 3 秒每半秒一份），用来确认「自己动」这类问题：

```
[Info   :GodMode] [GodMode][诊断] F4 之后 轴=0.00/0.00 moveDir=0.000,0.000 速度=0.000,0.000
                 落地=True 打滑=False ★摩擦=0.25 临时减速=0 兴奋剂=0 体力=100 精力=100 双腿没了=False
```

`★摩擦` 就是 `slowdownAmount`，正常应该是 prefab 里的原始值（**不是 0**；0 意味着摩擦被关掉，
角色会一直滑）。

每 5 秒还有一行心跳：

```
[GodMode][心跳] 帧=5053 调度=5053 驱动(插件Update/相机补丁/控制台补丁/自建HUD)=0/1010/5053/0 窗口焦点=True 任意键按住=False 玩家=就绪 控制台=就绪 世界=±Nx±M 限位=K NPC修正=S 技能修正=J 上帝=True 飞行=True
```

- `找不到方法，跳过` → 游戏更新后方法改名了，需要按新名字调整。
- `挂钩失败` → Harmony 绑定失败（通常是补丁函数的参数签名和原方法对不上）。
- `技能修正` 一直涨 → 有东西在反复把技能数值改坏，把日志发我。
