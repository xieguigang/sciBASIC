#Region "Microsoft.VisualBasic::64103c6b88a2c25c029928323655c5fa, sciBASIC#\Data_science\Mathematica\data\least-squares\test2\test\weightedFit.vb"

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
    '    Code Lines: 11
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 644.00 B


    ' Module weightedFitTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping

Module weightedFitTest

    Sub Main()
        Dim X#() = {0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.82}
        Dim Y#() = {4.09818, 4.39655, 4.61435, 4.95867, 5.26182, 5.55079, 5.84748, 6.11208, 6.5333333}
        Dim W#() = {1, 1, 1, 1, 1, 0.65, 0.1, 0.0023, 0.0000002}

        ' W = 1 / (X.AsVector ^ 2)

        ' Dim fit1 As WeightedFit = WeightedLinearRegression.Regress(X, Y, W, 1)
        Dim fit2 As WeightedFit = WeightedLinearRegression.Regress(X, Y, W, 5)

        '  Console.WriteLine(fit1)
        Console.WriteLine(fit2)

        Pause()
    End Sub
End Module
