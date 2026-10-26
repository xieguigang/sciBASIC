Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging.Driver

Public Class ComplexHeatMap : Inherits HeatmapPlot

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub
End Class
