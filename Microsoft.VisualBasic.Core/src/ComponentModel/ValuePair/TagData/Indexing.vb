#Region "Microsoft.VisualBasic::d6f951975dd5484f4957a0570a7c56c3, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\TagData\Indexing.vb"

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

    '   Total Lines: 86
    '    Code Lines: 54 (62.79%)
    ' Comment Lines: 20 (23.26%)
    '    - Xml Docs: 95.00%
    ' 
    '   Blank Lines: 12 (13.95%)
    '     File Size: 3.39 KB


    '     Module IndexingExtensions
    ' 
    '         Function: (+3 Overloads) BinarySearch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.TagData

    Public Module IndexingExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BinarySearch(Of K As IComparable(Of K), T)(source As IEnumerable(Of T),
                                                                   key As K,
                                                                   getKey As Func(Of T, K),
                                                                   Optional [default] As T = Nothing) As T

            Return source _
                .OrderBy(getKey) _
                .ToArray _
                .BinarySearch(key, getKey, [default])
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="inputArray">传入的数组必须是经过升序排序的</param>
        ''' <param name="key"></param>
        ''' <param name="getKey"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BinarySearch(Of K As IComparable(Of K), T)(inputArray As T(), key As K, getKey As Func(Of T, K), Optional [default] As T = Nothing) As T
            Dim min = 0
            Dim max = inputArray.Length - 1
            Dim mid%

            Do While min <= max
                [mid] = (min + max) / 2

                If key.CompareTo(getKey(inputArray(mid))) = 0 Then
                    Return inputArray(mid)
                ElseIf key.CompareTo(getKey(inputArray(mid))) < 0 Then
                    max = mid - 1
                Else
                    min = mid + 1
                End If
            Loop

            Return [default]
        End Function

        ''' <summary>
        ''' 传入的数组必须是经过升序排序的
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="inputArray">传入的数组必须是经过升序排序的</param>
        ''' <param name="key"></param>
        ''' <param name="getKey"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BinarySearch(Of K As IComparable(Of K), T)(inputArray As T(),
                                                                   key As K,
                                                                   getKey As Func(Of T, K),
                                                                   equals As Func(Of K, K, Boolean),
                                                                   Optional [default] As T = Nothing) As T
            Dim min = 0
            Dim max = inputArray.Length - 1
            Dim mid%

            Do While min <= max
                [mid] = (min + max) / 2

                If equals(key, getKey(inputArray(mid))) Then
                    Return inputArray(mid)
                ElseIf key.CompareTo(getKey(inputArray(mid))) < 0 Then
                    max = mid - 1
                Else
                    min = mid + 1
                End If
            Loop

            Return [default]
        End Function
    End Module
End Namespace
