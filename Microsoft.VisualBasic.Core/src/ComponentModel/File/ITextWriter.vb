Imports System.IO

Namespace ComponentModel

    ''' <summary>
    ''' <see cref="TextWriter"/>
    ''' </summary>
    Public MustInherit Class ITextWriter

        Public MustOverride Sub Write(text As String)
        Public MustOverride Sub WriteLine(text As String)

    End Class
End Namespace