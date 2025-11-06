' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
    Partial Public Class Layout
        ''' <summary>
        ''' Definition of the Layout used to Render the Body of the ProgressBar
        ''' </summary>
        Public Class LayoutBody
            ' Examples of ProgressBar:
            '      - Marquee is a Character moving around the ProgressBar
            '      
            '      With Progress available (Maximum defined):
            '          [■■■■■■■■■■■■········] -> Without Marquee
            '          [■■■■■■■■■■■■····+···] -> With Marquee (in pending space) 
            '          [■■■■■■■■#■■■········] -> With Marquee (in progress space)
            '          
            '      Without Progress available (don't have Maximum):
            '          [·······■············] -> Marquee is always displayed

            ''' <summary>
            ''' Element To show in Pending Section
            ''' </summary>
            Public ReadOnly Property Pending As Element(Of Char) = New Element(Of Char)()

            ''' <summary>
            ''' Element to show in Progress Section
            ''' </summary>
            Public ReadOnly Property Progress As Element(Of Char) = New Element(Of Char)()

            ''' <summary>
            ''' Layout for the Text 
            ''' </summary>
            Public ReadOnly Property Text As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Sets the LayoutBody value for Pending and Progress elements
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            Public Function SetValue(value As Char) As LayoutBody
                Return SetValue(Function(pb) value)
            End Function
            ''' <summary>
            ''' Sets the LayoutBody value for Pending and Progress elements
            ''' </summary>
            ''' <param name="valueGetter"></param>
            ''' <returns></returns>
            Public Function SetValue(valueGetter As Func(Of ProgressBar, Char)) As LayoutBody
                Pending.SetValue(valueGetter)
                Progress.SetValue(valueGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the ForegroundColor for Pending and Progress elements
            ''' </summary>
            ''' <param name="foregroundColor"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColor As ConsoleColor) As LayoutBody
                Return SetForegroundColor(Function(pb) foregroundColor)
            End Function

            ''' <summary>
            ''' Sets the ForegroundColor for Pending and Progress elements
            ''' </summary>
            ''' <param name="foregroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As LayoutBody
                Pending.SetForegroundColor(foregroundColorGetter)
                Progress.SetForegroundColor(foregroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the BackgroundColor for Pending and Progress elements
            ''' </summary>
            ''' <param name="backgroundColor"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColor As ConsoleColor) As LayoutBody
                Return SetBackgroundColor(Function(pb) backgroundColor)
            End Function
            ''' <summary>
            ''' Sets the BackgroundColor for Pending and Progress elements
            ''' </summary>
            ''' <param name="backgroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As LayoutBody
                Pending.SetBackgroundColor(backgroundColorGetter)
                Progress.SetBackgroundColor(backgroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Ctor
            ''' </summary>
            Public Sub New()
                Pending.SetValue("·"c).SetForegroundColor(ConsoleColor.DarkGray)
                Progress.SetValue("■"c).SetForegroundColor(ConsoleColor.DarkGreen)
                Text.SetVisible(False)
            End Sub
        End Class

    End Class
End Namespace
