
''' <summary>
''' # Lasso and Elastic-Net Regularized Generalized Linear Models
''' 
''' > https://glmnet.stanford.edu/
''' 
''' We provide extremely efficient procedures for fitting the entire lasso or elastic-net regularization path for linear regression (gaussian), multi-task gaussian, logistic and multinomial regression models (grouped or not), Poisson regression and the Cox model. The algorithm uses cyclical coordinate descent in a path-wise fashion. Details may be found in Friedman, Hastie, and Tibshirani (2010), Simon et al. (2011), Tibshirani et al. (2012), Simon, Friedman, and Hastie (2013), Tay, Narasimhan and Hastie (2023).
''' 
''' There are several vignettes under Articles which are helpful for getting started with glmnet, and getting into some of the details. Since the glmnet project is more than 18 years old, there is a document that outlines the history.
''' 
''' The companion book glmnet is (almost) all you need, by Tay, Craig, Narasimhan, Tibshirani, and Hastie, brings the package’s many capabilities together in one place, with worked examples. Accessible directly using the Book link in the menu bar. This book is somewhat dynamic, since it is in advanced draft mode, so will change slightly from time to time.
''' 
''' Version 5.0 is a major release with several new features, including:
''' 
''' All kernels are written in C++, so Fortran is now gone from glmnet;
''' All aspects of the Cox model are implemented efficiently, and the “Efron” method for tie-breaking is implemented;
''' glmnet.control() has been streamlined and reorganized, and absorbs some of the many arguments in earlier versions of glmnet.
''' These build on earlier versions, that included features such as:
''' 
''' Relaxed fitting to allow models in the path to be refit without regularization;
''' Assessment functions for displaying performance of models on test data. These include all the measures available via cv.glmnet, as well as confusion matrices and ROC plots for classification models;
''' Functions for building the x input matrix for glmnet that allow for one-hot-encoding of factor variables, appropriate treatment of missing values, and an option to create a sparse matrix if appropriate.
''' A function for fitting unpenalized a single version of any of the GLMs of glmnet.
''' The family argument allows for any GLM family used by glm(), in addition to the built-in glmnet families.
''' The scope for survival modeling has been enlarged, allowing for (start, stop) data and strata. We provide a much-requested method for survival:survfit.
''' Any model can use an x in sparse matrix format.
''' 
''' ## References
''' 
''' 1. Friedman, Jerome, Trevor Hastie, and Robert Tibshirani. 2010. “Regularization Paths for Generalized Linear Models via Coordinate Descent.” Journal of Statistical Software, Articles 33 (1): 1–22. https://doi.org/10.18637/jss.v033.i01.
''' 2. Simon, Noah, Jerome Friedman, and Trevor Hastie. 2013. “A Blockwise Descent Algorithm for Group-Penalized Multiresponse and Multinomial Regression.”
''' 3. Simon, Noah, Jerome Friedman, Trevor Hastie, and Robert Tibshirani. 2011. “Regularization Paths for Cox’s Proportional Hazards Model via Coordinate Descent.” Journal of Statistical Software, Articles 39 (5): 1–13. https://doi.org/10.18637/jss.v039.i05.
''' 4. Tibshirani, Robert, Jacob Bien, Jerome Friedman, Trevor Hastie, Noah Simon, Jonathan Taylor, and Ryan Tibshirani. 2012. “Strong Rules for Discarding Predictors in Lasso-Type Problems.” Journal of the Royal Statistical Society: Series B (Statistical Methodology) 74 (2): 245–66. https://doi.org/10.1111/j.1467-9868.2011.01004.x.
''' 5. Kenneth Tay, J, Narasimhan, Balasubramanian, Hastie, Trevor. 2023. “Elastic Net Regularization Paths for All Generalized Linear Models.” Journal of Statistical Software, Articles 106 (1): 1–31. https://doi.org/10.18637/jss.v106.i01.
''' 
''' ## 命名空间地图
''' 
''' | 类型 | 职责 |
''' |---|---|
''' | <see cref="GlmnetSolver"/> | <c>glmnet()</c> / <c>cv.glmnet()</c> 顶层入口与 family 分发 |
''' | <see cref="GlmnetOptions"/> | 与 R 5.1 对齐的全部拟合与 CV 参数 |
''' | <see cref="GlmnetControl"/> | 默认常量、lambda 网格与 <c>jerr</c> 错误码 |
''' | <see cref="GlmnetFit"/> | 统一路径结果（a0 / beta / lambda / df / dev.ratio / nulldev） |
''' | <see cref="GlmnetCVFit"/> | CV 曲线、<c>lambda.min</c> 与 <c>lambda.1se</c> |
''' | <see cref="GlmnetPredict"/> | predict / coef / deviance 与 <c>s</c> 插值 |
''' | <see cref="PredictionBundle"/> | link / response / coefficients / nonzero / class 的统一返回容器 |
''' | <see cref="SparseCSC"/> | 列压缩稀疏设计矩阵 |
''' | <see cref="AssessGLMNet"/> | assess / ROC / 混淆矩阵 |
''' | <see cref="RelaxedFitting"/> | relaxed lasso 的 gamma 网格与无惩罚重拟合 |
''' | <see cref="NumericTableGLMNet"/> | 二维表入口扩展方法 |
''' 
''' ## 六个 family
''' 
''' <c>gaussian</c> / <c>binomial</c> / <c>poisson</c> 共用加权 IRLS +
''' 弹性网坐标下降；<c>multinomial</c> 使用 softmax 组/非组惩罚；
''' <c>cox</c> 与 <c>mgaussian</c> 复用 <c>DataFittings</c> 中既有的
''' 偏似然与组-lasso 内核，再映射为统一结果对象。
''' 
''' ## 与 R 5.1 实测对齐的默认值
''' 
''' <c>alpha = 1</c>、<c>nlambda = 100</c>、<c>thresh = 1e-7</c>、
''' <c>maxit = 1e5</c>、<c>lambda.min.ratio = ifelse(nobs &lt; nvars, 0.01, 1e-4)</c>、
''' <c>type.multinomial = "ungrouped"</c>、<c>cox.ties = "breslow"</c>、
''' <c>glmnet.control()$eps = 1e-6</c>、<c>devmax = 0.999</c>。
''' 详见同目录 <c>README.md</c> 与 <c>test/R/check_version.R</c>。
''' </summary>
Module NamespaceDoc
End Module
