#Region "Microsoft.VisualBasic::07a4c9b714d10ef5926958918c39880c, Microsoft.VisualBasic.Core\Extensions\WebServices\RequestBuilder.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module RequestBuilder
    ' 
    '         Function: BuildParameters, IsPrimitive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.DataFrameColumnAttribute
Imports Microsoft.VisualBasic.Language

Namespace Net.Http

    Public Module RequestBuilder

        ''' <summary>
        ''' Encoding a class object as url parameters.
        ''' (当前的这个函数操作里面已经包含有了URL的编码工作了)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BuildParameters(Of T As Class)(obj As T) As String
            Dim type As Type = GetType(T)
            Dim args As New List(Of NamedValue(Of String))
            Dim ps = LoadMapping(type, mapsAll:=True)

            For Each p As BindProperty(Of DataFrameColumnAttribute) In ps.Values.Where(IsPrimitive)
                Dim value As Object = p.GetValue(obj)

                If Not value Is Nothing Then
                    Dim str$

                    If value.GetType.IsEnum Then
                        str = DirectCast(value, [Enum]).Description
                    Else
                        str = value.ToString
                    End If

                    args += New NamedValue(Of String) With {
                        .Value = str,
                        .Name = p.field.Name
                    }
                End If
            Next

            Dim param As String = args _
                .Select(Function(o) $"{o.Name}={o.Value.UrlEncode}") _
                .JoinBy("&")
            Return param
        End Function

        Private Function IsPrimitive() As Func(Of BindProperty(Of DataFrameColumnAttribute), Boolean)
            Return Function(o)
                       Return DataFramework.IsPrimitive(DirectCast(o.member, PropertyInfo).PropertyType)
                   End Function
        End Function
    End Module
End Namespace
