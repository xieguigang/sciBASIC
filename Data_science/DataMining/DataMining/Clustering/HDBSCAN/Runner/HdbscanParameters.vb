Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Hdbscanstar

Namespace HDBSCAN.Runner
    Public Class HdbscanParameters(Of T)
        Public Property CacheDistance As Boolean = True
        Public Property MaxDegreeOfParallelism As Integer = 1

        Public Property Distances As Double()()
        Public Property DataSet As T()
        Public Property DistanceFunction As IDistanceCalculator(Of T)

        Public Property MinPoints As Integer
        Public Property MinClusterSize As Integer
        Public Property Constraints As List(Of HdbscanConstraint)
    End Class
End Namespace
