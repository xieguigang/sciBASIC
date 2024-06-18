#Region "Microsoft.VisualBasic::73931b1f93a679937abd2d39a81e708b, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Set\SetsExtension.vb"

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

    '   Total Lines: 122
    '    Code Lines: 61 (50.00%)
    ' Comment Lines: 48 (39.34%)
    '    - Xml Docs: 97.92%
    ' 
    '   Blank Lines: 13 (10.66%)
    '     File Size: 3.83 KB


    ' Module SetsExtension
    ' 
    '     Function: (+2 Overloads) AsSet, Except, Intersection, PollFirst, PollLast
    '               (+3 Overloads) ToArray, Union
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures

Namespace ComponentModel.DataStructures

    Public Module SetsExtension

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function ToArray(Of T)(s As [Set], [ctype] As Func(Of Object, T)) As T()
            Return s.ToArray _
                .Select([ctype]) _
                .ToArray
        End Function

        ''' <summary>
        ''' DirectCast
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function ToArray(Of T)(s As [Set]) As T()
            Return s.ToArray _
                .Select(Function(x) DirectCast(x, T)) _
                .ToArray
        End Function

#Region "API"

        ''' <summary>
        ''' 差集，函数会从<paramref name="s1"/>集合之中删除<paramref name="s2"/>中共同的元素
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Except")>
        Public Function Except(s1 As [Set], s2 As [Set]) As [Set]
            Return s1 - s2
        End Function

        ''' <summary>
        ''' Performs a union of two sets.
        ''' (就并集)
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Union")>
        Public Function Union(s1 As [Set], s2 As [Set]) As [Set]
            Return s1 Or s2
        End Function

        ''' <summary>
        ''' Performs an intersection of two sets.
        ''' (求交集)
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Intersection")>
        Public Function Intersection(s1 As [Set], s2 As [Set]) As [Set]
            Return s1 And s2
        End Function

        ''' <summary>
        ''' 将任意序列转换为集合类型
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("As.Set")>
        <Extension>
        Public Function AsSet(source As IEnumerable) As [Set]
            Return New [Set](source)
        End Function

        <ExportAPI("As.Array")>
        Public Function ToArray([set] As [Set]) As Object()
            Return [set].ToArray
        End Function

        ''' <summary>
        ''' Create a string set
        ''' </summary>
        ''' <param name="strings"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsSet(strings As IEnumerable(Of String)) As StringSet
            Return New StringSet(strings)
        End Function
#End Region

        ''' <summary>
        ''' get the last element in the <paramref name="set"/>, and then removes the last element
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="[set]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PollLast(Of T)([set] As SortedSet(Of T)) As T
            Dim last = [set].Last
            [set].Remove(last)
            Return last
        End Function

        ''' <summary>
        ''' get the first element in the <paramref name="set"/>, and then removes the first element
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="[set]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PollFirst(Of T)([set] As SortedSet(Of T)) As T
            Dim first = [set].First
            [set].Remove(first)
            Return first
        End Function

    End Module
End Namespace