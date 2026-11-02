# GCN 图卷积神经网络详解
## 一、背景与动机
### 1.1 为什么需要 GCN？
传统 CNN/RNN 处理的是**规则结构数据**（图像的网格、文本的序列），但现实中大量数据是**图结构**：
- 社交网络（用户-好友关系）
- 引文网络（论文-引用关系）
- 分子结构（原子-化学键）
- 知识图谱、交通网络等
这类数据**没有固定拓扑顺序**，无法直接使用传统卷积。GCN 的核心目标：**在图结构数据上定义卷积操作，实现节点特征的聚合与学习**。
### 1.2 图的基本表示
图 $G = (V, E)$：
- $V$：节点集合，$|V| = n$
- $E$：边集合
对应的矩阵表示：
- **邻接矩阵** $A \in \mathbb{R}^{n \times n}$：$A_{ij} = 1$ 表示节点 $i$ 与 $j$ 有边
- **度矩阵** $D$：对角矩阵，$D_{ii} = \sum_j A_{ij}$（节点的度）
- **特征矩阵** $X \in \mathbb{R}^{n \times d}$：每个节点有 $d$ 维特征
---
## 二、谱域视角：GCN 的理论推导（选读，理解来源）
Kipf & Welling (2017) 的 GCN 源自**谱图理论**，下面简述推导脉络。
### 2.1 图拉普拉斯矩阵
$$L = D - A$$
归一化形式：
$$L_{sym} = I - D^{-1/2} A D^{-1/2}$$
拉普拉斯矩阵是图的“二阶差分算子”，刻画了图的平滑性（相邻节点特征差异）。
### 2.2 图傅里叶变换
- 拉普拉斯矩阵可特征分解：$L = U \Lambda U^T$
  - $U$：特征向量矩阵（图傅里叶基）
  - $\Lambda$：特征值（对应“频率”）
