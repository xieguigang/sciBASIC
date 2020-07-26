Namespace Terminal.ProgressBar

    ''' <summary>
    ''' 两个进度条叠加在一起
    ''' </summary>
    Public Class StackedBar

        Dim bottom, top, background As ConsoleColor

        Sub New(bottomColor As ConsoleColor, topColor As ConsoleColor, background As ConsoleColor)
            Me.bottom = bottomColor
            Me.top = topColor
            Me.background = background
        End Sub

        Public Sub StepProgress(bottom%, top%)

        End Sub
    End Class
End Namespace