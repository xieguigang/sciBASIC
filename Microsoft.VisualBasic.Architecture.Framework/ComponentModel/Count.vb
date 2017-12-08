Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' The object counter
    ''' </summary>
    Public Class Counter : Inherits Value(Of Integer)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Hit()
            Value += 1
        End Sub
    End Class
End Namespace