# I_FactorElement
_namespace: [Microsoft.VisualBasic.DataMining.DFL_Driver](./index.md)_

This object represents the factor which decides the node state changes.(决定@``T:Microsoft.VisualBasic.DataMining.DFL_Driver.dflNode``的状态的因素)



### Methods

#### get_InteractionQuantity
```csharp
Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement.get_InteractionQuantity
```
假若事件发生的话，这个函数决定了@``P:Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement.FunctionalState``所返回的计算值

#### Internal_getEventProbabilities
```csharp
Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement.Internal_getEventProbabilities
```
计算公式为 (1-w)， 即本函数返回的值越低，则事件越容易发生，请注意使用 rnd >= Internal_getEventProbabilities() 来描述事件发生

#### ShadowCopy
```csharp
Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement.ShadowCopy(Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement,Microsoft.VisualBasic.DataMining.DFL_Driver.dflNode)
```
@``T:Microsoft.VisualBasic.DataMining.DFL_Driver.dflNode``对象初始化的时候所使用的方法

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|Target|-|



### Properties

#### _ABS_Weight
1 - @``M:System.Math.Abs(System.Decimal)``(@``F:Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement._Weight``)
#### _Weight
@``F:Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement._Weight``越大,则@``F:Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement._ABS_Weight``越小，即事件发生的阈值越小
#### FunctionalState
Does this factor effects on the node states changes? value zero is no effects.
 (当前的这个因素是否会影响目标节点的状态值的改变，0表示不影响)
#### Weight
Weight = [-1,1]. (可以带有符号，介于-1到1之间)
