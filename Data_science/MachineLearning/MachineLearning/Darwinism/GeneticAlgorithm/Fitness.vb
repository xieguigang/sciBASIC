#Region "Microsoft.VisualBasic::6c05dd4022cf3a90a6bdd4db51a101a2, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Fitness.vb"

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

    '   Total Lines: 70
    '    Code Lines: 7
    ' Comment Lines: 58
    '   Blank Lines: 5
    '     File Size: 2.86 KB


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
        ''' <remarks>
        ''' the result is cached by the unique id of the target chr
        ''' </remarks>
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
        ''' <param name="parallel">
        ''' 在计算函数的内部是否是应该是并行的?
        ''' 
        ''' 应该遵循以下的准则:
        ''' 
        ''' 1. 如果外部调用这个计算函数是并行的,那么这个parallel参数应该设置为false
        ''' 2. 如果是单线程的外部代码调用这个计算函数,那么这个parallel参数可以是true,即在函数的内部实现并行化
        ''' </param>
        ''' <remarks>
        ''' smaller value is better
        ''' </remarks>
        Function Calculate(chromosome As Chr, parallel As Boolean) As Double
    End Interface
End Namespace
