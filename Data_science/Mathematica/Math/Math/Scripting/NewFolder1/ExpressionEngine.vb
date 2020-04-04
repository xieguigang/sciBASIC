Imports Microsoft.VisualBasic.Math.Scripting.Helpers
Imports stdNum = System.Math

Public Class ExpressionEngine

    ReadOnly symbols As New Dictionary(Of String, Double) From {
        {"PI", stdNum.PI},
        {"E", stdNum.E}
    }

    ''' <summary>
    ''' The mathematics calculation delegates collection with its specific name.
    ''' (具有特定名称的数学计算委托方法的集合) 
    ''' </summary>
    ''' <remarks></remarks>
    ReadOnly SystemFunctions As New Dictionary(Of String, Func(Of Double(), Double)) From {
 _
            {"abs", Function(args) stdNum.Abs(args(Scan0))},
            {"acos", Function(args) stdNum.Acos(args(Scan0))},
            {"asin", Function(args) stdNum.Asin(args(Scan0))},
            {"atan", Function(args) stdNum.Atan(args(Scan0))},
            {"atan2", Function(args) stdNum.Atan2(args(Scan0), args(1))},
            {"bigmul", Function(args) stdNum.BigMul(args(Scan0), args(1))},
            {"ceiling", Function(args) stdNum.Ceiling(args(Scan0))},
            {"cos", Function(args) stdNum.Cos(args(Scan0))},
            {"cosh", Function(args) stdNum.Cosh(args(Scan0))},
            {"exp", Function(args) stdNum.Exp(args(Scan0))},
            {"floor", Function(args) stdNum.Floor(args(Scan0))},
            {"ieeeremainder", Function(args) stdNum.IEEERemainder(args(Scan0), args(1))},
            {"log", Function(args) stdNum.Log(args(Scan0), newBase:=args(1))},
            {"ln", Function(args) stdNum.Log(args(Scan0))},
            {"log10", Function(args) stdNum.Log10(args(Scan0))},
            {"max", Function(args) stdNum.Max(args(Scan0), args(1))},
            {"min", Function(args) stdNum.Min(args(Scan0), args(1))},
            {"pow", Function(args) stdNum.Pow(args(Scan0), args(1))},
            {"round", Function(args) stdNum.Round(args(Scan0))},
            {"sign", Function(args) stdNum.Sign(args(Scan0))},
            {"sin", Function(args) stdNum.Sin(args(Scan0))},
            {"sinh", Function(args) stdNum.Sinh(args(Scan0))},
            {"sqrt", Function(args) stdNum.Sqrt(args(Scan0))},
            {"tan", Function(args) stdNum.Tan(args(Scan0))},
            {"tanh", Function(args) stdNum.Tanh(args(Scan0))},
            {"truncate", Function(args) stdNum.Truncate(args(Scan0))},
            {"rnd", Function(args) Arithmetic.RND(args(Scan0), args(1))},
            {"int", Function(args) CType(args(Scan0), Integer)}
    }

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="name">函数名</param>
    Public Function AddFunction(name As String, lambda As String) As ExpressionEngine

    End Function

    Public Function GetSymbolValue(name As String) As Double
        Return symbols(name)
    End Function

    Public Function GetFunction(name As String) As Func(Of Double(), Double)
        Return SystemFunctions(name)
    End Function

    Public Function SetSymbol(symbol As String, value As Double) As ExpressionEngine
        symbols(symbol) = value
        Return Me
    End Function

    Public Function Evaluate(expression As String) As Double
        Dim tokens As MathToken() = New ExpressionTokenIcer(expression).GetTokens.ToArray
        Dim exp As Expression = ExpressionBuilder.BuildExpression(tokens)
        Dim result As Double = exp.Evaluate(Me)

        Return result
    End Function
End Class
