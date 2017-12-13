Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' The object counter
    ''' </summary>
    Public Class Counter : Inherits int

        Sub New()
            MyBase.New(0)
        End Sub

        Sub New(hits As Integer)
            Call MyBase.New(x:=hits)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Hit()
            Value += 1
        End Sub
    End Class
End Namespace