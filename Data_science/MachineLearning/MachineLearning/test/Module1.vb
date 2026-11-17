#Region "Microsoft.VisualBasic::f7103ad0f91c170c4c2738409d417229, Data_science\MachineLearning\MachineLearning\test\Module1.vb"

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

    '   Total Lines: 20
    '    Code Lines: 14 (70.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (30.00%)
    '     File Size: 459 B


    ' Module Module1
    ' 
    '     Sub: functionParserTest, Main3
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations

Module Module1

    Sub Main3()
        Call functionParserTest()
    End Sub

    Sub functionParserTest()

        Dim func As New BipolarSigmoid(alpha:=3)
        Dim str = func.ToString
        Dim model = ActiveFunction.Parse(str)
        Dim func2 As BipolarSigmoid = model

        Pause()
    End Sub

    ''' <summary>
    ''' Verify the round trip of the activation function text expression:
    ''' object -> text -> expression tree -> data model -> object.
    ''' </summary>
    Public Sub activeFunctionParseTest()
        Dim reportDirectory As String = Path.Combine(AppContext.BaseDirectory, "report")

        If Not Directory.Exists(reportDirectory) Then
            Call Directory.CreateDirectory(reportDirectory)
        End If

        Call DebugAudit.Reset()
        Call DebugAudit.Mark("active function parse verification started")

        Call VerifyRoundTrip(New BipolarSigmoid(alpha:=3), "alpha", 3.0R)
        Call VerifyRoundTrip(New Sigmoid(alpha:=2), "alpha", 2.0R)
        Call VerifyRoundTrip(New Elu(alpha:=1), "alpha", 1.0R)
        Call VerifyRoundTrip(New ReLU)
        Call VerifyRoundTrip(New HyperbolicTangent)
        Call VerifyRoundTrip(New Threshold)
        Call VerifyRoundTrip(New Softplus)
        Call VerifyRoundTrip(New Sinc)
        Call VerifyRoundTrip(New Identical)
        Call VerifyRoundTrip(New QLinear)

        ' named arguments with whitespace and negative values are also valid
        Call VerifyExpression("BipolarSigmoid(alpha := -1.5)", "BipolarSigmoid", "alpha", -1.5R)
        Call VerifyExpression("Sigmoid( alpha:=2, beta:=3 )", "Sigmoid", "alpha", 2.0R)

        ' regression: plain math expressions must still work after the ':=' token change
        Call DebugAudit.Test(ScriptEngine.Expression.Evaluate("2 + 3 * 4") = 14.0R,
                             "plain arithmetic expression regression")
        Call DebugAudit.Test(ScriptEngine.ParseExpression("x^2 + 3*x") IsNot Nothing,
                             "symbolic expression parse regression")

        Call DebugAudit.Mark("active function parse verification completed")

        Dim reportPath As String = Path.Combine(reportDirectory, "active-function-parse-report.txt")
        Dim jsonPath As String = Path.Combine(reportDirectory, "active-function-parse-report.json")

        Call DebugAudit.SaveReport(reportPath)
        Call File.WriteAllText(jsonPath, DebugAudit.ToJson())
        Call Console.WriteLine(DebugAudit.Dump())

        If DebugAudit.FailureCount > 0 Then
            Call Console.WriteLine($"{DebugAudit.FailureCount} assertion(s) FAILED, see {reportPath}")
            Environment.ExitCode = 1
        Else
            Call Console.WriteLine("all activation function parse assertions passed.")
            Call Console.WriteLine($"report: {reportPath}")
        End If
    End Sub

    Private Sub VerifyRoundTrip(func As IActivationFunction,
                                Optional argumentName As String = Nothing,
                                Optional expected As Double = 0.0R)
        Dim text As String = func.ToString

        Call VerifyExpression(text, func.Store.name, argumentName, expected)

        Dim restored As IActivationFunction = ActiveFunction.Parse(text).CreateFunction

        Call DebugAudit.Test(restored IsNot Nothing, $"cannot restore function from {text}")
        Call DebugAudit.Test(restored.ToString = text, $"round trip text mismatch: {text} -> {restored}")
    End Sub

    Private Sub VerifyExpression(text As String, name As String, argumentName As String, expected As Double)
        Dim model As ActiveFunction = ActiveFunction.Parse(text)

        Call DebugAudit.Mark(text, model.name)
        Call DebugAudit.Test(model.name = name, $"function name mismatch for {text}")
        Call DebugAudit.Test(model.Arguments IsNot Nothing, $"null argument list for {text}")

        If argumentName IsNot Nothing Then
            Call DebugAudit.Test(model.HasKey(argumentName), $"missing argument {argumentName} for {text}")
            Call DebugAudit.Test(Math.Abs(model(argumentName) - expected) < 0.0000001R,
                                 $"argument value mismatch for {text}")
        End If
    End Sub

End Module
