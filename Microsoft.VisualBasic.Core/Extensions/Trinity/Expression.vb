Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity

    ''' <summary>
    ''' Natural expression builder for AI output
    ''' </summary>
    Public Module Expression

        <Extension>
        Public Function Concatenate(list As IEnumerable(Of String), Optional comma$ = ",", Optional andalso$ = "and", Optional etc$ = "etc") As String
            With list.ToArray
                If .Length = 1 Then
                    Return .ByRef(0)
                ElseIf .Length < 8 Then
                    Return .Take(.Length - 1).JoinBy(comma & " ") & $" {[andalso]} " & .Last
                Else
                    Return .Take(7).JoinBy(comma & " ") & $" {[andalso]} " & .ByRef(7) & $"{comma} {etc}"
                End If
            End With
        End Function
    End Module
End Namespace