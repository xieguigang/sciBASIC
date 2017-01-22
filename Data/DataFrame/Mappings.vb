Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' 在写csv的时候生成列域名的映射的一些快捷函数
''' </summary>
Public Class MappingsHelper

    ''' <summary>
    ''' <see cref="NamedValue(Of T)"/>
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="value$"></param>
    ''' <returns></returns>
    Public Shared Function NamedValueMapsWrite(name$, value$, Optional description$ = NameOf(NamedValue(Of Object).Description)) As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {
            {NameOf(NamedValue(Of Object).Name), name},
            {NameOf(NamedValue(Of Object).Value), value},
            {NameOf(NamedValue(Of Object).Description), description}
        }
    End Function
End Class
