Imports System.IO
Imports System.Text.Json

''' <summary>
''' 相机内参模型 - 针孔相机
''' Pinhole camera intrinsics: maps camera-space 3D points to pixel coordinates.
''' </summary>
Public Class CameraIntrinsics

    ''' <summary>水平焦距（像素单位）</summary>
    Public Property Fx As Double

    ''' <summary>垂直焦距（像素单位）</summary>
    Public Property Fy As Double

    ''' <summary>主点 x 坐标（像素）</summary>
    Public Property Cx As Double

    ''' <summary>主点 y 坐标（像素）</summary>
    Public Property Cy As Double

    ''' <summary>图像宽度</summary>
    Public Property Width As Integer

    ''' <summary>图像高度</summary>
    Public Property Height As Integer

    Public Sub New(fx As Double, fy As Double, cx As Double, cy As Double, width As Integer, height As Integer)
        Me.Fx = fx
        Me.Fy = fy
        Me.Cx = cx
        Me.Cy = cy
        Me.Width = width
        Me.Height = height
    End Sub

    ''' <summary>
    ''' 将相机坐标系下的 3D 点投影到像素坐标
    ''' Project a camera-space 3D point to pixel coordinates.
    ''' </summary>
    ''' <param name="x">相机坐标 X</param>
    ''' <param name="y">相机坐标 Y</param>
    ''' <param name="z">相机坐标 Z（必须 &gt; 0）</param>
    ''' <param name="u">输出：像素 x 坐标</param>
    ''' <param name="v">输出：像素 y 坐标</param>
    Public Sub Project(x As Double, y As Double, z As Double, ByRef u As Double, ByRef v As Double)
        If z <= 0 Then
            u = Double.NaN
            v = Double.NaN
            Return
        End If
        u = Fx * x / z + Cx
        v = Fy * y / z + Cy
    End Sub

    Public Function Clone() As CameraIntrinsics
        Return New CameraIntrinsics(Fx, Fy, Cx, Cy, Width, Height)
    End Function

End Class


''' <summary>
''' 相机外参模型 - 世界到相机的变换
''' Camera extrinsics: world-to-camera transformation (R, t).
''' 采用 OpenCV 约定：相机看向 +Z，X 向右，Y 向下。
''' </summary>
Public Class CameraExtrinsics

    ''' <summary>3x3 旋转矩阵（行优先存储）</summary>
    Public Property R As Double(,)

    ''' <summary>3x1 平移向量</summary>
    Public Property T As Double()

    Public Sub New(r As Double(,), t As Double())
        Me.R = r
        Me.T = t
    End Sub

    ''' <summary>
    ''' 将世界坐标点变换到相机坐标系
    ''' Transform a world-space point to camera space.
    ''' P_cam = R * P_world + t
    ''' </summary>
    Public Sub WorldToCamera(wx As Double, wy As Double, wz As Double,
                             ByRef cx As Double, ByRef cy As Double, ByRef cz As Double)
        cx = R(0, 0) * wx + R(0, 1) * wy + R(0, 2) * wz + T(0)
        cy = R(1, 0) * wx + R(1, 1) * wy + R(1, 2) * wz + T(1)
        cz = R(2, 0) * wx + R(2, 1) * wy + R(2, 2) * wz + T(2)
    End Sub

    ''' <summary>
    ''' 获取相机在世界坐标系中的位置
    ''' Camera position in world space: C = -R^T * t
    ''' </summary>
    Public ReadOnly Property Eye As Double()
        Get
            Dim e(2) As Double
            ' C = -R^T * t
            For i = 0 To 2
                Dim s = 0.0
                For j = 0 To 2
                    s += R(j, i) * T(j)
                Next
                e(i) = -s
            Next
            Return e
        End Get
    End Property

    Public Function Clone() As CameraExtrinsics
        Dim r2(2, 2) As Double
        Dim t2(2) As Double
        Array.Copy(R, r2, 9)
        Array.Copy(T, t2, 3)
        Return New CameraExtrinsics(r2, t2)
    End Function

End Class


''' <summary>
''' 完整的相机模型（内参 + 外参）
''' </summary>
Public Class Camera

    ''' <summary>相机内参</summary>
    Public Property Intrinsics As CameraIntrinsics

    ''' <summary>相机外参</summary>
    Public Property Extrinsics As CameraExtrinsics

    ''' <summary>视图编号</summary>
    Public Property ViewId As Integer

    Public Sub New(intrinsics As CameraIntrinsics, extrinsics As CameraExtrinsics, viewId As Integer)
        Me.Intrinsics = intrinsics
        Me.Extrinsics = extrinsics
        Me.ViewId = viewId
    End Sub

    ''' <summary>
    ''' 将世界坐标点投影到像素坐标
    ''' </summary>
    Public Sub ProjectWorldPoint(wx As Double, wy As Double, wz As Double,
                                 ByRef u As Double, ByRef v As Double, ByRef depth As Double)
        Dim cx, cy, cz As Double
        Extrinsics.WorldToCamera(wx, wy, wz, cx, cy, cz)
        depth = cz
        Intrinsics.Project(cx, cy, cz, u, v)
    End Sub

    ''' <summary>
    ''' 从 JSON 文件加载所有相机
    ''' Load all cameras from a JSON file produced by generate_scene.py.
    ''' </summary>
    Public Shared Function LoadFromJson(path As String) As (Intrinsics As CameraIntrinsics, Cameras As List(Of Camera))
        Dim json As String = File.ReadAllText(path)
        Dim doc As JsonDocument = JsonDocument.Parse(json)

        Dim intrRoot = doc.RootElement.GetProperty("intrinsics")
        Dim fx = intrRoot.GetProperty("fx").GetDouble()
        Dim fy = intrRoot.GetProperty("fy").GetDouble()
        Dim cx = intrRoot.GetProperty("cx").GetDouble()
        Dim cy = intrRoot.GetProperty("cy").GetDouble()
        Dim w = intrRoot.GetProperty("width").GetInt32()
        Dim h = intrRoot.GetProperty("height").GetInt32()
        Dim intr = New CameraIntrinsics(fx, fy, cx, cy, w, h)

        Dim cams As New List(Of Camera)()
        Dim camsArr = doc.RootElement.GetProperty("cameras")
        For Each camElem In camsArr.EnumerateArray()
            Dim r(2, 2) As Double
            Dim t(2) As Double
            Dim rArr = camElem.GetProperty("R")
            Dim tArr = camElem.GetProperty("t")
            ' R 是 3x3 嵌套数组，需要逐行枚举
            Dim rRows = rArr.EnumerateArray().ToArray()
            For i = 0 To 2
                Dim cols = rRows(i).EnumerateArray().ToArray()
                For j = 0 To 2
                    r(i, j) = cols(j).GetDouble()
                Next
            Next
            ' t 是长度为 3 的一维数组
            Dim tElems = tArr.EnumerateArray().ToArray()
            For i = 0 To 2
                t(i) = tElems(i).GetDouble()
            Next
            Dim viewId = camElem.GetProperty("view_id").GetInt32()
            Dim ext As New CameraExtrinsics(r, t)
            cams.Add(New Camera(intr.Clone(), ext, viewId))
        Next

        Return (intr, cams)
    End Function

End Class
