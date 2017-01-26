Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace Language

    ''' <summary>
    ''' Variable model in VisualBasic
    ''' </summary>
    Public Class Value : Inherits Value(Of Object)
        Implements INamedValue

        Public Property Name As String Implements IKeyedEntity(Of String).Key
        Public Property Type As Type
        ''' <summary>
        ''' 这个变量所在的函数的位置记录
        ''' </summary>
        ''' <returns></returns>
        Public Property Trace As NamedValue(Of Type)

        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return Nothing
            End If
            Return value.ToString
        End Function
    End Class
End Namespace