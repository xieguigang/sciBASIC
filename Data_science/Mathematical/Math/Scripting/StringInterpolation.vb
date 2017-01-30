Imports System.Runtime.CompilerServices

Namespace Scripting

    ''' <summary>
    ''' 简单的字符串插值引擎，可以用来调试字符串表达式的处理结果
    ''' </summary>
    Public Module StringInterpolation

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expr$"></param>
        ''' <param name="getValue">Get string value of the variable in the expression.</param>
        ''' <returns></returns>
        <Extension>
        Public Function Interpolate(expr$, getValue As Func(Of String, String)) As String

        End Function
    End Module
End Namespace