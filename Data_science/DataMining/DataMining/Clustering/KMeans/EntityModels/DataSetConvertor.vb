Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class DataSetConvertor

        ReadOnly maps As String()

        Sub New(rawInput As EntityClusterModel())
            maps = rawInput _
            .Select(Function(a) a.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray
        End Sub

        Public Iterator Function GetPoints(rawInput As EntityClusterModel()) As IEnumerable(Of Lloyds.Point)
            For Each xi As EntityClusterModel In rawInput
                Yield New Lloyds.Point(xi(maps)) With {.uid = xi.ID}
            Next
        End Function

        Public Iterator Function GetVectors(rawInput As EntityClusterModel()) As IEnumerable(Of ClusterEntity)
            For Each xi As EntityClusterModel In rawInput
                Yield xi.ToModel(projection:=maps)
            Next
        End Function

        Public Iterator Function GetObjects(Of T As ClusterEntity)(cluster As IEnumerable(Of T)) As IEnumerable(Of EntityClusterModel)
            For Each xi As ClusterEntity In cluster
                Yield xi.ToDataModel(maps)
            Next
        End Function

        Public Iterator Function GetObjects(Of T As ClusterEntity)(cluster As IEnumerable(Of T), setClass As Integer) As IEnumerable(Of EntityClusterModel)
            Dim yi As EntityClusterModel

            For Each xi As ClusterEntity In cluster
                yi = xi.ToDataModel(maps)
                yi.Cluster = setClass

                Yield yi
            Next
        End Function

        Public Overrides Function ToString() As String
            Return maps.GetJson
        End Function
    End Class
End Namespace