Imports System.CodeDom
Imports System.Collections.Generic
Imports System.Collections.Specialized

Namespace Parser

    ''' <summary>
    ''' Parses expressions written in strings into CodeDom expressions.  There is a certain 
    ''' amount of context that the parser may need to be familiar with.  This is why the 
    ''' parsing methods are not exposed as static.
    ''' </summary>
    Public Class Parser : Implements System.IDisposable

        Protected Friend Shared ReadOnly _RecognizedTypes As Dictionary(Of String, CodeTypeReference) = New Dictionary(Of String, CodeTypeReference) From {
            {"string", New CodeTypeReference(GetType(String))},
            {"int", New CodeTypeReference(GetType(Integer))},
            {"float", New CodeTypeReference(GetType(Single))},
            {"double", New CodeTypeReference(GetType(Double))},
            {"DateTime", New CodeTypeReference(GetType(DateTime))},
            {"TimeSpan", New CodeTypeReference(GetType(TimeSpan))},
            {"Guid", New CodeTypeReference(GetType(Guid))},
            {"char", New CodeTypeReference(GetType(Char))},
            {"long", New CodeTypeReference(GetType(Long))},
            {"Convert", New CodeTypeReference(GetType(Convert))},
            {"boolean", New CodeTypeReference(GetType(Boolean))},
            {"integer", New CodeTypeReference(GetType(Integer))},
            {"single", New CodeTypeReference(GetType(Single))}}

        Private _Enums As Dictionary(Of String, CodeTypeReference) = New Dictionary(Of String, CodeTypeReference)
        Private _Fields As StringCollection = New StringCollection

        ''' <summary>
        ''' A collection of identifiers that should be recognized as types.
        ''' </summary>
        Public ReadOnly Property RecognizedTypes() As Dictionary(Of String, CodeTypeReference)
            Get
                Return _RecognizedTypes
            End Get
        End Property

        ''' <summary>
        ''' A collection of identifiers that should be recognized as enums.
        ''' </summary>
        Public ReadOnly Property Enums() As Dictionary(Of String, CodeTypeReference)
            Get
                Return _Enums
            End Get
        End Property

        ''' <summary>
        ''' A collection of names of fields.
        ''' </summary>
        Public ReadOnly Property Fields() As StringCollection
            Get
                Return _Fields
            End Get
        End Property

        ''' <summary>
        ''' Parses an expression into a <see cref="CodeExpression"/>.
        ''' </summary>
        ''' <param name="exp">expression to parse</param>
        ''' <returns>CodeDom representing the expression</returns>
        Public Function ParseExpression(exp As String) As CodeExpression
            Dim t As New Tokenizer(exp)
            If Not t.IsInvalid Then
                t.GetNextToken()
                Return ReadExpression(t, TokenPriority.None)
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Recursive method that reads an expression.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="priority"></param>
        ''' <returns></returns>
        Private Function ReadExpression(t As Tokenizer, priority As TokenPriority) As CodeExpression
            Dim left As CodeExpression = Nothing, right As CodeExpression = Nothing
            Dim cont As Boolean = True, applyNot As Boolean = False, applyNegative As Boolean = False
            While cont
                Select Case t.Current.Type
                    Case TokenType.Primitive
                        left = New CodePrimitiveExpression(t.Current.ParsedObject)
                        t.GetNextToken()
                        cont = False
                    Case TokenType.[Operator]
                        ' An operator here is considered a unary operator.
                        Select Case t.Current.Text
                            Case "-"
                                applyNegative = True
                            Case "!"
                                applyNot = True
                            Case Else
                                Throw New Exception("Unexpected operator: " & t.Current.Text)
                        End Select
                        t.GetNextToken()
                        Continue While
                    Case TokenType.Identifier
                        left = ReadIdentifier(t)
                        cont = False
                    Case TokenType.OpenParens
                        t.GetNextToken()
                        left = ReadExpression(t, TokenPriority.None)
                        t.GetNextToken()
                        If TypeOf left Is CodeTypeReferenceExpression Then
                            left = New CodeCastExpression(TryCast(left, CodeTypeReferenceExpression).Type, ReadExpression(t, TokenPriority.None))
                        End If
                        cont = False
                End Select
                If t.IsInvalid Then
                    cont = False
                End If
            End While
            If left Is Nothing Then
                Throw New Exception("No expression found.")
            End If
            If applyNot Then
                left = New CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, New CodePrimitiveExpression(False))
            ElseIf applyNegative Then
                left = New CodeBinaryOperatorExpression(New CodePrimitiveExpression(0), CodeBinaryOperatorType.Subtract, left)
            End If
            If t.IsInvalid OrElse t.Current.Type = TokenType.CloseParens OrElse t.Current.Type = TokenType.Comma OrElse t.Current.Type = TokenType.CloseBracket Then
                Return left
            End If
            cont = True
            While cont AndAlso Not t.IsInvalid
                Dim token As Token = t.Current
                Select Case token.Type
                    Case TokenType.[Operator]
                        If t.Current.Priority < priority Then
                            cont = False
                        Else
                            ' In the case we have an operator, we'll assume it's a binary operator.
                            Dim binOp As CodeBinaryOperatorType
                            Dim notEquals As Boolean = False
                            Select Case token.Text
                                Case ">"
                                    binOp = CodeBinaryOperatorType.GreaterThan
                                Case ">="
                                    binOp = CodeBinaryOperatorType.GreaterThanOrEqual
                                Case "<"
                                    binOp = CodeBinaryOperatorType.LessThan
                                Case "<="
                                    binOp = CodeBinaryOperatorType.LessThanOrEqual
                                Case "=", "=="
                                    binOp = CodeBinaryOperatorType.ValueEquality
                                Case "!="
                                    binOp = CodeBinaryOperatorType.ValueEquality
                                    notEquals = True
                                Case "|"
                                    binOp = CodeBinaryOperatorType.BitwiseOr
                                Case "||"
                                    binOp = CodeBinaryOperatorType.BooleanOr
                                Case "&"
                                    binOp = CodeBinaryOperatorType.BitwiseAnd
                                Case "&&"
                                    binOp = CodeBinaryOperatorType.BooleanAnd
                                Case "-"
                                    binOp = CodeBinaryOperatorType.Subtract
                                Case "+"
                                    binOp = CodeBinaryOperatorType.Add
                                Case "/"
                                    binOp = CodeBinaryOperatorType.Divide
                                Case "%"
                                    binOp = CodeBinaryOperatorType.Modulus
                                Case "*"
                                    binOp = CodeBinaryOperatorType.Multiply
                                Case Else
                                    Throw New Exception("Unrecognized operator: " & t.Current.Text)
                            End Select
                            If t.IsInvalid Then
                                Throw New Exception("Expected token for right side of binary expression.")
                            End If
                            t.GetNextToken()
                            right = ReadExpression(t, token.Priority)
                            left = New CodeBinaryOperatorExpression(left, binOp, right)

                            ' If the operator was the not equals operator, we just negate the previous binary expression.
                            If notEquals Then
                                left = New CodeBinaryOperatorExpression(left, binOp, New CodePrimitiveExpression(False))
                            End If
                        End If
                    Case TokenType.CloseParens
                        't.GetNextToken();
                        cont = False
                    Case TokenType.Dot
                        ' A dot could appear after some parentheses.  In this case we need to parse 
                        ' what's after the dot as an identifier.
                        t.GetNextToken()
                        right = ReadIdentifier(t)
                        Dim ceTemp As CodeExpression = right
                        While True
                            If TypeOf ceTemp Is CodeVariableReferenceExpression Then
                                left = New CodePropertyReferenceExpression(left, TryCast(ceTemp, CodeVariableReferenceExpression).VariableName)
                                Exit While
                            ElseIf TypeOf ceTemp Is CodePropertyReferenceExpression Then
                                Dim cpre As CodePropertyReferenceExpression = TryCast(ceTemp, CodePropertyReferenceExpression)
                                If TypeOf cpre.TargetObject Is CodeThisReferenceExpression Then
                                    cpre.TargetObject = left
                                    left = cpre
                                    Exit While
                                Else
                                    ceTemp = cpre.TargetObject
                                End If
                            ElseIf TypeOf ceTemp Is CodeFieldReferenceExpression Then
                                Dim cfre As CodeFieldReferenceExpression = TryCast(ceTemp, CodeFieldReferenceExpression)
                                If TypeOf cfre.TargetObject Is CodeThisReferenceExpression Then
                                    cfre.TargetObject = left
                                    left = cfre
                                    Exit While
                                End If
                            ElseIf TypeOf ceTemp Is CodeMethodInvokeExpression Then
                                Dim cmie As CodeMethodInvokeExpression = TryCast(ceTemp, CodeMethodInvokeExpression)
                                If TypeOf cmie.Method.TargetObject Is CodeThisReferenceExpression Then
                                    cmie.Method.TargetObject = left
                                    left = cmie
                                    Exit While
                                Else
                                    ceTemp = cmie.Method.TargetObject
                                End If
                            Else
                                Throw New Exception("Unexpected identifier found after .")
                            End If
                        End While
                        cont = False
                    Case Else
                        Throw New Exception("Token not expected: " & token.Text)
                End Select
            End While
            Return left
        End Function

        ''' <summary>
        ''' When an identifier is encountered, it could be a number of things.  A single identifer by itself
        ''' is considered a variable.  The pattern identifier[.identifier]+ will consider the 
        ''' first identifier as a variable and the others as properties.  Any identifier that is followed
        ''' by an open parenthesis is considered to be a function call.  Indexes are not handled yet, but
        ''' should be handled in the future.  If the identifier is "this" then a this reference is used.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        Private Function ReadIdentifier(t As Tokenizer) As CodeExpression
            Dim ce As CodeExpression = Nothing
            Dim token As Token = t.Current
            If String.Equals(token.Text, "this") OrElse String.Equals(token.Text, "Me", StringComparison.OrdinalIgnoreCase) Then
                ce = New CodeThisReferenceExpression()
            ElseIf String.Equals(token.Text, "null") OrElse String.Equals(token.Text, "Nothing", StringComparison.OrdinalIgnoreCase) Then
                ce = New CodePrimitiveExpression(Nothing)
            ElseIf _RecognizedTypes.ContainsKey(token.Text) Then
                ' In this case, the identifier is a recognized type.  It is likely a cast expression.
                ' We return a CodeTypeReferenceExpression.  The parsing method will see this and 
                ' try to match it to open and close parentheses.  It will then turn it into a 
                ' CodeCastExpression.  The other possibility is that this method was called from 
                ' reading a typeof(), in which case the CodeTypeReferenceExpression is still valid.
                Dim ctre As New CodeTypeReferenceExpression(TryCast(_RecognizedTypes(token.Text), CodeTypeReference))
                t.GetNextToken()
                Return ctre
            ElseIf Enums.ContainsKey(token.Text) Then
                ' In this case, the identifier is a recognized enum.  We'll return the value of this
                ' immediately since enums are always in the same format.
                Dim ctre As New CodeTypeReferenceExpression(TryCast(_Enums(token.Text), CodeTypeReference))
                t.GetNextToken()
                If t.Current.Type <> TokenType.Dot Then
                    Throw New Exception("Expected dot after enum type: " & token.Text)
                End If
                t.GetNextToken()
                If t.Current.Type <> TokenType.Identifier Then
                    Throw New Exception("Expected identifier token after enum type: " & token.Text)
                End If
                ce = New CodeFieldReferenceExpression(ctre, t.Current.Text)
                t.GetNextToken()
                Return ce
            ElseIf String.Equals(token.Text, "typeof", StringComparison.OrdinalIgnoreCase) Then
                token = t.GetNextToken()
                If token.Type <> TokenType.OpenParens Then
                    Throw New Exception("Expected open parenthesis after typeof, instead found: " & token.Text)
                End If
                t.GetNextToken()
                ce = ReadIdentifier(t)
                If Not (TypeOf ce Is CodeTypeReferenceExpression) Then
                    Throw New Exception("Illegal argument in typeof.  Expecting a type.")
                End If
                If t.Current.Type <> TokenType.CloseParens Then
                    Throw New Exception("Expected close parenthesis after type in typeof expression, instead found: " & t.Current.Text)
                End If
                Dim ctoe As New CodeTypeOfExpression(TryCast(ce, CodeTypeReferenceExpression).Type)
                Return ctoe
            Else
                ce = New CodeVariableReferenceExpression(token.Text)
            End If
            token = t.GetNextToken()
            Dim cont As Boolean = True
            While cont
                If token.Type = TokenType.Dot Then
                    token = t.GetNextToken()
                    If token.Type <> TokenType.Identifier Then
                        Throw New Exception("Expected identifier after dot, instead found: " & token.Text)
                    End If
                    If _Fields.Contains(token.Text) Then
                        ce = New CodeFieldReferenceExpression(ce, token.Text)
                    Else
                        ce = New CodePropertyReferenceExpression(ce, token.Text)
                    End If
                    token = t.GetNextToken()
                    If token.Type <> TokenType.OpenParens Then
                        Continue While
                    End If
                ElseIf token.Type = TokenType.OpenBracket Then
                    ' If an open bracket follows, then we're dealing with an indexer.
                    Dim cie As New CodeIndexerExpression()
                    cie.TargetObject = ce
                    ce = cie
                    t.GetNextToken()
                    cie.Indices.Add(ReadExpression(t, TokenPriority.None))
                    If t.Current.Type <> TokenType.CloseBracket Then
                        Throw New Exception("Unexpected token: " & t.Current.Text)
                    End If
                    token = t.GetNextToken()
                    Continue While
                End If
                If token.Type = TokenType.OpenParens Then
                    ' An open parenthesis indicates a method, so we'll change from a variable or property reference into 
                    ' a CodeMethodInvokeExpression.
                    If TypeOf ce Is CodeThisReferenceExpression Then
                        Throw New Exception("Cannot use parentheses after this keyword.")
                    End If
                    Dim cmie As New CodeMethodInvokeExpression()
                    If TypeOf ce Is CodeVariableReferenceExpression Then
                        cmie.Method = New CodeMethodReferenceExpression(New CodeThisReferenceExpression(), TryCast(ce, CodeVariableReferenceExpression).VariableName)
                    ElseIf TypeOf ce Is CodeFieldReferenceExpression Then
                        Dim cfre As CodeFieldReferenceExpression = TryCast(ce, CodeFieldReferenceExpression)
                        cmie.Method = New CodeMethodReferenceExpression(cfre.TargetObject, cfre.FieldName)
                    Else
                        ' must be a property reference
                        Dim cpre As CodePropertyReferenceExpression = TryCast(ce, CodePropertyReferenceExpression)
                        cmie.Method = New CodeMethodReferenceExpression(cpre.TargetObject, cpre.PropertyName)
                    End If
                    ce = cmie
                    token = t.GetNextToken()
                    If token.Type <> TokenType.CloseParens Then
                        cmie.Parameters.Add(ReadExpression(t, TokenPriority.None))
                        While True
                            token = t.Current
                            If token.Type = TokenType.CloseParens Then
                                token = t.GetNextToken()
                                Exit While
                            End If
                            If token.Type = TokenType.Comma Then
                                t.GetNextToken()
                                cmie.Parameters.Add(ReadExpression(t, TokenPriority.None))
                            Else
                                Throw New Exception("Unexpected token: " & token.Text)
                            End If
                        End While
                    Else
                        token = t.GetNextToken()
                    End If
                Else
                    cont = False
                End If
            End While
            Return ce
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO:  释放托管状态(托管对象)。
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace