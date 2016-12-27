# APIDelegate
_namespace: [Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIDelegate.#ctor(Microsoft.VisualBasic.ComponentModel.Binding{Microsoft.VisualBasic.CommandLine.Reflection.ExportAPIAttribute,System.Reflection.MethodInfo},System.Func{System.Object[],System.Int32})
```
不可以使用本方法初始化目标对象为实例方法的类型

|Parameter Name|Remarks|
|--------------|-------|
|attribute|-|
|Invoke|-|


#### Execute
```csharp
Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIDelegate.Execute(System.Object[])
```


|Parameter Name|Remarks|
|--------------|-------|
|parameters|数组的长度必须与目标函数的参数的数目一致，否则短于目标函数的参数的数目的数组会使用Nothing来填充缺少的部分，而多于目标函数的参数会被截断|



### Properties

#### Name
The usage name of this command line entry point.(本命令行对象的调用命令名称)
