Public Class Simulation

    Friend frameDelay As Integer = 30
    Friend timeStepsPerFrame As Integer = 1
    Friend screenshotRate As Integer = 1
    Friend shouldTakeScreenshots As Boolean = False
    Friend screenshotName As String = "Screenshot"
    Private time As Integer = 0

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

    Public Overridable Sub reset()
        ' to be implemented in a subclass
    End Sub

    Public Overridable Sub advance()
        ' to be implemented in a subclass
    End Sub

    Public Overridable Sub draw()
        ' to be implemented in a subclass
    End Sub

    Public Overridable Sub run()
        reset()

        ' control when to show to save running time
        ' StdDraw.enableDoubleBuffering()

        Dim previouslyMousePressed = False

        ' animation loop
        While True
            'If StdDraw.isKeyPressed(KeyEvent.VK_A) Then
            '    ' if "a" key was pressed
            '    playModeField = PlayMode.ANIMATE
            'ElseIf StdDraw.isKeyPressed(KeyEvent.VK_C) Then
            '    ' if "c" key was pressed
            '    playModeField = PlayMode.CLICK_THROUGH
            'ElseIf StdDraw.isKeyPressed(KeyEvent.VK_R) Then
            '    ' if "r" key was pressed
            time = 0
            reset()
            draw()
            '    StdDraw.show()
            'End If

            ' draw frame depending on what play mode we are in
            'Select Case playModeField
            '    Case PlayMode.ANIMATE
            nextFrame()
            '        StdDraw.pause(frameDelay)
            '    Case PlayMode.CLICK_THROUGH
            '        If StdDraw.mousePressed() AndAlso Not previouslyMousePressed Then
            '            ' if new click
            '            nextFrame()
            '        End If
            'End Select
            'previouslyMousePressed = StdDraw.mousePressed()
        End While
    End Sub

    Private Sub nextFrame()
        For s As Integer = 0 To timeStepsPerFrame - 1
            If time Mod screenshotRate = 0 AndAlso shouldTakeScreenshots Then
                draw()
                ' StdDraw.show()
                ' String st = String.format("%08d", time);
                Dim st As String = "" & time.ToString()
                Dim filepath = screenshotName & "-T" & st & ".png"
                ' StdDraw.save(filepath)
            End If
            advance()
            time += 1
        Next
        draw()
        ' StdDraw.show()
    End Sub

End Class
