#Region "Microsoft.VisualBasic::1a94473c42166f426192891db5a2caa6, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Layouts\Margin.vb"

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

    '   Total Lines: 135
    '    Code Lines: 47 (34.81%)
    ' Comment Lines: 74 (54.81%)
    '    - Xml Docs: 77.03%
    ' 
    '   Blank Lines: 14 (10.37%)
    '     File Size: 6.09 KB


    '     Class Layout
    ' 
    ' 
    '         Class LayoutMargin
    ' 
    '             Properties: [End], Start
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: GetLength, (+2 Overloads) SetBackgroundColor, (+2 Overloads) SetForegroundColor, (+2 Overloads) SetValue, (+2 Overloads) SetVisible
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
        ''' Definition of the Layout used to Render the Margins of the ProgressBar
        ''' </summary>
        Public Class LayoutMargin
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
            ''' Element to show at the Margin Left (Start of the ProgressBar) 
            ''' </summary>
            Public ReadOnly Property Start As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Element to show at the Margin Right (End of the ProgressBar)
            ''' </summary>
            Public ReadOnly Property [End] As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Sets the LayoutMargin value for Start and End elements
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            Public Function SetValue(value As String) As LayoutMargin
                Return SetValue(Function(pb) value)
            End Function

            ''' <summary>
            ''' Sets the LayoutMargin value for Start and End elements
            ''' </summary>
            ''' <param name="valueGetter"></param>
            ''' <returns></returns>
            Public Function SetValue(valueGetter As Func(Of ProgressBar, String)) As LayoutMargin
                Start.SetValue(valueGetter)
                [End].SetValue(valueGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the ForegroundColor for Start and End elements
            ''' </summary>
            ''' <param name="foregroundColor"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColor As ConsoleColor) As LayoutMargin
                Return SetForegroundColor(Function(pb) foregroundColor)
            End Function

            ''' <summary>
            ''' Sets the ForegroundColor for Start and End elements
            ''' </summary>
            ''' <param name="foregroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As LayoutMargin
                Start.SetForegroundColor(foregroundColorGetter)
                [End].SetForegroundColor(foregroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the BackgroundColor for Start and End elements
            ''' </summary>
            ''' <param name="backgroundColor"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColor As ConsoleColor) As LayoutMargin
                Return SetBackgroundColor(Function(pb) backgroundColor)
            End Function

            ''' <summary>
            ''' Sets the BackgroundColor for Start and End elements
            ''' </summary>
            ''' <param name="backgroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As LayoutMargin
                Start.SetBackgroundColor(backgroundColorGetter)
                [End].SetBackgroundColor(backgroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the visibility for Start and End elements
            ''' </summary>
            ''' <param name="visible"></param>
            ''' <returns></returns>
            Public Function SetVisible(visible As Boolean) As LayoutMargin
                Return SetVisible(Function(pb) visible)
            End Function

            ''' <summary>
            ''' Sets the visibility for Start and End elements
            ''' </summary>
            ''' <param name="showGetter"></param>
            ''' <returns></returns>
            Public Function SetVisible(showGetter As Func(Of ProgressBar, Boolean)) As LayoutMargin
                Start.SetVisible(showGetter)
                [End].SetVisible(showGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Return the length of Margins
            ''' </summary>
            ''' <param name="progressBar"></param>
            ''' <returns></returns>
            Public Function GetLength(progressBar As ProgressBar) As Integer
                Return If(Start.GetValue(progressBar)?.Length, 0) + If([End].GetValue(progressBar)?.Length, 0)
            End Function

            ''' <summary>
            ''' Ctor
            ''' </summary>
            Public Sub New()
                Start.SetValue("[").SetForegroundColor(ConsoleColor.DarkBlue)
                [End].SetValue("]").SetForegroundColor(ConsoleColor.DarkBlue)
            End Sub
        End Class

    End Class
End Namespace
