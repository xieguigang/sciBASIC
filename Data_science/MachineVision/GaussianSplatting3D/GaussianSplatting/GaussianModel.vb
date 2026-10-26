' ---------------------------------------------------------------------------
' 3D 高斯模型
'
' 数据布局说明（性能关键）：
'   参数、梯度、Adam 状态全部以 **扁平 Double() 数组**为唯一真实存储
'   （行优先：第 i 个高斯的第 j 个参数位于 i * cols + j）。
'   Tensor 只是零拷贝视图（Tensor.Wrap，不复制数据），用于 I/O 与对外 API。
'
'   这样做的原因：
'     1. Tensor 的索引器 setter 每次写入都会 Interlocked.Increment 版本号
'        （见 Tensor.vb），在逐元素热循环里是纯粹的浪费；
'     2. 光栅化与反向传播需要连续内存上的向量化访问（SIMD / Span）；
'     3. 张量视图可以缓存，只在 Count 变化时重新绑定，避免逐次分配。
'
' 修改约定：任何绕过 Tensor 索引器就地改写数组之后，必须调用 MarkModified()，
' 让 Version 自增，从而使 fp32 镜像缓存与设备端显存副本失效。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 3D 高斯模型 - 存储所有高斯椭球的参数
'''
''' 每个高斯由以下参数定义：
'''   - 位置 (mean)        μ ∈ R³       3 个参数
'''   - 缩放 (scale)       s ∈ R³       3 个参数（log 空间，保证为正）
'''   - 旋转 (rotation)    q ∈ R⁴       4 个参数（四元数）
'''   - 颜色 (color)       c ∈ R³       3 个参数（RGB，0-1）
'''   - 不透明度 (opacity) α ∈ R        1 个参数（logit 空间）
'''
''' 协方差矩阵通过 scale 和 rotation 计算：
'''   Σ = R · diag(exp(s)²) · Rᵀ
''' 其中 R 是从四元数 q 构造的 3x3 旋转矩阵。
''' </summary>
Public Class GaussianModel

    ''' <summary>位置列数</summary>
    Public Const POSITION_COLS As Integer = 3
    ''' <summary>缩放列数</summary>
    Public Const SCALE_COLS As Integer = 3
    ''' <summary>旋转（四元数）列数</summary>
    Public Const ROTATION_COLS As Integer = 4
    ''' <summary>颜色列数</summary>
    Public Const COLOR_COLS As Integer = 3
    ''' <summary>不透明度列数</summary>
    Public Const OPACITY_COLS As Integer = 1

    ''' <summary>高斯数量</summary>
    Public Property Count As Integer

    ' ---- 参数（扁平、行优先）----
    Friend _pos As Double()
    Friend _scale As Double()
    Friend _rot As Double()
    Friend _color As Double()
    Friend _opac As Double()

    ' ---- 梯度 ----
    Friend _gPos As Double()
    Friend _gScale As Double()
    Friend _gRot As Double()
    Friend _gColor As Double()
    Friend _gOpac As Double()

    ' ---- Adam 一阶 / 二阶矩 ----
    Friend _mPos As Double(), _vPos As Double()
    Friend _mScale As Double(), _vScale As Double()
    Friend _mRot As Double(), _vRot As Double()
    Friend _mColor As Double(), _vColor As Double()
    Friend _mOpac As Double(), _vOpac As Double()

    ' ---- Tensor 视图（零拷贝，Count 变化时重新绑定）----
    Private _posT As Tensor, _scaleT As Tensor, _rotT As Tensor, _colorT As Tensor, _opacT As Tensor
    Private _gPosT As Tensor, _gScaleT As Tensor, _gRotT As Tensor, _gColorT As Tensor, _gOpacT As Tensor
    Private _mPosT As Tensor, _vPosT As Tensor
    Private _mScaleT As Tensor, _vScaleT As Tensor
    Private _mRotT As Tensor, _vRotT As Tensor
    Private _mColorT As Tensor, _vColorT As Tensor
    Private _mOpacT As Tensor, _vOpacT As Tensor

    ''' <summary>
    ''' 参数版本号。任何参数/状态发生写变更后自增，供 fp32 镜像缓存判断失效。
    ''' </summary>
    Public Property Version As Long

    ''' <summary>声明参数已被就地修改（使 fp32 镜像与显存缓存失效）。</summary>
    Public Sub MarkModified()
        Version += 1
    End Sub

    Public Sub New()
        Me.New(0)
    End Sub

    Public Sub New(count As Integer)
        Me.Count = std.Max(0, count)
        Call AllocateArrays(Me.Count)
        Call Rebind()
    End Sub

    ''' <summary>按当前 <see cref="Count"/> 分配全部存储（旧数据被丢弃）。</summary>
    Private Sub AllocateArrays(n As Integer)
        _pos = New Double(n * POSITION_COLS - 1) {}
        _scale = New Double(n * SCALE_COLS - 1) {}
        _rot = New Double(n * ROTATION_COLS - 1) {}
        _color = New Double(n * COLOR_COLS - 1) {}
        _opac = New Double(n * OPACITY_COLS - 1) {}

        _gPos = New Double(n * POSITION_COLS - 1) {}
        _gScale = New Double(n * SCALE_COLS - 1) {}
        _gRot = New Double(n * ROTATION_COLS - 1) {}
        _gColor = New Double(n * COLOR_COLS - 1) {}
        _gOpac = New Double(n * OPACITY_COLS - 1) {}

        _mPos = New Double(n * POSITION_COLS - 1) {} : _vPos = New Double(n * POSITION_COLS - 1) {}
        _mScale = New Double(n * SCALE_COLS - 1) {} : _vScale = New Double(n * SCALE_COLS - 1) {}
        _mRot = New Double(n * ROTATION_COLS - 1) {} : _vRot = New Double(n * ROTATION_COLS - 1) {}
        _mColor = New Double(n * COLOR_COLS - 1) {} : _vColor = New Double(n * COLOR_COLS - 1) {}
        _mOpac = New Double(n * OPACITY_COLS - 1) {} : _vOpac = New Double(n * OPACITY_COLS - 1) {}

        Call MarkModified()
    End Sub

    ''' <summary>重建 Tensor 零拷贝视图。</summary>
    Private Sub Rebind()
        Dim n = Me.Count

        _posT = Tensor.Wrap(_pos, n, POSITION_COLS)
        _scaleT = Tensor.Wrap(_scale, n, SCALE_COLS)
        _rotT = Tensor.Wrap(_rot, n, ROTATION_COLS)
        _colorT = Tensor.Wrap(_color, n, COLOR_COLS)
        _opacT = Tensor.Wrap(_opac, n, OPACITY_COLS)

        _gPosT = Tensor.Wrap(_gPos, n, POSITION_COLS)
        _gScaleT = Tensor.Wrap(_gScale, n, SCALE_COLS)
        _gRotT = Tensor.Wrap(_gRot, n, ROTATION_COLS)
        _gColorT = Tensor.Wrap(_gColor, n, COLOR_COLS)
        _gOpacT = Tensor.Wrap(_gOpac, n, OPACITY_COLS)

        _mPosT = Tensor.Wrap(_mPos, n, POSITION_COLS) : _vPosT = Tensor.Wrap(_vPos, n, POSITION_COLS)
        _mScaleT = Tensor.Wrap(_mScale, n, SCALE_COLS) : _vScaleT = Tensor.Wrap(_vScale, n, SCALE_COLS)
        _mRotT = Tensor.Wrap(_mRot, n, ROTATION_COLS) : _vRotT = Tensor.Wrap(_vRot, n, ROTATION_COLS)
        _mColorT = Tensor.Wrap(_mColor, n, COLOR_COLS) : _vColorT = Tensor.Wrap(_vColor, n, COLOR_COLS)
        _mOpacT = Tensor.Wrap(_mOpac, n, OPACITY_COLS) : _vOpacT = Tensor.Wrap(_vOpac, n, OPACITY_COLS)
    End Sub

