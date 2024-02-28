Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace JSONLogic

    Public Class TreeBuilder

        ReadOnly symbols As Dictionary(Of String, ParameterExpression)

        Public ReadOnly Property Parameters As IEnumerable(Of ParameterExpression)
            Get
                Return symbols.Values
            End Get
        End Property

        Sub New(symbols As IEnumerable(Of (name As String, type As Type)))
            Me.symbols = symbols _
                .SafeQuery _
                .ToDictionary(Function(i) i.name,
                              Function(n)
                                  Return Expression.Parameter(n.type, n.name)
                              End Function)
        End Sub

        ''' <summary>
        ''' convert the json logical tree as the .net clr lambda expression tree
        ''' </summary>
        ''' <param name="logic"></param>
        ''' <returns></returns>
        Public Shared Function Parse(logic As JsonElement, ParamArray symbols As (name As String, type As Type)()) As Expression
            Return New TreeBuilder(symbols).ParserInternal(logic)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Parse(exp As JsonElement) As Expression
            Return ParserInternal(exp)
        End Function

        Private Function ParserInternal(exp As JsonElement) As Expression
            If exp Is Nothing Then
                Return Expression.Constant(Nothing)
            ElseIf TypeOf exp Is JsonValue Then
                Return ParseLiteral(exp)
            ElseIf TypeOf exp Is JsonObject Then
                Return ParseExpression(exp)
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function workSingle(key As String, val As JsonElement) As Expression
            Select Case key.ToLower
                Case "var", "let", "dim" : Return symbolReference(val)
                Case "if" : Return tripleIif(val)
                Case "<" : Return lessThan(val)
            End Select

            Throw New NotImplementedException
        End Function

        Private Function lessThan(val As JsonElement) As Expression
            If Not TypeOf val Is JsonArray Then
                Throw New InvalidCastException
            End If

            Dim list As JsonArray = val
            Dim left As Expression = ParserInternal(list(0))
            Dim right As Expression = ParserInternal(list(1))

            Return Expression.LessThan(left, right)
        End Function

        ''' <summary>
        ''' If(test, true, false)
        ''' </summary>
        ''' <returns></returns>
        Private Function tripleIif(val As JsonElement) As Expression
            If Not TypeOf val Is JsonArray Then
                Throw New InvalidCastException
            Else
                Dim triple As JsonArray = val

                If triple.Length = 3 Then
                    ' if(test,a,b)
                    Dim test As Expression = ParserInternal(triple(0))
                    Dim true1 As Expression = ParserInternal(triple(1))
                    Dim false1 As Expression = ParserInternal(triple(2))

                    Return Expression.Condition(test, true1, false1)
                ElseIf triple.Length = 2 Then
                    ' if(a,b)
                    ' -> if(a isnot nothing, a, b)
                    Dim true1 As Expression = ParserInternal(triple(0))
                    Dim false1 As Expression = ParserInternal(triple(1))

                    Return Expression.Condition(Expression.ReferenceEqual(true1, Expression.Constant(Nothing)), false1, true1)
                ElseIf triple.Length > 3 Then
                    ' [a,b,c,d,e]
                    ' if (a,b, if (c,d,e))
                    Dim exps As Expression() = triple.Select(Function(a) ParserInternal(a)).ToArray
                    Dim pairs = exps.Split(2)
                    Dim false2 As Expression = pairs.Last()(0)

                    ' skip the last
                    For Each part As Expression() In pairs.Reverse.Skip(1)
                        false2 = Expression.Condition(part(0), part(1), false2)
                    Next

                    Return false2
                Else
                    Throw New InvalidCastException
                End If
            End If
        End Function

        Private Function symbolReference(val As JsonElement) As Expression
            Dim names As String()

            ' symbol reference
            If TypeOf val Is JsonValue Then
                names = {CStr(DirectCast(val, JsonValue))}
            ElseIf TypeOf val Is JsonArray Then
                names = DirectCast(val, JsonArray).Select(Function(r) CStr(DirectCast(r, JsonValue))).ToArray
            Else
                Throw New InvalidCastException("the json object can not be used for variable name!")
            End If

            If names.Length = 1 Then
                Return symbols(names(0))
            Else
                Return Expression.RuntimeVariables(names.Select(Function(r) symbols(r)))
            End If
        End Function

        Private Function ParseExpression(exp As JsonObject) As Expression
            If exp.size = 1 Then
                Return workSingle(key:=exp.ObjectKeys.First, val:=exp(exp.ObjectKeys.First))
            ElseIf exp.size = 0 Then
                ' empty for null?
                Return Expression.Constant(Nothing)
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function ParseLiteral(val As JsonValue) As Expression
            If val.value Is Nothing Then
                Return Expression.Constant(Nothing)
            ElseIf val.UnderlyingType Is GetType(String) Then
                Return Expression.Constant(CStr(val))
            Else
                Return Expression.Constant(val.value)
            End If
        End Function
    End Class
End Namespace