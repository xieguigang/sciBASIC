#Region "Microsoft.VisualBasic::63224f11017ac6549041d85b9b123373, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Layouts\Body.vb"

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

    '   Total Lines: 110
    '    Code Lines: 38 (34.55%)
    ' Comment Lines: 62 (56.36%)
    '    - Xml Docs: 72.58%
    ' 
    '   Blank Lines: 10 (9.09%)
    '     File Size: 5.07 KB


    '     Class Layout
    ' 
    ' 
    '         Class LayoutBody
    ' 
    '             Properties: Pending, Progress, Text
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: (+2 Overloads) SetBackgroundColor, (+2 Overloads) SetForegroundColor, (+2 Overloads) SetValue
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
