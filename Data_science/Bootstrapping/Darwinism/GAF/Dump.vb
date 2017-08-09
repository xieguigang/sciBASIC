#Region "Microsoft.VisualBasic::e7b458bd83a2359fe938ebd6be0b08a7, ..\sciBASIC#\Data_science\Bootstrapping\Darwinism\GAF\Dump.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Text

Namespace Darwinism.GAF

    Public Class Dump
        Implements IterartionListener(Of ParameterVector)

        Public model As Type
        Public n%, a%, b%
        Public y0 As Dictionary(Of String, Double)

        Dim i As New Uid(caseSensitive:=False)

        Public Sub Update(environment As GeneticAlgorithm(Of ParameterVector)) Implements IterartionListener(Of ParameterVector).Update
            Dim best As ParameterVector = environment.Best
            Dim vars As Dictionary(Of String, Double) =
                best _
                   .vars _
                   .ToDictionary(Function(var) var.Name,
                                 Function(var) var.value)
            Dim out As ODEsOut =
                MonteCarlo.Model.RunTest(model, y0, vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim path = App.CurrentProcessTemp & $"\{GetHashCode()}\debug_{+i}.csv"

            Call out.DataFrame("#TIME").Save(path$, Encodings.ASCII)
        End Sub
    End Class
End Namespace
