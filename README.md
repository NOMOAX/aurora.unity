# Aurora Unity

![license](https://img.shields.io/github/license/NOMOAX/aurora.unity)
![version](https://img.shields.io/badge/version-2.1.0-blue)
![lowest Unity version](https://img.shields.io/badge/Unity-2021.2%2B-blue)

High-performance, low-memory-consumption toolkit for Unity.

English | [中文](README.zh.md)

## Dependencies

- [Aurora](https://github.com/NOMOAX/aurora.git)
- Unity UI (`com.unity.ugui`)

## Installation

1. Open Unity package manager.
2. Click the `+` button in the upper-left corner, then select `Add package from git URL...`.
3. Input `https://github.com/NOMOAX/aurora.unity.git` and then click the `Add` button.

## Runtime Environment and Initialization

This package requires no manual initialization. When the program starts, a series of initialization steps is performed automatically, and the process is printed to the Unity console. These steps include:

- switching `Log.Logger` to `UnityConsoleLogger.Instance` so that logs are written to the Unity console;
- recording the Unity main thread ID, the `SynchronizationContext`, and its `TaskScheduler`;
- caching the values of the "Default", "Ignore Raycast", and "UI" layers;
- initializing `PlayerLoopUtility`;
- creating `UnityEnvironment.InactiveContainer`;
- in a player (non-editor) environment, additionally creating the quit listener object.

In the editor environment, entering and exiting play mode additionally handles the cleanup of `PlayerLoopUtility`, view containers, and the cursor stack.

### UnityEnvironment

`UnityEnvironment` provides values and operations related to the Unity runtime environment.

```csharp
// Whether the program is running (in the editor, whether it is in play mode)
if (UnityEnvironment.IsPlaying)
{
    // ...
}

// Whether the current thread is the Unity main thread
if (!UnityEnvironment.OnUnityMainThread)
{
    // ...
}

// The main thread ID, Unity's synchronization context, and its task scheduler
var mainThreadId = UnityEnvironment.UnityMainThreadId;
var synchronizationContext = UnityEnvironment.UnitySynchronizationContext;
var taskScheduler = UnityEnvironment.UnitySynchronizationContextTaskScheduler;

// A cancellation token that is cancelled when the program ends (in the editor, when play mode is exited)
await Task.Delay(1000, UnityEnvironment.ExitToken);

// Whether the editor is in the dark skin environment
var isProSkin = UnityEnvironment.IsProSkin;

// The values of three fixed layers (not customizable)
var defaultLayer = UnityEnvironment.DefaultLayer;
var ignoreRaycastLayer = UnityEnvironment.IgnoreRaycastLayer;
var uiLayer = UnityEnvironment.UILayer;

// Clipboard
var clipboard = UnityEnvironment.Clipboard;
UnityEnvironment.Clipboard = "hello";

// Screen size and aspect ratio
var screenSize = UnityEnvironment.ScreenSize;
var aspectRatio = UnityEnvironment.ScreenAspectRatio;

// Quit the program (in the editor, exit play mode)
UnityEnvironment.QuitApplication();
UnityEnvironment.QuitApplication(0);

// Register an instance to be disposed when the program ends; multiple instances are disposed in last-in-first-out order
UnityEnvironment.DisposeOnApplicationQuit(disposable);
```

`InactiveContainer` is a `Transform` that stays inactive throughout play mode: parenting the object to be instantiated under it prevents `MonoBehaviour.OnEnable` from being executed immediately during instantiation, so initialization code can safely be written in `OnEnable`. Do not set it active at runtime.

```csharp
var instance = Instantiate(prefab, UnityEnvironment.InactiveContainer, false); // OnEnable has not run yet
// TODO: initialize instance
instance.transform.SetParent(realParent, false);
instance.SetActive(true); // OnEnable runs here
```

`ExitToken` is created when the program starts and is cancelled when the program ends; in the editor environment, an already cancelled token is returned when not in play mode. Callbacks registered on it should not throw exceptions (a thrown exception is logged as an error when the program ends).

### SingletonBehaviour\<T\>

`SingletonBehaviour<T>` is the base class for singleton `MonoBehaviour`s, where `T` is the type of the class itself.

```csharp
[DoNotDestroyOnLoad]
[WithHideFlags(HideFlags.HideAndDontSave)]
public sealed class GameManager : SingletonBehaviour<GameManager>
{
}
```

`Instance` is assigned when the instance's `Awake` runs. For an instance that is inactive or disabled in the scene, `Awake` may not have run yet, in which case `Instance` is still `null`. You can explicitly call `FindInstance` to look it up in the scene (inactive objects are searched as well), or call `CreateInstance` to create a new `GameObject`.

```csharp
GameManager.FindInstance(); // Finds it in the scene and assigns it; does nothing when already assigned, logs a warning when not found
GameManager.CreateInstance(); // Creates a new GameObject named "GameManager" and assigns it

var gameManager = GameManager.Instance;
```

If a singleton instance already exists, and that instance was neither found by `FindInstance` nor created by `CreateInstance`, then the appearance of a second instance is treated as a program error and throws `InvalidOperationException`.

- `[DoNotDestroyOnLoad]`: calls `Object.DontDestroyOnLoad` on the instance when it is assigned.
- `[WithHideFlags(HideFlags)]`: performs a bitwise `OR` of the specified `HideFlags` with `hideFlags` when the instance is assigned.

### Cancellation Tokens Bound to the Active State

`GameObjectExtensions.GetDisableToken` returns a cancellation token bound to the active state of a `GameObject`: the token is cancelled when the object becomes inactive.

```csharp
var disableToken = gameObject.GetDisableToken();
await SomeLongRunningOperationAsync(disableToken); // Cancelled automatically when the object becomes inactive
```

## Player Loop

The Unity player loop consists of a series of phases executed in a fixed order, and each phase consists of several subsystems. Unity allows custom subsystems to be inserted at any position, and `PlayerLoopUtility` is built on this: it inserts multiple custom code execution points before and after the script callbacks of each phase, so that these pieces of code are invoked at the specified position in every frame.

### PlayerLoopPhase

`PlayerLoopPhase` represents a phase in the Unity player loop. There are 8 of them:

| Value           | Meaning                                                                                      |
|-----------------|----------------------------------------------------------------------------------------------|
| `FixedUpdating` | Before `FixedUpdate.ScriptRunBehaviourFixedUpdate`                                           |
| `FixedUpdated`  | After `FixedUpdate.ScriptRunBehaviourFixedUpdate`                                            |
| `Updating`      | Before `Update.ScriptRunBehaviourUpdate`                                                     |
| `Updated`       | After `Update.ScriptRunBehaviourUpdate` and before `Update.ScriptRunDelayedDynamicFrameRate` |
| `UpdateYielded` | After `Update.ScriptRunDelayedDynamicFrameRate` and before `Update.ScriptRunDelayedTasks`    |
| `UpdatePosted`  | After `Update.ScriptRunDelayedTasks`                                                         |
| `LateUpdating`  | Before `PreLateUpdate.ScriptRunBehaviourLateUpdate`                                          |
| `LateUpdated`   | After `PreLateUpdate.ScriptRunBehaviourLateUpdate`                                           |

### IPlayerLoopItem and PlayerLoopUtility

An object implementing `IPlayerLoopItem` can be executed once per frame at a specified phase.

```csharp
public sealed class MyPlayerLoopItem : IPlayerLoopItem
{
    void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
    {
        // Executed once per frame
    }
}
```

`PlayerLoopUtility` registers, unregisters, and queries these objects.

```csharp
var item = new MyPlayerLoopItem();
PlayerLoopUtility.AddPlayerLoopItem(item, PlayerLoopPhase.Updated);
PlayerLoopUtility.RemovePlayerLoopItem(item, PlayerLoopPhase.Updated);

var currentPhase = PlayerLoopUtility.CurrentPhase; // The phase being executed, or null when not inside any phase
```

Implementing `IPlayerLoopItem` yourself has many benefits: it can replace `Update` — a large number of `MonoBehaviour.Update` calls add call overhead and make execution time longer; logic can also be hooked to phases other than `Update`. Types in this package such as `PlayerLoopScope`, the various timers and counters, `ScrollView`, and `UnityMainThreadTaskScheduler` all implement it.

Besides implementing the interface, a delegate can be registered directly as a "continuation", which is executed only once. The overload taking a state parameter avoids a closure allocation.

```csharp
PlayerLoopUtility.AddContinuation(
    () => Debug.Log("Executed at the next PlayerLoopPhase.Updated"),
    PlayerLoopPhase.Updated
);

PlayerLoopUtility.AddContinuation(
    state => Debug.Log(state),
    "hello",
    PlayerLoopPhase.Updated
);
```

### PlayerLoopScope

`PlayerLoopScope` wraps "execute every frame → stop executing" as an `IDisposable` and uses a `using` statement to manage the lifetime. Besides passing a delegate directly, the constructor overload taking an `object` state can be used to pass the objects to be accessed in one go, avoiding a closure allocation.

```csharp
// A loading progress bar: the task takes some time, and the progress is refreshed every frame during it

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
            // TODO: perform the task that takes some time here, for example downloading and loading assets
        }
    }
}
```

### A Finite State Machine Updated Every Frame

`UnityUpdateStateMachine<T>` is a derived class of `StateMachine<T>` (from the Aurora package). Besides enter and exit callbacks, a state can implement `IUnityUpdateState<T>`, so that custom logic is executed once per frame while that state is current.

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
        // Executed once per frame while this is the current state
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
    // Call Update repeatedly first until it returns false, executing every pending state transition
    while (_stateMachine.Update())
    {
    }
    // Then call UnityUpdate once to execute the per-frame logic of the current state
    _stateMachine.UnityUpdate();
}
```

`CurrentUnityUpdateState` is the result of converting `CurrentState` to `IUnityUpdateState<T>`; it is `null` when the current state does not implement that interface.

`DoUnityUpdate` is a virtual method that can be overridden to insert custom logic before or after the current state's `OnUnityUpdate`.

```csharp
protected override void DoUnityUpdate(IUnityUpdateState<Type> currentUnityUpdateState)
{
    // Custom logic executed before OnUnityUpdate
    base.DoUnityUpdate(currentUnityUpdateState);
    // Custom logic executed after OnUnityUpdate
}
```

## Awaitables

These structs implement the C# awaitable pattern and can be awaited directly, for waiting on Unity's asynchronous operations inside `async` methods. They all provide overloads that accept a `CancellationToken`.

```csharp
// Wait for any AsyncOperation
await new AsyncOperationAwaitable(SceneManager.LoadSceneAsync("Level2"));

// Wait for a Resources load
var texture = await new ResourceRequestAwaitable<Texture2D>(Resources.LoadAsync<Texture2D>("Textures/Tree"));
var asset = await new ResourceRequestAwaitable(Resources.LoadAsync("Prefabs/Tree"));

// Wait for AssetBundle creation and loading
var assetBundle = await new AssetBundleCreateRequestAwaitable(AssetBundle.LoadFromFileAsync(path));
var prefab = await new AssetBundleRequestAwaitable<GameObject>(assetBundle.LoadAssetAsync<GameObject>("Tree"));
var allObjects = await new AssetBundleRequestAwaitable.All(assetBundle.LoadAllAssetsAsync());
var allTypedObjects = await new AssetBundleRequestAwaitable<GameObject>.All(assetBundle.LoadAllAssetsAsync<GameObject>());
```

`DelayFrameAwaitable` continues after a number of frames, and the frame count is counted at the specified phase, so it also waits for that phase to arrive. `PlayerLoopPhaseAwaitable` waits for the next occurrence of the specified phase, and its nested `Any` struct continues at whichever of the passed phases is executed first.

```csharp
// Wait for 1 frame, and wait until the Updated phase
await new DelayFrameAwaitable(1, PlayerLoopPhase.Updated);

// Wait for 2 frames, and wait until the Updated phase; can be cancelled
await new DelayFrameAwaitable(2, PlayerLoopPhase.Updated, cancellationToken);

// Wait for the next Updated phase
await new PlayerLoopPhaseAwaitable(PlayerLoopPhase.Updated);

// Continue at whichever of Update and LateUpdate comes first
await new PlayerLoopPhaseAwaitable.Any(new[] { PlayerLoopPhase.Updating, PlayerLoopPhase.LateUpdating });
```

## Tasks

### UnityTasks

`UnityTasks` provides methods that return a `Task` and are equivalent to the awaitables above, for cases where the task needs to be stored, composed, or awaited.

```csharp
// Wait for the specified player loop phase
await UnityTasks.WhenPlayerLoopPhase(PlayerLoopPhase.Updated);

// Wait for whichever of multiple player loop phases comes first
await UnityTasks.WhenAnyPlayerLoopPhase(new[] { PlayerLoopPhase.Updating, PlayerLoopPhase.LateUpdating });

// Wait for a Unity asynchronous operation to complete
await UnityTasks.WhenAsyncOperation(SceneManager.LoadSceneAsync("Level2"));

// Delay (timed with a Stopwatch)
await UnityTasks.Delay(TimeSpan.FromSeconds(1), PlayerLoopPhase.Updating);

// Delay (timed with Time.time when unscaled is false, with Time.unscaledTime when true)
await UnityTasks.DelayUnityTime(TimeSpan.FromSeconds(1), true, PlayerLoopPhase.Updating);

// Delay for a number of frames; it never completes when frameCount is -1 (it can only be ended by cancellation), which is useful for waiting forever
await UnityTasks.DelayFrame(1, PlayerLoopPhase.Updated);
await UnityTasks.DelayFrame(-1, PlayerLoopPhase.Updated);

// Wait for a screenshot to complete (a PNG file)
// Note: this feature is not tested on all platforms and may be unstable
await UnityTasks.WhenScreenshotCaptured(@"D:\screenshot.png");
```

All methods provide overloads that accept a `CancellationToken`.

### UnityMainThreadTaskScheduler

`UnityMainThreadTaskScheduler` is a `TaskScheduler` that schedules tasks onto the Unity main thread. It executes queued tasks every frame at the `PlayerLoopPhase.UpdateYielded` phase.

To keep the Unity main thread from deadlocking, it rejects tasks with `TaskCreationOptions.LongRunning` (and logs an error).

`BeginProcess` and `Continue` are `protected virtual` properties that control "whether a new round of processing may begin" and "whether the next task may be processed". They can be overridden in a derived class to implement a custom scheduling policy (for example, processing only a fixed number of tasks per frame).

## Timers and Counters

### Timer

`ITimer` is the timer interface. `Change` updates both the waiting time before the first trigger and the interval between subsequent triggers. `dueTime` and `period` have the same meaning as in `System.Threading.Timer`.

There are three implementations, which differ in what they use to measure time. All three check whether the time is up at the specified player loop phase:

- `UnityTimePlayerLoopTimer`: uses `Time.time`
- `UnityUnscaledTimePlayerLoopTimer`: uses `Time.unscaledTime`
- `StopwatchPlayerLoopTimer`: uses `Stopwatch`

```csharp
// The timer may be modified, disabled, or disposed inside the callback
var callback = (TimerTriggerCallback)((timer, state) => Debug.Log($"triggered by {state}"));

// Trigger once after 1 second
var timer = new UnityUnscaledTimePlayerLoopTimer(
    callback,
    "my timer",
    TimeSpan.FromSeconds(1), // Resets the timer, then triggers for the first time 1 second later
    Timeout.InfiniteTimeSpan, // Disables the timer after the first trigger
    PlayerLoopPhase.Updated
);

// It can also be created first and configured later with Change
var timer2 = new UnityUnscaledTimePlayerLoopTimer(callback, "my timer", PlayerLoopPhase.Updated);
timer2.Change(
    TimeSpan.Zero, // Resets the timer, then triggers for the first time immediately
    TimeSpan.FromSeconds(1) // Triggers again every 1 second after the first trigger
);
timer2.Change(
    Timeout.InfiniteTimeSpan, // Disables the timer
    Timeout.InfiniteTimeSpan // Disables the timer after the first trigger (since the timer is disabled, this argument is not actually used)
);

timer.Dispose();
timer2.Dispose(); // Remember to dispose it when it is no longer used
```

`UnityUtility.CancelAfter` is built on `StopwatchPlayerLoopTimer`: it cancels a `CancellationTokenSource` after the specified waiting time, and disposing the returned object terminates the cancellation.

```csharp
using (UnityUtility.CancelAfter(cancellationTokenSource, TimeSpan.FromSeconds(3)))
{
    // Within these 3 seconds, if other logic cancels cancellationTokenSource, this cancellation no longer happens
}
```

### Counter

`ICounter` is the counter interface. The parameters of `Change` correspond one-to-one to those of `ITimer.Change`, except that the unit is frames rather than time: `dueCount` of -1 disables it, 0 triggers immediately, and a value greater than 0 triggers after that number of frames; `period` of -1 disables it after the first trigger.

`UnityFrameCountPlayerLoopCounter` counts with `Time.frameCount` and checks at the specified player loop phase.

```csharp
// The counter may be modified, disabled, or disposed inside the callback
var callback = (CounterTriggerCallback)((counter, state) => Debug.Log($"triggered by {state}"));

// Trigger once after 3 frames
var counter = new UnityFrameCountPlayerLoopCounter(
    callback,
    "my counter",
    3, // Resets the counter, then triggers for the first time 3 frames later
    -1, // Does not trigger again after the first trigger
    PlayerLoopPhase.Updated
);

// It can also be created first and configured later
var counter2 = new UnityFrameCountPlayerLoopCounter(callback, "my counter", PlayerLoopPhase.Updated);
counter2.Change(
    0, // Resets the counter, then triggers for the first time immediately
    3 // Triggers again every 3 frames after the first trigger
);
counter2.Change(
    -1, // Disables the counter
    -1 // Disables the counter after the first trigger (since the counter is disabled, this argument is not actually used)
);

counter.Dispose();
counter2.Dispose();
```

### FromToTimer

`IFromToTimer` represents a timer that counts from a start point to an end point, suitable for progress bars, countdowns, numeric animations and the like. `PlayerLoopFromToTimer` is its implementation, advancing the current time with `Time.deltaTime` or `Time.unscaledDeltaTime` at the specified player loop phase.

```csharp
// Count from 0 to 100, using scaled time
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

timer.Running = true; // Starts counting; when To is reached it stops automatically and raises Completed

// The progress or the current time can also be set directly, which likewise raises the corresponding events
timer.Progress = 0.5;
timer.Time = 30;

timer.Running = false;
timer.Dispose();
```

- `From`: the start point.
- `To`: the end point. `double.PositiveInfinity` or `double.NegativeInfinity` are allowed, in which case the timer never ends.
- `Time`: the current time; assigning it clamps the value between `From` and `To`.
- `TimeTruncated`: the integer value of the current time with the fractional part removed.
- `Progress`: the progress, in the range `[0, 1]`; it is always 1 when `From` equals `To`.
- `Running`: whether it is counting. Setting it to `true` registers the timer on the player loop, and setting it to `false` unregisters it.
- `UseUnscaledTime`: whether unscaled or scaled time is used to advance the time.
- The event arguments of `TimeChanged` / `TimeTruncatedChanged` / `ProgressChanged` are `FromToTimerValueChangedEventArgs`, which contains `PreviousValue`, `NewValue`, and a `Causation` indicating the reason for the change: `Timing` means it was caused by timing, `Modification` means it was caused by an external assignment.
- `Completed` is raised when the timer reaches `To`.

## Graphics

These components all derive directly from `MaskableGraphic` and generate their mesh on the fly in `OnPopulateMesh`, so a shape is available without any image asset; they also implement `ILayoutElement`, so they can serve as layout elements in places like `Image` does.

| Component                | Description                                              |
|--------------------------|----------------------------------------------------------|
| `Block`                  | A solid color block                                      |
| `Clear`                  | Transparent (draws nothing, but takes part in UI events) |
| `Circle`                 | Circle                                                   |
| `Annulus`                | Annulus                                                  |
| `RoundedRectangle`       | Rounded rectangle                                        |
| `RoundedRectangleBorder` | Rounded rectangle border                                 |
| `CustomGraphic`          | A graphic whose vertices and triangles are user-defined  |

Besides the color (the `color` inherited from `Graphic`), these graphics share the following properties:

- `Texture`: the texture. Once set, the UVs of the vertices are computed from the normalized position of the shape inside its own rectangle, so the texture is "clipped" into the shape; when not set, the UVs are meaningless and the shape is colored with `color` alone.
- `Segments`: the number of subdivisions of the arc. More segments are smoother and produce more vertices.
- `UseExactRaycastLocation`: whether to use an exact click area. The default `false` determines clicks with the rectangle of the shape; setting it to `true` determines them with the polygon "point in polygon" algorithm, so the four corners of a circle are no longer misjudged, at the cost of iterating over the polygon vertices on every hit test.

A rounded rectangle additionally has a radius for each of its four corners, and each corner has a "use a normalized length" toggle and a radius value.

On top of that, `RoundedRectangleBorder` has `ThicknessNormalized` and `Thickness`, which describe the thickness of the border.

`CustomGraphic` describes the whole mesh with two lists: `Vertices` holds the normalized position and color of each vertex, and `Triangles` holds vertex indices in groups of three in clockwise order. Since these two lists are `List<T>` fields, `SetVerticesDirty` must be called after modifying them (or a redraw has to be triggered through the Inspector) for the changes to take effect. `NormalizedPositionAndColor` is a "normalized position plus color" pair.

## Controls

### EnhancedButton

`EnhancedButton` is a button control. It does not inherit `Button`, but is an implementation rewritten from `UIBehaviour`, so it is not constrained by `Selectable` and offers more than `Button`: it has built-in "toggle" capability, and it can color a `Graphic`'s `color` according to its state directly.

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

        // Raises the Updated event once manually to refresh the color of buttonBackgroundGraphic
        // (developers do not need to set the color of buttonBackgroundGraphic on the prefab)
        button.Refresh();
    }

    private void OnDisable()
    {
        button.Clicked -= OnButtonClicked;
        button.Updated -= OnButtonUpdated;
    }
}
```

`State` (`EnhancedButtonState`) is determined by the pointer position and the pressed state: `Default` when the pointer is outside the button, `Hovered` when it is inside, and `Pressed` when it is pressed inside the button and a click can still be triggered.

When the four events are raised:

| Event           | When it is raised                                                                                                                                  |
|-----------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| `Clicked`       | On a left click (delayed until the double click interval has passed when `doubleClick` is `true`), or on a right click when `rightClick` is `true` |
| `DoubleClicked` | On a double click (only when `doubleClick` is `true`)                                                                                              |
| `Toggled`       | When `IsOn` changes (`SetIsOnWithoutNotify` does not raise it)                                                                                     |
| `Updated`       | When the state, the toggle state, or the interactable state changes; it can also be raised manually with `Refresh`                                 |

After the button is pressed, the click is cancelled as soon as the pointer leaves the button (`eventData.eligibleForClick` is set to `false`).

`ColorBlock` describes the colors for six states, and `GetColor(button)` picks the corresponding color from the current state of the button and whether it is interactable.

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

The colors in `ColorBlock` are determined only by `State` and `Interactable`, and are unrelated to `IsOn`. If different colors are needed for the On and Off states of a button, use two `ColorBlock`s and pick between them according to `IsOn`:

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

`EnhancedButtonGroup` is a button group: buttons are registered with it through `EnhancedButton.Group`, and it coordinates cases such as title bars, toolbars, and radio button groups, where "at most one button is on at a time". The group does not require the buttons to be on the same hierarchy level; the registration is decided entirely by references.

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
            // The user clicked this button, which changed its IsOn from true to false;
            // but since this is a single-selection button group, one button must always stay on,
            // so it is set back to on immediately here
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

Note that only buttons that are active and enabled (`isActiveAndEnabled`) are present in the button group.

### EnhancedSlider

`EnhancedSlider` is a slider whose value is a `float` in `[0, 1]`.

```csharp
var slider = gameObject.AddComponent<EnhancedSlider>();

slider.Fill = fillRectTransform; // The fill part
slider.Handle = handleRectTransform; // The handle
slider.Direction = Slider.Direction.LeftToRight;
slider.Interactable = true;

slider.Value = 0.5f; // Assigning clamps it to [0, 1] and raises ValueChanged
slider.SetValueWithoutNotify(0.5f); // Assigns without raising ValueChanged

slider.ValueChanged += (s, value, oldValue) => Debug.Log($"{oldValue} -> {value}");
slider.OperationBeginning += s => Debug.Log("operation beginning");
slider.OperationEnded += s => Debug.Log("operation ended");

var isOperating = slider.IsOperating; // Whether it is being operated
```

`OperationBeginning` and `OperationEnded` always come in pairs, which can be used for logic such as "commit the value only when the drag ends".

## ScrollView

`ScrollView` would have belonged in the "Controls" section, but it is too large, so it has a section of its own.

`ScrollView` is a scrolling list that heavily reuses child objects, replacing the common approach built on `UnityEngine.UI.ScrollRect`: when there is a large amount of data, only the few items that are visible are instantiated, and items that scroll out of view are recycled. It is built on top of `ScrollRect` and uses its own `ScrollRect` component.

`ScrollView` is an abstract base class; in practice `HorizontalScrollView` or `VerticalScrollView` is used.

### IScrollViewController

`ScrollView` itself holds no data; all data is provided through a controller: implement `IScrollViewController` and hand it to the `ScrollView`.

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
        // Generally, items of different types (System.Type) have different sizes,
        // but if you want, items of the same type may have different sizes as well,
        // as long as the size corresponding to each index is fixed
        return 100;
    }

    public ScrollViewItem GetItem(ScrollView scrollView, int index, out bool isNewCreated)
    {
        // Takes out an item: a recycled one is reused when available, otherwise the prefab is instantiated
        // The actual data refresh is done by the item's own OnGet
        var item = scrollView.GetRecycledOrCreateNewItem(_itemPrefab, out isNewCreated);
        item.Initialize(_data[index]);
        return item;
    }

#if UNITY_EDITOR
    public string GetItemName(ScrollViewItem item)
    {
        // Only used in the editor to name an item for easier debugging; returning null or throwing is also fine when it is not needed
        return "MyScrollViewItem";
    }
#endif
}
```

Calling `GetRecycledOrCreateNewItem(itemPrefab, out isNewCreated)` inside `GetItem` is mandatory: it looks for a recycled item by `ScrollViewItem.identifier`, and only instantiates the passed prefab when none is found. Usually `GetItem` does only this and then hands the item's data to it; the distinction between "initialize once when created" and "refresh on every use" is left to the `OnGet(bool isNewCreated)` override of the `ScrollViewItem` subclass (see the next section).

`GetItemName` is compiled only in the editor environment, and is used only to give the item a readable name in the Hierarchy window.

### ScrollViewItem

`ScrollViewItem` is the base class of a list item, attached to the item prefab.

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
        // Refresh the data here when the item is taken out for use
        _text.text = _textContent;
    }

    protected override void OnVisible()
    {
        // When the item enters the viewport
    }

    protected override void OnInvisible()
    {
        // When the item leaves the viewport
    }

    protected override void OnReturn(bool isScrollViewBeingDestroyed)
    {
        // When the item is recycled; isScrollViewBeingDestroyed being true means the ScrollView is being destroyed
    }
}
```

- `identifier`: distinguishes different types of `ScrollViewItem`; assign it in the editor and do not change it at runtime. Recycling matches by `identifier`.
- `ScrollView`: the scrolling list it belongs to.
- `Index`: the current index in the list; -1 when it is not in use.
- `Visible`: whether it has entered the viewport.

`OnGet` and `OnReturn` come in pairs, and so do `OnVisible` and `OnInvisible`. Their relationship can be understood like this: an item has not yet entered the viewport when it is taken out for use, and `OnVisible` is raised only once it scrolls into the viewport; when it scrolls out of the viewport `OnInvisible` is raised first, and `OnReturn` is raised only when it is recycled.

### Attaching and Refreshing

```csharp
var scrollView = gameObject.AddComponent<VerticalScrollView>();

scrollView.SetControllerAndReload(new MyScrollViewController(itemPrefab));

// Reload after the data changes; the three overloads keep the current position, specify a content position,
// and specify a normalized scroll position respectively
scrollView.Reload();
scrollView.ReloadWithContentPosition(0);
scrollView.ReloadWithNormalizedScrollPosition(0.5);
```

`Reload` clears all current items, asks the controller again for the item count and the size of each item, and then lays them out again. It should be called once immediately after the data changes.

`Refresh`, on the other hand, "keeps the data unchanged and only recomputes which items should exist at the current position". It is driven automatically by `ScrollRect.onValueChanged`; if the latest active items are needed immediately after changing `ContentPosition` or `NormalizedScrollPosition` manually, `Refresh` can be called once by hand.

```csharp
scrollView.Refresh();
```

### Position, Size, and Index

```csharp
var itemCount = scrollView.ItemCount;
var viewportSize = scrollView.ViewportSize; // The viewport size
var contentSize = scrollView.ContentSize; // The content size
var overflowedContentSize = scrollView.OverflowedContentSize; // The part of the content beyond the viewport, or 0 when it does not overflow

// Content position: the coordinate along the scrolling direction
var contentPosition = scrollView.ContentPosition;
scrollView.ContentPosition = 0;

// Normalized scroll position: usually in [0, 1], of type double, more precise than ScrollRect.normalizedPosition
var normalizedScrollPosition = scrollView.NormalizedScrollPosition;
scrollView.NormalizedScrollPosition = 0.5;

// The indices of active items (items that have been instantiated) and visible items (items actually inside the viewport)
var firstActiveIndex = scrollView.FirstActiveIndex; // -1 when there is no active item
var lastActiveIndex = scrollView.LastActiveIndex;
var firstVisibleIndex = scrollView.FirstVisibleIndex; // -1 when there is no visible item
var lastVisibleIndex = scrollView.LastVisibleIndex;

// Gets the item at an index; returns null when there is no active item at that index
var item = scrollView[3];

// Gets all active items of the specified type
var items = new List<MyScrollViewItem>();
scrollView.GetActiveItems(items);

// Gets the begin and end content positions of an item
var itemBeginPosition = scrollView.GetItemBeginPosition(3);
var itemEndPosition = scrollView.GetItemEndPosition(3);

// Finds an index by content position
var firstIndex = scrollView.FindFirstIndex(contentPosition); // The first item whose begin position is greater than or equal to this position, or -1 when not found
var lastIndex = scrollView.FindLastIndex(contentPosition); // The first item whose end position is greater than or equal to this position, or -1 when not found
var closestIndex = scrollView.FindClosestIndex(contentPosition); // The item closest to this position, or -1 when there are no items
```

Conversions between the four kinds of positions:

```csharp
// Content position <-> normalized viewport position (0 means the viewport start touches the content start, 1 means the viewport end touches the content end)
var normalizedViewportPosition = scrollView.ConvertContentPositionToNormalizedViewportPosition(contentPosition);
var contentPosition2 = scrollView.ConvertNormalizedViewportPositionToContentPosition(0.5f);

// Content position <-> normalized scroll position
var normalizedScrollPosition2 = scrollView.ConvertContentPositionToNormalizedScrollPosition(contentPosition);
var contentPosition3 = scrollView.ConvertNormalizedScrollPositionToContentPosition(0.5);
```

### Padding, Spacing, and Preloading

`Padding` (`RectOffset`) and `Spacing` are forwarded to the layout group on the content object and trigger a reload when set; `ChildForceExpandSize` decides whether items fill the content along the non-scrolling axis.

```csharp
scrollView.Padding = new RectOffset(8, 8, 8, 8);
scrollView.Spacing = 4;
scrollView.ChildForceExpandSize = true;
```

`leadingActiveOffset` and `trailingActiveOffset` are used for preloading: keeping some extra distance beyond the start and the end of the viewport lets items that are about to enter the viewport be created in advance, which avoids blank areas while scrolling. The values should be greater than or equal to 0, and take effect on the next refresh.

```csharp
scrollView.leadingActiveOffset = 100; // Items within 100 pixels before the viewport start stay active as well
scrollView.trailingActiveOffset = 100;
```

### Automatic Snapping

`ScrollView` has built-in automatic snapping: once the scroll speed drops, the item closest to a certain position is aligned to the middle of the viewport.

```csharp
scrollView.snapTrigger =
    ScrollViewSnapTrigger.OnEndDrag | ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged;

scrollView.snapSpeedThreshold = 300; // Snapping is triggered when the speed is below this value
scrollView.scrollSnapDelay = 0.3f; // How long after scrolling stops snapping is triggered (used for OnNormalizedScrollPositionChanged)

scrollView.snapFindNormalizedViewportPosition = 0.5f; // The normalized viewport position used as the reference when finding the closest item
scrollView.snapIncludingSpacing = false; // Whether the spacing around an item (padding/spacing) counts when computing its begin and end positions
scrollView.snapNormalizedItemPosition = 0.5f; // The weight interpolated between the begin and end positions of an item, used to compute the snap target position
scrollView.snapJumpNormalizedViewportPosition = 0.5f; // The normalized viewport position the target position is snapped to

scrollView.snapDurationMode = ScrollViewSnapDurationMode.Dynamic; // Whether the snap duration is fixed or computed from the distance and speed
scrollView.snapDuration = 0.25f; // The duration in Fixed mode
scrollView.snapSpeed = 900; // The speed in Dynamic mode
scrollView.snapInterpolation = Interpolation.OutCubic; // The interpolation type used during snapping

// Snapping can also be triggered manually
scrollView.Snap();

// Stops the snap animation in progress
scrollView.StopTween();
```

`ScrollViewSnapTrigger` is a `[Flags]` enum and can be combined:

| Value                               | When it triggers                                                                                                                                                                                                      |
|-------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `None`                              | Never snaps                                                                                                                                                                                                           |
| `OnEndDrag`                         | Snaps immediately when the drag ends                                                                                                                                                                                  |
| `OnNormalizedScrollPositionChanged` | Snaps when there is no drag, the scroll position changes, and the speed is below the threshold (useful for the case where the list is "flung" and then slows down by inertia)                                         |
| `OnPointerUpWithLowSpeed`           | Snaps when there is no drag, the pointer is released, and the `ScrollRect` speed is extremely low (useful for the case where a drag is held and then released after stopping, usually combined with the previous one) |

### Scrollbar and Speed Limit

```csharp
scrollView.ScrollbarVisibility = ScrollbarVisibility.OnlyIfNeeded; // Never / OnlyIfNeeded / Always
scrollView.speedLimit = 0; // Limits the drag speed when greater than 0

var isDragging = scrollView.Dragging; // Whether it is being dragged
var isTweening = scrollView.Tweening; // Whether a snap animation is playing
```

### Creating a ScrollView

Creating a `ScrollView` by hand is very tedious. The `GameObject/UI/Scroll View - Aurora Unity` menu opens the `Create New ScrollView` window, where after choosing the direction, the size, the scrollbar position and thickness, and whether to add a `LayoutElement`, a structurally complete `HorizontalScrollView` / `VerticalScrollView` is generated in one go (including the viewport, the content, and the scrollbar, with all the references wired up).

The window's `Create Another` button generates one and keeps the window open for the next one; `Create` generates one and closes the window.

## Layout

### FlowLayoutGroup

`FlowLayoutGroup` is a flow layout group, the "wrap when a line is full" version of `HorizontalLayoutGroup` / `VerticalLayoutGroup`: it first lays children out along the main axis, and automatically moves to the next line (or column) when a line (or column) is full.

```csharp
var flowLayoutGroup = gameObject.AddComponent<FlowLayoutGroup>();

flowLayoutGroup.Axis = RectTransform.Axis.Horizontal; // The main axis
flowLayoutGroup.Spacing = new Vector2(8, 8); // The spacing (x is the spacing within a line, y is the spacing between lines)
flowLayoutGroup.PreferredSizeAloneAxis = 0; // The preferred size along the main axis

var lineCount = flowLayoutGroup.LineCount; // The number of lines (or columns)

if (flowLayoutGroup.TryGetIndexOf(child, out var indexAloneAxis, out var indexAloneOtherAxis))
{
    // The index along the main axis (the position of the child within its line/column) and the index along the other axis (the line/column index)
    // Which one is the first dimension depends on Axis and the order of the children; verify it in practice when needed
}

var childrenOfLine = new List<RectTransform>();
flowLayoutGroup.GetLayoutChildrenOfLine(0, childrenOfLine); // Gets all children of line 0
```

In addition, `FlowLayoutGroup` accumulates the preferred sizes by the number of lines (columns), so it also produces the correct height when used together with a `ContentSizeFitter` on the outermost object.

### ScrollLayoutGroup

`ScrollLayoutGroup` is a scrolling layout group: it arranges children in a single row with a fixed spacing along the horizontal (or vertical) direction, and a floating-point index decides which child is in the middle — this is the core of interactions such as pickers, date selection, and card carousels.

```csharp
var scrollLayoutGroup = gameObject.AddComponent<ScrollLayoutGroup>();

scrollLayoutGroup.horizontal = true; // Whether to arrange along the horizontal direction

scrollLayoutGroup.CenterIndex = 3.5f; // The index in the middle (a float: 3.5 means "exactly between index 3 and index 4")
var centerIndex = scrollLayoutGroup.CenterIndex;

var currentCenter = scrollLayoutGroup.CurrentCenter; // The child currently in the middle (the index of CenterIndex rounded to the nearest integer)
var childCount = scrollLayoutGroup.LayoutChildrenCount; // The number of children taking part in the layout

scrollLayoutGroup.SetLayoutChildToCenter(child); // Moves the specified child to the middle
var index = scrollLayoutGroup.GetLayoutChildIndex(child); // Gets the index of the specified child among all children

var rectMask2D = scrollLayoutGroup.RectMask2D; // The RectMask2D on the same object
```

`CenterIndex` can be read (to get the current selection) and also written (to scroll to a certain entry; the float written in is the interpolation result of the animation).

## View System

The view system organizes views (`View`) into a tree: every view is a node, every view can have multiple child views, and the whole tree is attached under a view container (`ViewContainer`). The container places root views under an appropriate parent, and also answers queries such as "which one is on top".

A view is not "`new`ed by itself": it is created by a `ViewHandler`, and the created view is required to be inactive or disabled, so that it is activated only in the last step of the opening flow, and initialization code can safely be written in `OnEnable`.

### ViewHandler

`ViewHandler` is responsible for creating and releasing a category of views. It has two members that must be implemented and two that can be overridden:

```csharp
public sealed class GeneralViewHandler : ViewHandler
{
    // The type of view this handler is responsible for creating
    public override Type HandledViewType => typeof(View); // Can handle every type of view in your project

    // Creates a view that is "inactive or disabled"
    public override async Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Loads the prefab
        // Assume the name of the prefab is XxxView.Prefab (where XxxView is the name of the concrete view type)
        // Assume the path of the prefab is <Unity project path>/Assets/Resources/Prefabs/Views/XxxView.prefab
        var prefab = await new ResourceRequestAwaitable<GameObject>(
            Resources.LoadAsync<GameObject>($"Prefabs/Views/{typeof(T).Name}"),
            cancellationToken
        );

        // Instantiates it under InactiveContainer so that OnEnable does not run early
        var gameObject = Object.Instantiate(prefab, UnityEnvironment.InactiveContainer);

        // Hands it over to the view system
        return (T)gameObject.GetComponent<View>();
    }

    // Releases the view; the default implementation is Object.Destroy, and it can be overridden when a pool is needed
    public override void ReleaseView(View view)
    {
        base.ReleaseView(view);
    }
}
```

The handler has to be registered before the view is opened:

```csharp
ViewHandler.Register(new GeneralViewHandler());
```

`PrefabLessViewHandler` is already registered automatically by this package (see the "Runtime Environment and Initialization" section of this document); it creates `PrefabLessView`s that have no prefab.

`ViewHandler.Get<T>()` (or `Get(Type)`) picks the most suitable one from all registered handlers: it first filters for handlers whose `HandledViewType` is `T` itself or a base class of `T`, and then takes the one with the shortest inheritance chain; since the `HandledViewType` of `PrefabLessViewHandler` is `PrefabLessView`, views that implement only `PrefabLessView` land on it. It returns `null` when none is found.

### Opening a View

Opening a "root view" requires a view container:

```csharp
// View container: rectTransform becomes the parent of all root views under this container;
// it must be active and be under a Canvas
var container = View.AddContainer(containerRectTransform);

var view = await View.OpenAsync<MainMenuView>(container);
```

Opening a "child view" requires a parent view instead:

```csharp
// state is the user data passed in when the view is opened; it is assigned to view.State
var childView = await View.OpenAsync<ItemDetailView>(parentView, state: new ItemDetailViewState { Id = 42 });
```

Both ways of opening have a "do not wait" version, `BeginOpen`, whose return type is `void` and which runs as `async void` internally; it is suitable for cases where the result does not matter.

```csharp
View.BeginOpen<MainMenuView>(container);
View.BeginOpen<ItemDetailView>(parentView, state);
```

The last step of the opening flow activates and enables the view; before that, `OnSettingActiveAndEnabling` is called once, and it can be overridden in a derived class to do preparation work "before activation".

```csharp
protected override void OnSettingActiveAndEnabling()
{
    // Executed before the view is activated and enabled
}
```

If the view created by the handler is already active and enabled, opening it throws `BehaviourActiveAndEnabledException`, so **a view prefab must be inactive or disabled**. In the editor, this package provides the `Aurora Unity/Validate View Prefabs` menu, which checks in bulk whether every prefab in the project meets this requirement.

All ways of opening have overloads that accept a `CancellationToken`.

### View

`View` is the base class of all views. It also implements `IEnumerable<View>`, so direct child views can be iterated with `foreach`.

```csharp
// Tree structure
var container = view.Container;
var parent = view.Parent;
var root = view.Root;
var isRoot = view.IsRoot;
var isLeaf = view.IsLeaf;
var isChild = view.IsChildOf(otherView);
var isTopmost = view.IsTopmost();

// User data passed along with opening and closing
var state = view.State;
var closeState = view.CloseState;

// Child views
var child = view.GetChild<MainMenuView>();
var children = new List<MainMenuView>();
view.GetChildren(children);

// Traversal
foreach (View directChild in view)
{
}

foreach (View descendant in view.GetEnumerator(TreeEnumOrder.DepthFirstDlr))
{
}

// Searching starting from the current view
var found = view.GetViewFromThis<MainMenuView>(TreeEnumOrder.DepthFirstLdl);
var results = new List<MainMenuView>();
view.GetViewsFromThis(TreeEnumOrder.BreadthFirstLr, results);

// Close; closeState is assigned to CloseState, and child views are closed as well
view.Close();
view.Close(closeState: 42);
```

`Container` and `Parent` can also be assigned, which moves the view under another container or another parent view; all descendant nodes in the view tree are moved together, and after the move they are realigned to the four edges of the parent.

`childContainer` is "the parent of child views": when it is not set, child views are attached directly under the `RectTransform` of the current view.

`GetView<T>()` and `GetViews<T>` (static methods) search across the whole view system:

```csharp
var view = View.GetView<MainMenuView>(); // Equivalent to GetView<MainMenuView>(TreeEnumOrder.DepthFirstRld)
var view2 = View.GetView<MainMenuView>(TreeEnumOrder.BreadthFirstLr);

var allViews = new List<MainMenuView>();
View.GetViews(TreeEnumOrder.DepthFirstDlr, allViews);

// The topmost view
var topmost = View.GetTopmostView();
if (View.IsTopmost(view))
{
    // ...
}

// Containers
var containerCount = View.ContainerCount;
var container = View.GetContainer(0);
View.RemoveContainerAt(0); // Throws InvalidOperationException when the container still holds views
```

### ViewContainer

`ViewContainer` describes "where root views are attached".

```csharp
var container = View.AddContainer(containerRectTransform);

var rectTransform = container.RectTransform; // The parent of the root views under this container

var view = container.GetViewFromContainer<MainMenuView>(TreeEnumOrder.DepthFirstDlr);

var views = new List<MainMenuView>();
container.GetViewsFromContainer(TreeEnumOrder.BreadthFirstLr, views);
```

The container requires the passed `RectTransform` to be active and to have a `Canvas` among its ancestors, otherwise it throws `GameObjectInactiveException` or `ComponentNotGotException`.

### View.Scope\<T\>

`View.Scope<T>` wraps "open → use → close" as an `IDisposable`, so a `using` statement can express "this view exists only inside this scope". There are two typical uses:

- Showing a **loading view**: blocking the screen while a long loading task runs, and closing it automatically when the task is finished.
- Showing a **full-screen blocking view**: blocking the layers below while a **dialog view** is shown, so the user cannot interact with what is behind the dialog.

```csharp
using (new View.Scope<MainMenuView>(view))
{
    // ...
} // view is closed when the scope is left
```

`Scope<T>.View` retrieves the view managed by the scope.

### PrefabLessView and MaskView

`PrefabLessView` is the base class of views that "need no prefab and are created directly at runtime". It is created by `PrefabLessViewHandler` (already registered by this package), which sets `GameObject.layer` to the "UI" layer when creating the object.

`MaskView` is a ready-made masking view: a semi-transparent block that fills its parent, which can either close itself on click or execute a piece of logic. Its arguments are passed in through `State`.

```csharp
await View.OpenAsync<MaskView>(
    parentView,
    new MaskView.Args
    {
        MaskColor = new Color(0, 0, 0, 0.5f), // The mask color
        CloseOnClick = true, // Closes itself first when clicked
        InvocationOnClick = new InvocationAction(() => Debug.Log("mask clicked"))
    }
);

// A color can also be passed directly
await View.OpenAsync<MaskView>(parentView, new Color(0, 0, 0, 0.5f));

var maskGraphic = maskView.MaskGraphic; // The Graphic used for masking
```

### View Inspector

`Window/Aurora Unity/View Inspector` opens a window named `View Inspector` that lists all current view containers with hierarchical indentation, along with the view tree inside each container: the container level lists its `RectTransform`, and below it every view it contains is listed recursively with more indentation (from root views down to child views).

This window is read-only (the whole content is wrapped in `EditorGUI.DisabledScope(true)`), so it can only be viewed, not modified; it repaints automatically whenever `View.Dirty` is set, so opening and closing views are reflected immediately. When there is no view container yet, the window only shows the sentence `There is nothing here.`.

It is very useful when debugging the view system: after opening a view, it tells you directly which container the view is attached to, whose child it is, and how deeply it is nested, without having to find the object by hand in the Hierarchy window.

## Spatial Indexing

### Quadtree\<T\>

`Quadtree<TElementPosition>` is a quadtree: it recursively divides two-dimensional space into four equal parts, trading space for time so that "querying the elements within a circle or a rectangle" does not require iterating over all elements. `TElementPosition` is the position type of the elements.

`Quadtree<T>` is an abstract class: elements have to implement `IQuadtreeElement<T>` (providing `Position` and `SetOwner`), and nodes have to be created by an `ICreateNodeHandler`.

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
            // The arguments are passed to the constructor of Node as they are
            return new MyNode(tree, parent, level, aabb2);
        }
    }

    private sealed class MyNode : Node
    {
        public MyNode(Quadtree<Vector2> tree, Node parent, int level, Aabb2 aabb2) : base(tree, parent, level, aabb2)
        {
        }

        // Determines whether the specified range contains the position, used to put an element into the correct child node
        protected override bool Contains(Aabb2 aabb2, Vector2 elementPosition)
        {
            return aabb2.Contains(elementPosition);
        }

        // The square of the distance from the position to the specified point
        protected override float GetSquareDistance(Vector2 elementPosition, Vector2 point)
        {
            return (elementPosition - point).sqrMagnitude;
        }
    }
}
```

An element implementation:

```csharp
public sealed class MyQuadtreeElement : IQuadtreeElement<Vector2>
{
    public Vector2 Position { get; set; }

