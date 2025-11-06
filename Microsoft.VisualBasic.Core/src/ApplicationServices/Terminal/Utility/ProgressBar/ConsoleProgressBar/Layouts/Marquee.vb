' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports System

Namespace iluvadev.ConsoleProgressBar

    Public Partial Class Layout
        ''' <summary>
        ''' Definition for the Marquee
        ''' The Marquee is a char that moves around the ProgressBar
        ''' </summary>
        Public Class LayoutMarquee
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
            ''' Marquee definition when it moves over 'Pending' section
            ''' </summary>
            Public ReadOnly Property OverPending As Element(Of Char) = New Element(Of Char)()

            ''' <summary>
            ''' Marquee definition when it moves over 'Progress' section
            ''' </summary>
            Public ReadOnly Property OverProgress As Element(Of Char) = New Element(Of Char)()

            ''' <summary>
            ''' Sets the Marqee definition when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            Public Function SetValue(value As Char) As LayoutMarquee
                Return SetValue(Function(pb) value)
            End Function

            ''' <summary>
            ''' Sets the Marqee definition when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="valueGetter"></param>
            ''' <returns></returns>
            Public Function SetValue(valueGetter As Func(Of ProgressBar, Char)) As LayoutMarquee
                OverPending.SetValue(valueGetter)
                OverProgress.SetValue(valueGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Marqee Foreground Color when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="foregroundColor"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColor As ConsoleColor) As LayoutMarquee
                Return SetForegroundColor(Function(pb) foregroundColor)
            End Function

            ''' <summary>
            ''' Sets the Marqee Foreground Color when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="foregroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As LayoutMarquee
                OverPending.SetForegroundColor(foregroundColorGetter)
                OverProgress.SetForegroundColor(foregroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Marqee Background Color when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="backgroundColor"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColor As ConsoleColor) As LayoutMarquee
                Return SetBackgroundColor(Function(pb) backgroundColor)
            End Function

            ''' <summary>
            ''' Sets the Marqee Background Color when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="backgroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As LayoutMarquee
                OverPending.SetBackgroundColor(backgroundColorGetter)
                OverProgress.SetBackgroundColor(backgroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Marqee Visibility when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="visible"></param>
            ''' <returns></returns>
            Public Function SetVisible(visible As Boolean) As LayoutMarquee
                Return SetVisible(Function(pb) visible)
            End Function

            ''' <summary>
            ''' Sets the Marqee Visibility when it moves over 'Pending' or 'Progress' section
            ''' </summary>
            ''' <param name="showGetter"></param>
            ''' <returns></returns>
            Public Function SetVisible(showGetter As Func(Of ProgressBar, Boolean)) As LayoutMarquee
                OverPending.SetVisible(showGetter)
                OverProgress.SetVisible(showGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Ctor
            ''' </summary>
            Public Sub New()
                OverPending.SetValue(Function(pb) If(pb.HasProgress, "+"c, "■"c)).SetForegroundColor(Function(pb) If(pb.HasProgress, ConsoleColor.Yellow, ConsoleColor.Green))

                OverProgress.SetValue("■"c).SetForegroundColor(ConsoleColor.Yellow)
            End Sub
        End Class
    End Class
End Namespace
