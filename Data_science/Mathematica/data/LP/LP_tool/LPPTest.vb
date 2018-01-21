Imports Microsoft.VisualBasic.Math.Algebra.LinearProgramming
''' <summary>
''' https://github.com/gthole/lpp
''' </summary>
Public Module LPPTest

    ' TODO: Fill in meta information beyond ObjectiveFunctionValue (constraint sensitivity, etc.)

    Sub main()
        Call testSolveMaximizeExample()
        Call testSolveMinimizeExample()
        Call testSolveSmallMinimizeExample()
        Call testSolveStrictEqualityExample()
        Call testSolveTransshipmentExample()

        Call Main2(Nothing)

        Console.ReadLine()
    End Sub

    Public Sub Main2(args() As String)
        Dim lpp As LPP = LPPExamples.transshipment()
        Console.WriteLine(lpp.ToString())

        Dim solved As LPPSolution = lpp.solve()
        Console.WriteLine(solved.ToString())
        Console.WriteLine(solved.constraintSensitivityString())
        Console.WriteLine(solved.SolutionLog)


    End Sub

    Public Sub testSolveTransshipmentExample()

        Dim lpp As LPP = LPPExamples.transshipment()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 10043, solved.ObjectiveFunctionValue, 0.0001)

    End Sub

    Public Sub testSolveMinimizeExample()

        Dim lpp As LPP = LPPExamples.minimizeExample()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 2.625, solved.ObjectiveFunctionValue, 0.0001)
    End Sub

    Public Sub testSolveSmallMinimizeExample()
        Dim lpp As LPP = LPPExamples.smallMinimizeExample()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 57, solved.ObjectiveFunctionValue, 0.0001)

    End Sub

    Public Sub testSolveMaximizeExample()
        Dim lpp As LPP = LPPExamples.maximizeExample()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 70, solved.ObjectiveFunctionValue, 0.0001)
    End Sub


    Public Sub testSolveStrictEqualityExample()

        Dim lpp As LPP = LPPExamples.strictEquality()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 55, solved.ObjectiveFunctionValue, 0.0001)

    End Sub

    Sub assertEquals(text$, x#, y#, esp#)
        If Math.Abs(x - y) <= esp Then
            Call Console.WriteLine($"{text} test success")
        Else
            Throw New Exception(text)
        End If
    End Sub
End Module
