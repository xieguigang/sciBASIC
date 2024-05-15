#Region "Microsoft.VisualBasic::a382f745cf46239b09c70d5f7b2e6336, Data_science\MachineLearning\MachineLearning\Darwinism\Models\Chromosome.vb"

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

    '   Total Lines: 109
    '    Code Lines: 8
    ' Comment Lines: 95
    '   Blank Lines: 6
    '     File Size: 6.68 KB


    '     Interface Chromosome
    ' 
    '         Properties: MutationRate
    ' 
    '         Function: Crossover, Mutate
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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Darwinism.Models

    ''' <summary>
    ''' In computer programming, genetic representation is a way of representing 
    ''' solutions/individuals in evolutionary computation methods. Genetic 
    ''' representation can encode appearance, behavior, physical qualities of 
    ''' individuals. Designing a good genetic representation that is expressive 
    ''' and evolvable is a hard problem in evolutionary computation. Difference 
    ''' in genetic representations is one of the major criteria drawing a line 
    ''' between known classes of evolutionary computation.
    '''
    ''' Terminology often comes by analogy With natural genetics. The block Of 
    ''' computer memory that represents one candidate solution Is called an individual. 
    ''' The data In that block Is called a chromosome. Each chromosome consists Of 
    ''' genes. The possible values Of a particular gene are called alleles. A 
    ''' programmer may represent all the individuals Of a population Using binary 
    ''' encoding, permutational encoding, encoding by tree, Or any one Of several 
    ''' other representations.
    '''
    ''' Genetic algorithms use linear binary representations. The most standard one 
    ''' Is an array Of bits. Arrays Of other types And structures can be used In 
    ''' essentially the same way. The main Property that makes these genetic 
    ''' representations convenient Is that their parts are easily aligned due To 
    ''' their fixed size. This facilitates simple crossover operation. Variable 
    ''' length representations were also explored In Genetic algorithms, but crossover 
    ''' implementation Is more complex In this Case.
    '''
    ''' Evolution strategy uses linear real-valued representations, e.g. an array 
    ''' Of real values. It uses mostly gaussian mutation And blending/averaging 
    ''' crossover.
    '''
    ''' Genetic programming(GP) pioneered tree-Like representations And developed 
    ''' genetic operators suitable For such representations. Tree-Like representations 
    ''' are used In GP To represent And evolve functional programs With desired 
    ''' properties.
    '''
    ''' Human-based genetic algorithm (HBGA) offers a way to avoid solving hard 
    ''' representation problems by outsourcing all genetic operators to outside 
    ''' agents, in this case, humans. The algorithm has no need for knowledge 
    ''' of a particular fixed genetic representation as long as there are enough 
    ''' external agents capable of handling those representations, allowing for 
    ''' free-form And evolving genetic representations.
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Interface Chromosome(Of Chr As {Class, Chromosome(Of Chr)}) : Inherits IReadOnlyId

        ''' <summary>
        ''' 突变的变异程度，这个值应该是位于(0, 1)闭区间内的
        ''' </summary>
        ''' <returns></returns>
        Property MutationRate As Double

        ''' <summary>
        ''' In genetic algorithms, crossover is a genetic operator used to vary the programming 
        ''' of a chromosome or chromosomes from one generation to the next. It is analogous to 
        ''' reproduction and biological crossover, upon which genetic algorithms are based. 
        ''' Cross over is a process of taking more than one parent solutions and producing a 
        ''' child solution from them. There are methods for selection of the chromosomes.
        ''' </summary>
        ''' <param name="another">The another chromosome.</param>
        ''' <returns></returns>
        Function Crossover(another As Chr) As IEnumerable(Of Chr)

        ''' <summary>
        ''' Mutation is a genetic operator used to maintain genetic diversity from one generation 
        ''' of a population of genetic algorithm chromosomes to the next. It is analogous to 
        ''' biological mutation. Mutation alters one or more gene values in a chromosome from its 
        ''' initial state. In mutation, the solution may change entirely from the previous solution. 
        ''' Hence GA can come to better solution by using mutation. Mutation occurs during evolution 
        ''' according to a user-definable mutation probability. This probability should be set low. 
        ''' If it is set too high, the search will turn into a primitive random search.
        '''
        ''' The classic example Of a mutation Operator involves a probability that an arbitrary bit 
        ''' In a genetic sequence will be changed from its original state. A common method Of 
        ''' implementing the mutation Operator involves generating a random variable For Each bit 
        ''' In a sequence. This random variable tells whether Or Not a particular bit will be modified. 
        ''' This mutation procedure, based On the biological point mutation, Is called Single point 
        ''' mutation. Other types are inversion And floating point mutation. When the gene encoding 
        ''' Is restrictive As In permutation problems, mutations are swaps, inversions, And scrambles.
        '''
        ''' The purpose Of mutation In GAs Is preserving And introducing diversity. Mutation should 
        ''' allow the algorithm To avoid local minima by preventing the population Of chromosomes 
        ''' from becoming too similar To Each other, thus slowing Or even stopping evolution. This 
        ''' reasoning also explains the fact that most GA systems avoid only taking the fittest Of 
        ''' the population In generating the Next but rather a random (Or semi-random) selection 
        ''' With a weighting toward those that are fitter.
        ''' </summary>
        ''' <returns></returns>
        Function Mutate() As Chr
    End Interface
End Namespace
