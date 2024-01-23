Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.SIMD
Imports Canopy = Microsoft.VisualBasic.DataMining.KMeans.Bisecting.Cluster

Namespace Clustering

    ''' <summary>
    ''' initial for k-means
    ''' </summary>
    Public Class CanopyBuilder

        Dim T1 As Double = Double.NaN
        Dim T2 As Double = Double.NaN

        ReadOnly points As List(Of ClusterEntity)
        ReadOnly canopies As New List(Of Canopy)

        Public Sub New(data As IEnumerable(Of ClusterEntity))
            points = data.ToList
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="T1"></param>
        ''' <param name="T2"></param>
        ''' <remarks>
        ''' value of <paramref name="T1"/> should greater than <paramref name="T2"/>, example as:
        ''' 
        ''' T1 = 8 and T2 = 4
        ''' </remarks>
        Sub New(data As IEnumerable(Of ClusterEntity), T1 As Double, T2 As Double)
            Call Me.New(data)

            Me.T1 = T1
            Me.T2 = T2
        End Sub

        Private Sub MeasureThreahold()
            T2 = AverageDistance(points)
            T1 = T2 * 2
        End Sub

        ''' <summary>
        ''' 得到平均距离
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        Private Shared Function AverageDistance(points As List(Of ClusterEntity)) As Double
            Dim pointSize As Integer = points.Count
            Dim parts As Double() = points _
                .AsParallel _
                .Select(Function(i)
                            Dim sum_i As Double = 0

                            For Each j As ClusterEntity In points
                                If i Is j Then
                                    Continue For
                                End If

                                sum_i += SquareDist(i, j)
                            Next

                            Return sum_i
                        End Function) _
                .ToArray
            Dim sum As Double = parts.Sum
            Dim distanceNumber As Integer = pointSize * (pointSize + 1) / 2
            ' 平均距离的1/8
            Dim T2 As Double = sum / distanceNumber / 32

            Return T2
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function SquareDist(i As Double(), j As Double()) As Double
            Return Exponent.f64_op_exponent_f64_scalar(
                v1:=Subtract.f64_op_subtract_f64(i, j),
                v2:=2
            ).Sum
        End Function

        Public Function Solve() As Canopy()
            If Not canopies.IsNullOrEmpty Then
                Return canopies.ToArray
            Else
                Call MeasureThreahold()

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
                    Dim d = SquareDist(current, center)

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
        ''' <summary>
        ''' &lt; T1
        ''' </summary>
        MARK_WEAK = 1
        ''' <summary>
        ''' &lt; T2
        ''' </summary>
        MARK_STRONG = 2
    End Enum
End Namespace