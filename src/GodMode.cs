// ============================================================================
//  Casualties Unknown (Demo) - 上帝模式 God Mode   v1.0.0
//  运行环境: BepInEx 5.4.23.4 / Unity 2022.3.62 / Mono
//  编译约束: 用 .NET Framework 自带 csc.exe => 只能 C# 5 语法
//            (禁用 ?. / 字符串插值 / 表达式体成员 / nameof)
//  逆向依据: Body/Limb 构造函数中的满值常量, ConsoleScript 的 noclip 实现
//
//  版本说明: v1.0.0 = 首个公开发布版, 内容等于下面的内部迭代 0.9.0.
//            下文 v0.7.0 / v0.8.0 / v0.9.0 是发布前的内部迭代记录, 保留供追溯.
//            发布前那些更早的迭代(0.1.0~0.6.0)以新增功能为主, 已并入本节说明.
//
//  v0.7.0 变更 (界面版):
//    * 【重要】本游戏里 IMGUI 完全不可用: 插件自身以及自己 AddComponent 出来的
//      组件, Update / OnGUI 都不被 Unity 调用(心跳日志里两个计数器恒为 0),
//      而游戏本体和 QoL 模组里声明 OnGUI 的方法数都是 0, 没有宿主可挂.
//      所以物品生成器改用 uGUI(Canvas + TextMeshProUGUI)重建 —— Canvas 由
//      Unity 的 Canvas 系统渲染, 不依赖我们组件的 Update, 一定能显示出来;
//      鼠标/键盘交互在已被证实每帧都在跑的 ConsoleScript.Update 后置补丁里
//      手工做命中测试(照搬 QoL 的 QoLMenuButton).
//    * 左上角状态字也改成 uGUI, 终于能看见了.
//
//  v0.8.0 变更 (生成万物 + 分类):
//    * 生成器从「只生成物品」扩展为「生成所有东西」, 并且分好类:
//        物品 / 生物(商人·居民·动物·怪物·蜘蛛·水晶怪) / 植物 / 建筑设施 / 方块
//    * 清单三个来源, 全部运行时从游戏里读, 游戏更新加了新东西会自动出现:
//        ① Item.GlobalItems —— 物品(显示名与分类都是游戏本地化的)
//        ② Resources.LoadAll<GameObject>("") 里带 Item / BuildingEntity 组件的
//           prefab —— 和游戏控制台 spawn 命令的自动补全完全同一来源
//        ③ WorldGeneration.GetBlockInfo(ushort) 递增探测 —— 方块(方块不是 prefab,
//           是世界数组里的 UInt16 id; 靠游戏自己的 BlockInfo 判合法性, 名字已本地化)
//    * 界面上第一排切大分类, 第二排 ◀▶ 切子分类, 第三排是生成选项.
//    * 打开生成器时导出 BepInEx\config\GodMode.生成物清单.log(分类|prefab|组件).
//
//  v0.9.0 变更 (名字 / 自己选位置 / 防卡死):
//    * 【名字】BuildingEntity.fullName 在 prefab 上是空字符串 —— 它只在实例化后的
//      Start() 里才被 Locale.GetBuilding(id) 填上, 所以 v0.8.0 的清单里实体名全是空的
//      (日志里那些 "(10)" "(10000)" 其实是 BuildingEntity.fullNameDisplay 里的生命值).
//      现在建清单时直接查游戏本地化表: Locale.GetBuilding / GetOther / GetItem.
//      物品的子分类键(medical/food/tool…)也一并翻成中文.
//    * 【自己选位置】新增「生成位置」三档: 角色脚下 / 鼠标处 / 自己选的点.
//      选点模式下鼠标扫过世界会实时移动一个黄点标记, 点一下就把生成点定在那里;
//      方向键微调(按 Shift 变 0.25 格). 生成点默认取打开窗口时角色的位置.
//      ★ 所有生成位置都会被夹回世界范围内(ClampToWorld) —— v0.8.0 的卡死极可能
//        就是"鼠标处的世界坐标落在世界外"造成的: 用户在左上角面板上点行, 鼠标屏幕坐标
//        换算出来的世界坐标跑到世界外面去了, 生成在世界外的实体每帧做区块查询会把游戏拖死.
//    * 【防卡死】① 生成位置一律 ClampToWorld; ② Time.timeScale 自愈: 只在 >0 时记账,
//      窗口关掉后若发现还冻着就强制解冻(避免把 0 存下来又恢复成 0, 游戏永久冻住);
//      ③ 每次生成都写一行日志(东西/分类/坐标), 再出问题可以直接看坐标.
//      ④ 导出日志的路径修好: Paths.ConfigPath 末尾带反斜杠, GetDirectoryName 会返回
//         上一级目录, 所以 v0.8.0 的清单/诊断文件掉到了 BepInEx 根目录.
// ============================================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CasualtiesGodMode
{
    [BepInPlugin("casualtiesunknown.godmode", "GodMode", "1.0.0")]
    public class GodModePlugin : BaseUnityPlugin
    {
        internal static ManualLogSource L;
        internal static Harmony H;

        internal static ConfigEntry<string> CGod;
        internal static ConfigEntry<string> CFly;
        internal static ConfigEntry<string> CSkill;
        internal static ConfigEntry<string> CHeal;
        internal static ConfigEntry<string> CTp;
        internal static ConfigEntry<string> CLight;
        internal static ConfigEntry<string> CKit;
        internal static ConfigEntry<string> CSpawner;
        internal static ConfigEntry<bool> CStartGod;
        internal static ConfigEntry<bool> CStartFly;
        internal static ConfigEntry<bool> CHud;
        internal static ConfigEntry<bool> CDebug;
        internal static ConfigEntry<bool> CMoodClear;

        internal static KeyCode KGod = KeyCode.F1;
        internal static KeyCode KFly = KeyCode.F2;
        internal static KeyCode KSkill = KeyCode.F3;
        internal static KeyCode KHeal = KeyCode.F4;
        internal static KeyCode KTp = KeyCode.F5;
        internal static KeyCode KLight = KeyCode.F6;
        internal static KeyCode KKit = KeyCode.F7;
        internal static KeyCode KSpawner = KeyCode.F8;

        internal static bool GodOn = false;
        internal static bool FlyOn = false;
        internal static bool LightOn = false;

        internal static string Msg = "";
        internal static float MsgUntil = 0f;
        internal static GUIStyle MsgStyle = null;
        internal static float NextErrLog = 0f;
        internal static float NextConsoleCheck = 0f;
        internal static float NextBeat = 0f;

        // ---- 诊断计数器 ----
        internal static int FrameGuard = -1;
        internal static int TickCount = 0;
        internal static int DrvPlugin = 0;
        internal static int DrvCamera = 0;
        internal static int DrvConsole = 0;
        internal static int DrvHud = 0;
        internal static GodModeHud Hud = null;
        internal static bool HudOk = false;

        // ---- 世界边界 / 卡死防护 ----
        internal static ConfigEntry<int> CMargin;
        internal static ConfigEntry<bool> CProtect;
        internal static float WorldHW = 0f;
        internal static float WorldHH = 0f;
        internal static float NextWorldProbe = 0f;
        internal static float NextLimitMsg = 0f;
        internal static float NextTradeMsg = 0f;
        internal static int ClampCount = 0;
        internal static int TraderSkipCount = 0;
        internal static readonly List<TraderScript> BadTraders = new List<TraderScript>();

        // ---- 技能等级防护 ----
        // 游戏的 GetExperienceForLevel(level) 是一个 O(level) 的循环，里面 step *= 1.18（20级后再 ×1.15，30级后再 ×2），
        // 累加值在 42 级就超过 int.MaxValue、109 级变成 Infinity；Mathf.RoundToInt(Infinity) 会得到一个负数，
        // 于是 CheckForLevelUp 里 "经验 >= 上限" 永远成立 —— 主线程死循环，游戏卡死。
        internal static ConfigEntry<int> CMaxSkillLv;
        internal static ConfigEntry<bool> CSkillGuard;
        internal const int SAFE_SKILL_LEVEL = 35;
        internal const float SAFE_XP = 1e8f;
        internal static int SkillFixCount = 0;

        // ---- 生成器（清单在运行时从游戏里读，游戏更新加了新东西会自动出现） ----
        // v0.9.0：生成位置改成「自己指定」——PosMode 0=角色脚下 1=鼠标处 2=自己选的点(锚点)
        internal static ConfigEntry<int> CSpawnPosMode;
        internal static Vector2 SpawnAnchor = Vector2.zero;   // 自己选的生成点
        internal static bool SpawnAnchorSet = false;
        internal static bool UiPickPos = false;               // 「选位置」模式：面板收起，下一次点世界 = 定锚点
        internal static ConfigEntry<float> CSpawnCond;
        internal static ConfigEntry<bool> CSpawnPickup;
        internal static ConfigEntry<int> CSpawnPerFrame;
        internal static ConfigEntry<int> CSpawnMaxBatch;
        internal static ConfigEntry<bool> CSpawnDump;
        internal static bool SpawnerOn = false;
        internal static string SpawnFilter = "";
        internal static string SpawnCat = "全部";
        internal static string SpawnGroup = "全部";
        internal static Vector2 SpawnScroll = Vector2.zero;
        internal static Rect SpawnWin = new Rect(0f, 0f, 620f, 620f);
        internal static bool SpawnWinInit = false;

        // 生成用的三种东西：
        //   K_ITEM  = 物品 prefab（走游戏自己的 Item.GlobalItems 表，显示名与分类都是游戏本地的）
        //   K_ENTITY= 生物/怪物/NPC/植物/建筑 prefab（走 Resources.LoadAll，和游戏控制台 spawn 命令同一来源）
        //   K_BLOCK = 方块（不是 prefab，是世界数组里的 UInt16 id，用 WorldGeneration.SetBlock 写进去）
        internal const int K_ITEM = 0;
        internal const int K_ENTITY = 1;
        internal const int K_BLOCK = 2;

        // 缓存的生成表：一一对应的四个数组 + 分类/分组字符串
        internal static readonly List<string> SpawnIds = new List<string>();
        internal static readonly List<string> SpawnNames = new List<string>();
        internal static readonly List<string> SpawnCats = new List<string>();     // "物品/医疗"、"生物/怪物"、"方块" …
        internal static readonly List<int> SpawnKinds = new List<int>();
        internal static readonly List<GameObject> SpawnObjs = new List<GameObject>();  // prefab 本体，直接 Instantiate
        internal static readonly List<int> SpawnBlocks = new List<int>();              // 方块 id
        internal static readonly List<string> SpawnGroups = new List<string>();        // 顶层分组
        internal static int SpawnCacheCount = -1;
        internal static readonly Queue<int> SpawnQueue = new Queue<int>();             // 存的是上表的索引
        internal static int SpawnGridX = 0;
        internal static int SpawnTotal = 0;
        internal static int SpawnDone = 0;
        internal static int SpawnFail = 0;

        // ---- 看门狗：主线程停滞检测（跑在后台线程，主线程卡死时它能救命） ----
        internal static ConfigEntry<bool> CWatchdog;
        internal static volatile int Beat = 0;
        internal static int WatchdogFires = 0;
        internal static float StallAtFrame = 0f;
        internal static bool WatchdogStarted = false;
        internal static readonly List<Skills> KnownSkills = new List<Skills>();

        // ---- 满值常量：全部取自游戏 Body/Limb 构造函数里的初始值 ----
        internal const float V_MAX = 100f;
        internal const float V_PRESSURE = 120f;
        internal const float V_HEARTRATE = 70f;
        internal const float V_RESPRATE = 100f;
        internal const float V_TEMP = 37f;

        void Awake()
        {
            L = Logger;

            CGod = Config.Bind<string>("Keys", "GodMode", "F1", "上帝模式 开/关");
            CFly = Config.Bind<string>("Keys", "FlyNoClip", "F2", "上天入地(飞行+穿墙) 开/关");
            CSkill = Config.Bind<string>("Keys", "MaxSkills", "F3", "力量/抗性/智力 拉满");
            CHeal = Config.Bind<string>("Keys", "FullHeal", "F4", "清除全部负面状态（治疗+情绪+中毒等）一次");
            CTp = Config.Bind<string>("Keys", "TeleportToMouse", "F5", "传送到鼠标位置");
            CLight = Config.Bind<string>("Keys", "Fullbright", "F6", "全亮 开/关");
            CKit = Config.Bind<string>("Keys", "StarterKit", "F7", "领取初始装备包");
            CSpawner = Config.Bind<string>("Keys", "ItemSpawner", "F8", "生成万物窗口 开/关");
            CStartGod = Config.Bind<bool>("Options", "StartWithGodMode", false, "启动时自动开启上帝模式");
            CStartFly = Config.Bind<bool>("Options", "StartWithFly", false, "启动时自动开启飞行");
            CHud = Config.Bind<bool>("Options", "ShowHud", true, "左上角显示状态提示");
            CDebug = Config.Bind<bool>("Options", "DebugLog", true, "写诊断日志(心跳/按键扫描)，排查问题时才需要开");
            CMoodClear = Config.Bind<bool>("Options", "ClearMoodAndMind", true, "F4 是否连情绪/精神类负面状态(抑郁、精神空洞等)一起清除");
            CMargin = Config.Bind<int>("Options", "WorldMargin", 12, "飞行时与世界边缘保持的格数（防止飞出世界把游戏搞崩）");
            CProtect = Config.Bind<bool>("Options", "CrashGuard", true, "崩溃防护：修正跑到世界外的 NPC，避免游戏每帧报错卡死");
            CMaxSkillLv = Config.Bind<int>("Options", "MaxSkillLevel", 30, "F3 技能拉满时设置的等级(安全上限35，超过42游戏会浮点溢出卡死)");
            CSkillGuard = Config.Bind<bool>("Options", "SkillGuard", true, "技能防护：阻止技能等级/经验溢出导致的卡死");
            CWatchdog = Config.Bind<bool>("Options", "Watchdog", true, "看门狗：主线程卡死超过8秒时强制把技能数值拉回安全值并写诊断日志");
            CSpawnPosMode = Config.Bind<int>("Options", "SpawnPosMode", 2, "生成位置：0=角色脚下 1=鼠标处 2=自己选的点（点窗口里的「选点」按钮，再点世界任意位置）");
            CSpawnCond = Config.Bind<float>("Options", "SpawnCondition", 1f, "生成物品的耐久度 0~1（1 = 全新）");
            CSpawnPickup = Config.Bind<bool>("Options", "SpawnAutoPickup", true, "生成在角色附近时自动拾取（和游戏自带 spawn 命令一致）");
            CSpawnPerFrame = Config.Bind<int>("Options", "SpawnBatchPerFrame", 15, "批量生成时每帧最多生成几个（防止一口气生成太多卡顿）");
            CSpawnMaxBatch = Config.Bind<int>("Options", "SpawnMaxBatch", 800, "一次批量生成的数量上限");
            CSpawnDump = Config.Bind<bool>("Options", "DumpInventory", true, "打开生成器时把「所有可生成的东西 + 它被分到哪一类 + 身上有哪些组件」写一份清单到 BepInEx\\config\\GodMode.生成物清单.log，方便核对分类");
            CPauseOpen = Config.Bind<bool>("Options", "PauseWhileOpen", true, "打开生成器时把游戏时间冻住（Time.timeScale=0），避免点界面时角色也跟着挥拳/用道具");

            KGod = ParseKey(CGod.Value, KeyCode.F1);
            KFly = ParseKey(CFly.Value, KeyCode.F2);
            KSkill = ParseKey(CSkill.Value, KeyCode.F3);
            KHeal = ParseKey(CHeal.Value, KeyCode.F4);
            KTp = ParseKey(CTp.Value, KeyCode.F5);
            KLight = ParseKey(CLight.Value, KeyCode.F6);
            KKit = ParseKey(CKit.Value, KeyCode.F7);
            KSpawner = ParseKey(CSpawner.Value, KeyCode.F8);

            GodOn = CStartGod.Value;
            FlyOn = CStartFly.Value;

            H = new Harmony("casualtiesunknown.godmode");
            Patches.Install();
            StartWatchdog();
            StartHud();

            L.LogInfo("[GodMode] 已加载 v1.0.0");
            L.LogInfo("[GodMode] 按键: 上帝=" + KGod + " 飞行=" + KFly + " 技能=" + KSkill
                + " 治疗=" + KHeal + " 传送=" + KTp + " 全亮=" + KLight + " 装备=" + KKit
                + " 生成器=" + KSpawner);
        }

        // 第三路驱动：插件自身的 Update
        // （实测本游戏的 BepInEx 管理器对象上，插件自身的 Update/OnGUI 从不被调用，
        //   所以这里只是兜底，真正的驱动是相机/控制台补丁和自建的 GodModeHud 对象）
        void Update()
        {
            DrvPlugin++;
            Tick();
        }

        // ------------------------------------------------------------------
        //  界面驱动器
        //  ★ 关键：实测发现插件自身组件的 Update/OnGUI 在本游戏里从不触发，
        //  所以自建一个 DontDestroyOnLoad 的 GameObject 来承载 Update + OnGUI。
        // ------------------------------------------------------------------
        internal static void StartHud()
        {
            try
            {
                GameObject go = new GameObject("GodModeHud");
                UnityEngine.Object.DontDestroyOnLoad(go);
                Hud = go.AddComponent<GodModeHud>();
                HudOk = (Hud != null);
                if (L != null)
                    L.LogInfo("[GodMode] 界面驱动已创建=" + HudOk + "（自建 HUD 对象，用于每帧驱动与绘制窗口）");
            }
            catch (Exception e)
            {
                HudOk = false;
                if (L != null) L.LogWarning("[GodMode] 界面驱动创建失败，回退到插件自身 OnGUI: " + e.Message);
            }
        }

        // ------------------------------------------------------------------
        //  每帧唯一入口。三路驱动任意一路调用即可，靠 frameCount 保证每帧只跑一次
        // ------------------------------------------------------------------
        internal static void Tick()
        {
            if (FrameGuard == Time.frameCount) return;
            FrameGuard = Time.frameCount;
            TickCount++;
            Beat = TickCount;
            StallAtFrame = Time.realtimeSinceStartup;

            try
            {
                ScanKeys();

                if (Input.GetKeyDown(KGod))
                {
                    GodOn = !GodOn;
                    if (GodOn)
                    {
                        Body pb = PlayerBody();
                        if (pb != null) RestoreBody(pb);
                        Notify("上帝模式：开启（无敌 / 全状态满值）");
                    }
                    else
                    {
                        Notify("上帝模式：关闭");
                    }
                }

                if (Input.GetKeyDown(KFly))
                {
                    FlyOn = !FlyOn;
                    Notify(FlyOn ? "上天入地：开启（WASD 飞行，Shift 加速）" : "上天入地：关闭");
                }

                if (Input.GetKeyDown(KSkill))
                {
                    Body pb2 = PlayerBody();
                    if (pb2 != null)
                    {
                        MaxSkills(pb2);
                        int lv = (pb2.skills != null) ? pb2.skills.STR : 0;
                        Notify("技能已拉满 (STR/RES/INT = " + lv + ")");
                    }
                    else Notify("未找到玩家角色");
                }

                if (Input.GetKeyDown(KHeal))
                {
                    Body pb3 = PlayerBody();
                    if (pb3 != null)
                    {
                        RestoreBody(pb3);
                        RefreshMoodles();
                        Notify("已清除全部负面状态（治疗 + 情绪 + 中毒…）");
                    }
                    else Notify("未找到玩家角色");
                }

                if (Input.GetKeyDown(KTp))
                {
                    if (RunCmd("tp")) Notify("已传送到鼠标位置");
                    else Notify("传送失败（游戏世界未就绪）");
                }

                if (Input.GetKeyDown(KLight))
                {
                    if (RunCmd("fullbright"))
                    {
                        LightOn = !LightOn;
                        Notify(LightOn ? "全亮：开启" : "全亮：关闭");
                    }
                }

                if (Input.GetKeyDown(KKit))
                {
                    if (RunCmd("starterkit")) Notify("已领取初始装备包");
                }

                if (Input.GetKeyDown(KSpawner))
                {
                    SpawnerOn = !SpawnerOn;
                    if (SpawnerOn)
                    {
                        BuildSpawnList(true);
                        UiShownDirty = true;
                        BuildUi();
                        // 第一次打开时把生成点默认设在角色脚下，这样「自己选的点」模式不会指到 (0,0)
                        if (!SpawnAnchorSet) SetAnchorFromPlayer();
                        UiSetVisible(true);
                        Notify("生成器：开启（共 " + SpawnIds.Count + " 种，"
                            + SpawnCats_Sorted.Count + " 个分类）  当前位置：" + PosModeName() + PosInfo());
                    }
                    else
                    {
                        UiSetVisible(false);
                        Notify("生成器：关闭");
                    }
                }

                Body p = PlayerBody();
                if (GodOn && p != null) RestoreBody(p);
                SyncNoClip();
                ClampFlight();
                RepairTraders();
                ProcessSpawnQueue();
                UiRefresh();
            }
            catch (Exception e)
            {
                if (Time.unscaledTime > NextErrLog)
                {
                    NextErrLog = Time.unscaledTime + 5f;
                    if (L != null) L.LogError("[GodMode] Tick 异常(已限流上报): " + e);
                }
            }

            Heartbeat();
        }

        // 诊断：把实际收到的按键写进日志
        internal static void ScanKeys()
        {
            if (L == null) return;
            if (CDebug == null || !CDebug.Value) return;
            for (int k = 1; k <= 320; k++)
            {
                if (Input.GetKeyDown((KeyCode)k))
                {
                    L.LogInfo("[GodMode][按键] 收到 KeyCode " + k + " = " + ((KeyCode)k).ToString());
                }
            }
        }

        // 诊断：每 5 秒报告一次运行状态
        internal static void Heartbeat()
        {
            if (L == null) return;
            if (CDebug == null || !CDebug.Value) return;
            if (Time.unscaledTime < NextBeat) return;
            NextBeat = Time.unscaledTime + 5f;

            string player;
            PlayerCamera cam = PlayerCamera.main;
            if (cam == null) player = "无相机";
            else if (cam.body == null) player = "有相机无身体";
            else player = "就绪";

            string console = ConsoleScript.instance == null ? "无" : "有";

            float hw, hh;
            string world = WorldLimits(out hw, out hh) ? ("±" + (int)hw + "x±" + (int)hh) : "未就绪";

            L.LogInfo("[GodMode][心跳] 帧=" + Time.frameCount
                + " 调度=" + TickCount
                + " 驱动(插件Update/相机补丁/控制台补丁/自建HUD)=" + DrvPlugin + "/" + DrvCamera + "/" + DrvConsole + "/" + DrvHud
                + " 窗口焦点=" + Application.isFocused
                + " 任意键按住=" + Input.anyKey
                + " 玩家=" + player
                + " 控制台=" + console
                + " 世界=" + world
                + " 限位=" + ClampCount + " NPC修正=" + TraderSkipCount + " 技能修正=" + SkillFixCount
                + " 上帝=" + GodOn + " 飞行=" + FlyOn);
        }

        // ==================================================================
        //  v0.7.0  uGUI 界面
        //
        //  为什么不用 IMGUI（实测结论，别再改回去）：
        //    * 插件自身的 Update / OnGUI 不被 Unity 调用（心跳里 插件Update 恒为 0）;
        //    * 自己 new GameObject + AddComponent 出来的组件，Update 同样不被调用
        //      （心跳里 自建HUD 恒为 0），所以挂在上面的 OnGUI 也不会触发;
        //    * 游戏本体 Assembly-CSharp.dll 与 QoL 模组里声明 OnGUI 的方法数都是 0,
        //      没有任何现成的 OnGUI 宿主可以挂。
        //  改用 uGUI：Canvas 的渲染由 Unity 的 Canvas 系统驱动，不依赖我们组件的
        //  Update，所以即使 Update 永不触发也照样显示。鼠标/键盘交互放在已经被证实
        //  每帧都在跑的 ConsoleScript.Update 后置补丁（Tick）里手工做命中测试，
        //  这也是 QoL 的 QoLMenuButton 用的办法。
        // ==================================================================

        internal const int UI_ROWS = 16;
        internal const float UI_PANEL_W = 900f;
        internal const float UI_ROW_H = 24f;
        internal const float UI_LIST_TOP = 178f;

        internal const int A_CAT_PREV = 1;
        internal const int A_CAT_NEXT = 2;
        internal const int A_POS = 3;
        internal const int A_COND_DN = 4;
        internal const int A_COND_UP = 5;
        internal const int A_PICKUP = 6;
        internal const int A_ALL = 7;
        internal const int A_R5 = 8;
        internal const int A_R20 = 9;
        internal const int A_STOP = 10;
        internal const int A_CLOSE = 11;
        internal const int A_CLEARF = 12;
        internal const int A_PGUP = 13;
        internal const int A_PGDN = 14;
        internal const int A_SEARCH = 15;
        internal const int A_G_ALL = 20;     // 顶层分组按钮：全部
        internal const int A_G_ITEM = 21;    // 物品
        internal const int A_G_LIFE = 22;    // 生物
        internal const int A_G_PLANT = 23;   // 植物
        internal const int A_G_BUILD = 24;   // 建筑
        internal const int A_G_BLOCK = 25;   // 方块
        internal const int A_CAT_ALL = 26;   // 子分类=全部
        internal const int A_PICKPOS = 27;   // 选位置（在世界上点一下定生成点）

        // 顶层分组名，顺序必须和 A_G_ALL..A_G_BLOCK 一致
        internal static readonly string[] GroupNames = { "全部", "物品", "生物", "植物", "建筑", "方块" };

        internal class UiRow
        {
            public RectTransform Rt;
            public Image Bg;
            public TextMeshProUGUI Tx;
        }

        internal class UiBtn
        {
            public RectTransform Rt;
            public Image Bg;
            public TextMeshProUGUI Tx;
            public int Act;
        }

        internal static GameObject UiRoot;
        internal static Canvas UiCanvas;
        internal static RectTransform UiPanel;
        internal static TextMeshProUGUI UiTitle;
        internal static TextMeshProUGUI UiInfo;
        internal static TextMeshProUGUI UiHint;
        internal static TextMeshProUGUI UiStatus;
        internal static readonly List<UiRow> UiRowList = new List<UiRow>();
        internal static readonly List<UiBtn> UiBtnList = new List<UiBtn>();
        internal static readonly List<int> UiShown = new List<int>();
        internal static bool UiBuilt = false;
        internal static bool UiFailed = false;
        internal static TMP_FontAsset UiFont;
        internal static int UiPage = 0;
        internal static int UiSel = 0;
        internal static int UiHover = -1;
        internal static bool UiShownDirty = true;
        internal static int UiShownStamp = -1;
        internal static bool UiSearchMode = false;
        internal static float UiNextRefresh = 0f;
        internal static bool UiPrevCursorVisible = true;
        internal static CursorLockMode UiPrevCursorLock = CursorLockMode.None;
        internal static ConfigEntry<bool> CPauseOpen;
        internal static float UiPrevTimeScale = 1f;
        internal static bool UiPaused = false;
        internal static bool UiModal = false;                 // 当前是否处于「窗口开着」的模态状态（冻时间+显示鼠标）
        internal static RectTransform UiCrossRt;              // 生成点十字标记（挂在 Canvas 根上，面板收起也看得见）
        internal static Image UiCrossBg;                      // 标记底色（选点模式下变红）
        internal static TextMeshProUGUI UiCrossTx;

        internal static RectTransform NewRect(string name, Transform parent)
        {
            GameObject g = new GameObject(name);
            g.hideFlags = HideFlags.HideAndDontSave;
            g.transform.SetParent(parent, false);
            RectTransform rt = g.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(100f, UI_ROW_H);
            return rt;
        }

        internal static TextMeshProUGUI NewText(string name, Transform parent, int size, Color col, TextAlignmentOptions align)
        {
            RectTransform rt = NewRect(name, parent);
            TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            try { if (UiFont != null) t.font = UiFont; }
            catch { }
            t.fontSize = size;
            t.color = col;
            t.alignment = align;
            try { t.enableWordWrapping = false; }
            catch { }
            try { t.overflowMode = TextOverflowModes.Overflow; }
            catch { }
            t.raycastTarget = false;
            return t;
        }

        // 在 parent 里铺满（带内边距）
        internal static void Fill(RectTransform child, float padX, float padY)
        {
            child.anchorMin = new Vector2(0f, 0f);
            child.anchorMax = new Vector2(1f, 1f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.offsetMin = new Vector2(padX, padY);
            child.offsetMax = new Vector2(-padX, -padY);
        }

        internal static TMP_FontAsset FindUiFont()
        {
            try
            {
                PlayerCamera cam = PlayerCamera.main;
                if (cam != null && cam.alertText != null && cam.alertText.font != null) return cam.alertText.font;
            }
            catch { }
            try
            {
                if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
            }
            catch { }
            try
            {
                TextMeshProUGUI[] all = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
                if (all != null)
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        if (all[i] != null && all[i].font != null) return all[i].font;
                    }
                }
            }
            catch { }
            return null;
        }

        internal static void AddBtn(float x, float y, float w, int act)
        {
            RectTransform rt = NewRect("Btn" + act, UiPanel);
            rt.sizeDelta = new Vector2(w, 26f);
            rt.anchoredPosition = new Vector2(x, -y);
            Image bg = rt.gameObject.AddComponent<Image>();
            bg.raycastTarget = false;
            bg.color = new Color(0.22f, 0.27f, 0.38f, 0.98f);
            TextMeshProUGUI t = NewText("Tx", rt, 15, Color.white, TextAlignmentOptions.Center);
            Fill(t.rectTransform, 2f, 1f);
            UiBtn b = new UiBtn();
            b.Rt = rt;
            b.Bg = bg;
            b.Tx = t;
            b.Act = act;
            UiBtnList.Add(b);
        }

        internal static void BuildUi()
        {
            if (UiBuilt || UiFailed) return;
            try
            {
                UiFont = FindUiFont();

                GameObject go = new GameObject("GodModeUI");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.HideAndDontSave;
                UiRoot = go;

                UiCanvas = go.AddComponent<Canvas>();
                UiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                UiCanvas.sortingOrder = 30000;
                CanvasScaler sc = go.AddComponent<CanvasScaler>();
                sc.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                sc.scaleFactor = 1f;
                sc.referencePixelsPerUnit = 100f;
                go.AddComponent<GraphicRaycaster>();

                // 左上角常驻状态字（原来那套 IMGUI 的黄字永远画不出来）
                UiStatus = NewText("Status", go.transform, 20, new Color(1f, 0.92f, 0.35f, 1f), TextAlignmentOptions.TopLeft);
                UiStatus.rectTransform.sizeDelta = new Vector2(900f, 30f);
                UiStatus.rectTransform.anchoredPosition = new Vector2(16f, -8f);

                // 主面板
                UiPanel = NewRect("Panel", go.transform);
                Image pbg = UiPanel.gameObject.AddComponent<Image>();
                pbg.raycastTarget = false;
                pbg.color = new Color(0.05f, 0.06f, 0.09f, 0.95f);
                float ph = Mathf.Min(UI_LIST_TOP + UI_ROWS * UI_ROW_H + 56f, Screen.height - 40f);
                UiPanel.sizeDelta = new Vector2(UI_PANEL_W, ph);
                UiPanel.anchoredPosition = new Vector2(40f, -40f);

                UiTitle = NewText("Title", UiPanel, 21, new Color(1f, 0.9f, 0.45f, 1f), TextAlignmentOptions.TopLeft);
                UiTitle.rectTransform.sizeDelta = new Vector2(UI_PANEL_W - 20f, 32f);
                UiTitle.rectTransform.anchoredPosition = new Vector2(10f, -6f);

                UiInfo = NewText("Info", UiPanel, 15, new Color(0.82f, 0.88f, 1f, 1f), TextAlignmentOptions.TopLeft);
                UiInfo.rectTransform.sizeDelta = new Vector2(UI_PANEL_W - 20f, 44f);
                UiInfo.rectTransform.anchoredPosition = new Vector2(10f, -42f);

                // 第一排：顶层分组
                AddBtn(10f, 88f, 56f, A_G_ALL);
                AddBtn(70f, 88f, 56f, A_G_ITEM);
                AddBtn(130f, 88f, 56f, A_G_LIFE);
                AddBtn(190f, 88f, 56f, A_G_PLANT);
                AddBtn(250f, 88f, 56f, A_G_BUILD);
                AddBtn(310f, 88f, 56f, A_G_BLOCK);
                AddBtn(372f, 88f, 96f, A_SEARCH);
                AddBtn(472f, 88f, 110f, A_CLEARF);
                AddBtn(706f, 88f, 78f, A_CLOSE);

                // 第二排：子分类 + 翻页
                AddBtn(10f, 116f, 64f, A_CAT_PREV);
                AddBtn(78f, 116f, 300f, 0);            // act=0 纯标签，显示当前子分类
                AddBtn(382f, 116f, 64f, A_CAT_NEXT);
                AddBtn(450f, 116f, 110f, A_CAT_ALL);
                AddBtn(624f, 116f, 78f, A_PGUP);
                AddBtn(706f, 116f, 78f, A_PGDN);

                // 第三排：生成选项与批量生成
                AddBtn(10f, 144f, 150f, A_POS);
                AddBtn(164f, 144f, 88f, A_PICKPOS);
                AddBtn(256f, 144f, 78f, A_COND_DN);
                AddBtn(338f, 144f, 78f, A_COND_UP);
                AddBtn(420f, 144f, 130f, A_PICKUP);
                AddBtn(554f, 144f, 140f, A_ALL);
                AddBtn(698f, 144f, 62f, A_R5);
                AddBtn(764f, 144f, 62f, A_R20);
                AddBtn(830f, 144f, 60f, A_STOP);

                // 生成点标记：跟着「自己选的点」在世界里的屏幕位置走
                UiCrossRt = NewRect("Cross", go.transform);
                UiCrossRt.anchorMin = new Vector2(0f, 0f);
                UiCrossRt.anchorMax = new Vector2(0f, 0f);
                UiCrossRt.pivot = new Vector2(0.5f, 0.5f);
                UiCrossRt.sizeDelta = new Vector2(24f, 24f);
                Image crossBg = UiCrossRt.gameObject.AddComponent<Image>();
                crossBg.raycastTarget = false;
                crossBg.color = new Color(0f, 0f, 0f, 0.65f);
                UiCrossBg = crossBg;
                RectTransform core = NewRect("Core", UiCrossRt);
                core.anchorMin = new Vector2(0.5f, 0.5f);
                core.anchorMax = new Vector2(0.5f, 0.5f);
                core.pivot = new Vector2(0.5f, 0.5f);
                core.sizeDelta = new Vector2(10f, 10f);
                core.anchoredPosition = Vector2.zero;
                Image coreImg = core.gameObject.AddComponent<Image>();
                coreImg.raycastTarget = false;
                coreImg.color = new Color(1f, 0.85f, 0.2f, 1f);
                UiCrossTx = NewText("CrossTx", go.transform, 16, new Color(1f, 0.9f, 0.4f, 1f), TextAlignmentOptions.BottomLeft);
                UiCrossTx.rectTransform.anchorMin = new Vector2(0f, 0f);
                UiCrossTx.rectTransform.anchorMax = new Vector2(0f, 0f);
                UiCrossTx.rectTransform.pivot = new Vector2(0f, 0f);
                UiCrossTx.rectTransform.sizeDelta = new Vector2(320f, 24f);
                UiCrossRt.gameObject.SetActive(false);
                UiCrossTx.gameObject.SetActive(false);

                for (int r = 0; r < UI_ROWS; r++)
                {
                    RectTransform rt = NewRect("Row" + r, UiPanel);
                    rt.sizeDelta = new Vector2(UI_PANEL_W - 20f, UI_ROW_H);
                    rt.anchoredPosition = new Vector2(10f, -(UI_LIST_TOP + r * UI_ROW_H));
                    Image bg = rt.gameObject.AddComponent<Image>();
                    bg.raycastTarget = false;
                    bg.color = new Color(1f, 1f, 1f, 0f);
                    TextMeshProUGUI t = NewText("Tx", rt, 16, Color.white, TextAlignmentOptions.Left);
                    Fill(t.rectTransform, 8f, 0f);
                    UiRow u = new UiRow();
                    u.Rt = rt;
                    u.Bg = bg;
                    u.Tx = t;
                    UiRowList.Add(u);
                }

                UiHint = NewText("Hint", UiPanel, 14, new Color(0.62f, 0.68f, 0.78f, 1f), TextAlignmentOptions.TopLeft);
                UiHint.rectTransform.sizeDelta = new Vector2(UI_PANEL_W - 20f, 46f);
                UiHint.rectTransform.anchoredPosition = new Vector2(10f, -(UI_LIST_TOP + UI_ROWS * UI_ROW_H + 4f));

                UiPanel.gameObject.SetActive(false);
                UiBuilt = true;
                if (L != null)
                    L.LogInfo("[GodMode][界面] uGUI 窗口已创建，字体 = "
                        + (UiFont != null ? UiFont.name : "（未找到游戏字体，将使用 TMP 默认字体）"));
            }
            catch (Exception e)
            {
                UiFailed = true;
                if (L != null) L.LogError("[GodMode][界面] 创建 uGUI 失败: " + e);
            }
        }

        internal static void UiSetVisible(bool v)
        {
            if (!UiBuilt) return;
            try { UiPanel.gameObject.SetActive(v); }
            catch { }
            if (v)
            {
                UiPrevCursorVisible = Cursor.visible;
                UiPrevCursorLock = Cursor.lockState;
                try { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
                catch { }
                // 冻住游戏时间：否则点界面的时候角色会同时挥拳/用掉手里的道具
                // v0.9.0：只在 timeScale > 0 时才记账，免得把 0 存下来又恢复成 0（那样子游戏会永久冻住）
                if (CPauseOpen != null && CPauseOpen.Value && !UiPaused && Time.timeScale > 0f)
                {
                    UiPrevTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                    UiPaused = true;
                }
            }
            else
            {
                try { Cursor.visible = UiPrevCursorVisible; Cursor.lockState = UiPrevCursorLock; }
                catch { }
                if (UiPaused)
                {
                    if (UiPrevTimeScale > 0f) Time.timeScale = UiPrevTimeScale;
                    UiPaused = false;
                }
            }
        }

        internal static int UiPageCount()
        {
            if (UiShown.Count <= 0) return 1;
            return (UiShown.Count + UI_ROWS - 1) / UI_ROWS;
        }

        internal static void UiClampPage()
        {
            int pc = UiPageCount();
            if (UiPage < 0) UiPage = 0;
            if (UiPage > pc - 1) UiPage = pc - 1;
            if (UiSel < 0) UiSel = 0;
            if (UiSel > UI_ROWS - 1) UiSel = UI_ROWS - 1;
        }

        internal static void UiRebuildShown()
        {
            UiShown.Clear();
            for (int i = 0; i < SpawnIds.Count; i++)
            {
                if (MatchFilter(i)) UiShown.Add(i);
            }
            UiShownStamp = SpawnIds.Count;
            UiClampPage();
        }

        // 当前分组下有哪些子分类（"全部" 永远在第一个）
        internal static List<string> CatListForGroup()
        {
            List<string> res = new List<string>();
            res.Add("全部");
            for (int i = 0; i < SpawnCats_Sorted.Count; i++)
            {
                string c = SpawnCats_Sorted[i];
                if (c == "全部") continue;
                bool ok = (SpawnGroup == "全部")
                    || c == SpawnGroup
                    || c.StartsWith(SpawnGroup + "/");
                if (ok && !res.Contains(c)) res.Add(c);
            }
            return res;
        }

        internal static void CycleCat(int d)
        {
            List<string> list = CatListForGroup();
            if (list.Count == 0) return;
            int cur = list.IndexOf(SpawnCat);
            if (cur < 0) cur = 0;
            cur = (cur + d) % list.Count;
            if (cur < 0) cur += list.Count;
            SpawnCat = list[cur];
            UiPage = 0;
            UiSel = 0;
            UiShownDirty = true;
        }

        internal static void SpawnRow(int row)
        {
            int idx = UiPage * UI_ROWS + row;
            if (idx < 0 || idx >= UiShown.Count) return;
            int i = UiShown[idx];
            SpawnQueue.Clear();
            Vector2 p = SpawnPos();
            string at = " @ " + Mathf.RoundToInt(p.x) + "," + Mathf.RoundToInt(p.y);
            if (SpawnOne(i, p))
            {
                Notify("已生成：" + SpawnNames[i] + at);
                if (L != null) L.LogInfo("[GodMode][生成器] 已生成 " + SpawnIds[i] + "（" + SpawnNames[i] + "）"
                    + at + "  分类=" + SpawnCats[i]);
            }
            else
            {
                Notify("生成失败：" + SpawnNames[i]);
                if (L != null) L.LogWarning("[GodMode][生成器] 生成失败 " + SpawnIds[i] + at);
            }
        }

        internal static void DoAction(int act)
        {
            if (act >= A_G_ALL && act <= A_G_BLOCK)
            {
                SpawnGroup = GroupNames[act - A_G_ALL];
                SpawnCat = "全部";
                UiPage = 0;
                UiSel = 0;
                UiShownDirty = true;
                return;
            }
            if (act == A_CAT_ALL) { SpawnCat = "全部"; UiPage = 0; UiSel = 0; UiShownDirty = true; return; }
            if (act == A_CAT_PREV) { CycleCat(-1); return; }
            if (act == A_CAT_NEXT) { CycleCat(1); return; }
            if (act == A_POS)
            {
                int m = (CSpawnPosMode != null) ? CSpawnPosMode.Value : 2;
                m = (m + 1) % 3;
                if (CSpawnPosMode != null) CSpawnPosMode.Value = m;
                if (m == 2 && !SpawnAnchorSet) SetAnchorFromPlayer();
                Notify("生成位置：" + PosModeName());
                return;
            }
            if (act == A_PICKPOS)
            {
                UiPickPos = !UiPickPos;
                if (UiPickPos) Notify("选位置：用鼠标点世界里任意位置（Esc 或右键取消，方向键微调，Shift+方向键细调）");
                else Notify("已取消选位置");
                return;
            }
            if (act == A_COND_DN) { if (CSpawnCond != null) CSpawnCond.Value = Mathf.Clamp01(CSpawnCond.Value - 0.1f); return; }
            if (act == A_COND_UP) { if (CSpawnCond != null) CSpawnCond.Value = Mathf.Clamp01(CSpawnCond.Value + 0.1f); return; }
            if (act == A_PICKUP) { if (CSpawnPickup != null) CSpawnPickup.Value = !CSpawnPickup.Value; return; }
            if (act == A_ALL) { SpawnAllFiltered(); Notify("正在生成筛选结果…"); return; }
            if (act == A_R5) { SpawnRandom(5); return; }
            if (act == A_R20) { SpawnRandom(20); return; }
            if (act == A_STOP) { SpawnQueue.Clear(); Notify("已停止生成队列"); return; }
            if (act == A_CLOSE) { SpawnerOn = false; return; }
            if (act == A_CLEARF) { SpawnFilter = ""; UiShownDirty = true; return; }
            if (act == A_PGUP) { UiPage--; UiClampPage(); return; }
            if (act == A_PGDN) { UiPage++; UiClampPage(); return; }
            if (act == A_SEARCH) { UiSearchMode = !UiSearchMode; return; }
        }

        internal static void UiInput()
        {
            Vector2 mp = Input.mousePosition;

            // ---- 选位置模式：鼠标扫过世界 = 移动锚点，点一下 = 定下来 ----
            if (UiPickPos)
            {
                bool overPanel = false;
                try { overPanel = (UiPanel != null) && RectTransformUtility.RectangleContainsScreenPoint(UiPanel, mp, null); }
                catch { overPanel = false; }

                if (!overPanel) AimAtScreen(mp);

                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    UiPickPos = false;
                    Notify("已取消选位置");
                    return;
                }

                float step = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0.25f : 1f;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) SpawnAnchor = ClampToWorld(SpawnAnchor + new Vector2(-step, 0f));
                if (Input.GetKeyDown(KeyCode.RightArrow)) SpawnAnchor = ClampToWorld(SpawnAnchor + new Vector2(step, 0f));
                if (Input.GetKeyDown(KeyCode.UpArrow)) SpawnAnchor = ClampToWorld(SpawnAnchor + new Vector2(0f, step));
                if (Input.GetKeyDown(KeyCode.DownArrow)) SpawnAnchor = ClampToWorld(SpawnAnchor + new Vector2(0f, -step));

                if (Input.GetMouseButtonDown(0) && !overPanel)
                {
                    SpawnAnchorSet = true;
                    if (CSpawnPosMode != null) CSpawnPosMode.Value = 2;
                    UiPickPos = false;
                    Notify("生成点已设为 " + Mathf.RoundToInt(SpawnAnchor.x) + ", " + Mathf.RoundToInt(SpawnAnchor.y)
                        + "（点列表里的东西就会生成在这里）");
                    return;
                }
            }

            UiHover = -1;
            for (int i = 0; i < UiRowList.Count; i++)
            {
                int idx = UiPage * UI_ROWS + i;
                if (idx >= UiShown.Count) break;
                if (RectTransformUtility.RectangleContainsScreenPoint(UiRowList[i].Rt, mp, null))
                {
                    UiHover = i;
                    break;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (UiHover >= 0) { UiSel = UiHover; SpawnRow(UiHover); return; }
                for (int i = 0; i < UiBtnList.Count; i++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(UiBtnList[i].Rt, mp, null))
                    {
                        DoAction(UiBtnList[i].Act);
                        return;
                    }
                }
            }

            float wheel = Input.mouseScrollDelta.y;
            if (wheel > 0.01f) { UiPage--; UiClampPage(); }
            else if (wheel < -0.01f) { UiPage++; UiClampPage(); }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UiSearchMode) UiSearchMode = false;
                else SpawnerOn = false;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Slash)) UiSearchMode = !UiSearchMode;

            if (UiSearchMode)
            {
                string s = Input.inputString;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '\b')
                    {
                        if (SpawnFilter.Length > 0) SpawnFilter = SpawnFilter.Substring(0, SpawnFilter.Length - 1);
                        UiShownDirty = true;
                    }
                    else if (c == '\n' || c == '\r') UiSearchMode = false;
                    else if (c >= ' ') { SpawnFilter = SpawnFilter + c; UiShownDirty = true; }
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.DownArrow)) { UiSel++; UiClampPage(); }
                if (Input.GetKeyDown(KeyCode.UpArrow)) { UiSel--; UiClampPage(); }
                if (Input.GetKeyDown(KeyCode.PageDown)) { UiPage++; UiClampPage(); }
                if (Input.GetKeyDown(KeyCode.PageUp)) { UiPage--; UiClampPage(); }
                if (Input.GetKeyDown(KeyCode.Home)) { UiPage = 0; UiSel = 0; }
                if (Input.GetKeyDown(KeyCode.End)) { UiPage = UiPageCount() - 1; UiClampPage(); }
                if (Input.GetKeyDown(KeyCode.Tab)) CycleCat(1);
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) SpawnRow(UiSel);
            }
        }

        internal static void UiRefresh()
        {
            if (!UiBuilt) return;

            try
            {
                // 常驻状态字
                string st = "";
                if (GodOn) st = st + "★ 上帝模式 ON   [F1 关]    ";
                if (FlyOn) st = st + "★ 上天入地 ON   [F2 关]    ";
                if (SpawnerOn) st = st + "★ 生成器   [F8 关]";
                if (UiStatus != null) UiStatus.text = st;

                // 窗口的显示/隐藏由这里统一处理：无论按键、按钮还是 Esc 关的，都能正确收起
                // 并恢复鼠标指针与 Time.timeScale
                if (UiPanel != null && UiPanel.gameObject.activeSelf != SpawnerOn) UiSetVisible(SpawnerOn);

                // v0.9.0 自愈：万一 timeScale 被冻住而窗口已经关了，这里强制解冻
                if (!SpawnerOn && UiPaused)
                {
                    try { if (UiPrevTimeScale > 0f) Time.timeScale = UiPrevTimeScale; }
                    catch { }
                    UiPaused = false;
                }

                UpdateCross();

                if (!SpawnerOn) return;

                UiInput();

                if (UiShownDirty || UiShownStamp != SpawnIds.Count) { UiRebuildShown(); UiShownDirty = false; }

                if (Time.unscaledTime < UiNextRefresh) return;
                UiNextRefresh = Time.unscaledTime + 0.1f;

                int pc = UiPageCount();
                UiClampPage();

                if (UiTitle != null)
                    UiTitle.text = "生成器（物品 / 生物 / 怪物 / 植物 / 建筑 / 方块）- 全部 " + SpawnIds.Count
                        + " 种 / 当前筛选 " + UiShown.Count + " 件 / 第 " + (UiPage + 1) + "/" + pc + " 页";

                float cond = (CSpawnCond != null) ? CSpawnCond.Value : 1f;
                bool pk = (CSpawnPickup != null && CSpawnPickup.Value);
                string q = (SpawnQueue.Count > 0)
                    ? ("   生成中 " + SpawnDone + "/" + SpawnTotal + " 失败 " + SpawnFail + "，剩 " + SpawnQueue.Count)
                    : "";
                if (UiInfo != null)
                    UiInfo.text = "分组: " + SpawnGroup + "    子分类: " + SpawnCat
                        + "（本分组 " + CatListForGroup().Count + " 类，◀▶ 或 Tab 切换）\n"
                        + "筛选: " + (SpawnFilter.Length > 0 ? SpawnFilter : "(空)") + (UiSearchMode ? "  << 输入中 >>" : "（按 / 输入）")
                        + "   放置: " + PosModeName() + PosInfo()
                        + "   耐久: " + Mathf.RoundToInt(cond * 100f) + "%"
                        + "   拾取: " + (pk ? "开" : "关") + q;

                for (int i = 0; i < UiRowList.Count; i++)
                {
                    int idx = UiPage * UI_ROWS + i;
                    UiRow u = UiRowList[i];
                    if (idx >= UiShown.Count)
                    {
                        u.Tx.text = "";
                        u.Bg.color = new Color(1f, 1f, 1f, 0f);
                        continue;
                    }
                    int k = UiShown[idx];
                    string mark = (i == UiSel) ? "> " : "  ";
                    string extra = (SpawnKinds[k] == K_BLOCK) ? ("方块#" + SpawnBlocks[k]) : SpawnIds[k];
                    u.Tx.text = mark + "[" + SpawnCats[k] + "] " + SpawnNames[k] + "   (" + extra + ")";
                    if (i == UiHover) u.Bg.color = new Color(0.28f, 0.40f, 0.62f, 0.85f);
                    else if (i == UiSel) u.Bg.color = new Color(0.30f, 0.42f, 0.22f, 0.55f);
                    else if ((idx % 2) == 1) u.Bg.color = new Color(1f, 1f, 1f, 0.045f);
                    else u.Bg.color = new Color(1f, 1f, 1f, 0f);
                    u.Tx.color = (i == UiHover) ? Color.white : new Color(0.90f, 0.93f, 1f, 1f);
                }

                RefreshBtnLabels(cond, pk);

                if (UiHint != null)
                    UiHint.text = "鼠标左键点一行 = 生成    滚轮/PageUp/PageDown = 翻页    ↑↓ 选中 + 回车 = 生成\n"
                        + "第一排切换大分类    ◀▶/Tab 切子分类    「选点」后点世界任意位置 = 指定生成点    / 搜索    Esc 关窗口    F8 开关";
            }
            catch (Exception e)
            {
                if (Time.unscaledTime > NextErrLog)
                {
                    NextErrLog = Time.unscaledTime + 5f;
                    if (L != null) L.LogWarning("[GodMode][界面] 刷新异常: " + e.Message);
                }
            }
        }

        internal static void RefreshBtnLabels(float cond, bool pk)
        {
            for (int i = 0; i < UiBtnList.Count; i++)
            {
                UiBtn b = UiBtnList[i];
                string s = b.Tx.text;
                if (b.Act >= A_G_ALL && b.Act <= A_G_BLOCK) s = GroupNames[b.Act - A_G_ALL];
                else if (b.Act == 0) s = SpawnCat;
                else if (b.Act == A_CAT_ALL) s = "子分类=全部";
                else if (b.Act == A_CAT_PREV) s = "<<";
                else if (b.Act == A_CAT_NEXT) s = ">>";
                else if (b.Act == A_POS) s = "位置: " + PosModeName();
                else if (b.Act == A_PICKPOS) s = UiPickPos ? "选点中…" : "选点";
                else if (b.Act == A_COND_DN) s = "耐久 -10%";
                else if (b.Act == A_COND_UP) s = "耐久 +10%";
                else if (b.Act == A_PICKUP) s = pk ? "自动拾取: 开" : "自动拾取: 关";
                else if (b.Act == A_SEARCH) s = UiSearchMode ? "搜索: 输入中" : "搜索: 关闭";
                else if (b.Act == A_ALL) s = "生成全部 (" + UiShown.Count + ")";
                else if (b.Act == A_R5) s = "随机 5";
                else if (b.Act == A_R20) s = "随机 20";
                else if (b.Act == A_STOP) s = "停止 (" + SpawnQueue.Count + ")";
                else if (b.Act == A_CLEARF) s = "清空筛选";
                else if (b.Act == A_PGUP) s = "上一页";
                else if (b.Act == A_PGDN) s = "下一页";
                else if (b.Act == A_CLOSE) s = "关闭";
                if (b.Tx.text != s) b.Tx.text = s;

                if (b.Act >= A_G_ALL && b.Act <= A_G_BLOCK && GroupNames[b.Act - A_G_ALL] == SpawnGroup)
                    b.Bg.color = new Color(0.30f, 0.46f, 0.30f, 0.98f);
                else if (b.Act == A_PICKPOS && UiPickPos) b.Bg.color = new Color(0.62f, 0.32f, 0.18f, 0.98f);
                else if (b.Act == A_PICKPOS && ((CSpawnPosMode != null ? CSpawnPosMode.Value : 2) == 2))
                    b.Bg.color = new Color(0.30f, 0.46f, 0.30f, 0.98f);
                else if (b.Act == A_SEARCH && UiSearchMode) b.Bg.color = new Color(0.55f, 0.42f, 0.15f, 0.98f);
                else if (b.Act == A_STOP && SpawnQueue.Count > 0) b.Bg.color = new Color(0.55f, 0.20f, 0.20f, 0.98f);
                else b.Bg.color = new Color(0.22f, 0.27f, 0.38f, 0.98f);
            }
        }

        // IMGUI 兜底：本游戏里不会触发（无 OnGUI 宿主），保留空实现避免误用
        void OnGUI()
        {
        }

        internal static void DrawGui()
        {
        }

        // ------------------------------------------------------------------
        //  核心：把玩家的身体恢复到「满状态」
        // ------------------------------------------------------------------
        internal static Body PlayerBody()
        {
            PlayerCamera cam = PlayerCamera.main;
            if (cam == null) return null;
            return cam.body;
        }

        internal static bool IsPlayer(Body b)
        {
            if (b == null) return false;
            Body p = PlayerBody();
            if (p == null) return false;
            return b == p;
        }

        internal static void RestoreBody(Body b)
        {
            if (b == null) return;
            Remember(b.skills);   // 登记给看门狗，主线程卡死时它能从后台线程抢救

            b.brainHealth = V_MAX;      // alive == (brainHealth > 0)，这就是不死的关键
            b.bloodVolume = V_MAX;
            b.bloodOxygen = V_MAX;
            b.bloodPressure = V_PRESSURE;
            b.hunger = V_MAX;
            b.thirst = V_MAX;
            b.stamina = V_MAX;
            b.energy = V_MAX;
            b.internalBleeding = 0f;
            b.totalBleedSpeed = 0f;
            b.painShock = 0f;
            b.averagePain = 0f;
            b.consciousness = V_MAX;
            b.heartRate = V_HEARTRATE;
            b.respiratoryRate = V_RESPRATE;
            b.temperature = V_TEMP;

            // ---- 负面状态全清（v0.6.0）----
            // 依据：MoodleManager.UpdateMoodles() 的反编译结果 —— 屏幕上每一个「坏图标」
            // 都是由下面这些字段推导出来的，把它们压回健康值，图标自然消失。
            b.venomCurrent = 0f;          // venom1..4        中毒
            b.venomTotal = 0f;
            b.sicknessAmount = 0f;        // sick1..4         疾病
            b.radiationSickness = 0f;     //                  辐射病
            b.tempDiffFromNormal = 0f;
            b.wetness = 0f;               // wet1..4          潮湿
            b.snowAmount = 0f;
            b.dirtyness = 0f;             // dirty1..2        肮脏
            b.immunity = V_MAX;           // lowimmunity      免疫力
            b.curImmunityMult = 1f;
            b.antibioticImmunityTime = 0f;
            b.weightOffset = 0f;          // under/overweight 体重偏差
            b.traumaAmount = 0f;          // trauma2..4       心理创伤
            b.badSleepAmount = 0f;        // badsleep         睡眠不足
            b.brainGrowSickness = 0f;     // rnrs             脑生长病
            if (CMoodClear == null || CMoodClear.Value)
            {
                // 情绪/精神类 —— 不影响生存，但也是屏幕上的坏图标，默认一起清掉；
                // 想留着看情绪的话把配置里的 ClearMoodAndMind 改成 false
                b.happiness = V_MAX;      // depression/gloomy/miserable 情绪
                b.opiateHappiness = 0f;
                b.antidepressantHappiness = 0f;
                b.horrifiedLevel = 0f;
                if (b.mindWipe != null) b.mindWipe.active = false;   // hollow 精神空洞
                if (b.harmer != null) b.harmer.timeWasStill = 0f;    // 自残者计时（miserable 用）
            }
            b.curAdrenaline = 0f;         // adrenaline/fightorflight 应激
            b.hearingLoss = 0f;           // hearingloss/deaf 听力
            b.clawHealth = V_MAX;         // clawdamage       爪子损伤
            b.clawRegrowTime = 0f;
            b.overEncumberance = 0f;      // encumbered       超重
            b.strokeAmount = 0f;          //                  中风（也参与 brainDamaged 判定）
            b.hemothorax = 0f;
            b.septicShock = 0f;
            b.shock = 0f;
            b.fibrillationProgress = 0f;
            b.fibrillationForced = false;
            b.hasPulmonaryEmbolism = false;
            b.overdoseIndex = 0;
            b.disfigured = false;         // rippedjaw        毁容
            b.eyeGone = false;            // rippedeye        眼睛
            b.bothEyesGone = false;       // rippedeyes

            Limb[] ls = b.limbs;
            if (ls != null)
            {
                for (int i = 0; i < ls.Length; i++)
                {
                    Limb l = ls[i];
                    if (l == null) continue;      // Limb 是组件，被销毁时 == null 为真
                    l.skinHealth = V_MAX;
                    l.muscleHealth = V_MAX;
                    l.bleedAmount = 0f;
                    l.infectionAmount = 0f;
                    l.pain = 0f;
                    l.broken = false;
                    l.dislocated = false;
                    l.dismembered = false;
                    SetF(l, "infected", false);
                    SetF(l, "splinted", false);
                    SetF(l, "strokeAffected", false);
                    SetF(l, "shrapnel", 0);
                }
            }
        }

        // ------------------------------------------------------------------
        //  立刻刷新状态图标（v0.6.0）
        //  MoodleManager.UpdateMoodles() 的开头就是 UpdatePrevMoodles() + ClearMoodles() + AddAllMoodles()，
        //  所以手动调它一次，被清掉的负面状态图标会当场消失，不用等游戏自己 0.5 秒一次的定时刷新。
        // ------------------------------------------------------------------
        internal static void RefreshMoodles()
        {
            try
            {
                MoodleManager mm = MoodleManager.main;
                if (mm != null) mm.UpdateMoodles();
            }
            catch (Exception e)
            {
                if (L != null) L.LogWarning("[GodMode] 刷新状态图标失败: " + e.Message);
            }
        }

        internal static void MaxSkills(Body b)
        {
            if (b == null) return;
            Skills s = b.skills;
            if (s == null) return;

            int lv = (CMaxSkillLv != null) ? CMaxSkillLv.Value : 30;
            if (lv < 0) lv = 0;
            if (lv > SAFE_SKILL_LEVEL) lv = SAFE_SKILL_LEVEL;

            s.STR = lv;
            s.RES = lv;
            s.INT = lv;
            s.UpdateExpBoundaries();     // 必须重算 min/max，否则经验条与上限对不上
            s.expSTR = s.minSTR;         // 经验归到本级别起点
            s.expRES = s.minRES;
            s.expINT = s.minINT;
        }

        // ------------------------------------------------------------------
        //  上天入地：完全复刻 ConsoleScript 里 noclip 命令的逻辑
        // ------------------------------------------------------------------
        internal static ConsoleScript GetConsole()
        {
            ConsoleScript cs = ConsoleScript.instance;
            if (cs != null) return cs;
            if (Time.unscaledTime < NextConsoleCheck) return null;
            NextConsoleCheck = Time.unscaledTime + 1f;
            try { ConsoleScript.CheckForConsole(); }
            catch (Exception e)
            {
                if (L != null) L.LogWarning("[GodMode] CheckForConsole 失败: " + e.Message);
            }
            return ConsoleScript.instance;
        }

        internal static void SyncNoClip()
        {
            ConsoleScript cs = GetConsole();
            if (cs == null) return;
            if (cs.noClip == FlyOn) return;

            Body p = PlayerBody();
            if (p == null) return;

            if (FlyOn)
            {
                cs.noClipPos = (Vector2)p.transform.position;
            }
            cs.noClip = FlyOn;

            Rigidbody2D rb = GetF(p, "rb") as Rigidbody2D;
            if (rb != null) rb.simulated = !FlyOn;
        }

        internal static bool RunCmd(string cmd)
        {
            ConsoleScript cs = GetConsole();
            if (cs == null) return false;
            try
            {
                cs.ExecuteCommand(cmd);
                if (FlyOn) SyncNoClipNow();
                return true;
            }
            catch (Exception e)
            {
                if (L != null) L.LogWarning("[GodMode] 命令失败 " + cmd + ": " + e.Message);
                return false;
            }
        }

        internal static void SyncNoClipNow()
        {
            ConsoleScript cs = ConsoleScript.instance;
            if (cs == null) return;
            Body p = PlayerBody();
            if (p == null) return;
            if (FlyOn) cs.noClipPos = (Vector2)p.transform.position;
        }

        // ------------------------------------------------------------------
        //  提示：既写日志，也用游戏自己的弹窗提示，并在左上角常驻显示
        // ------------------------------------------------------------------
        internal static void Notify(string m)
        {
            Msg = m;
            MsgUntil = Time.unscaledTime + 4f;
            if (L != null) L.LogInfo("[GodMode] " + m);

            PlayerCamera cam = PlayerCamera.main;
            if (cam != null)
            {
                try { cam.DoAlert(m, false); }
                catch { }
            }
        }

        internal static KeyCode ParseKey(string s, KeyCode def)
        {
            if (s == null) return def;
            s = s.Trim();
            if (s.Length == 0) return def;
            try { return (KeyCode)Enum.Parse(typeof(KeyCode), s, true); }
            catch { return def; }
        }

        // ==================================================================
        //  世界边界保护
        //  游戏把世界坐标映射成数组下标：WorldToBlockPos(p) = ((int)(p.x+halfWidth), (int)(p.y+halfHeight))
        //  合法下标是 [0,width) x [0,height)。超出后游戏自己的 SetLiquid 会 Math.Clamp 兜住，
        //  但 TraderScript.OnWillRenderObject 直接写 fluid[x,y] 没兜 —— 那是卡死的根源。
        // ==================================================================

        internal static bool WorldLimits(out float hw, out float hh)
        {
            if (Time.unscaledTime >= NextWorldProbe)
            {
                NextWorldProbe = Time.unscaledTime + 1f;
                try
                {
                    WorldGeneration w = WorldGeneration.world;
                    if (w != null)
                    {
                        float a = (float)w.halfWidth;
                        float b = (float)w.halfHeight;
                        if (a > 8f && b > 8f) { WorldHW = a; WorldHH = b; }
                    }
                }
                catch { }
            }
            hw = WorldHW; hh = WorldHH;
            return hw > 8f && hh > 8f;
        }

        // 把坐标夹到世界内（留出安全边距）
        internal static Vector2 ClampPos(Vector2 p, float hw, float hh)
        {
            float m = (CMargin != null) ? (float)CMargin.Value : 12f;
            if (m < 0f) m = 0f;
            float lx = hw - m; if (lx < 1f) lx = 1f;
            float ly = hh - m; if (ly < 1f) ly = 1f;
            float x = p.x, y = p.y;
            if (x > lx) x = lx; else if (x < -lx) x = -lx;
            if (y > ly) y = ly; else if (y < -ly) y = -ly;
            return new Vector2(x, y);
        }

        // 飞行时把玩家钳制在世界内。钩子在 ConsoleScript.Update 之后跑，
        // 正好接在游戏自己的 noclip 位移之后，所以玩家不可能停在界外。
        internal static void ClampFlight()
        {
            if (!FlyOn) return;
            ConsoleScript cs = ConsoleScript.instance;
            if (cs == null || !cs.noClip) return;

            float hw, hh;
            if (!WorldLimits(out hw, out hh)) return;

            Vector2 p = cs.noClipPos;
            Vector2 c = ClampPos(p, hw, hh);
            if (c.x == p.x && c.y == p.y) return;

            cs.noClipPos = c;
            Body b = PlayerBody();
            if (b != null && b.transform != null)
                b.transform.position = new Vector3(c.x, c.y, b.transform.position.z);

            ClampCount++;
            if (Time.unscaledTime >= NextLimitMsg)
            {
                NextLimitMsg = Time.unscaledTime + 6f;
                Notify("已到世界边界，飞行限位中（再往外飞会让游戏崩溃）");
            }
        }

        // 把登记过的"跑到世界外的 NPC"拉回世界内。放在正常 Update 上下文里做，安全。
        internal static void RepairTraders()
        {
            if (BadTraders.Count == 0) return;

            float hw, hh;
            if (!WorldLimits(out hw, out hh)) { BadTraders.Clear(); return; }

            int fixedCount = 0;
            for (int i = 0; i < BadTraders.Count; i++)
            {
                TraderScript t = BadTraders[i];
                if (t == null) continue;
                try
                {
                    Vector3 p = t.transform.position;
                    Vector2 c = ClampPos(new Vector2(p.x, p.y), hw, hh);
                    t.transform.position = new Vector3(c.x, c.y, p.z);

                    // desiredPos 也要一起修，否则它每帧 Lerp 回去，又跑出界
                    object dp = GetF(t, "desiredPos");
                    if (dp is Vector2)
                    {
                        Vector2 d = (Vector2)dp;
                        SetF(t, "desiredPos", ClampPos(d, hw, hh));
                    }
                    fixedCount++;
                }
                catch { }
            }
            BadTraders.Clear();

            if (fixedCount > 0 && Time.unscaledTime >= NextTradeMsg)
            {
                NextTradeMsg = Time.unscaledTime + 10f;
                if (L != null) L.LogInfo("[GodMode] 崩溃防护：已把 " + fixedCount + " 个跑到世界外的交易商拉回世界内");
            }
        }

        internal static readonly Dictionary<string, FieldInfo> FCache = new Dictionary<string, FieldInfo>();

        internal static FieldInfo Fld(Type t, string name)
        {
            string k = t.FullName + "|" + name;
            FieldInfo f;
            if (FCache.TryGetValue(k, out f)) return f;
            try { f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); }
            catch { f = null; }
            FCache[k] = f;
            return f;
        }

        internal static object GetF(object o, string name)
        {
            if (o == null) return null;
            FieldInfo f = Fld(o.GetType(), name);
            if (f == null) return null;
            try { return f.GetValue(o); }
            catch { return null; }
        }

        internal static void SetF(object o, string name, object v)
        {
            if (o == null) return;
            FieldInfo f = Fld(o.GetType(), name);
            if (f == null) return;
            try { f.SetValue(o, v); }
            catch { }
        }

        // ==================================================================
        //  生成器（v0.8.0）：物品 + 生物/怪物/NPC + 植物 + 建筑 + 方块
        //
        //  三个来源，全部在运行时从游戏里读，游戏更新加了新东西会自动出现：
        //   ① 物品   —— 游戏自己的 Item.GlobalItems（SetupItems() 填的表），
        //               键就是 Utils.Create 需要的资源名，显示名/分类都是游戏本地化的。
        //   ② 实体   —— Resources.LoadAll<GameObject>("") 里带 Item 或 BuildingEntity
        //               组件的 prefab，id 就是 prefab 名。这正是游戏自己的控制台
        //               spawn 命令枚举自动补全的方式（ConsoleScript.RegisterSpawnEntities），
        //               所以凡游戏能 spawn 的东西这里都有。
        //   ③ 方块   —— 方块不是 prefab，是世界数组里的 UInt16 id。游戏没有暴露方块表，
        //               但 WorldGeneration.GetBlockInfo(ushort) 对合法 id 返回非 null 的
        //               BlockInfo（.name 已是本地化名），所以递增探测即可枚举。
        // ==================================================================

        // 往表里放一条（四个数组必须同步增长）
        internal static void AddSpawn(string id, string name, string cat, int kind, GameObject obj, int block)
        {
            SpawnIds.Add(id);
            SpawnNames.Add(name);
            SpawnCats.Add(cat);
            SpawnKinds.Add(kind);
            SpawnObjs.Add(obj);
            SpawnBlocks.Add(block);
        }

        internal static bool PlantWord(string low)
        {
            return low.Contains("tree") || low.Contains("mushroom") || low.Contains("plant")
                || low.Contains("fern") || low.Contains("vine") || low.Contains("grass")
                || low.Contains("bush") || low.Contains("flower") || low.Contains("moss")
                || low.Contains("fungus") || low.Contains("sapling") || low.Contains("root")
                || low.Contains("berry") || low.Contains("alga") || low.Contains("coral")
                || low.Contains("shroom") || low.Contains("trunk") || low.Contains("leaf")
                // v0.9.0 补：之前漏掉的关键词，导致一堆植物被塞进「建筑/设施」
                || low.Contains("rye") || low.Contains("reed") || low.Contains("rose")
                || low.Contains("cap") || low.Contains("wheat") || low.Contains("crop")
                || low.Contains("weed") || low.Contains("bloom") || low.Contains("petal")
                || low.Contains("sprout") || low.Contains("seed") || low.Contains("pod")
                || low.Contains("kelp") || low.Contains("lichen") || low.Contains("clover")
                || low.Contains("frig") || low.Contains("xalor") || low.Contains("cact");
        }

        // 给一个 prefab 分类。判断依据是它身上挂了哪些组件（按类型名，避免引用可能不存在的类型）。
        // v0.9.0 按实测清单重排了顺序、修掉了几处误判。
        internal static string ClassifyPrefab(GameObject go, BuildingEntity be, HashSet<string> comps)
        {
            try
            {
                // 1) 明确是怪/敌人的
                if (comps.Contains("CrystalEnemy")) return "生物/怪物";
                if (comps.Contains("TraderScript")) return "生物/商人";
                if (comps.Contains("SpiderHandler") || comps.Contains("SpiderHandlerTBE")
                    || comps.Contains("CaveTickSpawner") || comps.Contains("ElderThornbackBehaviour")) return "生物/怪物";

                // 2) 明确是植物的
                if (comps.Contains("GrabberPlant") || comps.Contains("BananaPlantSlip")) return "植物";

                // 3) 尸体单独一类（它们身上挂的是 CorpseScript，不是活物）
                if (comps.Contains("CorpseScript")) return "其它/尸体";

                // 4) 会动的活物（BuildingEntity.animal 是游戏自己的标志）
                bool animal = false;
                try { if (be != null) animal = be.animal; } catch { }
                if (animal) return "生物/动物";
                if (comps.Contains("Body")) return "生物/怪物";

                // 5) 本身就是掉落的物品
                if (comps.Contains("Item")) return "物品";

                // 6) 名字像植物的（关键词表已补全）
                string low = (go != null ? go.name : "").ToLowerInvariant();
                if (PlantWord(low)) return "植物";

                // 7) 水晶簇是装饰矿物，不是怪（crystalenemy 上面已经归到怪物了）
                if (comps.Contains("CrystalBehaviour") || low.Contains("crystal")) return "建筑/水晶";

                // 8) 剩下的建筑实体
                if (comps.Contains("BuildingEntity")) return "建筑/设施";
                return "其它";
            }
            catch { return "其它"; }
        }

        // 查游戏自己的本地化表：命中返回译文，未命中原样返回 key（所以要把 key 本身当失败）
        internal static string Loc(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            try { string s = Locale.GetBuilding(key); if (!string.IsNullOrEmpty(s) && s != key) return s; }
            catch { }
            try { string s = Locale.GetOther(key); if (!string.IsNullOrEmpty(s) && s != key) return s; }
            catch { }
            try { string s = Locale.GetItem(key); if (!string.IsNullOrEmpty(s) && s != key) return s; }
            catch { }
            return null;
        }

        // 实体的显示名。
        // ★ BuildingEntity.fullName 在 prefab 上是空字符串 —— 它只在实例化后的 Start() 里
        //   才被 Locale.GetBuilding(id) 填上，所以直接读 prefab 会拿到空白（v0.8.0 的 bug）。
        internal static string EntityDisplayName(GameObject go, BuildingEntity be)
        {
            string id = null;
            try { if (be != null && !string.IsNullOrEmpty(be.id)) id = be.id; } catch { }
            if (string.IsNullOrEmpty(id) && go != null) id = go.name;

            string s = Loc(id);
            if (s != null) return s;

            try { if (be != null && !string.IsNullOrEmpty(be.fullName)) return be.fullName; } catch { }

            try
            {
                Item it = (go != null) ? go.GetComponent<Item>() : null;
                if (it != null)
                {
                    ItemInfo info = it.Stats;
                    if (info != null && !string.IsNullOrEmpty(info.fullName)) return info.fullName;
                }
            }
            catch { }
            return id;
        }

        internal static string CollectComponents(GameObject go)
        {
            // 返回逗号分隔的组件类型名（同时用于写清单和分类）
            try
            {
                List<string> names = new List<string>();
                Component[] arr = go.GetComponents<Component>();
                if (arr != null)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i] == null) continue;
                        string n = arr[i].GetType().Name;
                        if (!names.Contains(n)) names.Add(n);
                    }
                }
                names.Sort();
                return string.Join(",", names.ToArray());
            }
            catch { return ""; }
        }

        internal static void BuildSpawnList(bool force)
        {
            try
            {
                Dictionary<string, ItemInfo> gi = Item.GlobalItems;

                GameObject[] res = null;
                try { res = Resources.LoadAll<GameObject>(""); }
                catch { res = null; }

                int sig = (gi != null ? gi.Count : 0) * 100000 + (res != null ? res.Length : 0);
                if (!force && sig == SpawnCacheCount && SpawnIds.Count > 0) return;
                SpawnCacheCount = sig;

                SpawnIds.Clear();
                SpawnNames.Clear();
                SpawnCats.Clear();
                SpawnKinds.Clear();
                SpawnObjs.Clear();
                SpawnBlocks.Clear();

                HashSet<string> seen = new HashSet<string>();
                HashSet<string> cats = new HashSet<string>();
                System.Text.StringBuilder dump = null;
                if (CSpawnDump != null && CSpawnDump.Value) dump = new System.Text.StringBuilder();

                // ---- ① 物品 ----
                int nItem = 0;
                if (gi != null)
                {
                    foreach (KeyValuePair<string, ItemInfo> kv in gi)
                    {
                        ItemInfo info = kv.Value;
                        string nm = (info != null && !string.IsNullOrEmpty(info.fullName)) ? info.fullName : kv.Key;
                        if (string.IsNullOrEmpty(nm) || nm == kv.Key)
                        {
                            string ln = Loc(kv.Key);
                            if (ln != null) nm = ln;
                        }
                        string cat = (info != null && !string.IsNullOrEmpty(info.category)) ? info.category : "其它";
                        // category 是游戏内部的英文键（medical/food/tool…），显示前换成中文
                        if (cat != "其它")
                        {
                            string lc = Loc(cat);
                            if (lc != null) cat = lc;
                        }
                        AddSpawn(kv.Key, nm, "物品/" + cat, K_ITEM, null, 0);
                        seen.Add(kv.Key);
                        cats.Add("物品/" + cat);
                        nItem++;
                    }
                }

                // ---- ② 实体：生物 / 怪物 / NPC / 植物 / 建筑 ----
                int nEnt = 0;
                if (res != null)
                {
                    for (int i = 0; i < res.Length; i++)
                    {
                        GameObject go = res[i];
                        if (go == null) continue;
                        string gn = go.name;
                        if (string.IsNullOrEmpty(gn) || seen.Contains(gn)) continue;

                        Item it = null;
                        BuildingEntity be = null;
                        try { it = go.GetComponent<Item>(); } catch { it = null; }
                        try { be = go.GetComponent<BuildingEntity>(); } catch { be = null; }
                        if (it == null && be == null) continue;

                        seen.Add(gn);
                        string comps = CollectComponents(go);
                        string cat = ClassifyPrefab(go, be, SplitSet(comps));

                        string nm = EntityDisplayName(go, be);
                        AddSpawn(gn, nm, cat, K_ENTITY, go, 0);
                        cats.Add(cat);
                        nEnt++;
                        if (dump != null) dump.Append(cat).Append(" | ").Append(gn).Append(" | ").Append(comps).Append("\r\n");
                    }
                }

                // ---- ③ 方块 ----
                int nBlk = 0;
                try
                {
                    WorldGeneration w = WorldGeneration.world;
                    if (w != null)
                    {
                        int miss = 0;
                        for (int id = 0; id < 4096 && miss < 256; id++)
                        {
                            BlockInfo bi = null;
                            try { bi = w.GetBlockInfo((ushort)id); }
                            catch { bi = null; }
                            if (bi == null) { miss++; continue; }
                            miss = 0;
                            string bn = string.IsNullOrEmpty(bi.name) ? ("未知方块" + id) : bi.name;
                            AddSpawn("block" + id, bn, "方块", K_BLOCK, null, id);
                            nBlk++;
                        }
                        cats.Add("方块");
                    }
                }
                catch { }

                // ---- 排序 + 重建分类/分组表 ----
                for (int i = 0; i < SpawnNames.Count - 1; i++)
                {
                    for (int j = 0; j < SpawnNames.Count - 1 - i; j++)
                    {
                        if (string.Compare(SpawnNames[j], SpawnNames[j + 1], StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            string t;
                            t = SpawnNames[j]; SpawnNames[j] = SpawnNames[j + 1]; SpawnNames[j + 1] = t;
                            t = SpawnIds[j]; SpawnIds[j] = SpawnIds[j + 1]; SpawnIds[j + 1] = t;
                            t = SpawnCats[j]; SpawnCats[j] = SpawnCats[j + 1]; SpawnCats[j + 1] = t;
                            int k;
                            k = SpawnKinds[j]; SpawnKinds[j] = SpawnKinds[j + 1]; SpawnKinds[j + 1] = k;
                            k = SpawnBlocks[j]; SpawnBlocks[j] = SpawnBlocks[j + 1]; SpawnBlocks[j + 1] = k;
                            GameObject g;
                            g = SpawnObjs[j]; SpawnObjs[j] = SpawnObjs[j + 1]; SpawnObjs[j + 1] = g;
                        }
                    }
                }

                List<string> cl = new List<string>(cats);
                cl.Sort();
                SpawnCats_Sorted.Clear();
                SpawnCats_Sorted.Add("全部");
                for (int i = 0; i < cl.Count; i++) SpawnCats_Sorted.Add(cl[i]);

                SpawnGroups.Clear();
                for (int i = 0; i < GroupNames.Length; i++) SpawnGroups.Add(GroupNames[i]);

                // 子分类/分组现在可能失效了，拉回"全部"
                if (!SpawnCats_Sorted.Contains(SpawnCat)) SpawnCat = "全部";
                if (!SpawnGroups.Contains(SpawnGroup)) SpawnGroup = "全部";

                if (dump != null)
                {
                    try
                    {
                        File.WriteAllText(InventoryDumpPath(),
                            "分类 | prefab名 | 身上的组件\r\n" + dump.ToString(), System.Text.Encoding.UTF8);
                    }
                    catch { }
                }

                if (L != null)
                    L.LogInfo("[GodMode][生成器] 已载入：物品 " + nItem + " + 实体 " + nEnt + " + 方块 " + nBlk
                        + " = " + SpawnIds.Count + " 种，分类 " + SpawnCats_Sorted.Count + " 个（"
                        + string.Join("/", SpawnCats_Sorted.ToArray()) + "）");
            }
            catch (Exception e)
            {
                if (L != null) L.LogWarning("[GodMode][生成器] 读取清单失败: " + e.Message);
            }
        }

        internal static HashSet<string> SplitSet(string csv)
        {
            HashSet<string> s = new HashSet<string>();
            if (string.IsNullOrEmpty(csv)) return s;
            string[] parts = csv.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string p = parts[i].Trim();
                if (p.Length > 0) s.Add(p);
            }
            return s;
        }

        internal static string InventoryDumpPath()
        {
            try
            {
                // 注意：Paths.ConfigPath 末尾带反斜杠，GetDirectoryName 会返回上一级目录
                //（v0.8.0 的文件就是因此掉到了 BepInEx 根目录），所以直接用 Combine。
                if (Paths.ConfigPath != null)
                    return System.IO.Path.Combine(Paths.ConfigPath, "GodMode.生成物清单.log");
            }
            catch { }
            return "GodMode.生成物清单.log";
        }

        // 把世界坐标夹回世界内：生成到世界外面会让游戏的区块/流体代码越界出问题
        internal static Vector2 ClampToWorld(Vector2 p)
        {
            try
            {
                float hw, hh;
                if (WorldLimits(out hw, out hh))
                {
                    float lx = Mathf.Max(4f, hw - 6f);
                    float ly = Mathf.Max(4f, hh - 6f);
                    p.x = Mathf.Clamp(p.x, -lx, lx);
                    p.y = Mathf.Clamp(p.y, -ly, ly);
                }
            }
            catch { }
            return p;
        }

        internal static string PosModeName()
        {
            int m = (CSpawnPosMode != null) ? CSpawnPosMode.Value : 2;
            if (m == 0) return "角色脚下";
            if (m == 1) return "鼠标处";
            return "自己选的点";
        }

        internal static string PosInfo()
        {
            int m = (CSpawnPosMode != null) ? CSpawnPosMode.Value : 2;
            if (m != 2 || !SpawnAnchorSet) return "";
            return "(" + Mathf.RoundToInt(SpawnAnchor.x) + ", " + Mathf.RoundToInt(SpawnAnchor.y) + ")";
        }

        internal static void SetAnchorFromPlayer()
        {
            Body p = PlayerBody();
            if (p != null)
            {
                SpawnAnchor = ClampToWorld((Vector2)p.transform.position);
                SpawnAnchorSet = true;
            }
        }

        // 把屏幕上的一点换算成世界坐标并设为锚点
        internal static void AimAtScreen(Vector2 screen)
        {
            try
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                Vector3 w = cam.ScreenToWorldPoint(screen);
                SpawnAnchor = ClampToWorld(new Vector2(w.x, w.y));
                SpawnAnchorSet = true;
            }
            catch { }
        }

        internal static Vector2 SpawnPos()
        {
            int m = (CSpawnPosMode != null) ? CSpawnPosMode.Value : 2;
            if (m == 2 && SpawnAnchorSet) return ClampToWorld(SpawnAnchor);
            if (m == 1)
            {
                try
                {
                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
                        return ClampToWorld(new Vector2(w.x, w.y));
                    }
                }
                catch { }
            }
            Body p = PlayerBody();
            if (p != null) return ClampToWorld((Vector2)p.transform.position);
            return SpawnAnchorSet ? SpawnAnchor : Vector2.zero;
        }

        // 生成点标记：把世界坐标换算成屏幕坐标，画一个小黄点（选点模式下变红）
        internal static void UpdateCross()
        {
            if (UiCrossRt == null) return;
            try
            {
                int m = (CSpawnPosMode != null) ? CSpawnPosMode.Value : 2;
                bool show = SpawnerOn && SpawnAnchorSet && (UiPickPos || m == 2);
                Camera cam = Camera.main;
                Vector3 sp = Vector3.zero;
                if (show && cam != null)
                {
                    sp = cam.WorldToScreenPoint(new Vector3(SpawnAnchor.x, SpawnAnchor.y, 0f));
                    if (sp.z < 0f) show = false;   // 在相机背面
                }
                else show = false;

                if (UiCrossRt.gameObject.activeSelf != show) UiCrossRt.gameObject.SetActive(show);
                if (UiCrossTx != null && UiCrossTx.gameObject.activeSelf != show) UiCrossTx.gameObject.SetActive(show);
                if (!show) return;

                UiCrossRt.anchoredPosition = new Vector2(sp.x, sp.y);
                if (UiCrossBg != null)
                    UiCrossBg.color = UiPickPos ? new Color(1f, 0.30f, 0.22f, 0.80f) : new Color(0f, 0f, 0f, 0.65f);
                if (UiCrossTx != null)
                {
                    UiCrossTx.rectTransform.anchoredPosition = new Vector2(sp.x + 18f, sp.y + 14f);
                    UiCrossTx.text = (UiPickPos ? "点这里生成  " : "生成点  ")
                        + Mathf.RoundToInt(SpawnAnchor.x) + ", " + Mathf.RoundToInt(SpawnAnchor.y);
                }
            }
            catch { }
        }

        // 生成一个东西（按表索引）。返回 true 表示成功。
        internal static bool SpawnOne(int i, Vector2 pos)
        {
            if (i < 0 || i >= SpawnIds.Count) return false;
            try
            {
                if (WorldGeneration.world == null) return false;
            }
            catch { return false; }

            // 方块：写进世界数组
            if (SpawnKinds[i] == K_BLOCK) return SpawnBlockAt(SpawnBlocks[i], pos);

            string id = SpawnIds[i];
            try
            {
                // 优先走游戏自己的入口（和控制台 spawn 命令完全一致）。
                // Resources 里找不到时，退回直接 Instantiate 我们手上的 prefab 引用
                // —— 这样连不在 Resources 根目录、但被游戏某个字段引用着的 prefab 也能生成。
                GameObject go = null;
                try { go = Utils.Create(id, pos, 0f); }
                catch { go = null; }
                if (go == null && i < SpawnObjs.Count && SpawnObjs[i] != null)
                {
                    try
                    {
                        go = UnityEngine.Object.Instantiate(
                            SpawnObjs[i], (Vector3)pos, Quaternion.Euler(0f, 0f, 0f)) as GameObject;
                    }
                    catch { go = null; }
                }
                if (go == null) return false;

                Item it = null;
                try { it = go.GetComponent<Item>(); } catch { it = null; }
                if (it != null)
                {
                    float cond = (CSpawnCond != null) ? CSpawnCond.Value : 1f;
                    if (cond < 1f) it.SetCondition(cond);

                    Body p = PlayerBody();
                    if (p != null && CSpawnPickup != null && CSpawnPickup.Value)
                    {
                        if (Vector2.Distance(pos, (Vector2)p.transform.position) < 2f) p.AutoPickUpItem(it);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                if (SpawnFail < 3 && L != null) L.LogWarning("[GodMode][生成器] 生成 " + id + " 失败: " + e.Message);
                return false;
            }
        }

        // 放一个方块。世界数组带边界钳制（SetBlock 内部 Mathf.Clamp），所以传界外坐标也安全。
        internal static bool SpawnBlockAt(int block, Vector2 pos)
        {
            try
            {
                WorldGeneration w = WorldGeneration.world;
                if (w == null) return false;
                Vector2Int bp = w.WorldToBlockPos(pos);
                w.SetBlock(bp, (ushort)block);
                return true;
            }
            catch (Exception e)
            {
                if (SpawnFail < 3 && L != null) L.LogWarning("[GodMode][生成器] 放方块 " + block + " 失败: " + e.Message);
                return false;
            }
        }

        // 把要生成的表索引排队，由 ProcessSpawnQueue 每帧生成几个（避免一口气生成太多卡顿）
        internal static void QueueSpawn(List<int> idxs, string label)
        {
            int max = (CSpawnMaxBatch != null) ? CSpawnMaxBatch.Value : 800;
            if (max < 1) max = 1;
            int n = 0;
            for (int k = 0; k < idxs.Count; k++)
            {
                if (n >= max) break;
                SpawnQueue.Enqueue(idxs[k]);
                n++;
            }
            SpawnTotal = n;
            SpawnDone = 0;
            SpawnFail = 0;
            SpawnGridX = 0;
            Notify(label + "：" + n + " 个（分批生成中）");
        }

        internal static void ProcessSpawnQueue()
        {
            if (SpawnQueue.Count == 0) return;
            int per = (CSpawnPerFrame != null) ? CSpawnPerFrame.Value : 15;
            if (per < 1) per = 1;
            if (per > 200) per = 200;

            Vector2 basePos = ClampToWorld(SpawnPos());
            // 记一行日志：万一批量生成之后又出问题，至少知道东西被生成到了哪里
            if (SpawnDone == 0 && SpawnFail == 0 && L != null)
                L.LogInfo("[GodMode][生成器] 开始批量生成 " + SpawnTotal + " 个，基准点 "
                    + Mathf.RoundToInt(basePos.x) + "," + Mathf.RoundToInt(basePos.y)
                    + "（位置模式=" + PosModeName() + "）");
            int spawned = 0;
            while (SpawnQueue.Count > 0 && spawned < per)
            {
                int idx = SpawnQueue.Dequeue();
                int col = SpawnGridX % 12;
                int row = SpawnGridX / 12;
                SpawnGridX++;
                Vector2 off = new Vector2((col - 5.5f) * 1.6f, row * 1.6f);
                bool ok = SpawnOne(idx, ClampToWorld(basePos + off));
                if (ok) SpawnDone++; else SpawnFail++;
                spawned++;
            }

            if (SpawnQueue.Count == 0)
                Notify("生成完毕：成功 " + SpawnDone + " 个" + (SpawnFail > 0 ? "，失败 " + SpawnFail + " 个" : ""));
        }

        internal static void SpawnAllFiltered()
        {
            BuildSpawnList(false);
            List<int> list = new List<int>();
            for (int i = 0; i < SpawnIds.Count; i++)
            {
                if (MatchFilter(i)) list.Add(i);
            }
            if (list.Count == 0) { Notify("没有可生成的东西"); return; }
            QueueSpawn(list, "生成全部（" + SpawnGroup + "/" + SpawnCat
                + (SpawnFilter.Length > 0 ? " / 搜索:" + SpawnFilter : "") + "）");
        }

        internal static void SpawnRandom(int n)
        {
            BuildSpawnList(false);
            List<int> pool = new List<int>();
            for (int i = 0; i < SpawnIds.Count; i++) if (MatchFilter(i)) pool.Add(i);
            if (pool.Count == 0) { Notify("没有可生成的东西"); return; }
            List<int> picked = new List<int>();
            for (int k = 0; k < n; k++) picked.Add(pool[UnityEngine.Random.Range(0, pool.Count)]);
            QueueSpawn(picked, "随机生成");
        }

        internal static bool MatchFilter(int i)
        {
            if (i < 0 || i >= SpawnIds.Count) return false;
            if (SpawnGroup != "全部")
            {
                string c = SpawnCats[i];
                if (c != SpawnGroup && !c.StartsWith(SpawnGroup + "/")) return false;
            }
            if (SpawnCat != "全部" && SpawnCats[i] != SpawnCat) return false;
            if (SpawnFilter.Length == 0) return true;
            string f = SpawnFilter.ToLowerInvariant();
            return SpawnIds[i].ToLowerInvariant().Contains(f)
                || SpawnNames[i].ToLowerInvariant().Contains(f)
                || SpawnCats[i].ToLowerInvariant().Contains(f);
        }

        internal static readonly List<string> SpawnCats_Sorted = new List<string>();

        // ------------------------------------------------------------------
        //  看门狗：后台线程每 1 秒检查一次主线程是否还在推进。
        //  游戏引擎的 bug 可能让主线程陷入死循环(最典型的是技能 ExperienceForLevel
        //  浮点溢出导致 CheckForLevelUp 的 while 永远成立)。主线程一旦卡住，
        //  游戏里任何代码都不会再执行 —— 只有在别的线程上才可能自救。
        //  这里做两件事：把已知的 Skills 数值强行拉回安全区(纯托管字段写入，线程安全)，
        //  以及写一份诊断文件，方便事后定位。
        // ------------------------------------------------------------------
        internal static void Remember(Skills s)
        {
            if (s == null) return;
            for (int i = 0; i < KnownSkills.Count; i++) { if (KnownSkills[i] == s) return; }
            if (KnownSkills.Count < 64) KnownSkills.Add(s);
        }

        internal static void StartWatchdog()
        {
            if (WatchdogStarted) return;
            WatchdogStarted = true;
            try
            {
                Thread t = new Thread(WatchLoop);
                t.IsBackground = true;
                t.Name = "GodModeWatchdog";
                t.Start();
                if (L != null) L.LogInfo("[GodMode] 看门狗已启动");
            }
            catch (Exception e)
            {
                if (L != null) L.LogWarning("[GodMode] 看门狗启动失败: " + e.Message);
            }
        }

        static void WatchLoop()
        {
            int last = -1;
            int stall = 0;
            while (true)
            {
                try { Thread.Sleep(1000); } catch { return; }
                try
                {
                    int now = Beat;
                    if (now != last) { last = now; stall = 0; continue; }
                    stall++;
                    if (stall < 8) continue;

                    WatchdogFires++;
                    int fixedCount = EmergencySkillReset();
                    WriteStallReport(stall, fixedCount);
                }
                catch { }
            }
        }

        // 从后台线程把技能数值拉回安全区。只写托管字段，不碰任何 Unity API。
        internal static int EmergencySkillReset()
        {
            int n = 0;
            for (int i = 0; i < KnownSkills.Count; i++)
            {
                Skills s = KnownSkills[i];
                if (s == null) continue;
                try
                {
                    if (s.STR > SAFE_SKILL_LEVEL) { s.STR = SAFE_SKILL_LEVEL; n++; }
                    if (s.RES > SAFE_SKILL_LEVEL) { s.RES = SAFE_SKILL_LEVEL; n++; }
                    if (s.INT > SAFE_SKILL_LEVEL) { s.INT = SAFE_SKILL_LEVEL; n++; }
                    if (s.STR < 0) { s.STR = 0; n++; }
                    if (s.RES < 0) { s.RES = 0; n++; }
                    if (s.INT < 0) { s.INT = 0; n++; }
                    if (s.expSTR > SAFE_XP) { s.expSTR = 0f; n++; }
                    if (s.expRES > SAFE_XP) { s.expRES = 0f; n++; }
                    if (s.expINT > SAFE_XP) { s.expINT = 0f; n++; }
                    if (s.maxSTR <= 0) { s.maxSTR = 100; n++; }
                    if (s.maxRES <= 0) { s.maxRES = 100; n++; }
                    if (s.maxINT <= 0) { s.maxINT = 100; n++; }
                }
                catch { }
            }
            return n;
        }

        internal static string StallLogPath()
        {
            try
            {
                // 同上：ConfigPath 末尾带反斜杠，不能用 GetDirectoryName
                if (Paths.ConfigPath != null)
                    return System.IO.Path.Combine(Paths.ConfigPath, "GodMode.卡死诊断.log");
            }
            catch { }
            return "GodMode.卡死诊断.log";
        }

        static void WriteStallReport(int stall, int fixedCount)
        {
            try
            {
                string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + "  主线程已停滞约 " + stall + " 秒"
                    + "  最后帧号=" + TickCount
                    + "  (游戏时间 " + StallAtFrame.ToString("0.0") + "s)"
                    + "  已知技能对象=" + KnownSkills.Count
                    + "  本次修正字段=" + fixedCount
                    + "  累计触发=" + WatchdogFires
                    + "  上帝=" + GodOn + " 飞行=" + FlyOn + "\r\n";
                File.AppendAllText(StallLogPath(), line, System.Text.Encoding.UTF8);
            }
            catch { }
        }
    }

    // ======================================================================
    //  Harmony 挂钩
    // ======================================================================
    public static class Patches
    {
        public static void Install()
        {
            MethodInfo preBody = typeof(Patches).GetMethod("Pre_Body");
            MethodInfo postBody = typeof(Patches).GetMethod("Post_Body");
            MethodInfo preCirc = typeof(Patches).GetMethod("Pre_Circ");
            MethodInfo preImpact = typeof(Patches).GetMethod("Pre_Impact");
            MethodInfo preLast = typeof(Patches).GetMethod("Pre_LastStand");
            MethodInfo postAlive = typeof(Patches).GetMethod("Post_Alive");
            MethodInfo postCam = typeof(Patches).GetMethod("Post_CamUpdate");
            MethodInfo postCon = typeof(Patches).GetMethod("Post_ConsoleUpdate");
            MethodInfo preTrader = typeof(Patches).GetMethod("Pre_TraderRender");
            MethodInfo preXp = typeof(Patches).GetMethod("Pre_XpForLevel");
            MethodInfo postXp = typeof(Patches).GetMethod("Post_XpForLevel");
            MethodInfo postBound = typeof(Patches).GetMethod("Post_UpdateExpBoundaries");
            MethodInfo preLevel = typeof(Patches).GetMethod("Pre_CheckForLevelUp");
            MethodInfo preAdd = typeof(Patches).GetMethod("Pre_AddExp");

            Hook("Body.HandleBody(每帧身体模拟)", typeof(Body), "HandleBody", preBody, postBody);
            Hook("Body.HandleCirculation(血液循环/脑死亡)", typeof(Body), "HandleCirculation", preCirc, null);
            Hook("Limb.ImpactDamage(受击入口)", typeof(Limb), "ImpactDamage", preImpact, null);
            Hook("Body.TryLastStand(濒死挣扎)", typeof(Body), "TryLastStand", preLast, null);
            Hook("Body.get_alive(生死判定)", typeof(Body), "get_alive", null, postAlive);

            // 每帧驱动：跟 QoL 模组一样，挂在游戏自己的 Update 上，最可靠
            Hook("PlayerCamera.Update(每帧驱动)", typeof(PlayerCamera), "Update", null, postCam);
            Hook("ConsoleScript.Update(备用驱动)", typeof(ConsoleScript), "Update", null, postCon);

            // 崩溃防护：TraderScript.OnWillRenderObject 会直接写 fluid[x,y] 而不做边界检查，
            // 一旦交易商跑到世界外，渲染剔除阶段每帧抛 IndexOutOfRangeException，主线程会被拖死。
            Hook("TraderScript.OnWillRenderObject(越界防护)", typeof(TraderScript), "OnWillRenderObject", preTrader, null);

            // 技能等级防护：游戏 42 级经验值溢出 int，RoundToInt 返回负数，
            // CheckForLevelUp 的循环条件就永远成立 —— 主线程死循环卡死。
            Hook("Skills.GetExperienceForLevel(经验溢出防护)", typeof(Skills), "GetExperienceForLevel", preXp, postXp);
            Hook("Skills.UpdateExpBoundaries(上限修正)", typeof(Skills), "UpdateExpBoundaries", null, postBound);
            Hook("Skills.CheckForLevelUp(等级钳制)", typeof(Skills), "CheckForLevelUp", preLevel, null);
            Hook("Skills.AddExp(经验值钳制)", typeof(Skills), "AddExp", preAdd, null);
        }

        static void Hook(string label, Type t, string name, MethodInfo pre, MethodInfo post)
        {
            try
            {
                MethodInfo target = AccessTools.Method(t, name);
                if (target == null)
                {
                    GodModePlugin.L.LogWarning("[GodMode] 找不到方法，跳过: " + label);
                    return;
                }
                HarmonyMethod hmPre = pre == null ? null : new HarmonyMethod(pre);
                HarmonyMethod hmPost = post == null ? null : new HarmonyMethod(post);
                GodModePlugin.H.Patch(target, hmPre, hmPost);
                GodModePlugin.L.LogInfo("[GodMode] 已挂钩: " + label);
            }
            catch (Exception e)
            {
                GodModePlugin.L.LogError("[GodMode] 挂钩失败 " + label + ": " + e.Message);
            }
        }

        // ---- 崩溃防护：交易商越界拦截 ----
        // 游戏在这里的原始代码是：
        //   Vector2Int v = WorldGeneration.world.WorldToBlockPos(transform.position + Vector3.up);
        //   FluidManager.main.fluid[v.x, v.y] = 0;      // 没有 Math.Clamp，越界即抛异常
        // 这是在渲染剔除(Internal_Cull)阶段被调用的，异常会把整条渲染管线搞垮。
        // 处理办法：越界的这次直接跳过（不执行就不会抛异常），登记下来，
        // 回到正常的 Update 上下文里再把它拉回世界内 —— 那里改 transform 是安全的。
        public static bool Pre_TraderRender(TraderScript __instance)
        {
            try
            {
                if (__instance == null) return true;
                if (GodModePlugin.CProtect == null || !GodModePlugin.CProtect.Value) return true;

                WorldGeneration w = WorldGeneration.world;
                if (w == null) return true;

                Vector3 p = __instance.transform.position + Vector3.up;
                Vector2Int b = w.WorldToBlockPos(new Vector2(p.x, p.y));
                int wd = (int)w.width;
                int ht = (int)w.height;
                if (wd <= 0 || ht <= 0) return true;

                // 在界内 → 一切照旧
                if (b.x >= 0 && b.x < wd && b.y >= 0 && b.y < ht) return true;

                GodModePlugin.TraderSkipCount++;
                if (GodModePlugin.BadTraders.Count < 64 && !GodModePlugin.BadTraders.Contains(__instance))
                    GodModePlugin.BadTraders.Add(__instance);
                return false;
            }
            catch { return true; }
        }

        // ---- 技能溢出防护 ----
        // 游戏原始实现（IL 逐步还原）：
        //   Int32 GetExperienceForLevel(Int32 level) {
        //       if (level <= 0) return 0;
        //       float total = 0, step = 60;
        //       for (int i = 1; i <= level; i++) {
        //           if (i < 10) total += 60;
        //           else if (i == 10) { step = 100; total += step; }
        //           else { step *= 1.18f; if (i > 20) { step *= 1.15f; if (i >= 30) step *= 2f; } total += step; }
        //       }
        //       return Mathf.RoundToInt(total);
        //   }
        // 42 级 total 就超过 int.MaxValue，109 级变 Infinity，RoundToInt 之后是负数。
        internal static bool Pre_XpForLevel(int targetLevel, ref int __result)
        {
            if (GodModePlugin.CSkillGuard == null || !GodModePlugin.CSkillGuard.Value) return true;
            if (targetLevel <= 120) return true;
            // 再往上算必然是 Infinity，没必要真的循环那么多次（等级被改坏时这里能救命）
            __result = int.MaxValue;
            GodModePlugin.SkillFixCount++;
            return false;
        }

        internal static void Post_XpForLevel(int targetLevel, ref int __result)
        {
            if (GodModePlugin.CSkillGuard == null || !GodModePlugin.CSkillGuard.Value) return;
            if (targetLevel > 0 && __result <= 0) { __result = int.MaxValue; GodModePlugin.SkillFixCount++; }
        }

        internal static void Post_UpdateExpBoundaries(Skills __instance)
        {
            if (GodModePlugin.CSkillGuard == null || !GodModePlugin.CSkillGuard.Value) return;
            if (__instance == null) return;
            if (__instance.maxSTR <= 0) { __instance.maxSTR = int.MaxValue; GodModePlugin.SkillFixCount++; }
            if (__instance.maxRES <= 0) { __instance.maxRES = int.MaxValue; GodModePlugin.SkillFixCount++; }
            if (__instance.maxINT <= 0) { __instance.maxINT = int.MaxValue; GodModePlugin.SkillFixCount++; }
        }

        // level 是 Skills.STR 本身（按引用传入），钳制它就等于把等级拉回安全值。
        // 注意：xp 是按值传入的(Single)，Harmony 里不能用 ref 改它，所以经验值的钳制放在 Pre_AddExp 里。
        public static void Pre_CheckForLevelUp(ref int level)
        {
            if (GodModePlugin.CSkillGuard == null || !GodModePlugin.CSkillGuard.Value) return;
            if (level < 0) { level = 0; GodModePlugin.SkillFixCount++; }
            if (level > GodModePlugin.SAFE_SKILL_LEVEL) { level = GodModePlugin.SAFE_SKILL_LEVEL; GodModePlugin.SkillFixCount++; }
        }

        // AddExp 会在内部调用 CheckForLevelUp，所以要在它执行之前就把经验值压回安全范围
        public static void Pre_AddExp(Skills __instance)
        {
            if (GodModePlugin.CSkillGuard == null || !GodModePlugin.CSkillGuard.Value) return;
            if (__instance == null) return;
            if (__instance.expSTR > GodModePlugin.SAFE_XP) { __instance.expSTR = GodModePlugin.SAFE_XP; GodModePlugin.SkillFixCount++; }
            if (__instance.expRES > GodModePlugin.SAFE_XP) { __instance.expRES = GodModePlugin.SAFE_XP; GodModePlugin.SkillFixCount++; }
            if (__instance.expINT > GodModePlugin.SAFE_XP) { __instance.expINT = GodModePlugin.SAFE_XP; GodModePlugin.SkillFixCount++; }
        }


        // ------------------------------------------------------------------
        //  每帧驱动 ----
        public static void Post_CamUpdate()
        {
            GodModePlugin.DrvCamera++;
            GodModePlugin.Tick();        }

        public static void Post_ConsoleUpdate()
        {
            GodModePlugin.DrvConsole++;
            GodModePlugin.Tick();
        }

        // ---- 身体状态 ----
        public static void Pre_Body(Body __instance)
        {
            if (!GodModePlugin.GodOn) return;
            if (!GodModePlugin.IsPlayer(__instance)) return;
            GodModePlugin.RestoreBody(__instance);
        }

        public static void Post_Body(Body __instance)
        {
            if (!GodModePlugin.GodOn) return;
            if (!GodModePlugin.IsPlayer(__instance)) return;
            GodModePlugin.RestoreBody(__instance);
        }

        public static void Pre_Circ(Body __instance)
        {
            if (!GodModePlugin.GodOn) return;
            if (!GodModePlugin.IsPlayer(__instance)) return;
            GodModePlugin.RestoreBody(__instance);
        }

        public static bool Pre_Impact(Limb __instance)
        {
            if (!GodModePlugin.GodOn) return true;
            if (__instance == null) return true;
            Body owner = __instance.body;
            if (owner == null) return true;
            if (GodModePlugin.IsPlayer(owner)) return false;
            return true;
        }

        public static bool Pre_LastStand(Body __instance)
        {
            if (GodModePlugin.GodOn && GodModePlugin.IsPlayer(__instance)) return false;
            return true;
        }

        public static void Post_Alive(Body __instance, ref bool __result)
        {
            if (!GodModePlugin.GodOn) return;
            if (!GodModePlugin.IsPlayer(__instance)) return;
            __result = true;
        }
    }

    // ======================================================================
    //  自建 HUD 对象
    //  为什么需要它：实测本游戏的 BepInEx 插件管理器对象上，「插件自身」的
    //  Update / OnGUI 从不被 Unity 调用（心跳日志里 插件Update 恒为 0），
    //  于是每帧逻辑和 IMGUI 窗口全都不会跑。这里自己 new 一个
    //  DontDestroyOnLoad 的 GameObject 挂上本组件，Unity 会正常调用它的
    //  Update 与 OnGUI，物品生成器窗口才有地方画出来。
    // ======================================================================
    public class GodModeHud : MonoBehaviour
    {
        void Update()
        {
            GodModePlugin.DrvHud++;
            GodModePlugin.Tick();
        }

        void OnGUI()
        {
            GodModePlugin.DrawGui();
        }
    }
}
