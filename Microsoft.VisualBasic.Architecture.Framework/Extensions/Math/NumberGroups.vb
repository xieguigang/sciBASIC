#Region "Microsoft.VisualBasic::f04347f3f7fabf0e6711904b3b514f38, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\NumberGroups.vb"

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
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace Mathematical

    ''' <summary>
    ''' Simple number vector grouping
    ''' </summary>
    Public Module NumberGroups

        Public Interface IVector
            ReadOnly Property Data As Double()
        End Interface

        <Extension>
        Public Function Match(Of T As IVector)(a As IEnumerable(Of T), b As IEnumerable(Of T)) As Double
            Dim target As New List(Of T)(a)
            Dim mins = b.Select(Function(x) target.Min(x))
            Dim result As Double = mins.Sum(Function(tt) tt.Tag)

            With target
                For Each x In mins.Select(Function(o) o.value)
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
                .value = minX
            }
        End Function

        ''' <summary>
        ''' 将一维的数据按照一定的偏移量分组输出
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Groups(source As IEnumerable(Of Integer), offset As Integer) As List(Of Integer())
            Dim list As New List(Of Integer())
            Dim orders As Integer() = (From n As Integer
                                   In source
                                       Select n
                                       Order By n Ascending).ToArray
            Dim tag As Integer = orders(Scan0)
            Dim tmp As New List(Of Integer) From {tag}

            For Each x As Integer In orders.Skip(1)
                If x - tag <= offset Then  ' 因为已经是经过排序了的，所以后面总是大于前面的
                    tmp += x
                Else
                    tag = x
                    list += tmp.ToArray
                    tmp = New List(Of Integer) From {x}
                End If
            Next

            If tmp.Count > 0 Then
                list += tmp.ToArray
            End If

            Return list
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
