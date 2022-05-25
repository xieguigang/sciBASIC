Imports Microsoft.VisualBasic.Imaging.d3js.Layout

Namespace Drawing2D.Text.Nudge

    Public Class GraphicsTextHandle

        Public Property texts As Label()

        ''' <summary>
        ''' the internal plot region
        ''' </summary>
        ''' <returns></returns>
        Public Property canvas As GraphicsRegion

        Public Function get_xlim() As Double()
            With canvas.PlotRegion
                Return { .X, .Right}
            End With
        End Function

        Public Function get_ylim() As Double()
            With canvas.PlotRegion
                Return { .Top, .Bottom}
            End With
        End Function

        Friend Function get_figheight() As Double
            Return canvas.PlotRegion.Height
        End Function

        Friend Function get_figwidth() As Double
            Return canvas.PlotRegion.Width
        End Function
    End Class
End Namespace