# 强化学习与标注数据集的序列化扩展

## 引言

模型训练完成后，「结果怎么存」决定了后续能做什么分析。本包提供三组针对性的序列化能力，全部服务于机器学习的**可复现与可比较**：

| 能力 | 输出格式 | 用途 |
|---|---|---|
| Q 表 + 状态特征导出 | netCDF | 检查已学习的价值函数，跨运行比较 |
| 逐轮 Q 值曲线记录 | CSV | 收敛性诊断 |
| 带标签聚类矩阵保存 / 载入 | 紧凑二进制 | 保留聚类分配与标签的对应关系 |

## 为什么不用「一律存 CSV」

| 数据 | CSV 的问题 | 采用方案 |
|---|---|---|
| Q 表 | 多维结构（状态 × 动作）在 CSV 中难以表达 | netCDF（原生多维数组） |
| 聚类矩阵 | 浮点精度易在文本化时损失，体积也显著膨胀 | 二进制格式 |

## 核心能力

- **Q 表导出**：把训练好的 Q 表与其状态特征一起写入 netCDF；
- **Q 值曲线**：逐轮记录 Q 值到 CSV，用于判断是否收敛、是否震荡；
- **聚类矩阵往返**：以紧凑二进制保存 / 载入带标签的聚类矩阵。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Data

' 1. 导出 Q 表（含状态特征）
Call QTable.Export(q, "./qtable.nc", stateFeatures:=features)

' 2. 逐轮记录 Q 值曲线
For episode As Integer = 1 To episodes
    Call q.Step()
    Call QCurve.Record(episode, q.MeanValue(), "./qcurve.csv")
Next

' 3. 保存 / 载入带标签聚类矩阵
Call ClusteringMatrix.Save("./clusters.bin", labels, matrix)
Dim loaded = ClusteringMatrix.Load("./clusters.bin")
```

## 实现要点

- **为什么 Q 表要存成 netCDF**：Q 表本质是二维数组（状态 × 动作），且真值本身就是连续浮点数；netCDF 直接表达多维数组，且被 R / Python 生态广泛支持，便于跨工具检查。
- **收敛诊断为什么重要**：Q 学习的收敛不是单调的；只看最终 Q 值无法判断是「已收敛」还是「在震荡中偶然取到该值」。逐轮曲线才能说明问题。
- **二进制保存聚类结果的收益**：聚类矩阵往往很大（样本 × 特征），文本化会同时带来体积膨胀与精度损失；二进制往返则完全保真。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.Data`
- TargetFramework：`net10.0`
- Tags：`scibasic;machine-learning;q-learning;q-table;dataset;serialization;netcdf;binary-format`
- 许可：GPL-3.0-or-later
