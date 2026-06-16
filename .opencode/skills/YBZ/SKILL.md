---
name: YBZ
description: Use when working on the Dragonfall Unity project — Bullet Heaven / Vampire Survivors-like 2D pixel roguelike. Covers YLibrary framework API, MVC architecture, EquipBase hierarchy, code conventions, ScriptableObject patterns, DOTween/timeScale rules, and project structure. Trigger when editing code under DragonFall/Assets/ or discussing Dragonfall features.
---

# Dragonfall 项目约定

## 项目概览

- Unity 2022.3 LTS, 2D URP, 像素风 Bullet Heaven
- 所有代码在 `DragonFall/Assets/Scripts/` 下

## 架构

```
MVC 分层:
  GameController (局流程) → GameModel (数据) → GameEvents → View (UI)

Equip 体系（统一武器和属性道具）:
  EquipBase (abstract)
  ├── Weapon_Sword / Weapon_Dart / Weapon_Magic    ← 弹幕武器
  ├── EquipItem (abstract, 无Update)                ← 纯属性装备基类
  │   ├── EquipItem_MaxHp / EquipItem_MoveSpeed / EquipItem_Greed / EquipItem_Strength
  └── 新增装备子类遵循此模式

所有装备通过 WeaponConfig SO 配置，统一在 EquipConfig.equips 列表中。
被动属性物品复用 WeaponConfig SO — className 指向 EquipItem 子类即可。
```

## YLibrary 框架 API（在 Assets/YLibrary/）

### 入口
```
GameRoot : O_MonoSingleton<GameRoot>    场景入口，Drive 全局 Update
  Initialize() → 初始化所有 Manager（序: GameHelper, GameDataManager, GameConfig, EquipManager, GameController）
  Start()     → GameController.GameStart()
  Update()    → GameHelper.Update → GameController.Update → EquipManager.Update
```

### Singleton 模式（YBZ.Design 命名空间）
```
Singleton<T>         纯 C# 单例，new() 约束     → GameDataManager, SDKManager
D_MonoSingleton<T>   自动创建 + DontDestroy      → ResourceManager, GameHelper
O_MonoSingleton<T>   场景放置 + DontDestroy       → GameRoot, UIManager
S_MonoSingleton<T>   场景放置，重复销毁警告
```

### UIManager : O_MonoSingleton
```
canvasTransform / canvasWorldTransform           画布引用
PanelTransform / PopUpTransform / OtherTransform 层次容器
UICachas : Dictionary<Type, ViewBase>            缓存池（复用）
CurrentPanel : ViewBase                          当前打开的面板（同一时间仅一个）

T OpenUI<T>()  where T : ViewBase    打开并返回。Panel → PanelTransform, Popup → PopUpTransform
                                        已在缓存则复用，否则从 Resources/Prefab/UI/{TypeName} 加载
                                        打开新 Panel 时自动关闭当前 Panel
T CloseUI<T>()                       关闭 UI 实例
```

### ViewBase 生命周期
```
Show()    → Load() → 子类动画
Close()   → UnLoad() → 子类动画
```

### Y_PopupBase : ViewBase（弹窗基类）
```
allUI : RectTransform         内容根节点
cgUI / cgBG : CanvasGroup     自身 / 背景遮罩
animator : Animator           可选

CloseBtn / FinishBtn / CancelBtn : Button
CloseCall / FinishCall / CancelCall : Action   按钮回调委托

OnCloseClick()   → Close() + CloseCall?.Invoke()
OnFinishClick()  → FinishCall?.Invoke()
OnCancelClick()  → CancelCall?.Invoke()

ShowAnima()  DOTween 弹入: scale 0→1.15→1.0, 0.35s, .SetUpdate(true)
CloseAnima() DOTween 弹出: scale 1→1.1→0, 0.3s, 背景 fade 0→0.3s, .SetUpdate(true)
```

### EventManager（static）
```
AddListener(object key, EventCallBack cb)     cb 签名: void(params object[])
RemoveListener(object key, EventCallBack cb)
SendEvent(object key, params object[] values)
Clear()
```

### ObjectPool（static）
```
GetObj(GameObject prefab, Transform parent, bool zeroPosRot)
PushObj(GameObject obj)
PushAllChildren(Transform parent, params GameObject[] exclude)
Clear(string key) / ClearAll()
```
Key 为 prefab.name；池根节点 `ObjectPool/{key}Pool`。

