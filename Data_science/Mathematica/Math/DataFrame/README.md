# 带标签数据矩阵、相关性与距离工具

## 引言

数值线性代数库在 .NET 里并不少见，但它们都有一个共同的短板：**矩阵没有标签**。

一旦矩阵来自真实数据表，第 3 行是什么样本、第 7 列是什么特征，都必须记住——这在分析过程中极易出错。本包的做法是**把标签与矩阵绑定在一起**：

- 相关矩阵的行列名来自特征名；
- 距离矩阵的行列名来自样本名；
- 缩放 / 中心化 / 缺失值填补的统计量也随标签记录。

## 核心能力

| 能力 | 说明 |
|---|---|
| **带标签数值矩阵** | 行列均带名称的矩阵模型，运算结果自动继承标签 |
| **成对相关矩阵** | 计算特征间相关系数矩阵 |
| **成对距离矩阵** | 计算样本间距离矩阵 |
| **缺失值填补** | 缺失值的插补处理，避免下游算法报错 |
| **缩放与中心化** | 为多变量分析准备矩阵（标准化 / 均值中心化） |
| **Matrix Market I/O** | 读写 MTX / RUA 格式，与外部数值工具交换矩阵 |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.Matrix`（根） | 带标签矩阵模型、相关 / 距离矩阵、缺失值填补与变换 |
| `....Math.Matrix.MatrixMarket` | Matrix Market 文件格式读写 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Matrix

' 1. 由数据表建立带标签矩阵
Dim m = Matrix.FromDataFrame(frame)

' 2. 缺失值填补 + 标准化
Dim clean = m.ImputeMissing()
Dim scaled = clean.Scale()

' 3. 相关矩阵（行列名自动来自特征名）
Dim corr = scaled.CorrelationMatrix()
Console.WriteLine(corr("featureA", "featureB"))

' 4. 与外部工具交换
Call MatrixMarket.Write(corr, "./corr.mtx")
```

## 实现要点

- **为什么标签如此重要**：相关矩阵的 (i, j) 元素没有标签就毫无意义；把标签纳入类型系统后，「按名字取相关系数」成为一次 O(1) 查询，而不是「数第几行第几列」的易错操作。
- **缺失值的处理顺序**：必须**先填补再缩放**——若先缩放，缺失值会污染均值与标准差，导致所有样本的标准化结果都偏移。
- **Matrix Market 的用途**：它是稀疏矩阵交换的事实标准，被 MATLAB、SciPy、PETSc 等广泛支持；能读写它意味着数据可以在 .NET 与主流数值生态之间往返。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Matrix`
- TargetFramework：`net10.0`
- Tags：`scibasic;data-matrix;correlation;distance-matrix;matrix-market;imputation;scaling;centering`
- 许可：GPL-3.0-or-later
