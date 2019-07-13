#Region "Microsoft.VisualBasic::4f94211002e12eb417d2fb7e10b97dc2, Data_science\Visualization\Plots\g\DataScaler.vb"

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

    '     Module DataScalerExtensions
    ' 
    '         Function: TupleScaler
    ' 
    '     Class TermScaler
    ' 
    '         Properties: AxisTicks, X
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Translate, TranslateX
    ' 
    '     Class YScaler
    ' 
    '         Properties: region, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: TranslateHeight, (+2 Overloads) TranslateY
    ' 
    '     Class DataScaler
    ' 
    '         Properties: AxisTicks, X
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Translate, TranslateX
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Graphic

    Public Module DataScalerExtensions

        ''' <summary>
        ''' Translate the x/y value as a geom point.
        ''' </summary>
        ''' <param name="scaler"></param>
        ''' <param name="bottom">
        ''' 如果是正常的坐标系，那么这个值就必须是一个正数，值为绘图区域的<paramref name="bottom"/>的y值，
        ''' 否则获取得到的y值将会是颠倒过来的，除非将<see cref="Graphics"/>的旋转矩阵给颠倒了
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function TupleScaler(scaler As (x As d3js.scale.LinearScale, y As d3js.scale.LinearScale), Optional bottom% = -1) As Func(Of Double, Double, PointF)
            With scaler
                If bottom > 0 Then
                    Return Function(x, y)
                               Return New PointF(.x(x), bottom - .y(y))
                           End Function
                Else
                    Return Function(x, y)
                               Return New PointF(.x(x), .y(y))
                           End Function
                End If
            End With
        End Function
    End Module

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
                .X = TranslateX(x),
                .Y = TranslateY(y)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x As String) As Double
            Return Me.X.Value(x)
        End Function
    End Class

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

    ''' <summary>
    ''' 将用户数据转换为作图的时候所需要的空间数据
    ''' </summary>
    Public Class DataScaler : Inherits YScaler

        Public Property X As LinearScale
        Public Property AxisTicks As (X As Vector, Y As Vector)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rev">是否需要将Y坐标轴上下翻转颠倒</param>
        Sub New(Optional rev As Boolean = False)
            Call MyBase.New(reversed:=rev)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(x#, y#) As PointF
            Return New PointF With {
                .X = TranslateX(x),
                .Y = TranslateY(y)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(point As PointF) As PointF
            Return Translate(point.X, point.Y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x#) As Double
            Return Me.X(x)
        End Function
    End Class
End Namespace
