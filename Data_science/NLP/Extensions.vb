Imports System.Runtime.CompilerServices

Public Module Extensions

    <Extension>
    Public Iterator Function StemmerNormalize(docs As IEnumerable(Of String)) As IEnumerable(Of String)
        Dim s As New Stemmer()

        For Each str As String In docs
            Dim norm As New List(Of String)

            str = Strings.Trim(str).ToLower

            For Each token As String In str.Split
                For Each c As Char In token
                    Call s.add(c)
                Next

                Call norm.Add(s.ToString)
            Next

            Yield norm.JoinBy(" ")
        Next
    End Function

    Public Iterator Function StemmerNormalize(str As String) As IEnumerable(Of String)
        Dim s As New Stemmer()

        For Each token As String In Strings.Trim(str).ToLower.Split
            For Each c As Char In token
                Call s.add(c)
            Next

            Yield s.ToString
        Next
    End Function
End Module
