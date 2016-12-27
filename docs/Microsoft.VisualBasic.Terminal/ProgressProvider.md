# ProgressProvider
_namespace: [Microsoft.VisualBasic.Terminal](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Terminal.ProgressProvider.#ctor(System.Int32)
```
生成进度条的百分比值

|Parameter Name|Remarks|
|--------------|-------|
|total|-|


#### ETA
```csharp
Microsoft.VisualBasic.Terminal.ProgressProvider.ETA(System.Double,System.Double,System.Int64)
```


|Parameter Name|Remarks|
|--------------|-------|
|previous#|百分比|
|cur#|百分比|
|Elapsed#|当前的这个百分比差所经历过的时间|


#### Step
```csharp
Microsoft.VisualBasic.Terminal.ProgressProvider.Step
```
返回来的百分比小数，还需要乘以100才能得到进度

#### StepProgress
```csharp
Microsoft.VisualBasic.Terminal.ProgressProvider.StepProgress
```
百分比进度，不需要再乘以100了


### Properties

#### Current
当前已经完成的tick数
#### Target
整个工作的总的tick数
