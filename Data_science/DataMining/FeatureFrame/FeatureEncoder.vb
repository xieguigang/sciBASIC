Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame

''' <summary>
''' the base abstract data type of the vector encoder
''' </summary>
Public MustInherit Class FeatureEncoder

    Public MustOverride Function Encode(feature As FeatureVector) As DataFrame

    Protected Shared Function IndexNames(feature As FeatureVector) As String()
        Return feature.size _
            .Sequence _
            .Select(Function(i) (i + 1).ToString) _
            .ToArray
    End Function
End Class
