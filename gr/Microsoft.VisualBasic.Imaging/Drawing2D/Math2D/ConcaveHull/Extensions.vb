#Region "Microsoft.VisualBasic::37e0c2a1c638ef72d00b5316eaac2d46, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Extensions
    ' 
    '         Function: ConcaveHull
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D.ConcaveHull

    Public Module Extensions

        <Extension> Public Function ConcaveHull(points As IEnumerable(Of PointF), Optional r# = -1) As PointF()
            With New BallConcave(points)
                If r# <= 0 Then
                    r# = .RecomandedRadius
                End If
                Return .GetConcave_Ball(r).ToArray
            End With
        End Function
    End Module
End Namespace
