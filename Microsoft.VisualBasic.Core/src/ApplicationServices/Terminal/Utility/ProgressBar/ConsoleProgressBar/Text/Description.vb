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
