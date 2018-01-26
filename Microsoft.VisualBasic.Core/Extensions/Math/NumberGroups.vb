#Region "Microsoft.VisualBasic::1585693a19d1d942ebc15ac34e45c0ac, ..\sciBASIC#\Microsoft.VisualBasic.Core\Extensions\Math\NumberGroups.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Math

    ''' <summary>
    ''' Simple number vector grouping
    ''' </summary>
    Public Module NumberGroups

        ''' <summary>
        ''' The numeric vector model
        ''' </summary>
        Public Interface IVector
            ReadOnly Property Data As Double()
        End Interface

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
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="offsets"></param>
        ''' <returns></returns>
        <Extension> Public Function GroupBy(Of T)(source As IEnumerable(Of T), evaluate As Func(Of T, Double), offsets#) As NamedCollection(Of T)()
            Dim data As List(Of T) = source.AsList
            Dim tmp As New With {
                .values = New List(Of Double),
                .list = New List(Of T)
            }
            Dim groups = {
                tmp
            }.ToDictionary(Function(null) "",
                           Function(obj) obj)

            Call groups.Clear()

            Do While data.Count > 0
                Dim x As T = data.Pop
                Dim hit As Boolean = False
                Dim value# = evaluate(x)

                For Each group In groups.Values
                    If Abs(group.values.Average - value) <= offsets Then
                        group.values.Add(value)
                        group.list.Add(x)
                        hit = True
                        Exit For
                    End If
                Next

                If Not hit Then
                    tmp = New With {
                        .values = New List(Of Double),
                        .list = New List(Of T)
                    }
                    tmp.values.Add(value)
                    tmp.list.Add(x)
                    groups.Add(value, tmp)
                End If
            Loop

            Return groups _
                .Select(Function(tuple)
                            Return New NamedCollection(Of T) With {
                                .Name = tuple.Key,
                                .Value = tuple.Value.list
                            }
                        End Function) _
                .OrderBy(Function(tuple) Val(tuple.Name)) _
                .ToArray
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

    Public Interface INumberTag
        ReadOnly Property Tag As Integer
    End Interface
End Namespace
