# MatrixMathArithmetic
_namespace: [Microsoft.VisualBasic.Mathematical.BasicR.Helpers](./index.md)_

The basics arithmetic operators' definition of matrix object in mathematics.
 (数学意义上的基本的四则运算符号的定义)



### Methods

#### Evaluate
```csharp
Microsoft.VisualBasic.Mathematical.BasicR.Helpers.MatrixMathArithmetic.Evaluate(System.Double,System.Double,System.Char)
```
Do a basically arithmetic calculation.
 (进行一次简单的四则运算)

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|
|o|Arithmetic operator(运算符)|


#### Factorial
```csharp
Microsoft.VisualBasic.Mathematical.BasicR.Helpers.MatrixMathArithmetic.Factorial(System.Double,System.Double)
```
Calculate the factorial value of a number, as this function is the part of the arithmetic operation 
 delegate type of 'System.Func(Of Double, Double, Double)', so it must keep the form of two double 
 parameter, well, the parameter 'b As Double' is useless.
 (计算某一个数的阶乘值，由于这个函数是四则运算操作委托'System.Func(Of Double, Double, Double)'中的一部分，
 故而本函数保持着两个双精度浮点型数的函数参数的输入形式，也就是说本函数的第二个参数'b'是没有任何用途的)

|Parameter Name|Remarks|
|--------------|-------|
|a|The number that will be calculated(将要被计算的数字)|
|b|Useless parameter 'b'(无用的参数'b')|


_returns: 
 Return the factorial value of the number 'a', if 'a' is a negative number then this function 
 return value 1.
 (函数返回参数'a'的阶乘计算值，假若'a'是一个负数的话，则会返回1)
 _


### Properties

#### Arithmetic
+-*/\%^!
#### DOUBLE_NUMBER_REGX
A string constant RegularExpressions that stands a double type number.
 (一个用于表示一个双精度类型的实数的正则表达式)
#### OPERATORS
A string constant that enumerate all of the arithmetic operators.
 (一个枚举所有的基本运算符的字符串常数)