    private Quadtree<Vector2>.Node _owner;

    // Called by the quadtree to record "the node that directly holds this element",
    // so that it can be removed from that node correctly
    public void SetOwner(Quadtree<Vector2>.Node owner)
    {
        _owner = owner;
    }
}
```

Usage:

```csharp
var quadtree = new MyQuadtree(Aabb2.CenterSize(new Vector2(0, 0), new Vector2(1000, 1000)), levels: 5, maxElements: 8);

quadtree.Add(element); // Adds it; returns false when it is already in the tree
quadtree.Contains(elementPosition);
quadtree.Remove(element); // Removes it through the owner node recorded by the element

var rootNode = quadtree.RootNode;
var aabb2 = quadtree.Aabb2; // The coverage of the whole tree
var levels = quadtree.Levels; // The maximum number of levels (5 is recommended)
var maxElements = quadtree.MaxElements; // The upper limit of elements a single node holds directly (8 is recommended)

// Range queries
var inCircle = new List<IQuadtreeElement<Vector2>>();
quadtree.GetElementsInCircle(new Vector2(0, 0), 100, inCircle);

var inAabb2 = new List<IQuadtreeElement<Vector2>>();
quadtree.GetElementsInAabb2(Aabb2.CenterSize(new Vector2(0, 0), new Vector2(200, 200)), inAabb2);
```

A node (`Quadtree<T>.Node`) can also be used directly:

```csharp
var node = quadtree.RootNode;

