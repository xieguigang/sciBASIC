#Region "Microsoft.VisualBasic::79b7fdec7ccc63b6a3a733dfb733105d, Microsoft.VisualBasic.Core\src\Extensions\Collection\BucketOperators.vb"

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

    '   Total Lines: 104
    '    Code Lines: 44 (42.31%)
    ' Comment Lines: 48 (46.15%)
    '    - Xml Docs: 97.92%
    ' 
    '   Blank Lines: 12 (11.54%)
    '     File Size: 4.21 KB


    ' Module BucketOperators
    ' 
    '     Function: (+4 Overloads) Join, Split, SplitIterator
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

''' <summary>
''' 进行集合分块切割或者合并等操作
''' </summary>
Public Module BucketOperators

    ''' <summary>
    ''' Data partitioning function.
    ''' (将目标集合之中的数据按照<paramref name="partitionSize"></paramref>参数分配到子集合之中，
    ''' 这个函数之中不能够使用并行化Linq拓展，以保证元素之间的相互原有的顺序，
    ''' 每一个子集合之中的元素数量为<paramref name="partitionSize"/>)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="partitionSize">每一个子集合之中的元素的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Split(Of T)(source As IEnumerable(Of T), partitionSize As Integer) As T()()
        Return source.SplitIterator(partitionSize).ToArray
    End Function

    ''' <summary>
    ''' Performance the partitioning operation on the input sequence.
    ''' (请注意，这个函数只适用于数量较少的序列。对所输入的序列进行分区操作，<paramref name="partitionSize"/>函数参数是每一个分区里面的元素的数量)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="partitionSize">
    ''' The partition size should be less than the array upbound size
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function SplitIterator(Of T)(source As IEnumerable(Of T), partitionSize As Integer) As IEnumerable(Of T())
        Dim buffer As New List(Of T)(capacity:=partitionSize)

        For Each item As T In source
            buffer += item

            If buffer >= partitionSize Then
                Yield buffer.ToArray

                buffer *= 0
            End If
        Next

        If buffer > 0 Then
            Yield buffer.ToArray
        End If
    End Function

    ''' <summary>
    ''' Merge two type specific collection.(函数会忽略掉空的集合，函数会构建一个新的集合，原有的集合不受影响)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="target"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), target As IEnumerable(Of T)) As List(Of T)
        Dim srcList As List(Of T) = If(source Is Nothing, New List(Of T), source.AsList)
        If Not target Is Nothing Then
            Call srcList.AddRange(target)
        End If
        Return srcList
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), ParamArray data As T()) As List(Of T)
        Return source.Join(target:=data)
    End Function

    ''' <summary>
    ''' Source list join a new <paramref name="data"/> element.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), data As T) As List(Of T)
        Return source.Join({data})
    End Function

    ''' <summary>
    ''' ``X, ....``
    ''' 
    ''' (这个函数是一个安全的函数，当<paramref name="collection"/>为空值的时候回忽略掉<paramref name="collection"/>，
    ''' 只返回包含有一个<paramref name="obj"/>元素的列表)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="collection"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(obj As T, collection As IEnumerable(Of T)) As List(Of T)
        With New List(Of T) From {obj}
            If Not collection Is Nothing Then
                Call .AddRange(collection)
            End If

            Return .ByRef
        End With
    End Function
End Module
