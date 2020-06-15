Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Graphic.Axis

    Public Class TermScaler : Inherits YScaler

        Public Property X As OrdinalScale
        Public Property AxisTicks As (X As String(), Y As Vector)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rev">是否需要将Y坐标轴上下翻转颠倒</param>
        Sub New(Optional rev As Boolean = False)
            Call MyBase.New(reversed:=rev)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(x$, y#) As PointF
            Return New PointF With {
                .x = TranslateX(x),
                .y = TranslateY(y)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x As String) As Double
            Return Me.X.Value(x)
        End Function
    End Class
End Namespace