var tree = node.Tree;
var parent = node.Parent;
var level = node.Level; // 0 for the root node
var range = node.Aabb2;
var count = node.Count; // The total number of elements held directly and indirectly

node.Contains(elementPosition);

var elements = new List<IQuadtreeElement<Vector2>>();
node.GetElements(elements); // Gets all elements under this node and its subtrees

var typedElements = new List<MyQuadtreeElement>();
node.GetElements(typedElements); // Gets only elements of the specified type

var children = new Quadtree<Vector2>.Node[4];
node.GetChildren(children); // Gets the child nodes

node.Remove(element);
```

### Octree\<T\>

`Octree<TElementPosition>` is an octree whose usage corresponds exactly to the quadtree, except that the space goes from two dimensions to three: nodes have 8 children each, the range type is `Aabb3`, and the position type is usually `Vector3`.

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

Both indexes validate on construction: the range must not contain `NaN` or infinity, `levels` and `maxElements` must both be greater than or equal to 1, and `createNodeHandler` must not be `null`.

## Math and Geometry

### Aabb2

`Aabb2` is a two-dimensional axis-aligned bounding box. It can be serialized, formatted, and compared.

```csharp
// Four ways to construct it
var a = new Aabb2(1, 1); // Uses one point as both the minimum and the maximum
var b = new Aabb2(new Vector2(1, 1));
var c = new Aabb2(0, 0, 10, 10); // The minimum and the maximum
var d = new Aabb2(new Vector2(0, 0), new Vector2(10, 10));

