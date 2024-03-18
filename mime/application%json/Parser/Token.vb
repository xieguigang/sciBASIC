Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Class Token : Inherits CodeToken(Of JSONElements)

    Public Enum JSONElements
        Invalid
        [Boolean]
        [Integer]
        [Double]
        [String]
        Open
        Close
        Colon
        Key
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

    Sub New(type As JSONElements, buffer As Char())
        Call MyBase.New(type, New String(buffer))
    End Sub

    Public Function GetValue() As JsonValue
        Select Case name
            Case JSONElements.String
                Return New JsonValue(text)
            Case JSONElements.Boolean
                Return New JsonValue(text.ParseBoolean)
            Case JSONElements.Double
                Return New JsonValue(Val(text))
            Case JSONElements.Integer
                Return New JsonValue(Long.Parse(text))
            Case Else
                Throw New InvalidCastException($"{name.Description} could not be cast to a literal value!")
        End Select
    End Function

    Public Function IsJsonValue() As Boolean
        Select Case name
            Case JSONElements.Boolean, JSONElements.Double, JSONElements.Integer, JSONElements.String
                Return True
            Case Else
                Return False
        End Select
    End Function

End Class
