Imports Microsoft.VisualBasic.Math.Lambda
Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Module Module1

    Sub Main()
        Call complexExpressionTest()
    End Sub

    Sub complexExpressionTest()
        Dim test As String = "E:\GCModeller\src\runtime\sciBASIC#\mime\etc\kinetics2.xml"
        Dim exp As LambdaExpression = LambdaExpression.FromMathML(test.ReadAllText)
        Dim func = Compiler.CreateLambda(exp)
        Dim del = func.Compile()

        Console.WriteLine(func)
        ' Console.WriteLine(del(1, 0, 0.1, 0.01, 99, 10))

        Pause()
    End Sub

    Sub simpleExpressionTest()
        Dim test As String = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\MathLambda\mathML.xml"
        Dim exp As LambdaExpression = LambdaExpression.FromMathML(test.ReadAllText)
        Dim func = Compiler.CreateLambda(exp)
        Dim del As Func(Of Double, Double, Double, Double, Double, Double, Double) = func.Compile()

        Console.WriteLine(func)
        Console.WriteLine(del(1, 0, 0.1, 0.01, 99, 10))

        Pause()
    End Sub

End Module
