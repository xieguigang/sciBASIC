﻿#Region "Microsoft.VisualBasic::85f694a73ab063bd4ae417f3c4181fd5, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Math\NumberGroups.vb"

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

    '   Total Lines: 402
    '    Code Lines: 261
    ' Comment Lines: 94
    '   Blank Lines: 47
    '     File Size: 15.05 KB


    '     Interface INumericKey
    ' 
    '         Properties: key
    ' 
    '     Module NumberGroups
    ' 
    '         Function: BinarySearch, diff, (+5 Overloads) GroupBy, GroupByImpl, GroupByParallel
    '                   Groups, Match, Min
    ' 
    '     Interface IVector
    ' 
    '         Properties: Data
    ' 
    '     Interface INumberTag
    ' 
    '         Properties: Tag
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Statistics
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace Math

    ''' <summary>
    ''' Simple number vector grouping
    ''' </summary>
    Public Module NumberGroups

        ''' <summary>
        ''' ### Lagged Differences
        ''' 
        ''' Returns suitably lagged and iterated differences.
        ''' </summary>
        ''' <param name="x">
        ''' a numeric vector Or matrix containing the values To be differenced.
        ''' </param>
        ''' <returns>
        ''' for input vector element size is zero or else only one element inside, 
        ''' a empty diff vector will be generated from this function
        ''' </returns>
        ''' <remarks>
        ''' x1 - x0
        ''' </remarks>
        Public Function diff(x As Double()) As Double()
            If x.TryCount <= 1 Then
                Return New Double() {}
            Else
                Dim diffs As New List(Of Double)
                Dim base As Double = x(Scan0)

                For Each xi As Double In x.Skip(1)
                    diffs.Add(xi - base)
                    base = xi
                Next

                Return diffs.ToArray
            End If
        End Function

        <Extension>
        Public Function Match(Of T As IVector)(a As IEnumerable(Of T), b As IEnumerable(Of T)) As Double
            Dim target As New List(Of T)(a)
            Dim mins = b.Select(Function(x) target.Min(x))
            Dim result As Double = mins.Sum(Function(tt) tt.Tag)

            With target
                For Each x In mins.Select(Function(o) o.Value)
                    Call .Remove(item:=x)
                    If .Count = 0 Then
                        Exit For
                    End If
                Next
            End With

            Return result * (target.Count + 1)
        End Function

        ''' <summary>
        ''' 计算出<paramref name="target"/>集合之众的与<paramref name="v"/>距离最小的元素
        ''' （或者说是匹配度最高的元素）
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="target"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Min(Of T As IVector)(target As IEnumerable(Of T), v As T) As DoubleTagged(Of T)
            Dim minV# = Double.MaxValue
            Dim minX As T
            Dim vector#() = v.Data

            For Each x As T In target
                Dim d# = x.Data.EuclideanDistance(vector)

                If d < minV Then
                    minV = d
                    minX = x
                End If
            Next

            Return New DoubleTagged(Of T) With {
                .Tag = minV,
                .Value = minX
            }
        End Function

        ''' <summary>
        ''' Returns ``-1`` means no search result
        ''' </summary>
        ''' <param name="seq"></param>
        ''' <param name="target"></param>
        ''' <param name="equals"></param>
        ''' <returns>
        ''' gets the index value of the <paramref name="target"/> number inside 
        ''' the input source <paramref name="seq"/> where we find it.
        ''' </returns>
        <Extension>
        Public Function BinarySearch(seq As IEnumerable(Of Double), target#, equals As GenericLambda(Of Double).IEquals) As Integer
            With seq _
                .SeqIterator _
                .OrderBy(Function(x) x.value) _
                .ToArray

                Dim x As SeqValue(Of Double)
                Dim min% = 0
                Dim max% = .Length - 1
                Dim index%
                Dim value#

                If max = -1 Then
                    ' no elements
                    Return -1
                ElseIf max = 0 Then
                    ' one element
                    If equals(.ByRef(0).value, target) Then
                        Return 0
                    Else
                        ' 序列只有一个元素，但是不相等，则返回-1，否则后面的while会无限死循环
                        Return -1
                    End If
                End If

                Do While max <> (min + 1)
                    index = (max - min) / 2 + min
                    x = .ByRef(index)
                    value = x.value

                    If equals(target, value) Then
                        Return x.i
                    ElseIf target > value Then
                        min = index
                    Else
                        max = index
                    End If
                Loop

                If equals(.ByRef(min).value, target) Then
                    Return .ByRef(min).i
                ElseIf equals(.ByRef(max).value, target) Then
                    Return .ByRef(max).i
                Else
                    Return -1
                End If
            End With
        End Function

        '''' <summary>
        '''' 将一维的数据按照一定的偏移量分组输出
        '''' </summary>
        '''' <param name="source"></param>
        '''' <returns></returns>
        '<Extension>
        'Public Function GroupBy(Of T)(source As IEnumerable(Of T),
        '                              evaluate As Evaluate(Of T),
        '                              equals As GenericLambda(Of Double).IEquals) As IEnumerable(Of NamedCollection(Of T))

        '    Return source.GroupBy(AddressOf evaluate.Invoke, equals)
        'End Function

        ''' <summary>
        ''' implements the group by function via AVLTree for large scale data
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Iterator Function GroupByTree(source As IEnumerable(Of Double()), offset As Double) As IEnumerable(Of NamedCollection(Of Double))
            Dim sort As New AVLTree(Of Double, Double)(NumberGroupHelper(offset))

            For Each block As Double() In source.SafeQuery
                For Each d As Double In block.SafeQuery
                    Call sort.Add(d, d, valueReplace:=False)
                Next
            Next

            For Each bin As BinaryTree(Of Double, Double) In sort.GetAllNodes
                Yield New NamedCollection(Of Double)(bin.Key.ToString, bin.Members)
            Next
        End Function

        Private Function NumberGroupHelper(offset As Double) As Comparison(Of Double)
            Return Function(a, b)
                       If std.Abs(a - b) <= offset Then
                           Return 0
                       ElseIf a < b Then
                           Return 1
                       Else
                           Return -1
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GroupBy(source As IEnumerable(Of Double), offset As Double) As IEnumerable(Of NamedCollection(Of Double))
            ' 先进行预处理：求值然后进行排序
            Dim tagValues = source _
                .OrderBy(Function(o) o) _
                .ToArray
            Dim means As New Average

            ' 根据分组的平均值来进行分组操作
            For Each x As Double In tagValues
                If means.N = 0 Then
                    means.Add(x)
                Else
                    If std.Abs(means.Average - x) < offset Then
                        means += x
                    Else
                        Yield New NamedCollection(Of Double)(CStr(means.Average), means.getRaw)
                        means = New Average({x})
                    End If
                End If
            Next

            If means.N > 0 Then
                Yield New NamedCollection(Of Double)(CStr(means.Average), means.getRaw)
            End If
        End Function

        <Extension>
        Public Iterator Function GroupByTree(Of T)(source As IEnumerable(Of T()),
                                                   eval As Func(Of T, Double),
                                                   compares As Comparison(Of Double)) As IEnumerable(Of NamedCollection(Of T))

            Dim sort As New AVLTree(Of Double, T)(compares)

            For Each block As T() In source.SafeQuery
                For Each d As T In block.SafeQuery
                    Call sort.Add(eval(d), d, valueReplace:=False)
                Next
            Next

            For Each bin As BinaryTree(Of Double, T) In sort.GetAllNodes
                Yield New NamedCollection(Of T)(bin.Key.ToString, bin.Members)
            Next
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GroupBy(Of T)(source As IEnumerable(Of T),
                                               evaluate As Func(Of T, Double),
                                               equals As GenericLambda(Of Double).IEquals) As IEnumerable(Of NamedCollection(Of T))
            ' 先进行预处理：求值然后进行排序
            Dim tagValues = source _
                .Select(Function(o) (evaluate(o), o)) _
                .OrderBy(Function(o) o.Item1) _
                .ToArray
            Dim means As New Average
            Dim members As New List(Of T)

            ' 根据分组的平均值来进行分组操作
            For Each x As (val#, o As T) In tagValues
                If means.N = 0 Then
                    means += x.Item1
                    members += x.Item2
                Else
                    If equals(means.Average, x.Item1) Then
                        means += x.Item1
                        members += x.Item2
                    Else
                        Yield New NamedCollection(Of T)(CStr(means.Average), members)

                        means = New Average({x.Item1})
                        members = New List(Of T) From {x.Item2}
                    End If
                End If
            Next

            If members > 0 Then
                Yield New NamedCollection(Of T)(CStr(means.Average), members)
            End If
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GroupByParallel(Of T)(source As IEnumerable(Of T),
                                                       evaluate As Func(Of T, Double),
                                                       equals As GenericLambda(Of Double).IEquals,
                                                       Optional chunkSize% = 20000) As IEnumerable(Of NamedCollection(Of T))
            Dim partitions = source _
                .SplitIterator(partitionSize:=chunkSize) _
                .AsParallel _
                .Select(Function(part)
                            Return part.AsList.GroupByImpl(evaluate, equals)
                        End Function) _
                .IteratesALL _
                .AsList

            ' 先分割，再对name做分组
            Dim union = partitions.GroupByImpl(Function(part) Val(part.name), equals)

            For Each unionGroup In union
                Dim name$ = unionGroup.name
                Dim data = unionGroup _
                    .value _
                    .Select(Function(member) member.value) _
                    .IteratesALL _
                    .ToArray

                Yield New NamedCollection(Of T) With {
                    .name = name,
                    .value = data
                }
            Next
        End Function

        <Extension>
        Private Function GroupByImpl(Of T)(source As List(Of T), evaluate As Func(Of T, Double), equals As GenericLambda(Of Double).IEquals) As NamedCollection(Of T)()
            Dim tmp As New With {
                .avg = New Average({}),
                .list = New List(Of T)
            }
            Dim groups = {
                tmp
            }.AsList * 0

            Do While source.Count > 0
                Dim x As T = source.Pop
                Dim value# = evaluate(x)
                Dim hit% = groups _
                    .Select(Function(g)
                                Return g.avg.Average
                            End Function) _
                    .BinarySearch(value, equals)

                ' 在这里应该使用二分法查找来加快计算速度的
                If hit > -1 Then
                    With groups(hit)
                        .avg += value
                        .list.Add(x)
                    End With
                Else
                    groups += New With {
                        .avg = New Average({value}),
                        .list = New List(Of T) From {x}
                    }
                End If
            Loop

            Return groups _
                .Select(Function(tuple)
                            Return New NamedCollection(Of T) With {
                                .name = tuple.avg.Average,
                                .value = tuple.list
                            }
                        End Function) _
                .OrderBy(Function(tuple) Val(tuple.name)) _
                .ToArray
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="offsets"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GroupBy(Of T)(source As IEnumerable(Of T), evaluate As Func(Of T, Double), offsets#) As IEnumerable(Of NamedCollection(Of T))
            Return source.GroupBy(evaluate, equals:=Function(a, b) std.Abs(a - b) <= offsets)
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="numbers"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GroupBy(numbers As IEnumerable(Of Double), tolerance As GenericLambda(Of Double).IEquals) As IEnumerable(Of NamedCollection(Of Double))
            Return numbers.GroupBy(Self(Of Double), tolerance)
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="numbers"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GroupBy(Of T As INumericKey)(numbers As IEnumerable(Of T), tolerance As GenericLambda(Of Double).IEquals) As IEnumerable(Of NamedCollection(Of T))
            Return numbers.GroupBy(Function(i) i.key, tolerance)
        End Function

        ''' <summary>
        ''' 按照相邻的两个数值是否在offset区间内来进行简单的分组操作
        ''' </summary>
        ''' <typeparam name="TagObject"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Groups(Of TagObject As INumberTag)(source As IEnumerable(Of TagObject), offset As Integer) As GroupResult(Of TagObject, Integer)()
            Dim list As New List(Of GroupResult(Of TagObject, Integer))
            Dim orders As TagObject() = (From x As TagObject
                                         In source
                                         Select x
                                         Order By x.Tag Ascending).ToArray
            Dim tag As TagObject = orders(Scan0)
            Dim tmp As New List(Of TagObject) From {tag}

            For Each x As TagObject In orders.Skip(1)
                If x.Tag - tag.Tag <= offset Then  ' 因为已经是经过排序了的，所以后面总是大于前面的
                    tmp += x
                Else
                    tag = x
                    list += New GroupResult(Of TagObject, Integer)(tag.Tag, tmp)
                    tmp = New List(Of TagObject) From {x}
                End If
            Next

            If tmp.Count > 0 Then
                list += New GroupResult(Of TagObject, Integer)(tag.Tag, tmp)
            End If

            Return list
        End Function
    End Module

    ''' <summary>
    ''' The numeric vector model
    ''' </summary>
    Public Interface IVector

        ''' <summary>
        ''' the vector data
        ''' </summary>
        ''' <returns></returns>
        Property Data As Double()

    End Interface

    Public Interface INumberTag
        ReadOnly Property Tag As Integer
    End Interface
End Namespace
