Imports System.Runtime.CompilerServices

Namespace Math

    Public Module NumeralSystem

        ''' <summary>
        ''' 将十进制数转换到另外的一个数进制
        ''' </summary>
        ''' <param name="d%"></param>
        ''' <param name="alphas"></param>
        ''' <returns></returns>
        <Extension> Public Function TranslateDecimal(d%, alphas As Char()) As String
            Dim r = d Mod alphas.Length
            Dim result$

            If (d - r = 0) Then
                result = alphas(r)
            Else
                result = ((d - r) \ alphas.Length).TranslateDecimal(alphas) & alphas(r)
            End If

            Return result
        End Function
    End Module
End Namespace