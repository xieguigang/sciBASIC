#Region "Microsoft.VisualBasic::737b9a7ab097fdd23295973e1bf844c9, Data_science\MachineLearning\MachineLearning\Darwinism\Models\GeneralFitnessPool.vb"

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

    '   Total Lines: 113
    '    Code Lines: 69 (61.06%)
    ' Comment Lines: 25 (22.12%)
    '    - Xml Docs: 84.00%
    ' 
    '   Blank Lines: 19 (16.81%)
    '     File Size: 4.23 KB


    '     Class GeneralFitnessPool
    ' 
    '         Properties: Cacheable, evaluateFitness
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Fitness, getOrCacheOfFitness
    ' 
    '         Sub: Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF

Namespace Darwinism.Models

    ''' <summary>
    ''' Compute fitness and cache the result data in this pool.
    ''' </summary>
    ''' <typeparam name="Individual"></typeparam>
    ''' <remarks>
    ''' works for genetic algorithm and <see cref="DifferentialEvolution"/>
    ''' </remarks>
    Public Class GeneralFitnessPool(Of Individual) : Implements Fitness(Of Individual)

        Protected Friend maxCapacity%
        ''' <summary>
        ''' A fitness cache pool indexed via the unique id of target
        ''' </summary>
        Protected Friend ReadOnly cache As New Dictionary(Of String, Double)

        Dim istr As Func(Of Individual, String)

        Friend Shared ReadOnly defaultCacheSize As [Default](Of Integer) = 10000

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of Individual).Cacheable
            Get
                Return evaluateFitness.Cacheable
            End Get
        End Property

        Public ReadOnly Property evaluateFitness As Fitness(Of Individual)

        ''' <summary>
        ''' </summary>
        ''' <param name="cacl">Expression for descript how to calculate the fitness.</param>
        Sub New(cacl As Fitness(Of Individual), capacity%, toString As Func(Of Individual, String))
            evaluateFitness = cacl
            maxCapacity = capacity Or defaultCacheSize
            istr = toString

            If capacity <= 0 AndAlso cacl.Cacheable Then
                Call $"Target environment marked as cacheable, but cache size is invalid...".Warning
                Call $"Use default cache size for fitness: {defaultCacheSize.DefaultValue}".__INFO_ECHO
            ElseIf cacl.Cacheable Then
                Call $"Fitness was marked as cacheable with cache table size {capacity}".__INFO_ECHO
            End If
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' This function tells how well given individual performs at given problem.
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function Fitness([in] As Individual, parallel As Boolean) As Double Implements Fitness(Of Individual).Calculate
            ' 20200827
            ' the synlock will stop the parallel computing in GA engine
            'SyncLock evaluateFitness
            If Not evaluateFitness.Cacheable Then
                Return evaluateFitness.Calculate([in], parallel)
            Else
                Return getOrCacheOfFitness([in], parallel)
            End If
            'End SyncLock
        End Function

        Private Function getOrCacheOfFitness([in] As Individual, parallel As Boolean) As Double
            Dim key As String = istr([in])
            Dim fit As Double

            SyncLock cache
                If cache.ContainsKey(key$) Then
                    Return cache(key$)
                End If
            End SyncLock

            fit = evaluateFitness.Calculate([in], parallel)

            SyncLock cache
                cache(key$) = fit

                If cache.Count >= maxCapacity Then
                    Dim asc = From fitValue
                              In cache
                              Order By fitValue.Value Ascending
                              Take CInt(maxCapacity * 0.9)
                              Select ID = fitValue.Key

                    With asc.Indexing
                        For Each key In cache.Keys.ToArray
                            If .NotExists(key) Then
                                Call cache.Remove(key)
                            End If
                        Next
                    End With
                End If
            End SyncLock

            Return fit
        End Function

        ''' <summary>
        ''' Clear all cache data.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear()
            Call cache.Clear()
        End Sub
    End Class
End Namespace
