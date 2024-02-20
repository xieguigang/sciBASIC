Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.STDIO__

Namespace ApplicationServices.Terminal.LineEdit

    Public Class LineReader : Implements IShellDevice

        Dim prompt As String
        Dim line As LineEditor

        Sub New(reader As LineEditor)
            line = reader
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPrompt(s As String) Implements IShellDevice.SetPrompt
            prompt = s
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadLine() As String Implements IShellDevice.ReadLine
            Return line.Edit(prompt, "")
        End Function
    End Class
End Namespace