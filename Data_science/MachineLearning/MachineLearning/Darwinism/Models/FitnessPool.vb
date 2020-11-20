#Region "Microsoft.VisualBasic::8450d02c5fe4c4f80f81a9880e0f8dc0, Data_science\MachineLearning\MachineLearning\Darwinism\Models\FitnessPool.vb"

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

    '     Class FitnessPool
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
    Public Class FitnessPool(Of Individual) : Implements Fitness(Of Individual)

        ''' <summary>
        ''' Get unique id of each genome
        ''' </summary>
        Protected Friend indivToString As Func(Of Individual, String)
        Protected Friend maxCapacity%
        Protected Friend ReadOnly cache As New Dictionary(Of String, Double)

        Friend Shared ReadOnly defaultCacheSize As [Default](Of Integer) = 10000

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of Individual).Cacheable
            Get
                Return evaluateFitness.Cacheable
            End Get
        End Property

        Public ReadOnly Property evaluateFitness As Fitness(Of Individual)

        ''' <summary>
        ''' 因为这个缓存对象是默认通过``ToString``方法来生成键名的，所以假设<paramref name="toString"/>参数是空值的话，则必须要重写
        ''' 目标<typeparamref name="Individual"/>的``ToString``方法
        ''' </summary>
        ''' <param name="cacl">Expression for descript how to calculate the fitness.</param>
        ''' <param name="toString">Obj to dictionary key</param>
        Sub New(cacl As Fitness(Of Individual), capacity%, toString As Func(Of Individual, String))
            evaluateFitness = cacl
            indivToString = toString
            maxCapacity = capacity Or defaultCacheSize

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
            Dim key$ = indivToString([in])
            Dim fit As Double

            SyncLock cache
                If cache.ContainsKey(key$) Then
                    fit = cache(key$)
                Else
                    fit = evaluateFitness.Calculate([in], parallel)
                    cache.Add(key$, fit)

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
