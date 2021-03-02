Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My

Namespace ApplicationServices.Debugging

    Public Class Assert

        Public Shared Sub AreEqual(Of T As IComparable(Of T))(w As T, v As T, <CallerMemberName> Optional caller As String = Nothing)
            If w.CompareTo(v) = 0 Then
                Call Log4VB.Print($"[{caller}] AreEqual" & vbCrLf, ConsoleColor.White, ConsoleColor.Green)
            Else
                Call Log4VB.Print($"[{caller}] {w} and {v} those two value are not equal!" & vbCrLf, ConsoleColor.Yellow, ConsoleColor.Red)
            End If
        End Sub

        Public Shared Sub IsTrue(v As Boolean, <CallerMemberName> Optional caller As String = Nothing)
            If v Then
                Call Log4VB.Print($"[{caller}] assert is {v.ToString.ToUpper}" & vbCrLf, ConsoleColor.White, ConsoleColor.Green)
            Else
                Call Log4VB.Print($"[{caller}] assert is {v.ToString.ToUpper}" & vbCrLf, ConsoleColor.Yellow, ConsoleColor.Red)
            End If
        End Sub
    End Class
End Namespace