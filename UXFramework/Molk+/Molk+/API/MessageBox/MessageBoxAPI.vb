Public Module MessageBox

    Public Enum CYIcon
        [Error]
        Explorer
        Find
        Information
        Mail
        Media
        Print
        Question
        RecycleBinEmpty
        RecycleBinFull
        [Stop]
        User
        Warning
    End Enum

    Public Enum CYButtons
        AbortRetryIgnore
        OK
        OKCancel
        RetryCancel
        YesNo
        YesNoCancel
    End Enum

    ''' <summary>
    ''' Message: Text to display in the message box.
    ''' </summary>
    Public Function Show(Message As String) As DialogResult
        Return CYMessageBox.Show(Message)
    End Function

    ''' <summary>
    ''' Title: Text to display in the title bar of the messagebox.
    ''' </summary>
    Public Function Show(Message As String, Title As String) As DialogResult
        Return CYMessageBox.Show(Message, Title)
    End Function

    ''' <summary>
    ''' MButtons: Display CYButtons on the message box.
    ''' </summary>
    Public Function Show(Message As String, Title As String, MButtons As CYButtons) As DialogResult
        Return CYMessageBox.Show(Message, Title, MButtons)
    End Function

    ''' <summary>
    ''' MIcon: Display CYIcon on the message box.
    ''' </summary>
    Public Function Show(Message As String, Title As String, MButtons As CYButtons, MIcon As CYIcon) As DialogResult
        Return CYMessageBox.Show(Message, Title, MButtons, MIcon)
    End Function

End Module

