#Region "Microsoft.VisualBasic::e322e54b59a6f9a6a988ff7caf1fb53c, physics\WorldPlayer\World\WorldHost.vb"

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

    '   Total Lines: 294
    '    Code Lines: 182 (61.90%)
    ' Comment Lines: 58 (19.73%)
    '    - Xml Docs: 72.41%
    ' 
    '   Blank Lines: 54 (18.37%)
    '     File Size: 10.99 KB


    ' Class SpawnOptions
    ' 
    '     Properties: Color, Density, Depth, Edge, Friction
    '                 Height, Kind, Label, Mass, Orientation
    '                 Radius, Restitution, Segment, SizeX, SizeY
    '                 SizeZ, Velocity, Width, X, Y
    '                 Z
    ' 
    ' Class WorldHost
    ' 
    '     Properties: BodyLimit, ContactCount, DynamicCount, KillPlaneY, LastError
    '                 Paused, SimTime, SleepingCount
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BuildCollider, BuildMesh, Find, Remove, Spawn
    ' 
    '     Sub: AddGround, Clear, ClearDynamic, Cull, EnforceLimit
    '          SetGravity, Update
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Engine.Physics
Imports Microsoft.VisualBasic.Engine.Physics.Collision3D
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

''' <summary>
''' 脚本创建物体时提交的参数包，由 JS 侧的对象字面量解析而来。
''' </summary>
Public Class SpawnOptions

    ''' <summary>形状：box / cube / cuboid / sphere / ball / cone / cylinder / tetra / ramp。</summary>
    Public Property Kind As String = "box"
    Public Property Label As String = ""

    ''' <summary>放置点（构造原点：盒体中心、斜坡底面中心等）。</summary>
    Public Property X As Double = 0.0
    Public Property Y As Double = 0.0
    Public Property Z As Double = 0.0

    ''' <summary>长方体三边尺寸。</summary>
    Public Property SizeX As Double = 1.0
    Public Property SizeY As Double = 1.0
    Public Property SizeZ As Double = 1.0
    ''' <summary>球 / 圆锥 / 圆柱的半径。</summary>
    Public Property Radius As Double = 0.5
    ''' <summary>圆锥 / 圆柱的高度。</summary>
    Public Property Height As Double = 1.5
    ''' <summary>圆锥 / 圆柱的圆周分段（越大越圆、越慢）。</summary>
    Public Property Segment As Integer = 14
    ''' <summary>四面体棱长。</summary>
    Public Property Edge As Double = 1.0
    ''' <summary>斜坡的宽度 / 高度 / 深度。</summary>
    Public Property Width As Double = 8.0
    Public Property Depth As Double = 8.0

    ''' <summary>质量。&lt;= 0 表示静态。</summary>
    Public Property Mass As Double = 1.0
    ''' <summary>密度 &gt; 0 时按体积换算质量。</summary>
    Public Property Density As Double = 0.0
    ''' <summary>摩擦系数。</summary>
    Public Property Friction As Double = 0.4
    ''' <summary>恢复系数。</summary>
    Public Property Restitution As Double = 0.0
    ''' <summary>朝向（四元数）。</summary>
    Public Property Orientation As Quaternion = Quaternion.Identity
    ''' <summary>初始速度。</summary>
    Public Property Velocity As Vector3 = Nothing
    ''' <summary>颜色。</summary>
    Public Property Color As Color = Color.CornflowerBlue
End Class

