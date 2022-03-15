#Region "Microsoft.VisualBasic::59ea8d660ffcb6907c7ba949c82b16f1, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\ODEs\Dump.vb"

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

    '   Total Lines: 29
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.13 KB


    '     Class Dump
    ' 
    '         Sub: Update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Text

Namespace Darwinism.GAF.ODEs

    Public Class Dump

        Public model As Type
        Public n%, a%, b%
        Public y0 As Dictionary(Of String, Double)

        Dim i As New Uid(caseSensitive:=False)

        Public Sub Update(iteration%, fitness#, environment As GeneticAlgorithm(Of ParameterVector))
            Dim best As ParameterVector = environment.Best
            Dim vars As Dictionary(Of String, Double) =
                best _
                   .vars _
                   .ToDictionary(Function(var) var.Name,
                                 Function(var) var.value)
            Dim out As ODEsOut = MonteCarlo.Model.RunTest(model, y0, vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim path = App.CurrentProcessTemp & $"\{GetHashCode()}\debug_{+i}.csv"

            Call out.DataFrame("#TIME").Save(path$, Encodings.ASCII)
        End Sub
    End Class
End Namespace
