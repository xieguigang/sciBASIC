Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Class Token : Inherits CodeToken(Of JSONElements)

    Public Enum JSONElements
        Invalid
        Literal
        [String]
        Open
        Close
        Colon
        Delimiter
        ''' <summary>
        ''' hjson comment
        ''' </summary>
        ''' <remarks>
        ''' just parse the hjson comment, this module will not save the
        ''' comment data when do json serialization
        ''' </remarks>
        Comment
    End Enum

    Sub New(type As JSONElements, text As String)
        Call MyBase.New(type, text)
    End Sub

End Class
