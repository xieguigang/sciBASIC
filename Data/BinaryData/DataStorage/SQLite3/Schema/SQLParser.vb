Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text.Parser

Namespace ManagedSqlite.Core

    Public Enum TokenTypes
        name
        type
        keyword
        open
        close
    End Enum

    Public Class Token : Inherits CodeToken(Of TokenTypes)
    End Class

    Public Class SQLParser

        Dim sql As CharPtr
        ''' <summary>
        ''' 主要是针对name进行escape
        ''' </summary>
        Dim escape As Boolean
        Dim escapeChar As Char

        Sub New(sql As String)
            Me.sql = sql
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim token As New Value(Of Token)

            Do While Not sql
                If Not token = walkChar(++sql) Is Nothing Then
                    Yield token
                End If
            Loop
        End Function

        Private Function walkChar(c As Char) As Token
            If c = """"c Then
                If escape Then

                Else

                End If
            ElseIf c = "["c Then
                If escape Then

                Else

                End If
            End If
        End Function
    End Class
End Namespace