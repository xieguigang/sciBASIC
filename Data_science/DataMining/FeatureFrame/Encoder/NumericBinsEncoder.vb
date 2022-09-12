Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion
Imports Microsoft.VisualBasic.Math.DataFrame

Public Class NumericBinsEncoder : Inherits FeatureEncoder

    ReadOnly nbins As Integer
    ReadOnly format As String

    Sub New(nbins As Integer, Optional format As String = "G3")
        Me.nbins = nbins
        Me.format = format
    End Sub

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Return NumericBinsEncoder(feature, nbins, format)
    End Function

    Public Shared Function NumericBinsEncoder(feature As FeatureVector, nbins As Integer, Optional format As String = "G3") As DataFrame
        Dim raw As Double() = feature.TryCast(Of Double)
        Dim encoder As New Discretizer(raw, levels:=nbins)
        Dim extends As New Dictionary(Of String, Integer())
        Dim key As String
        Dim name As String = feature.name
        Dim binNames As String() = encoder.binList _
            .Select(Function(r)
                        Return $"{name} [{r.Min.ToString(format)},{r.Max.ToString(format)}]"
                    End Function) _
            .ToArray

        For i As Integer = 1 To encoder.binSize
            Call extends.Add(i, New Integer(raw.Length - 1) {})
        Next

        For i As Integer = 0 To raw.Length - 1
            key = encoder.GetLevel(raw(i)) + 1
            extends(key)(i) = 1
        Next

        Return New DataFrame With {
            .features = extends _
                .ToDictionary(Function(v) binNames(Integer.Parse(v.Key) - 1),
                              Function(v)
                                  Return New FeatureVector(binNames(Integer.Parse(v.Key) - 1), v.Value)
                              End Function),
            .rownames = IndexNames(feature)
        }
    End Function

End Class
