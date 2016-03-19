Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Linq

Namespace Logical

    Public Enum Tokens
        UNDEFINE
        ''' <summary>
        ''' And Or Not Xor Nor Nand
        ''' </summary>
        [Operator]
        ''' <summary>
        ''' &lt;&lt;, &lt;, &lt;=, >, =>, >>, ~=, =, &lt;>
        ''' </summary>
        Comparer
        ''' <summary>
        ''' Space or VbTab
        ''' </summary>
        WhiteSpace
    End Enum

    Public Module TokenIcer

        Public ReadOnly Property Tokens As IReadOnlyDictionary(Of String, Tokens) =
            New Dictionary(Of String, Tokens) From {
 _
            {"and", Logical.Tokens.Operator},
            {"or", Logical.Tokens.Operator},
            {"not", Logical.Tokens.Operator},
            {"xor", Logical.Tokens.Operator},
            {"nor", Logical.Tokens.Operator},
            {"nand", Logical.Tokens.Operator},
            {"<<", Logical.Tokens.Comparer},
            {"<", Logical.Tokens.Comparer},
            {"<=", Logical.Tokens.Comparer},
            {">", Logical.Tokens.Comparer},
            {"=>", Logical.Tokens.Comparer},
            {">>", Logical.Tokens.Comparer},
            {"~=", Logical.Tokens.Comparer},
            {"=", Logical.Tokens.Comparer},
            {"<>", Logical.Tokens.Comparer},
            {vbTab, Logical.Tokens.WhiteSpace},
            {" ", Logical.Tokens.WhiteSpace}
        }

        Const OPERATORS As String = "AndOrNotxXorNorNand"
        Const COMPARERS As String = "<<=>>~"

        <Extension> Private Function __parseUNDEFINE(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If OPERATORS.IndexOf(str.Current) = -1 AndAlso  COMPARERS.IndexOf(str.Current) = -1 Then
                    Call token.Add(str.Current)
                Else
                    Return True
                End If
            Loop

            Return False
        End Function

        <Extension> Private Function __parseOperator(str As CharEnumerator, ByRef last As Char, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If OPERATORS.IndexOf(str.Current) = -1 Then
                    last = str.Current
                    Return True
                Else
                    Call token.Add(str.Current)
                End If
            Loop

            Return False
        End Function

        <Extension> Private Function __parseComparer(str As CharEnumerator, ByRef last As Char, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If COMPARERS.IndexOf(str.Current) = -1 Then
                    last = str.Current
                    Return True
                Else
                    Call token.Add(str.Current)
                End If
            Loop

            Return False
        End Function

        Public Function TryParse(s As String) As List(Of Token(Of Tokens))
            Dim str As CharEnumerator = s.GetEnumerator
            Dim tokens As New List(Of Token(Of Tokens))
            Dim ch As Char
            Dim token As New List(Of Char)
            Dim type As Tokens = Logical.Tokens.UNDEFINE
            Dim exitb As Boolean = False

            If Not str.MoveNext() Then  ' Empty expression
                Return New List(Of Token(Of Tokens))
            End If

            Do While True
                ch = str.Current
                token += ch
CONTINUTES:
                If OPERATORS.IndexOf(ch) > -1 Then
                    Call __parseOperator(str, ch, token)
                    type = Logical.Tokens.Operator

                ElseIf COMPARERS.IndexOf(ch) > -1 Then
                    Call __parseComparer(str, ch, token)
                    type = Logical.Tokens.Comparer

                Else
                    exitb = str.__parseUNDEFINE(token)
                    type = Logical.Tokens.UNDEFINE
                    tokens += New Token(Of Tokens)(type, New String(token))
                End If

                If type <> Logical.Tokens.UNDEFINE Then
                    Dim st As String = New String(token).ToLower

                    If TokenIcer.Tokens.ContainsKey(st) Then
                        type = TokenIcer.Tokens(st)

                        If ch.IsWhiteSpace AndAlso type = Logical.Tokens.Operator Then
                            tokens += New Token(Of Tokens)(type, st)
                        ElseIf type = Logical.Tokens.Comparer Then
                            tokens += New Token(Of Tokens)(type, st)
                            Call token.Clear()
                            token += ch
                            GoTo CONTINUTES
                        Else
                            GoTo UNDEFINE
                        End If
                    Else
UNDEFINE:               If Not ch.IsWhiteSpace Then token += ch
                        exitb = str.__parseUNDEFINE(token)
                        type = Logical.Tokens.UNDEFINE
                        tokens += New Token(Of Tokens)(type, New String(token))
                    End If
                End If

                If Not exitb Then
                    Exit Do
                Else
                    token.Clear()
                End If
            Loop

            Return tokens.Removes(AddressOf IsWhiteSpace)
        End Function

        <Extension> Public Function Split(source As IEnumerable(Of Token(Of Tokens))) As List(Of MetaExpression(Of Token(Of Tokens)(), Token(Of Tokens)))
            Dim lst As New List(Of MetaExpression(Of Token(Of Tokens)(), Token(Of Tokens)))
            Return lst
        End Function

        <Extension> Public Function IsWhiteSpace(x As Token(Of Tokens)) As Boolean
            Return x.Type = Logical.Tokens.WhiteSpace OrElse
                (x.Type = Logical.Tokens.UNDEFINE AndAlso String.IsNullOrWhiteSpace(x.Text))
        End Function

        <Extension> Public Function IsWhiteSpace(ch As Char) As Boolean
            Dim s As String = CStr(ch)

            If Not Tokens.ContainsKey(s) Then
                Return False
            Else
                Return Tokens(s) = Logical.Tokens.WhiteSpace
            End If
        End Function
    End Module
End Namespace