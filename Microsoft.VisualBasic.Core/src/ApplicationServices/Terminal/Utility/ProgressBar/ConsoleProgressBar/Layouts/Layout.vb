#Region "Microsoft.VisualBasic::1f85cb386578879efbc5487c4de0e26c, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Layouts\Layout.vb"

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

    '   Total Lines: 151
    '    Code Lines: 66 (43.71%)
    ' Comment Lines: 53 (35.10%)
    '    - Xml Docs: 45.28%
    ' 
    '   Blank Lines: 32 (21.19%)
    '     File Size: 9.07 KB


    '     Class Layout
    ' 
    '         Properties: Body, Margins, Marquee, ProgressBarWidth
    ' 
    '         Function: GetInnerWidth, GetRenderActions
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
Imports std = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
    ''' <summary>
    ''' Definition of a Layout for a ProgressBar representation
    ''' </summary>
    Partial Public Class Layout
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
        ''' Layout definition for Margins
        ''' </summary>
        Public ReadOnly Property Margins As LayoutMargin = New LayoutMargin()

        ''' <summary>
        ''' Layout definition for Marquee (character moving around the ProgressBar)
        ''' </summary>
        Public ReadOnly Property Marquee As LayoutMarquee = New LayoutMarquee()

        ''' <summary>
        ''' Layout definition for Body
        ''' </summary>
        Public ReadOnly Property Body As LayoutBody = New LayoutBody()


        ''' <summary>
        ''' Width of entire ProgressBar
        ''' Default = 30
        ''' </summary>
        Public Property ProgressBarWidth As Integer = 30

        ''' <summary>
        ''' Gets the internal Width of the ProgressBar
        ''' </summary>
        Public Function GetInnerWidth(progressBar As ProgressBar) As Integer
            Return std.Max(ProgressBarWidth - Margins.GetLength(progressBar), 0)
        End Function

        ''' <summary>
        ''' Returns the Actions to do in order to Render the ProgressBar with this Layout
        ''' </summary>
        ''' <param name="progressBar"></param>
        ''' <returns></returns>
        Friend Function GetRenderActions(progressBar As ProgressBar) As List(Of Action)
            '  [■■■■■■■■■■■■········] -> Without Marquee
            '  [■■■■■■■■■■■■····+···] -> Marquee over Pending space
            '  [■■■■■■■■#■■■········] -> Marquee over Progress space
            '  [·····+··············] -> Marquee withot progress
            Dim list = New List(Of Action)()

            Dim innerWidth = GetInnerWidth(progressBar)
            Dim progressLenght = If(progressBar.HasProgress, Convert.ToInt32(progressBar.Percentage / (100.0F / innerWidth)), 0)
            Dim pendingLenght = innerWidth - progressLenght

            Dim marqueeInProgress = progressBar.HasProgress AndAlso progressBar.MarqueePosition >= 0 AndAlso progressBar.MarqueePosition < progressLenght AndAlso Marquee.OverProgress.GetVisible(progressBar)
            Dim marqueeInPending = progressBar.MarqueePosition >= progressLenght AndAlso Marquee.OverPending.GetVisible(progressBar)

            Dim progressBeforeMarqueeLength = progressLenght
            If marqueeInProgress Then progressBeforeMarqueeLength = progressBar.MarqueePosition

            Dim progressAfterMarqueeLength = 0
            If marqueeInProgress Then progressAfterMarqueeLength = progressLenght - progressBeforeMarqueeLength - 1

            Dim pendingBeforeMarqueeLength = pendingLenght
            If marqueeInPending Then pendingBeforeMarqueeLength = progressBar.MarqueePosition - progressLenght

            Dim pendingAfterMarqueeLength = 0
            If marqueeInPending Then pendingAfterMarqueeLength = pendingLenght - pendingBeforeMarqueeLength - 1

            Dim innerText = ""
            If Body.Text.GetVisible(progressBar) Then innerText = If(Body.Text.GetValue(progressBar), "").AdaptToMaxWidth(innerWidth)

            Dim textProgressBeforeMarquee As String = If(String.IsNullOrEmpty(innerText), New String(Body.Progress.GetValue(progressBar), progressBeforeMarqueeLength), innerText.Substring(0, progressBeforeMarqueeLength))

            Dim charProgressMarquee As Char? = Nothing
            If marqueeInProgress Then charProgressMarquee = If(String.IsNullOrEmpty(innerText), Marquee.OverProgress.GetValue(progressBar), innerText(progressBar.MarqueePosition))

            Dim textProgressAfterMarquee As String = If(String.IsNullOrEmpty(innerText), New String(Body.Progress.GetValue(progressBar), progressAfterMarqueeLength), innerText.Substring(progressBar.MarqueePosition + 1, progressAfterMarqueeLength))

            Dim textPendingBeforeMarquee As String = If(String.IsNullOrEmpty(innerText), New String(Body.Pending.GetValue(progressBar), pendingBeforeMarqueeLength), innerText.Substring(progressLenght, pendingBeforeMarqueeLength))

            Dim charPendingMarquee As Char? = Nothing
            If marqueeInPending Then charPendingMarquee = If(String.IsNullOrEmpty(innerText), Marquee.OverPending.GetValue(progressBar), innerText(progressBar.MarqueePosition))

            Dim textPendingAfterMarquee As String = If(String.IsNullOrEmpty(innerText), New String(Body.Pending.GetValue(progressBar), pendingAfterMarqueeLength), innerText.Substring(progressBar.MarqueePosition + 1, pendingAfterMarqueeLength))

            'Margin: Start
            list.AddRange(Margins.Start.GetRenderActions(progressBar))

            'Body: Progress before Marquee
            If Not String.IsNullOrEmpty(textProgressBeforeMarquee) Then
                Dim elementProgressBeforeMarquee = New Element(Of String)(textProgressBeforeMarquee, If(Body.Progress.GetForegroundColor(progressBar), Console.ForegroundColor), If(Body.Progress.GetBackgroundColor(progressBar), Console.BackgroundColor))
                list.AddRange(elementProgressBeforeMarquee.GetRenderActions(progressBar))
            End If

            'Body: Marquee in progress
            If charProgressMarquee.HasValue Then
                Dim elementProgressMarquee = New Element(Of Char)(charProgressMarquee.Value, If(Marquee.OverProgress.GetForegroundColor(progressBar), Console.ForegroundColor), If(Marquee.OverProgress.GetBackgroundColor(progressBar), Console.BackgroundColor))
                list.AddRange(elementProgressMarquee.GetRenderActions(progressBar))
            End If

            'Body: Progress after Marquee
            If Not String.IsNullOrEmpty(textProgressAfterMarquee) Then
                Dim elementProgressAfterMarquee = New Element(Of String)(textProgressAfterMarquee, If(Body.Progress.GetForegroundColor(progressBar), Console.ForegroundColor), If(Body.Progress.GetBackgroundColor(progressBar), Console.BackgroundColor))
                list.AddRange(elementProgressAfterMarquee.GetRenderActions(progressBar))
            End If

            'Body: Pending before Marquee
            If Not String.IsNullOrEmpty(textPendingBeforeMarquee) Then
                Dim elementPendingBeforeMarquee = New Element(Of String)(textPendingBeforeMarquee, If(Body.Pending.GetForegroundColor(progressBar), Console.ForegroundColor), If(Body.Pending.GetBackgroundColor(progressBar), Console.BackgroundColor))
                list.AddRange(elementPendingBeforeMarquee.GetRenderActions(progressBar))
            End If

            'Body: Marquee in Pending
            If charPendingMarquee.HasValue Then
                Dim elementPendingMarquee = New Element(Of Char)(charPendingMarquee.Value, If(Marquee.OverPending.GetForegroundColor(progressBar), Console.ForegroundColor), If(Marquee.OverPending.GetBackgroundColor(progressBar), Console.BackgroundColor))
                list.AddRange(elementPendingMarquee.GetRenderActions(progressBar))
            End If

            'Body: Pending after Marquee
            If Not String.IsNullOrEmpty(textPendingAfterMarquee) Then
                Dim elementPendingAfterMarquee = New Element(Of String)(textPendingAfterMarquee, If(Body.Pending.GetForegroundColor(progressBar), Console.ForegroundColor), If(Body.Pending.GetBackgroundColor(progressBar), Console.BackgroundColor))
                list.AddRange(elementPendingAfterMarquee.GetRenderActions(progressBar))
            End If

            'Margin: End
            list.AddRange(Margins.End.GetRenderActions(progressBar))

            Return list
        End Function
    End Class
End Namespace
