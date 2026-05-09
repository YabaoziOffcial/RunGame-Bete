# YLibrary

`YLibrary` 是当前项目的一套 Unity 基础框架，主要用于统一游戏启动、资源加载、UI 管理、音频管理、配置读写以及常用运行时工具。

它的目标不是做成很重的“大框架”，而是给项目提供一套开箱即用、便于扩展、适合快速迭代的基础层。

## 命名空间

框架当前主要使用以下命名空间：

- `YBZ`
- `YBZ.Design`
- `YBZ.Core`

通常可以理解为：

- `YBZ.Design`：偏基础设计模式、单例、通用基类
- `YBZ.Core`：偏游戏运行主流程和核心管理模块

## 适合做什么

这套框架适合中小型 Unity 项目快速搭建基础能力，尤其适合以下场景：

- 需要一个统一的游戏启动入口
- 需要统一管理 UI 的打开、关闭和缓存
- 需要统一播放 BGM / 音效并持久化音量配置
- 需要统一的资源加载入口，兼容 `Resources` 与可选的 `YooAsset`
- 需要一些高频使用的工具类来减少重复代码

## 目录概览

`Assets/YLibrary` 目前可以大致分成下面几部分：

```text
YLibrary
|-- GameRoot.cs
|-- SDKManager.cs
|-- Core
|   |-- GameHelper.cs
|   |-- MVC
|   `-- Module
|       |-- Aduio
|       |-- GameData
|       |-- ResourceManager.cs
|       `-- UI
|-- Utility
|   |-- TransformExtension.cs
|   |-- GameConfig.cs
|   |-- ObjectPool.cs
|   `-- ...
|-- Editor
|-- Packages
`-- Third
```

## 快速上手

### 1. 启动入口

通常情况下，`GameRoot` 是游戏启动入口。

当前启动流程大致如下：

1. 在 `GameRoot.Initialize()` 中初始化核心系统
2. 初始化游戏辅助、数据、控制器、网络、配置
3. 在 `Start()` 中打开首个 UI
4. 在 `Update()` / `FixedUpdate()` 中驱动运行时逻辑

你现在的主流程是：

- `GameHelper.Instance.Init()`
- `GameDataManager.Instance.Init()`
- `GameController.Instance.Init()`
- `NetworkManager.Instance.Init()`
- `GameConfig.Init()`
- `UIManager.Instance.OpenUI<StartPanel>()`

所以如果要接入新项目，通常第一步就是保证场景里有 `GameRoot`，并让它成为整套系统的启动点。

### 2. 资源加载

统一使用 `ResourceManager` 进行资源加载。

常见用法：

```csharp
var prefab = ResourceManager.Instance.LoadRes<GameObject>("Prefab/UI/StartPanel");
var clip = ResourceManager.Instance.LoadRes<AudioClip>("Audio/Click");
```

当前特点：

- 默认支持 `Resources.Load`
- 定义了缓存字典，避免重复加载
- 预留了 `YooAsset` 接入逻辑
- 提供同步、异步、场景加载、统一场景句柄能力

如果项目暂时不接远程资源，直接走 `Resources` 即可；如果后面要扩成热更资源体系，也可以继续沿用 `ResourceManager` 这个入口。

### 3. UI 管理

统一使用 `UIManager` 管理 UI。

当前约定：

- UI 预制体路径默认是 `Resources/Prefab/UI/`
- `OpenUI<T>()` 会按类型名加载同名预制体
- `Y_PanelBase` 作为主界面面板基类
- `Y_PopupBase` 作为弹窗基类
- `ViewBase` 作为 UI 抽象基类

示例：

```csharp
UIManager.Instance.OpenUI<StartPanel>();
UIManager.Instance.OpenUI<GamePanel>();
UIManager.Instance.CloseUI<GamePanel>();
```

当前 UI 框架有几个重要约定：

- 面板类 UI 同一时间通常只有一个当前面板
- UI 会被缓存，重复打开时优先复用
- Canvas 下建议保留 `Panel`、`PopUp`、`Other` 三个层级节点
- 面板显示/关闭过程可以在基类里统一接动画

如果你继续基于这套方式开发，建议所有业务 UI 都继承 `ViewBase` / `Y_PanelBase` / `Y_PopupBase`，不要绕开 `UIManager` 直接乱实例化。

### 4. 音频管理

统一使用 `AudioManager` 播放音效和音乐。

当前能力包括：

- BGM 与普通音效分类播放
- 基于 `SoundConfig` 的模板配置播放
- 音量持久化保存
- 主音量 / 音乐 / 音效三级控制
- 互斥音效与延迟播放支持

常见调用方式：

```csharp
AudioManager.Instance.PlaySoundTem("BGM:MainBGM");
AudioManager.Instance.PlaySoundTem("Other:Click");
AudioManager.Instance.SetVolumeLevels(1f, 0.8f, 0.8f);
```

