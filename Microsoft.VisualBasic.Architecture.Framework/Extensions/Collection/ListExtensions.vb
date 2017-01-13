#Region "Microsoft.VisualBasic::403faf51ed9f2faf4d974a0d3fb5883f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\ListExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Initializes a new instance of the <see cref="List"/>`1 class that
''' contains elements copied from the specified collection and has sufficient capacity
''' to accommodate the number of elements copied.
''' </summary>
Public Module ListExtensions

    ''' <summary>
    '''
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="indexs">所要获取的目标对象的下表的集合</param>
    ''' <param name="reversed">是否为反向选择，即返回所有不在目标index集合之中的元素列表</param>
    ''' <param name="OffSet">当进行反选的时候，本参数将不会起作用</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("takes")>
    <Extension> Public Function Takes(Of T)(source As IEnumerable(Of T),
                                            indexs%(),
                                            Optional offSet% = 0,
                                            Optional reversed As Boolean = False) As T()
        If reversed Then
            Return source.__reversedTake(indexs)
        End If

        Dim result As T() = New T(indexs.Length - 1) {}
        Dim indices As New IndexOf(Of Integer)(
            indexs.Select(Function(oi) oi + offSet))

        For Each x As SeqValue(Of T) In source.SeqIterator
            Dim i As Integer = indices(x.i)  ' 在这里得到的是x的index在indexs参数之中的索引位置

            If i > -1 Then  ' 当前的原始的下表位于indexs参数值中，则第i个indexs元素所指向的source的元素就是x，将其放入对应的结果列表之中
                result(i) = +x
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' 反选，即将所有不出现在<paramref name="indexs"></paramref>之中的元素都选取出来
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="collection"></param>
    ''' <param name="indexs"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension>
    Private Function __reversedTake(Of T)(collection As IEnumerable(Of T), indexs As Integer()) As T()
        Dim indices As New IndexOf(Of Integer)(indexs)
        Dim out As New List(Of T)

        For Each x As SeqValue(Of T) In collection.SeqIterator
            If indices.IndexOf(x.i) = -1 Then  ' 不存在于顶点的列表之中，即符合反选的条件，则添加进入结果之中
                out += x.value
            End If
        Next

        Return out
    End Function

    <Extension>
    Public Sub Swap(Of T)(ByRef l As System.Collections.Generic.List(Of T), i%, j%)
        Dim tmp = l(i)
        l(i) = l(j)
        l(j) = tmp
    End Sub

    <Extension>
    Public Sub ForEach(Of T)(source As IEnumerable(Of T), action As Action(Of T, Integer))
        For Each x As SeqValue(Of T) In source.SeqIterator
            Call action(x.value, x.i)
        Next
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="List"/>`1 class that
    ''' contains elements copied from the specified collection and has sufficient capacity
    ''' to accommodate the number of elements copied.
    ''' </summary>
    ''' <param name="source">The collection whose elements are copied to the new list.</param>
    <Extension> Public Function ToList(Of T, TOut)(
                                  source As IEnumerable(Of T),
                                 [CType] As Func(Of T, TOut),
                       Optional parallel As Boolean = False) As List(Of TOut)

        If source Is Nothing Then
            Return New List(Of TOut)
        End If

        Dim result As List(Of TOut)

        If parallel Then
            result = (From x As T In source.AsParallel Select [CType](x)).ToList
        Else
            result = (From x As T In source Select [CType](x)).ToList
        End If

        Return result
    End Function

    ''' <summary>
    ''' Initializes a new instance of the <see cref="List"/>`1 class that
    ''' contains elements copied from the specified collection and has sufficient capacity
    ''' to accommodate the number of elements copied.
    ''' </summary>
    ''' <param name="source">The collection whose elements are copied to the new list.</param>
    <Extension> Public Function ToList(Of T)(source As IEnumerable(Of T)) As List(Of T)
        Return New List(Of T)(source)
    End Function

    ''' <summary>
    ''' Initializes a new instance of the <see cref="List"/>`1 class that
    ''' contains elements copied from the specified collection and has sufficient capacity
    ''' to accommodate the number of elements copied.
    ''' </summary>
    ''' <param name="linq">The collection whose elements are copied to the new list.</param>
    <Extension> Public Function ToList(Of T)(linq As ParallelQuery(Of T)) As List(Of T)
        Return New List(Of T)(linq)
    End Function
End Module
