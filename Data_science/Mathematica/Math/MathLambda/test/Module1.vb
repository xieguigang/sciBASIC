Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Lambda
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Module Module1

    Sub Main()
        Call mathLambdaTest()
        Call complexExpressionTest()
    End Sub

    Sub mathLambdaTest()
        Dim exp = New ExpressionTokenIcer("(x+y+z) ^ 2").GetTokens.ToArray.DoCall(AddressOf BuildExpression)
        Dim lambda = ExpressionCompiler.CreateLambda({"x", "y", "z"}, exp)
        Dim fun As Func(Of Double, Double, Double, Double) = lambda.Compile

        Console.WriteLine(fun(1, 1, 1))

        Pause()
    End Sub

    Sub complexExpressionTest()
        Dim test As String = "E:\GCModeller\src\runtime\sciBASIC#\mime\etc\kinetics2.xml"
        Dim exp As LambdaExpression = LambdaExpression.FromMathML(test.ReadAllText)
        Dim func = MathMLCompiler.CreateLambda(exp)
        Dim del = func.Compile()

        Console.WriteLine(func)
        ' Console.WriteLine(del(1, 0, 0.1, 0.01, 99, 10))

        Pause()
    End Sub

    Sub simpleExpressionTest()
        Dim test As String = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\MathLambda\mathML.xml"
        Dim exp As LambdaExpression = LambdaExpression.FromMathML(test.ReadAllText)
        Dim func = MathMLCompiler.CreateLambda(exp)
        Dim del As Func(Of Double, Double, Double, Double, Double, Double, Double) = func.Compile()

        Console.WriteLine(func)
        Console.WriteLine(del(1, 0, 0.1, 0.01, 99, 10))

        Pause()
    End Sub

End Module
