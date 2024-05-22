#Region "Microsoft.VisualBasic::bc0775901ddb816844768b0bf5dc3adb, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Substitution\SimpleReplacement.vb"

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

    '   Total Lines: 37
    '    Code Lines: 16 (43.24%)
    ' Comment Lines: 15 (40.54%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 1.57 KB


    '     Structure SimpleReplacement
    ' 
    '         Properties: type
    ' 
    '         Function: newPopulation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population.SubstitutionStrategy

    ''' <summary>
    ''' 最简单的种群更替策略
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Structure SimpleReplacement(Of Chr As {Class, Chromosome(Of Chr)})
        Implements IStrategy(Of Chr)

        Public ReadOnly Property type As Strategies Implements IStrategy(Of Chr).type
            Get
                Return Strategies.Naive
            End Get
        End Property

        ''' <summary>
        ''' 下面的两个步骤是机器学习的关键
        ''' 
        ''' 通过排序,将错误率最小的种群排在前面
        ''' 错误率最大的种群排在后面
        ''' 然后对种群进行裁剪,将错误率比较大的种群删除
        ''' 从而实现了择优进化, 即程序模型对我们的训练数据集产生了学习
        ''' </summary>
        ''' <param name="newPop"></param>
        ''' <param name="GA"></param>
        ''' <returns></returns>
        Public Function newPopulation(newPop As Population(Of Chr), GA As GeneticAlgorithm(Of Chr)) As Population(Of Chr) Implements IStrategy(Of Chr).newPopulation
            Call newPop.SortPopulationByFitness(GA.chromosomesComparator) ' 通过fitness排序来进行择优
            Call newPop.Trim(newPop.capacitySize)                         ' 剪裁掉后面的对象，达到淘汰的效果

            Return newPop
        End Function
    End Structure

End Namespace
