#Region "Microsoft.VisualBasic::7084181b314ed199dab69445ddc1fa86, Data_science\Darwinism\NonlinearGrid\TopologyInference\GA\Environment.vb"

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

    ' Class Environment
    ' 
    '     Properties: Cacheable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Calculate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Environment : Implements Fitness(Of Genome)

    Dim matrix As (status As Vector, target As Double, targetID$)()

    Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of Genome).Cacheable
        Get
            Return False
        End Get
    End Property

    Sub New(trainingSet As IEnumerable(Of Sample))
        matrix = trainingSet _
            .Select(Function(sample)
                        Return (
                            sample.status.vector.AsVector,
                            sample.target(Scan0),
                            sample.target(Scan0).ToString
                        )
                    End Function) _
            .ToArray
    End Sub

    Public Function Calculate(chromosome As Genome) As Double Implements Fitness(Of Genome).Calculate
        ' 理论上是应该使用MAX err来作为fitness的
        ' 但是在最开始的时候,因为整个系统的大部分样本的计算结果误差都是Inf
        ' 所以使用MAX来作为fitness的话,会因为结果都是Inf而导致前期没有办法收敛
        ' 在这里应该是使用平均值来避免这个问题
        Return matrix _
            .Select(Function(sample)
                        Dim err = chromosome.CalculateError(sample.status, sample.target)

                        Return (errors:=err, id:=sample.targetID)
                    End Function) _
            .GroupBy(Function(g) g.id) _
            .Select(Function(g) g.Select(Function(s) s.errors).AverageError) _
            .Average
    End Function
End Class
