Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Public Class Metric : Inherits KdNodeAccessor(Of ClusterEntity)

    ReadOnly dims As Dictionary(Of String, Integer)
    ReadOnly dimNames As String()

    Sub New(dims As Integer)
        Me.dims = New Dictionary(Of String, Integer)

        For i As Integer = 0 To dims - 1
            Me.dims(i.ToString) = i
        Next

        Me.dimNames = Me.dims.Keys.ToArray
    End Sub

    Public Overrides Sub setByDimensin(x As ClusterEntity, dimName As String, value As Double)
        x(dims(dimName)) = value
    End Sub

    Public Overrides Function GetDimensions() As String()
        Return dimNames
    End Function

    Public Overrides Function metric(a As ClusterEntity, b As ClusterEntity) As Double
        Return DistanceMethods.EuclideanDistance(a.entityVector, b.entityVector)
    End Function

    Public Overrides Function getByDimension(x As ClusterEntity, dimName As String) As Double
        Return x(dims(dimName))
    End Function

    Public Overrides Function nodeIs(a As ClusterEntity, b As ClusterEntity) As Boolean
        Return a Is b
    End Function

    Public Overrides Function activate() As ClusterEntity
        Return New ClusterEntity With {
            .cluster = -1,
            .entityVector = New Double(dims.Count - 1) {},
            .uid = "n/a"
        }
    End Function
End Class