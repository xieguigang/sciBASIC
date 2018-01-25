Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace HTML.CSS

    Public Module Extensions

        ReadOnly tags As Dictionary(Of String, HtmlTags) =
            Enums(Of HtmlTags)() _
            .ToDictionary(Function(t) t.Description.ToLower)

        <Extension>
        Public Function GetTagValue(str As String) As HtmlTags
            With Strings.Trim(str).ToLower
                If tags.ContainsKey(.ref) Then
                    Return tags(.ref)
                Else
                    Return HtmlTags.NA
                End If
            End With
        End Function
    End Module
End Namespace