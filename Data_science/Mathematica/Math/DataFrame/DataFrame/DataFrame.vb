Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class DataFrame

    Public Property features As New Dictionary(Of String, FeatureVector)
    Public Property rownames As String()

    Public ReadOnly Property dims As Size
        Get
            Return New Size(width:=features.Count, height:=rownames.Length)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim size As Size = dims
        Dim featureSet As String = features _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Value.type.Name.ToLower
                          End Function) _
            .GetJson

        Return $"[{size.Width}x{size.Height}] {featureSet}"
    End Function
End Class
