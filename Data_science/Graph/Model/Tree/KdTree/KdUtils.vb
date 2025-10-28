Imports System.Data
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace KdTree

    Public Module KdUtils

        ''' <summary>
        ''' take sampling of some nodes and then evaluated the near neighbor distance
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="kdtree"></param>
        ''' <param name="sample">
        ''' number of the random samples 
        ''' </param>
        ''' <param name="k">
        ''' k for knn search based on the kd-tree
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function AverageDistance(Of T As New)(kdtree As KdTree(Of T), sample As Integer,
                                                     Optional k As Integer = 32,
                                                     <Out>
                                                     Optional ByRef sampleDist As Double() = Nothing) As Double

            Dim dist As Double() = New Double(sample - 1) {}
            Dim sampledata As T() = kdtree.GetPointSample(sample).ToArray

            Call System.Threading.Tasks.Parallel.For(0, sample,
                 Sub(i)
                     Dim knn = kdtree.nearest(sampledata(i), k).ToArray
                     Dim mean As Double = Aggregate x As KdNodeHeapItem(Of T)
                                          In knn
                                          Order By x.distance
                                          Take CInt(k / 2)
                                          Let d = x.distance
                                          Into Average(d) ' evaluate average distance inside this knn dataset
                     dist(i) = mean
                 End Sub)

            sampleDist = dist

            Return dist.Average
        End Function

    End Module
End Namespace