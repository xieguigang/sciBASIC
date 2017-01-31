Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Scripting

    ''' <summary>
    ''' 简单的字符串插值引擎，可以用来调试字符串表达式的处理结果
    ''' </summary>
    Public Module StringInterpolation

        ' "abcdefg$h$i is $k \$a"

        Const VB_str$ = "&VB_str"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expr$"></param>
        ''' <param name="getValue">Get string value of the variable in the expression.</param>
        ''' <returns></returns>
        <Extension>
        Public Function Interpolate(expr$, getValue As Func(Of String, String)) As String
            Dim sb As New StringBuilder(expr.Replace("\$", VB_str))
            Dim t = Regex.Matches(sb.ToString, "[$][a-z][a-z0-9]*", RegexICSng).ToArray

            For Each v$ In t
                Dim value$ = getValue(Mid(v, 2))

                If Not value Is Nothing Then
                    Call sb.Replace(v, value)
                End If
            Next

            With sb
                .Replace(VB_str, "$")
                .Replace("\t", vbTab)
                .Replace("\n", vbLf)

                expr = .ToString
            End With

            Return expr
        End Function
    End Module
End Namespace