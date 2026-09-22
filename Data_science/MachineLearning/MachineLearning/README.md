# 经典机器学习：随机森林、SVM、遗传算法与 Q-Learning

## 引言

神经网络并非万能：表格数据上**树模型**往往更准，小样本上 **SVM** 常常更稳，参数搜索用**进化算法**比网格搜索更高效。本包提供这些「非神经网络」的经典算法，与 `DeepLearning` 包形成互补。

## 算法一览

### 有监督学习

| 命名空间 | 模型 | 说明 |
|---|---|---|
| `SVM`（+ `StorageProcedure`） | 支持向量机 | LibSVM 风格实现，含核函数与求解器 |
| `RandomForests` | 随机森林 | 集成决策树，抗过拟合 |

### 进化算法（`Darwinism`）

| 命名空间 | 内容 |
|---|---|
| `Darwinism.GAF` | 遗传算法框架：种群与演化引擎 |
| `Darwinism.GAF.Population`（+ `SubstitutionStrategy`） | 种群模型与替换策略（决定新个体如何替代旧个体） |
| `Darwinism.GAF.Helper` | 辅助方法 |
| `Darwinism.Models` | 模型定义 |

另含**差分进化（differential evolution）**——适合连续参数优化的进化方法。

### 强化学习（`QLearning`）

`QLearning` 与 `QLearning.DataModel` 实现**表格型 Q 学习**：以状态-动作表存储价值估计，适合状态空间离散且规模可控的问题。

### 支撑类型（`ComponentModel`）

- 共享的学习算法接口；
- 神经网络模型使用的激活函数（`Activations`）；
- 归一化样本数据集类型。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF

' 1. SVM 分类
Dim svm As New SupportVectorMachine(kernel:=Kernel.RBF, c:=1.0, gamma:=0.5)
Call svm.Train(features, labels)
Dim predicted = svm.Predict(testFeatures)

' 2. 随机森林
Dim forest As New RandomForest(trees:=200)
Call forest.Train(features, labels)

' 3. 遗传算法做参数搜索
Dim ga As New GeneticAlgorithm(populationSize:=100)
Dim best = ga.Optimize(objective:=AddressOf MyFitness, generations:=200)

' 4. Q-Learning
Dim q As New QLearning(actions:=4, learningRate:=0.1, discount:=0.95)
Call q.Update(state, action, reward, nextState)
```

## 实现要点

- **为什么表格数据偏爱树模型**：决策树对特征**单调变换不敏感**（不需要标准化）、能自动处理特征交互、且对无关特征不敏感；随机森林通过多棵树的投票进一步降低方差。
- **SVM 的核技巧**：核函数让线性分类器能在高维空间中工作而无需显式构造高维特征；RBF 核是通用默认选择，`C` 控制容错程度，`gamma` 控制影响半径。
- **替换策略为什么重要**：遗传算法的收敛速度与多样性维持高度依赖「新个体如何替代旧个体」；精英保留（elitism）防止最优解丢失，而适度保留劣解有助于跳出局部最优。
- **Q-Learning 的适用边界**：表格型实现要求状态空间可枚举；状态连续或维度过高时应改用函数近似（如 DQN）。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning`
- TargetFramework：`net10.0`
- Tags：`scibasic;machine-learning;random-forest;svm;libsvm;genetic-algorithm;differential-evolution;q-learning;reinforcement-learning`
- 许可：GPL-3.0-or-later
