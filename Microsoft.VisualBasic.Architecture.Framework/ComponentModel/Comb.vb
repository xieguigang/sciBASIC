#Region "Microsoft.VisualBasic::a451bf50b7dfd1e08defa5935324ccd4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Comb.vb"

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
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' 对象类型的组合输出工具，即目标类型的集合之中的元素两两组合配对
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Comb(Of T)

        Dim lstSource As List(Of T)
        Dim p As Integer = 1
        Dim _NewLine As Boolean = True

        ''' <summary>
        ''' 对象列表是否已经完全组合输出
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property EOL As Boolean
            Get
                Return Not lstSource.Count >= 2
            End Get
        End Property

        ''' <summary>
        ''' 是否已经开始读取新的一行数据
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NewLine As Boolean
            Get
                Return _NewLine
            End Get
        End Property

        Public ReadOnly Property CombList As KeyValuePair(Of T, T)()()
            Get
                Dim LQuery = (From Handle As Integer In lstSource.Sequence.Take(lstSource.Count - 1).AsParallel
                              Let ArrayList = (From Hwnd As Integer In lstSource.Skip(Handle + 1).Sequence
                                               Let b = Handle + 1
                                               Select New KeyValuePair(Of T, T)(lstSource(Handle), lstSource(b + Hwnd))).ToArray
                              Select ArrayList
                              Order By ArrayList.Length Descending).ToArray
                Return LQuery

                'Dim n As Integer = ObjectCollection.Count - 1
                'Dim List As List(Of KeyValuePair(Of T, T)()) = New List(Of KeyValuePair(Of T, T)())
                'Dim TEMP As List(Of KeyValuePair(Of T, T)) = New List(Of KeyValuePair(Of T, T))

                'For i As Integer = 0 To ObjectCollection.Count - 1
                '    For idx As Integer = i + 1 To ObjectCollection.Count - 1
                '        Call TEMP.Add(New KeyValuePair(Of T, T)(ObjectCollection(i), ObjectCollection(idx)))
                '    Next
                '    Call List.Add(TEMP.ToArray)
                '    Call TEMP.Clear()
                'Next

                'Return List.ToArray
            End Get
        End Property

        Public Function GetObjectPair() As KeyValuePair(Of T, T)
            If lstSource.Count = 1 Then
                Return Nothing
            End If

            If p < lstSource.Count Then
                Dim [Object] As T = lstSource(p)
                _NewLine = False
                p += 1
                Return New KeyValuePair(Of T, T)(lstSource(0), [Object])
            Else
                lstSource.RemoveAt(0)
                p = 1
                Dim [ObjectPair] = GetObjectPair()
                _NewLine = True

                Return ObjectPair
            End If
        End Function

        Friend Sub New()
        End Sub

        Public Shared Function CreateObject(Collection As Generic.IEnumerable(Of T)) As Comb(Of T)
            Return New Comb(Of T) With {.lstSource = Collection.ToList}
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("Comb(Of {0})::There is {1} object last.", GetType(T).FullName, lstSource.Count)
        End Function

        Public Shared Widening Operator CType(ObjectCollection As T()) As Comb(Of T)
            Return New Comb(Of T) With {.lstSource = ObjectCollection.ToList}
        End Operator

        Public Shared Widening Operator CType(ObjectCollection As List(Of T)) As Comb(Of T)
            Return New Comb(Of T) With {.lstSource = ObjectCollection}
        End Operator

        ''' <summary>
        ''' Creates the completely combination of the elements in the target input collection source.
        ''' (创建完完全全的两两配对)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Iterator Function CreateCompleteObjectPairs(source As IEnumerable(Of T)) As IEnumerable(Of KeyValuePair(Of T, T)())
            Dim array As T() = source.ToArray

            For Each i As T In array
                Dim tmp As New List(Of KeyValuePair(Of T, T))

                For Each j As T In array
                    tmp += New KeyValuePair(Of T, T)(i, j)
                Next

                Yield tmp.ToArray
            Next
        End Function
    End Class

    ''' <summary>
    ''' 任意多个集合之间的对象之间相互组成组合输出
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Comb

        <Extension>
        Public Iterator Function CreateCombos(Of TA, TB)(sourceA As IEnumerable(Of TA), sourceB As IEnumerable(Of TB)) As IEnumerable(Of KeyValuePair(Of TA, TB))
            Dim b As TB() = sourceB.ToArray

            For Each i As TA In sourceA
                For Each j As TB In b
                    Yield New KeyValuePair(Of TA, TB)(i, j)
                Next
            Next
        End Function

        <Extension> Public Iterator Function Iteration(Of T)(source As T()()) As IEnumerable(Of T())
            Dim first As T() = source.First

            If source.Length = 2 Then ' 只剩下两个的时候，会退出递归操作
                Dim last As T() = source.Last

                For Each x As T In first
                    For Each _item As T In last
                        Yield {x, _item}
                    Next
                Next
            Else
                For Each x As T In first
                    For Each subArray As T() In source.Skip(1).ToArray.Iteration   ' 递归组合迭代
                        Yield New List(Of T)(x) + subArray
                    Next
                Next
            End If
        End Function

        Public Function Generate(Of T)(source As T()()) As T()()
            Return source.Iteration.ToArray
        End Function
    End Module
End Namespace