如果项目里所有声音都统一从这里走，后面无论是加设置界面、静音逻辑，还是扩展到更多音频通道，都会比较容易维护。

### 5. 配置与运行时工具

框架里还有几类比较常用的基础能力：

- `GameConfig`：用于读取和保存简单配置
- `GameHelper`：提供计时器、计数器、延迟执行、协程辅助、布局刷新等能力
- `TransformExtensions`：对 `Transform` / `RectTransform` / `Image` / `Text` / `Slider` 等常见操作做了便捷封装
- `ObjectPool`：对象池能力
- `MainThreadDispatcher`：主线程分发

例如：

- 想快速做一个延时逻辑，可以优先看 `GameHelper`
- 想少写 `GetComponent` 和重复 UI 操作代码，可以优先看 `TransformExtensions`

## 核心模块说明

### GameRoot

`GameRoot` 是整个运行时的装配入口，负责把各个系统串起来。

建议把以下职责都收口在这里：

- 启动时初始化核心管理器
- 注册全局事件
- 驱动全局 Update / FixedUpdate
- 处理退出时的释放与反注册

一句话理解：`GameRoot` 决定“游戏怎么开始”和“全局系统怎么接起来”。

### ResourceManager

`ResourceManager` 是资源访问唯一入口。

建议所有业务层都通过它来拿资源，而不是直接散落使用 `Resources.Load`。这样后续切换资源方案时，业务代码不用大改。

### UIManager

`UIManager` 是 UI 生命周期和层级关系的中心。

它负责：

- 打开 UI
- 关闭 UI
- 复用 UI
- 区分面板与弹窗挂点
- 维护当前面板

一句话理解：业务层只管“我要开哪个 UI”，UIManager 负责“这个 UI 应该怎么被创建和摆放”。

### AudioManager

`AudioManager` 是音频统一入口。

它负责：

- 找到或创建对应类型的 `AudioSource`
- 根据 `SoundConfig` 播放模板音频
- 读写音量设置
- 区分音乐和音效总线

一句话理解：业务层只发播放指令，音频细节由 `AudioManager` 统一处理。

### SDKManager

`SDKManager` 负责封装平台和渠道相关能力，目前包括：

- SDK 初始化 / 释放
- 振动
- Banner / Native / 插屏 / 激励广告
- 事件埋点
- 关卡开始 / 结束统计
- 隐私协议跳转

建议所有平台差异都继续收敛在这里，不要在业务代码里到处写条件编译。

### GameHelper

`GameHelper` 更像一个运行时工具集合，适合放一些高频但又不属于某个业务模块的能力，例如：

- 计时器
- 计数器
- 协程辅助
- 截图
- 布局强制刷新
- 延迟执行

### TransformExtensions

`TransformExtensions` 提供了一套偏“快速开发风格”的扩展方法，适合简化以下代码：

- 坐标、缩放、旋转设置
- UI 尺寸和填充设置
- 文本赋值
- 图片切换
- Slider / Toggle 操作
- 常见组件获取后的快捷处理

这类工具的价值主要在于：减少样板代码，提高业务层书写速度。

## 推荐使用约定

为了让这套框架更稳定，建议继续遵守下面这些约定：

1. `GameRoot` 只做全局装配，不堆具体业务细节
2. 所有资源加载统一从 `ResourceManager` 进入
3. 所有 UI 打开关闭统一从 `UIManager` 进入
4. 所有声音播放统一从 `AudioManager` 进入
5. 平台与渠道能力统一收口到 `SDKManager`
6. 业务脚本优先调用框架层提供的公共能力，不重复造轮子

## 一个推荐的调用链

下面是比较符合当前框架风格的一条业务调用链：

```text
GameRoot
    -> 初始化核心系统
    -> 打开首屏 UI
    -> UI 交互触发业务逻辑
    -> 业务逻辑按需访问 ResourceManager / AudioManager / SDKManager / GameData
```

也就是说，框架层负责“基础设施”，业务层负责“玩法和表现”，两边尽量分开。

## 后续可以继续完善的方向

如果后面继续打磨这套框架，可以优先考虑这几个方向：

- 给 `README` 增加“新建一个 UI 面板”的完整示例
- 给 `ResourceManager` 增加资源命名和目录规范
- 给 `AudioManager` 补一份 `SoundConfig` 配置说明
- 给 `GameRoot` 补一张启动时序图
- 给 `Core/MVC` 补一份使用示例

## 总结

当前 `YLibrary` 已经具备一个 Unity 项目基础框架的核心形态：

- 有统一入口：`GameRoot`
- 有统一资源层：`ResourceManager`
- 有统一 UI 层：`UIManager`
- 有统一音频层：`AudioManager`
- 有平台能力封装：`SDKManager`
- 有一批实用工具类：`GameHelper`、`TransformExtensions` 等

如果后续继续沿着“统一入口、统一管理器、统一工具层”的方向维护，这套框架会比较适合持续沉淀成你自己的项目底座。