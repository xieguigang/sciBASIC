Imports System.Text

Namespace VBProj.CodeDOM.Syntax

    ' ------------------------------------------------------------------
    ' statement cursor : skips leading attributes and modifiers
    ' ------------------------------------------------------------------
    Friend Class StmtParser

        Public Tokens As List(Of Token)
        Public Pos As Integer
        Public Attributes As New List(Of String)
        Public Modifiers As String = ""

        Public Sub New(tk As List(Of Token), Optional p As Integer = 0)
            Tokens = tk
            Pos = p
        End Sub

        Public ReadOnly Property Eof As Boolean
            Get
                Return Pos >= Tokens.Count
            End Get
        End Property

        Public ReadOnly Property Current As Token
            Get
                If Eof Then
                    Return New Token With {.Kind = TokenKind.Punctuation, .Text = ""}
                End If
                Return Tokens(Pos)
            End Get
        End Property

        Public Sub CollectLeading()
            Do
                If Not Eof AndAlso Current.Text = "<"c Then
                    Attributes.Add(ReadAttributeBlock())
                ElseIf Not Eof AndAlso IsModifier(Current.Text.ToLowerInvariant()) Then
                    If Modifiers.Length > 0 Then
                        Modifiers &= " "
                    End If
                    Modifiers &= Current.Text
                    Pos += 1
                Else
                    Exit Do
                End If
            Loop
        End Sub

        Public Function ReadAttributeBlock() As String
            ' Current is "<"
            Pos += 1
            Dim sb As New StringBuilder()
            Dim depth As Integer = 0

            While Not Eof
                Dim tk As Token = Current
                If tk.Text = "("c Then
                    depth += 1
                    sb.Append(tk.Text)
                    Pos += 1
                ElseIf tk.Text = ")"c Then
                    depth -= 1
                    sb.Append(tk.Text)
                    Pos += 1
                ElseIf tk.Text = ">"c AndAlso depth = 0 Then
                    Pos += 1
                    Exit While
                Else
                    sb.Append(tk.Text)
                    Pos += 1
                End If
            End While

            Return sb.ToString().Trim()
        End Function
    End Class

End Namespace