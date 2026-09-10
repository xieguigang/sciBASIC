# Data_science/MachineVision/GaussianSplatting3D/GaussianSplatting/GaussianSplatting.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
- AssemblyName  : Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
- TargetFramework: net10.0-windows
- Source files  : 6
- Existing Title: 3D Gaussian Splatting Scene Reconstruction and Rendering
- Existing Desc : 3D Gaussian splatting for sciBASIC#: fits anisotropic Gaussian ellipsoids with positions, scales, rotations, opacities and colours to multi-view images, then rasterises them with a camera model and Adam optimisation.
- Existing Tags : scibasic;gaussian-splatting;3d-reconstruction;neural-rendering;computer-vision

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineVision.GaussianSplatting3D)

## Public types
- Class CameraIntrinsics (Camera.vb) - 相机内参模型 - 针孔相机 Pinhole camera intrinsics: maps camera-space 3D points to pixel coordinates.
- Class CameraExtrinsics (Camera.vb) - 相机外参模型 - 世界到相机的变换 Camera extrinsics: world-to-camera transformation (R, t). 采用 OpenCV 约定：相机看向 +Z，X 向右，Y 向下。
- Class Camera (Camera.vb) - 完整的相机模型（内参 + 外参）
- Class GaussianModel (GaussianModel.vb) - 每个高斯由以下参数定义： - 位置 (mean) μ ∈ R³ 3 个参数 - 缩放 (scale) s ∈ R³ 3 个参数（各向异性缩放）
- Class GradientCalculator (GradientCalculator.vb) - - 实现简单，对任意可微参数都适用 - 不需要手动推导反向传播公式 - 易于验证正确性
- Class DensityController (GradientCalculator.vb) - 自适应密度控制 Adaptive Density Control ========================
- Class ImageLoader (ImageLoader.vb) - 图像加载器 - 从 PNG 文件加载图像并转换为 Tensor Image loader: convert PNG files to Tensor objects.
- Class PointCloudIO (ImageLoader.vb) - 点云 I/O - 读写 PLY 文件 Point cloud I/O: read/write PLY files (ASCII format).
- Class AdamOptimizer (Optimizer.vb) - Adam 优化器 Adam Optimizer ==============
- Class LRScheduler (Optimizer.vb) - 学习率调度器（余弦退火 + 阶梯衰减）
- Class SplatRenderer (SplatRenderer.vb) - 2. 将 3D 协方差矩阵投影到 2D 屏幕空间 Σ' = J * W * Σ * W^T * J^T 其中 W = [R | t] 是世界到相机的变换，J 是投影的雅可比矩阵

## Notable public members
- Public Property Fx As Double
- Public Property Fy As Double
- Public Property Cx As Double
- Public Property Cy As Double
- Public Property Width As Integer
- Public Property Height As Integer
- Public Sub New(fx As Double, fy As Double, cx As Double, cy As Double, width As Integer, height As Integer)
- Public Sub Project(x As Double, y As Double, z As Double, ByRef u As Double, ByRef v As Double)
- Public Function Clone() As CameraIntrinsics
- Public Property R As Double(,)
- Public Property T As Double()
- Public Sub New(r As Double(,), t As Double())
- Public Sub WorldToCamera(wx As Double, wy As Double, wz As Double,
- Public ReadOnly Property Eye As Double()
- Public Function Clone() As CameraExtrinsics
- Public Property Intrinsics As CameraIntrinsics
- Public Property Extrinsics As CameraExtrinsics
- Public Property ViewId As Integer
- Public Sub New(intrinsics As CameraIntrinsics, extrinsics As CameraExtrinsics, viewId As Integer)
- Public Sub ProjectWorldPoint(wx As Double, wy As Double, wz As Double,
- Public Shared Function LoadFromJson(path As String) As (Intrinsics As CameraIntrinsics, Cameras As List(Of Camera))
- Public Property Count As Integer
- Public Property Positions As Tensor
- Public Property Scales As Tensor
- Public Property Rotations As Tensor
- Public Property Colors As Tensor
- Public Property Opacities As Tensor
- Public Sub New(count As Integer)
- Public Sub ZeroGradients()
- Public Shared Function FromPointCloud(pts As Tensor, cols As Tensor, initScale As Single) As GaussianModel
- Public Shared Function FromRandom(count As Integer, bounds As Single(), initScale As Single, seed As Integer) As GaussianModel
- Public Shared Function QuaternionToMatrix(w As Double, x As Double, y As Double, z As Double) As Double(,)
- Public Function GetCovariance(i As Integer) As Double(,)
- Public Function GetOpacity(i As Integer) As Double
- Public Sub AddGaussians(newPositions As Tensor, newScales As Tensor, newRotations As Tensor,
- Public Sub RemoveGaussians(indicesToRemove As HashSet(Of Integer))
- Public Property Epsilon As Double = 0.01
- Public Property BatchSize As Integer = 0
- Public Sub New(Optional epsilon As Double = 0.01, Optional batchSize As Integer = 0)
- Public Sub ComputeGradients(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor))
- Public Property GradThreshold As Double = 0.0002
- Public Property MinOpacity As Double = 0.005
- Public Property MaxScale As Double = 0.5
- Public Property DensifyInterval As Integer = 100
- Public Sub New(Optional gradThreshold As Double = 0.0002)
- Public Function Densify(model As GaussianModel, [step] As Integer) As (Cloned As Integer, Split As Integer, Pruned As Integer)
- Public Shared Function Load(path As String) As Tensor
- Public Shared Sub Save(image As Tensor, path As String)
- Public Shared Function LoadAll(directory As String, numViews As Integer) As List(Of Tensor)
- Public Shared Sub SavePLY(path As String, positions As Tensor, colors As Tensor)
- Public Shared Sub SaveGaussianPLY(path As String, model As GaussianModel)
- Public Shared Function LoadPLY(path As String) As (Positions As Tensor, Colors As Tensor)
- Public Property LearningRate As Double = 0.01
- Public Property Beta1 As Double = 0.9
- Public Property Beta2 As Double = 0.999
- Public Property Epsilon As Double = 0.00000001
- Public Sub New(Optional lr As Double = 0.01)
- Public Sub IncrementStep()
- Public Sub StepModel(model As GaussianModel)
- Public Sub New(initialLr As Double, finalLr As Double, totalSteps As Integer)
- ... and 5 more

## Imports
- Microsoft.VisualBasic.MachineLearning.TensorFlow
- std = System.Math
- System.Drawing
- System.Drawing.Imaging
- System.IO
- System.Text.Json

## File tree
- Camera.vb
- GaussianModel.vb
- GradientCalculator.vb
- ImageLoader.vb
- Optimizer.vb
- SplatRenderer.vb

