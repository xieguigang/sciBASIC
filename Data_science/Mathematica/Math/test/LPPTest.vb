#Region "Microsoft.VisualBasic::b68fae1c46600e6bdc98cf14d449908b, sciBASIC#\Data_science\Mathematica\Math\test\LPPTest.vb"

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

    '   Total Lines: 109
    '    Code Lines: 59
    ' Comment Lines: 21
    '   Blank Lines: 29
    '     File Size: 3.39 KB


    ' Module LPPTest
    ' 
    '     Sub: assertEquals, main, Main2, testR, testSolveMaximizeExample
    '          testSolveMinimizeExample, testSolveSmallMinimizeExample, testSolveStrictEqualityExample, testSolveTransshipmentExample
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming

''' <summary>
''' https://github.com/gthole/lpp
''' </summary>
Public Module LPPTest

    ' TODO: Fill in meta information beyond ObjectiveFunctionValue (constraint sensitivity, etc.)

    Sub main()

        Call testR()

        Call testSolveMaximizeExample()
        Call testSolveMinimizeExample()
        Call testSolveSmallMinimizeExample()
        Call testSolveStrictEqualityExample()
        Call testSolveTransshipmentExample()

        Call Main2(Nothing)

        Console.ReadLine()
    End Sub

    Public Sub testR()

        '# Set up problem: maximize
        '# x1 + 9 x2 + x3 subject To
        '# x1 + 2 x2 + 3 x3 = 9
        '# 3 x1 + 2 x2 + 2 x3 = 15

        '> f.obj <- c(1, 9, 1)
        '> f.con <- matrix (c(1, 2, 3, 3, 2, 2), nrow=2, byrow=TRUE)
        '> f.dir <- c("=", "=")
        '> f.rhs <- c(9, 15)
        '> require(lpSolve)
        'Loading required package: lpSolve
        '> lp ("max", f.obj, f.con, f.dir, f.rhs)
        'Success: the objective function Is 30
        '> lp ("max", f.obj, f.con, f.dir, f.rhs)$solution
        '[1] 3 3 0
        '> 3+3*9+0
        '[1] 30
        '>
        Dim lp As New LPP(OptimizationType.MAX, {"A", "B", "C"}, {1, 9, 1}, {{1, 2, 3}, {3, 2, 2}}, {"=", "="}, {0, 0})
        Dim result As LPPSolution = lp.solve

        Call result.ToString.debug

        Pause()
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
