Imports System.Text

Namespace Logging

    Public Structure LogEntry

        Public Property Msg As String
        Public Property [Object] As String
        Public Property [Type] As MSG_TYPES
        Public Property Time As Date

        ''' <summary>
        ''' 生成日志文档之中的一行记录数据
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim obj As String = TrimObject()
            Dim str As String

            If Msg.Contains(vbCr) OrElse Msg.Contains(vbLf) Then  '多行模式
                str = $"[{Time.ToString}][{Type.ToString}][{[obj]}]{vbCrLf}{Msg}"
            Else                '单行模式
                str = $"[{Time.ToString}][{Type.ToString}][{[obj]}] {Msg}"
            End If

            Return str & vbCrLf
        End Function

        Private Function TrimObject() As String
            Dim str As String = Me.Object.Replace(vbCr, " ").Replace(vbLf, " ")
            Return str
        End Function
    End Structure
End Namespace