- 图信号 $x$ 的傅里叶变换：$\hat{x} = U^T x$
### 2.3 谱卷积定义
卷积定理：空域卷积 = 频域乘积
$$g_\theta * x = U g_\theta(\Lambda) U^T x$$
其中 $g_\theta$ 是可学习的滤波器。
### 2.4 简化之路（Chebyshev → 一阶近似）
**第一代（Spectral CNN）**：直接学习 $g_\theta(\Lambda)$，缺点是参数与图规模绑定、计算量大。
**第二代（ChebNet）**：用切比雪夫多项式 $T_k$ 近似滤波器，限制 $K$ 阶局部性：
$$g_\theta * x \approx \sum_{k=0}^{K-1} \theta_k T_k(\tilde{L}) x, \quad \tilde{L} = \frac{2}{\lambda_{max}}L - I$$
**第三代（Kipf & Welling, ICLR 2017）**：取 $K=1$ 的**一阶近似**，只聚合直接邻居：
$$H^{(l+1)} = \sigma\left(\tilde{D}^{-1/2} \tilde{A} \tilde{D}^{-1/2} H^{(l)} W^{(l)}\right)$$
这就是我们常用的 **GCN 传播公式**。
---
## 三、GCN 核心公式逐项解读
### 3.1 逐层传播规则
$$H^{(l+1)} = \sigma\left(\tilde{D}^{-1/2} \tilde{A} \tilde{D}^{-1/2} H^{(l)} W^{(l)}\right)$$
各项含义：
| 符号 | 含义 |
|------|------|
| $H^{(l)}$ | 第 $l$ 层的节点表示，$H^{(0)} = X$ |
| $\tilde{A} = A + I$ | **加自环的邻接矩阵**——节点也聚合自身特征 |
| $\tilde{D}$ | $\tilde{A}$ 对应的度矩阵 |
| $\tilde{D}^{-1/2}\tilde{A}\tilde{D}^{-1/2}$ | **对称归一化邻接矩阵** $\hat{A}$ |
| $W^{(l)}$ | 第 $l$ 层可学习权重 |
| $\sigma$ | 激活函数，如 ReLU |
### 3.2 每一部分的作用
**① 为什么加自环 $\tilde{A} = A + I$？**
如果不加自环，节点聚合邻居时会“丢失自己”的特征，导致过平滑后节点表示失去个性。加自环保证自身信息保留。
**② 为什么归一化 $\tilde{D}^{-1/2}\tilde{A}\tilde{D}^{-1/2}$？**
写成分量形式，对节点 $i$：
$$h_i^{(l+1)} = \sigma\left(\sum_{j \in \mathcal{N}(i) \cup \{i\}} \frac{1}{\sqrt{\tilde{d}_i}\sqrt{\tilde{d}_j}} h_j^{(l)} W^{(l)}\right)$$
直觉：**度数大的邻居（如大V节点）如果和度数小的邻居权重一样，会淹没信息**。归一化相当于按“邻居重要性”加权：
- 一个节点的度越大，它传出的每条边权重越小（它的信息要“分摊”给很多人）
- 类似一种隐式的注意力/加权平均
对比未归一化的简单聚合 $AH$：度数大的节点特征数值会异常大，训练不稳定。
**③ 为什么叫“卷积”？**
- 传统 CNN：3×3 卷积核在图像上滑动，聚合局部邻居像素
- GCN：每个节点聚合它的图邻居，$\hat{A}$ 决定了“聚合谁”，$W$ 决定了“怎么变换”
- 一层 GCN = 一阶邻居聚合；堆叠 $K$ 层 = 感受野扩展到 $K$ 阶邻居（类似 CNN 的深层感受野）
### 3.3 前向计算示例（两层 GCN 做节点分类）
$$Z = f(X, A) = \text{softmax}\left(\hat{A}\, \text{ReLU}\left(\hat{A} X W^{(0)}\right) W^{(1)}\right)$$
**计算流程：**
```
输入 X (n×d), A (n×n)
    │
    ▼
预计算 Â = D̃^(-1/2) (A+I) D̃^(-1/2)
    │
    ▼
第一层: H¹ = ReLU(Â X W⁰)     → 每个节点聚合1阶邻居
    │
    ▼
第二层: Z = softmax(Â H¹ W¹)  → 聚合2阶邻居
    │
    ▼
输出: 每个节点的类别分布 (n×C)
```
**损失函数**（半监督节点分类，只用有标签节点）：
$$\mathcal{L} = -\sum_{l \in \mathcal{Y}_L} \sum_c Y_{lc} \ln Z_{lc}$$
关键点：即使只有部分节点有标签，由于所有节点都参与消息传递，**无标签节点的特征和结构信息也被利用了**（归纳偏置：“相连节点倾向相似”）。
---
## 四、一个直观的小例子
设 4 个节点的图，节点 1 连接节点 2、3：
```
    (2)
     |
    (1)——(3)
     |
    (4)
```
节点 1 的新表示（一层 GCN）：
$$h_1^{new} = \sigma\left(\frac{1}{\sqrt{4}}\left(h_1 + \frac{h_2}{\sqrt{2}} + \frac{h_3}{\sqrt{2}} + \frac{h_4}{\sqrt{2}}\right) W\right)$$
（$\tilde{d}_1 = 4$ 含自环，$\tilde{d}_2 = \tilde{d}_3 = \tilde{d}_4 = 2$）
可见：**每个节点的新表示 = 自身与邻居的加权平均，再经过线性变换和激活**。
---
## 五、训练与实现
### 5.1 PyTorch 简洁实现
```python
import torch
import torch.nn as nn
import torch.nn.functional as F
class GCNLayer(nn.Module):
    def __init__(self, in_dim, out_dim):
        super().__init__()
        self.linear = nn.Linear(in_dim, out_dim)
    def forward(self, X, A_hat):
        # A_hat = D̃^(-1/2)(A+I)D̃^(-1/2)，预先计算
        return torch.matmul(A_hat, self.linear(X))
class GCN(nn.Module):
    def __init__(self, in_dim, hidden_dim, num_classes):
        super().__init__()
        self.gcn1 = GCNLayer(in_dim, hidden_dim)
        self.gcn2 = GCNLayer(hidden_dim, num_classes)
    def forward(self, X, A_hat):
        H = F.relu(self.gcn1(X, A_hat))
        return F.log_softmax(self.gcn2(H, A_hat), dim=1)
# 预计算归一化邻接矩阵
def normalize_adj(A):
    A_tilde = A + torch.eye(A.size(0))
    D = torch.diag(A_tilde.sum(dim=1))
    D_inv_sqrt = torch.pow(D, -0.5)
    D_inv_sqrt[D_inv_sqrt == float('inf')] = 0
    return D_inv_sqrt @ A_tilde @ D_inv_sqrt
```
### 5.2 训练要点
- **不引入 dropout 到消息传递矩阵**时梯度稳定，常用 tricks：权重归一化、early stopping
- 学习率一般 0.01，权重衰减 $5\times10^{-4}$
- 小图可直接全图训练；大图需要邻居采样（GraphSAGE、Cluster-GCN）
---
## 六、GCN 的关键性质与局限
### 6.1 优点
- 结构简单、参数量少（每层只有 $d_{in} \times d_{out}$）
- 半监督学习能力强，能利用无标签节点
- 可扩展出整个 GNN 家族
### 6.2 局限
| 问题 | 说明 | 改进方向 |
|------|------|----------|
| **过平滑** | 层数加深后所有节点表示趋同（$\hat{A}$ 反复乘导致低通滤波） | 通常只用 2~3 层；ResGCN、JK-Net |
| **转导式** | 需要整个图的邻接矩阵，新节点加入需重训 | GraphSAGE（归纳式） |
| **同质假设** | 所有邻居权重相同，不适配异质图 | GAT（注意力加权） |
| **无法处理有向/异质图** | 对称 $\hat{A}$ 的限制 | R-GCN、HGT |
| **过平滑 vs 感受野矛盾** | 要大感受野就要深，深就平滑 | MixHop、APPNP（传播与解耦） |
### 6.3 与后续模型的关系
```
GCN ──┬── GAT:        把固定归一化权重换成注意力权重 α_ij
      ├── GraphSAGE:  采样+聚合，支持归纳式学习
      ├── ChebNet:    GCN 的"父模型"，高阶多项式
      ├── APPNP:      用个性化 PageRank 扩大感受野且不过平滑
      └── GIN:        理论上最具判别力的消息传递形式
```
---
## 七、总结
一句话概括 GCN：
> **每一层 GCN 让每个节点聚合自身及邻居的（归一化加权）特征并做非线性变换，堆叠 $K$ 层即可捕获 $K$ 阶邻域信息。**
核心公式只需记住：
$$\boxed{H^{(l+1)} = \sigma\left(\tilde{D}^{-1/2}\tilde{A}\tilde{D}^{-1/2} H^{(l)} W^{(l)}\right)}$$
三个关键设计：
1. **加自环**：保留自身信息
2. **对称归一化**：平衡高度数节点的影响
3. **逐层堆叠**：扩大感受野
