Imports System
Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Namespace GraphEmbedding.util

    Public Class StringSplitter
        Private Shared ReadOnly r As Regex = New Regex("(\(x,y\))|(\(y,x\))|(\(y,z\))|(\(x,z\))|(\(z,x\))")

        Public Shared Function split(separator As String, original As String) As String()
            Dim separator_char As Char() = separator.ToCharArray()
            For i = 1 To separator_char.Length - 1
                original = original.Replace(separator_char(i), separator_char(0))
            Next
            original = r.Replace(original, "")
            Return original.Split(CChar(separator.Substring(0, 1)))
        End Function

        Public Shared Function RemoveEmptyEntries(original As String()) As String()
            Dim len = original.Length
            Dim list As IList(Of String) = New List(Of String)(len)
            For i = 0 To len - 1
                If Not ReferenceEquals(original(i), Nothing) AndAlso Not original(i).Equals("") Then
                    list.Add(original(i))
                End If
            Next
            Dim result = New String(list.Count - 1) {}
            For i = 0 To list.Count - 1
                result(i) = list(i).Trim()
            Next
            Return result
        End Function

    End Class

End Namespace
