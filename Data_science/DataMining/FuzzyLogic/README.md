# Microsoft.VisualBasic.DataMining.FuzzyLogic

模糊逻辑计算库，包含**两套并存但共享隶属函数实现**的推理语义：

| 命名空间 | 推理语义 | 用途 |
|---|---|---|
| `Logical.FuzzyLogic` | Mamdani：梯形四点术语 + `AND/OR` 文本规则 + 重心法去模糊 | 传统模糊控制器、FCL 风格 XML 模型（源自 DotFuzzy） |
| `TSK` | Takagi-Sugeno-Kang：可微隶属函数 + `Integer(,)` 规则表 + 乘积 T-范数 + 加权平均 | 可解释分类 / 辅助诊断 / 非线性回归与函数逼近 |

> 命名空间说明：工程 `RootNamespace` 是 `Microsoft.VisualBasic.DataMining.FuzzyLogic`，
> 存量 Mamdani 代码在文件里又声明了 `Namespace Logical.FuzzyLogic`，因此全名是
> `Microsoft.VisualBasic.DataMining.FuzzyLogic.Logical.FuzzyLogic.*`；
> 新内核的全名是 `Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK.*`。

---

## 一、TSK 可微推理内核

### 1.1 五层结构

```
x → 模糊化 μ(i,k) → 乘积触发强度 w_r → 归一化 w̄_r → 一阶线性结论 f_r = p_r·x + q_r → z = Σ w̄_r·f_r
```

- 隶属函数：高斯、广义钟形、Sigmoid（处处可微）、三角、梯形（分段可微，节点导数取 0）
- 每个隶属函数都提供 `Evaluate(x)`、`Gradient(x)`（解析导数）与 `LogGradient(x)`（对数导数）
- 一次前向保留全部中间量（`TskInferenceTrace`），可做可解释性诊断：哪些规则被激活、权重多少
- 反向只做解析推导，不依赖任何自动微分框架

### 1.2 最小用法

```vb
Imports Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK

' 1. 结构：每个输入 3 个高斯模糊集，规则数 = 3 x 3 = 9
Dim inputs As TskInputVariable() = {
    TskInputVariable.CreateGrid("x1", -1.0, 1.0, 3, MembershipKind.Gaussian),
    TskInputVariable.CreateGrid("x2", -1.0, 1.0, 3, MembershipKind.Gaussian)
}
Dim rules As FuzzyRuleBase = FuzzyRuleBase.CreateFullGrid({3, 3})

' 2. 也可以从数据直接建结构（定义域取自训练集）
Dim net As TskNetwork = TskNetworkBuilder.CreateGrid(x, y)

' 3. 结论系数（形状 [R*(n+1), 输出数]），可用最小二乘 / 梯度法在外部训练
net.Theta = theta

' 4. 推理与诊断
Dim z = net.PredictRow(New Double() {0.25, -0.5})
Dim activation = net.GetRuleActivations(New Double() {0.25, -0.5})   ' 规则触发强度
Dim members = net.GetMembershipDegrees(New Double() {0.25, -0.5})    ' 各输入隶属度

' 5. 持久化与导出
Dim json = net.ToJson()
Dim restored = TskNetwork.FromJson(json)
Dim fis = net.ExportFisText()
```

训练（LSE + 反向传播、早停、剪枝、交叉验证）不在本库，见
`Microsoft.VisualBasic.DeepLearning.ANFIS`：`AnfisFitting.Fit(x, y)`。

### 1.3 规则表

规则表是 `Integer(,)`（行 = 规则、列 = 输入、值 = 该输入的模糊集下标）：

```vb
Dim table As Integer(,) = FuzzyRuleBase.CreateFullGrid({2, 3, 2}).ToAntecedentTable()

' 只启用部分规则（注入专家知识 / 跳过不可能的组合）
Dim masked = FuzzyRuleBase.CreateFullGrid({2, 3, 2}, enableMask, maxRules:=1024)

' 剪枝：只保留前 6 条
Dim subset = masked.SelectRules({0, 1, 2, 3, 4, 5})
Dim text = subset.ToAntecedentText(inputs)   ' 文本仅用于展示，不参与计算
```

### 1.4 数值约定

- 乘积在对数域累加（`sum log μ`），低于 `-700` 视为 0
- 触发强度之和小于 `1e-12` 时该行输出 0、梯度为 0（避免 0/0）
- 前提参数梯度写成 `(f_r - z)·w̄_r·δ`，不需要除以 `μ`，所以隶属度下溢时不会出 NaN
- 宽度/斜率类参数在日志空间优化，天然保持为正

---

## 二、Mamdani 前端（DotFuzzy 派生，保持向后兼容）

```vb
Imports FL = Microsoft.VisualBasic.DataMining.FuzzyLogic.Logical.FuzzyLogic

Dim water As New FL.LinguisticVariable("Water")
water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Cold", 0, 0, 20, 40))
water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Tepid", 30, 50, 50, 70))
water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Hot", 50, 80, 100, 100))

Dim power As New FL.LinguisticVariable("Power")
power.MembershipFunctionCollection.Add(New FL.MembershipFunction("Low", 0, 25, 25, 50))
power.MembershipFunctionCollection.Add(New FL.MembershipFunction("High", 25, 50, 50, 75))

Dim engine As New FL.FuzzyEngine()
engine.LinguisticVariableCollection.Add(water)
engine.LinguisticVariableCollection.Add(power)
engine.Consequent = "Power"
engine.FuzzyRuleCollection.Add(New FL.FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High"))
engine.FuzzyRuleCollection.Add(New FL.FuzzyRule("IF (Water IS Hot) THEN Power IS Low"))

water.InputValue = 60
Console.WriteLine(engine.Defuzzify())          ' 重心法输出

Call engine.Save("fuzzyModel.xml", Encodings.UTF8)
engine = FL.Models.FuzzyModel.FromXml("fuzzyModel.xml")
```

### 与 TSK 内核的关系

`MembershipFunction` 现在是 `TSK.TrapezoidalMembership` 的门面：

- `X0`/`X1`/`X2`/`X3` 是对参数向量的投影（`a`/`b`/`c`/`d`）
- `LinguisticVariable.Fuzzify` 直接调用 `Evaluate`，不再有第二份分段 if/else
- 因此历史四点模型可以直接当作 `IMembershipFunction` 喂给 TSK 的结构（无需适配器）
- `Value`（规则触发强度累积器）、`Centorid()`、`Area()` 与 `FuzzyEngine` 的解析/去模糊逻辑**语义不变**

退化肩点（`X0 == X1` 或 `X2 == X3`）在端点处取 1，与历史实现逐点一致；
该等价性由 `_smoketest` 的 Mamdani 一致性一节（含 8 节共 54 条断言）守住。

---

## 三、自检

```powershell
cd _smoketest
dotnet run --project selftest.vbproj
```

覆盖：Mamdani 基线逐点一致、退化肩点语义、五种隶属函数的解析导数 vs 中心差分、
批量前向 vs 标量参考、前提/结论梯度 vs 中心差分、规则表枚举与掩膜与剪枝、
JSON 往返、XML 往返回归。

## 来源

Mamdani 部分源自 [DotFuzzy](https://github.com/MicheleBertoli/DotFuzzy)（GPL/LGPL 双许可，
见各源文件头部声明）；TSK 内核与统一后的求值路径为本仓库新增实现（GPL3）。
