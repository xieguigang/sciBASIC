Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq

Namespace DBSCAN

    <HideModuleName>
    Public Module Extensions

        <Extension>
        Public Iterator Function RunDbscanCluster(data As IEnumerable(Of EntityClusterModel), eps As Double, minPts As Integer) As IEnumerable(Of EntityClusterModel)
            With data.ToArray
                Dim metrix As New Metric(.Select(Function(v) v.Properties.Keys).IteratesALL)
                Dim dbscan As New DbscanAlgorithm(Of EntityClusterModel)(AddressOf metrix.DistanceTo)
                Dim result As NamedCollection(Of EntityClusterModel)() = .DoCall(Function(vec) dbscan.ComputeClusterDBSCAN(vec, eps, minPts))

                For Each cluster In result
                    For Each c As EntityClusterModel In cluster
                        c.Cluster = cluster.name
                        Yield c
                    Next
                Next
            End With
        End Function
    End Module
End Namespace