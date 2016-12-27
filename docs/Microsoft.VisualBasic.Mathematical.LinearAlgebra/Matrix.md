# Matrix
_namespace: [Microsoft.VisualBasic.Mathematical.LinearAlgebra](./index.md)_



> 
>  Matlab里常用的矩阵运算函数  
>  
>  %假设矩阵为A
>  
>  + det(A)   求矩阵行列式
>  + eig(A)   求矩阵特征值或特征向量
>  + inv(A)   矩阵A求逆
>  + pinv(A)  矩阵A求伪逆
>  + rank(A)  求矩阵A的秩
>  + svd(A)   求矩阵A的奇异值或进行奇异值分解
>  + gsvd(A)  求矩阵A的广义奇异值
>  + trace(A) 求矩阵A的迹
>  + schur(A) 对矩阵A进行Schur分解
>  + hess(A)  求矩阵A的Hessenburg标准型
>  + cond(A)  求矩阵A的范数
>  + chol(A)  对矩阵A进行Cholesky分解
>  + lu(A)    对矩阵A进行lu分解
>  + qr(A)    对矩阵A进行QR分解
>  + poly(A)  求矩阵A的特征多项式
>  


### Methods

#### Number
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.Number
```
获取仅包含有一个元素的矩阵对象

#### op_Addition
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.op_Addition(System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
实数加矩阵算符重载，各分量分别加实数

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|a1|-|


#### op_BitwiseOr
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.op_BitwiseOr(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
矩阵乘以向量(线性变换），即 b=Ax

|Parameter Name|Remarks|
|--------------|-------|
|A|-|
|x|-|


#### op_Division
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.op_Division(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Double)
```
矩阵除以实数算符重载，各分量分别除以实数

|Parameter Name|Remarks|
|--------------|-------|
|a1|-|
|x|-|


#### op_Multiply
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.op_Multiply(System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
实数乘矩阵算符重载，各分量分别乘以实数

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|a1|-|


#### op_Subtraction
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.op_Subtraction(System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
实数减矩阵算符重载，各分量分别减实数

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|a1|-|


#### Resize
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.Resize(System.Int32,System.Int32)
```
调整矩阵的大小，并保留原有的数据

|Parameter Name|Remarks|
|--------------|-------|
|m|-|
|n|-|


#### Transpose
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix.Transpose
```
获取当前的矩阵对象的转置矩阵


### Properties

#### GetSize
获取矩阵行数
