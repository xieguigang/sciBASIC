Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute

Namespace Net.Http

    Public Module RequestBuilder

        ''' <summary>
        ''' 当前的这个函数操作里面已经包含有了URL的编码工作了
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BuildParameters(Of T As Class)(x As T) As String
            Dim type As Type = GetType(T)
            Dim args As New List(Of NamedValue(Of String))
            Dim ps = LoadMapping(type, mapsAll:=True)

            For Each p In ps.Values.Where(Function(o) IsPrimitive(o.Property.PropertyType))
                Dim value As Object = p.GetValue(x)

                If Not value Is Nothing Then
                    Dim s As String = If(
                        value.GetType.IsEnum,
                        DirectCast(value, [Enum]).Description,
                        value.ToString)

                    args += New NamedValue(Of String) With {
                        .x = s,
                        .Name = p.Field.Name
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