// Constructing from a center and a size
var e = Aabb2.CenterSize(5, 5, 10, 10);
var f = Aabb2.CenterSize(new Vector2(5, 5), new Vector2(10, 10));

// Containing all the passed points
var g = Aabb2.Points(new[] { new Vector2(1, 1), new Vector2(3, 5), new Vector2(2, 2) });
```

```csharp
var aabb2 = new Aabb2(new Vector2(0, 0), new Vector2(10, 10));

// Components
var minX = aabb2.MinX;
var minY = aabb2.MinY;
var centerX = aabb2.CenterX;
var centerY = aabb2.CenterY;
var maxX = aabb2.MaxX;
var maxY = aabb2.MaxY;

// Reading and writing in vector form
aabb2.Min = new Vector2(0, 0);
aabb2.Center = new Vector2(5, 5);
aabb2.Max = new Vector2(10, 10);
aabb2.Size = new Vector2(10, 10); // The size
aabb2.Extends = new Vector2(5, 5); // The half size

// Converting between normalized and actual positions (t is the weight of each component)
var point = aabb2.Lerp(new Vector2(0.5f, 0.5f)); // (5, 5)
var t = aabb2.Unlerp(new Vector2(5, 5)); // (0.5, 0.5)

// Expanding the range
aabb2.Include(new Vector2(-5, -5));
aabb2.Include(new Aabb2(new Vector2(-5, -5), new Vector2(-1, -1)));

