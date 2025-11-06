' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
    ''' <summary>
    ''' A list of Elements of a ProgressBar
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class ElementList(Of T)
        ''' <summary>
        ''' The List of Elements of a Progressbar
        ''' </summary>
        Public ReadOnly Property List As New List(Of Element(Of T))()

        ''' <summary>
        ''' Clears the List of elements of a ProgressBar
        ''' </summary>
        ''' <returns></returns>
        Public Function Clear() As ElementList(Of T)
            List.Clear()
            Return Me
        End Function

        ''' <summary>
        ''' Adds new Element to the List
        ''' </summary>
        ''' <returns></returns>
        Public Function AddNew() As Element(Of T)
            Dim line = New Element(Of T)()
            List.Add(line)
            Return line
        End Function
    End Class

End Namespace
