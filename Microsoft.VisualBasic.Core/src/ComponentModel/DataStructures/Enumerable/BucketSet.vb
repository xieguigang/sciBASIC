#Region "Microsoft.VisualBasic::475bbcdeba38cbc62842319b1137f8b4, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Enumerable\BucketSet.vb"

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

    '   Total Lines: 53
    '    Code Lines: 42 (79.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (20.75%)
    '     File Size: 1.74 KB


    '     Class BucketSet
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ForEachBucket, GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace ComponentModel.Collection.Generic

    Public Class BucketSet(Of T) : Implements IEnumerable(Of T)

        ReadOnly buckets As New List(Of T())

        Public ReadOnly Property Count As Long
            Get
                Return Aggregate block As T()
                       In buckets
                       Let lngSize As Long = CLng(block.Length)
                       Into Sum(lngSize)
            End Get
        End Property

        Public ReadOnly Property PackSize As Integer()
            Get
                Return buckets.Select(Function(p) p.Length).ToArray
            End Get
        End Property

        Public ReadOnly Property Chunks As Integer
            Get
                Return buckets.Count
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(buckets As IEnumerable(Of IEnumerable(Of T)))
            For Each block As IEnumerable(Of T) In buckets
                Call Me.buckets.Add(block.ToArray)
            Next
        End Sub

        ''' <summary>
        ''' populate each pack data from the bucket data
        ''' </summary>
        ''' <returns></returns>
        Public Function ForEachBucket() As IEnumerable(Of T())
            Return buckets.AsEnumerable
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(block As IEnumerable(Of T))
            Call buckets.Add(block.ToArray)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(getData As Func(Of IEnumerable(Of T)))
            Call Add(getData())
        End Sub

        ''' <summary>
        ''' 根据全局索引获取元素
        ''' </summary>
        ''' <param name="index">全局索引（从0开始）</param>
        ''' <returns>指定索引处的元素</returns>
        Public Function GetItemByGlobalIndex(index As Long) As T
            If index < 0 OrElse index >= Count Then
                Throw New ArgumentOutOfRangeException(NameOf(index), "索引超出范围")
            End If

            Dim currentIndex As Long = 0
            For Each block In buckets
                If index < currentIndex + block.Length Then
                    Return block(index - currentIndex)
                End If
                currentIndex += block.Length
            Next

            Throw New ArgumentOutOfRangeException(NameOf(index), "索引超出范围")
        End Function

        ''' <summary>
        ''' 根据全局索引范围获取元素序列
        ''' </summary>
        ''' <param name="startIndex">起始索引（包含，从0开始）</param>
        ''' <param name="endIndex">结束索引（包含，从0开始）</param>
        ''' <returns>指定范围内的元素序列</returns>
        Public Iterator Function GetRange(startIndex As Long, endIndex As Long) As IEnumerable(Of T)
            If startIndex < 0 OrElse endIndex >= Count OrElse startIndex > endIndex Then
                Throw New ArgumentOutOfRangeException("索引范围无效")
            End If

            Dim currentIndex As Long = 0
            For Each block In buckets
                Dim blockStart As Long = currentIndex
                Dim blockEnd As Long = currentIndex + block.Length - 1

                ' 检查当前块是否与目标范围有重叠
                If blockEnd >= startIndex AndAlso blockStart <= endIndex Then
                    Dim startInBlock As Integer = std.Max(0, CInt(startIndex - blockStart))
                    Dim endInBlock As Integer = std.Min(block.Length - 1, CInt(endIndex - blockStart))

                    For i As Integer = startInBlock To endInBlock
                        Yield block(i)
                    Next
                End If

                currentIndex += block.Length
                If currentIndex > endIndex Then
                    Exit For
                End If
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"with {buckets.Count} buckets data"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each block As T() In buckets
                For Each item As T In block
                    Yield item
                Next
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
