Imports System.Drawing

Public Class Particle : Implements Layout2D

    Public Property X As Double Implements Layout2D.X
        Get
            Return predictedPosition.x
        End Get
        Set(value As Double)
            If Not predictedPosition Is Nothing Then
                predictedPosition.x = value
            End If
        End Set
    End Property

    Public Property Y As Double Implements Layout2D.Y
        Get
            Return predictedPosition.y
        End Get
        Set(value As Double)
            If Not predictedPosition Is Nothing Then
                predictedPosition.y = value
            End If
        End Set
    End Property

    Public position As Vector2
    Public velocity As Vector2
    Public index As Integer
    Public predictedPosition As Vector2

    ''' <summary>
    ''' Density, Near Density
    ''' </summary>
    Public density As Vector2

    Sub New(i As Integer, box As Size)
        index = i
        position = Vector2.random(box)
        velocity = Vector2.random(New SizeF(10, 10))
        predictedPosition = Vector2.zero
        density = Vector2.zero
    End Sub

End Class