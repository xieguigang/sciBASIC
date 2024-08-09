#Region "Microsoft.VisualBasic::185ac18f571a872caf274da2a2d91651, Microsoft.VisualBasic.Core\src\Extensions\Collection\IsNullOrEmptyExtensions.vb"

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

    '   Total Lines: 253
    '    Code Lines: 130 (51.38%)
    ' Comment Lines: 95 (37.55%)
    '    - Xml Docs: 58.95%
    ' 
    '   Blank Lines: 28 (11.07%)
    '     File Size: 8.50 KB


    ' Module IsNullOrEmptyExtensions
    ' 
    '     Function: Empty, (+2 Overloads) GetLength, (+16 Overloads) IsNullOrEmpty
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization

<HideModuleName>
Public Module IsNullOrEmptyExtensions

    ''' <summary>
    ''' check of the given collection is null or empty?
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Empty(Of T)(list As IEnumerable(Of T)) As Boolean
        Return list Is Nothing OrElse Not list.Any
    End Function

    ' 2018-6-11
    '
    ' 因为迭代器在访问linq序列的时候，对于非空序列，下面的IsNullOrEmpty函数总是会产生一次迭代
    ' 这个迭代可能会导致元素丢失的bug产生
    ' 所以在这里将这个linq函数注释掉
    ' 以后只需要判断迭代器是否是空值即可

    '''' <summary>
    '''' This object collection is a null object or contains zero count items.
    '''' </summary>
    '''' <typeparam name="T"></typeparam>
    '''' <param name="source"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    '<Extension> Public Function IsNullOrEmpty(Of T)(source As IEnumerable(Of T)) As Boolean
    '    If source Is Nothing Then
    '        Return True
    '    End If

    '    Dim i% = -1

    '    Using [try] = source.GetEnumerator
    '        Do While [try].MoveNext

    '            ' debug view
    '            Dim null = [try].Current

    '            ' 假若是存在元素的，则i的值会为零
    '            ' Some type of linq sequence not support this method.
    '            ' [try].Reset()
    '            i += 1

    '            ' If is not empty, then this For loop will be used.
    '            Return False
    '        Loop
    '    End Using

    '    ' 由于没有元素，所以For循环没有进行，i变量的值没有发生变化
    '    ' 使用count拓展进行判断或导致Linq被执行两次，现在使用FirstOrDefault来判断，
    '    ' 主需要查看第一个元素而不是便利整个Linq查询枚举， 从而提高了效率
    '    ' Due to the reason of source is empty, no elements, 
    '    ' so that i value Is Not changed as the For loop 
    '    ' didn 't used.
    '    Return i = -1
    'End Function

    ''' <summary>
    ''' The <see cref="StringBuilder"/> object its content is nothing?
    ''' </summary>
    ''' <param name="sBuilder"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function IsNullOrEmpty(sBuilder As StringBuilder) As Boolean
        Return sBuilder Is Nothing OrElse sBuilder.Length = 0
    End Function

    ''' <summary>
    ''' 字典之中是否是没有任何数据的？
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="dict"></param>
    ''' <returns></returns>
    <Extension> Public Function IsNullOrEmpty(Of TKey, TValue)(dict As IDictionary(Of TKey, TValue)) As Boolean
        If dict Is Nothing Then
            Return True
        End If
        Return dict.Count = 0
    End Function

    <Extension>
    Public Function IsNullOrEmpty(Of T As INamedValue)(table As Dictionary(Of T)) As Boolean
        If table Is Nothing Then
            Return True
        Else
            Return table.Count = 0
        End If
    End Function

    ''' <summary>
    ''' 字典之中是否是没有任何数据的？
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="dict"></param>
    ''' <returns></returns>
    <Extension> Public Function IsNullOrEmpty(Of TKey, TValue)(dict As IReadOnlyDictionary(Of TKey, TValue)) As Boolean
        If dict Is Nothing Then
            Return True
        End If
        Return dict.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of TKey, TValue)(dict As ReadOnlyDictionary(Of TKey, TValue)) As Boolean
        If dict Is Nothing Then
            Return True
        End If
        Return dict.Count = 0
    End Function

    ''' <summary>
    ''' 字典之中是否是没有任何数据的？
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="dict"></param>
    ''' <returns></returns>
    <Extension> Public Function IsNullOrEmpty(Of TKey, TValue)(dict As Dictionary(Of TKey, TValue)) As Boolean
        If dict Is Nothing Then
            Return True
        End If
        Return dict.Count = 0
    End Function

    ''' <summary>
    ''' 这个队列之中是否是没有任何数据的?
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="queue"></param>
    ''' <returns></returns>
    <Extension> Public Function IsNullOrEmpty(Of T)(queue As Queue(Of T)) As Boolean
        If queue Is Nothing Then
            Return True
        End If
        Return queue.Count = 0
    End Function

    <Extension>
    Public Function IsNullOrEmpty(Of T)(vector As Vector(Of T)) As Boolean
        If vector Is Nothing Then
            Return True
        End If
        Return vector.Length = 0
    End Function

    <Extension>
    Public Function IsNullOrEmpty(args As ArgumentCollection) As Boolean
        If args Is Nothing Then
            Return True
        End If
        Return args.Count = 0
    End Function

    ''' <summary>
    ''' 这个动态列表之中是否是没有任何数据的？
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension> Public Function IsNullOrEmpty(Of T)(list As ICollection(Of T)) As Boolean
        If list Is Nothing Then
            Return True
        End If
        Return list.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of T)(list As IList(Of T)) As Boolean
        If list Is Nothing Then
            Return True
        End If
        Return list.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of T)(list As System.Collections.Generic.List(Of T)) As Boolean
        If list Is Nothing Then
            Return True
        End If
        Return list.Count = 0
    End Function

    <Extension>
    Public Function IsNullOrEmpty(Of T)(collection As IReadOnlyCollection(Of T)) As Boolean
        If collection Is Nothing Then
            Return True
        Else
            Return collection.Count = 0
        End If
    End Function

    <Extension>
    Public Function IsNullOrEmpty(Of T)(collection As ReadOnlyCollection(Of T)) As Boolean
        If collection Is Nothing Then
            Return True
        Else
            Return collection.Count = 0
        End If
    End Function

    ''' <summary>
    ''' This object array is a null object or contains zero count items.
    ''' (判断某一个对象数组是否为空)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function IsNullOrEmpty(array As Array) As Boolean
        Return array Is Nothing OrElse array.Length = 0
    End Function

    ''' <summary>
    ''' This object array is a null object or contains zero count items.
    ''' (判断某一个对象数组是否为空)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function IsNullOrEmpty(Of T)(array As T()) As Boolean
        Return array Is Nothing OrElse array.Length = 0
    End Function

    ''' <summary>
    ''' 0 for null object
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetLength(Of T)(array As T()) As Integer
        If array Is Nothing Then
            Return 0
        Else
            Return array.Length
        End If
    End Function

    <Extension>
    Public Function GetLength(Of T)(collect As IEnumerable(Of T)) As Integer
        If collect Is Nothing Then
            Return 0
        Else
            Return collect.Count
        End If
    End Function
End Module
