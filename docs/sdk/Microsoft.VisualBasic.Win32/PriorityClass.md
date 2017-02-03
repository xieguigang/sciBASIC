# PriorityClass
_namespace: [Microsoft.VisualBasic.Win32](./index.md)_

Process priority class helper.



### Methods

#### GetCurrentProcess
```csharp
Microsoft.VisualBasic.Win32.PriorityClass.GetCurrentProcess
```
当前进程句柄

#### PriorityClass
```csharp
Microsoft.VisualBasic.Win32.PriorityClass.PriorityClass(System.Int32)
```
Set Priority Class for current process.

|Parameter Name|Remarks|
|--------------|-------|
|priority|@``F:Microsoft.VisualBasic.Win32.PriorityClass.IDLE_PRIORITY_CLASS``, @``F:Microsoft.VisualBasic.Win32.PriorityClass.HIGH_PRIORITY_CLASS``, @``F:Microsoft.VisualBasic.Win32.PriorityClass.NORMAL_PRIORITY_CLASS``|



### Properties

#### HIGH_PRIORITY_CLASS
新进程有非常高的优先级，它优先于大多数应用程序。基本值是13。注意尽量避免采用这个优先级
#### IDLE_PRIORITY_CLASS
新进程应该有非常低的优先级——只有在系统空闲的时候才能运行。基本值是4
#### NORMAL_PRIORITY_CLASS
标准优先级。如进程位于前台，则基本值是9；如在后台，则优先值是7
