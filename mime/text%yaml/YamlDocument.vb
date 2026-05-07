

Imports System.Data
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Public Class YamlDocument

    Dim yaml As String

    Private Sub New(yaml As String)
        Me.yaml = yaml
    End Sub

    Public Function GetDocument() As JsonElement
        Dim tokens As IEnumerator(Of Token) = New TokenIcer(yaml).GetTokens.GetEnumerator

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


    End Function

    Public Shared Function Parse(yaml As String) As JsonElement
        Return New YamlDocument(yaml).GetDocument
    End Function
End Class
