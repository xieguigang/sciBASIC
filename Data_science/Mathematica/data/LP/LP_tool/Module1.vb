#Region "Microsoft.VisualBasic::b1fd2c65eadb4f84b53cba4be6929ccc, Data_science\Mathematica\data\LP\LP_tool\Module1.vb"

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
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Algebra.LinearProgramming

Module Module1

    ''' <summary>
    ''' https://en.wikipedia.org/wiki/Simplex_algorithm
    ''' </summary>
    Sub Main()
        Dim lpModel As New LPP(
            OptimizationType.MIN,
            variableNames:={"x", "y", "z"},
            objectiveFunctionCoefficients:={-2, -3, -4},
            constraintCoefficients:={
                {3, 2, 1},
                {2, 5, 3}
            },
            constraintTypes:={"<=", "<="},
            constraintRightHandSides:={10, 15},
            objectiveFunctionValue:=0
        )

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
