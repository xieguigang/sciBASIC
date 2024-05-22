﻿#Region "Microsoft.VisualBasic::29195d05e441ece2a9df95217d648b6a, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\JoinExtensions.vb"

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
    '    Code Lines: 43 (58.11%)
    ' Comment Lines: 24 (32.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (9.46%)
    '     File Size: 2.79 KB


    '     Module JoinExtensions
    ' 
    '         Function: IteratesALL, (+2 Overloads) JoinIterates
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Linq

    <HideModuleName>
    Public Module JoinExtensions

        ''' <summary>
        ''' Iterates all of the elements in a two dimension collection as the data source 
        ''' for the linq expression or ForEach statement.
        ''' (适用于二维的集合做为linq的数据源，不像<see cref="Unlist"/>是进行转换，
        ''' 这个是返回迭代器的，推荐使用这个函数)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function is a safe function, the null collection value in the given
        ''' <paramref name="source"/> will be ignored
        ''' </remarks>
        <DebuggerStepThrough>
        <Extension>
        Public Iterator Function IteratesALL(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As IEnumerable(Of T)
            For Each line As IEnumerable(Of T) In source
                If Not line Is Nothing Then
                    Using iterator = line.GetEnumerator
                        Do While iterator.MoveNext
                            Yield iterator.Current
                        Loop
                    End Using
                End If
            Next
        End Function

        ''' <summary>
        ''' First, iterate populates the elements in collection <paramref name="a"/>, 
        ''' and then populate out all of the elements on collection <paramref name="b"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a">Object collection</param>
        ''' <param name="b">Another object collection.</param>
        ''' <returns>
        ''' <paramref name="a"/> or <paramref name="b"/> will be ignored if
        ''' collection object is nothing
        ''' </returns>
        <Extension>
        Public Iterator Function JoinIterates(Of T)(a As IEnumerable(Of T), b As IEnumerable(Of T)) As IEnumerable(Of T)
            If Not a Is Nothing Then
                For Each x As T In a
                    Yield x
                Next
            End If

            If Not b Is Nothing Then
                For Each x As T In b
                    Yield x
                Next
            End If
        End Function

        <Extension>
        Public Iterator Function JoinIterates(Of T)(a As IEnumerable(Of T), b As T) As IEnumerable(Of T)
            If Not a Is Nothing Then
                For Each x As T In a
                    Yield x
                Next
            End If

            If Not b Is Nothing Then
                Yield b
            End If
        End Function
    End Module
End Namespace
