#Region "Microsoft.VisualBasic::c3026202b75479261d80b20e0784f498, Data_science\Mathematica\Math\MathLambda\test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 47
    '    Code Lines: 34 (72.34%)
    ' Comment Lines: 1 (2.13%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (25.53%)
    '     File Size: 1.64 KB


    ' Module Module1
    ' 
    '     Sub: complexExpressionTest, Main, mathLambdaTest, simpleExpressionTest
    ' 
    ' /********************************************************************************/

#End Region

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
