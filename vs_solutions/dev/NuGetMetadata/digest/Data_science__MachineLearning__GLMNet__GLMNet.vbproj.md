# Data_science/MachineLearning/GLMNet/GLMNet.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.GLMNet
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.GLMNet
- TargetFramework: net10.0
- Source files  : 31
- Existing Title: Lasso and Elastic-Net Regularized Generalized Linear Models
- Existing Desc : We provide extremely efficient procedures for fitting the entire lasso or elastic-net regularization path for linear regression (gaussian), multi-task gaussian, logistic and multinomial regression models (grouped or not), Poisson regression and the Cox model. The algorithm uses cyclical coordinate descent in a path-wise fashion. Details may be found in Friedman, Hastie, and Tibshirani (2010), Simon et al. (2011), Tibshirani et al. (2012), Simon, Friedman, and Hastie (2013), Tay, Narasimhan and Hastie (2023).

There are several vignettes under Articles which are helpful for getting started with glmnet, and getting into some of the details. Since the glmnet project is more than 18 years old, there is a document that outlines the history.

The companion book glmnet is (almost) all you need, by Tay, Craig, Narasimhan, Tibshirani, and Hastie, brings the package’s many capabilities together in one place, with worked examples. Accessible directly using the Book link in the menu bar. This book is somewhat dynamic, since it is in advanced draft mode, so will change slightly from time to time.

Version 5.0 is a major release with several new features, including:

All kernels are written in C++, so Fortran is now gone from glmnet;
All aspects of the Cox model are implemented efficiently, and the “Efron” method for tie-breaking is implemented;
glmnet.control() has been streamlined and reorganized, and absorbs some of the many arguments in earlier versions of glmnet.
These build on earlier versions, that included features such as:

Relaxed fitting to allow models in the path to be refit without regularization;
Assessment functions for displaying performance of models on test data. These include all the measures available via cv.glmnet, as well as confusion matrices and ROC plots for classification models;
Functions for building the x input matrix for glmnet that allow for one-hot-encoding of factor variables, appropriate treatment of missing values, and an option to create a sparse matrix if appropriate.
A function for fitting unpenalized a single version of any of the GLMs of glmnet.
The family argument allows for any GLM family used by glm(), in addition to the built-in glmnet families.
The scope for survival modeling has been enlarged, allowing for (start, stop) data and strata. We provide a much-requested method for survival:survfit.
Any model can use an x in sparse matrix format.
References
Friedman, Jerome, Trevor Hastie, and Robert Tibshirani. 2010. “Regularization Paths for Generalized Linear Models via Coordinate Descent.” Journal of Statistical Software, Articles 33 (1): 1–22. https://doi.org/10.18637/jss.v033.i01.

Simon, Noah, Jerome Friedman, and Trevor Hastie. 2013. “A Blockwise Descent Algorithm for Group-Penalized Multiresponse and Multinomial Regression.”

Simon, Noah, Jerome Friedman, Trevor Hastie, and Robert Tibshirani. 2011. “Regularization Paths for Cox’s Proportional Hazards Model via Coordinate Descent.” Journal of Statistical Software, Articles 39 (5): 1–13. https://doi.org/10.18637/jss.v039.i05.

Tibshirani, Robert, Jacob Bien, Jerome Friedman, Trevor Hastie, Noah Simon, Jonathan Taylor, and Ryan Tibshirani. 2012. “Strong Rules for Discarding Predictors in Lasso-Type Problems.” Journal of the Royal Statistical Society: Series B (Statistical Methodology) 74 (2): 245–66. https://doi.org/10.1111/j.1467-9868.2011.01004.x.

Kenneth Tay, J, Narasimhan, Balasubramanian, Hastie, Trevor. 2023. “Elastic Net Regularization Paths for All Generalized Linear Models.” Journal of Statistical Software, Articles 106 (1): 1–31. https://doi.org/10.18637/jss.v106.i01.
- Existing Tags : 

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.GLMNet)

