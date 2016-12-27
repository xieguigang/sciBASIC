# StackTokens`1
_namespace: [Microsoft.VisualBasic.Scripting.TokenIcer](./index.md)_

进行栈树解析所必须要的一些基本元素



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.StackTokens`1.#ctor(System.Func{`0,`0,System.Boolean})
```
Tokens equals?

|Parameter Name|Remarks|
|--------------|-------|
|equals|-|


#### Equals
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.StackTokens`1.Equals(`0,`0)
```
Tokens equals?

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|



### Properties

#### LPair
向下一层堆栈符号，一般是左括号
#### ParamDeli
参数的分隔符，一般是逗号
#### Pretend
Pretend the root tokens as a true node
#### RPair
向上一层出栈符号，一般是右括号
