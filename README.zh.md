# Aurora Unity

![许可](https://img.shields.io/github/license/NOMOAX/aurora.unity)
![版本](https://img.shields.io/badge/version-2.0.5-blue)
![最低 Unity 版本](https://img.shields.io/badge/Unity-2021.2%2B-blue)

适用于 Unity 的高性能、低内存消耗的工具包。

[English](README.md) | 中文

## 依赖

- [Aurora](https://github.com/NOMOAX/aurora.git)
- Unity UI（`com.unity.ugui`）

## 安装

1. 打开 Unity package manager。
2. 点击左上角的 `+` 按钮，然后选择 `Add package from git URL...`。
3. 填入 `https://github.com/NOMOAX/aurora.unity.git` 并点击 `Add` 按钮。

## 运行环境与初始化

本包不需要任何手动初始化。程序开始运行时会自动完成一系列初始化，并把过程打印到 Unity 控制台，其中包括：

- 把 `Log.Logger` 切换为 `UnityConsoleLogger.Instance`，让日志写入 Unity 控制台；
- 记录 Unity 主线程 ID、`SynchronizationContext` 及其 `TaskScheduler`；
- 缓存 "Default"、"Ignore Raycast"、"UI" 三个层（layer）的值；
- 初始化 `PlayerLoopUtility`；
- 建立 `UnityEnvironment.InactiveContainer`；
- 在播放器（非编辑器）环境还会额外创建退出监听对象。

在编辑器环境下，进入播放模式/退出播放模式时还会额外处理 `PlayerLoopUtility`、界面容器与光标栈的清理。

### UnityEnvironment

`UnityEnvironment` 提供与 Unity 运行环境相关的值和操作。

```csharp
// 程序是否正在运行（编辑器环境下即是否处于播放模式）
if (UnityEnvironment.IsPlaying)
{
    // ...
}

// 当前线程是否是 Unity 主线程
if (!UnityEnvironment.OnUnityMainThread)
{
    // ...
}

// 主线程 ID、Unity 的同步上下文，以及它的任务调度器
var mainThreadId = UnityEnvironment.UnityMainThreadId;
var synchronizationContext = UnityEnvironment.UnitySynchronizationContext;
var taskScheduler = UnityEnvironment.UnitySynchronizationContextTaskScheduler;

// 程序结束时会被取消的取消令牌（编辑器环境下即退出播放模式时）
await Task.Delay(1000, UnityEnvironment.ExitToken);

// 是否处于编辑器的暗色皮肤环境
var isProSkin = UnityEnvironment.IsProSkin;

// 三个固定层（不可自定义）的值
var defaultLayer = UnityEnvironment.DefaultLayer;
var ignoreRaycastLayer = UnityEnvironment.IgnoreRaycastLayer;
var uiLayer = UnityEnvironment.UILayer;

// 剪贴板
var clipboard = UnityEnvironment.Clipboard;
UnityEnvironment.Clipboard = "hello";

// 屏幕尺寸与宽高比
var screenSize = UnityEnvironment.ScreenSize;
var aspectRatio = UnityEnvironment.ScreenAspectRatio;

// 退出程序（编辑器环境下即退出播放模式）
UnityEnvironment.QuitApplication();
UnityEnvironment.QuitApplication(0);

// 登记一个在程序结束时释放的实例；多个实例按后进先出的顺序释放
UnityEnvironment.DisposeOnApplicationQuit(disposable);
```

`InactiveContainer` 是一个在整个播放模式期间始终保持 inactive 的 `Transform`：把要实例化的对象挂在它下面，就不会在实例化过程中立刻执行 `MonoBehaviour.OnEnable`，从而可以放心地把初始化代码写在 `OnEnable` 里。不要在运行时把它设为 active。

```csharp
var instance = Instantiate(prefab, UnityEnvironment.InactiveContainer, false); // 此时 OnEnable 尚未执行
// TODO: 初始化 instance
instance.transform.SetParent(realParent, false);
instance.SetActive(true); // 到这里 OnEnable 才会执行
```

`ExitToken` 在程序开始时创建，在程序结束时被取消；编辑器环境下，如果当前不在播放模式，返回的是一个已经取消的令牌。注册在它上面的回调不应该抛出异常（抛出的异常会在程序结束时被记录为错误日志）。

### SingletonBehaviour\<T\>

`SingletonBehaviour<T>` 是单例 `MonoBehaviour` 的基类，`T` 是自身的类型。

```csharp
[DoNotDestroyOnLoad]
[WithHideFlags(HideFlags.HideAndDontSave)]
public sealed class GameManager : SingletonBehaviour<GameManager>
{
}
```

`Instance` 在实例 `Awake` 时被赋值。对于场景中 inactive 或 disabled 的实例，`Awake` 可能还没有执行，此时 `Instance` 还是 `null`，可以显式调用 `FindInstance` 去场景里查找（也会查找未激活的对象），或者调用 `CreateInstance` 创建一个新的 `GameObject`。

```csharp
GameManager.FindInstance(); // 在场景中查找并赋值；已经赋值时什么都不做，找不到时记录一条警告
GameManager.CreateInstance(); // 新建一个名为 "GameManager" 的 GameObject 并赋值

var gameManager = GameManager.Instance;
```

如果已经存在一个单例实例，而这个实例既不是由 `FindInstance` 找到的、也不是由 `CreateInstance` 创建的，那么再出现第二个实例会被视为程序错误并抛出 `InvalidOperationException`。

- `[DoNotDestroyOnLoad]`：实例被赋值时对它执行 `Object.DontDestroyOnLoad`。
- `[WithHideFlags(HideFlags)]`：实例被赋值时把指定的 `HideFlags` 与 `hideFlags` 做按位 `OR` 运算。

### 与激活状态绑定的取消令牌

`GameObjectExtensions.GetDisableToken` 返回一个与 `GameObject` 激活状态绑定的取消令牌：对象失活时令牌被取消。

```csharp
var disableToken = gameObject.GetDisableToken();
await SomeLongRunningOperationAsync(disableToken); // 对象失活时自动取消
```

## 主循环

Unity 主循环（player loop）由一系列按固定顺序执行的阶段组成，每个阶段又由若干子系统构成。Unity 允许在任意位置插入自定义的子系统，`PlayerLoopUtility` 就是基于这一点实现的：它在每个阶段的脚本回调前后插入了多个自定义代码执行点，于是这些代码会在每一帧的指定位置被调用。

### PlayerLoopPhase

`PlayerLoopPhase` 表示 Unity 主循环（player loop）中的阶段，共 8 个：

| 值              | 含义                                                                                      |
|-----------------|-------------------------------------------------------------------------------------------|
| `FixedUpdating` | 在 `FixedUpdate.ScriptRunBehaviourFixedUpdate` 之前                                       |
| `FixedUpdated`  | 在 `FixedUpdate.ScriptRunBehaviourFixedUpdate` 之后                                       |
| `Updating`      | 在 `Update.ScriptRunBehaviourUpdate` 之前                                                 |
| `Updated`       | 在 `Update.ScriptRunBehaviourUpdate` 之后、`Update.ScriptRunDelayedDynamicFrameRate` 之前 |
| `UpdateYielded` | 在 `Update.ScriptRunDelayedDynamicFrameRate` 之后、`Update.ScriptRunDelayedTasks` 之前    |
| `UpdatePosted`  | 在 `Update.ScriptRunDelayedTasks` 之后                                                    |
| `LateUpdating`  | 在 `PreLateUpdate.ScriptRunBehaviourLateUpdate` 之前                                      |
| `LateUpdated`   | 在 `PreLateUpdate.ScriptRunBehaviourLateUpdate` 之后                                      |

### IPlayerLoopItem 与 PlayerLoopUtility

实现 `IPlayerLoopItem` 的对象可以在指定的阶段每帧执行一次。

```csharp
public sealed class MyPlayerLoopItem : IPlayerLoopItem
{
    void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
    {
        // 每帧执行一次
    }
}
```

`PlayerLoopUtility` 负责注册、注销和查询这些对象。

```csharp
var item = new MyPlayerLoopItem();
PlayerLoopUtility.AddPlayerLoopItem(item, PlayerLoopPhase.Updated);
PlayerLoopUtility.RemovePlayerLoopItem(item, PlayerLoopPhase.Updated);

var currentPhase = PlayerLoopUtility.CurrentPhase; // 当前正在执行的阶段，不在任何阶段内时为 null
```

自己实现 `IPlayerLoopItem` 有很多好处：可以拿它取代 `Update`——大量的 `MonoBehaviour.Update` 会带来额外的调用开销，让执行时间变长；也可以把逻辑挂到 `Update` 之外的阶段上。本包的 `PlayerLoopScope`、各种计时器/计数器、`ScrollView`、`UnityMainThreadTaskScheduler` 等类型就都实现了它。

除了实现接口，也可以直接注册一个委托作为"延续"（continuation），它只会被执行一次。带状态参数的重载可以避免闭包分配。

```csharp
PlayerLoopUtility.AddContinuation(
    () => Debug.Log("下一次 PlayerLoopPhase.Updated 时执行"),
    PlayerLoopPhase.Updated
);

PlayerLoopUtility.AddContinuation(
    state => Debug.Log(state),
    "hello",
    PlayerLoopPhase.Updated
);
```

### PlayerLoopScope

`PlayerLoopScope` 把"每帧执行 → 停止执行"包装成 `IDisposable`，用 `using` 语句管理生命周期。除了直接传一个委托，也可以使用带 `object` 状态参数的构造函数，把要访问的对象一次性传进去，避免为闭包分配内存。

```csharp
// 加载进度条：任务需要一定时间，期间每帧刷新进度

public sealed class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar;

    private float _progress;

    private async Task LoadAsync()
    {
        using (new PlayerLoopScope(
                   state =>
                   {
                       var loadingScreen = (LoadingScreen)state;
                       loadingScreen.progressBar.value = loadingScreen._progress;
                   },
                   this,
                   PlayerLoopPhase.Updated
               ))
        {
            // TODO: 在这里执行需要一定时间的任务，例如下载并加载资源
        }
    }
}
```

### 每帧更新的有限状态机

`UnityUpdateStateMachine<T>` 是 `StateMachine<T>`（来自 Aurora 包）的派生类，状态除了进入/退出回调之外，还可以实现 `IUnityUpdateState<T>`，从而在当前状态下每帧执行一次自定义逻辑。

```csharp
public sealed class IdleState : IUnityUpdateState<Type>
{
    Type IState<Type>.Id => typeof(IdleState);

    void IState<Type>.OnEnter(StateMachine<Type> stateMachine, IState<Type> from)
    {
        // ...
    }

    void IState<Type>.OnExit(StateMachine<Type> stateMachine, IState<Type> to)
    {
        // ...
    }

    void IUnityUpdateState<Type>.OnUnityUpdate(UnityUpdateStateMachine<Type> stateMachine)
    {
        // 作为当前状态期间每帧执行一次
        // var deltaTime = Time.deltaTime;
        // TODO
    }
}

private UnityUpdateStateMachine<Type> _stateMachine;

private void OnEnable()
{
    _stateMachine = new UnityUpdateStateMachine<Type>();
    _stateMachine.AddState(new IdleState());
    _stateMachine.ScheduleTransitionTo(typeof(IdleState));
}

private void Update()
{
    // 先反复调用 Update，直到返回 false，把待处理的状态切换全部执行完
    while (_stateMachine.Update())
    {
    }
    // 再调用一次 UnityUpdate，执行当前状态的每帧逻辑
    _stateMachine.UnityUpdate();
}
```

`CurrentUnityUpdateState` 是 `CurrentState` 转换成 `IUnityUpdateState<T>` 的结果；当前状态没有实现该接口时为 `null`。

`DoUnityUpdate` 是虚方法，可以重写它，把自定义逻辑插在当前状态的 `OnUnityUpdate` 之前或之后。

```csharp
protected override void DoUnityUpdate(IUnityUpdateState<Type> currentUnityUpdateState)
{
    // 在 OnUnityUpdate 之前执行的自定义逻辑
    base.DoUnityUpdate(currentUnityUpdateState);
    // 在 OnUnityUpdate 之后执行的自定义逻辑
}
```

## 可等待对象

这些结构体实现了 C# 的可等待约定，可以直接 `await`，用于在 `async` 方法中等待 Unity 的异步操作。它们都提供了接受 `CancellationToken` 的重载。

```csharp
// 等待任意 AsyncOperation
await new AsyncOperationAwaitable(SceneManager.LoadSceneAsync("Level2"));

// 等待 Resources 加载
var texture = await new ResourceRequestAwaitable<Texture2D>(Resources.LoadAsync<Texture2D>("Textures/Tree"));
var asset = await new ResourceRequestAwaitable(Resources.LoadAsync("Prefabs/Tree"));

// 等待 AssetBundle 创建与加载
var assetBundle = await new AssetBundleCreateRequestAwaitable(AssetBundle.LoadFromFileAsync(path));
var prefab = await new AssetBundleRequestAwaitable<GameObject>(assetBundle.LoadAssetAsync<GameObject>("Tree"));
var allObjects = await new AssetBundleRequestAwaitable.All(assetBundle.LoadAllAssetsAsync());
var allTypedObjects = await new AssetBundleRequestAwaitable<GameObject>.All(assetBundle.LoadAllAssetsAsync<GameObject>());
```

`DelayFrameAwaitable` 等待若干帧之后继续，帧数在指定的阶段上计数，因此它同时也会等待该阶段的到来；`PlayerLoopPhaseAwaitable` 等待下一个指定阶段的到来，它的嵌套结构体 `Any` 则在传入的多个阶段里最早执行的那一个继续。

```csharp
// 等待 1 帧，并且等待到 Updated 阶段
await new DelayFrameAwaitable(1, PlayerLoopPhase.Updated);

// 等待 2 帧，并且等待到 Updated 阶段，可以被取消
await new DelayFrameAwaitable(2, PlayerLoopPhase.Updated, cancellationToken);

// 等待下一个 Updated 阶段
await new PlayerLoopPhaseAwaitable(PlayerLoopPhase.Updated);

// 在 Update 与 LateUpdate 中较早到来的那一个继续
await new PlayerLoopPhaseAwaitable.Any(new[] { PlayerLoopPhase.Updating, PlayerLoopPhase.LateUpdating });
```

## 任务

### UnityTasks

`UnityTasks` 提供与上面这些可等待对象等价的、返回 `Task` 的方法，适合需要把任务保存起来、组合或等待的场景。

```csharp
// 等待指定的主循环阶段
await UnityTasks.WhenPlayerLoopPhase(PlayerLoopPhase.Updated);

// 等待多个主循环阶段中最早到来的那一个
await UnityTasks.WhenAnyPlayerLoopPhase(new[] { PlayerLoopPhase.Updating, PlayerLoopPhase.LateUpdating });

// 等待 Unity 异步操作完成
await UnityTasks.WhenAsyncOperation(SceneManager.LoadSceneAsync("Level2"));

// 延迟（使用 Stopwatch 计时）
await UnityTasks.Delay(TimeSpan.FromSeconds(1), PlayerLoopPhase.Updating);

// 延迟（unscaled 为 false 时使用 Time.time 计时，为 true 时使用 Time.unscaledTime 计时）
await UnityTasks.DelayUnityTime(TimeSpan.FromSeconds(1), true, PlayerLoopPhase.Updating);

// 延迟若干帧；frameCount 为 -1 时永远不完成（只能通过取消结束），可用于需要一直等待的场合
await UnityTasks.DelayFrame(1, PlayerLoopPhase.Updated);
await UnityTasks.DelayFrame(-1, PlayerLoopPhase.Updated);

// 等待截图完成（PNG 文件）
// 注意：此功能未在所有平台上测试，可能不稳定
await UnityTasks.WhenScreenshotCaptured(@"D:\screenshot.png");
```

所有方法都提供了接受 `CancellationToken` 的重载。

### UnityMainThreadTaskScheduler

`UnityMainThreadTaskScheduler` 是把任务调度到 Unity 主线程的 `TaskScheduler`。它每帧在 `PlayerLoopPhase.UpdateYielded` 阶段执行排队中的任务。

为了让 Unity 主线程不被死锁，它拒绝接受带 `TaskCreationOptions.LongRunning` 的任务（会记录一条错误日志）。

`BeginProcess` 与 `Continue` 是 `protected virtual` 属性，分别控制"是否可以开始新一轮处理"和"是否可以处理下一个任务"，可以在派生类中重写以实现自己的调度策略（例如每帧只处理固定数量的任务）。

## 计时器与计数器

### Timer

`ITimer` 是计时器接口，`Change` 用来更新首次触发之前的等待时间，以及之后再次触发的间隔。`dueTime` 与 `period` 的含义与 `System.Threading.Timer` 一致。

实现有三个，区别在于用什么来计时，三者都在指定的主循环阶段检查是否到时：

- `UnityTimePlayerLoopTimer`：使用 `Time.time`
- `UnityUnscaledTimePlayerLoopTimer`：使用 `Time.unscaledTime`
- `StopwatchPlayerLoopTimer`：使用 `Stopwatch`

```csharp
// 允许在 callback 内部修改、禁用或 Dispose 计时器
var callback = (TimerTriggerCallback)((timer, state) => Debug.Log($"triggered by {state}"));

// 1 秒后触发一次
var timer = new UnityUnscaledTimePlayerLoopTimer(
    callback,
    "my timer",
    TimeSpan.FromSeconds(1), // 重置计时器，然后 1 秒后首次触发
    Timeout.InfiniteTimeSpan, // 首次触发后，禁用计时器
    PlayerLoopPhase.Updated
);

// 也可以先创建，再通过 Change 配置
var timer2 = new UnityUnscaledTimePlayerLoopTimer(callback, "my timer", PlayerLoopPhase.Updated);
timer2.Change(
    TimeSpan.Zero, // 重置计时器，然后立即首次触发
    TimeSpan.FromSeconds(1) // 首次触发后，每隔 1 秒再次触发
);
timer2.Change(
    Timeout.InfiniteTimeSpan, // 禁用计时器
    Timeout.InfiniteTimeSpan // 首次触发后，禁用计时器（由于计时器已禁用，所以实际不会使用这个参数）
);

timer.Dispose();
timer2.Dispose(); // 不再使用时记得释放
```

`UnityUtility.CancelAfter` 就是基于 `StopwatchPlayerLoopTimer` 实现的：它在指定的等待时间之后取消一个 `CancellationTokenSource`，返回的对象释放时终止这次取消。

```csharp
using (UnityUtility.CancelAfter(cancellationTokenSource, TimeSpan.FromSeconds(3)))
{
    // 这 3 秒之内，如果其他逻辑取消了 cancellationTokenSource，这次取消就不再发生
}
```

### Counter

`ICounter` 是计数器接口，`Change` 的参数含义与 `ITimer.Change` 一一对应，只是单位是帧数而不是时间：`dueCount` 为 -1 时禁用，为 0 时立即触发，大于 0 时在指定帧数之后触发；`period` 为 -1 时首次触发后禁用。

`UnityFrameCountPlayerLoopCounter` 使用 `Time.frameCount` 计数，并在指定的主循环阶段检查。

```csharp
// 允许在 callback 内部修改、禁用或 Dispose 计数器
var callback = (CounterTriggerCallback)((counter, state) => Debug.Log($"triggered by {state}"));

// 3 帧后触发一次
var counter = new UnityFrameCountPlayerLoopCounter(
    callback,
    "my counter",
    3, // 重置计数器，然后 3 帧后首次触发
    -1, // 首次触发后，不再触发
    PlayerLoopPhase.Updated
);

// 也可以先创建，再配置
var counter2 = new UnityFrameCountPlayerLoopCounter(callback, "my counter", PlayerLoopPhase.Updated);
counter2.Change(
    0, // 重置计数器，然后立即首次触发
    3 // 首次触发后，每隔 3 帧再次触发
);
counter2.Change(
    -1, // 禁用计数器
    -1 // 首次触发后，禁用计数器（由于计数器已禁用，所以实际不会使用这个参数）
);

counter.Dispose();
counter2.Dispose();
```

### FromToTimer

`IFromToTimer` 表示一个"从起点计数到终点"的计时器，适合做进度条、倒计时、数值动画之类的东西。`PlayerLoopFromToTimer` 是它的实现，在指定的主循环阶段用 `Time.deltaTime` 或 `Time.unscaledDeltaTime` 推进当前时间。

```csharp
// 从 0 计数到 100，使用缩放时间推进
var timer = new PlayerLoopFromToTimer(PlayerLoopPhase.Updated)
{
    UseUnscaledTime = false
};
timer.From = 0;
timer.To = 100;

timer.TimeChanged += (sender, e) =>
    Debug.Log($"Time: {e.PreviousValue} -> {e.NewValue} ({e.Causation})");
timer.ProgressChanged += (sender, e) => Debug.Log($"Progress: {e.NewValue}");
timer.Completed += sender => Debug.Log("Completed");

timer.Running = true; // 开始计时；计时到 To 之后会自动停止并触发 Completed

// 也可以直接设置进度或当前时间，这时同样会触发相应的事件
timer.Progress = 0.5;
timer.Time = 30;

timer.Running = false;
timer.Dispose();
```

- `From`：起点。
- `To`：终点。允许取 `double.PositiveInfinity` 或 `double.NegativeInfinity`，此时计时永不结束。
- `Time`：当前时间；赋值时会被限制在 `From` 与 `To` 之间。
- `TimeTruncated`：当前时间去掉小数部分之后的整数值。
- `Progress`：进度，取值 `[0, 1]`；`From` 与 `To` 相等时恒为 1。
- `Running`：是否正在计时。设为 `true` 时把计时器注册到主循环，设为 `false` 时注销。
- `UseUnscaledTime`：推进时间时使用未缩放时间还是缩放时间。
- `TimeChanged` / `TimeTruncatedChanged` / `ProgressChanged` 的事件参数是 `FromToTimerValueChangedEventArgs`，包含 `PreviousValue`、`NewValue`，以及表示变化原因的 `Causation`：`Timing` 表示由计时引起，`Modification` 表示由外部赋值引起。
- `Completed` 在计时到达 `To` 时触发。

## 图形

这一组组件都直接继承 `MaskableGraphic`，用 `OnPopulateMesh` 现场生成网格，因此不需要任何图片资源就能得到形状；它们同时实现了 `ILayoutElement`，在 `Image` 那样的场合也可以作为布局元素使用。

| 组件                     | 说明                               |
|--------------------------|------------------------------------|
| `Block`                  | 色块                               |
| `Clear`                  | 透明（不绘制，可参与 UI 事件检测） |
| `Circle`                 | 圆                                 |
| `Annulus`                | 环                                 |
| `RoundedRectangle`       | 圆角矩形                           |
| `RoundedRectangleBorder` | 圆角矩形边框                       |
| `CustomGraphic`          | 由用户自己定义顶点与三角形的图形   |

除了颜色（继承自 `Graphic` 的 `color`）之外，这些图形的公共属性有：

- `Texture`：贴图。设置之后，顶点的 UV 会按照图形在自身矩形内的归一化位置来计算，于是贴图会被"裁剪"成图形的形状；不设置时 UV 没有意义，图形只用 `color` 着色。
- `Segments`：圆弧的细分段数。段数越多越平滑，顶点也越多。
- `UseExactRaycastLocation`：是否使用精确的点击区域。默认的 `false` 表示用图形的矩形来判定点击；设为 `true` 之后会用多边形的"点在多边形内"算法判定，圆形的四个角上不会再误判，代价是每次判定都要遍历多边形的顶点。

圆角矩形还有四个角各自的半径，每个角都有一个"是否使用归一化长度"的开关和半径值。

`RoundedRectangleBorder` 在此基础上多了 `ThicknessNormalized` 与 `Thickness`，用来描述边框的粗细。

`CustomGraphic` 用两个列表描述整张网格：`Vertices` 是顶点的归一化位置与颜色，`Triangles` 是三个一组、按顺时针排列的顶点下标。因为这两个列表是 `List<T>` 字段，修改之后需要调用 `SetVerticesDirty`（或通过 Inspector 触发一次重绘）才会生效。`NormalizedPositionAndColor` 就是"归一化位置 + 颜色"的一对值。

## 控件

### EnhancedButton

`EnhancedButton` 是按钮控件，它没有继承 `Button`，而是从 `UIBehaviour` 重写的一套实现，因此不受 `Selectable` 限制，功能也比 `Button` 多：内置了"开关"（toggle）能力，并且可以直接对 `Graphic` 的 `color` 做状态着色。

```csharp
public class Example : MonoBehaviour
{
    [SerializeField]
    private EnhancedButton button;

    [SerializeField]
    private Graphic buttonBackgroundGraphic;

    [SerializeField]
    private EnhancedButton.ColorBlock colorBlock;

    private void OnButtonClicked(EnhancedButton b, PointerEventData eventData)
    {
        Debug.Log("Button clicked!");
    }

    private void OnButtonUpdated(EnhancedButton b)
    {
        buttonBackgroundGraphic.color = colorBlock.GetColor(b);
    }

    private void OnEnable()
    {
        button.Clicked += OnButtonClicked;
        button.Updated += OnButtonUpdated;

        // 手动引发一次 Updated 事件，刷新 buttonBackgroundGraphic 的颜色
        // （开发者无需设置预制体上 buttonBackgroundGraphic 的颜色）
        button.Refresh();
    }

    private void OnDisable()
    {
        button.Clicked -= OnButtonClicked;
        button.Updated -= OnButtonUpdated;
    }
}
```

`State`（`EnhancedButtonState`）由指针的位置与按下状态决定：指针不在按钮内是 `Default`，指针在按钮内是 `Hovered`，在按钮内按下且仍然可以触发点击是 `Pressed`。

四个事件的触发时机：

| 事件            | 触发时机                                                                                                  |
|-----------------|-----------------------------------------------------------------------------------------------------------|
| `Clicked`       | 左键点击（`doubleClick` 为 `true` 时会延迟到超过双击间隔之后才触发），或右键点击且 `rightClick` 为 `true` |
| `DoubleClicked` | 双击（仅在 `doubleClick` 为 `true` 时）                                                                   |
| `Toggled`       | `IsOn` 发生变化（`SetIsOnWithoutNotify` 不触发）                                                          |
| `Updated`       | 状态、开关状态或可交互状态发生变化；也可以通过 `Refresh` 主动触发                                         |

按钮按下之后，只要指针移出按钮，这次点击就会被取消（`eventData.eligibleForClick` 被置为 `false`）。

`ColorBlock` 描述六种状态下的颜色，`GetColor(button)` 会根据按钮当前的状态和是否可交互取出对应颜色。

```csharp
var colorBlock = new EnhancedButton.ColorBlock
{
    defaultColor = Color.gray,
    hoveredColor = Color.white,
    pressedColor = Color.gray,
    nonInteractableDefaultColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
    nonInteractableHoveredColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
    nonInteractablePressedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f)
};

var color = colorBlock.GetColor(button);
```

`ColorBlock` 里的颜色只由 `State` 与 `Interactable` 决定，与 `IsOn` 无关。如果你需要为 On / Off 状态下的按钮使用不同的颜色，可以使用两个 `ColorBlock`，根据 `IsOn` 选用：

```csharp
[SerializeField]
private EnhancedButton.ColorBlock onColorBlock;

[SerializeField]
private EnhancedButton.ColorBlock offColorBlock;

private void OnButtonUpdated(EnhancedButton b)
{
    var colorBlock = b.IsOn ? onColorBlock : offColorBlock;
    buttonBackgroundGraphic.color = colorBlock.GetColor(b);
}
```

### EnhancedButtonGroup

`EnhancedButtonGroup` 是按钮组：把按钮通过 `EnhancedButton.Group` 注册进来，标题栏、工具栏、单选按钮组这类"同一时刻最多只有一个按钮处于开启状态"的场合都由它来协调。按钮组不要求与按钮处于同一层级，注册关系完全由引用决定。

```csharp
public sealed class SingleSelectionExample : MonoBehaviour
{
    [SerializeField]
    private EnhancedButton button;

    private void OnButtonToggled(EnhancedButton b)
    {
        if (b.IsOn)
        {
            b.Group.SetAllButtonsOffWithoutNotify(b);
        }
        else
        {
            // 用户点击了此按钮，使得此按钮的 IsOn 从 true 变为了 false；
            // 但由于这是单选按钮组，需要始终有一个按钮保持开启，所以这里立刻把它重新恢复为开启状态
            b.SetIsOnWithoutNotify(true);
        }
    }

    private void OnEnable()
    {
        button.Toggled += OnButtonToggled;
    }

    private void OnDisable()
    {
        button.Toggled -= OnButtonToggled;
    }
}
```

需要注意的是，只有处于激活且启用状态（`isActiveAndEnabled`）的按钮才会存在于按钮组中。

### EnhancedSlider

`EnhancedSlider` 是滑块，取值是一个 `[0, 1]` 之间的 `float`。

```csharp
var slider = gameObject.AddComponent<EnhancedSlider>();

slider.Fill = fillRectTransform; // 填充部分
slider.Handle = handleRectTransform; // 手柄
slider.Direction = Slider.Direction.LeftToRight;
slider.Interactable = true;

slider.Value = 0.5f; // 赋值时会被限制到 [0, 1]，并触发 ValueChanged
slider.SetValueWithoutNotify(0.5f); // 赋值但不触发 ValueChanged

slider.ValueChanged += (s, value, oldValue) => Debug.Log($"{oldValue} -> {value}");
slider.OperationBeginning += s => Debug.Log("operation beginning");
slider.OperationEnded += s => Debug.Log("operation ended");

var isOperating = slider.IsOperating; // 是否正在被操作
```

`OperationBeginning` 与 `OperationEnded` 严格成对出现，可以用来实现"拖动结束才提交数值"这类逻辑。

## ScrollView

`ScrollView` 原本应该放在"控件"一节内，但它的体量太大，因此独立成节。

`ScrollView` 是一个大量复用子对象的滚动列表，用来替代 `UnityEngine.UI.ScrollRect` 之上的常见做法：数据量很大时只实例化"能看见的那几个"item，滚出可视范围的 item 会被回收再用。它建立在 `ScrollRect` 之上，使用它自己的 `ScrollRect` 组件。

`ScrollView` 是抽象基类，实际使用 `HorizontalScrollView` 或 `VerticalScrollView`。

### IScrollViewController

`ScrollView` 本身不持有数据，所有数据都通过控制器提供：实现 `IScrollViewController`，把它交给 `ScrollView`。

```csharp
public sealed class MyScrollViewController : MonoBehaviour, IScrollViewController
{
    [SerializeField]
    private MyScrollViewItem _itemPrefab;

    private readonly List<string> _data = new() { "A", "B", "C" };

    public int GetItemCount(ScrollView scrollView)
    {
        return _data.Count;
    }

    public float GetItemSize(ScrollView scrollView, int index)
    {
        // 一般情况下，不同类型（System.Type）的 item 具有不同的尺寸
        // 但如果你愿意，也可以让同一类型的 item 具有不同的尺寸，只要保证每一个 index 对应的尺寸是固定的
        return 100;
    }

    public ScrollViewItem GetItem(ScrollView scrollView, int index, out bool isNewCreated)
    {
        // 取出一个 item：优先复用已回收的，没有可复用的就实例化预制体
        // 具体的数据刷新由 item 自己的 OnGet 完成
        var item = scrollView.GetRecycledOrCreateNewItem(_itemPrefab, out isNewCreated);
        item.Initialize(_data[index]);
        return item;
    }

#if UNITY_EDITOR
    public string GetItemName(ScrollViewItem item)
    {
        // 仅在编辑器下用于给 item 命名，方便调试；不需要支持时返回 null 或抛异常都可以
        return "MyScrollViewItem";
    }
#endif
}
```

`GetItem` 里调用 `GetRecycledOrCreateNewItem(itemPrefab, out isNewCreated)` 是必须的，它会按 `ScrollViewItem.identifier` 去找已回收的 item，找不到才实例化传入的预制体。通常情况下，`GetItem` 只做这件事，再将 item 的数据交给它；真正做到"新建时初始化一次"与"每次取用时刷新"的区分，交给 `ScrollViewItem` 子类重写的 `OnGet(bool isNewCreated)`（见下一节）。

`GetItemName` 只在编辑器环境下编译，仅用于给 Hierarchy 窗口里的 item 起一个可读的名字。

### ScrollViewItem

`ScrollViewItem` 是列表 item 的基类，挂在 item 预制体上。

```csharp
public sealed class MyScrollViewItem : ScrollViewItem
{
    [SerializeField]
    private Text _text;

    private string _textContent;

    public void Initialize(string textContent)
    {
        _textContent = textContent;
    }

    protected override void OnGet(bool isNewCreated)
    {
        // item 被取出使用时，在这里刷新数据
        _text.text = _textContent;
    }

    protected override void OnVisible()
    {
        // item 进入视口时
    }

    protected override void OnInvisible()
    {
        // item 离开视口时
    }

    protected override void OnReturn(bool isScrollViewBeingDestroyed)
    {
        // item 被回收时；isScrollViewBeingDestroyed 为 true 表示 ScrollView 正在被销毁
    }
}
```

- `identifier`：区分不同类型的 `ScrollViewItem`，在编辑期赋值，运行时不要修改。回收是按 `identifier` 匹配的。
- `ScrollView`：所属的滚动列表。
- `Index`：当前在列表中的下标；不在使用中时为 -1。
- `Visible`：是否已经进入视口。

`OnGet` 与 `OnReturn` 成对出现，`OnVisible` 与 `OnInvisible` 成对出现，它们的关系可以这样理解：item 被取出使用时还没有进入视口，滚动到视口之内才触发 `OnVisible`；滚出视口先触发 `OnInvisible`，被回收时才触发 `OnReturn`。

### 挂载与刷新

```csharp
var scrollView = gameObject.AddComponent<VerticalScrollView>();

scrollView.SetControllerAndReload(new MyScrollViewController(itemPrefab));

// 数据变化之后重新加载；三种重载分别保持当前位置、指定内容位置、指定归一化滚动位置
scrollView.Reload();
scrollView.ReloadWithContentPosition(0);
scrollView.ReloadWithNormalizedScrollPosition(0.5);
```

`Reload` 会清空当前所有 item、重新向控制器询问 item 数量与每个 item 的尺寸，然后重新摆放。数据变化之后应当立刻调用一次。

`Refresh` 则是"保持数据不变、只按当前位置重新计算哪些 item 该存在"，它由 `ScrollRect.onValueChanged` 自动驱动；如果手动改了 `ContentPosition` 或 `NormalizedScrollPosition` 之后需要立刻拿到最新的活动 item，可以自己调用一次 `Refresh`。

```csharp
scrollView.Refresh();
```

### 位置、尺寸与索引

```csharp
var itemCount = scrollView.ItemCount;
var viewportSize = scrollView.ViewportSize; // 视口尺寸
var contentSize = scrollView.ContentSize; // 内容尺寸
var overflowedContentSize = scrollView.OverflowedContentSize; // 内容超出视口的部分，不超出时为 0

// 内容位置：沿滚动方向的坐标
var contentPosition = scrollView.ContentPosition;
scrollView.ContentPosition = 0;

// 归一化滚动位置：通常为 [0, 1]，类型是 double，精度比 ScrollRect.normalizedPosition 高
var normalizedScrollPosition = scrollView.NormalizedScrollPosition;
scrollView.NormalizedScrollPosition = 0.5;

// 活动 item（被实例化出来的 item）与可见 item（真正落在视口内的 item）的下标
var firstActiveIndex = scrollView.FirstActiveIndex; // 没有活动 item 时为 -1
var lastActiveIndex = scrollView.LastActiveIndex;
var firstVisibleIndex = scrollView.FirstVisibleIndex; // 没有可见 item 时为 -1
var lastVisibleIndex = scrollView.LastVisibleIndex;

// 取某个下标处的 item；该下标没有活动 item 时返回 null
var item = scrollView[3];

// 取所有指定类型的活动 item
var items = new List<MyScrollViewItem>();
scrollView.GetActiveItems(items);

// 取某个 item 的起止内容位置
var itemBeginPosition = scrollView.GetItemBeginPosition(3);
var itemEndPosition = scrollView.GetItemEndPosition(3);

// 按内容位置查找下标
var firstIndex = scrollView.FindFirstIndex(contentPosition); // 起点位置大于等于该位置的第一个 item，找不到为 -1
var lastIndex = scrollView.FindLastIndex(contentPosition); // 终点位置大于等于该位置的第一个 item，找不到为 -1
var closestIndex = scrollView.FindClosestIndex(contentPosition); // 距离该位置最近的 item，item 数为 0 时为 -1
```

四种位置之间的换算：

```csharp
// 内容位置 <-> 归一化视口位置（0 表示视口起点贴住内容起点，1 表示视口终点贴住内容终点）
var normalizedViewportPosition = scrollView.ConvertContentPositionToNormalizedViewportPosition(contentPosition);
var contentPosition2 = scrollView.ConvertNormalizedViewportPositionToContentPosition(0.5f);

// 内容位置 <-> 归一化滚动位置
var normalizedScrollPosition2 = scrollView.ConvertContentPositionToNormalizedScrollPosition(contentPosition);
var contentPosition3 = scrollView.ConvertNormalizedScrollPositionToContentPosition(0.5);
```

### 内边距、间距与预加载

`Padding`（`RectOffset`）与 `Spacing` 会被转发给内容对象上的布局组，设置之后会自动重新加载；`ChildForceExpandSize` 决定 item 在非滚动方向上是否填满内容。

```csharp
scrollView.Padding = new RectOffset(8, 8, 8, 8);
scrollView.Spacing = 4;
scrollView.ChildForceExpandSize = true;
```

`leadingActiveOffset` 与 `trailingActiveOffset` 用来做预加载：在视口的起点与终点之外再多保留一段距离，让即将进入视口的 item 提前被创建好，从而避免滚动时出现空白。取值应当大于等于 0，设置之后在下次刷新时生效。

```csharp
scrollView.leadingActiveOffset = 100; // 视口起点之前 100 像素内的 item 也保持活动
scrollView.trailingActiveOffset = 100;
```

### 自动吸附

`ScrollView` 内置了自动吸附：滚动速度降下来之后，自动把最靠近某个位置的 item 对齐到视口中间。

```csharp
scrollView.snapTrigger =
    ScrollViewSnapTrigger.OnEndDrag | ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged;

scrollView.snapSpeedThreshold = 300; // 速度小于该值时触发吸附
scrollView.scrollSnapDelay = 0.3f; // 滚动停止多久之后触发吸附（用于 OnNormalizedScrollPositionChanged）

scrollView.snapFindNormalizedViewportPosition = 0.5f; // 以视口的这个归一化位置为基准寻找最近的 item
scrollView.snapIncludingSpacing = false; // 计算 item 起止位置时是否把 item 前后的留白（内边距/间距）也算进去
scrollView.snapNormalizedItemPosition = 0.5f; // 在 item 的起止位置之间插值的权重，用来算吸附目标位置
scrollView.snapJumpNormalizedViewportPosition = 0.5f; // 把目标位置吸附到视口的这个归一化位置

scrollView.snapDurationMode = ScrollViewSnapDurationMode.Dynamic; // 吸附时长是固定值还是由距离与速度算出来
scrollView.snapDuration = 0.25f; // Fixed 模式下的时长
scrollView.snapSpeed = 900; // Dynamic 模式下的速度
scrollView.snapInterpolation = Interpolation.OutCubic; // 吸附过程使用的插值类型

// 也可以主动触发一次吸附
scrollView.Snap();

// 停止正在进行的吸附动画
scrollView.StopTween();
```

`ScrollViewSnapTrigger` 是一个 `[Flags]` 枚举，可以组合：

| 值                                  | 触发时机                                                                                                     |
|-------------------------------------|--------------------------------------------------------------------------------------------------------------|
| `None`                              | 不吸附                                                                                                       |
| `OnEndDrag`                         | 拖拽结束时立刻吸附                                                                                           |
| `OnNormalizedScrollPositionChanged` | 没有拖拽、滚动位置发生变化且速度低于阈值时吸附（适用于"甩"出去之后靠惯性慢慢减速的场景）                     |
| `OnPointerUpWithLowSpeed`           | 没有拖拽、松开指针且 `ScrollRect` 速度极低时吸附（适用于按住拖拽之后停住再松开的场景，通常与上一个组合使用） |

### 滚动条与速度限制

```csharp
scrollView.ScrollbarVisibility = ScrollbarVisibility.OnlyIfNeeded; // Never / OnlyIfNeeded / Always
scrollView.speedLimit = 0; // 大于 0 时限制拖动速度

var isDragging = scrollView.Dragging; // 是否正在被拖拽
var isTweening = scrollView.Tweening; // 是否正在播放吸附动画
```

### 创建 ScrollView

手动创建 `ScrollView` 的过程非常繁琐，`GameObject/UI/Scroll View - Aurora Unity` 菜单会打开 `Create New ScrollView` 窗口，让你选好方向、尺寸与滚动条位置之后，一次性生成结构完整的`HorizontalScrollView` / `VerticalScrollView`（包括视口、内容、滚动条，并且把该接的引用都接好）。

## 布局

### FlowLayoutGroup

`FlowLayoutGroup` 是流式布局组，对应 `HorizontalLayoutGroup` / `VerticalLayoutGroup` 的"一行放不下就换行"版本：先沿着主轴排列子对象，一行（或一列）放不下时自动换到下一行（或下一列）。

```csharp
var flowLayoutGroup = gameObject.AddComponent<FlowLayoutGroup>();

flowLayoutGroup.Axis = RectTransform.Axis.Horizontal; // 主轴
flowLayoutGroup.Spacing = new Vector2(8, 8); // 间距（x 为同一行内的间距，y 为行与行之间的间距）
flowLayoutGroup.PreferredSizeAloneAxis = 0; // 主轴方向上的首选尺寸

var lineCount = flowLayoutGroup.LineCount; // 行数（或列数）

if (flowLayoutGroup.TryGetIndexOf(child, out var indexAloneAxis, out var indexAloneOtherAxis))
{
    // 沿主轴方向的下标（即该子对象在本行/本列内的位置）与沿另一轴方向的下标（即行/列下标）
    // 具体哪个是第一维取决于 Axis 与子对象的排列顺序，需要时请实测确认
}

var childrenOfLine = new List<RectTransform>();
flowLayoutGroup.GetLayoutChildrenOfLine(0, childrenOfLine); // 取第 0 行的所有子对象
```

另外，`FlowLayoutGroup` 会把首选尺寸按行（列）数累计起来，因此配合最外层的 `ContentSizeFitter` 使用时也能得到正确的高度。

### ScrollLayoutGroup

`ScrollLayoutGroup` 是滚动布局组：它把子对象沿水平（或垂直）方向排成间距固定的一列，并且可以用一个浮点索引决定哪一个子对象位于正中间——这是做选择器、日期选择、卡片轮播这类交互的核心。

```csharp
var scrollLayoutGroup = gameObject.AddComponent<ScrollLayoutGroup>();

scrollLayoutGroup.horizontal = true; // 是否沿水平方向排列

scrollLayoutGroup.CenterIndex = 3.5f; // 位于正中的下标（浮点数：3.5 表示"3 号与 4 号的正中间"）
var centerIndex = scrollLayoutGroup.CenterIndex;

var currentCenter = scrollLayoutGroup.CurrentCenter; // 当前位于正中的子对象（取 CenterIndex 四舍五入之后的下标）
var childCount = scrollLayoutGroup.LayoutChildrenCount; // 参与布局的子对象个数

scrollLayoutGroup.SetLayoutChildToCenter(child); // 把指定的子对象移到正中
var index = scrollLayoutGroup.GetLayoutChildIndex(child); // 取指定子对象在所有子对象中的下标

var rectMask2D = scrollLayoutGroup.RectMask2D; // 同一对象上的 RectMask2D
```

`CenterIndex` 可以读取（用来取当前选中项），也可以写入（用来滚动到某一项，写进去的浮点数就是动画的插值结果）。

## 界面系统

界面系统把界面（`View`）组织成一棵树：每个界面都是一个节点，每个界面都可以拥有多个子界面，整棵树挂在界面容器（`ViewContainer`）之下。容器负责把根界面放到合适的父对象上，也负责"谁在最上面"这类查询。

界面不是"自己 `new` 出来"的：它由 `ViewHandler` 创建，并且要求创建出来的界面处于未激活或未启用状态，这样在打开流程的最后一步才激活它，`OnEnable` 里就可以放心地写初始化代码。

### ViewHandler

`ViewHandler` 负责创建和释放某一类界面。它有两个必须实现、两个可以重写的成员：

```csharp
public sealed class GeneralViewHandler : ViewHandler
{
    // 这个处理器负责创建的界面类型
    public override Type HandledViewType => typeof(View); // 可处理你的项目中所有类型的界面

    // 创建一个"未激活或未启用"的界面
    public override async Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 加载预制体
        // 假设预制体的名称是 XxxView.Prefab（其中 XxxView 是具体界面类型的名称）
        // 假设预制体的路径是 <Unity project path>/Assets/Resources/Prefabs/Views/XxxView.prefab
        var prefab = await new ResourceRequestAwaitable<GameObject>(
            Resources.LoadAsync<GameObject>($"Prefabs/Views/{typeof(T).Name}"),
            cancellationToken
        );

        // 在 InactiveContainer 下实例化，确保 OnEnable 不会提前执行
        var gameObject = Object.Instantiate(prefab, UnityEnvironment.InactiveContainer);

        // 交给界面系统
        return (T)gameObject.GetComponent<View>();
    }

    // 释放界面；默认实现是 Object.Destroy，需要对象池时可以重写
    public override void ReleaseView(View view)
    {
        base.ReleaseView(view);
    }
}
```

处理器需要在打开界面之前注册：

```csharp
ViewHandler.Register(new GeneralViewHandler());
```

`PrefabLessViewHandler` 已经由本包自动注册（见本文档的"运行环境与初始化"一节），它负责创建不带预制体的 `PrefabLessView`。

`ViewHandler.Get<T>()`（或 `Get(Type)`）会从所有已注册的处理器里挑出最合适的一个：先筛出 `HandledViewType` 是 `T` 本身或 `T` 的基类的处理器，再取继承链条最短的那个；因为 `PrefabLessViewHandler` 的 `HandledViewType` 是 `PrefabLessView`，所以只实现了 `PrefabLessView` 的界面会落到它上面。找不到时会返回 `null`。

### 打开界面

打开一个"根界面"需要一个界面容器：

```csharp
// 界面容器：rectTransform 会成为该容器下所有根界面的父对象，必须处于激活状态并且位于某个 Canvas 之下
var container = View.AddContainer(containerRectTransform);

var view = await View.OpenAsync<MainMenuView>(container);
```

打开一个"子界面"则需要一个父界面：

```csharp
// state 是打开界面时传入的用户数据，会赋值给 view.State
var childView = await View.OpenAsync<ItemDetailView>(parentView, state: new ItemDetailViewState { Id = 42 });
```

两种打开方式都有"不等待"的版本 `BeginOpen`，它的返回类型是 `void`，内部以 `async void` 的方式执行，适合不关心结果的场合。

```csharp
View.BeginOpen<MainMenuView>(container);
View.BeginOpen<ItemDetailView>(parentView, state);
```

打开流程的最后一步会把界面激活并启用；在这之前，`OnSettingActiveAndEnabling` 会被调用一次，可以在派生类里重写它，做"激活之前"的准备工作。

```csharp
protected override void OnSettingActiveAndEnabling()
{
    // 在界面被激活、启用之前执行
}
```

如果处理器创建出来的界面已经是激活且启用状态，打开时会抛出 `BehaviourActiveAndEnabledException`，所以 **界面预制体必须是未激活或已禁用的**。本包在编辑器下提供了 `Aurora Unity/Validate View Prefabs` 菜单，可以批量检查工程里所有预制体是否符合这个要求。

所有打开方式都有接受 `CancellationToken` 的重载。

### View

`View` 是所有界面的基类，它同时实现了 `IEnumerable<View>`，可直接用 `foreach` 遍历直接子界面。

```csharp
// 树结构
var container = view.Container;
var parent = view.Parent;
var root = view.Root;
var isRoot = view.IsRoot;
var isLeaf = view.IsLeaf;
var isChild = view.IsChildOf(otherView);
var isTopmost = view.IsTopmost();

// 与打开/关闭一起传递的用户数据
var state = view.State;
var closeState = view.CloseState;

// 子界面
var child = view.GetChild<MainMenuView>();
var children = new List<MainMenuView>();
view.GetChildren(children);

// 遍历
foreach (View directChild in view)
{
}

foreach (View descendant in view.GetEnumerator(TreeEnumOrder.DepthFirstDlr))
{
}

// 从当前界面出发查找
var found = view.GetViewFromThis<MainMenuView>(TreeEnumOrder.DepthFirstLdl);
var results = new List<MainMenuView>();
view.GetViewsFromThis(TreeEnumOrder.BreadthFirstLr, results);

// 关闭；closeState 会赋值给 CloseState，子界面会一并关闭
view.Close();
view.Close(closeState: 42);
```

`Container` 与 `Parent` 也可以赋值，用来把界面移动到别的容器或别的父界面之下；界面树里的所有后代节点会一起被移动，移动之后会重新对齐到父对象的四条边。

`childContainer` 是"子界面的父对象"：不设置时子界面直接挂在当前界面的 `RectTransform` 之下。

`GetView<T>()` 与 `GetViews<T>`（静态方法）在整个界面系统范围内查找：

```csharp
var view = View.GetView<MainMenuView>(); // 等价于 GetView<MainMenuView>(TreeEnumOrder.DepthFirstRld)
var view2 = View.GetView<MainMenuView>(TreeEnumOrder.BreadthFirstLr);

var allViews = new List<MainMenuView>();
View.GetViews(TreeEnumOrder.DepthFirstDlr, allViews);

// 最上面的界面
var topmost = View.GetTopmostView();
if (View.IsTopmost(view))
{
    // ...
}

// 容器
var containerCount = View.ContainerCount;
var container = View.GetContainer(0);
View.RemoveContainerAt(0); // 容器里还有界面时会抛出 InvalidOperationException
```

### ViewContainer

`ViewContainer` 描述"根界面挂在哪里"。

```csharp
var container = View.AddContainer(containerRectTransform);

var rectTransform = container.RectTransform; // 该容器下根界面的父对象

var view = container.GetViewFromContainer<MainMenuView>(TreeEnumOrder.DepthFirstDlr);

var views = new List<MainMenuView>();
container.GetViewsFromContainer(TreeEnumOrder.BreadthFirstLr, views);
```

容器要求传入的 `RectTransform` 处于激活状态，并且能在父级上找到一个 `Canvas`，否则会抛出 `GameObjectInactiveException` 或 `ComponentNotGotException`。

### View.Scope\<T\>

`View.Scope<T>` 把"打开 → 使用 → 关闭"包装成 `IDisposable`，适合用 `using` 语句表达"这个界面只在这个作用域内存在"。典型用法有两种：

- 显示 **加载界面**：在执行长时间加载任务期间挡住屏幕，任务结束后自动关闭。
- 显示 **全屏遮挡界面**：在显示 **对话框界面** 期间挡住下层界面，避免用户操作到对话框背后的内容。

```csharp
using (new View.Scope<MainMenuView>(view))
{
    // ...
} // 离开作用域时 view 被关闭
```

`Scope<T>.View` 可以取回作用域所管理的界面。

### PrefabLessView 与 MaskView

`PrefabLessView` 是"不需要预制体、在运行时直接创建"的界面基类。它由 `PrefabLessViewHandler` 创建（本包已经注册好），创建时会顺带把 `GameObject.layer` 设为 "UI" 层。

`MaskView` 是现成的遮罩界面：一个填满父对象的半透明色块，点击时可以选择关闭自己，也可以执行一段逻辑。它的参数通过 `State` 传入。

```csharp
await View.OpenAsync<MaskView>(
    parentView,
    new MaskView.Args
    {
        MaskColor = new Color(0, 0, 0, 0.5f), // 遮罩颜色
        CloseOnClick = true, // 点击时先关闭自己
        InvocationOnClick = new InvocationAction(() => Debug.Log("mask clicked"))
    }
);

// 也可以直接传颜色
await View.OpenAsync<MaskView>(parentView, new Color(0, 0, 0, 0.5f));

var maskGraphic = maskView.MaskGraphic; // 遮罩用的 Graphic
```

### View Inspector

`Window/Aurora Unity/View Inspector` 会打开一个名为 `View Inspector` 的窗口，按层级缩进列出当前所有的界面容器，以及每个容器里的界面树：容器一层列出它的 `RectTransform`，下面递归缩进列出它包含的每一个界面（从根界面到子界面）。

这个窗口是只读的（整体套在 `EditorGUI.DisabledScope(true)` 里），只能看不能改；`View.Dirty` 被置位时它会自动重绘，所以打开界面、关闭界面都会立刻反映出来。当还没有任何界面容器时，窗口里只显示一句 `There is nothing here.`。

调试界面系统时很有用：打开某个界面后它能直接告诉你这个界面挂在了哪个容器、是谁的子界面、嵌了多深，不必去 Hierarchy 窗口里手工找对象。

## 空间索引

### Quadtree\<T\>

`Quadtree<TElementPosition>` 是四叉树：把二维空间递归地四等分，用空间换时间，让"查询某个圆形或矩形范围内的元素"不必遍历全部元素。`TElementPosition` 是元素的位置类型。

`Quadtree<T>` 是抽象类：元素需要实现 `IQuadtreeElement<T>`（提供 `Position` 与 `SetOwner`），节点需要由 `ICreateNodeHandler` 创建。

```csharp
public sealed class MyQuadtree : Quadtree<Vector2>
{
    public MyQuadtree(Aabb2 aabb2, int levels, int maxElements) : base(new CreateNodeHandler(), aabb2, levels, maxElements)
    {
    }

    private sealed class CreateNodeHandler : ICreateNodeHandler
    {
        public Node CreateNode(Quadtree<Vector2> tree, Node parent, int level, Aabb2 aabb2)
        {
            // 参数按原样交给 Node 的构造函数
            return new MyNode(tree, parent, level, aabb2);
        }
    }

    private sealed class MyNode : Node
    {
        public MyNode(Quadtree<Vector2> tree, Node parent, int level, Aabb2 aabb2) : base(tree, parent, level, aabb2)
        {
        }

        // 判断指定的范围是否包含该位置，用于把元素放进正确的子节点
        protected override bool Contains(Aabb2 aabb2, Vector2 elementPosition)
        {
            return aabb2.Contains(elementPosition);
        }

        // 该位置到指定点的距离的平方
        protected override float GetSquareDistance(Vector2 elementPosition, Vector2 point)
        {
            return (elementPosition - point).sqrMagnitude;
        }
    }
}
```

元素实现：

```csharp
public sealed class MyQuadtreeElement : IQuadtreeElement<Vector2>
{
    public Vector2 Position { get; set; }

    private Quadtree<Vector2>.Node _owner;

    // 由四叉树调用，用于记录"直接持有自己的节点"，这样移除时才能正确地从那个节点里摘除
    public void SetOwner(Quadtree<Vector2>.Node owner)
    {
        _owner = owner;
    }
}
```

使用：

```csharp
var quadtree = new MyQuadtree(Aabb2.CenterSize(new Vector2(0, 0), new Vector2(1000, 1000)), levels: 5, maxElements: 8);

quadtree.Add(element); // 加入；已经在树里时返回 false
quadtree.Contains(elementPosition);
quadtree.Remove(element); // 通过元素记录的 owner 节点移除

var rootNode = quadtree.RootNode;
var aabb2 = quadtree.Aabb2; // 整棵树的覆盖范围
var levels = quadtree.Levels; // 最大层数（推荐 5）
var maxElements = quadtree.MaxElements; // 单个节点直接容纳的元素个数上限（推荐 8）

// 范围查询
var inCircle = new List<IQuadtreeElement<Vector2>>();
quadtree.GetElementsInCircle(new Vector2(0, 0), 100, inCircle);

var inAabb2 = new List<IQuadtreeElement<Vector2>>();
quadtree.GetElementsInAabb2(Aabb2.CenterSize(new Vector2(0, 0), new Vector2(200, 200)), inAabb2);
```

节点（`Quadtree<T>.Node`）也可以直接用：

```csharp
var node = quadtree.RootNode;

var tree = node.Tree;
var parent = node.Parent;
var level = node.Level; // 根节点为 0
var range = node.Aabb2;
var count = node.Count; // 直接与间接持有的元素总数

node.Contains(elementPosition);

var elements = new List<IQuadtreeElement<Vector2>>();
node.GetElements(elements); // 取该节点及其子树下的所有元素

var typedElements = new List<MyQuadtreeElement>();
node.GetElements(typedElements); // 只取指定类型的元素

var children = new Quadtree<Vector2>.Node[4];
node.GetChildren(children); // 取子节点

node.Remove(element);
```

### Octree\<T\>

`Octree<TElementPosition>` 是八叉树，用法与四叉树完全对应，只是空间从二维变成三维：节点各有 8 个子节点，范围类型是 `Aabb3`，位置类型通常用 `Vector3`。

```csharp
public sealed class MyOctree : Octree<Vector3>
{
    public MyOctree(Aabb3 aabb3, int levels, int maxElements) : base(new CreateNodeHandler(), aabb3, levels, maxElements)
    {
    }

    private sealed class CreateNodeHandler : ICreateNodeHandler
    {
        public Node CreateNode(Octree<Vector3> tree, Node parent, int level, Aabb3 aabb3)
        {
            return new MyNode(tree, parent, level, aabb3);
        }
    }

    private sealed class MyNode : Node
    {
        public MyNode(Octree<Vector3> tree, Node parent, int level, Aabb3 aabb3) : base(tree, parent, level, aabb3)
        {
        }

        protected override bool Contains(Aabb3 aabb3, Vector3 elementPosition)
        {
            return aabb3.Contains(elementPosition);
        }

        protected override float GetSquareDistance(Vector3 elementPosition, Vector3 point)
        {
            return (elementPosition - point).sqrMagnitude;
        }
    }
}
```

```csharp
var octree = new MyOctree(Aabb3.CenterSize(Vector3.zero, new Vector3(1000, 1000, 1000)), levels: 5, maxElements: 8);

octree.Add(element);
var inSphere = new List<IOctreeElement<Vector3>>();
octree.GetElementsInSphere(Vector3.zero, 100, inSphere);

var inAabb3 = new List<IOctreeElement<Vector3>>();
octree.GetElementsInAabb3(Aabb3.CenterSize(Vector3.zero, new Vector3(200, 200, 200)), inAabb3);
```

两个索引在构造时都会校验：范围里不能出现 `NaN` 或无穷大，`levels` 与 `maxElements` 都必须大于等于 1，`createNodeHandler` 不能为 `null`。

## 数学与几何

### Aabb2

`Aabb2` 是二维轴对齐包围盒（axis-aligned bounding box），可以序列化、可格式化、可比较。

```csharp
// 四种构造方式
var a = new Aabb2(1, 1); // 以一个点为最小值和最大值
var b = new Aabb2(new Vector2(1, 1));
var c = new Aabb2(0, 0, 10, 10); // 最小值与最大值
var d = new Aabb2(new Vector2(0, 0), new Vector2(10, 10));

// 以中心与尺寸构造
var e = Aabb2.CenterSize(5, 5, 10, 10);
var f = Aabb2.CenterSize(new Vector2(5, 5), new Vector2(10, 10));

// 包含所有传入的点
var g = Aabb2.Points(new[] { new Vector2(1, 1), new Vector2(3, 5), new Vector2(2, 2) });
```

```csharp
var aabb2 = new Aabb2(new Vector2(0, 0), new Vector2(10, 10));

// 分量
var minX = aabb2.MinX;
var minY = aabb2.MinY;
var centerX = aabb2.CenterX;
var centerY = aabb2.CenterY;
var maxX = aabb2.MaxX;
var maxY = aabb2.MaxY;

// 向量形式的读写
aabb2.Min = new Vector2(0, 0);
aabb2.Center = new Vector2(5, 5);
aabb2.Max = new Vector2(10, 10);
aabb2.Size = new Vector2(10, 10); // 尺寸
aabb2.Extends = new Vector2(5, 5); // 半尺寸

// 归一化位置与实际位置互转（t 为各分量的权重）
var point = aabb2.Lerp(new Vector2(0.5f, 0.5f)); // (5, 5)
var t = aabb2.Unlerp(new Vector2(5, 5)); // (0.5, 0.5)

// 扩大范围
aabb2.Include(new Vector2(-5, -5));
aabb2.Include(new Aabb2(new Vector2(-5, -5), new Vector2(-1, -1)));

// 判断
aabb2.Contains(new Vector2(5, 5));
aabb2.Contains(new Aabb2(new Vector2(1, 1), new Vector2(2, 2)));
aabb2.Overlaps(new Aabb2(new Vector2(-1, -1), new Vector2(1, 1)));

// 与 Unity 的类型互转
Rect rect = (Rect)aabb2; // 显式转换
Aabb2 back = (Aabb2)rect;
Aabb3 aabb3 = aabb2; // 隐式转换到三维
Aabb2 back2 = aabb3;
```

需要注意的是 `Contains(Vector2)` 的判定是"包含下边界与左边界，不包含上边界与右边界"（`minX <= x && maxX > x`），与 `Rect` 的约定一致。

### Aabb3

`Aabb3` 是三维版本，用法与 `Aabb2` 一一对应，另外还可以包含二维的点和盒子、与 `Bounds` 互转。

```csharp
var aabb3 = Aabb3.CenterSize(Vector3.zero, Vector3.one * 10);

aabb3.Include(new Vector2(1, 1)); // 二维点按 z = 0 处理
aabb3.Include(new Vector3(1, 1, 1));
aabb3.Include(Aabb2.CenterSize(Vector2.zero, Vector2.one * 10));
aabb3.Include(Aabb3.CenterSize(Vector3.zero, Vector3.one * 10));

aabb3.Contains(new Vector2(1, 1));
aabb3.Contains(new Vector3(1, 1, 1));
aabb3.Contains(Aabb2.CenterSize(Vector2.zero, Vector2.one));
aabb3.Contains(Aabb3.CenterSize(Vector3.zero, Vector3.one));
aabb3.Overlaps(Aabb3.CenterSize(Vector3.zero, Vector3.one));

Bounds bounds = (Bounds)aabb3;
Aabb3 back = (Aabb3)bounds;
```

### UnityMath

`UnityMath` 是一组补充 `Mathf` 的数学方法，覆盖三角函数、不做范围限制的插值、矩形内外坐标换算、重心坐标与点是否在多边形内。

```csharp
// 一次同时得到余弦与正弦，避免调用两次三角函数
var cosSin = UnityMath.CosSin(Mathf.PI * 0.5f); // (0, 1)

// 点相对于矩形的 UV（类似 Rect.PointToNormalized，但结果不限制在 [0, 1] 内）
var uv = UnityMath.GetUV(new Rect(0, 0, 100, 100), new Vector2(50, 50)); // (0.5, 0.5, 0, 0)

// 不做范围限制的插值（Unity 内置的 Vector2.Lerp 会把 t 限制到 [0, 1] 内）
var lerped2 = UnityMath.LerpUnclamped(new Vector2(0, 0), new Vector2(10, 10), new Vector2(2, 2));
var lerped3 = UnityMath.LerpUnclamped(Vector3.zero, Vector3.one, Vector3.one * 2);
var lerped4 = UnityMath.LerpUnclamped(Vector4.zero, Vector4.one, Vector4.one * 2);

// 归一化坐标与矩形内的点互转（结果都不限制在 [0, 1] 内）
var point = UnityMath.NormalizedToPointUnclamped(new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
var normalized = UnityMath.PointToNormalizedUnclamped(new Rect(0, 0, 100, 100), new Vector2(50, 50));

// 三角形、多边形
var barycentric = UnityMath.GetBarycentricCoordinatesOfTriangle(p, a, b, c); // 计算三角形的重心坐标，三个分量依次是 α、β、γ
var isInsideTriangle = UnityMath.IsPointInsideTriangle(barycentric);
var isInsidePolygon = UnityMath.IsPointInsidePolygon(point2, polygonVertices);
```

### RectExtensions

`RectExtensions.Contains` 判断一个矩形是否完全包含另一个矩形（`Rect` 自带的 `Contains` 只接受一个点）。

```csharp
var outer = new Rect(0, 0, 100, 100);
var inner = new Rect(10, 10, 10, 10);
var contains = outer.Contains(inner); // true
```

### 点是否在多边形内

`IInclusionOfAPointInAPolygonAlgorithm` 定义了"判断点是否在多边形内"的算法接口，本包提供了基于环绕数（winding number）的实现 `WindingNumber`。`UnityMath.IsPointInsidePolygon` 内部用的就是它。

```csharp
IInclusionOfAPointInAPolygonAlgorithm algorithm = new WindingNumber();
var isInside = algorithm.IsPointInsidePolygon(point, polygonVertices);
```

## 对象与组件工具

### UnityEngineObjectUtility

`UnityEngineObjectUtility` 主要是为了绕过 Unity 主线程的限制：像 `UnityEngine.Object` 的相等判断这类操作，默认只能在主线程调用，而这些方法可以安全地在其他线程上使用。它通过反射取到 Unity 内部能力（只在类型初始化时做一次），从而可以在非主线程上判断对象的存活、相等与实例 ID。

```csharp
var ptr = UnityEngineObjectUtility.GetCachedPtr(obj); // 包装的原生 C++ 对象的内存地址
var instanceId = UnityEngineObjectUtility.GetInstanceId(obj); // 实例 ID
var exists = UnityEngineObjectUtility.DoesObjectWithInstanceIdExist(instanceId); // 该 ID 的对象是否还存在

var isAlive = UnityEngineObjectUtility.IsAlive(obj); // 实例是否还活着（已销毁时为 false）

// 不受"只能在主线程调用"限制的相等性判断
if (UnityEngineObjectUtility.Equals(a, b))
{
    // ...
}
```

`UnityEngineObjectEqualityComparer.Instance` 是与之配套的 `IEqualityComparer<Object>`，可以用在 `Dictionary`、`HashSet` 等集合里，让已销毁的对象与 `null` 被视为相等。

```csharp
var set = new HashSet<GameObject>(UnityEngineObjectEqualityComparer.Instance);
```

### ComponentExtensions

```csharp
var rigidbody = component.GetOrAddComponent<Rigidbody>(); // 有就取，没有就加
component.RemoveComponent<Rigidbody>(); // 有就销毁
```

### GameObjectExtensions

```csharp
gameObject.SetLayerRecursively(UnityEnvironment.UILayer); // 连同所有子对象一起设置层
gameObject.SetTagRecursively("Player"); // 连同所有子对象一起设置标签

var component = gameObject.GetOrAddComponent<Rigidbody>();
gameObject.RemoveComponent<Rigidbody>();

var disableToken = gameObject.GetDisableToken(); // 与激活状态绑定的取消令牌

gameObject.DestroyChildren(); // 销毁所有子对象（倒序）
gameObject.DestroyChildrenImmediate(); // 立刻销毁所有子对象
```

### CommonExtensions

`List<T>` 上一组"销毁并清空"的扩展方法，用来替代手写循环。泛型约束是 `UnityEngine.Object`。

```csharp
var enemies = new List<Enemy>();
enemies.ClearAndDestroy(); // 逐个 Destroy 之后清空列表
enemies.ClearAndDestroyImmediate(); // 逐个 DestroyImmediate 之后清空列表
```

### TransformExtensions

```csharp
transform.SwapSibling(otherTransform); // 与另一个同级对象交换顺序
transform.SetBeforeSibling(otherTransform); // 排到另一个同级对象之前
transform.SetAfterSibling(otherTransform); // 排到另一个同级对象之后

var hierarchies = new List<Transform>();
transform.GetHierarchies(hierarchies); // 整条父链上的 Transform

var hierarchyNames = new List<string>();
transform.GetHierarchyNames(hierarchyNames); // 整条父链上的名字

var scenePath = transform.GetScenePath(); // 在场景中的路径，例如 "Canvas/Panel/Button"

transform.DestroyChildren();
transform.DestroyChildrenImmediate();
```

### TransformUtility

```csharp
var same = TransformUtility.AreTransformsShareSameParent(a, b);
var same2 = TransformUtility.AreTransformsShareSameParent(a, b, c);
var same3 = TransformUtility.AreTransformsShareSameParent(a, b, c, d);
var same4 = TransformUtility.AreTransformsShareSameParent(transforms); // params 重载
```

### 父子层级深度比较器

`GameObjectParentCountComparer` 与 `ComponentParentCountComparer` 按"父级层数"（在 Hierarchy 窗口中嵌套的深度）比较对象，用于给排序提供稳定且符合直觉的顺序（例如 UI 里越靠上的对象越先更新）。

```csharp
objects.Sort(GameObjectParentCountComparer.Instance);
components.Sort(ComponentParentCountComparer.Instance);

objects.Sort(GameObjectParentCountComparer.InstanceReversed); // 反序
components.Sort(ComponentParentCountComparer.InstanceReversed);
```

### 异常与相关枚举

本包内置了一组语义明确的异常，用来在"前置条件不满足"时尽早失败并给出可读的信息。

```csharp
throw new GameObjectActiveException(gameObject); // 期望对象不处于激活状态时
throw new GameObjectInactiveException(gameObject); // 期望对象处于激活状态时
throw new BehaviourActiveAndEnabledException(behaviour); // 期望 Behaviour 不处于"激活且启用"时
throw new BehaviourInactiveAndDisabledException(behaviour); // 期望 Behaviour 处于"不激活且不启用"时
throw new BehaviourInactiveOrDisabledException(behaviour); // 期望 Behaviour 处于"不激活或不启用"时
throw new ComponentNotGotException(gameObject, GetComponentMethod.Self, typeof(Canvas)); // 期望能取到某个组件时
throw new UnityWebRequestException(unityWebRequest); // 网络请求出错时
```

这些异常都带有对应的对象引用（`GameObject`、`Behaviour`、`UnityWebRequest` 等），方便定位。

`GetComponentMethod` 枚举描述"从哪里取组件"，用在 `ComponentNotGotException` 里：

| 值                          | 对应的方法                        |
|-----------------------------|-----------------------------------|
| `Self`                      | `GetComponent<T>`                 |
| `Parent`                    | `GetComponentInParent<T>()`       |
| `ParentIncludingInactive`   | `GetComponentInParent<T>(true)`   |
| `Children`                  | `GetComponentInChildren<T>()`     |
| `ChildrenIncludingInactive` | `GetComponentInChildren<T>(true)` |

`When` 枚举描述"什么时候"（`Always`、`Playing`、`NotPlaying`），被 `[GuiDisable]` 用来控制在什么情况下把字段置灰。

## 网络请求

### UnityWebRequestUtility 与 UnityWebRequestExtensions

`UnityWebRequestUtility` 提供一组基于任务（Task）的方法，用来替代 `UnityWebRequest` 的回调式 API。`UnityWebRequestExtensions` 是同样功能的扩展方法版本，可以链式书写。

```csharp
using var unityWebRequest = UnityWebRequest.Get("https://example.com");
unityWebRequest.timeout = 60;

// 只发送请求
await unityWebRequest.SendWebRequestAsync(cancellationToken);

// 发送请求并取回数据
var text = await unityWebRequest.GetStringAsync(cancellationToken);
var bytes = await unityWebRequest.GetByteArrayAsync(cancellationToken);
var stream = await unityWebRequest.GetStreamAsync(cancellationToken);

// 也可以调用静态方法（把请求作为第一个参数传入）
await UnityWebRequestUtility.SendWebRequestAsync(unityWebRequest, cancellationToken);
var text2 = await UnityWebRequestUtility.GetStringAsync(unityWebRequest, cancellationToken);
```

几个判断与校验方法：

```csharp
// 请求是否已经释放
if (UnityWebRequestUtility.IsDisposed(unityWebRequest))
{
    // ...
}

// 请求是否超时（错误信息等于 UnityUtility.UnityWebRequestTimeoutString 时视为超时）
var isTimeout = unityWebRequest.IsTimeout();
var isTimeout2 = UnityWebRequestUtility.IsTimeout(unityWebRequest);

// HTTP 状态码是否表示成功
var isSuccess = unityWebRequest.IsSuccessStatusCode();

// 抛出异常形式的校验
UnityWebRequestUtility.ThrowIfDisposed(unityWebRequest); // 已释放 -> ObjectDisposedException
UnityWebRequestUtility.ThrowIfFaulted(unityWebRequest); // 请求出错 -> UnityWebRequestException
UnityWebRequestUtility.ThrowIfNotSuccessStatusCode(unityWebRequest); // 状态码不是成功 -> UnityWebRequestException
unityWebRequest.ThrowIfNotSuccessStatusCode();
```

注意 `GetStringAsync` 等方法在请求出错时会抛出 `UnityWebRequestException`，取消时抛出 `OperationCanceledException`；它们只有在 Unity 主线程上调用才是安全的（否则会抛出 `UnityException`）。

### UnityWebRequestHandler

`UnityWebRequestHandler` 是用 `UnityWebRequest` 实现的 `HttpMessageHandler`，于是可以直接把 Unity 的网络栈接到 `System.Net.Http.HttpClient` 上，用 `HttpClient` 的全部能力（`HttpRequestMessage`、`HttpResponseMessage`、拦截器等）。

```csharp
// 基本用法
using var httpClient = new HttpClient(new UnityWebRequestHandler());

// 在每次请求发出之前做一些准备（设置超时、证书等）
using var httpClient2 = new HttpClient(
    new UnityWebRequestHandler(unityWebRequest =>
    {
        unityWebRequest.timeout = 60;
    })
);

var response = await httpClient.GetAsync("https://example.com");
var content = await response.Content.ReadAsStringAsync();
```

它有两个职责值得注意：

- 如果 `SendAsync` 不是在 Unity 主线程上调用的（例如在 `Task.Run` 里，或者 `HttpClient` 把请求调度到了线程池），它会先等待并回到主线程再发起请求。这是 `UnityWebRequest` 本身的限制。
- `UnityWebRequest` 抛出的 `UnityWebRequestException` 会被转换成 `HttpRequestException`，以符合 `HttpMessageHandler` 的约定。

## Preference

### Preference\<TValue\>

`Preference<TValue>` 是对 `UnityEngine.PlayerPrefs` 的封装基类：用 `UnityEngine.PlayerPrefs` 支持的三种基本类型（`int`、`float`、`string`）存放值，再用一对转换方法把它与用户真正使用的类型 `TValue` 对应起来。

派生类按基本类型分三种，它们的构造函数需要一个 `PreferenceConverterPair`：

```csharp
// 基本类型是 int
public class Int32Preference<TValue> : Preference<TValue> { ... }

// 基本类型是 float
public class SinglePreference<TValue> : Preference<TValue> { ... }

// 基本类型是 string
public class StringPreference<TValue> : Preference<TValue> { ... }
```

基类提供的成员：

```csharp
var key = preference.Key;
var keyExists = preference.KeyExists; // PlayerPrefs 里是否存在该键
var valueType = preference.ValueType; // Int32 / Single / String

preference.SetValue(value);
var value2 = preference.GetValue();
var value3 = preference.GetValue(defaultValue); // 键不存在时返回 defaultValue

preference.Remove(); // 从 PlayerPrefs 移除
```

```csharp
var hp = new IdentityInt32Preference("hp");
hp.SetValue(100);
var hpValue = hp.GetValue();

var masterVolume = new IdentitySinglePreference("masterVolume");
masterVolume.SetValue(0.8f);
var volume = masterVolume.GetValue();

var playerName = new IdentityStringPreference("playerName");
playerName.SetValue("Kevin");
var name = playerName.GetValue();
```

用户类型不是基本类型时，传入自己的转换方法；`PreferenceConverterPair<TValue, TPreferenceValue>` 就是"值 → 基本值"与"基本值 → 值"这一对转换：

```csharp
// 用 Vector2Int 作为用户类型，底层用 string 存放
public sealed class Vector2IntPreference : StringPreference<Vector2Int>
{
    public Vector2IntPreference(string key) : base(
        key,
        new PreferenceConverterPair<Vector2Int, string>(
            value => $"{value.x},{value.y}",
            preferenceValue =>
            {
                var parts = preferenceValue.Split(',');
                return new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
            }
        )
    )
    {
    }
}
```

也可以直接使用 `PreferenceConverterPair<TValue, TPreferenceValue>` 而不写派生类。

### 包装与被覆盖的偏好设置

`WrappedPreference<TValue>` 包装了另一个 `Preference<TValue>`，用来在它的基础上附加行为；`DefaultValuePreference<TValue>` 进一步附加了一个默认值。本包提供两个现成的实现：

- `OverridePreference<TValue>`：当被包装的 `GetValue()` 抛出异常时，把原始值覆盖为 `DefaultValue` 再返回它（读取原始值会抛异常，所以要把它覆盖掉）。
- `OverlyPreference<TValue>`：当被包装的值不存在时，强行把 `DefaultValue` 写入原始值再返回它；其余情况一律返回被包装的值。

`PreferenceValueType` 是"底层基本类型"的枚举：`Int32`、`Single`、`String`。

如果需要"原样存取值"（不做任何转换），可以用 `IdentityInt32Preference`、`IdentitySinglePreference`、`IdentityStringPreference`。

## 光标

`CursorInfo` 是"光标贴图 + 热点 + 模式"的一组值；`CursorStack` 用栈的形式管理光标：`Push` 设置一个新光标（通常是"鼠标悬停在某个区域上"），`Pop` 恢复到上一个。

```csharp
// 如果在 PlayerSettings 里设置了默认光标，程序启动之后应当把初始光标告诉它
CursorStack.InitialCursorInfo = new CursorInfo
{
    texture = initialTexture,
    hotspot = Vector2.zero,
    cursorMode = CursorMode.Auto
};

CursorStack.Push(new CursorInfo
{
    texture = handTexture,
    hotspot = new Vector2(16, 16),
    cursorMode = CursorMode.Auto
});
CursorStack.Pop();

var count = CursorStack.Count; // 栈里暂存的光标个数
```

在编辑器环境下退出播放模式时，光标栈会被自动清空。

## 颜色

### AuroraColor

`AuroraColor` 是用四个 `byte`（RGBA）表示的颜色，它通过 `StructLayout(LayoutKind.Explicit)` 把一个 `int` 和四个 `byte` 重叠在同一个 4 字节内存上，因此和 `Color32` 的布局一致，转换不需要任何额外开销。

```csharp
var color = new AuroraColor(255, 0, 0); // 默认不透明
var translucent = new AuroraColor(255, 0, 0, 128);

// HTML 颜色字符串：#RGB、#RRGGBB、#RGBA、#RRGGBBAA；开头的 # 可以省略；不支持 red 这样的颜色名
var fromHtml = new AuroraColor("#FF0000");
var fromHtml2 = new AuroraColor("FF000080");

// 分量
var r = color.R;
var g = color.G;
var b = color.B;
var a = color.A;

// 按下标读写：0 -> R，1 -> G，2 -> B，3 -> A，其余下标抛出 IndexOutOfRangeException
color[0] = 128;

// 与 Unity 的类型互转（都是隐式转换）
Color32 unityColor32 = color;
Color unityColor = color;
AuroraColor fromUnityColor32 = unityColor32;
AuroraColor fromUnityColor = unityColor;
```

### ColorUtility

`ColorUtility.ParseHtmlString` 是把 HTML 颜色字符串直接解析成 `Color` 的静态方法。

```csharp
var color = ColorUtility.ParseHtmlString("#FF0000");
```

### 颜色扩展方法

`ColorExtensions` 与 `Color32Extensions` 提供解构、色相判断与换色相的操作。

```csharp
var color = new Color(1, 0, 0, 1);

// 解构成分量
var (r, g, b, a) = color;
var (h, s, v) = color; // 解构成 HSV

// R、G、B 三个分量相等时色相没有意义
if (color.IsHUndefined())
{
    // ...
}

// Color32 版本
var color32 = (Color32)color;
var (r32, g32, b32, a32) = color32;
var (h32, s32, v32) = color32;
if (color32.IsHUndefined())
{
    // ...
}

// 复制并换一个色相（h 取值 [0, 1]，越界会抛出 ArgumentOutOfRangeException）
var changed = color32.WithH(0.5f);

// 换色相，并告诉你颜色是否真的变了
// 例如红色换成色相 0 或 1 都还是红色（色相是循环的），此时新旧颜色相同，所以返回 false
if (color32.TryWithH(0.5f, out var result))
{
    // ...
}
```

`Color` 没有 `WithH`，需要换色相时可以用 `Color32` 版本，或者自己用 `Color.HSVToRGB` 组合。

## 资源、截图与其他运行时工具

### AsyncOperationUtility

```csharp
if (AsyncOperationUtility.IsDisposed(asyncOperation))
{
    // ...
}
```

### UnityUtility

```csharp
// 常量
var clickDelayTime = UnityUtility.ClickDelayTime; // 连击的两次点击之间的最大间隔，0.3 秒
var timeoutString = UnityUtility.UnityWebRequestTimeoutString; // "Request timeout"
var vertexCountMax = UnityUtility.VertexCountPerMeshMaxValue; // 65000 - 1

// 把对象名字里多余的内容去掉，变成更适合日志与 Hierarchy 窗口显示的形式
UnityUtility.OptimizeName(@object);

// 如果指定的对象正是当前事件系统的选中对象，就取消选中
UnityUtility.DeselectEventSystemCurrentSelectedGameObject(gameObject);
// 如果指定的对象正是指定事件系统的选中对象，就取消选中
UnityUtility.DeselectEventSystemCurrentSelectedGameObject(gameObject, eventSystem);

// 在指定的等待时间之后取消一个 CancellationTokenSource；释放返回值可以终止这次取消
using (UnityUtility.CancelAfter(cancellationTokenSource, TimeSpan.FromSeconds(3)))
{
    // ...
}

// 截图（PNG 文件）
UnityUtility.BeginCaptureScreenshot(@"D:\screenshot.png"); // 不等截图结束
await UnityUtility.CaptureScreenshotAsync(@"D:\screenshot.png"); // 等截图结束
await UnityUtility.CaptureScreenshotAsync(@"D:\screenshot.png", cancellationToken);
```

### SpriteUtility 与 SpriteRendererUtility

把"归一化坐标"换算成精灵的本地坐标或世界坐标，在做特效挂点、血条定位这类事情时很有用。

```csharp
var localPosition = SpriteUtility.NormalizedToLocalPosition(sprite, new Vector2(0.5f, 1f)); // 精灵顶部中点

var localPosition2 = SpriteRendererUtility.NormalizedToLocalPosition(spriteRenderer, new Vector2(0.5f, 1f));
var worldPosition = SpriteRendererUtility.NormalizedToWorldPosition(spriteRenderer, new Vector2(0.5f, 1f));

// 与 flipX / flipY 相关的乘数（未翻转分量为 1，翻转分量为 -1）
var multiplier = SpriteRendererUtility.GetFlipMultiplier(spriteRenderer);
```

### Texture2DUtility

```csharp
if (Texture2DUtility.IsRedQuestionMarkTexture(texture))
{
    // 判断这张贴图是否是 Unity 在无法读取图片内容时生成的红色问号图（尺寸 8x8，全局唯一）
}
```

### ProfilerScope

```csharp
using (new ProfilerScope("MySection"))
{
    // 这段代码会被 Profiler 采样
}

using (new ProfilerScope("MySection", targetObject))
{
    // 采样会关联到 targetObject，便于在 Profiler 里定位
}
```

### ServerTimeOwner

`ServerTimeOwner` 用来记录"服务器时间"：客户端在某一个时刻做一次时间同步，之后就可以用它来推算当前的服务器时间。

```csharp
var serverTimeOwner = new ServerTimeOwner();

// 在收到服务器时间的那一刻设置
serverTimeOwner.CurrentTime = DateTimeOffset.UtcNow;

// 之后再取，得到的是按本地时间流逝推算出来的时间
var currentTime = serverTimeOwner.CurrentTime; // 未设置时为 null
```

### 屏幕变化通知

`NotifyScreenSizeChangedScope` 与 `NotifyScreenOrientationChangedScope` 把"监听屏幕尺寸/方向变化"包装成 `IDisposable`：变化时在指定的主循环阶段回调，释放时停止监听。

```csharp
using (var scope = new NotifyScreenSizeChangedScope(
           size => Debug.Log($"screen size changed: {size}"),
           PlayerLoopPhase.Updated
       ))
{
    // ...
}

using var scope = new NotifyScreenOrientationChangedScope(
    orientation => Debug.Log($"orientation changed: {orientation}"),
    PlayerLoopPhase.Updated
);
```

## 编辑器工具

编辑器功能位于 `Aurora.UnityEditor` 程序集，命名空间是 `Aurora.UnityEditor`。

### 右键菜单

在 Inspector 窗口中右键组件标题，会出现一组互转菜单，用来把一种 UI 组件替换成另一种，并尽量保留原有的属性（颜色、材质、射线检测开关、是否可遮罩等），同时保持组件在 `GameObject` 上的顺序：

- `Button` ⇄ `EnhancedButton`
- `Image`、`RawImage`、`Clear`、`Block`、`Circle`、`Annulus`、`RoundedRectangle`、`RoundedRectangleBorder`、`CustomGraphic` 之间可以两两互转
- `HorizontalLayoutGroup` ⇄ `VerticalLayoutGroup`
- `Clear` 上还有 `Delete Useless Properties`，用来删掉透明图形上那些没有意义的属性

### Aurora Unity 菜单

`Aurora Unity` 菜单（在 Unity 主菜单栏上）收集了日常开发中用得上的小工具，每一项都可以用 `UnityEditorUtility.MenuItems` 里的名字定位：

| 菜单项                                  | 说明                                                                                                 |
|-----------------------------------------|------------------------------------------------------------------------------------------------------|
| `Initialize`                            | 手动重新执行一次初始化流程                                                                           |
| `Allow Unsafe Code`                     | 切换 `PlayerSettings.allowUnsafeCode`（菜单项会显示当前勾选状态）                                    |
| `Clear Log Entries`                     | 清空 Console 窗口里的日志                                                                            |
| `Request Script Compilation`            | 向 Unity 引擎请求一次脚本编译                                                                        |
| `Layout/Ping Layout Root`               | 定位选中 `RectTransform` 所在的布局根节点                                                            |
| `Layout/Mark Layout For Rebuild`        | 把选中的 `RectTransform` 标记为需要重建布局                                                          |
| `Layout/Force Rebuild Layout Immediate` | 立刻强制重建选中 `RectTransform` 的布局                                                              |
| `Log Graphic Raycast Target`            | 打印选中 `Graphic` 的 `raycastTarget` 值                                                             |
| `Log RectTransform`                     | 打印选中 `RectTransform` 的关键信息（锚点、枢轴、位置、尺寸等）                                      |
| `Optimize Object Name`                  | 把选中资源/对象的名字规范化                                                                          |
| `Clipboard/Convert Path to GUID`        | 把剪贴板里的资源路径换成 GUID                                                                        |
| `Clipboard/Ping Path`                   | 定位剪贴板里的资源路径                                                                               |
| `Clipboard/Convert GUID to Path`        | 把剪贴板里的 GUID 换成资源路径                                                                       |
| `Clipboard/Ping GUID`                   | 定位剪贴板里的 GUID 对应的资源                                                                       |
| `Capture Screenshot to Desktop`         | 截屏并保存到桌面                                                                                     |
| `Open Persistent Data Path`             | 在文件管理器里打开 `Application.persistentDataPath`                                                  |
| `Validate View Prefabs`                 | 扫描工程里所有预制体，找出"界面处于激活且启用状态"的预制体（界面系统要求它们必须是未激活或已禁用的） |

### DefineSymbolScope

`DefineSymbolScope` 把"批量增删预处理符号"包装成 `IDisposable`：作用域内只操作内存中的符号列表，离开作用域时再一次性写入构建目标组。

```csharp
using (var scope = new DefineSymbolScope())
{
    scope.Add("MY_SYMBOL");
    scope.Remove("OTHER_SYMBOL");
    var isDefined = scope.IsDefined("MY_SYMBOL");
} // 离开作用域时，如果符号有改动，会把当前的符号列表真正写入构建目标组
```

默认作用于当前选中的构建目标组，也可以显式传入 `BuildTargetGroup`。

### ReorderableListHelper

`ReorderableListHelper` 提供使用 `UnityEditorInternal.ReorderableList` 时需要的常量与辅助方法，用来把元素高度、行距、嵌套列表的页脚高度算对。

```csharp
var list = new ReorderableList(serializedObject, serializedProperty);
list.elementHeightCallback = index => ReorderableListHelper.GetElementHeight(lineCount: 2);
list.drawElementCallback = (rect, index, isActive, isFocused) =>
{
    ReorderableListHelper.InitializeY(ref rect);
    ReorderableListHelper.SetSingleLineHeight(ref rect);
    // 画第一行
    ReorderableListHelper.NextLine(ref rect);
    // 画第二行
};
```

### ReorderableListWithState

`ReorderableListWithState` 是 `ReorderableList` 的派生类，在原有回调上多带一个用户自定义状态对象，避免为了给回调传状态而写一堆闭包。

```csharp
var list = new ReorderableListWithState(serializedObject, serializedProperty, state: myState);
list.drawElementCallback = (rect, index, isActive, isFocused, state) =>
{
    // ...
};
var state = list.State;
```

### UnityEditorUtility

`UnityEditorUtility` 提供编辑器侧的通用能力：

```csharp
var isChildrenIncluded = UnityEditorUtility.IsChildrenIncluded(property);
UnityEditorUtility.ThrowIfSymbolInvalid("MY_SYMBOL");
var isValid = UnityEditorUtility.IsGuidValid("0123456789abcdef0123456789abcdef");

var projectPath = UnityEditorUtility.ProjectPath;
var consoleWindowType = UnityEditorUtility.EditorWindowTypes.Console; // 还有 Game / Hierarchy / Inspector / Project / Scene
```

### UnityEditorGUIUtility

`UnityEditorGUIUtility` 提供 IMGUI 绘制辅助：`DrawOuterBorder`、`DrawInnerBorder` 与把网格状区域一次画完的 `DrawCellsArea`（配合 `DrawCellsAreaOptions` 使用）。

`DrawCellsAreaOptions` 描述网格区域的绘制方式：行下标从下往上还是从上往下（`CellRowOrigin.Bottom` / `Top`）、背景与单元格怎么画、增删行列按钮怎么画、轴标签与下标标签的样式与偏移。

```csharp
var options = new DrawCellsAreaOptions
{
    RowOrigin = CellRowOrigin.Top,
    OnDrawCell = (cellRect, cellPosition, state) => EditorGUI.DrawRect(cellRect, Color.gray),
    OnDrawBackground = (rect, state) => EditorGUI.DrawRect(rect, Color.black),
    OnDrawAddColumnButton = (rect, columnIndex, state) =>
    {
        // ...
    },
    OnDrawDeleteColumnButton = (rect, columnIndex, state) =>
    {
        // ...
    },
    OnDrawAddRowButton = (rect, rowIndex, state) =>
    {
        // ...
    },
    OnDrawDeleteRowButton = (rect, rowIndex, state) =>
    {
        // ...
    }
};

UnityEditorGUIUtility.DrawCellsArea(
    padding: Vector4.zero,
    cellsAreaPadding: Vector4.zero,
    cellDimensions: new Vector2Int(4, 4),
    cellSize: new Vector2(32, 32),
    cellSpacing: new Vector2(2, 2),
    options: options,
    state: null
);
```
