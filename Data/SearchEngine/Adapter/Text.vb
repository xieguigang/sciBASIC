Public Structure Text

    ''' <summary>
    ''' The string value.
    ''' </summary>
    ''' <returns></returns>
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Structure