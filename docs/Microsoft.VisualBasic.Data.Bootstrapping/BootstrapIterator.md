# BootstrapIterator
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping](./index.md)_

参数估计的过程之中的迭代器，这个模块内的函数主要是用来产生数据源的



### Methods

#### Bootstrapping
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.BootstrapIterator.Bootstrapping(System.Type,System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{Microsoft.VisualBasic.Mathematical.IValueProvider}},System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{Microsoft.VisualBasic.Mathematical.IValueProvider}},System.Int64,System.Int32,System.Double,System.Double,System.Boolean,System.Boolean,System.Boolean)
```
Bootstrapping 参数估计分析，这个函数用于生成基本的采样数据

|Parameter Name|Remarks|
|--------------|-------|
|parameters|各个参数的变化范围|
|model|具体的求解方程组|
|k|重复的次数|
|y0|
 ``Y0``初值，在进行参数估计的时候应该是被固定的，在进行系统状态分布的计算的时候才是随机的
 |
|parallel|并行计算模式有极大的内存泄漏的危险|


#### Bootstrapping``1
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.BootstrapIterator.Bootstrapping``1(Microsoft.VisualBasic.Mathematical.PreciseRandom,System.Collections.Generic.IEnumerable{System.String},System.Collections.Generic.IEnumerable{System.String},System.Int64,System.Int32,System.Double,System.Double,System.Boolean,System.Boolean)
```
这个更加适合没有任何参数信息的时候的情况

|Parameter Name|Remarks|
|--------------|-------|
|range|-|
|vars|-|
|yinit|-|
|k|-|
|n|-|
|a|-|
|b|-|
|trimNaN|-|
|parallel|并行计算模式有极大的内存泄漏的危险|


#### Iterates
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.BootstrapIterator.Iterates(Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{Microsoft.VisualBasic.Mathematical.IValueProvider}[],System.Type,Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{Microsoft.VisualBasic.Mathematical.IValueProvider}[],System.Collections.Generic.Dictionary{System.String,System.Action{System.Object,System.Double}},System.Int32,System.Double,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|parms|-|
|y0|-|
|ps|-|
|n|-|
|a|-|
|b|-|

> 在Linux服务器上面有内存泄漏的危险

#### SetParameters
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.BootstrapIterator.SetParameters(System.Collections.Generic.IEnumerable{System.String},System.Type)
```


|Parameter Name|Remarks|
|--------------|-------|
|model|@``T:Microsoft.VisualBasic.Mathematical.Calculus.ODEs``类型|
|vars|-|



