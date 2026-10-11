' /********************************************************************************/
'
'   SphBoundary3D.vb
'
'   3D SPH 计算域边界抽象
'
'   作用：
'       旧引擎把容器硬编码为轴对齐长方盒（HandleCollisions 内 6 面反弹），
'       无法表达发酵罐这类圆柱容器。这里把边界抽象为可替换对象：
'
'         - BoxBoundary3D        —— 与旧引擎完全等价的长方盒（默认，零回归）
'         - CylinderBoundary3D   —— 圆柱罐壁 + 罐底 + 顶部开口（自由液面）
'
'   设计说明：
'       - Project(state, i, padding, damping) 只处理单个粒子：把越界粒子推回域内
'         并反射（衰减）其法向速度分量，切向分量保留（自由滑移）或按摩擦衰减。
'       - Domain() 给出包围盒，供 UniformGrid3D 建格使用。
'       - 本类型是纯 CPU 逻辑（每步 O(n)），不进入 GPU 内核，因此 CPU/GPU
'         两条路径的边界行为完全一致。
'
' /********************************************************************************/

Imports std = System.Math

''' <summary>
''' The computational domain boundary of the 3D SPH solver: pushes escaped
''' particles back into the domain and resolves the wall collision response.
''' </summary>
Public MustInherit Class SphBoundary3D

    ''' <summary>
    ''' apply the boundary constraint onto a single particle.
    ''' </summary>
    ''' <param name="state">the SoA particle state</param>
    ''' <param name="i">particle index</param>
    ''' <param name="padding">wall padding (half of the particle size)</param>
    ''' <param name="damping">normal restitution damping, 0 = fully inelastic</param>
    Public MustOverride Sub Project(state As SphState3D, i As Integer, padding As Single, damping As Single)

    ''' <summary>
    ''' the axis aligned bounding box of the domain (used to size the uniform grid).
    ''' </summary>
    Public MustOverride Function Domain() As (minX As Single, minY As Single, minZ As Single, maxX As Single, maxY As Single, maxZ As Single)

End Class

''' <summary>
''' The axis aligned box domain used by the original engine: the fluid lives
''' inside <c>[0,size]</c> on every axis and escapes are bounced back with the
''' configured damping. This is the default boundary so that the existing
''' behaviour of <see cref="FluidEngine3D"/> is preserved.
''' </summary>
Public Class BoxBoundary3D : Inherits SphBoundary3D

    ''' <summary>box extent along x</summary>
    Public Property SizeX As Single
    ''' <summary>box extent along y</summary>
    Public Property SizeY As Single
    ''' <summary>box extent along z</summary>
    Public Property SizeZ As Single

    Sub New(sizeX As Single, sizeY As Single, sizeZ As Single)
        Me.SizeX = sizeX
        Me.SizeY = sizeY
        Me.SizeZ = sizeZ
    End Sub

    Public Overrides Sub Project(state As SphState3D, i As Integer, padding As Single, damping As Single)
        Dim x = state.px(i), y = state.py(i), z = state.pz(i)
        Dim vx = state.vx(i), vy = state.vy(i), vz = state.vz(i)

        If x > SizeX - padding Then
            x = SizeX - padding
            If vx > 0 Then vx = -vx * damping
        ElseIf x < padding Then
            x = padding
            If vx < 0 Then vx = -vx * damping
        End If

        If y > SizeY - padding Then
            y = SizeY - padding
            If vy > 0 Then vy = -vy * damping
        ElseIf y < padding Then
            y = padding
            If vy < 0 Then vy = -vy * damping
        End If

        If z > SizeZ - padding Then
            z = SizeZ - padding
            If vz > 0 Then vz = -vz * damping
        ElseIf z < padding Then
            z = padding
            If vz < 0 Then vz = -vz * damping
        End If

        state.px(i) = x
        state.py(i) = y
        state.pz(i) = z
        state.vx(i) = vx
        state.vy(i) = vy
        state.vz(i) = vz
    End Sub

    Public Overrides Function Domain() As (minX As Single, minY As Single, minZ As Single, maxX As Single, maxY As Single, maxZ As Single)
        Return (0.0F, 0.0F, 0.0F, SizeX, SizeY, SizeZ)
    End Function

End Class

''' <summary>
''' A vertical cylindrical vessel (axis along +Z, Z is the "up" direction of
''' the fermenter): a radial wall of the given radius, a flat bottom and an
''' open top (the free surface of the fermentation broth).
''' </summary>
Public Class CylinderBoundary3D : Inherits SphBoundary3D

    ''' <summary>axis position along x</summary>
    Public Property CenterX As Single
    ''' <summary>axis position along y</summary>
    Public Property CenterY As Single
    ''' <summary>inner radius of the vessel</summary>
    Public Property Radius As Single
    ''' <summary>z of the vessel bottom</summary>
    Public Property ZBottom As Single
    ''' <summary>z of the vessel top (open / free surface)</summary>
    Public Property ZTop As Single

    ''' <summary>
    ''' tangential wall friction: 0 = free slip, 1 = no slip (the tangential
    ''' velocity is fully removed at the wall).
    ''' </summary>
    Public Property WallFriction As Single = 0.15F

    Sub New(centerX As Single, centerY As Single, radius As Single, zBottom As Single, zTop As Single)
        Me.CenterX = centerX
        Me.CenterY = centerY
        Me.Radius = radius
        Me.ZBottom = zBottom
        Me.ZTop = zTop
    End Sub

    Public Overrides Sub Project(state As SphState3D, i As Integer, padding As Single, damping As Single)
        Dim dx = state.px(i) - CenterX
        Dim dy = state.py(i) - CenterY
        Dim r = CSng(std.Sqrt(dx * dx + dy * dy))
        Dim limit = Radius - padding

        If r > limit AndAlso r > 0.000001F Then
            ' push back onto the wall and reflect the radial velocity component
            Dim nx = dx / r, ny = dy / r
            state.px(i) = CenterX + nx * limit
            state.py(i) = CenterY + ny * limit

            Dim vr = state.vx(i) * nx + state.vy(i) * ny
            If vr > 0 Then
                Dim tx = state.vx(i) - vr * nx
                Dim ty = state.vy(i) - vr * ny
                Dim vn = -vr * damping
                Dim slip = 1.0F - WallFriction

                state.vx(i) = tx * slip + vn * nx
                state.vy(i) = ty * slip + vn * ny
            End If
        End If

        ' bottom plate
        If state.pz(i) < ZBottom + padding Then
            state.pz(i) = ZBottom + padding
            If state.vz(i) < 0 Then state.vz(i) = -state.vz(i) * damping
        End If

        ' open top: only a safety clamp so that particles never leave the
        ' uniform grid of the neighbour search
        If state.pz(i) > ZTop - padding Then
            state.pz(i) = ZTop - padding
            If state.vz(i) > 0 Then state.vz(i) = -state.vz(i) * damping
        End If
    End Sub

    Public Overrides Function Domain() As (minX As Single, minY As Single, minZ As Single, maxX As Single, maxY As Single, maxZ As Single)
        Return (CenterX - Radius, CenterY - Radius, ZBottom,
                CenterX + Radius, CenterY + Radius, ZTop)
    End Function

End Class
