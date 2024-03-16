Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Class Schema

    Public Property rownames As String()
    Public Property cols As Dictionary(Of String, VectorSchema)
    Public Property dims As Integer()

    Sub New(df As DataFrame)
        rownames = df.rownames
        dims = {df.dims.Height, df.dims.Width}
        cols = df.features _
            .ToDictionary(Function(f) f.Key,
                          Function(f)
                              Return New VectorSchema(f.Value)
                          End Function)
    End Sub

End Class

Public Class VectorSchema

    Public Property type As TypeCode
    Public Property isScalar As Boolean

    Sub New(feature As FeatureVector)
        type = feature.type.PrimitiveTypeCode
        isScalar = feature.isScalar
    End Sub

End Class