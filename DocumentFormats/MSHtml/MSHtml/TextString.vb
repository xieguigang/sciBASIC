Imports System.Drawing

Public Class TextString

    Public Property Font As Font
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Class

Public Module TextAPI

    ''' <summary>
    ''' 执行栈空间解析
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    Public Function TryParse(html As String) As TextString()
        Dim chars As New List(Of Char)
    End Function
End Module
