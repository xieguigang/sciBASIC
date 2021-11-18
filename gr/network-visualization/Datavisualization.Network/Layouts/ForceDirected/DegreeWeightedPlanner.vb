
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Layouts.ForceDirected

    ''' <summary>
    ''' 依据节点度值的大小对每一个节点动态生产排斥力权重：
    ''' 
    ''' 度值越大则该节点对周围节点的排斥力越大
    ''' </summary>
    Public Class DegreeWeightedPlanner : Inherits Planner

        Public Sub New(g As NetworkGraph,
                       Optional ejectFactor As Integer = 6,
                       Optional condenseFactor As Integer = 3,
                       Optional maxtx As Integer = 4,
                       Optional maxty As Integer = 3,
                       Optional dist_threshold As String = "30,250",
                       Optional size As String = "1000,1000",
                       Optional avoidRegions() As Drawing.RectangleF = Nothing)

            Call MyBase.New(g, ejectFactor, condenseFactor, maxtx, maxty, dist_threshold, size, avoidRegions)
            Call BuildAttractionPool()
        End Sub

        Protected Overrides Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim id As String
            Dim ejectFactor = Me.ejectFactor
            Dim dx, dy As Double
            Dim d As Double

            For Each v As Node In g.vertex
                id = v.label

                mDxMap(id) = 0.0
                mDyMap(id) = 0.0

                For Each u As Node In g.vertex.Where(Function(ui) Not ui Is v)
                    distX = v.data.initialPostion.x - u.data.initialPostion.x
                    distY = v.data.initialPostion.y - u.data.initialPostion.y
                    dist = stdNum.Sqrt(distX ^ 2 + distY ^ 2)

                    ' 排斥力只在一定的距离内生效
                    If dist > 0 AndAlso dist < dist_thresh.Max Then
                        d = stdNum.Max(u.degree.In + u.degree.Out, v.degree.In + v.degree.Out)
                        dx = (distX / dist) * (k * k / dist) * ejectFactor * d
                        dy = (distY / dist) * (k * k / dist) * ejectFactor * d

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next
            Next
        End Sub

        ReadOnly poolIndex As New Index(Of String)

        Private Sub BuildAttractionPool()
            Dim list As New List(Of String)

            For Each v As Node In g.vertex
                For Each u As Node In g.vertex.Where(Function(ui) Not ui Is v)
                    Dim nu = u.adjacencies.Neighborhood.Where(Function(vi) vi IsNot v).ToArray
                    Dim nv = v.adjacencies.Neighborhood.Where(Function(ui) ui IsNot u).ToArray

                    If Not nu.Any(Function(ui) nv.Any(Function(vi) ui Is vi)) Then
                        Continue For
                    Else
                        list.Add($"{v.label}-{u.label}")
                        list.Add($"{u.label}-{v.label}")
                    End If
                Next
            Next

            poolIndex.Clear()
            poolIndex.Add(list.Distinct.ToArray)
        End Sub

        ''' <summary>
        ''' 具有边连接或者间接边连接的节点都可以相互吸引
        ''' </summary>
        Protected Overrides Sub runAttraction()
            ' 具有直接边连接
            Call MyBase.runAttraction()

            ' 计算出间接边连接的节点间的吸引力
            Dim distX, distY, dist As Double
            Dim id As String
            Dim ejectFactor = Me.ejectFactor
            Dim dx, dy As Double

            For Each v As Node In g.vertex
                id = v.label

                mDxMap(id) = 0.0
                mDyMap(id) = 0.0

                For Each u As Node In g.vertex.Where(Function(ui) Not ui Is v)
                    Dim key As String = $"{u.label}-{v.label}"

                    If Not key Like poolIndex Then
                        Continue For
                    End If

                    distX = u.data.initialPostion.x - v.data.initialPostion.x
                    distY = u.data.initialPostion.y - v.data.initialPostion.y
                    dist = stdNum.Sqrt(distX * distX + distY * distY)
                    dx = distX * dist / k * condenseFactor
                    dy = distY * dist / k * condenseFactor

                    mDxMap(u.label) = mDxMap(u.label) - dx
                    mDyMap(u.label) = mDyMap(u.label) - dy
                    mDxMap(v.label) = mDxMap(v.label) + dx
                    mDyMap(v.label) = mDyMap(v.label) + dy
                Next
            Next
        End Sub
    End Class
End Namespace