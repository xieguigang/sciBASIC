Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D.ConcaveHull

    Public Module Extensions

        <Extension> Public Function ConcaveHull(points As IEnumerable(Of Point)) As Point()
            With New BallConcave(points)
                Return .GetConcave_Ball(.RecomandedRadius).ToArray
            End With
        End Function
    End Module
End Namespace