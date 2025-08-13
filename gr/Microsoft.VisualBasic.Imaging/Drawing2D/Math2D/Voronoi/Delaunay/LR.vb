Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class LR

        Public Shared ReadOnly LEFT As LR = New LR("left")
        Public Shared ReadOnly RIGHT As LR = New LR("right")

        Private name As String

        Public Sub New(name As String)
            Me.name = name
        End Sub

        Public Shared Function Other(leftRight As LR) As LR
            Return If(leftRight Is LEFT, RIGHT, LEFT)
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class
End Namespace
