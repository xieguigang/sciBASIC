Imports System.Collections.Generic

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Triangle

        Public ReadOnly Property Sites As List(Of Site)

        Public Sub New(a As Site, b As Site, c As Site)
            Sites = New List(Of Site)() From {a, b, c}
        End Sub

        Public Sub Dispose()
            Sites.Clear()
        End Sub
    End Class
End Namespace
