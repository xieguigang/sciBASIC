Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace FuzzyCMeans

    Public Module AlgorithmsUtils

        Public Function GetMaxElement(values As List(Of List(Of Double))) As Double
            Dim max As Double = Double.MinValue
            For i As Integer = 0 To values.Count - 1
                For j As Integer = 0 To values(0).Count - 1
                    If values(i)(j) > max Then
                        max = values(i)(j)
                    End If

                Next
            Next

            Return max
        End Function

        ''' <summary>
        ''' 差异矩阵
        ''' </summary>
        ''' <param name="matrix1"></param>
        ''' <param name="matrix2"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DifferenceMatrix(matrix1 As IEnumerable(Of List(Of Double)), matrix2 As List(Of List(Of Double))) As List(Of List(Of Double))
            Dim d As New List(Of List(Of Double))()
            Dim l% = matrix1.First.Count
            Dim line As List(Of Double)

            For Each row As SeqValue(Of List(Of Double)) In matrix1.SeqIterator
                Dim rowDifferences As New List(Of Double)()

                line = (+row)

                For j As Integer = 0 To l - 1
                    Dim result As Double = Math.Abs(line(j) - matrix2(row.i)(j))
                    rowDifferences.Add(result)
                Next

                d.Add(rowDifferences)
            Next

            Return d
        End Function

        Public Function GetElementIndex(list As List(Of List(Of Double)), element As List(Of Double)) As Integer
            For i As Integer = 0 To list.Count - 1
                If VectorEqualityComparer.VectorEqualsToAnother(list(i), element) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        <Extension>
        Public Function DistanceToClusterCenters(ls As List(Of Entity), clusterCenters As List(Of Entity)) As Dictionary(Of Entity, List(Of Double))
            Dim map As New Dictionary(Of Entity, List(Of Double))()

            For Each x As Entity In ls
                Dim distancesToCenters As New List(Of Double)()

                For Each c As Entity In clusterCenters
                    Dim distance As Double

                    For i As Integer = 0 To x.Length - 1
                        distance += Math.Pow(x(i) - c(i), 2)
                    Next

                    distance = Math.Sqrt(distance)
                    distancesToCenters.Add(distance)
                Next

                Call map.Add(x, distancesToCenters)
            Next

            Return map
        End Function

        Public Function GenerateDataPoints(dimension As Integer) As HashSet(Of Vector)
            Dim coordinates As New HashSet(Of Vector)(New VectorEqualityComparer())
            Dim random As New Random()

            While coordinates.Count < 50
                Dim list As New List(Of Double)()
                For j As Integer = 0 To dimension - 1
                    list.Add(random.[Next](0, 100))
                Next

                ' 不要将这个b的定义放进条件编译模块，因为最后一个list还没有被添加
                Dim b As Boolean = coordinates.Add(list)
#Region "DEBUG"
#If DEBUG Then
                If Not b Then _
                    Call "Duplicate detected while generating data points".Warning
#End If
#End Region
            End While

            Return coordinates
        End Function

        Public Function MakeInitialSeeds(coordinates As List(Of Entity), numberOfClusters As Integer) As List(Of Entity)
            Dim random As New Random()
            Dim coordinatesCopy As List(Of Entity) = coordinates.ToList()
            Dim initialClusterCenters As New List(Of Entity)()
            For i As Integer = 0 To numberOfClusters - 1
                Dim clusterCenterPointNumber As Integer = random.[Next](0, coordinatesCopy.Count)
                initialClusterCenters.Add(coordinatesCopy(clusterCenterPointNumber))
                coordinatesCopy.RemoveAt(clusterCenterPointNumber)
            Next

            Return initialClusterCenters
        End Function
    End Module
End Namespace