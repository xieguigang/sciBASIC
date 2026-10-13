Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Scripting.Runtime


''' <summary>
''' Recursive-descent parser: token stream → <see cref="Program"/> AST.
''' <para>Operator precedence (low→high): sequence (comma), assignment, ternary,
''' <c>||</c>, <c>??</c>, <c>&amp;&amp;</c>, bitwise-or, bitwise-xor, bitwise-and,
''' equality, relational (<c>in</c>/<c>instanceof</c>), shift, additive,
''' multiplicative, unary, postfix, call/member, primary.</para>
''' <para>Automatic-semicolon-insertion: a missing ';' is accepted before a
''' line break, '}' or EOF. Arrow functions are parsed with backtracking.</para>
''' </summary>
Public NotInheritable Class Parser

    Private ReadOnly _tokens As List(Of Token)
    Private _pos As Integer
    Private _loopDepth As Integer
    Private _funcDepth As Integer
    Private _switchDepth As Integer
    ''' <summary>Suppresses the 'in' operator inside a for-header (for-in grammar).</summary>
    Private _noIn As Boolean
    ''' <summary>Labels in scope: (name, isLoopLabel) — targets of labeled break/continue.</summary>
    Private ReadOnly _labels As New List(Of KeyValuePair(Of String, Boolean))

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

    ''' <summary>Contextual keyword 'of' (for-of header) — kept as an identifier token.</summary>
    Private Function IsAtOf() As Boolean
        Return Cur.name = TokenType.Identifier AndAlso Cur.text = "of"
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
        ' labeled statement: `identifier ':'` at statement start is unambiguously a
        ' label (no expression statement can begin with `ident :`)
        If Cur.name = TokenType.Identifier AndAlso _pos + 1 < _tokens.Count Then
            Dim nxt = _tokens(_pos + 1)
            If nxt.name = TokenType.Punct AndAlso nxt.text = ":" Then
                Dim label = Cur.text
                _pos += 2
                ' a label wrapping a loop may be the target of a labeled continue
                Dim isLoopLabel = AtKeyword("while") OrElse AtKeyword("do") OrElse AtKeyword("for")
                _labels.Add(New KeyValuePair(Of String, Boolean)(label, isLoopLabel))
                Try
                    Dim body = ParseStatement()
                    Return New LabeledStmt(label, body)
                Finally
                    _labels.RemoveAt(_labels.Count - 1)
                End Try
            End If
        End If
        If Cur.name = TokenType.Keyword Then
            Select Case Cur.text
                Case "var", "let", "const" : Dim s = ParseVar() : ConsumeStatementEnd() : Return s
                Case "function" : Return ParseFunctionDecl()
                Case "if" : Return ParseIf()
                Case "for" : Return ParseFor()
                Case "while" : Return ParseWhile()
                Case "do" : Return ParseDoWhile()
                Case "switch" : Return ParseSwitch()
                Case "break"
                    _pos += 1
                    Dim blabel = TryParseLabelRef()
                    If blabel Is Nothing AndAlso _loopDepth = 0 AndAlso _switchDepth = 0 Then
                        Throw New ParseException("'break' outside of a loop or switch")
                    End If
                    ConsumeStatementEnd()
                    Return New BreakStmt(blabel)
                Case "continue"
                    _pos += 1
                    Dim clabel = TryParseLabelRef()
                    If clabel Is Nothing Then
                        If _loopDepth = 0 Then Throw New ParseException("'continue' outside of a loop")
                    Else
                        Dim entry = _labels.Find(Function(kv) kv.Key = clabel)
                        If Not entry.Value Then
                            Throw New ParseException($"'{clabel}' is not a loop label — continue requires a loop")
                        End If
                    End If
                    ConsumeStatementEnd()
                    Return New ContinueStmt(clabel)
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

    ''' <summary>Consume an identifier naming a label in scope; Nothing when absent.</summary>
    Private Function TryParseLabelRef() As String
        If Cur.name = TokenType.Identifier AndAlso Not Cur.LineBreakBefore AndAlso
           _labels.Exists(Function(kv) kv.Key = Cur.text) Then
            Dim label = Cur.text
            _pos += 1
            Return label
        End If
        Return Nothing
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
            Dim target = ParseBindingTarget()
            Dim init As Expression = Nothing
            If Match("=") Then init = ParseAssignment()
            decls.Add(New VarDeclarant(target, init))
        Loop While Match(",")
        Return New VarStmt(kind, decls)
    End Function

    ''' <summary>A declaration target: simple name or destructuring pattern.</summary>
    Private Function ParseBindingTarget() As Object
        If At("[") OrElse At("{") Then
            Dim e = ParseConditional()
            Dim p = TryConvertToPattern(e)
            If p Is Nothing Then
                Throw New ParseException($"invalid destructuring target at {Cur.Location}")
            End If
            Return p
        End If
        Return ExpectIdentifier()
    End Function

    Private Function ParseFunctionDecl() As Statement
        EatKeyword("function")
        Dim name = ExpectIdentifier()
        Dim f = ParseFunctionRest(name)
        Return New FuncDeclStmt(name, f.Parameters, f.Body, f.Defaults, f.RestParam)
    End Function

    ''' <summary>Parse "(params) { body }" (function keyword already consumed).</summary>
    Private Function ParseFunctionRest(name As String) As FuncExpr
        Dim spec = ParseParameterList()
        Dim outerFunc = _funcDepth
        Dim outerLoop = _loopDepth
        _funcDepth += 1
        _loopDepth = 0
        Dim body = ParseBlock()
        _funcDepth = outerFunc
        _loopDepth = outerLoop
        Return New FuncExpr(name, spec.Parameters.ToArray(), body,
                            defaults:=spec.Defaults.ToArray(), restParam:=spec.Rest)
    End Function

    ''' <summary>Parameter list with default values and a trailing rest parameter.</summary>
    Private Function ParseParameterList() As ParamSpec
        Dim spec As New ParamSpec()
        Eat("(")
        If Not At(")") Then
            Do
                If At("...") Then
                    _pos += 1
                    spec.Rest = ExpectIdentifier()
                    Exit Do          ' rest must be the last parameter
                End If
                Dim pname = ExpectIdentifier()
                Dim dflt As Expression = Nothing
                If Match("=") Then dflt = ParseAssignment()
                spec.Parameters.Add(pname)
                spec.Defaults.Add(dflt)
            Loop While Match(",") AndAlso Not At(")")
        End If
        Eat(")")
        Return spec
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

        If At(";") Then
            _pos += 1
            Return ParseForRest(Nothing, Nothing)
        End If

        ' ---- declaration head: var / let / const ----
        If AtKeyword("var") OrElse AtKeyword("let") OrElse AtKeyword("const") Then
            Dim kind = Cur.text
            _pos += 1
            Dim target = ParseBindingTarget()

            If IsAtOf() Then
                _pos += 1
                Dim iter = ParseAssignment()
                Eat(")")
                Return New ForOfStmt(kind, target, iter, ParseLoopBody())
            End If
            If AtKeyword("in") Then
                If TypeOf target IsNot String Then
                    Throw New ParseException("for-in target must be a simple variable name")
                End If
                _pos += 1
                Dim obj = ParseExpression()
                Eat(")")
                Return New ForInStmt(CStr(target), obj, ParseLoopBody())
            End If

            ' plain for: first declarator, then optional more
            Dim decls As New List(Of VarDeclarant)
            Dim init1 As Expression = Nothing
            If Match("=") Then init1 = ParseAssignment()
            decls.Add(New VarDeclarant(target, init1))
            While Match(",")
                Dim t2 = ParseBindingTarget()
                Dim init2 As Expression = Nothing
                If Match("=") Then init2 = ParseAssignment()
                decls.Add(New VarDeclarant(t2, init2))
            End While
            Eat(";")
            Return ParseForRest(New VarStmt(kind, decls), Nothing)
        End If

        ' ---- expression head (assignment target or plain expression) ----
        ' 'in' is NOT an operator inside a for header (for-in grammar)
        _noIn = True
        Dim initExpr As Expression = Nothing
        Try
            initExpr = ParseExpression()
        Finally
            _noIn = False
        End Try

        If IsAtOf() Then
            _pos += 1
            Dim iter = ParseAssignment()
            Eat(")")
            Dim ofTarget = TryConvertToPattern(initExpr)
            If ofTarget Is Nothing Then
                Throw New ParseException("invalid for-of assignment target")
            End If
            Return New ForOfStmt(Nothing, ofTarget, iter, ParseLoopBody())
        End If
        If AtKeyword("in") Then
            _pos += 1
            Dim obj = ParseExpression()
            Eat(")")
            If TypeOf initExpr IsNot IdentExpr Then
                Throw New ParseException("for-in target must be an identifier")
            End If
            Return New ForInStmt(DirectCast(initExpr, IdentExpr).Name, obj, ParseLoopBody())
        End If
        Eat(";")
        Return ParseForRest(Nothing, initExpr)
    End Function

    ''' <summary>Shared tail of the plain for-loop grammar (test / update / body).</summary>
    Private Function ParseForRest(initVar As VarStmt, initExpr As Expression) As Statement
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

    Private Function ParseSwitch() As Statement
        EatKeyword("switch")
        Eat("(")
        Dim disc = ParseExpression()
        Eat(")")
        Eat("{")
        Dim cases As New List(Of SwitchCase)
        _switchDepth += 1
        Try
            While Not At("}")
                If Cur.name = TokenType.Eof Then
                    Throw New ParseException("missing '}' — unexpected end of input in switch")
                End If
                If MatchKeyword("case") Then
                    Dim test = ParseExpression()
                    Eat(":")
                    cases.Add(New SwitchCase(test, ParseCaseBody()))
                ElseIf MatchKeyword("default") Then
                    Eat(":")
                    cases.Add(New SwitchCase(Nothing, ParseCaseBody()))
                Else
                    Throw New ParseException($"expected 'case' or 'default' but found '{Cur.text}' at {Cur.Location}")
                End If
            End While
        Finally
            _switchDepth -= 1
        End Try
        Eat("}")
        Return New SwitchStmt(disc, cases)
    End Function

    ''' <summary>Statements up to the next case/default arm or the closing '}'.</summary>
    Private Function ParseCaseBody() As List(Of Statement)
        Dim body As New List(Of Statement)
        While Not At("}") AndAlso Not AtKeyword("case") AndAlso Not AtKeyword("default")
            If Cur.name = TokenType.Eof Then
                Throw New ParseException("missing '}' — unexpected end of input in switch")
            End If
            body.Add(ParseStatement())
        End While
        Return body
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
        Dim first = ParseAssignment()
        If Not At(",") Then Return first
        ' comma operator: evaluate all, return the last
        Dim exprs As New List(Of Expression) From {first}
        While Match(",")
            exprs.Add(ParseAssignment())
        End While
        Return New SequenceExpr(exprs)
    End Function

    Private Function ParseAssignment() As Expression
        ' arrow function: single identifier followed by "=>" on the same line
        If Cur.name = TokenType.Identifier AndAlso _pos + 1 < _tokens.Count Then
            Dim nxt = _tokens(_pos + 1)
            If nxt.name = TokenType.Punct AndAlso nxt.text = "=>" AndAlso Not nxt.LineBreakBefore Then
                Dim pname = Cur.text
                _pos += 2
                Return ParseArrowBody(ParamSpec.FromName(pname))
            End If
        End If
        ' arrow function: "(params) =>" (params may carry defaults / rest)
        If At("(") Then
            Dim save = _pos
            Dim ok = True
            Dim spec As New ParamSpec()
            _pos += 1
            If At(")") Then
                _pos += 1
            Else
                Do
                    If At("...") Then
                        _pos += 1
                        If Cur.name <> TokenType.Identifier Then ok = False : Exit Do
                        spec.Rest = Cur.text
                        _pos += 1
                        Exit Do          ' rest must be last
                    End If
                    If Cur.name <> TokenType.Identifier Then ok = False : Exit Do
                    spec.Parameters.Add(Cur.text)
                    _pos += 1
                    Dim dflt As Expression = Nothing
                    If At("=") Then
                        _pos += 1
                        dflt = ParseAssignment()
                    End If
                    spec.Defaults.Add(dflt)
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
                Return ParseArrowBody(spec)
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
            If op = "=" AndAlso Not (TypeOf left Is IdentExpr OrElse TypeOf left Is MemberExpr) Then
                ' destructuring assignment: [a, b] = expr / ({a} = expr)
                Dim pattern = TryConvertToPattern(left)
                If pattern Is Nothing Then
                    Throw New ParseException("invalid assignment target")
                End If
                Return New DestructureAssignExpr(pattern, right)
            End If
            If Not (TypeOf left Is IdentExpr OrElse TypeOf left Is MemberExpr) Then
                Throw New ParseException("invalid assignment target")
            End If
            Return New AssignExpr(op, left, right)
        End If
        Return left
    End Function

    Private Function ParseArrowBody(spec As ParamSpec) As FuncExpr
        If At("{") Then
            Dim outerFunc = _funcDepth, outerLoop = _loopDepth
            _funcDepth += 1
            _loopDepth = 0
            Dim body = ParseBlock()
            _funcDepth = outerFunc
            _loopDepth = outerLoop
            Return New FuncExpr(Nothing, spec.Parameters.ToArray(), body, isArrow:=True,
                                defaults:=spec.Defaults.ToArray(), restParam:=spec.Rest)
        End If
        ' expression body → { return expr; }
        Dim expr = ParseAssignment()
        Dim body2 As New List(Of Statement) From {New ReturnStmt(expr)}
        Return New FuncExpr(Nothing, spec.Parameters.ToArray(), body2, isArrow:=True,
                            defaults:=spec.Defaults.ToArray(), restParam:=spec.Rest)
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
        Dim left = ParseNullishCoalescing()
        While At("||")
            _pos += 1
            left = New LogicalExpr("||", left, ParseNullishCoalescing())
        End While
        Return left
    End Function

    ''' <summary>Nullish coalescing (??) — short-circuits on null/undefined.</summary>
    Private Function ParseNullishCoalescing() As Expression
        Dim left = ParseLogicalAnd()
        While At("??")
            _pos += 1
            left = New LogicalExpr("??", left, ParseLogicalAnd())
        End While
        Return left
    End Function

    Private Function ParseLogicalAnd() As Expression
        Dim left = ParseBitwiseOr()
        While At("&&")
            _pos += 1
            left = New LogicalExpr("&&", left, ParseBitwiseOr())
        End While
        Return left
    End Function

    Private Function ParseBitwiseOr() As Expression
        Dim left = ParseBitwiseXor()
        While At("|")
            _pos += 1
            left = New BinaryExpr("|", left, ParseBitwiseXor())
        End While
        Return left
    End Function

    Private Function ParseBitwiseXor() As Expression
        Dim left = ParseBitwiseAnd()
        While At("^")
            _pos += 1
            left = New BinaryExpr("^", left, ParseBitwiseAnd())
        End While
        Return left
    End Function

    Private Function ParseBitwiseAnd() As Expression
        Dim left = ParseEquality()
        While At("&")
            _pos += 1
            left = New BinaryExpr("&", left, ParseEquality())
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

    ''' <summary>Relational level: &lt; &lt;= &gt; &gt;= plus the in/instanceof keywords.</summary>
    Private Function ParseRelational() As Expression
        Dim left = ParseShift()
        While True
            If Cur.name = TokenType.Punct AndAlso
               (Cur.text = "<" OrElse Cur.text = "<=" OrElse Cur.text = ">" OrElse Cur.text = ">=") Then
                Dim op = Cur.Text
                _pos += 1
                left = New BinaryExpr(op, left, ParseShift())
            ElseIf Not _noIn AndAlso AtKeyword("in") Then
                _pos += 1
                left = New BinaryExpr("in", left, ParseShift())
            ElseIf AtKeyword("instanceof") Then
                _pos += 1
                left = New BinaryExpr("instanceof", left, ParseShift())
            Else
                Exit While
            End If
        End While
        Return left
    End Function

    Private Function ParseShift() As Expression
        Dim left = ParseAdditive()
        While At("<<") OrElse At(">>") OrElse At(">>>")
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
        If AtKeyword("delete") OrElse AtKeyword("void") Then
            Dim op = Cur.Text
            _pos += 1
            Return New UnaryExpr(op, ParseUnary())
        End If
        If At("!") OrElse At("-") OrElse At("+") OrElse At("~") Then
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
        Dim anyOptional = False
        While True
            If At(".") Then
                _pos += 1
                Dim name = ExpectIdentifier()
                e = New MemberExpr(e, name, False)
            ElseIf At("?.") Then
                _pos += 1
                anyOptional = True
                If At("(") Then
                    ' optional call: a?.(args)
                    Dim args2 = ParseArguments()
                    e = New CallExpr(e, args2, isOptional:=True)
                ElseIf At("[") Then
                    _pos += 1
                    Dim idx = ParseExpression()
                    Eat("]")
                    e = New MemberExpr(e, idx, True, isOptional:=True)
                Else
                    Dim name = ExpectIdentifier()
                    e = New MemberExpr(e, name, False, isOptional:=True)
                End If
            ElseIf At("(") Then
                e = New CallExpr(e, ParseArguments(), isOptional:=False)
            ElseIf At("[") Then
                _pos += 1
                Dim idx = ParseExpression()
                Eat("]")
                e = New MemberExpr(e, idx, True)
            Else
                Exit While
            End If
        End While
        If anyOptional Then Return New OptionalChainExpr(e)
        Return e
    End Function

    ''' <summary>Call argument list with spread elements and trailing commas.</summary>
    Private Function ParseArguments() As List(Of Expression)
        Eat("(")
        Dim args As New List(Of Expression)
        If Not At(")") Then
            Do
                If At("...") Then
                    _pos += 1
                    args.Add(New SpreadElement(ParseAssignment()))
                Else
                    args.Add(ParseExpression())
                End If
            Loop While Match(",") AndAlso Not At(")")
        End If
        Eat(")")
        Return args
    End Function

    Private Function ParsePrimary() As Expression
        Select Case Cur.name
            Case TokenType.Number
                Dim v = CDbl(Cur.Value)
                _pos += 1
                Return New LiteralExpr(JsValue.Number(v))
            Case TokenType.String
                Dim s = CStr(Cur.Value)
                _pos += 1
                Return New LiteralExpr(JsValue.Str(s))
            Case TokenType.Template
                Return ParseTemplate()
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
                    Return ParseArrayLiteral()
                End If
                If At("{") Then
                    Return ParseObjectLiteral()
                End If
        End Select
        If Cur.name = TokenType.Keyword Then
            Select Case Cur.text
                Case "true" : _pos += 1 : Return New LiteralExpr(JsValue.Boolean_(True))
                Case "false" : _pos += 1 : Return New LiteralExpr(JsValue.Boolean_(False))
                Case "null" : _pos += 1 : Return New LiteralExpr(JsValue.Null)
                Case "undefined" : _pos += 1 : Return New LiteralExpr(JsValue.Undef)
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

    ''' <summary>Template literal token → chunks of cooked text and parsed expressions.</summary>
    Private Function ParseTemplate() As Expression
        Dim parts = DirectCast(Cur.Value, List(Of TemplateChunk))
        _pos += 1
        Dim chunks As New List(Of Object)
        For Each part In parts
            If part.IsExpression Then
                chunks.Add(ParseEmbeddedExpression(part.Text))
            Else
                chunks.Add(part.Text)
            End If
        Next
        Return New TemplateExpr(chunks)
    End Function

    ''' <summary>Recursively parse the raw source of a ${...} placeholder.</summary>
    Private Shared Function ParseEmbeddedExpression(source As String) As Expression
        Dim program = Parser.Parse(source)
        If program.Body.Count <> 1 OrElse Not TypeOf program.Body(0) Is ExprStmt Then
            Throw New ParseException("invalid template expression placeholder")
        End If
        Return DirectCast(program.Body(0), ExprStmt).Expr
    End Function

    ''' <summary>Array literal with holes ([a, , b]) and spread elements ([...xs]).</summary>
    Private Function ParseArrayLiteral() As Expression
        Eat("[")
        Dim elements As New List(Of Expression)
        If Not At("]") Then
            Do
                If At("]") Then Exit Do               ' trailing comma — no hole
                If At(",") Then
                    elements.Add(Nothing)             ' hole
                    _pos += 1
                    Continue Do
                End If
                If At("...") Then
                    _pos += 1
                    elements.Add(New SpreadElement(ParseAssignment()))
                Else
                    elements.Add(ParseAssignment())
                End If
            Loop While Match(",")
        End If
        Eat("]")
        Return New ArrayExpr(elements)
    End Function

    ''' <summary>
    ''' Object literal: spread, getter/setter, methods, computed keys,
    ''' key:value and shorthand (incl. shorthand-with-default sugar).
    ''' </summary>
    Private Function ParseObjectLiteral() As Expression
        Eat("{")
        Dim props As New List(Of ObjectProperty)
        If Not At("}") Then
            Do
                props.Add(ParseObjectProperty())
            Loop While Match(",") AndAlso Not At("}")
        End If
        Eat("}")
        Return New ObjectExpr(props)
    End Function

    Private Function ParseObjectProperty() As ObjectProperty
        ' {...expr} object spread
        If At("...") Then
            _pos += 1
            Return New ObjectProperty(Nothing, Nothing, ParseAssignment(), ObjectPropertyKind.Spread)
        End If

        ' get/set accessors — contextual keywords: they only count when a key
        ' and '(' follow ('{get: 1}' stays a normal property named "get")
        If Cur.name = TokenType.Identifier AndAlso (Cur.text = "get" OrElse Cur.text = "set") AndAlso
           IsAccessorHeader() Then
            Dim isGetter = Cur.text = "get"
            _pos += 1
            Dim key = ParsePropertyName()
            Dim spec = ParseParameterList()
            If isGetter AndAlso spec.Parameters.Count <> 0 Then
                Throw New ParseException("getter must not take parameters")
            End If
            If Not isGetter AndAlso spec.Parameters.Count <> 1 Then
                Throw New ParseException("setter must take exactly one parameter")
            End If
            Dim outerFunc = _funcDepth, outerLoop = _loopDepth
            _funcDepth += 1
            _loopDepth = 0
            Dim body = ParseBlock()
            _funcDepth = outerFunc
            _loopDepth = outerLoop
            Dim fn = New FuncExpr(Nothing, spec.Parameters.ToArray(), body)
            Return New ObjectProperty(key, Nothing, fn,
                                      If(isGetter, ObjectPropertyKind.Getter, ObjectPropertyKind.Setter))
        End If

        ' computed key: [expr]: value
        If At("[") Then
            _pos += 1
            Dim keyExpr = ParseAssignment()
            Eat("]")
            Eat(":")
            Return New ObjectProperty(Nothing, keyExpr, ParseAssignment(), ObjectPropertyKind.Normal)
        End If

        ' method: key(params) { body }
        If IsMethodHeader() Then
            Dim key = ParsePropertyName()
            Dim spec = ParseParameterList()
            Dim outerFunc = _funcDepth, outerLoop = _loopDepth
            _funcDepth += 1
            _loopDepth = 0
            Dim body = ParseBlock()
            _funcDepth = outerFunc
            _loopDepth = outerLoop
            Dim fn = New FuncExpr(Nothing, spec.Parameters.ToArray(), body,
                                  defaults:=spec.Defaults.ToArray(), restParam:=spec.Rest)
            Return New ObjectProperty(key, Nothing, fn, ObjectPropertyKind.Normal)
        End If

        ' plain key
        Dim k = ParsePropertyName()
        If Match(":") Then
            Return New ObjectProperty(k, Nothing, ParseAssignment(), ObjectPropertyKind.Normal)
        End If
        ' shorthand { a } — or { a = default } (destructuring sugar; evaluated as
        ' an assignment when the literal lands in expression position)
        If Match("=") Then
            Return New ObjectProperty(k, Nothing,
                                      New AssignExpr("=", New IdentExpr(k), ParseAssignment()),
                                      ObjectPropertyKind.Shorthand)
        End If
        Return New ObjectProperty(k, Nothing, Nothing, ObjectPropertyKind.Shorthand)
    End Function

    ''' <summary>Identifier / keyword / string / numeric property name.</summary>
    Private Function ParsePropertyName() As String
        Select Case Cur.name
            Case TokenType.Identifier, TokenType.Keyword
                Dim n = Cur.text
                _pos += 1
                Return n
            Case TokenType.String
                Dim s = CStr(Cur.Value)
                _pos += 1
                Return s
            Case TokenType.Number
                Dim n = JsRuntime.JsStr(CDbl(Cur.Value))
                _pos += 1
                Return n
            Case Else
                Throw New ParseException($"bad object key '{Cur.text}' at {Cur.Location}")
        End Select
    End Function

    ''' <summary>True when the current token starts a get/set accessor header.</summary>
    Private Function IsAccessorHeader() As Boolean
        If _pos + 2 >= _tokens.Count Then Return False
        Dim keyTok = _tokens(_pos + 1)
        Dim parenTok = _tokens(_pos + 2)
        If Not (keyTok.name = TokenType.Identifier OrElse keyTok.name = TokenType.Keyword OrElse
                keyTok.name = TokenType.String OrElse keyTok.name = TokenType.Number) Then Return False
        Return parenTok.name = TokenType.Punct AndAlso parenTok.text = "("
    End Function

    ''' <summary>True when the current key token is directly followed by '(' (method).</summary>
    Private Function IsMethodHeader() As Boolean
        If _pos + 1 >= _tokens.Count Then Return False
        If Not (Cur.name = TokenType.Identifier OrElse Cur.name = TokenType.Keyword OrElse
                Cur.name = TokenType.String OrElse Cur.name = TokenType.Number) Then Return False
        Dim nxt = _tokens(_pos + 1)
        Return nxt.name = TokenType.Punct AndAlso nxt.text = "("
    End Function

    ' ---------------- destructuring conversion ----------------

    ''' <summary>
    ''' Convert an already-parsed expression into a destructuring target
    ''' (JS parses the pattern as an expression, then reinterprets it):
    ''' identifier / member → target; array literal → array pattern;
    ''' object literal → object pattern; <c>target = default</c> → default pattern.
    ''' Returns Nothing when the expression is not a valid target.
    ''' </summary>
    Private Shared Function TryConvertToPattern(e As Expression) As BindingTarget
        Select Case True
            Case TypeOf e Is IdentExpr
                Return New NameTarget(DirectCast(e, IdentExpr).Name)
            Case TypeOf e Is MemberExpr
                Dim m = DirectCast(e, MemberExpr)
                Return New MemberTarget(m.Obj, m.Name, m.Computed)
            Case TypeOf e Is ArrayExpr
                Dim a = DirectCast(e, ArrayExpr)
                Dim els As New List(Of BindingTarget)
                Dim rest As BindingTarget = Nothing
                For Each el In a.Elements
                    If el Is Nothing Then
                        els.Add(Nothing)                       ' hole
                    ElseIf TypeOf el Is SpreadElement Then
                        rest = ConvertRestTarget(DirectCast(el, SpreadElement).Argument)
                        If rest Is Nothing Then Return Nothing
                    Else
                        Dim t = TryConvertToPattern(el)
                        If t Is Nothing Then Return Nothing
                        els.Add(t)
                    End If
                Next
                Return New ArrayTarget(els, rest)
            Case TypeOf e Is ObjectExpr
                Dim o = DirectCast(e, ObjectExpr)
                Dim props As New List(Of KeyValuePair(Of String, BindingTarget))
                Dim rest As BindingTarget = Nothing
                For Each p In o.Properties
                    Select Case p.Kind
                        Case ObjectPropertyKind.Shorthand
                            If p.Value Is Nothing Then
                                props.Add(New KeyValuePair(Of String, BindingTarget)(p.Key, New NameTarget(p.Key)))
                            Else
                                ' { a = default } sugar
                                Dim dflt = DirectCast(p.Value, AssignExpr)
                                props.Add(New KeyValuePair(Of String, BindingTarget)(
                                          p.Key, New DefaultTarget(New NameTarget(p.Key), dflt.Value)))
                            End If
                        Case ObjectPropertyKind.Normal
                            If p.KeyExpr IsNot Nothing Then Return Nothing   ' computed keys are not patterns
                            Dim t = TryConvertToPattern(p.Value)
                            If t Is Nothing Then Return Nothing
                            props.Add(New KeyValuePair(Of String, BindingTarget)(p.Key, t))
                        Case ObjectPropertyKind.Spread
                            rest = ConvertRestTarget(p.Value)
                            If rest Is Nothing Then Return Nothing
                        Case Else
                            Return Nothing       ' getters/setters cannot be patterns
                    End Select
                Next
                Return New ObjectTarget(props, rest)
            Case TypeOf e Is AssignExpr
                Dim a = DirectCast(e, AssignExpr)
                If a.Op <> "=" Then Return Nothing
                Dim t = TryConvertToPattern(a.Target)
                If t Is Nothing Then Return Nothing
                Return New DefaultTarget(t, a.Value)
        End Select
        Return Nothing
    End Function

    ''' <summary>Rest targets must be a plain name or a member reference.</summary>
    Private Shared Function ConvertRestTarget(e As Expression) As BindingTarget
        If TypeOf e Is IdentExpr Then Return New NameTarget(DirectCast(e, IdentExpr).Name)
        If TypeOf e Is MemberExpr Then
            Dim m = DirectCast(e, MemberExpr)
            Return New MemberTarget(m.Obj, m.Name, m.Computed)
        End If
        Return Nothing
    End Function

    ''' <summary>Parsed parameter list: names + aligned defaults + optional rest.</summary>
    Private NotInheritable Class ParamSpec

        Public ReadOnly Parameters As New List(Of String)
        Public ReadOnly Defaults As New List(Of Expression)
        ''' <summary>Rest parameter name (...args), or Nothing.</summary>
        Public Rest As String

        Public Shared Function FromName(name As String) As ParamSpec
            Dim spec As New ParamSpec()
            spec.Parameters.Add(name)
            spec.Defaults.Add(Nothing)
            Return spec
        End Function
    End Class

End Class
