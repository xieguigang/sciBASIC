Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations
Imports Canopy = Microsoft.VisualBasic.DataMining.KMeans.Bisecting.Cluster

Namespace Clustering

    Public Class CanopyBuilder

        Dim T1 As Double = 8
        Dim T2 As Double = 4

        ReadOnly points As List(Of ClusterEntity)
        ReadOnly canopies As New List(Of Canopy)

        Public Sub New(data As IEnumerable(Of ClusterEntity))
            points = data.ToList
        End Sub

        Public Function Solve() As Canopy()
            If Not canopies.IsNullOrEmpty Then
                Return canopies.ToArray
            Else
                While points.Count > 0
                    Dim poll = IterateSingle() _
                        .OrderByDescending(Function(i) i) _
                        .ToArray

                    For Each i As Integer In poll
                        Call points.RemoveAt(i)
                    Next
                End While
            End If

            For Each c As Canopy In canopies
                c.centroid = c.CalculateClusterMean
            Next

            Return canopies.ToArray
        End Function

        Private Iterator Function IterateSingle() As IEnumerable(Of Integer)
            For i As Integer = 0 To points.Count - 1
                Dim current As ClusterEntity = points(i)

                ' 取一个点做为初始canopy
                If canopies.Count = 0 Then
                    Dim canopy As New Canopy() With {
                        .centroid = current.entityVector,
                        .DataPoints = New List(Of ClusterEntity) From {current}
                    }
                    Call canopies.Add(canopy)

                    Yield i
                    Continue For
                End If

                Dim isRemove = False
                Dim index = 0

                For Each canopy As Canopy In canopies
                    Dim center As Double() = canopy.centroid
                    Dim d = ManhattanDistance(current.entityVector, center)

                    ' 距离小于T1加入canopy，打上弱标记
                    If d < T1 Then
                        current.cluster = Mark.MARK_WEAK
                        canopy.addPoint(current)
                    ElseIf d > T1 Then
                        index += 1
                    End If

                    ' 距离小于T2则从列表中移除，打上强标记
                    If d <= T2 Then
                        current.cluster = Mark.MARK_STRONG
                        isRemove = True
                    End If
                Next

                ' 如果到所有canopy的距离都大于T1,生成新的canopy
                If index = canopies.Count Then
                    Dim newCanopy As New Canopy() With {.centroid = current.entityVector}
                    newCanopy.addPoint(current)
                    canopies.Add(newCanopy)
                    isRemove = True
                End If

                If isRemove Then
                    Yield i
                End If
            Next
        End Function
    End Class

    Public Enum Mark As Integer
        MARK_WEAK
        MARK_STRONG
    End Enum
End Namespace