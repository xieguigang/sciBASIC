Imports Microsoft.VisualBasic.Math.Algebra.LinearProgramming

Module Module1
    Sub Main()
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
End Module
