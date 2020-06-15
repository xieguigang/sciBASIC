Imports System.Runtime.CompilerServices

Namespace Graphic.Canvas

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Function IsNullOrEmpty(layout As Layout) As Boolean
            If layout Is Nothing Then
                Return True
            Else
                Return layout.isEmpty
            End If
        End Function
    End Module
End Namespace