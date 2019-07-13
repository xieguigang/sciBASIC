#Region "Microsoft.VisualBasic::1ff49d6a4003e4c1f8027636338f4e85, Data_science\Mathematica\Math\Math\test\Module1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Sub: lppTest, Main, Ptest, vectorAPItest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Algebra.LinearProgramming
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub lppTest()
        Dim lpModel As New LPP("Min",
                              variableNames:=New String() {"x", "y", "z"},
                             objectiveFunctionCoefficients:=New Double() {-2, -3, -4},
                            constraintCoefficients:=New Double()() {
                                  New Double() {3, 2, 1},
                                  New Double() {2, 5, 3}
                              },
                            constraintTypes:=New String() {"<=", "<="},
                             constraintRightHandSides:=New Double() {10, 15},
                              objectiveFunctionValue:=0)

        Console.WriteLine("Print model")
        Console.WriteLine(lpModel.ToString())

        Console.WriteLine("Print result")
        Dim solved As LPPSolution = lpModel.solve()
        Console.WriteLine(solved.ToString())
        Console.WriteLine(solved.constraintSensitivityString())
        Console.WriteLine(solved.SolutionLog)

        Console.ReadLine()
    End Sub

    Sub Ptest()
        Dim x = 1.25
        Dim p = Distributions.AboveStandardDistribution(x, 10000000, 0, 1)

        Call p.__DEBUG_ECHO

        Pause()
    End Sub

    Sub Main()

        Call Ptest()

        Call vectorAPItest()
    End Sub

    Sub vectorAPItest()
        Dim x As Vector = {97, 93, 85, 74, 32, 100, 99, 67}
        Dim order = x.Order

        ' 5 8 4 3 2 1 7 6
        Call order.GetJson.__DEBUG_ECHO

        Pause()
    End Sub
End Module
