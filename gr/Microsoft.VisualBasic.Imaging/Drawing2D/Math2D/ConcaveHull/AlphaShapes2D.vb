#Region "Microsoft.VisualBasic::7cef72db460c8ac3f449cead2fd91c39, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\BallConcave.vb"

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

'   Total Lines: 305
'    Code Lines: 254 (83.28%)
' Comment Lines: 11 (3.61%)
'    - Xml Docs: 36.36%
' 
'   Blank Lines: 40 (13.11%)
'     File Size: 11.91 KB


'     Class BallConcave
' 
'         Properties: RecomandedRadius
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: CheckValid, CompareAngel, GetCircleCenter, GetConcave_Ball, GetConcave_Edge
'                   GetCross, GetInRNeighbourList, GetMinEdgeLength, GetNextPoint_BallPivoting, GetNextPoint_EdgePivoting
'                   GetSortedNeighbours, HasPointsInCircle, IsInCircle
' 
'         Sub: InitDistanceMap, InitNearestList, SortAdjListByAngel
'         Structure Point2dInfo
' 
'             Constructor: (+1 Overloads) Sub New
'             Function: CompareTo, ToString
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Drawing2D.Math2D.ConcaveHull

    Public Class AlphaShapes2D
        Private points As PointF()
        Private distanceMap As Double(,)

        Public Sub New(pointList As IEnumerable(Of PointF))
            Me.points = pointList.ToArray
            PrecomputeDistances()
        End Sub

        ''' <summary>
        ''' 预计算所有点对之间的距离平方，优化性能
        ''' </summary>
        Private Sub PrecomputeDistances()
            Dim n As Integer = points.Count
            distanceMap = New Double(n - 1, n - 1) {}

            For i As Integer = 0 To n - 1
                For j As Integer = i + 1 To n - 1
                    Dim dx As Double = points(i).X - points(j).X
                    Dim dy As Double = points(i).Y - points(j).Y
                    distanceMap(i, j) = dx * dx + dy * dy
                    distanceMap(j, i) = distanceMap(i, j)
                Next
            Next
        End Sub

        ''' <summary>
        ''' 基于Delaunay三角网的高效Alpha Shapes实现
        ''' </summary>
        ''' <param name="alpha">Alpha参数，控制轮廓的紧密度</param>
        ''' <returns>轮廓点列表，按顺序排列</returns>
        Public Function ComputeAlphaShape(alpha As Double) As List(Of PointF)
            If points.Count < 3 Then Return points.ToList()

            ' 步骤1：构建Delaunay三角网
            Dim triangles As List(Of Triangle) = BuildDelaunayTriangulation()
            If triangles Is Nothing OrElse triangles.Count = 0 Then
                Return New List(Of PointF)()
            End If

            ' 步骤2：根据Alpha值筛选边界边
            Dim alphaEdges As HashSet(Of Edge) = GetAlphaEdges(triangles, alpha)

            ' 步骤3：从边界边构建轮廓多边形
            Return BuildContourFromEdges(alphaEdges)
        End Function

        ''' <summary>
        ''' 使用Bowyer-Watson算法构建Delaunay三角网
        ''' </summary>
        Private Function BuildDelaunayTriangulation() As List(Of Triangle)
            If points.Count < 3 Then Return New List(Of Triangle)()

            Dim triangles As New List(Of Triangle)()

            ' 创建超级三角形，包含所有点
            Dim superTriangle As Triangle = CreateSuperTriangle()
            triangles.Add(superTriangle)

            ' 逐点插入
            For i As Integer = 0 To points.Count - 1
                triangles = InsertPoint(triangles, i)
            Next

            ' 移除与超级三角形相关的三角形
            triangles.RemoveAll(Function(t)
                                    Return t.Vertices.Any(Function(v) v >= points.Count)
                                End Function)

            Return triangles
        End Function

        ''' <summary>
        ''' 创建包含所有点的超级三角形
        ''' </summary>
        Private Function CreateSuperTriangle() As Triangle
            Dim minX As Double = points.Min(Function(p) p.X)
            Dim maxX As Double = points.Max(Function(p) p.X)
            Dim minY As Double = points.Min(Function(p) p.Y)
            Dim maxY As Double = points.Max(Function(p) p.Y)

            Dim dx As Double = maxX - minX
            Dim dy As Double = maxY - minY
            Dim deltaMax As Double = std.Max(dx, dy) * 10

            ' 超级三角形的顶点（使用虚拟索引，大于实际点数）
            Dim p1 As New PointF(minX - deltaMax, minY - deltaMax)
            Dim p2 As New PointF(maxX + deltaMax, minY - deltaMax)
            Dim p3 As New PointF(minX + dx / 2, maxY + deltaMax)

            ' 将超级三角形的点添加到点集末尾
            points.Add(p1)
            points.Add(p2)
            points.Add(p3)

            Return New Triangle(points.Count - 3, points.Count - 2, points.Count - 1)
        End Function

        ''' <summary>
        ''' 向三角网中插入新点
        ''' </summary>
        Private Function InsertPoint(triangles As List(Of Triangle), pointIndex As Integer) As List(Of Triangle)
            Dim badTriangles As New List(Of Triangle)()
            Dim polygon As New HashSet(Of Edge)(New EdgeEqualityComparer())

            ' 查找包含新点的坏三角形（外接圆包含新点）
            For Each tri In triangles
                If IsPointInCircumcircle(tri, pointIndex) Then
                    badTriangles.Add(tri)
                End If
            Next

            ' 构建多边形边界
            For Each tri In badTriangles
                For Each edge In tri.GetEdges()
                    Dim isShared As Boolean = False
                    For Each otherTri In badTriangles
                        If otherTri IsNot tri AndAlso otherTri.GetEdges().Contains(edge, New EdgeEqualityComparer()) Then
                            isShared = True
                            Exit For
                        End If
                    Next
                    If Not isShared Then
                        polygon.Add(edge)
                    End If
                Next
            Next

            ' 移除坏三角形
            triangles.RemoveAll(Function(t) badTriangles.Contains(t))

            ' 创建新三角形
            For Each edge In polygon
                triangles.Add(New Triangle(edge.Point1, edge.Point2, pointIndex))
            Next

            Return triangles
        End Function

        ''' <summary>
        ''' 判断点是否在三角形的外接圆内
        ''' </summary>
        Private Function IsPointInCircumcircle(tri As Triangle, pointIndex As Integer) As Boolean
            Dim A As PointF = points(tri.Vertices(0))
            Dim B As PointF = points(tri.Vertices(1))
            Dim C As PointF = points(tri.Vertices(2))
            Dim P As PointF = points(pointIndex)

            Dim d As Double = (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) * 2

            If std.Abs(d) < 0.0000000001 Then Return False

            Dim centerX As Double = ((A.X * A.X + A.Y * A.Y) * (B.Y - C.Y) +
                               (B.X * B.X + B.Y * B.Y) * (C.Y - A.Y) +
                               (C.X * C.X + C.Y * C.Y) * (A.Y - B.Y)) / d

            Dim centerY As Double = ((A.X * A.X + A.Y * A.Y) * (C.X - B.X) +
                               (B.X * B.X + B.Y * B.Y) * (A.X - C.X) +
                               (C.X * C.X + C.Y * C.Y) * (B.X - A.X)) / d

            Dim radius As Double = std.Sqrt((centerX - A.X) ^ 2 + (centerY - A.Y) ^ 2)
            Dim dist As Double = std.Sqrt((centerX - P.X) ^ 2 + (centerY - P.Y) ^ 2)

            Return dist <= radius
        End Function

        ''' <summary>
        ''' 根据Alpha值从三角网中提取边界边
        ''' </summary>
        Private Function GetAlphaEdges(triangles As List(Of Triangle), alpha As Double) As HashSet(Of Edge)
            Dim alphaEdges As New HashSet(Of Edge)(New EdgeEqualityComparer())
            Dim edgeCount As New Dictionary(Of Edge, Integer)(New EdgeEqualityComparer())

            ' 统计每条边被多少个三角形共享
            For Each triangle In triangles
                For Each edge In triangle.GetEdges()
                    If edgeCount.ContainsKey(edge) Then
                        edgeCount(edge) += 1
                    Else
                        edgeCount(edge) = 1
                    End If
                Next
            Next

            ' 筛选边界边（只被一个三角形共享）或外接圆半径大于Alpha的边
            For Each triangle In triangles
                For Each edge In triangle.GetEdges()
                    Dim isBoundary As Boolean = edgeCount(edge) = 1
                    Dim circumRadius As Double = triangle.CalculateCircumradius(points)

                    ' Alpha Shapes核心条件：边长小于2*alpha或为边界边
                    Dim edgeLength As Double = std.Sqrt(distanceMap(edge.Point1, edge.Point2))
                    If isBoundary OrElse edgeLength < 2 * alpha Then
                        alphaEdges.Add(edge)
                    End If
                Next
            Next

            Return alphaEdges
        End Function

        ''' <summary>
        ''' 从边界边构建有序的轮廓多边形
        ''' </summary>
        Private Function BuildContourFromEdges(edges As HashSet(Of Edge)) As List(Of PointF)
            If edges.Count = 0 Then Return New List(Of PointF)()

            Dim edgeDict As New Dictionary(Of Integer, List(Of Integer))()
            Dim pointIndexMap As New Dictionary(Of Integer, Integer)()

            ' 构建邻接表
            For Each edge In edges
                If Not edgeDict.ContainsKey(edge.Point1) Then
                    edgeDict(edge.Point1) = New List(Of Integer)()
                End If
                If Not edgeDict.ContainsKey(edge.Point2) Then
                    edgeDict(edge.Point2) = New List(Of Integer)()
                End If

                edgeDict(edge.Point1).Add(edge.Point2)
                edgeDict(edge.Point2).Add(edge.Point1)
            Next

            ' 找到轮廓起点（度数最小的点）
            Dim startPoint As Integer = edgeDict.OrderBy(Function(kv) kv.Value.Count).First().Key

            ' 追踪轮廓
            Dim contour As New List(Of PointF)()
            Dim visited As New HashSet(Of Integer)()
            Dim current As Integer = startPoint

            Do While True
                contour.Add(points(current))
                visited.Add(current)

                Dim neighbors = edgeDict(current).Where(Function(n) Not visited.Contains(n)).ToList()
                If neighbors.Count = 0 Then Exit Do

                ' 选择角度最小的邻居（保证顺时针顺序）
                Dim nextPoint As Integer = GetNextContourPoint(neighbors, current, If(contour.Count < 2, Nothing, contour(contour.Count - 2)), contour)
                current = nextPoint

                If current = startPoint OrElse visited.Contains(current) Then Exit Do
            Loop

            ' 闭合轮廓
            If contour.Count > 2 AndAlso contour.First() <> contour.Last() Then
                contour.Add(contour.First())
            End If

            Return contour
        End Function

        ''' <summary>
        ''' 选择下一个轮廓点（基于向量角度）
        ''' </summary>
        Private Function GetNextContourPoint(neighbors As List(Of Integer), current As Integer,
                                       previous As PointF?, contour As List(Of PointF)) As Integer
            If neighbors.Count = 1 Then Return neighbors(0)

            Dim currentPoint As PointF = points(current)
            Dim prevPoint As PointF = If(previous.HasValue, previous.Value,
                                   New PointF(currentPoint.X - 1, currentPoint.Y))

            ' 计算参考向量（从前一点到当前点）
            Dim refVector As New PointF(currentPoint.X - prevPoint.X, currentPoint.Y - prevPoint.Y)
            Dim refAngle As Double = std.Atan2(refVector.Y, refVector.X)

            Dim bestNeighbor As Integer = neighbors(0)
            Dim minAngleDiff As Double = Double.MaxValue

            For Each neighbor In neighbors
                Dim neighborPoint As PointF = points(neighbor)
                Dim toNeighbor As New PointF(neighborPoint.X - currentPoint.X, neighborPoint.Y - currentPoint.Y)
                Dim neighborAngle As Double = std.Atan2(toNeighbor.Y, toNeighbor.X)

                ' 计算角度差（确保顺时针方向）
                Dim angleDiff As Double = refAngle - neighborAngle
                If angleDiff < 0 Then angleDiff += 2 * std.PI

                If angleDiff < minAngleDiff Then
                    minAngleDiff = angleDiff
                    bestNeighbor = neighbor
                End If
            Next

            Return bestNeighbor
        End Function

        ''' <summary>
        ''' 自动计算推荐的Alpha值
        ''' </summary>
        Public Function ComputeOptimalAlpha() As Double
            If points.Count < 3 Then Return 0.0

            Dim triangles = BuildDelaunayTriangulation()
            If triangles.Count = 0 Then Return 0.0

            ' 计算所有三角形外接圆半径的中位数作为推荐Alpha值
            Dim radii = triangles.Select(Function(t) t.CalculateCircumradius(points)).OrderBy(Function(r) r).ToList()
            Dim medianRadius As Double = radii(radii.Count \ 2)

            Return medianRadius * 1.5
        End Function

        ' 辅助类：三角形
        Public Class Triangle

            <XmlAttribute> Public Property Vertices As Integer()

            Public Sub New(v1 As Integer, v2 As Integer, v3 As Integer)
                Vertices = {v1, v2, v3}
                Array.Sort(Vertices)
            End Sub

            Public Overrides Function ToString() As String
                Return Vertices.GetJson
            End Function

            Public Function GetEdges() As List(Of Edge)
                Return New List(Of Edge) From {
                    New Edge(Vertices(0), Vertices(1)),
                    New Edge(Vertices(1), Vertices(2)),
                    New Edge(Vertices(2), Vertices(0))
                }
            End Function

            ''' <summary>
            ''' 计算三角形外接圆半径
            ''' </summary>
            Public Function CalculateCircumradius(points As PointF()) As Double
                Dim pA As PointF = points(Vertices(0))
                Dim pB As PointF = points(Vertices(1))
                Dim pC As PointF = points(Vertices(2))

                Dim a As Double = std.Sqrt((pB.X - pC.X) ^ 2 + (pB.Y - pC.Y) ^ 2)
                Dim b As Double = std.Sqrt((pA.X - pC.X) ^ 2 + (pA.Y - pC.Y) ^ 2)
                Dim c As Double = std.Sqrt((pA.X - pB.X) ^ 2 + (pA.Y - pB.Y) ^ 2)

                Dim area As Double = std.Abs((pB.X - pA.X) * (pC.Y - pA.Y) - (pC.X - pA.X) * (pB.Y - pA.Y)) / 2

                If area < 0.0000000001 Then Return Double.MaxValue

                Return (a * b * c) / (4 * area)
            End Function
        End Class

        ' 辅助类：边
        Public Class Edge

            <XmlAttribute> Public Property Point1 As Integer
            <XmlAttribute> Public Property Point2 As Integer

            Public Sub New(p1 As Integer, p2 As Integer)
                Point1 = std.Min(p1, p2)
                Point2 = std.Max(p1, p2)
            End Sub
        End Class

        ' 边比较器
        Public Class EdgeEqualityComparer : Implements IEqualityComparer(Of Edge)

            Public Overloads Function Equals(x As Edge, y As Edge) As Boolean Implements IEqualityComparer(Of Edge).Equals
                Return x.Point1 = y.Point1 AndAlso x.Point2 = y.Point2
            End Function

            Public Overloads Function GetHashCode(obj As Edge) As Integer Implements IEqualityComparer(Of Edge).GetHashCode
                Return obj.Point1.GetHashCode() Xor obj.Point2.GetHashCode()
            End Function
        End Class
    End Class
End Namespace
