Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Namespace FuzzyCMeans

    Public Class AlgorithmsUtils

        Public Shared Function CalculateDistancesToClusterCenters(points As List(Of Entity), clusterCenters As List(Of Entity)) As Dictionary(Of Entity, List(Of Double))
            Dim map As New Dictionary(Of Entity, List(Of Double))()

            For Each pointCoordinates As Entity In points
                Dim distancesToCenters As New List(Of Double)()
                For Each clusterCenter As Entity In clusterCenters

                    Dim distance As Double = 0
                    For i As Integer = 0 To pointCoordinates.Length - 1
                        distance += Math.Pow(pointCoordinates(i) - clusterCenter(i), 2)
                    Next

                    distance = Math.Sqrt(distance)
                    distancesToCenters.Add(distance)
                Next
                map.Add(pointCoordinates, distancesToCenters)
            Next

            Return map
        End Function

        Public Shared Function GenerateDataPoints(dimension As Integer) As HashSet(Of Vector)
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

        Public Shared Function MakeInitialSeeds(coordinates As List(Of Entity), numberOfClusters As Integer) As List(Of Entity)
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
    End Class
End Namespace