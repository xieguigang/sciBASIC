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
