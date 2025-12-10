#Region "Microsoft.VisualBasic::068b12194bdd47408192e7d659c846f4, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Text\Description.vb"

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

    '   Total Lines: 83
    '    Code Lines: 39 (46.99%)
    ' Comment Lines: 34 (40.96%)
    '    - Xml Docs: 79.41%
    ' 
    '   Blank Lines: 10 (12.05%)
    '     File Size: 3.69 KB


    '     Class Text
    ' 
    ' 
    '         Class TextDescription
    ' 
    '             Properties: Done, Indentation, Paused, Processing
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: Clear, FinishedText, GetCurrentDefinitionList
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
        ''' Definition for the Description lines in a ProgressBar
        ''' </summary>
        Public Class TextDescription
            ''' <summary>
            ''' Description lines definition when ProgressBar is "Processing"
            ''' </summary>
            Public ReadOnly Property Processing As ElementList(Of String) = New ElementList(Of String)()

            ''' <summary>
            ''' Description lines definition when ProgressBar is "Paused"
            ''' </summary>
            Public ReadOnly Property Paused As ElementList(Of String) = New ElementList(Of String)()

            ''' <summary>
            ''' Description lines definition when ProgressBar is "Done"
            ''' </summary>
            Public ReadOnly Property Done As ElementList(Of String) = New ElementList(Of String)()

            ''' <summary>
            ''' Indentation for Description lines
            ''' </summary>
            Public ReadOnly Property Indentation As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Ctor
            ''' </summary>
            Public Sub New()
                Processing.AddNew().SetValue(Function(pb) pb.ElementName).SetVisible(Function(pb) Not String.IsNullOrEmpty(pb.ElementName)).SetForegroundColor(ConsoleColor.DarkYellow)
                Paused.AddNew().SetValue("[Paused]").SetForegroundColor(ConsoleColor.DarkCyan)
                Done.AddNew().SetValue(AddressOf FinishedText).SetForegroundColor(ConsoleColor.DarkGray)
                Indentation.SetValue("  -> ").SetForegroundColor(ConsoleColor.DarkBlue)
            End Sub

            Private Function FinishedText(pb As ProgressBar) As String
                Return $"{pb.Value} in {pb.TimeProcessing.ToStringWithAllHours()} ({pb.TimePerElement.ToStringWithAllHours()} each one)"
            End Function

            ''' <summary>
            ''' Clears Description Lines
            ''' </summary>
            ''' <returns></returns>
            Public Function Clear() As TextDescription
                Processing.Clear()
                Paused.Clear()
                Done.Clear()
                Return Me
            End Function

            ''' <summary>
            ''' Gets the current Description Lines definition by the ProgressBar context ("Processing", "Paused" or "Done")
            ''' </summary>
            ''' <param name="progressBar"></param>
            ''' <returns></returns>
            Public Function GetCurrentDefinitionList(progressBar As ProgressBar) As ElementList(Of String)
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