// Tests
aabb2.Contains(new Vector2(5, 5));
aabb2.Contains(new Aabb2(new Vector2(1, 1), new Vector2(2, 2)));
aabb2.Overlaps(new Aabb2(new Vector2(-1, -1), new Vector2(1, 1)));

// Converting to and from Unity's types
Rect rect = (Rect)aabb2; // An explicit conversion
Aabb2 back = (Aabb2)rect;
Aabb3 aabb3 = aabb2; // An implicit conversion to three dimensions
Aabb2 back2 = aabb3;
```

Note that `Contains(Vector2)` is decided as "the lower and left boundaries are included, the upper and right boundaries are not" (`minX <= x && maxX > x`), consistent with the convention of `Rect`.

### Aabb3

`Aabb3` is the three-dimensional version, corresponding one-to-one in usage to `Aabb2`. It can additionally contain two-dimensional points and boxes, and convert to and from `Bounds`.

```csharp
var aabb3 = Aabb3.CenterSize(Vector3.zero, Vector3.one * 10);

aabb3.Include(new Vector2(1, 1)); // A two-dimensional point is treated as z = 0
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

`UnityMath` is a set of math methods that complement `Mathf`, covering trigonometry, interpolation without range limiting, conversions between coordinates inside and outside a rectangle, barycentric coordinates, and point-in-polygon tests.

