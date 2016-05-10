Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Terminal

    Public MustInherit Class AbstractBar

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Prints a simple message 
        ''' </summary>
        ''' <param name="msg">Message to print</param>
        Public Sub PrintMessage(msg As String)
            Call Console.WriteLine(msg)
        End Sub

        Public MustOverride Sub [Step]()
    End Class
End Namespace