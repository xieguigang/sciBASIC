Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity

    Public Module Extensions

        Public ReadOnly Property ClassTable As IReadOnlyDictionary(Of String, WordClass)

        Sub New()
            ClassTable = Enums(Of WordClass).ToDictionary(Function(c) c.Description)
        End Sub

        Public Function GetClass(tag As String) As WordClass
            With LCase(tag)
                If ClassTable.ContainsKey(.ByRef) Then
                    Return ClassTable(.ByRef)
                Else
                    Return WordClass.NA
                End If
            End With
        End Function
    End Module
End Namespace