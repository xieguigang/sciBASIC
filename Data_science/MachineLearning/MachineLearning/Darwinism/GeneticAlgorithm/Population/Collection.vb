#Region "Microsoft.VisualBasic::73f96ac5135174c8d69454d22763ec6a, sciBASIC#\Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Collection.vb"

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

    '   Total Lines: 74
    '    Code Lines: 51
    ' Comment Lines: 8
    '   Blank Lines: 15
    '     File Size: 2.70 KB


    '     Class PopulationCollection
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class PopulationList
    ' 
    '         Properties: Count
    ' 
    '         Function: GetCollection
    ' 
    '         Sub: Add, OrderBy, Trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    Public MustInherit Class PopulationCollection(Of Chr As {Class, Chromosome(Of Chr)})

        Public MustOverride ReadOnly Property Count As Integer
        Default Public MustOverride ReadOnly Property Item(index As Integer) As Chr

        Protected Sub New()
        End Sub

        Public MustOverride Sub Add(chr As Chr)
        Public MustOverride Sub Trim(capacitySize As Integer)
        ''' <summary>
        ''' 按照fitness进行升序排序,fitness越小,排在越前面
        ''' </summary>
        ''' <param name="fitness"></param>
        Public MustOverride Sub OrderBy(fitness As Func(Of String, Double))
        Public MustOverride Function GetCollection() As IEnumerable(Of Chr)

    End Class

    Public Class PopulationList(Of Chr As {Class, Chromosome(Of Chr)}) : Inherits PopulationCollection(Of Chr)

        Const DEFAULT_NUMBER_OF_CHROMOSOMES% = 32

        Dim innerList As New List(Of Chr)(capacity:=DEFAULT_NUMBER_OF_CHROMOSOMES)

        Public Overrides ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return innerList.Count
            End Get
        End Property

        Default Public Overrides ReadOnly Property Item(index As Integer) As Chr
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return innerList(index)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Add(chr As Chr)
            Call innerList.Add(chr)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Trim(capacitySize As Integer)
            innerList = innerList.subList(0, capacitySize)
        End Sub

        Public Overrides Function GetCollection() As IEnumerable(Of Chr)
            Return innerList
        End Function

        ''' <summary>
        ''' Order by [unique_hashKey => fitness]
        ''' </summary>
        ''' <param name="fitness"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub OrderBy(fitness As Func(Of String, Double))
            innerList = innerList _
                .OrderBy(Function(c)
                             Return fitness(c.UniqueHashKey)
                         End Function) _
                .AsList
        End Sub
    End Class
End Namespace
