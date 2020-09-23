Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions




Public Class Program
    Public Shared Sub Main(args As String())
        Dim data As ClusterEntity() = Enumerable.Range(0, 100) _
            .[Select](Function(x)
                          Return New ClusterEntity With {
                              .uid = x.ToString,
                              .entityVector = New Double() {randf.randf(0, 99999), randf.randf(-1000, 999), randf.randf(1, 10), randf.randf(-1000000, 10), randf.randf(-1000000, 10000)}
                          }
                      End Function) _
            .ToArray()
        Dim result As Classify() = CMeans.CMeans(10, data)

        For Each item In result
            Console.WriteLine($"===== {item.Id} (Count:{item.members.Count}) =====")

            For Each item2 In item.members.OrderBy(Function(x) x.entityVector.Average())
                Console.WriteLine(item2.ToString)
            Next
        Next

        Console.ReadKey()
    End Sub
End Class