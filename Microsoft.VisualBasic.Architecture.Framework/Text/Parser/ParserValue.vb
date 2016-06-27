Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>这个对象的用途和<see cref="NamedValue(Of T)"/>对象的用途是一致的，只不过差异在于所想要表达的寓意上面</remarks>
    Public Structure ParserValue(Of T)
        Public Raw As String
        Public value As T

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace