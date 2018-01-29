#Region "Microsoft.VisualBasic::747b114ffacaabed52780c263fd5d32e, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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

