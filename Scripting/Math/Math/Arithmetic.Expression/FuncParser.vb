Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Parser for the user define <see cref="Func"/>.(用户自定义函数的解析器)
''' </summary>
Public Module FuncParser

    Public Function TryParse(s As String) As Func
        Dim define As String = Mid(s, 1, InStr(s, ")"))
        Dim expr As String = Mid(s, define.Length + 1)
        Return define.__defineParser(expr)
    End Function

    <Extension>
    Private Function __defineParser(define As String, expr As String) As Func
        Dim name As String = Mid(define, 1, InStr(define, "(") - 1)
        Dim args As String = Mid(define, name.Length + 1).GetStackValue("(", ")")
        Return New Func With {
            .Args = args.Split(","c).ToArray(Function(s) s.Trim),
            .Expression = expr,
            .Name = name
        }
    End Function
End Module