```csharp
// Gets the cosine and the sine at once, avoiding two trigonometric calls
var cosSin = UnityMath.CosSin(Mathf.PI * 0.5f); // (0, 1)

// The UV of a point relative to a rectangle (similar to Rect.PointToNormalized, but the result is not restricted to [0, 1])
var uv = UnityMath.GetUV(new Rect(0, 0, 100, 100), new Vector2(50, 50)); // (0.5, 0.5, 0, 0)

// Interpolation without range limiting (Unity's built-in Vector2.Lerp restricts t to [0, 1])
var lerped2 = UnityMath.LerpUnclamped(new Vector2(0, 0), new Vector2(10, 10), new Vector2(2, 2));
var lerped3 = UnityMath.LerpUnclamped(Vector3.zero, Vector3.one, Vector3.one * 2);
var lerped4 = UnityMath.LerpUnclamped(Vector4.zero, Vector4.one, Vector4.one * 2);

// Conversions between normalized coordinates and points inside a rectangle (neither is restricted to [0, 1])
var point = UnityMath.NormalizedToPointUnclamped(new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
var normalized = UnityMath.PointToNormalizedUnclamped(new Rect(0, 0, 100, 100), new Vector2(50, 50));

// Triangles and polygons
var barycentric = UnityMath.GetBarycentricCoordinatesOfTriangle(p, a, b, c); // Computes the barycentric coordinates of the triangle; the three components are α, β, γ
var isInsideTriangle = UnityMath.IsPointInsideTriangle(barycentric);
var isInsidePolygon = UnityMath.IsPointInsidePolygon(point2, polygonVertices);
```

### RectExtensions

`RectExtensions.Contains` determines whether one rectangle completely contains another (the `Contains` built into `Rect` accepts only a point).

```csharp
var outer = new Rect(0, 0, 100, 100);
var inner = new Rect(10, 10, 10, 10);
var contains = outer.Contains(inner); // true
```

### Point in Polygon

`IInclusionOfAPointInAPolygonAlgorithm` defines the interface of a "determine whether a point is inside a polygon" algorithm. This package provides the implementation based on the winding number, `WindingNumber`. `UnityMath.IsPointInsidePolygon` uses it internally.

```csharp
IInclusionOfAPointInAPolygonAlgorithm algorithm = new WindingNumber();
var isInside = algorithm.IsPointInsidePolygon(point, polygonVertices);
```

## Object and Component Utilities

### UnityEngineObjectUtility

`UnityEngineObjectUtility` exists mainly to bypass the restrictions of the Unity main thread: operations such as equality tests on `UnityEngine.Object` can by default only be called on the main thread, while these methods can safely be used on other threads. It retrieves Unity internals through reflection (only once, during type initialization), so that the liveness, equality, and instance ID of objects can be determined off the main thread.

```csharp
var ptr = UnityEngineObjectUtility.GetCachedPtr(obj); // The memory address of the wrapped native C++ object
var instanceId = UnityEngineObjectUtility.GetInstanceId(obj); // The instance ID
var exists = UnityEngineObjectUtility.DoesObjectWithInstanceIdExist(instanceId); // Whether an object with this ID still exists

var isAlive = UnityEngineObjectUtility.IsAlive(obj); // Whether the instance is still alive (false when destroyed)

// An equality test that is not restricted to "main thread only"
if (UnityEngineObjectUtility.Equals(a, b))
{
    // ...
}
```

`UnityEngineObjectEqualityComparer.Instance` is the accompanying `IEqualityComparer<Object>`, which can be used in collections such as `Dictionary` and `HashSet` to treat destroyed objects and `null` as equal.

```csharp
var set = new HashSet<GameObject>(UnityEngineObjectEqualityComparer.Instance);
```

### ComponentExtensions

```csharp
var rigidbody = component.GetOrAddComponent<Rigidbody>(); // Gets it when present, adds it otherwise
component.RemoveComponent<Rigidbody>(); // Destroys it when present
```

### GameObjectExtensions

```csharp
gameObject.SetLayerRecursively(UnityEnvironment.UILayer); // Sets the layer together with all children
gameObject.SetTagRecursively("Player"); // Sets the tag together with all children

var component = gameObject.GetOrAddComponent<Rigidbody>();
gameObject.RemoveComponent<Rigidbody>();

var disableToken = gameObject.GetDisableToken(); // A cancellation token bound to the active state

gameObject.DestroyChildren(); // Destroys all children (in reverse order)
gameObject.DestroyChildrenImmediate(); // Destroys all children immediately
```

### CommonExtensions

A group of "destroy and clear" extension methods on `List<T>`, replacing hand-written loops. The generic constraint is `UnityEngine.Object`.

```csharp
var enemies = new List<Enemy>();
enemies.ClearAndDestroy(); // Destroys each one and then clears the list
enemies.ClearAndDestroyImmediate(); // Calls DestroyImmediate on each one and then clears the list
```

### TransformExtensions

```csharp
transform.SwapSibling(otherTransform); // Swaps the order with another sibling
transform.SetBeforeSibling(otherTransform); // Places it before another sibling
transform.SetAfterSibling(otherTransform); // Places it after another sibling

var hierarchies = new List<Transform>();
transform.GetHierarchies(hierarchies); // The Transforms along the whole parent chain

var hierarchyNames = new List<string>();
transform.GetHierarchyNames(hierarchyNames); // The names along the whole parent chain

var scenePath = transform.GetScenePath(); // The path in the scene, for example "Canvas/Panel/Button"

transform.DestroyChildren();
transform.DestroyChildrenImmediate();
```

### TransformUtility

```csharp
var same = TransformUtility.AreTransformsShareSameParent(a, b);
var same2 = TransformUtility.AreTransformsShareSameParent(a, b, c);
var same3 = TransformUtility.AreTransformsShareSameParent(a, b, c, d);
var same4 = TransformUtility.AreTransformsShareSameParent(transforms); // The params overload
```

### Parent Depth Comparers

`GameObjectParentCountComparer` and `ComponentParentCountComparer` compare objects by "number of parent levels" (how deeply they are nested in the Hierarchy window), providing a stable and intuitive order for sorting (for example, in UI the higher objects update first).

