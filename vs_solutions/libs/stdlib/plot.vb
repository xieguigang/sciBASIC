Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module plot

    Public Function scatter(x As IEnumerable(Of Double), y As IEnumerable(Of Double), Optional title As String = "scatter", Optional size As String = "1200,800") As GraphicsData
        Dim s As New Series With {.Name = title, .X = x.ToArray, .Y = y.ToArray}
        Return plot.scatter(s, size.SizeParser)
    End Function

    Public Function scatter(data As Series, size As Size, Optional theme As PlotTheme = Nothing, Optional driver As Drivers = Drivers.Default) As GraphicsData
        Return New ScatterPlot().Plot(New List(Of Series) From {data}, size, driver:=driver)
    End Function

End Module
