Namespace Terminal.xConsole

    Public NotInheritable Class CoolWriteSettings

        ''' <summary>
        ''' Gradual typing the output into console
        ''' </summary>
        Public Shared Property CoolWriting As Boolean = False

        ''' <summary>
        ''' Write speed
        ''' </summary>
        Public Shared Property CoolWritingDelay As Integer = 8

        ''' <summary>
        ''' Set the delay when write a new line or dots. (Default = 200).
        ''' </summary>
        Public Shared Property CWRDDelay As Integer = 280

        Private Sub New()
        End Sub
    End Class

End Namespace