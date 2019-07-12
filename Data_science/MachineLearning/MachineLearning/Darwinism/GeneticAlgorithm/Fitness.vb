#Region "Microsoft.VisualBasic::bc3de2c6bbb70d9a3900f4ecefd4ec3f, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Fitness.vb"

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

    '     Interface Fitness
    ' 
    '         Properties: Cacheable
    ' 
    '         Function: Calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' *****************************************************************************
' Copyright 2012 Yuriy Lagodiuk
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    ''' <summary>
    ''' A function wrapper for calculate genome fitness in current environment.
    ''' 
    ''' (描述了如何从将目标染色体计算为fitness，从而能够量化突变带来的的优点)
    ''' </summary>
    ''' <typeparam name="Chr">
    ''' 这个泛型类型应该是集成至<see cref="Chromosome(Of Chr)"/>,但是为了兼容``DifferentialEvolution``
    ''' 模块计算函数的类型约束,在这里就不添加约束了
    ''' </typeparam>
    Public Interface Fitness(Of Chr)

        ''' <summary>
        ''' 这个计算模块是否会缓存计算结果?
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Cacheable As Boolean

        ''' <summary>
        ''' Assume that ``chromosome1`` is better than ``chromosome2``
        ''' 
        ''' ```vbnet
        ''' fit1 = calculate(chromosome1)
        ''' fit2 = calculate(chromosome2)
        ''' ```
        ''' 
        ''' So the following condition must be true:
        ''' 
        ''' ```vbnet
        ''' fit1.compareTo(fit2) &lt;= 0
        ''' ```
        ''' 
        ''' (假若是并行模式的之下，还要求这个函数是线程安全的)
        ''' </summary>
        Function Calculate(chromosome As Chr) As Double
    End Interface
End Namespace
