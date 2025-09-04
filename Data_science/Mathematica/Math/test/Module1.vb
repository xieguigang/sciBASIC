#Region "Microsoft.VisualBasic::a3347ced0506ff8a5d8d3b71d4822413, sciBASIC#\Data_science\Mathematica\Math\test\Module1.vb"

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

    '   Total Lines: 57
    '    Code Lines: 42
    ' Comment Lines: 1
    '   Blank Lines: 14
    '     File Size: 1.76 KB


    ' Module Module1
    ' 
    '     Sub: lppTest, Main, Ptest, vectorAPItest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
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

        Call p.debug

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
        Call order.GetJson.debug

        Pause()
    End Sub
End Module
