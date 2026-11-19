# Data_science/MachineLearning/ANFIS/ANFIS.vbproj

- RootNamespace : Microsoft.VisualBasic.DeepLearning.ANFIS
- AssemblyName  : Microsoft.VisualBasic.DeepLearning.ANFIS
- TargetFramework: net10.0
- Source files  : 10
- Existing Title: Adaptive Network-based Fuzzy Inference System
- Existing Desc : Takagi-Sugeno ANFIS training layer: hybrid least squares / back propagation learning, rule pruning and cross validation on top of the shared differentiable TSK inference kernel (Microsoft.VisualBasic.DataMining.FuzzyLogic).
- Existing Tags : scibasic;anfis;neuro-fuzzy;fuzzy-logic;takagi-sugeno;machine-learning

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DeepLearning.ANFIS)

## Public types
- Module Program (_smoketest\Program.vb) - ANFIS 自检程序：把"算法是否真的按公式实现"变成可执行的断言。 重点不是跑出一个好看的 R2，而是守住三条数值契约： <list type="number">
- Class AnfisFitResult (AnfisFitResult.vb) - ANFIS 的拟合结果：误差指标、训练历史与可解释性诊断。
- Class AnfisCrossValidationResult (AnfisFitResult.vb)
- Module AnfisFitting (AnfisFitting.vb) - Dim result = AnfisFitting.Fit(x, y) ' x: Double()()（行 = 样本），y: Double() Dim yhat = AnfisFitting.Predict(result.Model, xTest) </code>
- Class NamespaceDoc (NamespaceDoc.vb) - <item>乘积在对数域累加（<c>sum log mu</c>），低于 <c>-700</c> 视为 0；</item> <item>触发强度之和小于 <c>1e-12</c> 时该行输出 0 且梯度为 0（避免 0/0）；</item> <item>前提参数梯度用 <c>(f_r - z)*w-bar_r*delta</c> 的形式，
- Class AnfisHybridTrainer (Training\AnfisHybridTrainer.vb) - <item>**反向**：结论参数固定，按行块累加前提参数梯度，再由优化器更新隶属函数的中心/宽度。</item> </list> <para>
- Class AnfisOptimizer (Training\AnfisOptimizer.vb) - <para> 关键设计：**必须为正的参数（各隶属函数的宽度、斜率）在内部以对数参数化**。 设 <c>s = ln p</c>，则 <c>dE/ds = p * dE/dp</c>，更新在 <c>s</c> 空间完成后再
- Enum AnfisLearningMode (Training\AnfisOptions.vb) - ANFIS 的参数更新策略（对应 readme 中列出的四种混合方案）。
- Enum AnfisOptimizerKind (Training\AnfisOptions.vb)
- Class AnfisOptions (Training\AnfisOptions.vb) - <para> 网络**结构**参数（隶属函数类型与个数、规则启用掩膜、规模上限、分块行数）继承自 <see cref="TskStructureOptions"/>，与推理内核共享同一份定义；
- Class LeastSquaresAccumulator (Training\LeastSquaresAccumulator.vb) - <para> 之所以要有这个"累加器"而不是直接构造整张设计矩阵再调 <c>OLS</c>： ANFIS 的设计矩阵是 <c>[N, R*(n+1)]</c>，N 大一点就会占用几十 MB 到上百 MB，
- Class RecursiveLeastSquares (Training\RecursiveLeastSquares.vb) - gain = u / denom theta += gain * (y - a' * theta) P = (P - gain * u') / forget

