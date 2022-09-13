Imports Microsoft.VisualBasic.Math.DataFrame

''' <summary>
''' the feature type should be the string type
''' </summary>
Public Class EnumEncoder : Inherits FeatureEncoder

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Dim strs As String() = feature.TryCast(Of String)
        Dim str As String
        Dim extends As New Dictionary(Of String, Integer())
        Dim factors As String() = strs.Distinct.ToArray
        Dim name As String = feature.name

        For Each key As String In factors
            Call extends.Add(key, New Integer(strs.Length - 1) {})
        Next

        For i As Integer = 0 To strs.Length - 1
            str = strs(i)
            extends(str)(i) = 1
        Next

        Return New DataFrame With {
            .features = extends _
                .ToDictionary(Function(v) $"{name}.{v.Key}",
                              Function(v)
                                  Return New FeatureVector(name, v.Value)
                              End Function),
            .rownames = IndexNames(feature)
        }
    End Function
End Class
