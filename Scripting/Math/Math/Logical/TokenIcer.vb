Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

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
            {"<<", Logical.Tokens.Operator},
            {"<", Logical.Tokens.Operator},
            {"<=", Logical.Tokens.Operator},
            {">", Logical.Tokens.Operator},
            {"=>", Logical.Tokens.Operator},
            {">>", Logical.Tokens.Operator},
            {"~=", Logical.Tokens.Operator},
            {"=", Logical.Tokens.Operator},
            {"<>", Logical.Tokens.Operator},
            {vbTab, Logical.Tokens.WhiteSpace},
            {" ", Logical.Tokens.WhiteSpace}
        }

        Const OPERATORS As String = "AndOrNotXorNorNand"
        Const COMPARERS As String = "<<=>>~"

        <Extension> Private Function __parseUNDEFINE(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean

        End Function

        <Extension> Private Function __parseOperator(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean

        End Function

        <Extension> Private Function __parseComparer(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean

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

                If OPERATORS.IndexOf(ch) > -1 Then
                    Call __parseOperator(str, token)

                ElseIf COMPARERS.IndexOf(ch) > -1 Then
                    Call __parseComparer(str, token)

                Else
                    exitb = str.__parseUNDEFINE(token)
                    type = Mathematical.Tokens.UNDEFINE
                    tokens += New Token(Of Tokens)(type, New String(token))
                End If

                If Not exitb Then
                    Exit Do
                Else
                    token.Clear()
                End If
            Loop

            Return tokens
        End Function
    End Module
End Namespace