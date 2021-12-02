Namespace ApplicationServices.Terminal.TablePrinter

    Public Class ConsoleColorNullable
        Public Sub New()
        End Sub

        Public Sub New(foregroundColor As ConsoleColor)
            Me.ForegroundColor = foregroundColor
            IsForegroundColorNull = False
        End Sub

        Public Sub New(foregroundColor As ConsoleColor, backgroundColor As ConsoleColor)
            Me.ForegroundColor = foregroundColor
            Me.BackgroundColor = backgroundColor
            IsForegroundColorNull = False
            IsBackgroundColorNull = False
        End Sub

        Public IsForegroundColorNull As Boolean = True
        Public IsBackgroundColorNull As Boolean = True
        Public Property ForegroundColor As ConsoleColor
        Public Property BackgroundColor As ConsoleColor
    End Class
End Namespace
