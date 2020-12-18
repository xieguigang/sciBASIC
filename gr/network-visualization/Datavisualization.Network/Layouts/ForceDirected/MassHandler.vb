Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Layouts.ForceDirected

    Public Class MassHandler

        ReadOnly maxRatio As Double = 2

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">node to move</param>
        ''' <param name="b"></param>
        ''' <param name="dx#"></param>
        ''' <param name="dy#"></param>
        ''' <returns></returns>
        Public Function DeltaMass(a As Node, b As Node, dx#, dy#) As PointF
            If a.data.mass = 0.0 AndAlso b.data.mass = 0.0 Then
                Return New PointF(dx, dy)
            ElseIf a.data.mass = 0 Then
                Return New PointF(dx, dy)
            ElseIf b.data.mass = 0 Then
                Return New PointF(0, 0)
            Else
                Dim ratio As Double = b.data.mass / a.data.mass

                If ratio > maxRatio Then
                    ratio = maxRatio
                End If

                Return New PointF(ratio * dx, ratio * dy)
            End If
        End Function
    End Class
End Namespace