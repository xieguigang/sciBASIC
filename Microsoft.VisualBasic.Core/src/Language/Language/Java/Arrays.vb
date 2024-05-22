#Region "Microsoft.VisualBasic::b12fd4bd45ec42d9f26eafdc45f9be8c, Microsoft.VisualBasic.Core\src\Language\Language\Java\Arrays.vb"

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

    '   Total Lines: 68
    '    Code Lines: 36 (52.94%)
    ' Comment Lines: 24 (35.29%)
    '    - Xml Docs: 95.83%
    ' 
    '   Blank Lines: 8 (11.76%)
    '     File Size: 2.79 KB


    '     Module Arrays
    ' 
    '         Function: copyOfRange, Max, Min, shuffle, subList
    ' 
    '         Sub: fill
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Language.Java

    Public Module Arrays

        ''' <summary>
        ''' fill and set all elements in target array 
        ''' <paramref name="a"/> with a specific value
        ''' <paramref name="val"/>.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="val"></param>
        ''' <remarks>
        ''' this function will not break the given vector its class reference
        ''' </remarks>
        <Extension>
        Public Sub fill(Of T)(<Out> ByRef a As T(), val As T)
            For i% = 0 To a.Length - 1
                a(i%) = val
            Next
        End Sub

        Public Function copyOfRange(Of T)(matrix As T(), start As Integer, length As Integer) As T()
            Dim out As T() = New T(length - 1) {}
            Call Array.Copy(matrix, start, out, Scan0, length)
            Return out
        End Function

        <Extension>
        Public Function shuffle(Of T)(ByRef list As List(Of T)) As List(Of T)
            Call randf.seeds.Shuffle(list)
            Return list
        End Function

        ''' <summary>
        ''' Returns a view of the portion of this list between the specified fromIndex, inclusive, 
        ''' and toIndex, exclusive. (If fromIndex and toIndex are equal, the returned list is empty.) 
        ''' The returned list is backed by this list, so non-structural changes in the returned 
        ''' list are reflected in this list, and vice-versa. The returned list supports all of the 
        ''' optional list operations supported by this list.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="fromIndex%">low endpoint (inclusive) of the subList</param>
        ''' <param name="toIndex%">high endpoint (exclusive) of the subList</param>
        ''' <returns>a view of the specified range within this list</returns>
        ''' 
        <Extension>
        Public Function subList(Of T)(list As IList(Of T), fromIndex%, toIndex%) As List(Of T)
            Return list.Skip(fromIndex).Take(toIndex - fromIndex).AsList
        End Function

        <Extension>
        Public Function Min(Of T As IComparable(Of T))(source As IEnumerable(Of T)) As T
            Return Enumerable.Min(source)
        End Function

        <Extension>
        Public Function Max(Of T As IComparable(Of T))(source As IEnumerable(Of T)) As T
            Return Enumerable.Max(source)
        End Function
    End Module
End Namespace
