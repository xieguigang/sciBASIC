#Region "Microsoft.VisualBasic::fcfd6f8aa2a311c65bdd3b4495384d06, physics\WorldPlayer\Rendering\CameraRig.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 302
    '    Code Lines: 179 (59.27%)
    ' Comment Lines: 69 (22.85%)
    '    - Xml Docs: 84.06%
    ' 
    '   Blank Lines: 54 (17.88%)
    '     File Size: 11.86 KB


    ' Class CameraRig
    ' 
    '     Properties: AngleX, AngleY, Distance, FocusX, FocusY
    '                 FocusZ, Smoothing, TargetAngleX, TargetAngleY, TargetViewDistance
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: RotationMatrix, ScreenToPlane, ScreenToRay, ToPhysics, ToRender
    '               Unrotate, WrapDeg
    ' 
    '     Sub: CaptureHome, Orbit, Reset, SetAngles, SetDistance
    '          SetFieldOfView, SetFocus, SetScreen, Tick, Zoom
    '     Class Matrix3x3D
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Engine.Physics
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports std = System.Math

''' <summary>
''' 相机装置：包装 Drawing3D 的 <see cref="Camera"/>（欧拉角 + 视距），
''' 额外提供轨道旋转、缩放与"屏幕坐标 → 世界射线 / 平面"反投影。
''' </summary>
''' <remarks>
''' <see cref="Camera"/> 没有 LookAt / Unproject：相机永远位于世界 -Z 侧、距离
''' <see cref="Camera.ViewDistance"/> 处朝 +Z 看，朝向靠旋转整个世界实现。
''' 因此这里复刻 <c>Camera.vb:189-210</c> 的旋转矩阵 M（行向量约定 result = v·M），
''' 再用 Mᵀ 把相机空间点变换回渲染空间；最后把渲染坐标变换回物理坐标
''' （渲染空间 X→右、Y→下、Z→远，物理空间 Y 向上）。
''' </remarks>
Public Class CameraRig

    ''' <summary>底层相机（每帧同步 Screen 与角度）。</summary>
    Public ReadOnly Camera As New Camera()

    ''' <summary>目标俯仰角（度）。</summary>
    Public Property TargetAngleX As Double = 24.0
    ''' <summary>目标偏航角（度）。</summary>
    Public Property TargetAngleY As Double = -38.0
    ''' <summary>目标视距。</summary>
    Public Property TargetViewDistance As Double = 48.0

    ''' <summary>世界焦点（物理坐标）：渲染时从所有几何中减去，使焦点落在屏幕中心。</summary>
    Public Property FocusX As Double = 0.0
    Public Property FocusY As Double = 2.5
    Public Property FocusZ As Double = 0.0

    ''' <summary>每帧逼近目标的比例（缓动）。</summary>
    Public Property Smoothing As Double = 0.18

    ''' <summary>视距上下限。</summary>
    Public Const MinDistance As Double = 8.0
    Public Const MaxDistance As Double = 400.0

    Public Sub New()
        Camera.AngleX = CSng(TargetAngleX)
        Camera.AngleY = CSng(TargetAngleY)
        Camera.AngleZ = 0.0F
        Camera.FieldOfView = 256.0F
        Camera.ViewDistance = CSng(TargetViewDistance)
        Camera.Offset = New PointF(0, 0)
        Camera.LightDirection = New Point3D(2, -1, 3).Normalize()
        Camera.AmbientStrength = 0.28
        Camera.LightColor = Color.White
    End Sub

    ''' <summary>同步画布尺寸（尺寸变化时必须调用）。</summary>
    Public Sub SetScreen(size As Size)
        Camera.Screen = size
    End Sub

    ''' <summary>每帧推进缓动插值。</summary>
    Public Sub Tick()
        Camera.AngleX += (TargetAngleX - Camera.AngleX) * Smoothing
        Camera.AngleY += (TargetAngleY - Camera.AngleY) * Smoothing
        Camera.ViewDistance += CSng(TargetViewDistance - Camera.ViewDistance) * Smoothing
    End Sub

    ''' <summary>轨道旋转（度）。</summary>
    Public Sub Orbit(dx As Double, dy As Double)
        TargetAngleY = WrapDeg(TargetAngleY + dx)
        TargetAngleX = std.Max(-89.0, std.Min(89.0, TargetAngleX + dy))
    End Sub

    ''' <summary>缩放：视距乘以 factor。</summary>
    Public Sub Zoom(factor As Double)
        If factor <= 0.0 Then Return

        TargetViewDistance = std.Max(MinDistance, std.Min(MaxDistance, TargetViewDistance * factor))
    End Sub

    ' 脚本登记的"主视角"，camera.reset() 会恢复到这里
    Private homeX As Double = 24.0
    Private homeY As Double = -38.0
    Private homeDistance As Double = 48.0
    Private homeFx As Double = 0.0
    Private homeFy As Double = 2.5
    Private homeFz As Double = 0.0

    ''' <summary>
    ''' 把当前目标机位登记为"主视角"。脚本在 <c>setup()</c> 末尾调用一次，
    ''' 之后无论用户怎么转视角，<c>camera.reset()</c> 都能回到这个机位。
    ''' </summary>
    Public Sub CaptureHome()
        homeX = TargetAngleX
        homeY = TargetAngleY
        homeDistance = TargetViewDistance

        homeFx = FocusX
        homeFy = FocusY
        homeFz = FocusZ
    End Sub

    ''' <summary>重置为脚本登记的主视角。</summary>
    Public Sub Reset()
        TargetAngleX = homeX
        TargetAngleY = homeY
        TargetViewDistance = homeDistance
        FocusX = homeFx
        FocusY = homeFy
        FocusZ = homeFz
    End Sub

    ''' <summary>设置焦点（物理坐标）。</summary>
    Public Sub SetFocus(x As Double, y As Double, z As Double)
        FocusX = x
        FocusY = y
        FocusZ = z
    End Sub

    ''' <summary>设置欧拉角（度）。</summary>
    Public Sub SetAngles(ax As Double, ay As Double)
        TargetAngleX = std.Max(-89.0, std.Min(89.0, ax))
        TargetAngleY = WrapDeg(ay)
    End Sub

    ''' <summary>设置视距。</summary>
    Public Sub SetDistance(d As Double)
        TargetViewDistance = std.Max(MinDistance, std.Min(MaxDistance, d))
    End Sub

    ''' <summary>
    ''' 设置投影放大系数（Drawing3D 里叫 FieldOfView）。
    ''' 库内公式是 <c>factor = FieldOfView / depth</c>，因此<b>值越大画面越近</b>，
    ''' 与真实相机"FOV 越大视野越宽"的直觉相反。
    ''' </summary>
    Public Sub SetFieldOfView(v As Double)
        Camera.FieldOfView = CSng(std.Max(16.0, std.Min(4000.0, v)))
    End Sub

    ''' <summary>当前视距（瞬时值）。</summary>
    Public ReadOnly Property Distance As Double
        Get
            Return Camera.ViewDistance
        End Get
    End Property

    Public ReadOnly Property AngleX As Double
        Get
            Return Camera.AngleX
        End Get
    End Property

    Public ReadOnly Property AngleY As Double
        Get
            Return Camera.AngleY
        End Get
    End Property

    Private Shared Function WrapDeg(a As Double) As Double
        Dim x As Double = a Mod 360.0

        Return If(x < 0.0, x + 360.0, x)
    End Function

    ' /********************************************************************************/
    '  坐标变换
    ' /********************************************************************************/

    ''' <summary>物理坐标 → 渲染坐标（相对焦点）。</summary>
    ''' <remarks>
    ''' 物理空间：X 向右、Y 向上、Z 朝观察者；渲染空间：X 向右、Y 向下、Z 向远。
    ''' 因此映射为 <c>(x, -y, -z)</c>：行列式 = (+1)·(-1)·(-1) = +1，是<b>真旋转</b>，
    ''' 既不镜像也不翻转面绕向与光照。
    ''' 若写成 <c>(x, -z, y)</c>（把"高度"映射到深度、"前后"映射到屏幕竖直方向），
    ''' 得到的其实是俯视图，地面会呈现"近处在上、远处在下"的倒置观感。
    ''' </remarks>
    Public Function ToRender(x As Double, y As Double, z As Double) As Point3D
        Return New Point3D(x - FocusX, -(y - FocusY), -(z - FocusZ))
    End Function

    ''' <summary>渲染坐标 → 物理坐标（<see cref="ToRender"/> 的逆变换）。</summary>
    Public Function ToPhysics(r As Point3D) As Vector3
        Return New Vector3(r.X + FocusX, -r.Y + FocusY, -r.Z + FocusZ)
    End Function

    ''' <summary>
    ''' 屏幕坐标 → 世界射线（物理坐标）。
    ''' 先把屏幕点反投影到相机空间 <c>Zc = 0</c> 的平面上，再用 Mᵀ 变换回渲染空间。
    ''' </summary>
    Public Function ScreenToRay(sx As Double, sy As Double,
                                ByRef ox As Double, ByRef oy As Double, ByRef oz As Double,
                                ByRef dx As Double, ByRef dy As Double, ByRef dz As Double) As Boolean
        Dim w As Double = Camera.Screen.Width
        Dim h As Double = Camera.Screen.Height
        Dim fov As Double = Camera.FieldOfView
        Dim vd As Double = Camera.ViewDistance
        Dim offX As Double = Camera.Offset.X
        Dim offY As Double = Camera.Offset.Y

        If w <= 0.0 OrElse h <= 0.0 OrElse fov <= 0.0 OrElse vd <= 0.0 Then Return False

        ' 相机空间：Zc = 0 平面上的点（depth = ViewDistance）
        Dim cx As Double = (sx - w / 2.0 - offX) * vd / fov
        Dim cy As Double = (sy - h / 2.0 - offY) * vd / fov

        Dim m As Matrix3x3D = RotationMatrix()

        ' 相机位置在相机空间 (0, 0, -VD)
        Dim eyeR As Point3D = Unrotate(0.0, 0.0, -vd, m)
        Dim hitR As Point3D = Unrotate(cx, cy, 0.0, m)

        ' 渲染 → 物理：(rx, ry, rz) → (rx + Fx, -ry + Fy, -rz + Fz)
        ox = eyeR.X + FocusX
        oy = -eyeR.Y + FocusY
        oz = -eyeR.Z + FocusZ

        ' 方向是线性部分：物理 = (drx, -dry, -drz)
        Dim rx As Double = hitR.X - eyeR.X
        Dim ry As Double = -(hitR.Y - eyeR.Y)
        Dim rz As Double = -(hitR.Z - eyeR.Z)
        Dim len As Double = std.Sqrt(rx * rx + ry * ry + rz * rz)

        If len < 1.0E-9 Then Return False

        dx = rx / len
        dy = ry / len
        dz = rz / len

        Return True
    End Function

    ''' <summary>
    ''' 屏幕坐标 → 与平面 <c>dot(n, p) = d</c> 的交点（物理坐标）。
    ''' </summary>
    Public Function ScreenToPlane(sx As Double, sy As Double,
                                  nx As Double, ny As Double, nz As Double, d As Double,
                                  ByRef px As Double, ByRef py As Double, ByRef pz As Double) As Boolean
        Dim ox, oy, oz, dx, dy, dz As Double

        If Not ScreenToRay(sx, sy, ox, oy, oz, dx, dy, dz) Then Return False

        Dim nl As Double = std.Sqrt(nx * nx + ny * ny + nz * nz)

        If nl < 1.0E-9 Then Return False

        nx /= nl : ny /= nl : nz /= nl
        d /= nl

        Dim denom As Double = nx * dx + ny * dy + nz * dz

        If std.Abs(denom) < 1.0E-9 Then Return False

        Dim t As Double = (d - (nx * ox + ny * oy + nz * oz)) / denom

        If t < 0.0 Then Return False

        px = ox + dx * t
        py = oy + dy * t
        pz = oz + dz * t

        Return True
    End Function

    ''' <summary>
    ''' 与 <see cref="Camera"/> 内部等价的旋转矩阵（行向量约定 result = v·M）。
    ''' 元素命名沿用 <c>Camera.RotationMatrix()</c>：result.X = v·M 的第一列。
    ''' </summary>
    Private Function RotationMatrix() As Matrix3x3D
        Dim rX As Double = Camera.AngleX * std.PI / 180.0
        Dim rY As Double = Camera.AngleY * std.PI / 180.0
        Dim rZ As Double = Camera.AngleZ * std.PI / 180.0
        Dim cX As Double = std.Cos(rX), sX As Double = std.Sin(rX)
        Dim cY As Double = std.Cos(rY), sY As Double = std.Sin(rY)
        Dim cZ As Double = std.Cos(rZ), sZ As Double = std.Sin(rZ)
        Dim m As New Matrix3x3D()

        ' result.X = p.X*M11 + p.Y*M21 + p.Z*M31
        m.M11 = cY * cZ
        m.M21 = sX * sY * cZ - cX * sZ
        m.M31 = cX * sY * cZ + sX * sZ
        ' result.Y
        m.M12 = cY * sZ
        m.M22 = sX * sY * sZ + cX * cZ
        m.M32 = cX * sY * sZ - sX * cZ
        ' result.Z
        m.M13 = -sY
        m.M23 = sX * cY
        m.M33 = cX * cY

        Return m
    End Function

    ''' <summary>用 Mᵀ 把相机空间点变换回渲染空间（行向量约定下逆变换即转置）。</summary>
    Private Shared Function Unrotate(cx As Double, cy As Double, cz As Double, m As Matrix3x3D) As Point3D
        Return New Point3D(
            cx * m.M11 + cy * m.M12 + cz * m.M13,
            cx * m.M21 + cy * m.M22 + cz * m.M23,
            cx * m.M31 + cy * m.M32 + cz * m.M33)
    End Function

    ''' <summary>小型 3x3 矩阵容器，避免为一次反投影去构造 Drawing3D 的矩阵类型。</summary>
    Private Class Matrix3x3D
        Public M11, M12, M13, M21, M22, M23, M31, M32, M33 As Double
    End Class
End Class

