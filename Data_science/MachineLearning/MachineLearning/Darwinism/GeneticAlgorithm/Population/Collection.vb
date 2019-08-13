Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Serialization

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
            innerList = innerList.SubList(0, capacitySize)
        End Sub

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