```csharp
objects.Sort(GameObjectParentCountComparer.Instance);
components.Sort(ComponentParentCountComparer.Instance);

objects.Sort(GameObjectParentCountComparer.InstanceReversed); // Reversed
components.Sort(ComponentParentCountComparer.InstanceReversed);
```

### Exceptions and Related Enums

This package includes a set of exceptions with clear semantics, used to fail early with a readable message when "a precondition is not met".

```csharp
throw new GameObjectActiveException(gameObject); // When the object is expected not to be active
throw new GameObjectInactiveException(gameObject); // When the object is expected to be active
throw new BehaviourActiveAndEnabledException(behaviour); // When the Behaviour is expected not to be "active and enabled"
throw new BehaviourInactiveAndDisabledException(behaviour); // When the Behaviour is expected to be "inactive and disabled"
throw new BehaviourInactiveOrDisabledException(behaviour); // When the Behaviour is expected to be "inactive or disabled"
throw new ComponentNotGotException(gameObject, GetComponentMethod.Self, typeof(Canvas)); // When a component is expected to be found
throw new UnityWebRequestException(unityWebRequest); // When a web request fails
```

These exceptions all carry the corresponding object reference (`GameObject`, `Behaviour`, `UnityWebRequest`, and so on), which makes them easy to locate.

The `GetComponentMethod` enum describes "where the component is taken from", and is used in `ComponentNotGotException`:

| Value                       | The corresponding method          |
|-----------------------------|-----------------------------------|
| `Self`                      | `GetComponent<T>`                 |
| `Parent`                    | `GetComponentInParent<T>()`       |
| `ParentIncludingInactive`   | `GetComponentInParent<T>(true)`   |
| `Children`                  | `GetComponentInChildren<T>()`     |
| `ChildrenIncludingInactive` | `GetComponentInChildren<T>(true)` |

The `When` enum describes "when" (`Always`, `Playing`, `NotPlaying`), and is used by `[GuiDisable]` to control under which circumstances a field is greyed out.

## Web Requests

### UnityWebRequestUtility and UnityWebRequestExtensions

`UnityWebRequestUtility` provides a set of task-based methods that replace the callback-style API of `UnityWebRequest`. `UnityWebRequestExtensions` is the extension-method version of the same functionality, allowing chained calls.

```csharp
using var unityWebRequest = UnityWebRequest.Get("https://example.com");
unityWebRequest.timeout = 60;

// Sends the request only
await unityWebRequest.SendWebRequestAsync(cancellationToken);

// Sends the request and retrieves the data
var text = await unityWebRequest.GetStringAsync(cancellationToken);
var bytes = await unityWebRequest.GetByteArrayAsync(cancellationToken);
var stream = await unityWebRequest.GetStreamAsync(cancellationToken);

// The static methods can also be called (passing the request as the first argument)
await UnityWebRequestUtility.SendWebRequestAsync(unityWebRequest, cancellationToken);
var text2 = await UnityWebRequestUtility.GetStringAsync(unityWebRequest, cancellationToken);
```

Several tests and validation methods:

```csharp
// Whether the request has already been disposed
if (UnityWebRequestUtility.IsDisposed(unityWebRequest))
{
    // ...
}

// Whether the request timed out (treated as a timeout when the error message equals UnityUtility.UnityWebRequestTimeoutString)
var isTimeout = unityWebRequest.IsTimeout();
var isTimeout2 = UnityWebRequestUtility.IsTimeout(unityWebRequest);

// Whether the HTTP status code indicates success
var isSuccess = unityWebRequest.IsSuccessStatusCode();

// Validations in the form of throwing
UnityWebRequestUtility.ThrowIfDisposed(unityWebRequest); // Already disposed -> ObjectDisposedException
UnityWebRequestUtility.ThrowIfFaulted(unityWebRequest); // The request failed -> UnityWebRequestException
UnityWebRequestUtility.ThrowIfNotSuccessStatusCode(unityWebRequest); // The status code is not a success -> UnityWebRequestException
unityWebRequest.ThrowIfNotSuccessStatusCode();
```

Note that methods such as `GetStringAsync` throw `UnityWebRequestException` when the request fails and `OperationCanceledException` when it is cancelled; they are safe to call only on the Unity main thread (otherwise they throw `UnityException`).

### UnityWebRequestHandler

`UnityWebRequestHandler` is an `HttpMessageHandler` implemented with `UnityWebRequest`, so Unity's networking stack can be plugged directly into `System.Net.Http.HttpClient`, gaining the full capabilities of `HttpClient` (`HttpRequestMessage`, `HttpResponseMessage`, interceptors, and so on).

```csharp
// Basic usage
using var httpClient = new HttpClient(new UnityWebRequestHandler());

// Doing some preparation before each request is sent (setting the timeout, certificates, and so on)
using var httpClient2 = new HttpClient(
    new UnityWebRequestHandler(unityWebRequest =>
    {
        unityWebRequest.timeout = 60;
    })
);

var response = await httpClient.GetAsync("https://example.com");
var content = await response.Content.ReadAsStringAsync();
```

Two of its responsibilities are worth noting:

- If `SendAsync` is not called on the Unity main thread (for example inside `Task.Run`, or when `HttpClient` schedules the request onto the thread pool), it first waits to get back to the main thread before sending the request. This is a limitation of `UnityWebRequest` itself.
- The `UnityWebRequestException` thrown by `UnityWebRequest` is converted to `HttpRequestException`, to comply with the convention of `HttpMessageHandler`.

## Preference

### Preference\<TValue\>

`Preference<TValue>` is a wrapper base class over `UnityEngine.PlayerPrefs`: it stores values using the three primitive types supported by `UnityEngine.PlayerPrefs` (`int`, `float`, `string`), and then maps them to the type `TValue` actually used by the caller through a pair of conversion methods.

Derived classes come in three kinds by primitive type, and their constructors take a `PreferenceConverterPair`:

```csharp
// The primitive type is int
public class Int32Preference<TValue> : Preference<TValue> { ... }

// The primitive type is float
public class SinglePreference<TValue> : Preference<TValue> { ... }

// The primitive type is string
public class StringPreference<TValue> : Preference<TValue> { ... }
```

The members provided by the base class:

```csharp
var key = preference.Key;
var keyExists = preference.KeyExists; // Whether the key exists in PlayerPrefs
var valueType = preference.ValueType; // Int32 / Single / String

preference.SetValue(value);
var value2 = preference.GetValue();
var value3 = preference.GetValue(defaultValue); // Returns defaultValue when the key does not exist

preference.Remove(); // Removes it from PlayerPrefs
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

When the user type is not a primitive type, pass in your own conversion methods; `PreferenceConverterPair<TValue, TPreferenceValue>` is the pair of "value → primitive value" and "primitive value → value" conversions:

```csharp
// Using Vector2Int as the user type and storing it as a string
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

`PreferenceConverterPair<TValue, TPreferenceValue>` can also be used directly without writing a derived class.

### Wrapping and Overriding Preferences

`WrappedPreference<TValue>` wraps another `Preference<TValue>` in order to attach behavior on top of it; `DefaultValuePreference<TValue>` additionally attaches a default value. This package provides two ready-made implementations:

- `OverridePreference<TValue>`: when the wrapped `GetValue()` throws, it overwrites the original value with `DefaultValue` and then returns it (reading the original value throws, so it has to be overwritten).
- `OverlyPreference<TValue>`: when the wrapped value does not exist, it forcibly writes `DefaultValue` into the original value and returns it; in all other cases it returns the wrapped value.

`PreferenceValueType` is the enum of "the underlying primitive type": `Int32`, `Single`, `String`.

If a value needs to be stored "as is" (without any conversion), `IdentityInt32Preference`, `IdentitySinglePreference`, and `IdentityStringPreference` can be used.

## Cursor

`CursorInfo` is a set of "cursor texture + hotspot + mode" values; `CursorStack` manages cursors as a stack: `Push` sets a new cursor (usually because "the mouse is hovering over some area"), and `Pop` restores the previous one.

```csharp
// If a default cursor is set in PlayerSettings, the initial cursor should be told to it after the program starts
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

var count = CursorStack.Count; // The number of cursors currently on the stack
```

In the editor environment, the cursor stack is cleared automatically when play mode is exited.

## Color

### AuroraColor

`AuroraColor` is a color represented by four `byte`s (RGBA). Through `StructLayout(LayoutKind.Explicit)` it overlaps an `int` and four `byte`s on the same 4 bytes of memory, so its layout matches `Color32` and converting between them costs nothing extra.

```csharp
var color = new AuroraColor(255, 0, 0); // Opaque by default
var translucent = new AuroraColor(255, 0, 0, 128);

// HTML color strings: #RGB, #RRGGBB, #RGBA, #RRGGBBAA; the leading # can be omitted; color names such as red are not supported
var fromHtml = new AuroraColor("#FF0000");
var fromHtml2 = new AuroraColor("FF000080");

// Components
var r = color.R;
var g = color.G;
var b = color.B;
var a = color.A;

// Reading and writing by index: 0 -> R, 1 -> G, 2 -> B, 3 -> A; any other index throws IndexOutOfRangeException
color[0] = 128;

// Converting to and from Unity's types (both are implicit conversions)
Color32 unityColor32 = color;
Color unityColor = color;
AuroraColor fromUnityColor32 = unityColor32;
AuroraColor fromUnityColor = unityColor;
```

### ColorUtility

`ColorUtility.ParseHtmlString` is a static method that parses an HTML color string directly into a `Color`.

```csharp
var color = ColorUtility.ParseHtmlString("#FF0000");
```

### Color Extension Methods

`ColorExtensions` and `Color32Extensions` provide deconstruction, hue tests, and hue replacement.

```csharp
var color = new Color(1, 0, 0, 1);

// Deconstructing into the components
var (r, g, b, a) = color;
var (h, s, v) = color; // Deconstructing into HSV

// The hue is meaningless when the R, G, and B components are equal
if (color.IsHUndefined())
{
    // ...
}

// The Color32 version
var color32 = (Color32)color;
var (r32, g32, b32, a32) = color32;
var (h32, s32, v32) = color32;
if (color32.IsHUndefined())
{
    // ...
}

// Copies it and sets a new hue (h is in [0, 1]; going outside the range throws ArgumentOutOfRangeException)
var changed = color32.WithH(0.5f);

// Sets the hue and tells you whether the color actually changed
// For example, a red color with hue 0 or hue 1 is still red (the hue is cyclic), so the old and new colors are the same and false is returned
if (color32.TryWithH(0.5f, out var result))
{
    // ...
}
```