## Public types
- Module AssessGLMNet (Evaluation\AssessGLMNet.vb) - <c>assess.glmnet</c> / <c>roc.glmnet</c> / <c>confusion.glmnet</c> 的等价能力。 所有指标都按整条 lambda 路径逐点计算，返回"指标名 → 曲线"的映射， 便于直接导出为 <c>NumericTable</c> 供绘图。
- Module RelaxedFitting (Evaluation\RelaxedFit.vb)
- Module TypeMeasure (Evaluation\TypeMeasure.vb) - <c>cv.glmnet</c> / <c>assess.glmnet</c> 的折外评估指标实现。
- Module FamilyRegistry (Families\FamilyRegistry.vb) - family 字符串 / 枚举解析与求解器分发。
- Interface IGlmnetFamily (Families\IGlmnetFamily.vb) - 六个 family 求解器共用的最小契约。
- Module SingleResponsePath (Families\SingleResponsePath.vb) - gaussian / binomial / poisson 共用的单响应 IRLS + 坐标下降路径求解器。 <para> 三个 family 只在"链接函数、工作权重、偏差公式"上不同，其余
- Module GlmnetControl (GlmnetControl.vb) - glmnet 5.1 的全局数值控制与默认常量（对应 R 端 <c>glmnet.control()</c>）。 本模块集中所有"魔法数"，family 求解器不得各自硬编码默认值； 这样对拍 R 时只需在一处校准。
- Enum FamilyKind (GlmnetEnums.vb)
- Enum PredictionType (GlmnetEnums.vb)
- Enum CVMeasure (GlmnetEnums.vb)
- Enum GlmnetError (GlmnetEnums.vb)
- Enum GaussianAlgorithm (GlmnetEnums.vb)
- Enum MultinomialGrouping (GlmnetEnums.vb)
- Enum CoxTiesMode (GlmnetEnums.vb)
- Enum LogisticAlgorithm (GlmnetEnums.vb)
- Module GlmnetPredict (GlmnetPredict.vb) - 统一的 <c>predict.glmnet</c> / <c>coef.glmnet</c> / <c>deviance.glmnet</c> 入口。
- Module GlmnetSolver (GlmnetSolver.vb) - 顶层统一入口：对应 R 端 <c>glmnet()</c> 与 <c>cv.glmnet()</c>。 调用形式为 <c>GlmnetSolver.Fit(x, y, "gaussian", options)</c>； 模块名不使用 <c>glmnet</c> 是为了避开 VB 大小写不敏感导致的
- Module Convergence (Internals\Convergence.vb)
- Module CoordinateDescent (Internals\CoordinateDescent.vb) - 共享的加权弹性网坐标下降内核（column-major、残差增量更新）。 求解 <c>1/(2n)*sum_i w_i*(z_i - a0 - x_i'beta)^2 + lambda*[alpha*sum_j pf_j*|beta_j| + (1-alpha)/2*sum_j beta_j^2]</c>。
- Module IrlsKernel (Internals\IrlsKernel.vb) - 二项 / 泊松 / 多分类共用的 IRLS 数学工具：工作响应、工作权重与偏差。 glmnet 统一最小化 <c>1/(2n)*deviance + lambda*P(beta)</c>； IRLS 在当前的线性预测子处把偏差局部二次化：
- Module LambdaGrid (Internals\LambdaGrid.vb) - lambda 网格：由零模型梯度（KKT 条件）推导 <c>lambda.max</c>，并按对数等间隔下降。 glmnet 对所有 GLM 统一最小化 <c>1/(2n)*deviance + lambda*P(beta)</c>， 因此在 beta=0 处，变量 j 的梯度为
- Module NamespaceDoc (NamespaceDoc.vb) - <c>gaussian</c> / <c>binomial</c> / <c>poisson</c> 共用加权 IRLS + 弹性网坐标下降；<c>multinomial</c> 使用 softmax 组/非组惩罚； <c>cox</c> 与 <c>mgaussian</c> 复用 <c>DataFittings</c> 中既有的
- Module NumericTableGLMNet (NumericTableGLMNet.vb) - glmnet 的 <c>NumericTable</c> 二维表入口。 与 <c>NumericTableElasticNet.vb</c> 的约定一致：自变量取表的全部特征列， 响应由 <c>y</c> 指定的标签列给出（缺省为第一个标签列）；

## Notable public members
- Public Function Assess(fit As GlmnetFit, newx As Double()(), newy As Double()()) As Dictionary(Of String, Double())
- Public Function Roc(scores As Double(), labels As Double()) As Double()()
- Public Function ConfusionMatrix(predicted As Integer(), actual As Integer(), numberOfClasses As Integer) As Integer()()
- Public Function toNumericTable(curves As Dictionary(Of String, Double())) As NumericTable
- Public Property Gamma As Double()
- Public Property Lambda As Double()
- Public Property Coefficients As Double()()()()
- Public Property NZero As Integer()()
- Public Property BestGammaIndex As Integer
- Public Function Fit(model As GlmnetFit, options As GlmnetOptions) As RelaxedFit
- Public Function IsMaximize(measure As CVMeasure) As Boolean
- Public Function EvaluateSingle(linkPredictions As Double()()(), truth As GlmnetData,
- Public Function EvaluateMulti(linkPredictions As Double()()(), truth As GlmnetData,
- Public Function Auc(scores As Double(), labels As Double(), weights As Double()) As Double
- Public Function CIndex(risk As Double(), time As Double(), status As Double()) As Double
- Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
- Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
- Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
- Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
- Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
- Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
- Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
- Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
- Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
- Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
- Public Property Lambda As Double()
- Public Property A0 As Double()()
- Public Property Beta As Double()()()
- Public Property Df As Integer()
- Public Property DevRatio As Double()
- Public Property Deviance As Double()
- Public Property NullDev As Double
- Public Property NZero As Integer()
- Public Property Converged As Boolean()
- Public Property Iterations As Integer()
- Public Property Jerr As GlmnetError
- Public Property Family As FamilyKind
- Public Property Options As GlmnetOptions
- Public Property FeatureNames As String()
- Public Property ResponseNames As String()
- Public Property ClassLabels As String()
- Public Property TrainingData As GlmnetData
- Public ReadOnly Property Count As Integer
- Public Function Parse(family As String) As FamilyKind
- Public Function Name(kind As FamilyKind) As String
- Public Function Create(kind As FamilyKind) As IGlmnetFamily
- Public Function MeasureName(measure As CVMeasure) As String
- Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
- Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
- Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
- Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
- Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
- Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
- Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
- Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
- Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
- Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
- Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
- Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
- Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
- ... and 205 more

## Imports
- CoxNS = Microsoft.VisualBasic.Data.Bootstrapping.Cox
- MGaussianNS = Microsoft.VisualBasic.Data.Bootstrapping.MGaussian
- Microsoft.VisualBasic.Data
- std = System.Math
- System.Linq
- System.Runtime.CompilerServices

## File tree
- Evaluation\AssessGLMNet.vb
- Evaluation\RelaxedFit.vb
- Evaluation\TypeMeasure.vb
- Families\BinomialFamily.vb
- Families\CoxFamily.vb
- Families\FamilyPath.vb
- Families\FamilyRegistry.vb
- Families\GaussianFamily.vb
- Families\IGlmnetFamily.vb
- Families\MGaussianFamily.vb
- Families\MultinomialFamily.vb
- Families\PoissonFamily.vb
- Families\PredictionBundle.vb
- Families\SingleResponsePath.vb
- GlmnetControl.vb
- GlmnetCVFit.vb
- GlmnetEnums.vb
- GlmnetFit.vb
- GlmnetOptions.vb
- GlmnetPredict.vb
- GlmnetSolver.vb
- Internals\Convergence.vb
- Internals\CoordinateDescent.vb
- Internals\GlmnetData.vb
- Internals\IrlsKernel.vb
- Internals\LambdaGrid.vb
- Internals\PenaltyFactors.vb
- Internals\SparseCSC.vb
- Internals\WeightedStandardizer.vb
- NamespaceDoc.vb
- NumericTableGLMNet.vb

