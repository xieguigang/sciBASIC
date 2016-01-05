Module GDI_API

    ''' <summary>
    ''' 获取目标窗口的截图，截图失败会返回空值
    ''' </summary>
    ''' <param name="Form"></param>
    ''' <returns></returns>
    Public Function GetScreenshot(Form As Form) As Image
        If Form Is Nothing OrElse Not Form.IsHandleCreated Then
            Return Nothing
        Else
            Return GetScreenshot(Form.Bounds)
        End If
    End Function

    ''' <summary>
    ''' 获取屏幕上面的指定区域的截图
    ''' </summary>
    ''' <param name="ScreenRECT"></param>
    ''' <returns></returns>
    Public Function GetScreenshot(ScreenRECT As Rectangle) As Image
        ' gets the upper left hand coordinate of the form
        Dim frmleft As System.Drawing.Point = ScreenRECT.Location
        'use the commented out version for the full screen
        'Dim bmp As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)

        'this version get the size of the form1  The + 8 adds a little to right and bottom of what is captured.
        Dim bmp As New Bitmap(ScreenRECT.Width + 8, ScreenRECT.Height + 8)

        'creates the grapgic
        Using graph As Graphics = Graphics.FromImage(bmp)

            'Gets the x,y coordinates from the upper left start point
            'used below 
            Dim screenx As Integer = frmleft.X
            Dim screeny As Integer = frmleft.Y

            ' The - 5 here allows more of the form to be shown for the top and left sides.
            Call graph.CopyFromScreen(screenx - 5, screeny - 5, 0, 0, bmp.Size)

            Return bmp
        End Using
    End Function

    ''' <summary>
    ''' 获取整个屏幕的截图
    ''' </summary>
    ''' <returns></returns>
    Public Function GetScreenshot() As Image
        Return GetScreenshot(Screen.PrimaryScreen.Bounds)
    End Function
End Module
