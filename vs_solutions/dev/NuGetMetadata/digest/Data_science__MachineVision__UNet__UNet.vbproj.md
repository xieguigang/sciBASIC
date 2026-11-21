# Data_science/MachineVision/UNet/UNet.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineVision.UNet
- AssemblyName  : Microsoft.VisualBasic.MachineVision.UNet
- TargetFramework: net10.0
- Source files  : 9
- Existing Title: U-Net: Convolutional Networks for Biomedical Image Segmentation
- Existing Desc : 
- Existing Tags : 

## Namespaces
- UNet  [files: 9]

## Public types
- Module DsbDataset (DsbDataset.vb) - DSB2018 数据集的加载与缓存。
- Module NamespaceDoc (NamespaceDoc.vb) - <para> 本命名空间在 sciBASIC# 已有的 CNN 层（卷积 / 池化 / ReLU）之上补齐了 U-Net 需要的 两块拼图（转置卷积与跳跃连接拼接层位于 <c>MachineLearning\DeepLearning\CNN</c>），
- Module RleCodec (RleCodec.vb) - DSB 风格的游程编码（RLE）掩膜解码。
- Class SegmentationMetrics (SegmentationLoss.vb) - 一次评估的结果。
- Module SegmentationLoss (SegmentationLoss.vb) - 逐像素 softmax 交叉熵及其梯度。
- Class SegmentationSample (SegmentationSample.vb) - 一个分割样本：归一化后的图像 + 每像素类别标签。 图像像素按 <c>(y * Width + x) * Channels + c</c> 排列（与 <see cref="DataBlock"/> 的 <c>(SX*y + x)*Depth + depth</c> 索引公式一致），取值落在 [0,1]；
- Module SyntheticSamples (SyntheticSamples.vb) - 确定性合成样本生成器。 真实数据（DSB2018）用于端到端的训练与评估，而**数值类**断言（梯度校验、 结构自检）需要"小而可控、不依赖外部文件"的数据，故保留这个生成器：
- Class UNetModel (UNetModel.vb) - U-Net 语义分割模型：对称编码器—解码器 + 跳跃连接。
- Class UNetOptions (UNetOptions.vb) - U-Net 网络的结构超参数。 网络是严格对称的： <list type="bullet">
- Class UNetTrainer (UNetTrainer.vb) - 小批量 Adam / SGD(momentum) 训练器。

## Notable public members
- Public Const TrainFolder As String = "stage1_train"
- Public Const TestFolder As String = "stage1_test"
- Public Const SolutionFile As String = "stage1_solution.csv"
- Public Const TrainLabelFile As String = "stage1_train_labels.csv"
- Public Function LoadTrain(root As String, imageSize As Integer,
- Public Function LoadTest(root As String, imageSize As Integer,
- Public Function LoadMaskFromPng(maskDir As String, width As Integer, height As Integer) As Boolean()
- Public Function ReadSolution(path As String) As Dictionary(Of String, List(Of String))
- Public Function ReadSolutionSizes(path As String) As Dictionary(Of String, Integer())
- Public Function ResizeImage(src As Double(), w As Integer, h As Integer, size As Integer) As Double()
- Public Function ResizeMask(src As Boolean(), w As Integer, h As Integer, size As Integer) As Boolean()
- Public Sub NormalizeContrast(pixels As Double())
- Public Function Decode(encoded As String, height As Integer, width As Integer,
- Public Function Merge(encodedList As IEnumerable(Of String), height As Integer, width As Integer,
- Public Function IoU(a As Boolean(), b As Boolean()) As Double
- Public Function ToLabels(mask As Boolean()) As Integer()
- Public Property PixelAccuracy As Double
- Public Property ForegroundIoU As Double
- Public Property BackgroundIoU As Double
- Public Property MeanIoU As Double
- Public Property Dice As Double
- Public Property PredictedForegroundRatio As Double
- Public Property TargetForegroundRatio As Double
- Public Overrides Function ToString() As String
- Public Function CrossEntropy(logits As DataBlock, labels As Integer(),
- Public Function ClassWeightsFromData(samples As SegmentationSample(),
- Public Function Evaluate(predicted As Integer(), target As Integer(), numClasses As Integer) As SegmentationMetrics
- Public Property Id As String
- Public Property Width As Integer
- Public Property Height As Integer
- Public Property Channels As Integer
- Public Property Pixels As Double()
- Public Property Labels As Integer()
- Public ReadOnly Property PixelCount As Integer
- Public Function WriteTo(block As DataBlock) As DataBlock
- Public Function Clone() As SegmentationSample
- Public Function Augment(rng As Random) As SegmentationSample
- Public Overrides Function ToString() As String
- Public Function Generate(count As Integer, size As Integer,
- Public Property Layer As CnnLayer
- Public Property Inputs As Integer()
- Public Property Output As DataBlock
- Public Property Kind As String
- Public Overrides Function ToString() As String
- Public ReadOnly Property Options As UNetOptions
- Public ReadOnly Property Output As DataBlock
- Public ReadOnly Property NodeCount As Integer
- Public Sub New(options As UNetOptions)
- Public ReadOnly Property Parameters As BackPropResult()
- Public ReadOnly Property ParameterCount As Long
- Public Function CreateInput() As DataBlock
- Public Function Forward(x As DataBlock, Optional training As Boolean = False) As DataBlock
- Public Sub Backward(dLogits As Double())
- Public Function Segment(x As DataBlock) As Integer()
- Public Function Probabilities(x As DataBlock) As Double()
- Public Sub Save(path As String)
- Public Shared Function Load(path As String) As UNetModel
- Public Function Describe() As String
- Public Overrides Function ToString() As String
- Public Property InputWidth As Integer = 64
- ... and 34 more

## Imports
- CnnLayer = Microsoft.VisualBasic.MachineLearning.CNN.layers.Layer
- Microsoft.VisualBasic.Imaging.BitmapImage.Png
- Microsoft.VisualBasic.MachineLearning.CNN
- Microsoft.VisualBasic.MachineLearning.CNN.data
- Microsoft.VisualBasic.MachineLearning.CNN.layers
- std = System.Math
- System.IO
- System.Linq

## File tree
- DsbDataset.vb
- NamespaceDoc.vb
- RleCodec.vb
- SegmentationLoss.vb
- SegmentationSample.vb
- SyntheticSamples.vb
- UNetModel.vb
- UNetOptions.vb
- UNetTrainer.vb

