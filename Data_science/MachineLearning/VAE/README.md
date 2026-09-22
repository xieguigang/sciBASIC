# 变分自编码器：图 VAE、高斯混合 VAE 与 GMM

## 引言

**自编码器**压缩再重建，但它的潜在空间是「散乱」的——随机采一个点往往解码不出合理结果。**变分自编码器（VAE）**通过让编码器输出**分布**（均值与方差）而非单点，并对潜在空间施加先验约束，使得潜在空间变得**连续且可采样**。

本包提供三类相关模型，并让潜在空间变得**可解释**。

## 三个模型

| 类型 | 结构 | 特点 |
|---|---|---|
| **`GraphVAE`** | GCN 编码器 + 内积解码器 | 面向**图结构**的生成式建模：把整图压入连续潜在空间 |
| **高斯混合 VAE** | 类别式潜在变量 | 潜在变量是**离散的类别分配**，因此潜在空间直接对应聚类 |
| **GMM** | EM 训练的混合高斯 | 经典软聚类方法，无需神经网络 |

## 高斯混合 VAE 为什么特别有价值

普通 VAE 的潜在表示是一团连续的概率云，难以解释；高斯混合 VAE 用**类别潜在变量**（categorical latent），相当于让模型自己学习「这个样本属于哪个成分」。因此：

- 潜在变量即**软聚类分配**；
- 生成时可以**指定成分**采样（条件生成）；
- 聚类结果与生成模型**共享同一套参数**，一致性更好。

## 数学工具（`MathUtils`）

VAE 的成功率高度依赖数值稳定性，本包提供的辅助包括：

| 工具 | 解决的问题 |
|---|---|
| **log-sum-exp** | 在对数域做加法而不下溢（KL 与似然计算的基础） |
| **KL 散度** | 变分下界中的正则项 |
| **重参数化采样** | 让「从分布采样」这一步对参数可微（VAE 可训练的前提） |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder

' 1. 图变分自编码器
Dim gvae As New GraphVAE(latentDim:=32)
Call gvae.Train(graphDataset, epochs:=100)

Dim z = gvae.Encode(graph)
Dim reconstructed = gvae.Decode(z)

' 2. 高斯混合 VAE（潜在成分 = 软聚类）
Dim gmvae As New GaussianMixtureVAE(components:=8, latentDim:=16)
Call gmvae.Train(data, epochs:=150)

Dim assignment = gmvae.ClusterAssignments(data)
Dim generated = gmvae.Sample(component:=3, n:=10)

' 3. 经典 GMM（EM 求解，无需神经网络）
Dim gmm As New GaussianMixture(components:=5)
Call gmm.Fit(data)
```

## 实现要点

- **重参数化技巧为什么关键**：采样本身是随机的、不可微的，无法反传。重参数化把采样写成 `z = μ + σ·ε`（ε 来自标准正态），于是梯度可以穿过 μ 与 σ——这是 VAE 能端到端训练的根本原因。
- **KL 项的双重作用**：它既约束潜在分布接近先验（保证潜在空间规整、可采样），也起到正则化作用（防止编码器把每个样本编码到互不相干的点上）。
- **GCN 编码器的意义**：图结构数据的「重建」不是恢复像素，而是恢复**邻接关系**；内积解码器正是用两个节点向量的点积预测它们之间是否存在边。
- **与 `GNN` 包的关系**：`GNN` 面向判别任务（分类、预测），本包的 `GraphVAE` 面向生成任务（重建、采样）；两者共享图卷积的思想但目标不同。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder`
- TargetFramework：`net10.0`
- Tags：`scibasic;variational-autoencoder;graph-vae;gaussian-mixture;gmm;generative-model;latent-space;clustering`
- 许可：GPL-3.0-or-later
