# Expression
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

Expression Evaluation Engine



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Expression.#ctor
```
Creates a new mathematics expression evaluation engine

#### Compile
```csharp
Microsoft.VisualBasic.Mathematical.Expression.Compile(System.String)
```
当需要进行重复大量计算的时候，反复解析表达式字符串会浪费大量的计算时间，
 则可以使用这个解析出表达式，后面使用@``M:Microsoft.VisualBasic.Mathematical.Expression.SetVariable(System.String,System.Double)``更新变量
 的值即可快速的进行重复计算

|Parameter Name|Remarks|
|--------------|-------|
|expr$|-|


#### Evaluate
```csharp
Microsoft.VisualBasic.Mathematical.Expression.Evaluate(System.String)
```
This shared method using the default expression engine for the evaluation.

|Parameter Name|Remarks|
|--------------|-------|
|expr|-|


#### Evaluation
```csharp
Microsoft.VisualBasic.Mathematical.Expression.Evaluation(System.String)
```
Evaluate the a specific mathematics expression string to a double value, the functions, constants, 
 bracket pairs can be include in this expression but the function are those were originally exists 
 in the visualbasic. I'm sorry for this...
 (对一个包含有函数、常数和匹配的括号的一个复杂表达式进行求值，但是对于表达式中的函数而言：仅能够使用在
 VisualBaisc语言中存在的有限的几个数学函数。)

|Parameter Name|Remarks|
|--------------|-------|
|expr|-|


#### GetValue
```csharp
Microsoft.VisualBasic.Mathematical.Expression.GetValue(System.String)
```
先常量，后变量

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



### Properties

#### DefaultEngine
The default expression evaluation engine.
#### value
Gets constant or variable value, but only sets variable value.
