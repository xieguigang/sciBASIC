#Region "Microsoft.VisualBasic::bba54e2d03b32551d6198df4c123ce42, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Text\Body.vb"

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

    '   Total Lines: 158
    '    Code Lines: 74 (46.84%)
    ' Comment Lines: 67 (42.41%)
    '    - Xml Docs: 89.55%
    ' 
    '   Blank Lines: 17 (10.76%)
    '     File Size: 7.03 KB


    '     Class Text
    ' 
    ' 
    '         Class TextBody
    ' 
    '             Properties: Done, Paused, Processing
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: GetCurrentText, GetPauseText, GetProgressText, (+2 Overloads) SetBackgroundColor, (+2 Overloads) SetForegroundColor
    '                       (+2 Overloads) SetValue, (+2 Overloads) SetVisible
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

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar.Extensions

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
    Partial Public Class Text
        ''' <summary>
        ''' Definition for the Texts in a ProgressBar
        ''' </summary>
        Public Class TextBody
            ''' <summary>
            ''' Text in Body definition when ProgressBar is "Processing"
            ''' </summary>
            Public ReadOnly Property Processing As New Element(Of String)()

            ''' <summary>
            ''' Text in Body definition when ProgressBar is "Paused"
            ''' </summary>
            Public ReadOnly Property Paused As New Element(Of String)()

            ''' <summary>
            ''' Text in Body definition when ProgressBar is "Done"
            ''' </summary>
            Public ReadOnly Property Done As New Element(Of String)()

            ''' <summary>
            ''' Sets the Body Text visibility
            ''' </summary>
            ''' <param name="show"></param>
            ''' <returns></returns>
            Public Function SetVisible(show As Boolean) As TextBody
                Return SetVisible(Function(pb) show)
            End Function

            ''' <summary>
            ''' Sets the Body Text visibility
            ''' </summary>
            ''' <param name="showGetter"></param>
            ''' <returns></returns>
            Public Function SetVisible(showGetter As Func(Of ProgressBar, Boolean)) As TextBody
                Processing.SetVisible(showGetter)
                Paused.SetVisible(showGetter)
                Done.SetVisible(showGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Body Text definition in all ProgressBar states ("Processing", "Paused", "Done")
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            Public Function SetValue(value As String) As TextBody
                Return SetValue(Function(pb) value)
            End Function

            ''' <summary>
            ''' Sets the Body Text definition in all ProgressBar states ("Processing", "Paused", "Done")
            ''' </summary>
            ''' <param name="valueGetter"></param>
            ''' <returns></returns>
            Public Function SetValue(valueGetter As Func(Of ProgressBar, String)) As TextBody
                Processing.SetValue(valueGetter)
                Paused.SetValue(valueGetter)
                Done.SetValue(valueGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Body Text Foreground Color
            ''' </summary>
            ''' <param name="foregroundColor"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColor As ConsoleColor) As TextBody
                Return SetForegroundColor(Function(pb) foregroundColor)
            End Function

            ''' <summary>
            ''' Sets the Body Text Foreground Color
            ''' </summary>
            ''' <param name="foregroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As TextBody
                Processing.SetForegroundColor(foregroundColorGetter)
                Paused.SetForegroundColor(foregroundColorGetter)
                Done.SetForegroundColor(foregroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Body Text Background Color
            ''' </summary>
            ''' <param name="backgroundColor"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColor As ConsoleColor) As TextBody
                Return SetBackgroundColor(Function(pb) backgroundColor)
            End Function

            ''' <summary>
            ''' Sets the Body Text Background Color
            ''' </summary>
            ''' <param name="backgroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As TextBody
                Processing.SetBackgroundColor(backgroundColorGetter)
                Paused.SetBackgroundColor(backgroundColorGetter)
                Done.SetBackgroundColor(backgroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Ctor
            ''' </summary>
            Public Sub New()
                Processing.SetValue(AddressOf GetProgressText).SetForegroundColor(ConsoleColor.Cyan)
                Paused.SetValue(AddressOf GetPauseText).SetForegroundColor(ConsoleColor.DarkCyan)
                Done.SetValue("Done!").SetForegroundColor(ConsoleColor.DarkYellow)
            End Sub

            Private Function GetPauseText(pb As ProgressBar) As String
                Return If(pb.HasProgress,
                    $"Paused... Running time: {pb.TimeProcessing.ToStringWithAllHours()}",
                    $"{pb.Value} of {pb.Maximum} in {pb.TimeProcessing.ToStringWithAllHours()} (paused)")
            End Function

            Private Function GetProgressText(pb As ProgressBar) As String
                Return If(pb.HasProgress,
                    $"{pb.Value} of {pb.Maximum} in {pb.TimeProcessing.ToStringWithAllHours()}, remaining: {pb.TimeRemaining.ToStringAsSumarizedRemainingText()}",
                    $"Processing... ({pb.Value} in {pb.TimeProcessing.ToStringWithAllHours()})")
            End Function

            ''' <summary>
            ''' Gets the current Text Body definition by the ProgressBar context ("Processing", "Paused" or "Done")
            ''' </summary>
            ''' <param name="progressBar"></param>
            ''' <returns></returns>
            Public Function GetCurrentText(progressBar As ProgressBar) As Element(Of String)
                If progressBar Is Nothing Then
                    Return Nothing
                ElseIf progressBar.IsPaused Then
                    Return Paused
                ElseIf progressBar.IsDone Then
                    Return Done
                ElseIf progressBar.IsStarted Then
                    Return Processing
                Else
                    Return Nothing
                End If
            End Function
        End Class

    End Class
End Namespace

