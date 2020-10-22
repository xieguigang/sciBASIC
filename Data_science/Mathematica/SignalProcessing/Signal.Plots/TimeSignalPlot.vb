Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.SignalProcessing

Public Class TimeSignalPlot : Inherits Plot

    Public Sub New(theme As Theme)
        MyBase.New(theme)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

    End Sub

    Public Overloads Shared Function Plot(signals As IEnumerable(Of GeneralSignal),
                                          Optional size$ = "2700,2400",
                                          Optional padding$ = g.DefaultLargerPadding,
                                          Optional bg$ = "white") As GraphicsData

    End Function
End Class
