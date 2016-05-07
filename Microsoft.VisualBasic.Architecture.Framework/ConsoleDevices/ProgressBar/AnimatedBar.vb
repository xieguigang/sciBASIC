Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Terminal

    Public Class AnimatedBar
        Inherits AbstractBar

        Dim animation As List(Of String)
        Dim counter As Integer

        Public Sub New()
            MyBase.New()
            Me.animation = New List(Of String)() From {"/", "-", "\", "|"}
            Me.counter = 0
        End Sub

        ''' <summary>
        ''' prints the character found in the animation according to the current index
        ''' </summary>
        Public Overrides Sub [Step]()
            Console.Write(vbCr)
            Console.Write(Me.animation(Me.counter) & vbBack)
            Me.counter += 1
            If Me.counter = Me.animation.Count Then
                Me.counter = 0
            End If
        End Sub
    End Class
End Namespace