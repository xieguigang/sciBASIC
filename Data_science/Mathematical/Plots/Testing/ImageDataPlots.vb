Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.ImageDataExtensions
Imports Microsoft.VisualBasic.Imaging

Module ImageDataPlots

    Sub Main()
        Call Plot2DMap()
        Call Plot3DMap()

        Pause()
    End Sub

    Sub Plot2DMap()
        Dim img As Image = LoadImage("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.SPCC.png")
        Dim out As Image = Image2DMap(img)

        Call out.SaveAs("./testmap.png")
    End Sub

    Sub Plot3DMap()

    End Sub
End Module
