Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Module SyntaxParser

    ''' <summary>
    ''' 大小写敏感，在使用之前要先用tolower或者toupper
    ''' </summary>
    ''' <param name="term$"></param>
    ''' <param name="searchIn$"></param>
    ''' <returns></returns>
    Public Function ContainsAny(term$, searchIn$) As Boolean
        Dim t1$() = term.Split(ASCII.Symbols)  ' term
        Dim t2$() = term.Split(ASCII.Symbols)  ' 目标

        For Each t$ In t1$
            If t2.Located(t$) <> -1 Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Function MustContains(term$, searchIn$) As Boolean
        Return InStr(searchIn, term$, CompareMethod.Text) > 0
    End Function

    Const AND$ = "AND"
    Const OR$ = "OR"
    Const NOT$ = "NOT"

    Const op$ = [AND] & [OR] & [NOT]
    Const op_start$ = "AON"
    Const op_second$ = "NRO"

    Const stackOpen As Char = "("c
    Const stackClose As Char = ")"c

    ''' <summary>
    ''' 判断当前的栈是否是在提取操作符
    ''' </summary>
    ''' <param name="tmp"></param>
    ''' <returns></returns>
    Public Function IsOptr(tmp As List(Of Char), Optional ByRef type As Tokens = Tokens.AnyTerm) As Boolean
        Select Case tmp.Count
            Case 1
                Return op_start.Contains(tmp.First)
            Case 2
                Return is_Op2nd(a:=tmp(Scan0), b:=tmp(1), type:=type)
            Case 3
                If is_Op2nd(a:=tmp(0), b:=tmp(1), type:=type) Then
                    If type = Tokens.op_OR Then ' OR只有两个字符，在这里出现了第三个字符，则很明显不是
                        Return False
                    Else
                        Dim c As Char = tmp(2)

                        If type = Tokens.op_AND Then
                            Return c = "D"c
                        Else
                            Return c = "T"c
                        End If
                    End If
                End If
            Case Else
                Return False
        End Select

        Return False
    End Function

    Private Function is_Op2nd(a As Char, b As Char, Optional ByRef type As Tokens = Tokens.AnyTerm) As Boolean
        If a = "A"c AndAlso b = "N"c Then 'AND
            type = Tokens.op_AND
            Return True
        End If
        If a = "O"c AndAlso b = "R"c Then 'OR
            type = Tokens.op_OR
            Return True
        End If
        If a = "N"c AndAlso b = "O"c Then 'NOT
            type = Tokens.op_NOT
            Return True
        End If

        Return False
    End Function

    Public Function Parser(query$) As List(Of Token(Of Tokens))
        Dim tklist As New List(Of Token(Of Tokens))
        Dim quotOpen As Boolean
        Dim escape As Boolean
        Dim tmp As New List(Of Char)
        Dim getOp As Boolean
        Dim t As New Pointer(Of Char)(query$)

        Do While Not t.EndRead
            Dim c As Char = +t

            If c = ASCII.Quot Then
                If escape Then
                    tmp += c
                    escape = False
                Else
                    Dim isQuotClose As Boolean = quotOpen

                    quotOpen = Not quotOpen
                    tmp += c

                    If isQuotClose Then  ' 这里的这个双引号是结束符
                        tklist += New Token(Of Tokens)(Tokens.MustTerm, New String(tmp))
                        tmp.Clear()
                    End If
                End If
            ElseIf c = " "c OrElse c = ASCII.TAB Then
                If Not quotOpen AndAlso Not escape Then
                    tklist += New Token(Of Tokens)(Tokens.AnyTerm, New String(tmp))
                    tmp.Clear()
                Else
                    tmp += c
                End If
            Else
                tmp += c

                If quotOpen Then
                    If c = "\"c Then
                        escape = Not escape
                    Else
                        If escape Then
                            escape = Not escape
                        End If
                    End If
                    Continue Do
                End If

                If c = stackOpen Then
                    tklist += New Token(Of Tokens)(Tokens.stackOpen, "(")
                    tmp.Clear()
                    Continue Do
                ElseIf c = stackClose Then
                    tklist += New Token(Of Tokens)(Tokens.AnyTerm, New String(tmp))
                    tklist += New Token(Of Tokens)(Tokens.stackClose, ")")
                    tmp.Clear()
                    Continue Do
                End If

                If tmp.Count = 1 AndAlso op_start.IndexOf(c) > -1 Then
                    ' 可能是操作符的起始
                    getOp = True
                Else ' 第二个或者第三个
                    If getOp Then
                        Dim type As Tokens

                        If Not IsOptr(tmp, type) Then
                            getOp = False
                        End If

                        If type = Tokens.op_OR Then
                            getOp = False ' 因为提取到的已经是OR了，所以无论如何都要关闭提取

                            If t.Current = " "c Then
                                ' 是OR
                                tklist += New Token(Of Tokens)(Tokens.op_OR, "OR")
                                tmp.Clear()
                                t += 1

                                Continue Do
                            End If
                        ElseIf type = Tokens.op_AND OrElse type = Tokens.op_NOT Then
                            If tmp.Count = 3 Then
                                getOp = False ' 因为提取到3个字符了，所以无论如何都要关闭提取
                            End If

                            If t.Current = " "c Then
                                ' 是AND/NOT
                                tklist += New Token(Of Tokens)(type, type.ToString)
                                tmp.Clear()
                                t += 1

                                Continue Do
                            End If
                        End If
                    End If
                End If
            End If
        Loop

        tklist += New Token(Of Tokens)(Tokens.AnyTerm, New String(tmp))
        tklist = New List(Of Token(Of Tokens))(tklist.Where(Function(x) Not String.IsNullOrEmpty(x.Text)))

        Return tklist
    End Function

    Public Enum Tokens
        AnyTerm
        MustTerm
        Field
        op_AND
        op_NOT
        op_OR
        stackOpen
        stackClose
    End Enum
End Module
