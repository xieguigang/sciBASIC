Imports Microsoft.VisualBasic.Math.DataFrame

''' <summary>
''' the feature type should be the boolean type
''' </summary>
Public Class FlagEncoder : Inherits FeatureEncoder

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Dim ints As Double() = New Double(feature.size - 1) {}
        Dim bools As Boolean() = feature.vector
        Dim name As String = feature.name

        For i As Integer = 0 To ints.Length - 1
            ints(i) = If(bools(i), 1, 0)
        Next

        feature = New FeatureVector(name, ints)

        Return New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector) From {{name, feature}},
            .rownames = IndexNames(feature)
        }
    End Function
End Class
