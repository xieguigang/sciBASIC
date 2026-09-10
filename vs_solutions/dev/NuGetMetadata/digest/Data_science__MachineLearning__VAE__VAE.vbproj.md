# Data_science/MachineLearning/VAE/VAE.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.VAE
- TargetFramework: net10.0
- Source files  : 4
- Existing Title: Variational Autoencoders: Graph VAE, Gaussian Mixture VAE and GMM
- Existing Desc : Generative modelling library for sciBASIC#: a graph variational autoencoder with GCN encoder and inner-product decoder, a Gaussian mixture VAE with categorical latents, and an EM-trained Gaussian mixture model.
- Existing Tags : scibasic;variational-autoencoder;gmm;generative-model;clustering

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder)

## Public types
- Class GaussianMixtureModel (GMM\GMM.vb) - 高斯混合模型 (Gaussian Mixture Model) 使用 EM 算法对数据进行聚类 / 密度估计 / 信号分解 模型形式: p(x) = Σ_k π_k · N(x | μ_k, Σ_k)
- Enum InitMethod (GMM\GMM.vb)
- Class GMVAE (GMVAE\GMVAE.vb) - 高斯混合变分自编码器 (Gaussian Mixture VAE) 通过引入类别隐变量 y 和连续隐变量 z, 实现对数据的多模态建模 适合于聚类 + 生成任务
- Module GraphUtils (GraphVAE.vb)
- Class LinearLayer (GraphVAE.vb)
- Class GCNLayer (GraphVAE.vb)
- Structure GraphVAEConfig (GraphVAE.vb)
- Class GraphVAE (GraphVAE.vb)
- Module MathUtils (MathUtils.vb) - 数学辅助函数库

## Notable public members
- Public Property Weights As Tensor
- Public Property Means As Tensor
- Public Property Covariances As Tensor
- Public Property Converged As Boolean = False
- Public Property NIter As Integer = 0
- Public Property LogLikelihood As Double = Double.NegativeInfinity
- Public Property LogLikelihoodHistory As New List(Of Double)
- Public Sub New(Optional nComponents As Integer = 3,
- Public Sub Fit(X As Tensor)
- Public Function ComputeLogGaussianPDF(X As Tensor) As Tensor
- Public Function PredictProba(X As Tensor) As Tensor
- Public Function Predict(X As Tensor) As Integer()
- Public Function Score(X As Tensor) As Double
- Public Function ScoreSamples(X As Tensor) As Tensor
- Public Function Sample(n As Integer, Optional seed As Integer? = Nothing) As Tensor
- Public Shared Function MatrixInverse(mat As Tensor, n As Integer) As Tensor
- Public Shared Function LogDeterminant(mat As Tensor, n As Integer) As Double
- Public Function GetComponentMean(k As Integer) As Tensor
- Public Function GetComponentVariance(k As Integer) As Tensor
- Public Function GetComponentStdDev(k As Integer) As Tensor
- Public Overrides Function ToString() As String
- Public Shared Function Predicts(rowdatas() As ClusterEntity, components As Integer, threshold As Double, strict As Boolean) As GaussianMixtureModel
- Public Shared Function Predicts(samples() As Double, components As Integer, threshold As Double, verbose As Boolean) As GaussianMixtureModel
- Public ReadOnly Property InputDim As Integer
- Public ReadOnly Property LatentDim As Integer
- Public ReadOnly Property NComponents As Integer
- Public ReadOnly Property LossHistory As List(Of Double)
- Public Sub New(inputDim As Integer,
- Public Sub Forward(x As Tensor, rng As Random,
- Public Sub Fit(X As Tensor,
- Public Sub Encode(x As Tensor,
- Public Function Reconstruct(x As Tensor, Optional useMean As Boolean = True) As Tensor
- Public Function Generate(n As Integer, Optional componentIdx As Integer = -1, Optional seed As Integer? = Nothing) As Tensor
- Public Function PredictClusters(X As Tensor) As Integer()
- Public Overrides Function ToString() As String
- Public Function NormalizeAdjacency(adj As Tensor) As Tensor
- Public Function Sigmoid(x As Double) As Double
- Public Function ApplySigmoid(t As Tensor) As Tensor
- Public Function ApplyReLU(t As Tensor) As Tensor
- Public Function ReLUDerivFromInput(preActivation As Tensor, gradOutput As Tensor) As Tensor
- Public Function BCELossSum(pred As Tensor, target As Tensor) As Double
- Public Function KLDivergence(mu As Tensor, logVar As Tensor) As Double
- Public Function AddBias(mat As Tensor, bias As Tensor) As Tensor
- Public Function BroadcastRow(t As Tensor, m As Integer) As Tensor
- Public Function Threshold(t As Tensor, thresholdVal As Double) As Tensor
- Public Function BinaryAccuracy(pred As Tensor, target As Tensor, thresholdVal As Double) As Double
- Public Sub New(inDim As Integer, outDim As Integer, Optional seed As Integer? = Nothing)
- Public Function Forward(input As Tensor) As Tensor
- Public Function Backward(gradOutput As Tensor) As Tensor
- Public Sub Update(lr As Single, beta1 As Single, beta2 As Single, eps As Single)
- Public Sub New(inDim As Integer, outDim As Integer, Optional seed As Integer? = Nothing)
- Public Function Forward(normAdj As Tensor, input As Tensor) As Tensor
- Public Function Backward(gradOutput As Tensor) As Tensor
- Public Sub Update(lr As Single, beta1 As Single, beta2 As Single, eps As Single)
- Public Sub New(config As GraphVAEConfig, Optional seed As Integer? = Nothing)
- Public Function Forward(adj As Tensor, features As Tensor) As Tuple(Of Tensor, Tensor, Tensor, Tensor, Tensor)
- Public Function ComputeLoss(reconAdj As Tensor, adj As Tensor,
- Public Sub Backward(adj As Tensor, features As Tensor,
- Public Sub Update(lr As Single, Optional beta1 As Single = 0.9F, Optional beta2 As Single = 0.999F, Optional eps As Single = 0.00000001F)
- Public Function Train(graphs As List(Of Tuple(Of Tensor, Tensor)),
- ... and 7 more

## Imports
- Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
- Microsoft.VisualBasic.MachineLearning.TensorFlow
- std = System.Math

## File tree
- GMM\GMM.vb
- GMVAE\GMVAE.vb
- GraphVAE.vb
- MathUtils.vb

