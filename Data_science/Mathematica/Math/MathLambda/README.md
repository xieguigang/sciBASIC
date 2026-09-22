# 符号数学引擎：微积分、代数与 MathML 编译

## 引言

数值计算给出「答案是多少」，符号计算给出「式子是什么」。后者在科研中不可替代：

- 推导出的公式可以写进论文、可以被物理解释；
- 求导 / 求极限的结果可以继续参与后续推导；
- 复杂的表达式可以先化简再求值，避免数值不稳定。

本包提供一个**不可变表达式树**驱动的计算机代数引擎，并打通了「MathML → 符号表达式 → 编译后的 LINQ 表达式」这条链路。

## 设计目标

- **不可变**：任何化简 / 变换都产出新表达式，不修改原对象——因此可安全并发使用，也便于比较与缓存；
- **结构化变换**：求导、积分、极限、Taylor 展开都实现为**树的重写规则**；
- **可编译执行**：符号表达式最终可编译为 LINQ 表达式树，运行时不解释。

## 核心能力

| 领域 | 能力 |
|---|---|
| **微积分** | 求导、积分、极限、Taylor 展开 |
| **代数** | 多项式因式分解、表达式化简、布尔代数重写 |
| **编译** | MathML → 符号表达式 → LINQ 表达式树（编译为委托） |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.Lambda`（根） | 引擎入口与共享类型 |
| `....Math.Lambda.Symbolic` | 表达式模型、化简与变换规则、MathML 编译 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Lambda.Symbolic

' 1. 从字符串构造表达式
Dim f = Symbolic.Parse("sin(x) * x^2")

' 2. 符号求导（结果为新的表达式）
Dim df = f.Differentiate("x")
Console.WriteLine(df.ToString())

' 3. 化简
Dim simplified = df.Simplify()

' 4. 编译为可执行委托（求值不再解释）
Dim g = simplified.Compile()
Console.WriteLine(g(1.5))

' 5. 从 MathML 读入公式
Dim fromMathML = Symbolic.FromMathML(mathmlText)
```

## 实现要点

- **为什么强调不可变**：化简规则会反复应用于子表达式；若表达式可变，一个分支的改写会意外影响共享的子节点。不可变结构从根本上杜绝这类错误。
- **『符号求导』与『数值求导』的差别**：数值求导用差分近似，存在截断误差与步长选择问题；符号求导得到的是**精确**的导数表达式，可继续参与符号运算。
- **编译的价值**：符号化简会显著增大表达式（即使化简后仍比原式复杂），若每次求值都遍历树，开销会很大；编译为 LINQ 表达式后，求值过程由 JIT 编译为原生代码。
- **MathML 桥接的意义**：MathML 是公式的通用交换格式；能读它意味着公式可以来自文档、编辑器或其它数学软件。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Lambda`
- TargetFramework：`net10.0`
- Tags：`scibasic;symbolic-math;computer-algebra;calculus;differentiation;integration;mathml;polynomial;immutable-expression-tree`
- 许可：GPL-3.0-or-later
