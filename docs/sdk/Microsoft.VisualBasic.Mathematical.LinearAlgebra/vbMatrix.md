# vbMatrix
_namespace: [Microsoft.VisualBasic.Mathematical.LinearAlgebra](./index.md)_





### Methods

#### Adj
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Adj(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵伴随矩阵

|Parameter Name|Remarks|
|--------------|-------|
|K|目标方阵|
|n|方阵K的阶数|
|Ret|获得的伴随矩阵|

> 
>  函数采用求代数余子式的方式进行求解,这样就存在一个问题,当目标矩阵的阶数很大的时候,本函数效率是相当慢的。
>  建议使用左连翠提出的《伴随矩阵的新求法》里的方法进行求解。里面的方法可以求解非满秩矩阵的伴随矩阵。
>  

#### Cond
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Cond(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
矩阵范数Cond及

|Parameter Name|Remarks|
|--------------|-------|
|k|目标矩阵|
|m|矩阵的行数|

> 
>  函数运行原理是先求矩阵的奇异值,然后用最大的奇异值除以最小的奇异值即得矩阵的范数.对于只有1行或者1列的还得另行处理.这个函数和Matlab的Cond命令一样,即2范数
>  

#### Cramer22
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Cramer22(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
求Kx=B的最小二乘解

|Parameter Name|Remarks|
|--------------|-------|
|K|是x的系数矩阵|
|B|是等式右边的常数矩阵|
|k_m|矩阵K的行数|
|x|求解得到的解|


#### Det2
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Det2(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
求行列式

|Parameter Name|Remarks|
|--------------|-------|
|k|所求的n阶方阵|
|N|方阵K的阶数|


_returns: 函数成功返回其行列式的大小_

#### DetF
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.DetF(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
求行列式，函数执行成功返回其行列式大小.其原理是按行列式定义依次展开求解.不适合大于5阶的方阵，K的数组大小为N*N的,不然程序出错

|Parameter Name|Remarks|
|--------------|-------|
|k|为n阶方阵|
|N|为矩阵A的阶数|


#### DFT
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.DFT(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
离散傅里叶变换

|Parameter Name|Remarks|
|--------------|-------|
|k|m*2的矩阵数据(数据点)K里的第一列代表数据的实数部分,第2列代表数据的虚数部分|
|m|矩阵k的行数|
|Number|离散点数|
|X|离散傅里叶变换的结果矩阵是Number*2的矩阵,X里的第一列代表数据的实数部分,第2列代表数据的虚数部分|


_returns: 本函数执行成功返回True.本函数相当于Matlab的快速傅里叶变换函数FFT_

#### EigenValue
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.EigenValue(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,System.Int16,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Boolean)
```
方阵求特征值

|Parameter Name|Remarks|
|--------------|-------|
|K11|要求特征值的方阵|
|n|方阵K1的阶数|
|LoopNumber|循环次数|
|Errro|误差控制变量|
|Ret|返回的特征值,Ret是是n*2的数组,第一列是实数部分,第2列为虚数部分|
|IsHess|K1是否已经是上Hessenberg矩阵|


#### EigSym
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.EigSym(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
求对称方阵特征值

|Parameter Name|Remarks|
|--------------|-------|
|A|对称方阵|
|n|方阵A的阶数|
|Erro1|误差控制变量|
|Ret|返回的特征值|
|Ret_Eigenvectors|返回的特征值对应的特征向量|

> 本代码采用雅可比过关法求解

#### EigTorF
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.EigTorF(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵特征值获取特征值向量

|Parameter Name|Remarks|
|--------------|-------|
|A1|目标方阵|
|A_m|矩阵A的行数|
|EigValve|方阵A的一个特征值|
|X|函数执行成功后得到的一个特征向量|

> 
>  函数原理:已知方阵A的一个特征值为r,则求解方程组(A-r*E)*X=0的解X即为我们的一个特征向量(这里E为单位矩阵),
>  我们下面采用的是全选主元素法求解.但是需要注意的是,由于这个方程组是非满秩矩阵,因此在最后处理解的时候,我们
>  总是令X解中的一个量为1(当然,你可以设置为其它数,建议设置为非0的数据),然后根据这个量导出其它的量
>  
>  例子:
>  ```
>  a =
>   [ -1.0000000000000   0.00000000000000   0.00000000000000
>     8.00000000000000   2.00000000000000   4.00000000000000
>     8.00000000000000   3.00000000000000   3.00000000000000 ]
> 
>   Math_Matrix_EigTor(a,3,6,x)'上面矩阵a的一个特征值为6,则我们执行如下的命令后求得6的特征向量x如下
>  x =
>   [ 0.00000000000000
>     1.00000000000000
>     1.00000000000000 ]
>  ```
>  

#### GetRank
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.GetRank(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16)
```
矩阵求秩，函数执行成功返回秩的大小

|Parameter Name|Remarks|
|--------------|-------|
|K|要求秩的矩阵|
|error_|误差控制参数|


#### Hamiltonian
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Hamiltonian(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
构建哈密顿矩阵

|Parameter Name|Remarks|
|--------------|-------|
|k|m阶的对称矩阵|
|m|矩阵k的行数|
|ret|获得的关于矩阵K的Hamiltonian矩阵|


#### Hessenberg
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Hessenberg(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
将方阵化为上(Hessenberg)矩阵，函数成功返回Ret的阶数

|Parameter Name|Remarks|
|--------------|-------|
|A|要化为上(Hessenberg)矩阵的矩阵|
|n|为方阵A的阶数|
|ret|化为上(Hessenberg)矩阵后的矩阵|


_returns: 函数成功返回Ret的阶数_

#### IDFT
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.IDFT(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
离散傅里叶变换逆变换

|Parameter Name|Remarks|
|--------------|-------|
|k|m*2的矩阵数据(数据点)K里的第一列代表数据的实数部分,第2列代表数据的虚数部分|
|m|矩阵k的行数|
|Number|离散点数|
|X|离散傅里叶变换逆变换的结果矩阵是Number*2的矩阵,X里的第一列代表数据的实数部分,第2列代表数据的虚数部分|


_returns: 本函数执行成功返回True.本函数相当于Matlab的快速傅里叶变换逆变换函数IFFT_

#### Inv
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Inv(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵求逆

|Parameter Name|Remarks|
|--------------|-------|
|K|为要求逆的方阵|
|Return_K|为所求得的逆|


#### Inv2
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Inv2(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
矩阵求逆

|Parameter Name|Remarks|
|--------------|-------|
|K|目标方阵|
|Return_K|求得的逆矩阵|
|N|方阵K的阶数|


#### Lehmer
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Lehmer(System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
构建Lehmer矩阵

|Parameter Name|Remarks|
|--------------|-------|
|n|构建Lehmer矩阵的阶数|
|k|构建的Lehmer矩阵|

> Lehmer Matrix

#### LLt
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.LLt(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Boolean)
```
矩阵的LLt分解

|Parameter Name|Remarks|
|--------------|-------|
|A|要进行LLt分解的方阵|
|L|分解得到的L方阵|
|is1_是否已经正定|-|


_returns: 函数成功返回True,失败返回False.(其中Lt是L的转置,即分解后 A=L×Lt)_

#### LU
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.LU(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
方阵LU分解

|Parameter Name|Remarks|
|--------------|-------|
|K|为要LU分解的方阵|
|n|方阵K的阶数|
|L|为分解得到的L矩阵|
|U|为分解得到的U矩阵|


_returns: 其意义是K=LU.函数执行成功返回True,失败返回False_

#### Magic
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Magic(System.Int32,System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
幻方

|Parameter Name|Remarks|
|--------------|-------|
|n|幻方的阶数(大于2)|
|start|幻方的中最小的正整数,一般可以设置为1|
|k|获得的幻方|


#### Mul
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Mul(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵相乘

|Parameter Name|Remarks|
|--------------|-------|
|K1|K1为矩阵乘法中左边的矩阵|
|K2|为矩阵乘法中右边的矩阵|
|n|代表K1的列数,K2的行数|
|Return_K|执行成功后返回的乘的结果的矩阵|


#### Orth
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Orth(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
求矩阵的一个正交基Orth

|Parameter Name|Remarks|
|--------------|-------|
|k|目标矩阵|
|m|k的行数|
|ret|获得的一个正交基矩阵|


_returns: 函数失败返回小于1的数据，成功返回ret的行数_
> 对矩阵进行svd分解即用SvdSplit得到k=usv*,则s是奇异值矩阵,可以奇异值是否为0获得矩阵的秩r,然后ret就是m*r的矩阵且其就是u里的m*r的部分值

#### Pascal
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Pascal(System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
n阶帕斯卡(Pascal)矩阵

|Parameter Name|Remarks|
|--------------|-------|
|n|表示产生帕斯卡(Pascal)矩阵的阶数|
|k|产生的n阶帕斯卡(Pascal)矩阵|

> Pascal Matrix即产生n阶的帕斯卡矩阵由杨辉三角形表组成的矩阵称为帕斯卡(Pascal)矩阵

#### Pinv
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Pinv(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵的广义逆A+ ，返回m*n矩阵Return_K(,)的m。此广义逆是Moore-Penrose A+逆

|Parameter Name|Remarks|
|--------------|-------|
|K|要求广义逆的矩阵|
|Return_K|求得的广义逆矩阵|


_returns: 函数执行成功返回m,其中m代表Return_K的行数_

#### Pinv2
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Pinv2(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵的广义逆A-，函数执行成功返回Ret的行数,出错返回0

|Parameter Name|Remarks|
|--------------|-------|
|K|要求广义逆的矩阵|
|Erro|误差控制参数|
|m|矩阵K的行数|
|Ret|求得的广义逆矩阵|


#### PolyDiv
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyDiv(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
多项式除法

|Parameter Name|Remarks|
|--------------|-------|
|A1|被除数存储多项式系数|
|A2|除数存储多项式系数|
|RetMod|求得的余数多项式系数|
|Ret|求得的多项式商系数|
|Erro|误差控制参数|

> A1/A2=Ret……RetMod

#### PolyDivEx
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyDivEx(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
多项式除法

|Parameter Name|Remarks|
|--------------|-------|
|A1|被除数存储多项式系数|
|A2|除数存储多项式系数|
|RetMod|求得的余数多项式系数|
|Ret|求得的多项式商系数|
|Erro|误差控制参数|

> A1/A2=Ret……RetMod

#### PolyGCF
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyGCF(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
多项式提取最大公因式

|Parameter Name|Remarks|
|--------------|-------|
|A1|1*A1_n的存储多项式系数的矩阵|
|A1_n|A1的列数|
|A2|为1*A2_n的存储多项式系数的矩阵|
|A2_n|A2的列数|
|Ret|获得的最大公因式多项式系数|
|Erro|误差控制参数|


#### PolyGCFCall
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyGCFCall(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
求2个多项式的最大公因式Ret，A1为1*A1_n的矩阵，A2为1*A2_n的矩阵。函数执行后返回公因式Ret的大小

|Parameter Name|Remarks|
|--------------|-------|
|A1|-|
|A1_n|-|
|A2|-|
|A2_n|-|
|Ret|-|
|Erro|-|


#### PolyMod
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyMod(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32)
```
多项式求余数

|Parameter Name|Remarks|
|--------------|-------|
|A1|被除数多项式系数|
|A2|除数多项式系数|
|Ret|求得的余数多项式系数|
|Erro|误差控制参数|

> A1%A2=Ret

#### PolyMul
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyMul(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
多项式乘法

|Parameter Name|Remarks|
|--------------|-------|
|Mul1|乘数多项式系数|
|Mul2|乘数多项式系数|
|Ret|获得的乘积结果多项式系数|

> Ret=Mul1*Mul2

#### PolyRoots2
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.PolyRoots2(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,System.Int16,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
求多项式复数根贝尔斯托(Bairstow)算法

|Parameter Name|Remarks|
|--------------|-------|
|A|多项式系数矩阵,为1*A_n的矩阵。A中的数据依次为多项式最高项系数,次高项系数……常数项系数|
|A_n|A矩阵的列数或大小|
|LoopNumber|控制的循环次数|
|Erro|误差控制变量|
|Ret|返回的一个n*2的矩阵|


_returns: 函数执行完毕返回Ret的行数_
> 
>  对于多项式f(x)=(x^2+2x+3)(x^2-5x+9)=x^4-3x^3+2x^2+3x+27,则A(0,0)=1,A(0,1)=-3,A(0,2)=2,A(0,3)=3,A(0,4)=27,A_n=5.
>  当执行下面的函数后,Ret是一个2×2的矩阵,即Ret(0,0)=2,Ret(0,1)=3,Ret(0,0)的2对应于(x^2+2x+3)当中2x的2,Ret(0,1)的3
>  对应于(x^2+2x+3)当中常系数的3.用此函数前建议先把重根与实数根处理掉
>  

#### Pow
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Pow(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
方阵求n次方

|Parameter Name|Remarks|
|--------------|-------|
|A|目标方阵|
|m|方阵A的阶数|
|n|方阵A要求的次方数|
|Ret|方阵A进行n次方后获得的返回值|

> 
>  注意,本代码没有采用特征值法。而是直接采用2个矩阵相乘的方法(但又不是老老实实地去乘n次),因为用程序去求一个方阵的特征值,
>  可能运算复杂度超过了你直接对矩阵相乘的复杂度,至少在n在1000以内大概是这样。
>  

#### QR
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.QR(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
方阵的QR分解

|Parameter Name|Remarks|
|--------------|-------|
|K|要QR分解的矩阵，K必须是非奇异的n阶方阵|
|Q|分解后的Q矩阵|
|R|分解后的R矩阵|


_returns: 函数执行成功返回True,失败返回False_

#### QR2
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.QR2(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,System.Int16)
```
矩阵的QR分解

|Parameter Name|Remarks|
|--------------|-------|
|A|要QR分解的矩阵(不一定是方阵)|
|Q|分解得到的Q矩阵|
|R|分解得到的R矩阵|
|Q_n|返回Q矩阵的列数|
|R_n|返回R矩阵的列数|


_returns: 函数成功返回True,失败返回False.使用本函数时,A矩阵的行数不能小于列数_

#### QR22
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.QR22(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,System.Int16)
```
矩阵的QR分解

|Parameter Name|Remarks|
|--------------|-------|
|A|要QR分解的矩阵（不一定是方阵）|
|Q|分解得到的Q矩阵|
|R|分解得到的R矩阵|
|Q_n|返回Q矩阵的列数|
|R_n|返回R矩阵的列数|


#### RU
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.RU(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
右极分解，即F=R*U

|Parameter Name|Remarks|
|--------------|-------|
|F|目标方阵|
|n|方阵F的阶数|
|R|分解得到的一个正交矩阵|
|U|分解得到的一个对称正定矩阵|

> 
>  原理:任何一个可逆方阵均可以唯一的进行右极分解与左极分解,即F=R*U=V*R,其中U^2=T(F)*F,V^2=F*T(F)
> 【其中T(F)表示F的转置】,则我们可以先通过F求得U或V,然后求R=F*Inv(U)=Inv(V)*F
>  
>  例子:
>  a =
>   [ 67.5919611787386     69.8554906388072     38.8768396987006     89.3106376236820
>     17.0671848194055     1.12767200969517     31.5601159499772     96.9140055109346
>     40.6681714768839     51.0876563615574     86.9885893943666     77.3506165842296
>     73.6101518727886     87.9281915202402     23.9508483670423     3.45968334165387 ]
>  
>  Math_Matrix_RU(a,4,r,u)'进行右极分解得到如下结果
>  r =
>   [ -0.01806739003090   0.71913865214108  -0.27739456376250   0.63664949766408
>      0.45823822484909  -0.57214688695250   0.04612554269224   0.67904624212867
>     -0.09675742692290   0.20970810955629   0.95672796354849   0.17708590026142
>      0.88326786307616   0.33437989881145   0.07527225185216  -0.31939364294910  ]
>  u =
>   [ 67.6841644139219  71.9849022272286  26.4791752992667  38.3171883770002
>     71.9863924990090  89.6984480926248  36.1676400008128  26.2017432919420
>     26.4820423188876  36.1668446729677  75.6890493922576  53.9420595009903
>     38.3140405638833  26.2042694530458  53.9408769958847  135.276124831623  ]
> 
>  Math_Matrix_VR(a,4,v,r)'进行左极分解得到如下结果
>  v =
>   [ 95.0902981485406     53.4500420292151     61.1182769307013     57.4613130458042
>     53.4438098825046     74.4408296746995     45.9449363126334    -13.1234428571640
>     61.1187415068155     45.9418557217637     103.699821171875     34.8450484220080
>     57.4619847804705    -13.1244040926920     34.8441839555448     95.1168377384350 ]
>  r =
>   [ -0.01835015300540     0.71939575068999    -0.27678823268990     0.63628327754090
>      0.45799761221593    -0.57191020194030     0.04534571824682     0.67953103744654
>     -0.09684035444340     0.20977163011655     0.95676617484504     0.17707600892713
>      0.88364856259618     0.33406264059053     0.07478222238892    -0.31912379017620 ]
>  

#### Scatter
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Scatter(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
构建散点图矩阵(Scatter Matrix)

|Parameter Name|Remarks|
|--------------|-------|
|X|目标矩阵|
|m|X矩阵的行数|
|S|获得的散点矩阵|


#### Schmidt
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Schmidt(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵施密特(Schmidt)正交规范化

|Parameter Name|Remarks|
|--------------|-------|
|K|要施密特(Schmidt)正交规范化的矩阵|
|Ret|正交规范化后的矩阵|


_returns: 函数执行成功返回True,失败返回False_

#### SG
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.SG(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵的满秩分解Math_Matrinx_SG，把矩阵K分解成一种行满秩Return_m 是m*r与列满秩的矩阵Return_n是r*n.返回值为r.r是其秩

|Parameter Name|Remarks|
|--------------|-------|
|K|为要满秩分解的方阵|
|Return_M|所求得的m*r矩阵|
|Return_N|所求得的r*n矩阵|

> 
>  其中A为m*n的矩阵,r为A的秩.即A=Return_M*Return_N.函数执行成功返回r(也就是其秩)
>  

#### Sove2
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Sove2(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
高斯全选主元素法解方程，本函数是求解AX=B这类问题的。函数采用全选主元素的高斯消元法，对于出现非满秩矩阵时(A的化简过程中的A)，
 只要函数有解(可能不止一组解,此时只返回一组解)，本函数都能返回其解

|Parameter Name|Remarks|
|--------------|-------|
|A|A_m*n的矩阵|
|b|B_m*1的矩阵|
|A_m|-|
|B_m|-|
|X|求解得到的矩阵|

> 
>  例子:
>  a =
>   [ 89.7234413259306  12.9170338217714  79.9443395249286  78.1627263772128
>     62.8960442556516  63.9951517172135  2.9257326400493   57.119458800703
>     83.5902038885235  55.9411662425572  89.4671598865963  33.7297967792162 ]
>  
>  b =
>   [ 65.2027291083721
>     54.2041894766522
>     63.722165657078   ]
>  
>  经过本函数后得到的解如下
>  x =
>   [ -0.826689550370445
>     0.737377350436936
>     0.646558671079671
>     1                 ]
>  
>  即AX=B
>  

#### SPD
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.SPD(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵正规、对称、正定性判断

|Parameter Name|Remarks|
|--------------|-------|
|K|为要判断的矩阵|


_returns: 函数返回-1矩阵非对称矩阵,返回0矩阵不正定,返回1矩阵正定_

#### Sqrt
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Sqrt(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵求平方根（sqrtm）

|Parameter Name|Remarks|
|--------------|-------|
|K|目标方阵|
|n|方阵K的阶数|
|ks|求得的平方根.即ks*ks=K|

> 
>  如果K可以化成K=Inv(P)*diag(R)*P,其中Inv(P)表示P的逆矩阵,diag(R)*为K的特征值组成的对角矩阵,
>  那么ks=Inv(P)*diag(R^0.5)*P,根据对角化原理,P*K*Inv(P)=Diag(R),其中Inv(P)是特征值R对应于K
>  的特征向量,因此我们的算法=求特征值R,如果所有R均为正实数,则求R对应的特征向量Inv(P),然后讲R每
>  个值取根放入对角矩阵对结果相乘即可
>  
>  例子:
>  ```
>  c =
>   [  192.291902022941   136.423323830855  -22.2582056347830   10.9878603820001
>     -176.869155076020  -120.047935463800   20.4023293672721  -16.5962890811120
>     -21.6722775306690  -60.5101175154120   135.025037886378   5.36535497517843
>      31.2279467353500   93.4954928282741  -106.961070363850   59.2865617399033  ]
>   
>  Math_Matrix_Sqrt(a,4,x)'求a平方根如下,可以进行x*x进行验证
>  x =
>   [  18.0067271094031   10.1514259204440  -0.80764239842560   0.96148324464486
>     -13.6679053566890  -5.69822839694800   0.61394536284630  -1.38430646207250
>     -3.17102914065820  -5.10751280313280   11.6191076697081   0.04254172112235
>      4.81532683312183   7.92820343443191  -5.38805756370240   8.16305909603188  ]
>  ```
>  

#### Svd
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.Svd(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
矩阵奇异值

|Parameter Name|Remarks|
|--------------|-------|
|A|为目的矩阵|
|m|为A矩阵的行数|
|Ret|获取到的奇异值矩阵,即返回的Ret是m*1的矩阵|


_returns: 函数执行成功返回奇异值的个数,即Ret的行数,失败返回-1_

#### SvdSplit
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.SvdSplit(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16)
```
对矩阵A进行奇异值分解

|Parameter Name|Remarks|
|--------------|-------|
|A|目标矩阵|
|m|A矩阵的行数|
|V|分解得到的一个V矩阵|
|V_m|V矩阵的行数|
|S|分解得到的一个S矩阵|
|S_m|S矩阵的行数|
|U|分解得到的一个U矩阵|
|U_m|U矩阵的行数|


#### SymTridMatrix
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.SymTridMatrix(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int16,System.Boolean,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
实对称阵化为对称三对角阵

|Parameter Name|Remarks|
|--------------|-------|
|A|目标方阵|
|n|方阵A的阶数|
|Is对称|不确定是否对称直接填False,对称则直接填True|
|ret|返回的三对角阵|

> 本函数采用用豪斯赫尔蒙德变换将实对称阵化为对称三对角

#### VR
```csharp
Microsoft.VisualBasic.Mathematical.LinearAlgebra.vbMatrix.VR(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix)
```
左极分解

|Parameter Name|Remarks|
|--------------|-------|
|F|目标方阵|
|n|方阵F的阶数|
|V|分解得到的一个对称正定矩阵|
|R|分解得到的一个正交矩阵，即F=V*R|



