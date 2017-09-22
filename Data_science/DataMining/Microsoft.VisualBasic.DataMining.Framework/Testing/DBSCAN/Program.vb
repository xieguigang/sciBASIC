Imports System.Collections.Generic
Imports Microsoft.VisualBasic.DataMining.DBSCAN

Class Program
    Friend Shared Sub Main(args As String())
        Dim featureData As MyCustomDatasetItem() = {}

        Dim testPoints As New List(Of MyCustomDatasetItem)()
        For i As Integer = 0 To 999
            'points around (1,1) with most 1 distance
            testPoints.Add(New MyCustomDatasetItem(1, 1 + (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(1, 1 - (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(1 - (CSng(i) / 1000), 1))
            testPoints.Add(New MyCustomDatasetItem(1 + (CSng(i) / 1000), 1))

            'points around (5,5) with most 1 distance
            testPoints.Add(New MyCustomDatasetItem(5, 5 + (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(5, 5 - (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(5 - (CSng(i) / 1000), 5))
            testPoints.Add(New MyCustomDatasetItem(5 + (CSng(i) / 1000), 5))
        Next
        featureData = testPoints.ToArray()
        Dim clusters As HashSet(Of MyCustomDatasetItem()) = Nothing

        Dim dbs = New DbscanAlgorithm(Of MyCustomDatasetItem)(Function(x, y) Math.Sqrt(((x.X - y.X) * (x.X - y.X)) + ((x.Y - y.Y) * (x.Y - y.Y))))
        dbs.ComputeClusterDbscan(allPoints:=featureData, epsilon:=0.01, minPts:=10, clusters:=clusters)



        Console.ReadKey()
    End Sub
End Class
