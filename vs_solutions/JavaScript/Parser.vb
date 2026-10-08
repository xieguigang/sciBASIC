Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime


''' <summary>
''' Recursive-descent parser: token stream → <see cref="Program"/> AST.
''' <para>Operator precedence (low→high): assignment, ternary, ||, &amp;&amp;,
''' equality, relational, additive, multiplicative, unary, postfix,
''' call/member, primary.</para>
''' <para>Automatic-semicolon-insertion: a missing ';' is accepted before a
''' line break, '}' or EOF. Arrow functions are parsed with backtracking.</para>
''' </summary>
Public NotInheritable Class Parser

    Private ReadOnly _tokens As List(Of Token)
    Private _pos As Integer
    Private _loopDepth As Integer
    Private _funcDepth As Integer

    Private Sub New(tokens As List(Of Token))
        _tokens = tokens
    End Sub

    Public Shared Function Parse(source As String) As Program
        Return New Parser(Lexer.Lex(source)).ParseProgram()
    End Function

    ' ---------------- token helpers ----------------

    Private ReadOnly Property Cur As Token
        Get
            Return _tokens(_pos)
        End Get
    End Property

    Private ReadOnly Property Prev As Token
        Get
            Return _tokens(_pos - 1)
        End Get
    End Property

    Private Function At(text As String) As Boolean
        Return Cur.name = TokenType.Punct AndAlso Cur.text = text
    End Function

    Private Function AtKeyword(word As String) As Boolean
        Return Cur.name = TokenType.Keyword AndAlso Cur.text = word
    End Function

    Private Function Match(text As String) As Boolean
        If At(text) Then
            _pos += 1
            Return True
        End If
        Return False
    End Function

    Private Function MatchKeyword(word As String) As Boolean
        If AtKeyword(word) Then
            _pos += 1
            Return True
        End If
        Return False
    End Function

    Private Function Eat(text As String) As Token
        If Not At(text) Then
            Throw New ParseException($"expected '{text}' but found '{Cur.text}' at {Cur.Location}")
        End If
        Dim t = Cur
        _pos += 1
        Return t
    End Function

    Private Sub EatKeyword(word As String)
        If Not AtKeyword(word) Then
            Throw New ParseException($"expected '{word}' but found '{Cur.text}' at {Cur.Location}")
        End If
        _pos += 1
    End Sub

    Private Function ExpectIdentifier() As String
        If Cur.name <> TokenType.Identifier Then
            Throw New ParseException($"expected identifier but found '{Cur.text}' at {Cur.Location}")
        End If
        Dim name = Cur.Text
        _pos += 1
        Return name
    End Function

    ''' <summary>Consume ';' or accept ASI (line break / '}' / EOF before current token).</summary>
    Private Sub ConsumeStatementEnd()
        If Match(";") Then Return
        If At("}") OrElse Cur.name = TokenType.Eof OrElse Cur.LineBreakBefore Then Return
        Throw New ParseException($"expected ';' before '{Cur.text}' at {Cur.Location}")
    End Sub

    ' ---------------- program / statements ----------------

    Private Function ParseProgram() As Program
        Dim body As New List(Of Statement)
        While Cur.name <> TokenType.Eof
            body.Add(ParseStatement())
        End While
        Return New Program(body)
    End Function

    Private Function ParseStatement() As Statement
        If At(";") Then
            _pos += 1
            Return New EmptyStmt()
        End If
        If At("{") Then
            ' NOTE: a block does NOT reset loop context — `for(;;){continue}` is legal JS;
            ' only a function body does (handled in ParseFunctionRest).
            Dim b = ParseBlock()
            Return New BlockStmt(b)
        End If
        If Cur.name = TokenType.Keyword Then
            Select Case Cur.text
                Case "var", "let", "const" : Dim s = ParseVar() : ConsumeStatementEnd() : Return s
                Case "function" : Return ParseFunctionDecl()
                Case "if" : Return ParseIf()
                Case "for" : Return ParseFor()
                Case "while" : Return ParseWhile()
                Case "do" : Return ParseDoWhile()
                Case "break"
                    _pos += 1
                    If _loopDepth = 0 Then Throw New ParseException("'break' outside of a loop")
                    ConsumeStatementEnd()
                    Return New BreakStmt()
                Case "continue"
                    _pos += 1
                    If _loopDepth = 0 Then Throw New ParseException("'continue' outside of a loop")
                    ConsumeStatementEnd()
                    Return New ContinueStmt()
                Case "return"
                    _pos += 1
                    If _funcDepth = 0 Then Throw New ParseException("'return' outside of a function")
                    Dim value As Expression = Nothing
                    ' ASI: return [linebreak] → value-less return
                    If Not At(";") AndAlso Not At("}") AndAlso Cur.name <> TokenType.Eof AndAlso
                       Not Cur.LineBreakBefore Then
                        value = ParseExpression()
                    End If
                    ConsumeStatementEnd()
                    Return New ReturnStmt(value)
                Case "throw"
                    _pos += 1
                    If Cur.LineBreakBefore Then Throw New ParseException("illegal newline after 'throw'")
                    Dim v = ParseExpression()
                    ConsumeStatementEnd()
                    Return New ThrowStmt(v)
                Case "try" : Return ParseTry()
                Case "this"
                    Throw New ParseException("'this' is not supported by this parser (line " & Cur.span.line & ")")
                Case "new"
                    Throw New ParseException("'new' is not supported by this parser (line " & Cur.span.line & ")")
            End Select
        End If
        Dim expr = ParseExpression()
        ConsumeStatementEnd()
        Return New ExprStmt(expr)
    End Function

    Private Function ParseBlock() As List(Of Statement)
        Eat("{")
        Dim body As New List(Of Statement)
        While Not At("}")
            If Cur.name = TokenType.Eof Then Throw New ParseException("missing '}' — unexpected end of input")
            body.Add(ParseStatement())
        End While
        Eat("}")
        Return body
    End Function

    Private Function ParseVar() As VarStmt
        Dim kind = Cur.Text
        _pos += 1
        Dim decls As New List(Of VarDeclarant)
        Do
            Dim name = ExpectIdentifier()
            Dim init As Expression = Nothing
            If Match("=") Then init = ParseAssignment()
            decls.Add(New VarDeclarant(name, init))
        Loop While Match(",")
        Return New VarStmt(kind, decls)
    End Function

    Private Function ParseFunctionDecl() As Statement
        EatKeyword("function")
        Dim name = ExpectIdentifier()
        Dim f = ParseFunctionRest(name)
        Return New FuncDeclStmt(name, f.Parameters, f.Body)
    End Function

    ''' <summary>Parse "(params) { body }" (function keyword already consumed).</summary>
    Private Function ParseFunctionRest(name As String) As FuncExpr
        Eat("(")
        Dim parameters As New List(Of String)
        If Not At(")") Then
            Do
                parameters.Add(ExpectIdentifier())
            Loop While Match(",")
        End If
        Eat(")")
        Dim outerFunc = _funcDepth
        Dim outerLoop = _loopDepth
        _funcDepth += 1
        _loopDepth = 0
        Dim body = ParseBlock()
        _funcDepth = outerFunc
        _loopDepth = outerLoop
        Return New FuncExpr(name, parameters.ToArray(), body)
    End Function

    Private Function ParseIf() As Statement
        EatKeyword("if")
        Eat("(")
        Dim test = ParseExpression()
        Eat(")")
        Dim thenBranch = ParseStatement()
        Dim elseBranch As Statement = Nothing
        If MatchKeyword("else") Then
            elseBranch = If(AtKeyword("if"), ParseIf(), ParseStatement())
        End If
        Return New IfStmt(test, thenBranch, elseBranch)
    End Function

    Private Function ParseFor() As Statement
        EatKeyword("for")
        Eat("(")

        Dim initVar As VarStmt = Nothing
        Dim initExpr As Expression = Nothing
        If At(";") Then
            _pos += 1
        ElseIf AtKeyword("var") OrElse AtKeyword("let") OrElse AtKeyword("const") Then
            initVar = ParseVar()
            If AtKeyword("in") Then
                ' for (var k in obj)
                If initVar.Declarations.Count <> 1 OrElse initVar.Declarations(0).Init IsNot Nothing Then
                    Throw New ParseException("for-in header must declare a single uninitialised variable")
                End If
                _pos += 1
                Dim obj = ParseExpression()
                Eat(")")
                Dim body = ParseLoopBody()
                Return New ForInStmt(initVar.Declarations(0).Name, obj, body)
            End If
            Eat(";")
        Else
            initExpr = ParseExpression()
            If AtKeyword("in") Then
                _pos += 1
                Dim obj = ParseExpression()
                Eat(")")
                Dim body = ParseLoopBody()
                If TypeOf initExpr IsNot IdentExpr Then
                    Throw New ParseException("for-in target must be an identifier")
                End If
                Return New ForInStmt(DirectCast(initExpr, IdentExpr).Name, obj, body)
            End If
            Eat(";")
        End If

        Dim test As Expression = Nothing
        If Not At(";") Then test = ParseExpression()
        Eat(";")
        Dim update As Expression = Nothing
        If Not At(")") Then update = ParseExpression()
        Eat(")")
        Dim body2 = ParseLoopBody()
        Return New ForStmt(If(initVar, CType(initExpr, Object)), test, update, body2)
    End Function

    Private Function ParseLoopBody() As Statement
        _loopDepth += 1
        Dim body = ParseStatement()
        _loopDepth -= 1
        Return body
    End Function

    Private Function ParseWhile() As Statement
        EatKeyword("while")
        Eat("(")
        Dim test = ParseExpression()
        Eat(")")
        Dim body = ParseLoopBody()
        Return New WhileStmt(test, body)
    End Function

    Private Function ParseDoWhile() As Statement
        EatKeyword("do")
        Dim body = ParseLoopBody()
        EatKeyword("while")
        Eat("(")
        Dim test = ParseExpression()
        Eat(")")
        ConsumeStatementEnd()
        Return New DoWhileStmt(body, test)
    End Function

    Private Function ParseTry() As Statement
        EatKeyword("try")
        Dim tryBlock = ParseBlock()
        Dim catchParam As String = Nothing
        Dim catchBlock As List(Of Statement) = Nothing
        Dim finallyBlock As List(Of Statement) = Nothing
        If MatchKeyword("catch") Then
            If Match("(") Then
                catchParam = ExpectIdentifier()
                Eat(")")
            End If
            catchBlock = ParseBlock()
        End If
        If MatchKeyword("finally") Then
            finallyBlock = ParseBlock()
        End If
        If catchBlock Is Nothing AndAlso finallyBlock Is Nothing Then
            Throw New ParseException("try statement requires a catch or finally block")
        End If
        Return New TryStmt(tryBlock, catchParam, catchBlock, finallyBlock)
    End Function

    ' ---------------- expressions (precedence climbing) ----------------

    Private Function ParseExpression() As Expression
        Return ParseAssignment()
    End Function

    Private Function ParseAssignment() As Expression
        ' arrow function: single identifier followed by "=>" on the same line
        If Cur.name = TokenType.Identifier AndAlso _pos + 1 < _tokens.Count Then
            Dim nxt = _tokens(_pos + 1)
            If nxt.name = TokenType.Punct AndAlso nxt.text = "=>" AndAlso Not nxt.LineBreakBefore Then
                Dim pname = Cur.text
                _pos += 2
                Return ParseArrowBody({pname})
            End If
        End If
        ' arrow function: "(a, b) =>"
        If At("(") Then
            Dim save = _pos
            Dim ok = True
            _pos += 1
            Dim parameters As New List(Of String)
            If At(")") Then
                _pos += 1
            Else
                Do
                    If Cur.name <> TokenType.Identifier Then ok = False : Exit Do
                    parameters.Add(Cur.Text)
                    _pos += 1
                    If At(",") Then
                        _pos += 1
                    ElseIf At(")") Then
                        _pos += 1
                        Exit Do
                    Else
                        ok = False : Exit Do
                    End If
                Loop
            End If
            If ok AndAlso At("=>") AndAlso Not Cur.LineBreakBefore Then
                _pos += 1
                Return ParseArrowBody(parameters.ToArray())
            End If
            _pos = save   ' backtrack: it is a parenthesised expression / call
        End If

        Dim left = ParseConditional()
        If Cur.name = TokenType.Punct AndAlso
           (Cur.text = "=" OrElse Cur.text = "+=" OrElse Cur.text = "-=" OrElse
            Cur.text = "*=" OrElse Cur.text = "/=" OrElse Cur.text = "%=") Then
            Dim op = Cur.text
            _pos += 1
            Dim right = ParseAssignment()
            If Not (TypeOf left Is IdentExpr OrElse TypeOf left Is MemberExpr) Then
                Throw New ParseException("invalid assignment target")
            End If
            Return New AssignExpr(op, left, right)
        End If
        Return left
    End Function

    Private Function ParseArrowBody(parameters As String()) As FuncExpr
        If At("{") Then
            Dim outerFunc = _funcDepth, outerLoop = _loopDepth
            _funcDepth += 1
            _loopDepth = 0
            Dim body = ParseBlock()
            _funcDepth = outerFunc
            _loopDepth = outerLoop
            Return New FuncExpr(Nothing, parameters, body)
        End If
        ' expression body → { return expr; }
        Dim expr = ParseAssignment()
        Dim body2 As New List(Of Statement) From {New ReturnStmt(expr)}
        Return New FuncExpr(Nothing, parameters, body2)
    End Function

    Private Function ParseConditional() As Expression
        Dim test = ParseLogicalOr()
        If Match("?") Then
            Dim consequent = ParseAssignment()
            Eat(":")
            Dim alternate = ParseAssignment()
            Return New CondExpr(test, consequent, alternate)
        End If
        Return test
    End Function

    Private Function ParseLogicalOr() As Expression
        Dim left = ParseLogicalAnd()
        While At("||")
            _pos += 1
            left = New LogicalExpr("||", left, ParseLogicalAnd())
        End While
        Return left
    End Function

    Private Function ParseLogicalAnd() As Expression
        Dim left = ParseEquality()
        While At("&&")
            _pos += 1
            left = New LogicalExpr("&&", left, ParseEquality())
        End While
        Return left
    End Function

    Private Function ParseEquality() As Expression
        Dim left = ParseRelational()
        While Cur.name = TokenType.Punct AndAlso
              (Cur.text = "==" OrElse Cur.text = "!=" OrElse Cur.text = "===" OrElse Cur.text = "!==")
            Dim op = Cur.Text
            _pos += 1
            left = New BinaryExpr(op, left, ParseRelational())
        End While
        Return left
    End Function

    Private Function ParseRelational() As Expression
        Dim left = ParseAdditive()
        While Cur.name = TokenType.Punct AndAlso
              (Cur.text = "<" OrElse Cur.text = "<=" OrElse Cur.text = ">" OrElse Cur.text = ">=")
            Dim op = Cur.Text
            _pos += 1
            left = New BinaryExpr(op, left, ParseAdditive())
        End While
        Return left
    End Function

    Private Function ParseAdditive() As Expression
        Dim left = ParseMultiplicative()
        While At("+") OrElse At("-")
            Dim op = Cur.Text
            _pos += 1
            left = New BinaryExpr(op, left, ParseMultiplicative())
        End While
        Return left
    End Function

    Private Function ParseMultiplicative() As Expression
        Dim left = ParseExponent()
        While At("*") OrElse At("/") OrElse At("%")
            Dim op = Cur.Text
            _pos += 1
            left = New BinaryExpr(op, left, ParseExponent())
        End While
        Return left
    End Function

    ''' <summary>Right-associative exponentiation, above unary binding (JS spec).</summary>
    Private Function ParseExponent() As Expression
        Dim left = ParseUnary()
        If At("**") Then
            _pos += 1
            Return New BinaryExpr("**", left, ParseExponent())
        End If
        Return left
    End Function

    Private Function ParseUnary() As Expression
        If Cur.name = TokenType.Keyword AndAlso Cur.text = "typeof" Then
            _pos += 1
            Return New UnaryExpr("typeof", ParseUnary())
        End If
        If At("!") OrElse At("-") OrElse At("+") Then
            Dim op = Cur.Text
            _pos += 1
            Return New UnaryExpr(op, ParseUnary())
        End If
        If At("++") OrElse At("--") Then
            Dim op = Cur.Text
            _pos += 1
            Dim target = ParseUnary()
            If Not (TypeOf target Is IdentExpr OrElse TypeOf target Is MemberExpr) Then
                Throw New ParseException("invalid ++/-- target")
            End If
            Return New UpdateExpr(op, target, True)
        End If
        Return ParsePostfix()
    End Function

    Private Function ParsePostfix() As Expression
        Dim e = ParseCallOrMember()
        If (At("++") OrElse At("--")) AndAlso Not Cur.LineBreakBefore Then
            Dim op = Cur.Text
            _pos += 1
            If Not (TypeOf e Is IdentExpr OrElse TypeOf e Is MemberExpr) Then
                Throw New ParseException("invalid ++/-- target")
            End If
            Return New UpdateExpr(op, e, False)
        End If
        Return e
    End Function

    Private Function ParseCallOrMember() As Expression
        Dim e = ParsePrimary()
        While True
            If At(".") Then
                _pos += 1
                Dim name = ExpectIdentifier()
                e = New MemberExpr(e, name, False)
            ElseIf At("(") Then
                _pos += 1
                Dim args As New List(Of Expression)
                If Not At(")") Then
                    Do
                        args.Add(ParseExpression())
                    Loop While Match(",")
                End If
                Eat(")")
                e = New CallExpr(e, args)
            ElseIf At("[") Then
                _pos += 1
                Dim idx = ParseExpression()
                Eat("]")
                e = New MemberExpr(e, idx, True)
            Else
                Exit While
            End If
        End While
        Return e
    End Function

    Private Function ParsePrimary() As Expression
        Select Case Cur.name
            Case TokenType.Number
                Dim v = CDbl(Cur.Value)
                _pos += 1
                Return New LiteralExpr(v)
            Case TokenType.String
                Dim s = CStr(Cur.Value)
                _pos += 1
                Return New LiteralExpr(s)
            Case TokenType.Identifier
                Dim name = Cur.text
                _pos += 1
                Return New IdentExpr(name)
            Case TokenType.Punct
                If At("(") Then
                    _pos += 1
                    Dim e = ParseExpression()
                    Eat(")")
                    Return e
                End If
                If At("[") Then
                    _pos += 1
                    Dim elements As New List(Of Expression)
                    If Not At("]") Then
                        Do
                            elements.Add(ParseExpression())
                        Loop While Match(",")
                    End If
                    Eat("]")
                    Return New ArrayExpr(elements)
                End If
                If At("{") Then
                    _pos += 1
                    Dim props As New List(Of KeyValuePair(Of String, Expression))
                    If Not At("}") Then
                        Do
                            Dim key As String
                            Select Case Cur.name
                                Case TokenType.Identifier, TokenType.Keyword : key = Cur.text : _pos += 1
                                Case TokenType.String : key = CStr(Cur.Value) : _pos += 1
                                Case TokenType.Number : key = JsRuntime.JsStr(CDbl(Cur.Value)) : _pos += 1
                                Case Else
                                    Throw New ParseException($"bad object key '{Cur.text}' at {Cur.Location}")
                            End Select
                            Eat(":")
                            props.Add(New KeyValuePair(Of String, Expression)(key, ParseAssignment()))
                        Loop While Match(",")
                    End If
                    Eat("}")
                    Return New ObjectExpr(props)
                End If
        End Select
        If Cur.name = TokenType.Keyword Then
            Select Case Cur.text
                Case "true" : _pos += 1 : Return New LiteralExpr(True)
                Case "false" : _pos += 1 : Return New LiteralExpr(False)
                Case "null" : _pos += 1 : Return New LiteralExpr(Nothing)
                Case "undefined" : _pos += 1 : Return New LiteralExpr(JsRuntime.Undef)
                Case "function"
                    _pos += 1
                    Dim name As String = Nothing
                    If Cur.name = TokenType.Identifier Then
                        name = Cur.text
                        _pos += 1
                    End If
                    Return ParseFunctionRest(name)
            End Select
        End If
        Throw New ParseException($"unexpected token '{Cur.text}' at {Cur.Location}")
    End Function

End Class
