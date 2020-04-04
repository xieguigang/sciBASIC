Imports System.Linq.Expressions
Imports FactorialLiteral = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.Factorial
Imports MathExpression = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.Expression
Imports NumericLiteral = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.Literal
Imports OperatorBinary = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.BinaryExpression
Imports SymbolReference = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.SymbolExpression

Public Module ExpressionCompiler

    Public Function CreateLambda(arguments As String(), model As MathExpression) As LambdaExpression
        Dim parameters As Dictionary(Of String, ParameterExpression) = arguments _
            .ToDictionary(Function(name) name,
                          Function(name)
                              Return Expression.Parameter(GetType(Double), name)
                          End Function)
        Dim body As Expression = CreateExpression(parameters, model)
        Dim lambda As LambdaExpression = Expression.Lambda(body, parameters.Values.ToArray)

        Return lambda
    End Function

    Private Function CreateExpression(parameters As Dictionary(Of String, ParameterExpression), model As MathExpression) As Expression
        Select Case model.GetType
            Case GetType(NumericLiteral)
                Return Expression.Constant(DirectCast(model, NumericLiteral).Evaluate(Nothing), GetType(Double))
            Case GetType(FactorialLiteral)
                Return Expression.Constant(DirectCast(model, FactorialLiteral).Evaluate(Nothing), GetType(Double))
            Case GetType(SymbolReference)
                Return parameters(DirectCast(model, SymbolReference).symbolName)
            Case GetType(OperatorBinary)
                Dim bin As OperatorBinary = DirectCast(model, OperatorBinary)
                Dim left As Expression = CreateExpression(parameters, bin.left)
                Dim right As Expression = CreateExpression(parameters, bin.right)

                Select Case bin.operator
                    Case "+"c : Return Expression.Add(left, right)
                    Case "-"c : Return Expression.Subtract(left, right)
                    Case "*"c : Return Expression.Multiply(left, right)
                    Case "/"c : Return Expression.Divide(left, right)
                    Case "^"c : Return Expression.Power(left, right)
                    Case "%"c : Return Expression.Modulo(left, right)
                    Case Else
                        Throw New InvalidCastException(bin.ToString)
                End Select

            Case Else
                Throw New InvalidProgramException(model.GetType.FullName)
        End Select
    End Function
End Module
