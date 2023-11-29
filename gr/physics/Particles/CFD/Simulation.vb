Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.My.JavaScript

Public MustInherit Class Simulation : Implements requestAnimationFrame

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

        Me.xdim = xdim
        Me.ydim = ydim

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
    Protected MustOverride Sub draw(g As IGraphics) Implements requestAnimationFrame.requestAnimationFrame

End Class

''' <summary>
''' the simulation viewer
''' </summary>
Public Class AnimationBuilder

    Public Property fs As IFileSystemEnvironment
    Public Property xdim As Integer = 1920
    Public Property ydim As Integer = 1080

    Friend frameDelay As Integer = 30
    Friend timeStepsPerFrame As Integer = 1
    Friend screenshotRate As Integer = 250
    Friend shouldTakeScreenshots As Boolean = True
    Friend screenshotName As String = "Screenshot"

    Dim time As Integer = 0
    Dim i As Integer = 0

    Sub Run(source As Simulation)
        Call source.reset()
        Call source.setDimentions(0, 0, xdim, ydim)

        Do While True
            If time Mod screenshotRate = 0 AndAlso shouldTakeScreenshots Then
                Dim g As Graphics2D = Graphics2D.CreateDevice(New Size(xdim, ydim))
                Dim st As String = "" & i.ToString().PadLeft(5, "0"c)
                Dim filepath = fs.OpenFile("video/" & screenshotName & "-T" & st & ".png",, IO.FileAccess.Write)
                Call DirectCast(source, requestAnimationFrame).requestAnimationFrame(g)
                Call g.Flush()
                Call g.ImageResource.Save(filepath, ImageFormats.Png.GetFormat)
                Call filepath.Flush()
                Call filepath.Dispose()
                i += 1
            End If
            source.advance()
            time += 1
        Loop
    End Sub

End Class