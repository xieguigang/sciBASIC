Imports System.Collections.Generic

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Triangle

        Private sitesField As List(Of Site)
        Public ReadOnly Property Sites As List(Of Site)
            Get
                Return sitesField
            End Get
        End Property

        Public Sub New(a As Site, b As Site, c As Site)
            sitesField = New List(Of Site)()
            sitesField.Add(a)
            sitesField.Add(b)
            sitesField.Add(c)
        End Sub

        Public Sub Dispose()
            sitesField.Clear()
        End Sub
    End Class
End Namespace
