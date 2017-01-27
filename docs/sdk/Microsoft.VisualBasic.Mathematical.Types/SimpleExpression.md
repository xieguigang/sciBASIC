# SimpleExpression
_namespace: [Microsoft.VisualBasic.Mathematical.Types](./index.md)_

A class object stand for a very simple mathematic expression that have no bracket or function.
 It only contains limited operator such as +-*/\%!^ in it.
 (一个用于表达非常简单的数学表达式的对象，在这个所表示的简单表达式之中不能够包含有任何括号或者函数，
 其仅包含有有限的计算符号在其中，例如：+-*/\%^!)



### Methods

#### Evaluate
```csharp
Microsoft.VisualBasic.Mathematical.Types.SimpleExpression.Evaluate(System.String)
```
Using the default math script expression engine.

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### ToString
```csharp
Microsoft.VisualBasic.Mathematical.Types.SimpleExpression.ToString
```
Debugging displaying in VS IDE


### Properties

#### LastOperator
The last operator of this expression.
#### MetaList
A simple expression can be view as a list collection of meta expression.
 (可以将一个简单表达式看作为一个元表达式的集合)
#### ReferenceDepth
通过比较引用深度来决定在系统初始化的时候表达式对象的计算的先后顺序，深度越小的约优先计算
