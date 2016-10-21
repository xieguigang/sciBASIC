Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LP

    ''' <summary>
    ''' Subject to these <see cref="Equation"/>
    ''' </summary>
    Public Structure Equation

        Dim xyz#(), c#

        Public Overrides Function ToString() As String
            Dim alpha As New Uid(Scan0, "xyzabcdefghijklmnopqrstuvw")
            Dim t As New List(Of String)

            For Each x In xyz
                If x = 1.0R Then
                    t += (+alpha).ToString
                Else
                    t += x & (+alpha).ToString
                End If
            Next

            Return $"{t.JoinBy(" + ")} = {c}"
        End Function
    End Structure
End Namespace