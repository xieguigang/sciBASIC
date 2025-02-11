Imports std = System.Math

Namespace EMD
    ''' <summary>
    ''' @author Telmo Menezes (telmo@telmomenezes.com)
    ''' 
    ''' </summary>
    Public Class Feature2D
        Implements Feature

        Private x As Double
        Private y As Double

        Public Sub New(x As Double, y As Double)
            Me.x = x
            Me.y = y
        End Sub

        Public Overridable Function groundDist(f As Feature) As Double Implements Feature.groundDist
            Dim f2d = CType(f, Feature2D)
            Dim deltaX = x - f2d.x
            Dim deltaY = y - f2d.y
            Return std.Sqrt(deltaX * deltaX + deltaY * deltaY)
        End Function

        Public Overrides Function ToString() As String
            Return $"({x}, {y})"
        End Function
    End Class
End Namespace
