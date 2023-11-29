Imports System.Drawing

Public MustInherit Class Simulation

    Friend frameDelay As Integer = 30
    Friend timeStepsPerFrame As Integer = 1
    Friend screenshotRate As Integer = 3
    Friend shouldTakeScreenshots As Boolean = True
    Friend screenshotName As String = "Screenshot"
    Private time As Integer = 0

    ' *************************************************************************
    '                                - DIMENTIONS -                            *
    ' **************************************************************************

    ' simulation canvas size
    Friend Shared width As Integer = 1200, height As Integer = 480
    ' number of data points / pixels per dimention
    Friend xdim As Integer = 1920, ydim As Integer = 1080 ' HD
    ' static int xdim = 2400, ydim = 960;
    ' static int xdim = 1200, ydim = 480;
    ' static int xdim = 600, ydim = 240;
    ' static int xdim = 400, ydim = 160;
    ' static int xdim = 200, ydim = 80;
    ' static int xdim = 100, ydim = 40;

    Public Overridable Sub setDimentions(width As Integer, height As Integer, xdim As Integer, ydim As Integer)
        ' StdDraw.setCanvasSize(width, height)

        ' Set the drawing scale to dimentions
        ' the -.5 is so that the coordinates align with the center of the pixel
        ' StdDraw.setXscale(0 - .5, xdim - .5)
        ' StdDraw.setYscale(0 - .5, ydim - .5)

        ' Set 1px pen radius
        Dim r = 1.0 / width
        ' StdDraw.PenRadius = r
    End Sub

    ''' <summary>
    ''' *************************************************************************
    ''' METHODS                                                                  *
    ''' **************************************************************************
    ''' </summary>

    Public MustOverride Sub reset()
    Public MustOverride Sub advance()
    Public MustOverride Sub draw(g As IGraphics)

    Public Overridable Sub run()
        reset()

        ' animation loop
        While True
            nextFrame()
        End While
    End Sub

    Dim i As Integer = 0

    Private Sub nextFrame()
        For s As Integer = 0 To timeStepsPerFrame - 1
            If time Mod screenshotRate = 0 AndAlso shouldTakeScreenshots Then
                Dim g As Graphics2D = Graphics2D.CreateDevice(New Size(xdim, ydim))
                draw(g)
                Dim st As String = "" & i.ToString().PadLeft(5, "0"c)
                Dim filepath = "video/" & screenshotName & "-T" & st & ".png"
                Call g.Flush()
                Call g.ImageResource.SaveAs(filepath)
                i += 1
            End If
            advance()
            time += 1
        Next
    End Sub

End Class