''' <summary>
''' 三维物理世界宿主：持有 <see cref="PhysicsWorld3D"/>，并把脚本侧的
''' "字符串 id + 参数包" 转换成刚体。所有物理细节都被封装在这里，
''' VB.NET 宿主代码不包含任何场景内容。
''' </summary>
Public Class WorldHost

    ''' <summary>底层物理世界。</summary>
    Public ReadOnly World As New PhysicsWorld3D()

    ''' <summary>全部可渲染刚体（含静态关卡几何）。</summary>
    Public ReadOnly Bodies As New List(Of BodyVisual)()

    Private ReadOnly index As New Dictionary(Of String, BodyVisual)()
    Private seq As Integer = 0

    ''' <summary>累计仿真时长（秒）。</summary>
    Public Property SimTime As Double = 0.0
    ''' <summary>是否暂停。</summary>
    Public Property Paused As Boolean = False
    ''' <summary>动态刚体上限，超出后回收最老的一个。</summary>
    Public Property BodyLimit As Integer = 220
    ''' <summary>低于该高度的刚体会被剔除（掉出世界）。</summary>
    Public Property KillPlaneY As Double = -60.0

    ''' <summary>上一次运行的脚本错误（Nothing 表示正常）。</summary>
    Public Property LastError As String = Nothing

    Public Sub New()
        ' 交互式场景的实时性优先：配合跨帧热启动，8 次迭代 × 2 个子步
        ' 已能让堆叠稳定静止，而求解开销只有库默认值（16 × 4）的四分之一。
        ' 脚本可以用 sim.iterations(n) / sim.substeps(n) 再调整。
        World.Iterations = 8
        World.Substeps = 2
    End Sub

    ' /********************************************************************************/
    '  生命周期
    ' /********************************************************************************/

    ''' <summary>清空世界中的所有刚体。</summary>
    Public Sub Clear()
        Call World.Clear()
        Call Bodies.Clear()
        Call index.Clear()

        seq = 0
        SimTime = 0.0
    End Sub

    ''' <summary>只移除动态刚体，保留地面 / 平台 / 斜坡等关卡几何。</summary>
    Public Sub ClearDynamic()
        For i As Integer = Bodies.Count - 1 To 0 Step -1
            If Not Bodies(i).IsStaticGeometry Then
                Call Remove(Bodies(i).Id)
            End If
        Next
    End Sub

    ''' <summary>推进一个真实帧：步进物理、累加物体年龄并回收越界物体。</summary>
    Public Sub Update(dt As Double)
        If Paused Then Return

        World.Step(dt)
        SimTime += dt

        ' 年龄用于"刚生成时高亮、随后渐变回本色"的表现
        For Each v In Bodies
            v.Age += dt
        Next

        Call Cull()
    End Sub

    Private Sub Cull()
        For i As Integer = Bodies.Count - 1 To 0 Step -1
            Dim v As BodyVisual = Bodies(i)

            If v.IsStaticGeometry Then Continue For
            If v.Body.Position.y > KillPlaneY Then Continue For

            Call Remove(v.Id)
        Next
    End Sub

    ' /********************************************************************************/
    '  创建
    ' /********************************************************************************/

    ''' <summary>
    ''' 按参数包创建一个刚体。放置点 <paramref name="o"/> 是"构造原点"，
    ''' 内部按 <c>Position = 放置点 + q · PivotOffset</c> 换算到质心。
    ''' </summary>
    Public Function Spawn(o As SpawnOptions) As String
        seq += 1

        Dim id As String = $"b{seq}"
        Dim mesh As ConvexMesh3D = BuildMesh(o)
        Dim collider As Collider3D = Nothing
        Dim mass As Double = If(o.Mass > 0.0, o.Mass, 1.0)

        If o.Mass <= 0.0 Then
            ' 静态
            collider = BuildCollider(o, mesh)
        Else
            collider = BuildCollider(o, mesh)

            If o.Density > 0.0 Then
                mass = o.Density * std.Max(mesh.Volume(), 1.0E-6)
            End If
        End If

        Dim body As New RigidBody3D(collider, mass, New PhysicsMaterial(o.Friction, o.Restitution))

        body.ID = id
        body.Label = If(String.IsNullOrEmpty(o.Label), id, o.Label)
        body.Position = New Vector3(o.X, o.Y, o.Z) + o.Orientation.Rotate(mesh.PivotOffset)
        body.Orientation = o.Orientation

        If o.Velocity IsNot Nothing Then
            body.Velocity = o.Velocity
        End If

        If o.Mass <= 0.0 Then
            Call body.SetStatic()
        End If

        World.Add(body)

        Dim visual As New BodyVisual With {
            .Id = id,
            .Label = body.Label,
            .Body = body,
            .Mesh = mesh,
            .Color = o.Color,
            .IsStaticGeometry = body.IsStatic,
            .Age = 0.0
        }

        Bodies.Add(visual)
        index(id) = visual

        Call EnforceLimit()

        Return id
    End Function

    ''' <summary>动态刚体超限时回收最老的一个。</summary>
    Private Sub EnforceLimit()
        If Bodies.Count <= BodyLimit Then Return

        Dim dynamic As New List(Of BodyVisual)(Bodies.Where(Function(v) Not v.IsStaticGeometry))

        While dynamic.Count > BodyLimit
            Call Remove(dynamic(0).Id)
            dynamic.RemoveAt(0)
        End While
    End Sub

    Private Function BuildMesh(o As SpawnOptions) As ConvexMesh3D
        Select Case o.Kind.ToLower()
            Case "sphere", "ball"
                Return ConvexMesh3D.CreateSphereMesh(o.Radius, 12, 8)
            Case "cone", "pyramid"
                Return ConvexMesh3D.CreateCone(o.Radius, o.Height, o.Segment)
            Case "cylinder", "cyl"
                Return ConvexMesh3D.CreateCylinder(o.Radius, o.Height, o.Segment)
            Case "tetra", "triangle", "tetrahedron"
                Return ConvexMesh3D.CreateTetrahedron(o.Edge)
            Case "ramp", "wedge", "slope"
                Return ConvexMesh3D.CreateWedge(o.Width, o.SizeY, o.Depth)
            Case Else
                ' box / cube / cuboid / 未知都退化为长方体
                Return ConvexMesh3D.CreateBox(o.SizeX, o.SizeY, o.SizeZ)
        End Select
    End Function

    Private Function BuildCollider(o As SpawnOptions, mesh As ConvexMesh3D) As Collider3D
        Select Case o.Kind.ToLower()
            Case "sphere", "ball"
                Return New SphereCollider3D(o.Radius)
            Case "box", "cube", "cuboid"
                ' 盒体仍走解析形状：盒-盒/盒-凸会被转成等价凸壳
                Return New BoxCollider3D(o.SizeX, o.SizeY, o.SizeZ)
            Case Else
                Return New ConvexPolyhedronCollider3D(mesh)
        End Select
    End Function

    ' /********************************************************************************/
    '  查询与操作
    ' /********************************************************************************/

    ''' <summary>按 id 或 label 查找。</summary>
    Public Function Find(idOrLabel As String) As BodyVisual
        If String.IsNullOrEmpty(idOrLabel) Then Return Nothing
        If index.ContainsKey(idOrLabel) Then Return index(idOrLabel)

        Return Bodies.FirstOrDefault(Function(v) v.Label = idOrLabel)
    End Function

    Public Function Remove(idOrLabel As String) As Boolean
        Dim v As BodyVisual = Find(idOrLabel)

        If v Is Nothing Then Return False

        Call index.Remove(v.Id)
        Call Bodies.Remove(v)
        Call World.Remove(v.Body)

        Return True
    End Function

    ''' <summary>动态刚体数量。</summary>
    Public ReadOnly Property DynamicCount As Integer
        Get
            Return Bodies.Where(Function(v) Not v.IsStaticGeometry).Count()
        End Get
    End Property

    ''' <summary>最近一次步进产生的接触流形数量。</summary>
    Public ReadOnly Property ContactCount As Integer
        Get
            Return World.Contacts.Count
        End Get
    End Property

    ''' <summary>累计进入休眠的刚体数量。</summary>
    Public ReadOnly Property SleepingCount As Integer
        Get
            Return Bodies.Where(Function(v) Not v.IsStaticGeometry AndAlso v.Body.IsSleeping).Count()
        End Get
    End Property

    Public Sub SetGravity(x As Double, y As Double, z As Double)
        World.Gravity = New Vector3(x, y, z)
    End Sub

    Public Sub AddGround(height As Double)
        World.Add(PhysicsWorld3D.GroundPlane(height, New PhysicsMaterial(0.55, 0.0)))
    End Sub
End Class
