#Region "Microsoft.VisualBasic::db58ff13194cd94ac30cc6393fbfcd8f, gr\build_3DEngine\isometric\IsometricView\Program.vb"

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

    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Program

    Sub Main()

        With New IsometricViewTest

            Call .doScreenshotCylinder()
            Call .doScreenshotExtrude()
            Call .doScreenshotGrid()
            Call .doScreenshotKnot()
            Call .doScreenshotOctahedron()
            Call .doScreenshotOne()
            Call .doScreenshotPath3D()
            Call .doScreenshotPrism()
            Call .doScreenshotPyramid()
            Call .doScreenshotRotateZ()
            Call .doScreenshotScale()
            Call .doScreenshotStairs()
            Call .doScreenshotThree()
            Call .doScreenshotTranslate()
            Call .doScreenshotTwo()

        End With
    End Sub
End Module
