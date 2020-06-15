Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.scale

Namespace Graphic.Axis

    Public Class YScaler

        Public Property Y As LinearScale

        ''' <summary>
        ''' The charting region in <see cref="Rectangle"/> data structure.
        ''' </summary>
        Public Property region As Rectangle

        ''' <summary>
        ''' 是否需要将Y坐标轴上下翻转颠倒
        ''' </summary>
        Dim reversed As Boolean

        Sub New(reversed As Boolean)
            Me.reversed = reversed
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2018-1-16
        ''' 
        ''' 因为绘图的时候有margin的，故而Y不是从零开始的，而是从margin的top开始的
        ''' 所以需要额外的加上一个top值
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateY(y#) As Double
            If reversed Then
                Return Me.Y(y)
            Else
                Return region.Bottom - Me.Y(y) + region.Top
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateY(y As IEnumerable(Of Double)) As IEnumerable(Of Double)
            Return y.Select(AddressOf TranslateY)
        End Function

        Public Function TranslateHeight(y As Double) As Double
            Return region.Bottom - Me.Y(y)
        End Function
    End Class
End Namespace