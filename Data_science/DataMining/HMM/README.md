# 隐马尔可夫模型：训练与解码算法

## 引言

隐马尔可夫模型（HMM）处理的是这样一类问题：**我们能观测到序列，但真正关心的是生成这些观测的隐藏状态序列**。

- 观测是句子里的词，隐藏状态是词性 → 词性标注；
- 观测是碱基序列，隐藏状态是「编码区 / 非编码区」 → 基因识别；
- 观测是信号采样，隐藏状态是「正常 / 异常」 → 异常检测。

围绕这个模型有三个经典问题，正好对应本包的三个算法族。

## 三个经典问题与对应实现

| 问题 | 算法 | 命名空间 |
|---|---|---|
| 给定模型，这条观测序列出现的概率是多少？ | 前向 / 后向评估 | `Algorithm` |
| 给定模型与观测，最可能的隐藏状态序列是什么？ | Viterbi 解码 | `Algorithm.HMMAlgorithm` |
| 给定观测，如何估计模型参数？ | Baum-Welch（EM） | `Algorithm.HMMAlgorithm` |

此外 `Algorithm.HMMChainAlgorithm` 把同样的操作扩展到马尔可夫链，`Models` 命名空间承载模型数据结构。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.DataMining.HiddenMarkovChain`（根） | 隐马尔可夫链模型与共享抽象 |
| `....HiddenMarkovChain.Models` | 模型数据结构 |
| `....HiddenMarkovChain.Algorithm` | 前向 / 后向评估与 Bayes 评分 |
| `....HiddenMarkovChain.Algorithm.HMMAlgorithm` | Viterbi 解码与 Baum-Welch 重估 |
| `....HiddenMarkovChain.Algorithm.HMMChainAlgorithm` | 马尔可夫链算法 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm

' 1. 构造模型（初始概率、转移概率、发射概率）
Dim hmm As New HMMChain(initial, transition, emission)

' 2. 评估：这条观测序列的概率
Dim score = HMMAlgorithm.Forward(hmm, observations)

' 3. 解码：最可能的隐藏状态序列
Dim path = HMMAlgorithm.Viterbi(hmm, observations)

' 4. 训练：从无标注数据重估参数（EM）
Dim trained = HMMAlgorithm.BaumWelch(hmm, unlabelledObservations, iterations:=50)
```

## 实现要点

- **前向算法为什么不用枚举**：隐藏状态序列的数量随序列长度**指数增长**；前向算法利用马尔可夫性质做动态规划，把复杂度降到 O(状态数² × 序列长度)。
- **Viterbi 与前向的差别**：前向对**所有**路径求和，Viterbi 只取**概率最大**的那条路径；因此 Viterbi 需要额外保存「每个格子的最优前驱」以便回溯。
- **Baum-Welch 的本质**：它是 EM 算法在 HMM 上的特例——E 步用前向 / 后向概率估计每个状态的出现期望，M 步用这些期望重新估计参数；理论上每轮迭代都不会降低似然。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.HiddenMarkovChain`
- TargetFramework：`net10.0`
- Tags：`scibasic;hidden-markov-model;viterbi;baum-welch;forward-backward;markov-chain;sequence-decoding`
- 许可：GPL-3.0-or-later
