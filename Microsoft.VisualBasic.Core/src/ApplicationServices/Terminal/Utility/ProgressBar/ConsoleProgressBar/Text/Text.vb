' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Namespace iluvadev.ConsoleProgressBar
    ''' <summary>
    ''' Definitions for Texts in a ProgressBar
    ''' </summary>
    Public Partial Class Text
        ''' <summary>
        ''' Definition of the text in the same line as ProgressBar (Body)
        ''' </summary>
        Public ReadOnly Property Body As TextBody = New TextBody()

        ''' <summary>
        ''' Definition of the texts in the lines below a ProgressBar (Description)
        ''' </summary>
        Public ReadOnly Property Description As TextDescription = New TextDescription()
    End Class
End Namespace
