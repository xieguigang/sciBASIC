Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D.ConcaveHull

    Public Module Extensions

        <Extension> Public Function ConcaveHull(points As IEnumerable(Of Point), Optional r# = -1) As Point()
            With New BallConcave(points)
                If r# <= 0 Then
                    r# = .RecomandedRadius
                End If
                Return .GetConcave_Ball(r).ToArray
            End With
        End Function
    End Module
End Namespace