Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion
Imports Microsoft.VisualBasic.Math.DataFrame

Public Class NumericBinsEncoder : Inherits FeatureEncoder

    ReadOnly nbins As Integer

    Sub New(nbins As Integer)
        Me.nbins = nbins
    End Sub

    Public Function Encode()

    End Function

    Public Shared Function NumericBinsEncoder(feature As FeatureVector, name As String, nbins As Integer) As DataFrame
        Dim raw As Double() = feature.TryCast(Of Double)
        Dim encoder As New Discretizer(raw, levels:=nbins)
        Dim extends As New Dictionary(Of String, Integer())
        Dim key As String

        For i As Integer = 1 To encoder.binSize
            Call extends.Add(i, New Integer(raw.Length - 1) {})
        Next

        For i As Integer = 0 To raw.Length - 1
            key = encoder.GetLevel(raw(i)) + 1
            extends(key)(i) = 1
        Next

        Return New DataFrame With {
            .features = extends _
                .ToDictionary(Function(v) $"{name}.{v.Key}",
                              Function(v)
                                  Return New FeatureVector(v.Value)
                              End Function),
            .rownames = IndexNames(feature)
        }
    End Function

End Class
