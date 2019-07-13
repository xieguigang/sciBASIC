#Region "Microsoft.VisualBasic::675a967a44add44cc604a19e846f2e28, Data_science\Visualization\test\ColorLegendTest.vb"

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

    ' Module ColorLegendTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Module ColorLegendTest

    Sub Main()

        Dim colors As SolidBrush() = Designer.GetColors(ColorBrewer.DivergingSchemes.RdBu9, 200).GetBrushes
        Dim ticks = AxisScalling.CreateAxisTicks(data:=New Double() {-10.3301, 13.7566}, ticks:=20)
        Dim range = ticks.Range

        Using g As Graphics2D = New Size(1200, 200).CreateGDIDevice

            Call colors.ColorLegendHorizontal(ticks, g, New Rectangle(New Point, g.Size))
            Call g.Save("./test.legend.png", ImageFormats.Png)

        End Using
    End Sub
End Module
