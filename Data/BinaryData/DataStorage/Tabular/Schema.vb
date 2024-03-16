Imports Microsoft.VisualBasic.Math.DataFrame

Public Class Schema

    Public Property rownames As String()
    Public Property cols As Dictionary(Of String, Type)
    Public Property dims As Integer()

    Sub New(df As DataFrame)
        rownames = df.rownames
        dims = {df.dims.Height, df.dims.Width}
        cols = df.features _
            .ToDictionary(Function(f) f.Key,
                          Function(f)
                              Return f.Value.type
                          End Function)
    End Sub

End Class
