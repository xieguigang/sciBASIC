Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging

    Public Class Assert

        Public Shared Sub AreEqual(Of T As IComparable(Of T))(w As T, v As T, <CallerMemberName> Optional caller As String = Nothing)
            If w.CompareTo(v) = 0 Then
                Call Console.WriteLine($"[{caller}] AreEqual")
            Else
                Call Console.WriteLine($"[{caller}] {w} and {v} those two value are not equal!")
            End If
        End Sub

        Public Shared Sub IsTrue(v As Boolean, <CallerMemberName> Optional caller As String = Nothing)
            Call Console.WriteLine($"[{caller}] assert is {v.ToString.ToUpper}")
        End Sub
    End Class
End Namespace