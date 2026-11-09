' ---------------------------------------------------------------------------
' COLMAP 稀疏重建的数据模型
'
' 这里刻意<b>不</b>直接使用 GaussianSplatting 的 Camera 类型：
'   * COLMAP 的相机模型比高斯溅射需要的针孔模型多（含畸变参数），
'     先把原始参数完整保留下来，再在需要的地方投影成针孔内参，避免信息丢失；
'   * 稀疏点云（positions3D + 颜色）要同时被高斯初始化与点云重建用到，
'     需要一个与具体算法解耦的中间表示。
' 坐标系约定：与 COLMAP 一致，均为 OpenCV 约定（相机看向 +Z），
' 世界→相机：P_cam = R·P_world + t，相机中心 C = −Rᵀ·t。
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Colmap

    ''' <summary>
    ''' COLMAP 相机（内参模型，保留原始参数数组）。
    ''' </summary>
    Public Class ColmapCamera

        Public Property CameraId As Integer
        Public Property ModelId As Integer
        Public Property Width As Integer
        Public Property Height As Integer
        Public Property Parameters As Double()

        ''' <summary>水平焦距（像素）。</summary>
        Public ReadOnly Property Fx As Double
            Get
                Select Case ModelId
                    Case 0, 2, 7, 8, 9
                        ' SIMPLE_PINHOLE / SIMPLE_RADIAL / FOV / SIMPLE_RADIAL_FISHEYE / RADIAL_FISHEYE
                        Return Parameters(0)
                    Case Else
                        Return Parameters(0)
                End Select
            End Get
        End Property

        ''' <summary>垂直焦距（像素）。</summary>
        Public ReadOnly Property Fy As Double
            Get
                Select Case ModelId
                    Case 0, 2, 7, 8, 9
                        ' 单焦距模型：fx = fy = params[0]
                        Return Parameters(0)
                    Case Else
                        Return Parameters(1)
                End Select
            End Get
        End Property

        ''' <summary>主点 x。</summary>
        Public ReadOnly Property Cx As Double
            Get
                Select Case ModelId
                    Case 0, 2, 7, 8, 9
                        Return Parameters(1)
                    Case Else
                        Return Parameters(2)
                End Select
            End Get
        End Property

        ''' <summary>主点 y。</summary>
        Public ReadOnly Property Cy As Double
            Get
                Select Case ModelId
                    Case 0, 2, 7, 8, 9
                        Return Parameters(2)
                    Case Else
                        Return Parameters(3)
                End Select
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"camera #{CameraId} model={ModelId} {Width}×{Height} fx={Fx:F3} fy={Fy:F3} cx={Cx:F3} cy={Cy:F3}"
        End Function

    End Class

    ''' <summary>
    ''' 一张注册成功的照片及其位姿。
    ''' </summary>
    Public Class ColmapImage

        Public Property ImageId As Integer
        Public Property Name As String
        Public Property CameraId As Integer

        ''' <summary>四元数 (w, x, y, z)，COLMAP 约定。</summary>
        Public Property Qw As Double
        Public Property Qx As Double
        Public Property Qy As Double
        Public Property Qz As Double

        ''' <summary>平移向量（世界→相机）。</summary>
        Public Property Tx As Double
        Public Property Ty As Double
        Public Property Tz As Double

        Private _rotation As Double(,)

        ''' <summary>
        ''' 世界→相机旋转矩阵（行优先 3×3）。
        ''' </summary>
        Public ReadOnly Property Rotation As Double(,)
            Get
                If _rotation Is Nothing Then
                    _rotation = QuaternionToMatrix(Qw, Qx, Qy, Qz)
                End If

                Return _rotation
            End Get
        End Property

        ''' <summary>
        ''' 相机中心（世界坐标）：C = −Rᵀ·t。
        ''' </summary>
        Public Function CameraCenter() As Double()
            Dim r As Double(,) = Rotation
            Dim center(2) As Double

            For i As Integer = 0 To 2
                center(i) = -(r(0, i) * Tx + r(1, i) * Ty + r(2, i) * Tz)
            Next

            Return center
        End Function

        ''' <summary>
        ''' 四元数 (w,x,y,z) → 旋转矩阵（旋转矩阵 = 世界→相机）。
        ''' </summary>
        Public Shared Function QuaternionToMatrix(w As Double, x As Double, y As Double, z As Double) As Double(,)
            ' 归一化，防止 COLMAP 输出的四元数存在数值漂移导致矩阵非正交
            Dim norm As Double = std.Sqrt(w * w + x * x + y * y + z * z)

            If norm > 0.0 Then
                w /= norm
                x /= norm
                y /= norm
                z /= norm
            End If

            Dim m(2, 2) As Double

            m(0, 0) = 1.0 - 2.0 * (y * y + z * z)
            m(0, 1) = 2.0 * (x * y - w * z)
            m(0, 2) = 2.0 * (x * z + w * y)

            m(1, 0) = 2.0 * (x * y + w * z)
            m(1, 1) = 1.0 - 2.0 * (x * x + z * z)
            m(1, 2) = 2.0 * (y * z - w * x)

            m(2, 0) = 2.0 * (x * z - w * y)
            m(2, 1) = 2.0 * (y * z + w * x)
            m(2, 2) = 1.0 - 2.0 * (x * x + y * y)

            Return m
        End Function

        Public Overrides Function ToString() As String
            Dim c As Double() = CameraCenter()

            Return $"{Name} #{ImageId} center=({c(0):F2}, {c(1):F2}, {c(2):F2})"
        End Function

    End Class

    ''' <summary>
    ''' 一整套 COLMAP 稀疏重建结果。
    ''' </summary>
    Public Class ColmapScene

        Public Property SparseDir As String

        Public Property Cameras As New Dictionary(Of Integer, ColmapCamera)()

        ''' <summary>按图像文件名排序后的全部注册视图。</summary>
        Public Property Images As New List(Of ColmapImage)()

        ''' <summary>稀疏点坐标，长度 = PointCount × 3。</summary>
        Public Property Points As Double()

        ''' <summary>稀疏点颜色（0..1），长度 = PointCount × 3。</summary>
        Public Property Colors As Double()

        ''' <summary>重投影误差，长度 = PointCount。</summary>
        Public Property Errors As Double()

        Public ReadOnly Property PointCount As Integer
            Get
                If Points Is Nothing Then Return 0

                Return Points.Length \ 3
            End Get
        End Property

        ''' <summary>按名字排序（COLMAP 的 images.bin 顺序不保证有序）。</summary>
        Public Sub SortImages()
            Images.Sort(Function(a, b) String.CompareOrdinal(a.Name, b.Name))
        End Sub

        ''' <summary>
        ''' 稀疏点的包围盒对角线与平均点距估计（用于给高斯设置初始尺度）。
        ''' </summary>
        Public Function EstimateSpacing() As Double
            If PointCount = 0 Then Return 0.01

            Dim minV(2) As Double, maxV(2) As Double

            For i As Integer = 0 To 2
                minV(i) = Double.MaxValue
                maxV(i) = Double.MinValue
            Next

            For i As Integer = 0 To PointCount - 1
                For c As Integer = 0 To 2
                    Dim v As Double = Points(i * 3 + c)

                    If v < minV(c) Then minV(c) = v
                    If v > maxV(c) Then maxV(c) = v
                Next
            Next

            Dim dx As Double = maxV(0) - minV(0)
            Dim dy As Double = maxV(1) - minV(1)
            Dim dz As Double = maxV(2) - minV(2)
            Dim volume As Double = dx * dy * dz

            If volume > 0.0 Then
                Return std.Pow(volume / PointCount, 1.0 / 3.0)
            End If

            Dim diagonal As Double = std.Sqrt(dx * dx + dy * dy + dz * dz)

            Return std.Max(diagonal / std.Max(1, PointCount), 1.0E-6)
        End Function

    End Class

End Namespace
