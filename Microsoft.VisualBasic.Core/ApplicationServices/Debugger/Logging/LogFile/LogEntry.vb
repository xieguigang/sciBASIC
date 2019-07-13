#Region "Microsoft.VisualBasic::b140e1247998449c2e44aee59c798206, Microsoft.VisualBasic.Core\ApplicationServices\Debugger\Logging\LogFile\LogEntry.vb"

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

    '     Structure LogEntry
    ' 
    '         Properties: [Object], [Type], Msg, Time
    ' 
    '         Function: FormatMessage, ToString, TrimObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace ApplicationServices.Debugging.Logging

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FormatMessage(header$, message$, level As MSG_TYPES) As String
            Return New LogEntry With {
                .Msg = message,
                .Object = header,
                .Type = level,
                .Time = Now
            }.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function TrimObject() As String
            Return Object$.Replace(vbCr, " ").Replace(vbLf, " ")
        End Function
    End Structure
End Namespace
