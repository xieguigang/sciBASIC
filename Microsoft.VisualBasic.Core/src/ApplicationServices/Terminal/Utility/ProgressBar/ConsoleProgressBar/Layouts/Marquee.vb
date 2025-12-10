#Region "Microsoft.VisualBasic::13f7d251c4fb36937ef4df0c3eea470f, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Layouts\Marquee.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 128
    '    Code Lines: 44 (34.38%)
    ' Comment Lines: 70 (54.69%)
    '    - Xml Docs: 75.71%
    ' 
    '   Blank Lines: 14 (10.94%)
    '     File Size: 6.10 KB


    '     Class Layout
    ' 
    ' 
    '         Class LayoutMarquee
    ' 
    '             Properties: OverPending, OverProgress
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: (+2 Overloads) SetBackgroundColor, (+2 Overloads) SetForegroundColor, (+2 Overloads) SetValue, (+2 Overloads) SetVisible
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