#Region "参数 / 梯度 / 状态的访问器"

    ' 扁平数组（热路径）：不再经过 Tensor 索引器
    Public ReadOnly Property PositionsData As Double()
        Get
            Return _pos
        End Get
    End Property

    Public ReadOnly Property ScalesData As Double()
        Get
            Return _scale
        End Get
    End Property

    Public ReadOnly Property RotationsData As Double()
        Get
            Return _rot
        End Get
    End Property

    Public ReadOnly Property ColorsData As Double()
        Get
            Return _color
        End Get
    End Property

    Public ReadOnly Property OpacitiesData As Double()
        Get
            Return _opac
        End Get
    End Property

    Public ReadOnly Property PositionsGradData As Double()
        Get
            Return _gPos
        End Get
    End Property

    Public ReadOnly Property ScalesGradData As Double()
        Get
            Return _gScale
        End Get
    End Property

    Public ReadOnly Property RotationsGradData As Double()
        Get
            Return _gRot
        End Get
    End Property

    Public ReadOnly Property ColorsGradData As Double()
        Get
            Return _gColor
        End Get
    End Property

    Public ReadOnly Property OpacitiesGradData As Double()
        Get
            Return _gOpac
        End Get
    End Property

    Public ReadOnly Property PositionsMData As Double()
        Get
            Return _mPos
        End Get
    End Property

    Public ReadOnly Property PositionsVData As Double()
        Get
            Return _vPos
        End Get
    End Property

    Public ReadOnly Property ScalesMData As Double()
        Get
            Return _mScale
        End Get
    End Property

    Public ReadOnly Property ScalesVData As Double()
        Get
            Return _vScale
        End Get
    End Property

    Public ReadOnly Property RotationsMData As Double()
        Get
            Return _mRot
        End Get
    End Property

    Public ReadOnly Property RotationsVData As Double()
        Get
            Return _vRot
        End Get
    End Property

    Public ReadOnly Property ColorsMData As Double()
        Get
            Return _mColor
        End Get
    End Property

    Public ReadOnly Property ColorsVData As Double()
        Get
            Return _vColor
        End Get
    End Property

    Public ReadOnly Property OpacitiesMData As Double()
        Get
            Return _mOpac
        End Get
    End Property

    Public ReadOnly Property OpacitiesVData As Double()
        Get
            Return _vOpac
        End Get
    End Property

    ''' <summary>位置 [N, 3]（零拷贝视图）</summary>
    Public ReadOnly Property Positions As Tensor
        Get
            Return _posT
        End Get
    End Property

    ''' <summary>缩放（log 空间）[N, 3]（零拷贝视图）</summary>
    Public ReadOnly Property Scales As Tensor
        Get
            Return _scaleT
        End Get
    End Property

    ''' <summary>旋转四元数 [N, 4]（零拷贝视图）</summary>
    Public ReadOnly Property Rotations As Tensor
        Get
            Return _rotT
        End Get
    End Property

    ''' <summary>颜色 RGB [N, 3]（零拷贝视图）</summary>
    Public ReadOnly Property Colors As Tensor
        Get
            Return _colorT
        End Get
    End Property

    ''' <summary>不透明度（logit 空间）[N, 1]（零拷贝视图）</summary>
    Public ReadOnly Property Opacities As Tensor
        Get
            Return _opacT
        End Get
    End Property

    Public ReadOnly Property PositionsGrad As Tensor
        Get
            Return _gPosT
        End Get
    End Property

    Public ReadOnly Property ScalesGrad As Tensor
        Get
            Return _gScaleT
        End Get
    End Property

    Public ReadOnly Property RotationsGrad As Tensor
        Get
            Return _gRotT
        End Get
    End Property

    Public ReadOnly Property ColorsGrad As Tensor
        Get
            Return _gColorT
        End Get
    End Property

    Public ReadOnly Property OpacitiesGrad As Tensor
        Get
            Return _gOpacT
        End Get
    End Property

    Public ReadOnly Property PositionsM As Tensor
        Get
            Return _mPosT
        End Get
    End Property

    Public ReadOnly Property PositionsV As Tensor
        Get
            Return _vPosT
        End Get
    End Property

    Public ReadOnly Property ScalesM As Tensor
        Get
            Return _mScaleT
        End Get
    End Property

    Public ReadOnly Property ScalesV As Tensor
        Get
            Return _vScaleT
        End Get
    End Property

    Public ReadOnly Property RotationsM As Tensor
        Get
            Return _mRotT
        End Get
    End Property

    Public ReadOnly Property RotationsV As Tensor
        Get
            Return _vRotT
        End Get
    End Property

    Public ReadOnly Property ColorsM As Tensor
        Get
            Return _mColorT
        End Get
    End Property

    Public ReadOnly Property ColorsV As Tensor
        Get
            Return _vColorT
        End Get
    End Property

    Public ReadOnly Property OpacitiesM As Tensor
        Get
            Return _mOpacT
        End Get
    End Property

    Public ReadOnly Property OpacitiesV As Tensor
        Get
            Return _vOpacT
        End Get
    End Property

#End Region

    ''' <summary>
    ''' 将所有梯度清零（复用已分配的缓冲，不重新分配）。
    ''' </summary>
    Public Sub ZeroGradients()
        Array.Clear(_gPos, 0, _gPos.Length)
        Array.Clear(_gScale, 0, _gScale.Length)
        Array.Clear(_gRot, 0, _gRot.Length)
        Array.Clear(_gColor, 0, _gColor.Length)
        Array.Clear(_gOpac, 0, _gOpac.Length)
    End Sub

    ''' <summary>
    ''' 从一个 3D 点云初始化高斯模型。
    ''' 每个点变成一个小的各向同性高斯。
    ''' </summary>
    ''' <param name="pts">点云位置 [N, 3]</param>
    ''' <param name="cols">点云颜色 [N, 3]，范围 [0,1]</param>
    ''' <param name="initScale">初始缩放（世界单位）</param>
    Public Shared Function FromPointCloud(pts As Tensor, cols As Tensor, initScale As Single) As GaussianModel
        Return FromPointCloud(pts.Data, cols.Data, pts.Shape(0), initScale)
    End Function

    ''' <summary>
    ''' 从扁平数组（行优先 [N,3]）初始化高斯模型。
    ''' </summary>
    Public Shared Function FromPointCloud(pts As Double(), cols As Double(), n As Integer, initScale As Single) As GaussianModel
        Dim model As New GaussianModel(n)
        Dim logScale = std.Log(initScale)
        Dim pos = model._pos
        Dim color = model._color
        Dim scale = model._scale
        Dim rot = model._rot
        Dim opac = model._opac

        For i = 0 To n - 1
            Dim o3 = i * 3
            pos(o3) = pts(o3) : pos(o3 + 1) = pts(o3 + 1) : pos(o3 + 2) = pts(o3 + 2)
            color(o3) = std.Max(0.0, std.Min(1.0, cols(o3)))
            color(o3 + 1) = std.Max(0.0, std.Min(1.0, cols(o3 + 1)))
            color(o3 + 2) = std.Max(0.0, std.Min(1.0, cols(o3 + 2)))
            scale(o3) = logScale : scale(o3 + 1) = logScale : scale(o3 + 2) = logScale

            Dim o4 = i * 4
            rot(o4) = 1.0 : rot(o4 + 1) = 0.0 : rot(o4 + 2) = 0.0 : rot(o4 + 3) = 0.0
            opac(i) = -2.197
        Next

        Call model.MarkModified()

        Return model
    End Function

    ''' <summary>
    ''' 从均匀分布的随机点云初始化（用于无先验的从头训练）
    ''' </summary>
    Public Shared Function FromRandom(count As Integer, bounds As Single(), initScale As Single, seed As Integer) As GaussianModel
        Dim rng As New Random(seed)
        Dim model As New GaussianModel(count)
        Dim logScale = std.Log(initScale)

        Dim pos = model._pos, color = model._color, scale = model._scale, rot = model._rot, opac = model._opac

        For i = 0 To count - 1
            Dim o3 = i * 3

            pos(o3) = rng.NextDouble() * (bounds(3) - bounds(0)) + bounds(0)
            pos(o3 + 1) = rng.NextDouble() * (bounds(4) - bounds(1)) + bounds(1)
            pos(o3 + 2) = rng.NextDouble() * (bounds(5) - bounds(2)) + bounds(2)

            color(o3) = rng.NextDouble()
            color(o3 + 1) = rng.NextDouble()
            color(o3 + 2) = rng.NextDouble()

            scale(o3) = logScale : scale(o3 + 1) = logScale : scale(o3 + 2) = logScale

            rot(i * 4) = 1.0
            opac(i) = -2.197
        Next

        Call model.MarkModified()

        Return model
    End Function

    ''' <summary>
    ''' 从四元数 (w, x, y, z) 构造 3x3 旋转矩阵。
    ''' </summary>
    Public Shared Function QuaternionToMatrix(w As Double, x As Double, y As Double, z As Double) As Double(,)
        Dim flat(8) As Double
        Call QuaternionToMatrix(w, x, y, z, flat)

        Dim matrix(2, 2) As Double

        For row = 0 To 2
            For col = 0 To 2
                matrix(row, col) = flat(row * 3 + col)
            Next
        Next

        Return matrix
    End Function

    ''' <summary>
    ''' 从四元数 (w, x, y, z) 构造 3x3 旋转矩阵（写入行优先扁平数组）。
    ''' </summary>
    Public Shared Sub QuaternionToMatrix(w As Double, x As Double, y As Double, z As Double, R As Double())
        Dim n = std.Sqrt(w * w + x * x + y * y + z * z)

        If n < 0.000000000001 Then
            Call Identity(R)
            Return
        End If

        w /= n : x /= n : y /= n : z /= n

        Dim xx = x * x, yy = y * y, zz = z * z
        Dim xy = x * y, xz = x * z, yz = y * z
        Dim wx = w * x, wy = w * y, wz = w * z

        R(0) = 1 - 2 * (yy + zz) : R(1) = 2 * (xy - wz) : R(2) = 2 * (xz + wy)
        R(3) = 2 * (xy + wz) : R(4) = 1 - 2 * (xx + zz) : R(5) = 2 * (yz - wx)
        R(6) = 2 * (xz - wy) : R(7) = 2 * (yz + wx) : R(8) = 1 - 2 * (xx + yy)
    End Sub

    Private Shared Sub Identity(R As Double())
        Array.Clear(R, 0, 9)
        R(0) = 1 : R(4) = 1 : R(8) = 1
    End Sub

    ''' <summary>
    ''' 计算第 i 个高斯的 3x3 协方差矩阵 Σ = R · diag(s²) · Rᵀ。
    ''' </summary>
    Public Function GetCovariance(i As Integer) As Double(,)
        Dim sigma(2, 2) As Double
        Dim flat(8) As Double
        Call GetCovarianceFlat(i, flat)

        For r = 0 To 2
            For c = 0 To 2
                sigma(r, c) = flat(r * 3 + c)
            Next
        Next

        Return sigma
    End Function

    ''' <summary>
    ''' 计算第 i 个高斯的 3x3 协方差矩阵（行优先 9 元素扁平输出，只保证 6 个独立分量正确）。
    ''' </summary>
    Public Sub GetCovarianceFlat(i As Integer, sigma As Double())
        Dim o3 = i * 3
        Dim sx = std.Exp(_scale(o3)), sy = std.Exp(_scale(o3 + 1)), sz = std.Exp(_scale(o3 + 2))
        Dim o4 = i * 4

        Dim rot(8) As Double
        Call QuaternionToMatrix(_rot(o4), _rot(o4 + 1), _rot(o4 + 2), _rot(o4 + 3), rot)

        ' M = diag(sx², sy², sz²)
        Dim m0 = sx * sx, m1 = sy * sy, m2 = sz * sz

        ' Sigma = R * M * R^T
        For row = 0 To 2
            Dim r0 = rot(row * 3), r1 = rot(row * 3 + 1), r2 = rot(row * 3 + 2)

            For col = 0 To 2
                sigma(row * 3 + col) = r0 * m0 * rot(col * 3) + r1 * m1 * rot(col * 3 + 1) + r2 * m2 * rot(col * 3 + 2)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 获取第 i 个高斯的不透明度（经过 sigmoid）。
    ''' </summary>
    Public Function GetOpacity(i As Integer) As Double
        Return Sigmoid(_opac(i))
    End Function

    ''' <summary>数值稳定的 sigmoid。</summary>
    Public Shared Function Sigmoid(x As Double) As Double
        If x >= 0 Then
            Dim ex = std.Exp(-x)
            Return 1.0 / (1.0 + ex)
        Else
            Dim ex = std.Exp(x)
            Return ex / (1.0 + ex)
        End If
    End Function

    ''' <summary>
    ''' 追加新的高斯（用于自适应密度控制）。
    '''
    ''' 已存在的 N 行参数、梯度与 Adam 状态原样保留（这是关键：旧实现会把这些状态
    ''' 整体清零，等于每次致密化都把优化器重启一次）；新增行的高斯参数取自入参，
    ''' 梯度与 Adam 矩初始化为 0（与参考实现一致）。
    ''' </summary>
    Public Sub AddGaussians(newPositions As Tensor, newScales As Tensor, newRotations As Tensor,
                            newColors As Tensor, newOpacities As Tensor)
        Dim newCount = newPositions.Shape(0)
        If newCount <= 0 Then Return

        Call AppendGaussians(newPositions.Data, newScales.Data, newRotations.Data,
                             newColors.Data, newOpacities.Data, newCount)
    End Sub

    ''' <summary>
    ''' 追加新的高斯（扁平数组入参，行优先）。
    ''' </summary>
    Public Sub AppendGaussians(pos As Double(), scale As Double(), rot As Double(),
                               color As Double(), opac As Double(), newCount As Integer)
        If newCount <= 0 Then Return

        Dim oldCount = Me.Count
        Dim total = oldCount + newCount

        _pos = Concat(_pos, oldCount, POSITION_COLS, pos, newCount)
        _scale = Concat(_scale, oldCount, SCALE_COLS, scale, newCount)
        _rot = Concat(_rot, oldCount, ROTATION_COLS, rot, newCount)
        _color = Concat(_color, oldCount, COLOR_COLS, color, newCount)
        _opac = Concat(_opac, oldCount, OPACITY_COLS, opac, newCount)

        _gPos = Grow(_gPos, oldCount, POSITION_COLS, newCount)
        _gScale = Grow(_gScale, oldCount, SCALE_COLS, newCount)
        _gRot = Grow(_gRot, oldCount, ROTATION_COLS, newCount)
        _gColor = Grow(_gColor, oldCount, COLOR_COLS, newCount)
        _gOpac = Grow(_gOpac, oldCount, OPACITY_COLS, newCount)

        _mPos = Grow(_mPos, oldCount, POSITION_COLS, newCount) : _vPos = Grow(_vPos, oldCount, POSITION_COLS, newCount)
        _mScale = Grow(_mScale, oldCount, SCALE_COLS, newCount) : _vScale = Grow(_vScale, oldCount, SCALE_COLS, newCount)
        _mRot = Grow(_mRot, oldCount, ROTATION_COLS, newCount) : _vRot = Grow(_vRot, oldCount, ROTATION_COLS, newCount)
        _mColor = Grow(_mColor, oldCount, COLOR_COLS, newCount) : _vColor = Grow(_vColor, oldCount, COLOR_COLS, newCount)
        _mOpac = Grow(_mOpac, oldCount, OPACITY_COLS, newCount) : _vOpac = Grow(_vOpac, oldCount, OPACITY_COLS, newCount)

        Me.Count = total
        Call Rebind()
        Call MarkModified()
    End Sub

    ''' <summary>
    ''' 删除指定索引的高斯（用于自适应密度控制）。梯度与 Adam 状态按同一掩膜搬移。
    ''' </summary>
    Public Sub RemoveGaussians(indicesToRemove As HashSet(Of Integer))
        If indicesToRemove Is Nothing OrElse indicesToRemove.Count = 0 Then Return

        Dim keep(std.Max(1, Count) - 1) As Integer
        Dim k = 0

        For i = 0 To Count - 1
            If Not indicesToRemove.Contains(i) Then
                keep(k) = i
                k += 1
            End If
        Next

        If k = Count Then Return
        If k = 0 Then Return

        Dim kept(k - 1) As Integer
        Array.Copy(keep, kept, k)

        _pos = SelectRows(_pos, POSITION_COLS, kept, k)
        _scale = SelectRows(_scale, SCALE_COLS, kept, k)
        _rot = SelectRows(_rot, ROTATION_COLS, kept, k)
        _color = SelectRows(_color, COLOR_COLS, kept, k)
        _opac = SelectRows(_opac, OPACITY_COLS, kept, k)

        _gPos = SelectRows(_gPos, POSITION_COLS, kept, k)
        _gScale = SelectRows(_gScale, SCALE_COLS, kept, k)
        _gRot = SelectRows(_gRot, ROTATION_COLS, kept, k)
        _gColor = SelectRows(_gColor, COLOR_COLS, kept, k)
        _gOpac = SelectRows(_gOpac, OPACITY_COLS, kept, k)

        _mPos = SelectRows(_mPos, POSITION_COLS, kept, k) : _vPos = SelectRows(_vPos, POSITION_COLS, kept, k)
        _mScale = SelectRows(_mScale, SCALE_COLS, kept, k) : _vScale = SelectRows(_vScale, SCALE_COLS, kept, k)
        _mRot = SelectRows(_mRot, ROTATION_COLS, kept, k) : _vRot = SelectRows(_vRot, ROTATION_COLS, kept, k)
        _mColor = SelectRows(_mColor, COLOR_COLS, kept, k) : _vColor = SelectRows(_vColor, COLOR_COLS, kept, k)
        _mOpac = SelectRows(_mOpac, OPACITY_COLS, kept, k) : _vOpac = SelectRows(_vOpac, OPACITY_COLS, kept, k)

        Me.Count = k
        Call Rebind()
        Call MarkModified()
    End Sub

    ''' <summary>把 (n, cols) 的行块拼接到目标数组之后。</summary>
    Private Shared Function Concat(existing As Double(), oldCount As Integer, cols As Integer,
                                   appended As Double(), newCount As Integer) As Double()
        Dim result(oldCount * cols + newCount * cols - 1) As Double

        If oldCount > 0 Then Call Array.Copy(existing, 0, result, 0, oldCount * cols)
        Call Array.Copy(appended, 0, result, oldCount * cols, newCount * cols)

        Return result
    End Function

    ''' <summary>保留前 oldCount 行并在尾部追加 newCount 行的零块。</summary>
    Private Shared Function Grow(existing As Double(), oldCount As Integer, cols As Integer, newCount As Integer) As Double()
        Dim result(oldCount * cols + newCount * cols - 1) As Double

        If oldCount > 0 Then Call Array.Copy(existing, 0, result, 0, oldCount * cols)

        Return result
    End Function

    ''' <summary>按索引挑选行。</summary>
    Private Shared Function SelectRows(source As Double(), cols As Integer, indices As Integer(), count As Integer) As Double()
        Dim result(count * cols - 1) As Double

        For i = 0 To count - 1
            Call Array.Copy(source, indices(i) * cols, result, i * cols, cols)
        Next

        Return result
    End Function

    ''' <summary>生成参数快照（用于对拍 / 调试）。</summary>
    Public Function Snapshot() As Double()()
        Dim result As Double()() = New Double(4)() {}

        result(0) = CType(_pos.Clone(), Double())
        result(1) = CType(_scale.Clone(), Double())
        result(2) = CType(_rot.Clone(), Double())
        result(3) = CType(_color.Clone(), Double())
        result(4) = CType(_opac.Clone(), Double())

        Return result
    End Function

End Class
