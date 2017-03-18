Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Text.Xml

    Public Module Extensions

        ''' <summary>
        ''' 使用这个函数删除xml文本字符串之中的无效的字符
        ''' </summary>
        ''' <param name="xml$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StripInvalidCharacters(xml$) As String
            Dim sb As New StringBuilder(xml)

            Call sb.Replace("&#x8;", ".")

            Return sb.ToString
        End Function
    End Module
End Namespace