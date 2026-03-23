#Region "Microsoft.VisualBasic::e10675fba2ca4afdb44b1e5540e67ea1, Data_science\Graph\KNN\HNSW\ReverseComparer.vb"

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

    '   Total Lines: 52
    '    Code Lines: 20 (38.46%)
    ' Comment Lines: 25 (48.08%)
    '    - Xml Docs: 84.00%
    ' 
    '   Blank Lines: 7 (13.46%)
    '     File Size: 2.06 KB


    '     Class ReverseComparer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Compare
    ' 
    '     Module ReverseComparerExtensions
    ' 
    '         Function: Reverse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        Public Shared ReadOnly [Default] As New ReverseComparer(Of T)(Generic.Comparer(Of T).Default)

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

