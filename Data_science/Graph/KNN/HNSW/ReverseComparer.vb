' <copyright file="ReverseComparer.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System.Runtime.CompilerServices

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Reverses the order of the nested comparer.
    ''' </summary>
    ''' <typeparam name="T">The types of items to comapre.</typeparam>
    Public Class ReverseComparer(Of T)
        Implements IComparer(Of T)
        ''' <summary>
        ''' Gets a default sort order comparer for the type specified by the generic argument.
        ''' </summary>
        Public Shared ReadOnly [Default] As ReverseComparer(Of T) = New ReverseComparer(Of T)(Generic.Comparer(Of T).Default)

        Private ReadOnly comparer As IComparer(Of T) = [Default]

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ReverseComparer(Of T)"/> class.
        ''' </summary>
        ''' <param name="comparer">The comparer to invert.</param>
        Public Sub New(comparer As IComparer(Of T))
            Me.comparer = comparer
        End Sub

        ''' <inheritdoc/>
        Public Function Compare(x As T, y As T) As Integer Implements IComparer(Of T).Compare
            Return comparer.Compare(y, x)
        End Function
    End Class

    ''' <summary>
    ''' Extension methods to shortcut <see cref="ReverseComparer(Of T)"/> usage.
    ''' </summary>
    Public Module ReverseComparerExtensions
        ''' <summary>
        ''' Creates new <see cref="ReverseComparer(Of T)"/> wrapper for the given comparer.
        ''' </summary>
        ''' <typeparam name="T">The types of items to comapre.</typeparam>
        ''' <param name="comparer">The source comparer.</param>
        ''' <returns>The inverted to source comparer.</returns>
        <Extension()>
        Public Function Reverse(Of T)(comparer As IComparer(Of T)) As ReverseComparer(Of T)
            Return New ReverseComparer(Of T)(comparer)
        End Function
    End Module
End Namespace
