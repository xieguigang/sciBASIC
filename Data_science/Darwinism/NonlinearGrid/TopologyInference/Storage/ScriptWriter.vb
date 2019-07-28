Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.Linq

Public Module ScriptWriter

    <Extension>
    Public Function ToString(model As GridMatrix, lang As Languages) As String
        Dim [const] = model.const
        Dim correlations = model.correlations
        Dim direction = model.direction
        Dim visitX = Function(i As Integer)
                         Select Case lang
                             Case Languages.TypeScript
                                 Return $"X[{i}]"
                             Case Languages.PHP
                                 Return $"$X[{i}]"
                             Case Languages.R
                                 Return $"X[{i + 1}]"
                             Case Languages.VisualBasic
                                 Return $"X({i})"
                             Case Else
                                 Return $"X({i})"
                         End Select
                     End Function
        Dim pow = Function(x$, y$)
                      Select Case lang
                          Case Languages.VisualBasic, Languages.R
                              Return $"({x} ^ {y})"
                          Case Languages.TypeScript
                              Return $"Math.pow({x}, {y})"
                          Case Else
                              Return $"pow({x}, {y})"
                      End Select
                  End Function
        Dim formulaText As String = [const].A & " + " &
            correlations _
                .Select(Function(c, i)
                            Return $"{[const].B(i)} + " & c _
                                .AsEnumerable _
                                .Select(Function(cj, j)
                                            Return $"({cj} * {visitX(j)}])"
                                        End Function) _
                                .JoinBy(" + ")
                        End Function) _
                .Select(Function(power, i)
                            Return $"({direction(i)} * {pow(visitX(i), power)})"
                        End Function) _
                .JoinBy(" + " & vbCrLf)

        Select Case lang
            Case Languages.VisualBasic
                Return $"Public Function Grid(X As Double()) As Double
    Return {formulaText}
End Function"
            Case Languages.TypeScript
                Return $"export function Grid(X: number[]) : number {{
    return {formulaText};
}}"
            Case Languages.R
                Return $"
Grid <- function(X) {{
    {formulaText};
}}"
            Case Else
                Return $"function Grid($x) {{
    return {formulaText};
}}"
        End Select
    End Function
End Module