## Notable public members
- Public Property Model As TskNetwork
- Public Property R2 As Double
- Public Property RMSE As Double
- Public Property SSE As Double
- Public Property SST As Double
- Public Property FittedValues As Double()()
- Public Property Residuals As Double()()
- Public Property TrainRmseHistory As Double()
- Public Property ValidationRmseHistory As Double()
- Public Property ThetaNormHistory As Double()
- Public Property RuleActivations As Double()
- Public Property PrunedRuleCount As Integer
- Public Property Iterations As Integer
- Public Property BestEpoch As Integer
- Public Property Converged As Boolean
- Public Property Warnings As String()
- Public Function GetY(index As Integer) As Double()
- Public Function GetRuleSummary() As String()
- Public Overrides Function ToString() As String
- Public Property FoldRmse As Double()
- Public Property FoldR2 As Double()
- Public Property Models As TskNetwork()
- Public ReadOnly Property MeanRmse As Double
- Public ReadOnly Property MeanR2 As Double
- Public ReadOnly Property StdRmse As Double
- Public Overrides Function ToString() As String
- Public Function Fit(x As Double()(), y As Double(), Optional options As AnfisOptions = Nothing) As AnfisFitResult
- Public Function Fit(x As Double()(), y As Double()(), Optional options As AnfisOptions = Nothing) As AnfisFitResult
- Public Function CreateModel(x As Double()(), y As Double()(), Optional options As AnfisOptions = Nothing) As TskNetwork
- Public Function Predict(model As TskNetwork, x As Double()(), Optional chunkRows As Integer = 1024) As Double()()
- Public Function PredictScalar(model As TskNetwork, x As Double()()) As Double()
- Public Function Evaluate(model As TskNetwork, x As Double()(), y As Double()()) As AnfisFitResult
- Public Function PruneRules(model As TskNetwork,
- Public Function CrossValidate(x As Double()(),
- Public Shared Sub Validate(x As Double()(), y As Double()())
- Public Shared Function ToMatrix(y As Double()()) As Double(,)
- Public Shared Function SliceRows(matrix As Double(,), start As Integer, count As Integer) As Double(,)
- Public Shared Sub CopyRowsInto(dst As Double(,), src As Double(,), start As Integer, count As Integer)
- Public Shared Function SelectRows(x As Double()(), indices As Integer()) As Double()()
- Public Shared Function SelectRows(matrix As Double(,), indices As Integer()) As Double(,)
- Public Shared Function Shuffle(count As Integer, seed As Integer) As Integer()
- Public Shared Sub Split(sampleCount As Integer,
- Public Sub New(network As TskNetwork, Optional options As AnfisOptions = Nothing)
- Public ReadOnly Property Warnings As String()
- Public Function Train(x As Double()(), y As Double()()) As AnfisFitResult
- Public Sub New(parameterCount As Integer, positiveMask As Boolean(), options As AnfisOptions)
- Public ReadOnly Property StepCount As Integer
- Public Sub ApplyGradients(parameters As Double(), gradients As Double(), learningRate As Double, Optional gradientClip As Double = 5.0)
- Public Sub Reset()
- Public Property LearningMode As AnfisLearningMode = AnfisLearningMode.Hybrid
- Public Property Optimizer As AnfisOptimizerKind = AnfisOptimizerKind.Adam
- Public Property LearningRate As Double = 0.02
- Public Property LearningRateDecay As Double = 0.05
- Public Property Momentum As Double = 0.9
- Public Property Beta1 As Double = 0.9
- Public Property Beta2 As Double = 0.999
- Public Property Epsilon As Double = 0.00000001
- Public Property WeightDecay As Double = 0.0
- Public Property GradientClip As Double = 5.0
- Public Property Ridge As Double = 0.000001
- ... and 26 more

## Imports
- Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
- Microsoft.VisualBasic.DeepLearning.ANFIS
- Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
- Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
- SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics
- std = System.Math
- System
- System.Linq
- System.Text

## File tree
- _smoketest\Program.vb
- AnfisFitResult.vb
- AnfisFitting.vb
- NamespaceDoc.vb
- Training\AnfisData.vb
- Training\AnfisHybridTrainer.vb
- Training\AnfisOptimizer.vb
- Training\AnfisOptions.vb
- Training\LeastSquaresAccumulator.vb
- Training\RecursiveLeastSquares.vb

