#Region "Microsoft.VisualBasic::53be0e0688910726f8ba1fbef0059abc, gr\physics\Debugger.vb"

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

    ' Module Debugger
    ' 
    '     Function: Vector2D
    ' 
    '     Sub: ShowForce
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Debugger

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(X), v(Y))
    End Function

    <Extension> Public Sub ShowForce(m As MassPoint, ByRef canvas As Graphics2D, F As IEnumerable(Of Force), Optional offset As PointF = Nothing)
        Dim font As New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Bold)
        Dim a = m.Point.Vector2D.OffSet2D(offset)

        '#If DEBUG Then
        '        Call $"Sum({F.JoinBy(", ")}) = {F.Sum}".__DEBUG_ECHO
        '#End If

        With canvas

            Call .DrawCircle(a, 10, Brushes.Black)
            ' Call .DrawString(m.ToString, font, Brushes.Black, a)

            Dim draw = Sub(force As Force, color As SolidBrush)
                           Dim v = force.Decomposition2D
                           Dim b = (m.Point + v).Vector2D.OffSet2D(offset)
                           Dim pen As New Pen(color.Color) With {
                               .EndCap = LineCap.ArrowAnchor,
                               .Width = 8
                           }

                           Call .DrawLine(pen, a, b)
                           Call .DrawString(force.ToString, font, color, b)
                       End Sub

            For Each force As Force In F
                Call draw(force, Brushes.SkyBlue)
            Next

            ' 绘制出合力
            Call draw(F.Sum, Brushes.Violet)
        End With
    End Sub
End Module
