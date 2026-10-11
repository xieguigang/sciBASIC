Imports std = System.Math

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' 三维轴对齐包围盒（Double 精度），作为八叉树等空间划分结构的基础几何单元。
    ''' </summary>
    Public Structure BoundingBox3D

        Public MinX As Double
        Public MinY As Double
        Public MinZ As Double
        Public MaxX As Double
        Public MaxY As Double
        Public MaxZ As Double

        Sub New(minX As Double, minY As Double, minZ As Double, maxX As Double, maxY As Double, maxZ As Double)
            Me.MinX = minX
            Me.MinY = minY
            Me.MinZ = minZ
            Me.MaxX = maxX
            Me.MaxY = maxY
            Me.MaxZ = maxZ
        End Sub

        ''' <summary>由一组三维坐标点构建包围盒。</summary>
        Public Shared Function FromPoints(x As IEnumerable(Of Double), y As IEnumerable(Of Double), z As IEnumerable(Of Double)) As BoundingBox3D
            Dim xs As Double() = x.ToArray
            Dim ys As Double() = y.ToArray
            Dim zs As Double() = z.ToArray
            Dim minX As Double = xs.Min, minY As Double = ys.Min, minZ As Double = zs.Min
            Dim maxX As Double = xs.Max, maxY As Double = ys.Max, maxZ As Double = zs.Max
            Return New BoundingBox3D(minX, minY, minZ, maxX, maxY, maxZ)
        End Function

        Public ReadOnly Property CenterX As Double
            Get
                Return (MinX + MaxX) * 0.5
            End Get
        End Property

        Public ReadOnly Property CenterY As Double
            Get
                Return (MinY + MaxY) * 0.5
            End Get
        End Property

        Public ReadOnly Property CenterZ As Double
            Get
                Return (MinZ + MaxZ) * 0.5
            End Get
        End Property

        Public ReadOnly Property SizeX As Double
            Get
                Return MaxX - MinX
            End Get
        End Property

        Public ReadOnly Property SizeY As Double
            Get
                Return MaxY - MinY
            End Get
        End Property

        Public ReadOnly Property SizeZ As Double
            Get
                Return MaxZ - MinZ
            End Get
        End Property

        Public ReadOnly Property Volume As Double
            Get
                Return SizeX * SizeY * SizeZ
            End Get
        End Property

        ''' <summary>点是否落在包围盒内（含边界）。</summary>
        Public Function Contains(x As Double, y As Double, z As Double) As Boolean
            Return x >= MinX AndAlso x <= MaxX AndAlso
                   y >= MinY AndAlso y <= MaxY AndAlso
                   z >= MinZ AndAlso z <= MaxZ
        End Function

        ''' <summary>与另一个包围盒是否相交（接触算相交）。</summary>
        Public Function Overlaps(other As BoundingBox3D) As Boolean
            Return Not (MaxX < other.MinX OrElse MinX > other.MaxX OrElse
                        MaxY < other.MinY OrElse MinY > other.MaxY OrElse
                        MaxZ < other.MinZ OrElse MinZ > other.MaxZ)
        End Function

        ''' <summary>全包围另一个包围盒。</summary>
        Public Function Encloses(other As BoundingBox3D) As Boolean
            Return MinX <= other.MinX AndAlso MinY <= other.MinY AndAlso MinZ <= other.MinZ AndAlso
                   MaxX >= other.MaxX AndAlso MaxY >= other.MaxY AndAlso MaxZ >= other.MaxZ
        End Function

        ''' <summary>按给定边距向外扩张。</summary>
        Public Function Expand(margin As Double) As BoundingBox3D
            Return New BoundingBox3D(MinX - margin, MinY - margin, MinZ - margin,
                                     MaxX + margin, MaxY + margin, MaxZ + margin)
        End Function

        ''' <summary>点到包围盒的最短距离（点在盒内为 0）。</summary>
        Public Function DistanceTo(x As Double, y As Double, z As Double) As Double
            Dim dx As Double = std.Max(std.Max(MinX - x, x - MaxX), 0)
            Dim dy As Double = std.Max(std.Max(MinY - y, y - MaxY), 0)
            Dim dz As Double = std.Max(std.Max(MinZ - z, z - MaxZ), 0)
            Return std.Sqrt(dx * dx + dy * dy + dz * dz)
        End Function

        ''' <summary>取第 <paramref name="index"/> 个八分子盒（0-7，按 x/y/z 的高低半区编码：bit0=x高，bit1=y高，bit2=z高）。</summary>
        Public Function Octant(index As Integer) As BoundingBox3D
            Dim cx As Double = CenterX
            Dim cy As Double = CenterY
            Dim cz As Double = CenterZ
            ' 注意：局部变量不能与字段 MinX/MaxX 等仅大小写不同（VB 不区分大小写会遮蔽字段）
            Dim loX As Double = If((index And 1) = 1, cx, MinX)
            Dim hiX As Double = If((index And 1) = 1, MaxX, cx)
            Dim loY As Double = If((index And 2) = 2, cy, MinY)
            Dim hiY As Double = If((index And 2) = 2, MaxY, cy)
            Dim loZ As Double = If((index And 4) = 4, cz, MinZ)
            Dim hiZ As Double = If((index And 4) = 4, MaxZ, cz)
            Return New BoundingBox3D(loX, loY, loZ, hiX, hiY, hiZ)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{MinX.ToString("G4")}, {MinY.ToString("G4")}, {MinZ.ToString("G4")}] .. [{MaxX.ToString("G4")}, {MaxY.ToString("G4")}, {MaxZ.ToString("G4")}]"
        End Function
    End Structure

    ''' <summary>
    ''' 八叉树中的空间点及其携带的负载对象。
    ''' </summary>
    Public Structure OctreePoint(Of T)

        Public X As Double
        Public Y As Double
        Public Z As Double
        Public Value As T

        Sub New(x As Double, y As Double, z As Double, value As T)
            Me.X = x
            Me.Y = y
            Me.Z = z
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"({X.ToString("G4")}, {Y.ToString("G4")}, {Z.ToString("G4")})"
        End Function
    End Structure

    ''' <summary>
    ''' 通用八叉树（Octree）：对三维点做空间划分索引。
    ''' 支持插入、AABB 区域查询、球体查询、最近邻搜索（分支限界）与节点遍历。
    ''' 叶节点容量与最大深度共同控制树的规模（见构造函数参数 capacity 与 maxDepth）。
    ''' </summary>
    Public Class Octree(Of T)

        ReadOnly _bounds As BoundingBox3D
        ReadOnly _capacity As Integer
        ReadOnly _maxDepth As Integer
        Dim _depth As Integer
        Dim _children As Octree(Of T)()
        Dim _points As List(Of OctreePoint(Of T))

        ''' <summary>
        ''' 构建以 <paramref name="bounds"/> 为根空间的八叉树。
        ''' </summary>
        ''' <param name="bounds">根节点的空间包围盒。</param>
        ''' <param name="capacity">叶节点最大点容量，超过即触发细分。</param>
        ''' <param name="maxDepth">最大细分深度。</param>
        Sub New(bounds As BoundingBox3D, Optional capacity As Integer = 8, Optional maxDepth As Integer = 12)
            _bounds = bounds
            _capacity = capacity
            _maxDepth = maxDepth
            _depth = 0
            _points = New List(Of OctreePoint(Of T))
        End Sub

        Public ReadOnly Property Bounds As BoundingBox3D
            Get
                Return _bounds
            End Get
        End Property

        ''' <summary>当前节点是否为叶节点（未细分）。</summary>
        Public ReadOnly Property IsLeaf As Boolean
            Get
                Return _children Is Nothing
            End Get
        End Property

        ''' <summary>节点所处深度（根为 0）。</summary>
        Public ReadOnly Property Depth As Integer
            Get
                Return _depth
            End Get
        End Property

        ''' <summary>当前节点（含子树）内的点数。</summary>
        Public ReadOnly Property Count As Integer
            Get
                If IsLeaf Then
                    Return _points.Count
                Else
                    Dim n As Integer = 0
                    For Each child As Octree(Of T) In _children
                        n += child.Count
                    Next
                    Return n
                End If
            End Get
        End Property

        ''' <summary>插入一个空间点。</summary>
        Public Sub Insert(x As Double, y As Double, z As Double, value As T)
            Call Insert(New OctreePoint(Of T)(x, y, z, value))
        End Sub

        Public Sub Insert(point As OctreePoint(Of T))
            If Not _bounds.Contains(point.X, point.Y, point.Z) Then
                Throw New ArgumentOutOfRangeException(NameOf(point),
                    $"point {point.ToString} is out of the octree bounds {_bounds.ToString}")
            End If

            Call insertNode(point)
        End Sub

        Private Sub insertNode(point As OctreePoint(Of T))
            If IsLeaf Then
                _points.Add(point)

                If _points.Count > _capacity AndAlso _depth < _maxDepth Then
                    Call subdivide()
                End If
            Else
                Call childOf(point.X, point.Y, point.Z).insertNode(point)
            End If
        End Sub

        ''' <summary>将当前叶节点细分为 8 个子节点，并下沉已有点。</summary>
        Private Sub subdivide()
            _children = New Octree(Of T)(7) {}

            For i As Integer = 0 To 7
                _children(i) = New Octree(Of T)(_bounds.Octant(i), _capacity, _maxDepth)
                _children(i)._depth = _depth + 1
            Next

            Dim points As List(Of OctreePoint(Of T)) = _points
            _points = New List(Of OctreePoint(Of T))

            For Each p As OctreePoint(Of T) In points
                Call childOf(p.X, p.Y, p.Z)._points.Add(p)
            Next
        End Sub

        Private Function childOf(x As Double, y As Double, z As Double) As Octree(Of T)
            ' 八分子盒在共享面上重叠，取第一个包含点的子盒（确定性）
            For i As Integer = 0 To 7
                If _children(i)._bounds.Contains(x, y, z) Then
                    Return _children(i)
                End If
            Next

            ' 数值边界兜底：取距离中心最近的象限
            Dim ox As Integer = If(x >= _bounds.CenterX, 1, 0)
            Dim oy As Integer = If(y >= _bounds.CenterY, 2, 0)
            Dim oz As Integer = If(z >= _bounds.CenterZ, 4, 0)
            Return _children(ox Or oy Or oz)
        End Function

        ''' <summary>查询与给定包围盒相交的全部点。</summary>
        Public Function Query(bounds As BoundingBox3D) As List(Of OctreePoint(Of T))
            Dim result As New List(Of OctreePoint(Of T))
            Call queryNode(bounds, result)
            Return result
        End Function

        Private Sub queryNode(bounds As BoundingBox3D, result As List(Of OctreePoint(Of T)))
            If Not _bounds.Overlaps(bounds) Then
                Return
            ElseIf IsLeaf Then
                For Each p As OctreePoint(Of T) In _points
                    If bounds.Contains(p.X, p.Y, p.Z) Then
                        result.Add(p)
                    End If
                Next
            Else
                For Each child As Octree(Of T) In _children
                    Call child.queryNode(bounds, result)
                Next
            End If
        End Sub

        ''' <summary>查询以 (<paramref name="cx"/>, <paramref name="cy"/>, <paramref name="cz"/>) 为球心、<paramref name="radius"/> 为半径的球体内全部点。</summary>
        Public Function QuerySphere(cx As Double, cy As Double, cz As Double, radius As Double) As List(Of OctreePoint(Of T))
            Dim bounds As New BoundingBox3D(cx - radius, cy - radius, cz - radius, cx + radius, cy + radius, cz + radius)
            Dim result As New List(Of OctreePoint(Of T))
            Dim r2 As Double = radius * radius
            Call querySphereNode(bounds, cx, cy, cz, r2, result)
            Return result
        End Function

        Private Sub querySphereNode(bounds As BoundingBox3D, cx As Double, cy As Double, cz As Double, r2 As Double, result As List(Of OctreePoint(Of T)))
            If Not _bounds.Overlaps(bounds) Then
                Return
            ElseIf IsLeaf Then
                For Each p As OctreePoint(Of T) In _points
                    Dim dx As Double = p.X - cx
                    Dim dy As Double = p.Y - cy
                    Dim dz As Double = p.Z - cz

                    If dx * dx + dy * dy + dz * dz <= r2 Then
                        result.Add(p)
                    End If
                Next
            Else
                For Each child As Octree(Of T) In _children
                    Call child.querySphereNode(bounds, cx, cy, cz, r2, result)
                Next
            End If
        End Sub

        ''' <summary>
        ''' 最近邻搜索（分支限界）。返回距离 (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>) 最近的点；
        ''' 距离通过 <paramref name="distance"/> 返回。空树返回 Nothing 且距离为 +∞。
        ''' </summary>
        Public Function Nearest(x As Double, y As Double, z As Double, ByRef distance As Double) As OctreePoint(Of T)
            distance = Double.PositiveInfinity
            Dim best As New OctreePoint(Of T)
            Call nearestNode(x, y, z, best, distance)
            If Double.IsPositiveInfinity(distance) Then
                Return Nothing
            Else
                Return best
            End If
        End Function

        Private Sub nearestNode(x As Double, y As Double, z As Double, ByRef best As OctreePoint(Of T), ByRef bestDist As Double)
            If _bounds.DistanceTo(x, y, z) >= bestDist Then
                Return
            ElseIf IsLeaf Then
                For Each p As OctreePoint(Of T) In _points
                    Dim dx As Double = p.X - x
                    Dim dy As Double = p.Y - y
                    Dim dz As Double = p.Z - z
                    Dim d As Double = std.Sqrt(dx * dx + dy * dy + dz * dz)

                    If d < bestDist Then
                        bestDist = d
                        best = p
                    End If
                Next
            Else
                For Each child As Octree(Of T) In _children
                    Call child.nearestNode(x, y, z, best, bestDist)
                Next
            End If
        End Sub

        ''' <summary>前序遍历所有节点（含内部节点与叶节点）。</summary>
        Public Sub Traverse(action As Action(Of Octree(Of T)))
            Call action(Me)

            If Not IsLeaf Then
                For Each child As Octree(Of T) In _children
                    Call child.Traverse(action)
                Next
            End If
        End Sub

        ''' <summary>枚举全部叶节点。</summary>
        Public Iterator Function Leaves() As IEnumerable(Of Octree(Of T))
            If IsLeaf Then
                Yield Me
            Else
                For Each child As Octree(Of T) In _children
                    For Each leaf As Octree(Of T) In child.Leaves()
                        Yield leaf
                    Next
                Next
            End If
        End Function
    End Class
End Namespace
