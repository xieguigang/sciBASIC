# crandn
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

正态分布随机数



### Methods

#### rand
```csharp
Microsoft.VisualBasic.Mathematical.crandn.rand(System.Int32,System.Int32)
```
混合同余法产生[0，1]均匀分布随机数向量

|Parameter Name|Remarks|
|--------------|-------|
|m|向量维数|
|seed|种子|


#### randn
```csharp
Microsoft.VisualBasic.Mathematical.crandn.randn(System.Int32,System.Int32,System.Int32)
```
生成标准正态分布随机矩阵

|Parameter Name|Remarks|
|--------------|-------|
|m|矩阵维数|
|n|矩阵维数|
|seed|混合同余种子|

> 
>  由于矩阵的行和列都可能为奇数，所以先判断总元素
>  的奇偶性，如果是偶数，先生成偶数随机向量，后把
>  随机向量赋值给随机矩阵。
>  如果总元素为奇数，按照VEC rand中类似的方法先
>  生成随机向量，后把向量复制给随机矩阵。
>  


