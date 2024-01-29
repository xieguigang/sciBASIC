Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population

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
                             Return fitness(c.Identity)
                         End Function) _
                .AsList
        End Sub
    End Class
End Namespace