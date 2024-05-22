#Region "Microsoft.VisualBasic::181baee34a76331677bd01b292702aaf, Data_science\DataMining\DataMining\Clustering\Canopy.vb"

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

    '   Total Lines: 254
    '    Code Lines: 166 (65.35%)
    ' Comment Lines: 43 (16.93%)
    '    - Xml Docs: 79.07%
    ' 
    '   Blank Lines: 45 (17.72%)
    '     File Size: 9.03 KB


    '     Class CanopySeeds
    ' 
    '         Properties: Canopy, k
    ' 
    '     Class CanopyBuilder
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) AverageDistance, IterateSingle, KMeansSeeds, Solve, SquareDist
    '                   TotalDistance
    ' 
    '         Sub: MeasureThreahold
    '         Class AverageDistanceTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    '     Enum Mark
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel.Design
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Parallel
Imports Canopy = Microsoft.VisualBasic.DataMining.KMeans.Bisecting.Cluster

Namespace Clustering

    Public Class CanopySeeds

        Public Property Canopy As IVector()
        Public ReadOnly Property k As Integer
            Get
                Return Canopy.TryCount
            End Get
        End Property

    End Class

    ''' <summary>
    ''' initial for k-means
    ''' </summary>
    ''' <remarks>
    ''' 与传统的聚类算法(比如K-means)不同，Canopy聚类最大的特点是不需要事先指定k值(即clustering的个数)，
    ''' 因此具有很大的实际应用价值。与其他聚类算法相比，Canopy聚类虽然精度较低，但其在速度上有很大优势，
    ''' 因此可以使用Canopy聚类先对数据进行“粗”聚类，得到k值，以及大致的K个初始质心，再使用K-means进行
    ''' 进一步“细”聚类。所以Canopy+K-means这种形式聚类算法聚类效果良好。
    ''' </remarks>
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
            If T1.IsNaNImaginary OrElse T2.IsNaNImaginary Then
                ' T1 > T2
                T2 = AverageDistance(points.ToArray()) * 3
                T1 = T2 * 2

                Call VBDebugger.EchoLine($"measure T2 threashold via the average distance: {T2}")
            Else
                Call VBDebugger.EchoLine($"T2 threshold from user specific input: {T2}")
            End If
        End Sub

        Public Shared Function TotalDistance(i As ClusterEntity, points As ClusterEntity()) As Double
            Dim sum_i As Double = 0

            For Each j As ClusterEntity In points
                If i Is j OrElse i.uid = j.uid Then
                    Continue For
                End If

                sum_i += SquareDist(i, j)
            Next

            Return sum_i
        End Function

        ''' <summary>
        ''' 得到平均距离
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        Private Shared Function AverageDistance(points As ClusterEntity()) As Double
            Dim pointSize As Integer = points.Length
            Dim task As AverageDistanceTask = New AverageDistanceTask(points).Run
            Dim parts As Double() = task.sum_i

            Return AverageDistance(pointSize, parts)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pointSize"></param>
        ''' <param name="parts">sum(parts)</param>
        ''' <returns></returns>
        Public Shared Function AverageDistance(pointSize As Double, parts As Double()) As Double
            Dim sum As Double = parts.Sum
            Dim distanceNumber As Double = pointSize * (pointSize + 1) / 2
            ' 平均距离的1/8
            Dim T2 As Double = sum / distanceNumber / 32

            Return T2
        End Function

        Private Class AverageDistanceTask : Inherits VectorTask

            Public ReadOnly points As ClusterEntity()
            Public ReadOnly sum_i As Double()

            Sub New(points As ClusterEntity())
                Call MyBase.New(points.Length)

                Me.points = points
                Me.sum_i = Allocate(Of Double)(all:=False)
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim sum_i As Double = 0

                For offset As Integer = start To ends
                    sum_i += TotalDistance(points(offset), points)
                Next

                Me.sum_i(cpu_id) = sum_i
            End Sub
        End Class

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SquareDist(i As Double(), j As Double()) As Double
            Return Exponent.f64_op_exponent_f64_scalar(
                v1:=Subtract.f64_op_subtract_f64(i, j),
                v2:=2
            ).Sum
        End Function

        Public Function KMeansSeeds() As CanopySeeds
            Return New CanopySeeds With {
                .Canopy = Solve _
                    .Select(Function(seed)
                                Return New ClusterEntity With {
                                    .cluster = seed.Cluster,
                                    .entityVector = seed.centroid,
                                    .uid = $"fake_seed_canopy_{seed.Cluster}"
                                }
                            End Function) _
                    .ToArray
            }
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

                    If poll.Length = 0 Then
                        Exit While
                    Else
                        Call VBDebugger.EchoLine($"removes {poll.Length} points, and get {canopies.Count} canopy candidates!")
                    End If

                    For Each i As Integer In poll
                        Call points.RemoveAt(i)
                    Next
                End While
            End If

            For Each c As Canopy In canopies
                c.Cluster = c.GetHashCode
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
