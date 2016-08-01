Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute

Namespace Net.Http

    Public Module RequestBuilder

        <Extension>
        Public Function BuildParameters(Of T As Class)(x As T) As String
            Dim type As Type = GetType(T)
            Dim args As New List(Of NamedValue(Of String))
            Dim ps = LoadMapping(type, mapsAll:=True)

            For Each p In ps.Values.Where(Function(o) IsPrimitive(o.Property.PropertyType))
                Dim value As Object = p.GetValue(x)
                If Not value Is Nothing Then
                    args += New NamedValue(Of String) With {
                        .Name = p.Field.Name,
                        .x = value.ToString
                    }
                End If
            Next

            Dim param As String = args _
                .ToArray(Function(o) $"{o.Name}={o.x.UrlEncode}") _
                .JoinBy("&")
            Return param
        End Function
    End Module
End Namespace