# TimerTrigger
_namespace: [Microsoft.VisualBasic.ComponentModel.Triggers](./index.md)_

在指定的日期和时间呗触发，因此这个触发器只会运行一次



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.Triggers.TimerTrigger.#ctor(System.DateTime,System.Action,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|time|只精确到分，不会比较秒数|
|task|-|
|interval|ms|


#### __test
```csharp
Microsoft.VisualBasic.ComponentModel.Triggers.TimerTrigger.__test
```
不计算毫秒

#### Start
```csharp
Microsoft.VisualBasic.ComponentModel.Triggers.TimerTrigger.Start
```
启动计时器线程，这个方法不会阻塞当前的线程


### Properties

#### Interval
ms
#### Time
当判定到达这个指定的时间之后就会触发动作
