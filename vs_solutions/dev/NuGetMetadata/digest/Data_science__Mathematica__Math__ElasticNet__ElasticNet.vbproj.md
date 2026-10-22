# Data_science/Mathematica/Math/ElasticNet/ElasticNet.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet
- AssemblyName  : Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: Regularization and Variable Selection via the Elastic Net
- Existing Desc : Elastic net regression for sciBASIC#: a coordinate-descent solver that combines L1 and L2 penalties with an unpenalized intercept, warm-started regularization paths, cross validation over the l1Ratio and alpha grid, a polynomial view of the fitted model and NumericTable entry points for table based workflows.
- Existing Tags : scibasic;elastic-net;regularization;lasso;ridge;coordinate-descent;cross-validation;feature-selection

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet)

## Public types
- Module Program (_smoketest\Program.vb) - Elastic Net 弹性网络算法的自检程序。 覆盖：正交设计之上的闭式解、共线特征的成组效应、两种退化（纯 Ridge / 无惩罚）、 稀疏性、正则化路径、交叉验证选参、预测写回与参数校验。
- Class ElasticNetCVResult (ElasticNetCVResult.vb) - 弹性网络的交叉验证结果：在 ``l1Ratio × alpha`` 的二维网格上面 逐一计算折外均方误差，并以误差最小的组合作为最优超参数。 由于不同的 ``l1Ratio`` 具有不同的 alpha 上界（最大 alpha 与 L1 惩罚的
- Class ElasticNetFit (ElasticNetFit.vb) - 弹性网络（Elastic Net）在给定 ``(alpha, l1Ratio)`` 之下的拟合结果 模型的形式为 ``y = c + b1*x1 + b2*x2 + ...``，其中截距 ``c`` 不参与惩罚， 系数 ``b`` 则是 L1 与 L2 联合惩罚之下收缩之后的解。
- Module ElasticNetFitting (ElasticNetFitting.vb) - 混合比例：0 就是纯 Ridge、1 就是纯 Lasso。 求解使用**坐标下降**（与 sklearn / glmnet 同源）：每次只更新一个系数， 用软阈值算子实现 L1 的稀疏性，同时维护残差向量做增量更新，
- Class ElasticNetData (ElasticNetFitting.vb) - 预处理之后的数据视图：中心化并且（可选）标准化
- Class PathSolution (ElasticNetFitting.vb) - 一条正则化路径上的解
- Class ElasticNetPath (ElasticNetPath.vb) - 弹性网络在一条 alpha 正则化路径上面的全部解（正则化路径 / 系数路径） 路径上面的每一步都由 <see cref="Alphas"/> 之中的一个 alpha 值、 <see cref="Intercepts"/> 之中的一个截距与 <see cref="Coefficients"/>
- Class ElasticNetPolynomial (ElasticNetPolynomial.vb) - 弹性网络模型的多项式形式：``y = c + b1*x1 + b2*x2 + ...`` 既有的 <see cref="MultivariatePolynomial"/> 只计算 ``sum(x * b)`` 而没有 截距项，而弹性网络的截距是不施加惩罚的独立参数，因此这里单独给出一个
- Module NamespaceDoc (NamespaceDoc.vb) - Elastic Net (L1 + L2 regularized) regression. 弹性网络是 Lasso 与 Ridge 的组合：它同时施加 L1 与 L2 惩罚， 既保留了 Lasso 把系数压缩到零（从而具备特征选择能力）的性质，
- Module NumericTableElasticNet (NumericTableElasticNet.vb) - 弹性网络回归的统一二维表入口 这一组扩展方法必须留在 ElasticNet 程序集里面：本工程引用了 DataFittings， 而 DataFittings 并不（也不可能）反过来引用本工程，因此把入口写在

## Notable public members
- Public Property L1Ratios As Double()
- Public Property Alphas As Double()()
- Public Property MSE As Double()()
- Public Property Folds As Integer
- Public Property BestL1RatioIndex As Integer
- Public Property BestAlphaIndex As Integer
- Public Property Fit As ElasticNetFit
- Public ReadOnly Property BestL1Ratio As Double
- Public ReadOnly Property BestAlpha As Double
- Public ReadOnly Property BestMSE As Double
- Public Function toNumericTable() As NumericTable
- Public Overrides Function ToString() As String
- Public Property Alpha As Double
- Public Property L1Ratio As Double
- Public Property Intercept As Double
- Public Property Coefficients As Double()
- Public Property FeatureNames As String()
- Public Property SSE As Double
- Public Property SST As Double
- Public Property Iterations As Integer
- Public Property Converged As Boolean
- Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest
- Public ReadOnly Property NonZeroCount As Integer
- Public ReadOnly Property R2 As Double Implements IFitted.R2
- Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
- Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
- Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable
- Public Overrides Function ToString() As String
- Public ReadOnly Property DefaultL1Ratios As Double()
- Public Function Fit(x As Double()(),
- Public Function FitColumns(columns As Double()(),
- Public Function RegularizationPath(x As Double()(),
- Public Function RegularizationPathColumns(columns As Double()(),
- Public Function CrossValidate(x As Double()(),
- Public Property L1Ratio As Double
- Public Property Alphas As Double()
- Public Property Intercepts As Double()
- Public Property Coefficients As Double()()
- Public Property RSquared As Double()
- Public Property NonZeroCounts As Integer()
- Public Property FeatureNames As String()
- Public Property StoppedEarly As Boolean
- Public ReadOnly Property Count As Integer
- Public Function getCoefficients(idx As Integer) As Double()
- Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable
- Public Overrides Function ToString() As String
- Public Property Intercept As Double
- Public Overrides Function Evaluate(ParamArray x() As Double) As Double
- Public Overrides Function ToString(format As String, Optional html As Boolean = False) As String
- Public Overrides Function ToString(variables() As String, format As String, Optional html As Boolean = False) As String
- Public Function ElasticNetRegression(table As NumericTable,
- Public Function ElasticNetRegularizationPath(table As NumericTable,
- Public Function ElasticNetCrossValidate(table As NumericTable,

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.Data
- Microsoft.VisualBasic.Data.Bootstrapping
- Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet
- Microsoft.VisualBasic.Math.LinearAlgebra
- Microsoft.VisualBasic.Math.SIMD
- std = System.Math
- System
- System.Linq
- System.Runtime.CompilerServices

## File tree
- _smoketest\Program.vb
- ElasticNetCVResult.vb
- ElasticNetFit.vb
- ElasticNetFitting.vb
- ElasticNetPath.vb
- ElasticNetPolynomial.vb
- NamespaceDoc.vb
- NumericTableElasticNet.vb

