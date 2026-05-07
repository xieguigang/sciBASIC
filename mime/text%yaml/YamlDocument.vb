

Imports System.Data
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Public Class YamlDocument

    Dim yaml As String

    Private Sub New(yaml As String)
        Me.yaml = yaml
    End Sub

    Public Function GetDocument() As JsonElement
        Dim doc As Token() = New TokenIcer(yaml).GetTokens.ToArray
        Dim tokens As IEnumerator(Of Token) = doc.AsEnumerable.GetEnumerator

        If Not tokens.MoveNext Then
            ' empty collection 
            Return Nothing
        End If

        If tokens.Current.IsJsonValue Then
            Dim scalar As JsonValue = tokens.Current.GetValue

            If tokens.MoveNext Then
                Throw New InvalidExpressionException("the yaml literal value should be a scalar token value!")
            Else
                Return scalar
            End If
        Else
            Return PullYaml(tokens)
        End If
    End Function

    Private Function PullYaml(pull As IEnumerator(Of Token)) As JsonElement
        Dim t As Token = pull.Current

        If t Is Nothing Then
            Return Nothing
        End If

        Select Case t.name
            Case Token.JSONElements.Key
                Return PullObject(pull)
            Case Token.JSONElements.Serial
                Return PullArray(pull)
            Case Else
                If t.IsJsonValue Then
                    Return t.GetValue
                Else
                    Throw New InvalidProgramException($"invalid yaml syntax: the required token should be literal, object open or array open! (yaml_document_line: {t.span.line})")
                End If
        End Select
    End Function

    Private Function PullObject(pull As IEnumerator(Of Token)) As JsonObject
        Dim obj As New JsonObject
        Dim t As Token = pull.Current

        Return obj
    End Function

    Private Function PullArray(pull As IEnumerator(Of Token)) As JsonArray
        Dim array As New JsonArray
        Dim t As Token = pull.Current

        Return array
    End Function

    Public Shared Function Parse(yaml As String) As JsonElement
        Return New YamlDocument(yaml).GetDocument
    End Function
End Class
