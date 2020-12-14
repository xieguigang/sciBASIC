Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class AlignmentComparison : Inherits ComparisonProvider

    ReadOnly dataIndex As Dictionary(Of String, Double())

    Sub New(dataset As NamedValue(Of DynamicPropertyBase(Of Double))(), equals As Double, gt As Double)
        Call MyBase.New(equals, gt)

        Dim names As String() = dataset _
            .Select(Function(a) a.Value.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray

        dataIndex = dataset _
            .ToDictionary(Function(d) d.Name,
                          Function(d)
                              Return names _
                                  .Select(Function(col) d.Value(col)) _
                                  .ToArray
                          End Function)
    End Sub

    Protected Overrides Function GetSimilarity(x As String, y As String) As Double
        Dim xvec As New Vector(dataIndex(x))
        Dim yvec As New Vector(dataIndex(y))

        Return SSM(xvec, yvec)
    End Function
End Class
