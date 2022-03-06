Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Driver

    ''' <summary>
    ''' 可以借助这个画布对象创建多图层的绘图操作
    ''' </summary>
    Public Class InternalCanvas

        Dim plots As New List(Of IPlot)

        Public Property size As Size
        Public Property padding As Padding
        Public Property bg As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function InvokePlot() As GraphicsData
            Return GraphicsPlots(
                    size, padding, bg,
                    Sub(ByRef g, rect)

                        For Each plot As IPlot In plots
                            Call plot(g, rect)
                        Next
                    End Sub)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(g As InternalCanvas, plot As IPlot) As InternalCanvas
            g.plots += plot
            Return g
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(g As InternalCanvas, plot As IPlot()) As InternalCanvas
            g.plots += plot
            Return g
        End Operator

        Public Shared Narrowing Operator CType(g As InternalCanvas) As GraphicsData
            Return g.InvokePlot
        End Operator

        ''' <summary>
        ''' canvas invoke this plot.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        Public Shared Operator <=(g As InternalCanvas, plot As IPlot) As GraphicsData
            Dim size As Size = g.size
            Dim margin = g.padding
            Dim bg As String = g.bg

            Return GraphicsPlots(size, margin, bg, plot)
        End Operator

        Public Shared Operator >=(g As InternalCanvas, plot As IPlot) As GraphicsData
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace