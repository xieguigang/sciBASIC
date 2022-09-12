Imports Microsoft.VisualBasic.Math.DataFrame

Public Class NumericEncoder : Inherits FeatureEncoder

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Dim name As String = feature.name
        Dim values As Double() = feature.TryCast(Of Double)

        feature = New FeatureVector(name, values)

        Return New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector) From {{name, feature}},
            .rownames = IndexNames(feature)
        }
    End Function
End Class
