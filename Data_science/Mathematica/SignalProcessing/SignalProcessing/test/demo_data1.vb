#Region "Microsoft.VisualBasic::b5f4996d55d5c23c0fc15ff01462fa8c, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\demo_data1.vb"

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

    '   Total Lines: 51
    '    Code Lines: 37 (72.55%)
    ' Comment Lines: 2 (3.92%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (23.53%)
    '     File Size: 1.69 KB


    ' Module demo_data1
    ' 
    '     Sub: fitCurveTest, fitMultipleGauss, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.SignalProcessing.EmGaussian
Imports Microsoft.VisualBasic.Serialization.JSON

Module demo_data1

    Dim v As Double() = {0, 0.1, 0.2, 0.5, 0.9, 1.3, 1.25, 0.99, 0.7, 0.35, 0.4, 0.5, 0.6, 0.65, 0.45, 0.4, 0.35, 0.2, 0.1, 0}

    Sub Main()
        Call fitMultipleGauss()

        ' Call fitCurveTest()
    End Sub

    Sub fitMultipleGauss()
        Dim gauss As New GaussianFit(Opts.GetDefault)
        Dim logp As Double() = Nothing
        Dim result = gauss.fit(v, npeaks:=9)

        For Each peak In result
            Call Console.WriteLine(peak.GetJson)
        Next

        ' Call Console.WriteLine(result.GetJson)
        Call Console.WriteLine(logp.GetJson)

        Pause()
    End Sub

    Sub fitCurveTest()
        Dim data As DataPoint() = v.Select(Function(vi, i) New DataPoint(i, vi)).ToArray
        Dim gauss As GaussNewtonSolver.FitFunction =
            Function(x As Double, args As NumericMatrix) As Double
                Return pnorm.ProbabilityDensity(x, args(0, 0), args(1, 0))
            End Function
        Dim solver As New GaussNewtonSolver(gauss)
        Dim result = solver.Fit(data, 9, 2)
        Dim m = result(0)
        Dim sd = result(1)

        Dim y2 As Double() = v.Select(Function(vi, i) pnorm.ProbabilityDensity(i, m, sd)).ToArray

        Call Console.WriteLine(v.GetJson)
        Call Console.WriteLine(y2.GetJson)

        Pause()
    End Sub
End Module