`Color` has no `WithH`; when the hue needs to be changed, the `Color32` version can be used, or `Color.HSVToRGB` can be combined by hand.

## Assets, Screenshots, and Other Runtime Utilities

### AsyncOperationUtility

```csharp
if (AsyncOperationUtility.IsDisposed(asyncOperation))
{
    // ...
}
```

### UnityUtility

```csharp
// Constants
var clickDelayTime = UnityUtility.ClickDelayTime; // The maximum interval between the two clicks of a double click, 0.3 seconds
var timeoutString = UnityUtility.UnityWebRequestTimeoutString; // "Request timeout"
var vertexCountMax = UnityUtility.VertexCountPerMeshMaxValue; // 65000 - 1

// Removes the redundant parts of an object's name, turning it into a form better suited for logs and the Hierarchy window
UnityUtility.OptimizeName(@object);

// Deselects the specified object if it is exactly the current selection of the event system
UnityUtility.DeselectEventSystemCurrentSelectedGameObject(gameObject);
// Deselects the specified object if it is exactly the selection of the specified event system
UnityUtility.DeselectEventSystemCurrentSelectedGameObject(gameObject, eventSystem);

// Cancels a CancellationTokenSource after the specified waiting time; disposing the returned object terminates the cancellation
using (UnityUtility.CancelAfter(cancellationTokenSource, TimeSpan.FromSeconds(3)))
{
    // ...
}

// Screenshots (PNG files)
UnityUtility.BeginCaptureScreenshot(@"D:\screenshot.png"); // Does not wait for the screenshot to finish
await UnityUtility.CaptureScreenshotAsync(@"D:\screenshot.png"); // Waits for the screenshot to finish
await UnityUtility.CaptureScreenshotAsync(@"D:\screenshot.png", cancellationToken);
```

### SpriteUtility and SpriteRendererUtility

Converting a "normalized coordinate" into the local or world coordinate of a sprite is useful for things like attaching effects and positioning health bars.

```csharp
var localPosition = SpriteUtility.NormalizedToLocalPosition(sprite, new Vector2(0.5f, 1f)); // The middle of the top edge of the sprite

var localPosition2 = SpriteRendererUtility.NormalizedToLocalPosition(spriteRenderer, new Vector2(0.5f, 1f));
var worldPosition = SpriteRendererUtility.NormalizedToWorldPosition(spriteRenderer, new Vector2(0.5f, 1f));

// The multipliers related to flipX / flipY (1 for a component that is not flipped, -1 for one that is)
var multiplier = SpriteRendererUtility.GetFlipMultiplier(spriteRenderer);
```

### Texture2DUtility

```csharp
if (Texture2DUtility.IsRedQuestionMarkTexture(texture))
{
    // Determines whether this texture is the red question mark image Unity generates when it cannot read the image content (8x8 in size, globally unique)
}
```

### ProfilerScope

```csharp
using (new ProfilerScope("MySection"))
{
    // This piece of code is sampled by the Profiler
}

using (new ProfilerScope("MySection", targetObject))
{
    // The sampling is associated with targetObject, making it easier to locate in the Profiler
}
```

### ServerTimeOwner

`ServerTimeOwner` records "server time": the client synchronizes the time once at some moment, and afterwards it can be used to derive the current server time.

```csharp
var serverTimeOwner = new ServerTimeOwner();

// Set it at the moment the server time is received
serverTimeOwner.CurrentTime = DateTimeOffset.UtcNow;

// Read afterwards: this is the time derived from the elapsed local time
var currentTime = serverTimeOwner.CurrentTime; // null when it has not been set
```

### Screen Change Notifications

`NotifyScreenSizeChangedScope` and `NotifyScreenOrientationChangedScope` wrap "listening for screen size/orientation changes" as an `IDisposable`: the callback runs at the specified player loop phase when the change happens, and the listening stops when it is disposed.

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

## Editor Tools

The editor functionality lives in the `Aurora.UnityEditor` assembly, in the `Aurora.UnityEditor` namespace.

### Context Menus

Right-clicking a component header in the Inspector window brings up a group of conversion menus, which replace one UI component with another while preserving as many of the original properties as possible (color, material, the raycast toggle, whether it is maskable, and so on) and keeping the component's order on the `GameObject`:

- `Button` ⇄ `EnhancedButton`
- `Image`, `RawImage`, `Clear`, `Block`, `Circle`, `Annulus`, `RoundedRectangle`, `RoundedRectangleBorder`, and `CustomGraphic` can be converted between one another in any combination
- `HorizontalLayoutGroup` ⇄ `VerticalLayoutGroup`
- `Clear` additionally has `Delete Useless Properties`, which removes the meaningless properties on a transparent graphic

### Aurora Unity Menu

The `Aurora Unity` menu (on the Unity main menu bar) collects small tools useful in daily development. Each entry can be located by its name in `UnityEditorUtility.MenuItems`:

| Menu item                               | Description                                                                                                                                            |
|-----------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Initialize`                            | Runs the initialization process once again manually                                                                                                    |
| `Allow Unsafe Code`                     | Toggles `PlayerSettings.allowUnsafeCode` (the menu item shows the current checked state)                                                               |
| `Clear Log Entries`                     | Clears the entries in the Console window                                                                                                               |
| `Request Script Compilation`            | Requests a script compilation from the Unity engine                                                                                                    |
| `Layout/Ping Layout Root`               | Pings the layout root node of the selected `RectTransform`                                                                                             |
| `Layout/Mark Layout For Rebuild`        | Marks the selected `RectTransform` as needing a layout rebuild                                                                                         |
| `Layout/Force Rebuild Layout Immediate` | Immediately forces a rebuild of the layout of the selected `RectTransform`                                                                             |
| `Log Graphic Raycast Target`            | Logs the `raycastTarget` value of the selected `Graphic`                                                                                               |
| `Log RectTransform`                     | Logs the key information of the selected `RectTransform` (anchors, pivot, position, size, and so on)                                                   |
| `Optimize Object Name`                  | Normalizes the name of the selected asset/object                                                                                                       |
| `Clipboard/Convert Path to GUID`        | Converts the asset path in the clipboard into a GUID                                                                                                   |
| `Clipboard/Ping Path`                   | Pings the asset path in the clipboard                                                                                                                  |
| `Clipboard/Convert GUID to Path`        | Converts the GUID in the clipboard into an asset path                                                                                                  |
| `Clipboard/Ping GUID`                   | Pings the asset corresponding to the GUID in the clipboard                                                                                             |
| `Capture Screenshot to Desktop`         | Takes a screenshot and saves it to the desktop                                                                                                         |
| `Open Persistent Data Path`             | Opens `Application.persistentDataPath` in the file manager                                                                                             |
| `Validate View Prefabs`                 | Scans every prefab in the project and finds the ones where "the view is active and enabled" (the view system requires them to be inactive or disabled) |

### DefineSymbolScope

`DefineSymbolScope` wraps "adding and removing preprocessor symbols in bulk" as an `IDisposable`: only the in-memory symbol list is modified inside the scope, and the whole list is written to the build target group once when the scope is left.

```csharp
using (var scope = new DefineSymbolScope())
{
    scope.Add("MY_SYMBOL");
    scope.Remove("OTHER_SYMBOL");
    var isDefined = scope.IsDefined("MY_SYMBOL");
} // When the scope is left, the current symbol list is actually written to the build target group if it changed
```

By default it applies to the currently selected build target group; a `BuildTargetGroup` can also be passed explicitly.

### ReorderableListHelper

`ReorderableListHelper` provides the constants and helper methods needed when using `UnityEditorInternal.ReorderableList`, to compute element height, line spacing, and the footer height of nested lists correctly.

```csharp
var list = new ReorderableList(serializedObject, serializedProperty);
list.elementHeightCallback = index => ReorderableListHelper.GetElementHeight(lineCount: 2);
list.drawElementCallback = (rect, index, isActive, isFocused) =>
{
    ReorderableListHelper.InitializeY(ref rect);
    ReorderableListHelper.SetSingleLineHeight(ref rect);
    // Draws the first line
    ReorderableListHelper.NextLine(ref rect);
    // Draws the second line
};
```

### ReorderableListWithState

`ReorderableListWithState` is a derived class of `ReorderableList` that carries a user-defined state object on top of the original callbacks, avoiding a pile of closures just to pass state to the callbacks.

```csharp
var list = new ReorderableListWithState(serializedObject, serializedProperty, state: myState);
list.drawElementCallback = (rect, index, isActive, isFocused, state) =>
{
    // ...
};
var state = list.State;
```

### UnityEditorUtility

`UnityEditorUtility` provides general editor-side capabilities:

```csharp
var isChildrenIncluded = UnityEditorUtility.IsChildrenIncluded(property);
UnityEditorUtility.ThrowIfSymbolInvalid("MY_SYMBOL");
var isValid = UnityEditorUtility.IsGuidValid("0123456789abcdef0123456789abcdef");

var projectPath = UnityEditorUtility.ProjectPath;
var consoleWindowType = UnityEditorUtility.EditorWindowTypes.Console; // Also Game / Hierarchy / Inspector / Project / Scene
```

### UnityEditorGUIUtility

`UnityEditorGUIUtility` provides IMGUI drawing helpers: `DrawOuterBorder`, `DrawInnerBorder`, and `DrawCellsArea`, which draws a grid-like area in one go (used together with `DrawCellsAreaOptions`).

`DrawCellsAreaOptions` describes how a grid area is drawn: whether row indices go from bottom to top or from top to bottom (`CellRowOrigin.Bottom` / `Top`), how the background and the cells are drawn, how the add and delete row/column buttons are drawn, and the styles and offsets of the axis labels and index labels.

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
