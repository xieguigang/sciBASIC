Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Scripting

Namespace ComponentModel.Ranges

    Public Module Extensions

        ''' <summary>
        ''' + ``min -> max``
        ''' + ``[min,max]``
        ''' + ``{min,max}``
        ''' + ``(min,max)``
        ''' + ``min,max``
        ''' </summary>
        ''' <param name="exp$"></param>
        ''' <param name="min#"></param>
        ''' <param name="max#"></param>
        <Extension> Public Sub Parser(exp$, ByRef min#, ByRef max#)
            Dim t$()
            Dim raw$ = exp

            If InStr(exp, "->") > 0 Then
                t = Strings.Split(exp, "->")
            Else
                exp = Regex.Match(exp, "\d+\s*,\s*\d+").Value

                If String.IsNullOrEmpty(exp) Then
                    exp = $"'{raw}' is not a valid expression format!"
                    Throw New FormatException(exp)
                Else
                    t = exp.Split(","c)
                End If
            End If

            t = t.Select(AddressOf Trim)

            min = Casting.ParseNumeric(t(Scan0))
            max = Casting.ParseNumeric(t(1))
        End Sub
    End Module
End Namespace