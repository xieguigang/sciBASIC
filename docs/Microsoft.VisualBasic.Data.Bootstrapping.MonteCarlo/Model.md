# Model
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo](./index.md)_

``y`` 声明的类型为@``T:Microsoft.VisualBasic.Mathematical.Calculus.var``类型的域;
 ``parameter`` 声明的类型为@``T:System.Double``类型的域



### Methods

#### eigenvector
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Model.eigenvector
```
在计算聚类的相似度的时候对y变量的特征提取

#### params
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Model.params
```
系统的状态列表，即方程里面的参数(应用于参数估计)

#### RunTest
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Model.RunTest(System.Collections.Generic.Dictionary{System.String,System.Double}[],System.Int32,System.Int32,System.Int32,System.Collections.Generic.Dictionary{System.String,System.Double})
```


|Parameter Name|Remarks|
|--------------|-------|
|estimates|-|
|n%|-|
|a%|-|
|b%|-|
|modify|修改部分数据|

> 线程不安全的

#### yinit
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Model.yinit
```
系统的初始值列表(应用于系统状态随机聚类)


