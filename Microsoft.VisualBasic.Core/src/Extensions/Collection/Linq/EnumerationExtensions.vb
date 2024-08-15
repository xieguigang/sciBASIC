﻿#Region "Microsoft.VisualBasic::92775223ab390e0edfcdde473b164fd0, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\EnumerationExtensions.vb"

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

    '   Total Lines: 63
    '    Code Lines: 31 (49.21%)
    ' Comment Lines: 27 (42.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (7.94%)
    '     File Size: 2.65 KB


    '     Module EnumerationExtensions
    ' 
    '         Function: AsEnumerable, (+2 Overloads) AsObjectEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Linq

    <HideModuleName>
    Public Module EnumerationExtensions

        ''' <summary>
        ''' 将一个<see cref="Array"/>对象转换为一个<see cref="Object"/>对象的枚举序列
        ''' </summary>
        ''' <param name="enums"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 使用这个拓展函数的原因是<see cref="Array"/>对象不能够产生对象的枚举序列用于Linq拓展函数
        ''' </remarks>
        <Extension>
        Public Iterator Function AsObjectEnumerator(enums As Array) As IEnumerable(Of Object)
            If Not enums Is Nothing Then
                For i As Integer = 0 To enums.Length - 1
                    Yield enums.GetValue(i)
                Next
            End If
        End Function

        ''' <summary>
        ''' 将一个<see cref="Array"/>对象转换为一个<see cref="Object"/>对象的枚举序列
        ''' </summary>
        ''' <param name="enums"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 使用这个拓展函数的原因是<see cref="Array"/>对象不能够产生对象的枚举序列用于Linq拓展函数
        ''' </remarks>
        <Extension>
        Public Iterator Function AsObjectEnumerator(Of T)(enums As Array) As IEnumerable(Of T)
            For i As Integer = 0 To enums.Length - 1
                Yield DirectCast(enums.GetValue(i), T)
            Next
        End Function

        ''' <summary>
        ''' Returns the input typed as <see cref="IEnumerable(Of T)"/>.
        ''' </summary>
        ''' <typeparam name="T">The type of the elements of source.</typeparam>
        ''' <param name="enums">The sequence to type as <see cref="IEnumerable(Of T)"/></param>
        ''' <returns>The input sequence typed as <see cref="IEnumerable(Of T)"/>.</returns>
        ''' <remarks>
        ''' this iterator function is a kind of safe function: for the given data source is nothing,
        ''' this function will returns a empty collection, null reference error will not happends 
        ''' in this iterator function.
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsEnumerable(Of T)(enums As Enumeration(Of T)) As IEnumerable(Of T)
            If enums Is Nothing Then
                Return {}
            Else
                Return New Enumerator(Of T) With {
                    .Enumeration = enums
                }
            End If
        End Function
    End Module
End Namespace