### ResourceManager : D_MonoSingleton
```
GameObject LoadRes(string path)
T LoadRes<T>(string path) where T : Object
Task<T> LoadResAsync<T>(string path)
void LoadScene(string path, Action done, LoadSceneMode mode)
```
路径: `Resources.Load("path")` — 如 `"Config/EquipConfig"` 对应 `Assets/Resources/Config/EquipConfig.asset`。

### GameHelper : D_MonoSingleton
```
WaitForSeconds WaitSecond(float t)            缓存复用
AddTimer(string key, float sec, Action done)  命名定时器
DelaySeconds(float delay, Action action)      DOTween 延迟执行
```

### DOTween 使用约定
```
所有 UI Tween 必须 .SetUpdate(true)          脱离 Time.timeScale
Sequence 链式: .Append(tween).AppendInterval(t).AppendCallback(action).Play()
常用 Ease: OutSine(关闭), Linear(进度条)
```

## 关键约定

### 时间控制
- `Time.timeScale = 0` 暂停游戏（SelectView/GameOverView 打开时）
- **所有 DOTween 动画必须 `.SetUpdate(true)`** 脱离 timeScale
- 游戏时间用 `Time.time`（跟随 timeScale），游戏内计时器同理

### ScriptableObject
- 配置放 `Assets/Resources/Config/` 下
- `EquipConfig.asset` — 装备总表
- `EnemySpawnConfig/LevelX.asset` — 关卡刷怪配置
- `WeaponConfig/WeaponXxxxxConfig.asset` — 各装备等级配置
- 编辑器生成工具在 `WeaponConfigGenerator.cs`，菜单: `DragonFall/装备/`

### 代码风格
- **不加注释**，除非绝对必要
- 字段: `m_` 前缀（私有）、`m_` 前缀 + `[SerializeField]`（序列化私有）
- 常量: `const` 大写或 `MAX_XXX` 模式
- 方法用 XML 文档注释说明意图

### 物理/碰撞
- 弹幕: Rigidbody2D Kinematic + Collider2D IsTrigger
- Physics2D 碰撞矩阵仅勾选 Bullet↔Enemy
- 敌人检测用 `Physics2D.OverlapCircleNonAlloc`（带预分配缓冲数组）

## 升级选卡流程

```
经验拾取 → GameModel.AddExp → LevelUp 事件
  → GameController.OnUiLevelUp → m_PendingLevelUpCount++ → OpenSelectView()
  → Time.timeScale = 0 → 弹窗显示 3 张随机卡
  → 点击 → EquipUnit.OnEquipUnitClick → EquipManager.AddEquip/UpgradeEquip
  → CompleteCurrentLevelUpSelection → CloseSelectView → Time.timeScale = 1
```

选卡池 = 已拥有可升级的 + EquipConfig 中未发现的，随机洗牌取 3。

## 当前状态
- 核心战斗循环可用（移动/攻击/敌人AI/经验/升级）
- 仅 3 个武器 + 4 个属性装备
- 单关卡场景 `Game.unity`
- 无主菜单、无角色选择、无 BGM/SFX
- CardManager 完全空壳，塔罗牌系统未实现

## 提交前检查清单

**每次完成代码编写后必须执行以下检查：**

### 编译错误检查
1. 搜索所有新增/修改的 `.cs` 文件中是否有：
   - 缺失的 `using` 语句（`System.Linq`、`UnityEditor.UIElements` 等）
   - `#if UNITY_EDITOR` / `#endif` 是否成对
   - Editor 代码（`UnityEditor` 命名空间）是否被非 Editor 目录的文件引用
   - `.ToList()`、`.First()` 等 LINQ 方法是否对应 `using System.Linq`
2. 检查新创建文件所在目录是否需要 `.asmdef`（当前项目不使用 asmdef，全部在默认程序集）
3. 新增的 `.meta` 孤儿文件（如移动/删除文件夹后留下的 `.meta`）

### 执行方式
- 使用 `grep` 在整个 `Assets/Scripts/` 下搜索 `#if UNITY_EDITOR`，用 `task(explore)` 逐文件阅读确认无编辑器引用泄漏
- 所有 Editor 目录外的 `.cs` 文件中不应该出现 `UnityEditor` 字样
- 所有 `#if UNITY_EDITOR` 必须有对应的 `#endif`

### 运行时检查（Play 模式）
- SelectView 面板打开 → 游戏暂停 → 关闭 → 游戏恢复
- 装备选择后 GamePanel HUD 图标更新
- 地图 Chunk 回收复用无闪烁/错位

