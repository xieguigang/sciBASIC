# GAFFitness
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver.GAFFitness.#ctor(System.Type,Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.Collections.Generic.Dictionary{System.String,System.Double},System.Boolean)
```
从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)

|Parameter Name|Remarks|
|--------------|-------|
|observation|只需要其中的@``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.y``有数据就行了|


#### __init
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver.GAFFitness.__init
```
初始化一些共同的数据

#### RunTest
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver.GAFFitness.RunTest(System.Collections.Generic.Dictionary{System.String,System.Double})
```
使用指定的参数测试计算模型的输出

|Parameter Name|Remarks|
|--------------|-------|
|parms|-|



### Properties

#### a
RK4 parameters
#### b
RK4 parameters
#### Ignores
被忽略比较的y变量名称
#### Model
具体的计算模型
#### modelVariables
模型之中所定义的y变量
#### n
RK4 parameters
#### observation
真实的实验观察数据
#### ref
样本列表部分计算的参考值
#### samples
计算的采样数
#### y0
ODEs y0
