# UnitExpression
_namespace: [Microsoft.VisualBasic.Mathematical.Types](./index.md)_

A class object stand for a very simple mathematic expression that have no bracket or function.
 It only contains limited operator such as +-*/\%!^ in it.
 (一个用于表达非常简单的数学表达式的对象，在这个所表示的简单表达式之中不能够包含有任何括号或者函数，
 其仅包含有有限的计算符号在其中，例如：+-*/\%^!)



### Methods

#### Evaluate
```csharp
Microsoft.VisualBasic.Mathematical.Types.UnitExpression.Evaluate
```
Calculate the value of this simple expression object.
 (计算这一个简单表达式对象的值)

#### op_Explicit
```csharp
Microsoft.VisualBasic.Mathematical.Types.UnitExpression.op_Explicit(Microsoft.VisualBasic.Mathematical.Types.UnitExpression)~System.Double
```
Get the value of this simple expression object.
 (计算这一个简单表达式对象的值)

|Parameter Name|Remarks|
|--------------|-------|
|e|-|


#### op_Implicit
```csharp
Microsoft.VisualBasic.Mathematical.Types.UnitExpression.op_Implicit(System.String)~Microsoft.VisualBasic.Mathematical.Types.UnitExpression
```
Convert the expression in the string type to this class object type.
 (将字符串形式的简单表达式转换为本对象类型)

|Parameter Name|Remarks|
|--------------|-------|
|expression|
 The string type arithmetic expression, please make sure that it must be contains no blank 
 space char exists in this string.
 (字符串类型的算术表达式，请确保本字符串中没有任何的空格符号)
 |



### Properties

#### LEFT
The number a in the function of "Arithmetic.Evaluate".
 (函数'Arithmetic.Evaluate'中的参数'a')
#### Operator
Arithmetic operator(运算符)
#### RIGHT
The number b in the function of "Arithmetic.Evaluate".
 (函数'Arithmetic.Evaluate'中的参数'b')
