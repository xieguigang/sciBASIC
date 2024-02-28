Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
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
                Case "var", "let", "dim"
                    Dim names As String()

                    ' symbol reference
                    If TypeOf val Is JsonValue Then
                        names = {CStr(DirectCast(val, JsonValue))}
                    ElseIf TypeOf val Is JsonArray Then
                        names = DirectCast(val, JsonArray).Select(Function(r) CStr(DirectCast(r, JsonValue))).ToArray
                    Else
                        Throw New InvalidCastException("the json object can not be used for variable name!")
                    End If

                    Return Expression.RuntimeVariables(names.Select(Function(r) symbols(r)))
            End Select

            Throw New NotImplementedException
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
            Else
                Return Expression.Constant(val.value)
            End If
        End Function
    End Class
End Namespace