#Region "Microsoft.VisualBasic::41a62e8ca23286d534368fb294144320, Data_science\DataMining\DataMining\AprioriRules\Algorithm\ItemSet.vb"

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

    '   Total Lines: 144
    '    Code Lines: 105 (72.92%)
    ' Comment Lines: 5 (3.47%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 34 (23.61%)
    '     File Size: 4.48 KB


    '     Structure ItemSet
    ' 
    '         Properties: HashCode, Length
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo, Contains, Empty, Equals, GetCompareValue
    '                   GetHashCode, IsNullOrEmpty, PopLast, Remove, Slice
    '                   SorterSortTokens, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq.JoinExtensions

Namespace AprioriRules

    Public Structure ItemSet : Implements IComparable(Of ItemSet)

        Public Items As Item()

        Public ReadOnly Property Length As Integer
            Get
                Return Items.Length
            End Get
        End Property

        Default Public ReadOnly Property Item(offset As Integer) As Item
            Get
                Return Items(offset)
            End Get
        End Property

        Public ReadOnly Property HashCode As Integer
            Get
                Return GetHashCode()
            End Get
        End Property

        Sub New(scalar As Item)
            Items = {scalar}
        End Sub

        Sub New(items As IEnumerable(Of Item))
            Me.Items = items.ToArray
        End Sub

        ''' <summary>
        ''' 顺序敏感的项集哈希。
        ''' </summary>
        ''' <remarks>
        ''' 使用 <see cref="System.HashCode"/> 而不是手写的 ``31 * hash + x``：
        ''' 后者在 VB 默认开启的整数溢出检查下（本工程 Debug/Release|AnyCPU 均未设置
        ''' RemoveIntegerChecks）对 4 项以上的项集会直接抛出 OverflowException。
        ''' </remarks>
        Public Overrides Function GetHashCode() As Integer
            If IsNullOrEmpty() Then
                Return 0
            End If

            Dim hash As New System.HashCode()

            For Each element As Item In Items
                hash.Add(element.Code)
            Next

            Return hash.ToHashCode()
        End Function

        Public Function IsNullOrEmpty() As Boolean
            Return Items Is Nothing OrElse Items.Length = 0
        End Function

        Public Function SorterSortTokens() As ItemSet
            Return New ItemSet With {.Items = Items.OrderBy(Function(a) a).ToArray}
        End Function

        Public Overrides Function ToString() As String
            Return "{" & Items.Select(Function(i) i.Item).JoinBy(", ") & "}"
        End Function

        Public Function Contains(i As Item) As Boolean
            For Each i32 As Item In Items
                If i32 = i Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Function PopLast() As ItemSet
            Return New ItemSet(scalar:=Items.Last)
        End Function

        Public Shared Function Empty() As ItemSet
            Return New ItemSet With {.Items = {}}
        End Function

        ''' <summary>
        ''' GetRemaining: removes all child elements from parent
        ''' </summary>
        ''' <param name="child"></param>
        ''' <returns></returns>
        Public Function Remove(child As ItemSet) As ItemSet
            Dim remaining As New List(Of Item)

            For Each i As Item In Items
                If Not child.Contains(i) Then
                    Call remaining.Add(i)
                End If
            Next

            Return New ItemSet(remaining)
        End Function

        Public Function Slice(start As Integer, count As Integer) As ItemSet
            Return New ItemSet With {.Items = Items.Skip(start).Take(count).ToArray}
        End Function

        Public Overloads Shared Operator &(a As ItemSet, b As ItemSet) As ItemSet
            Return New ItemSet With {.Items = a.Items.JoinIterates(b.Items).ToArray}
        End Operator

        Public Overloads Shared Operator =(a As ItemSet, b As ItemSet) As Boolean
            Return a.Items.SequenceEqual(b.Items)
        End Operator

        Public Overloads Shared Operator <>(a As ItemSet, b As ItemSet) As Boolean
            Return Not a = b
        End Operator

        ''' <summary>
        ''' 逐元素精确比较：长度相同且每个位置的 <see cref="Item.Code"/> 都相同才算相等。
        ''' </summary>
        ''' <remarks>
        ''' 早期实现只比较两个 32 位哈希值，导致哈希碰撞的两个不同项集被当作同一个键
        ''' （支持度被算到错误的项集上，并会在候选字典里抛出
        ''' ``ArgumentException: An item with the same key has already been added``）。
        ''' </remarks>
        Public Overrides Function Equals(obj As Object) As Boolean
            If obj Is Nothing Then
                Return Items Is Nothing
            ElseIf Not TypeOf obj Is ItemSet Then
                Return False
            End If

            Dim other As ItemSet = DirectCast(obj, ItemSet)

            If Items Is Nothing OrElse other.Items Is Nothing Then
                Return Items Is Nothing AndAlso other.Items Is Nothing
            End If
            If Items.Length <> other.Items.Length Then
                Return False
            End If

            For i As Integer = 0 To Items.Length - 1
                If Items(i).Code <> other.Items(i).Code Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 按 <see cref="Item.Code"/> 的字典序比较，短项集排在前面。
        ''' </summary>
        ''' <remarks>
        ''' 早期实现用 ``p += code * (10 ^ i)``（注意 VB 的 ^ 是幂运算）得到一个 ULong 比较值，
        ''' 既不是真正的字典序，在 i 较大时还会因 ULong 溢出/转换直接抛 OverflowException。
        ''' </remarks>
        Public Function CompareTo(other As ItemSet) As Integer Implements IComparable(Of ItemSet).CompareTo
            If IsNullOrEmpty() Then
                If other.IsNullOrEmpty() Then
                    Return 0
                Else
                    Return -1
                End If
            ElseIf other.IsNullOrEmpty() Then
                Return 1
            End If

            Dim n As Integer = System.Math.Min(Items.Length, other.Items.Length)

            For i As Integer = 0 To n - 1
                If Items(i).Code < other.Items(i).Code Then
                    Return -1
                ElseIf Items(i).Code > other.Items(i).Code Then
                    Return 1
                End If
            Next

            Return Items.Length.CompareTo(other.Items.Length)
        End Function
    End Structure

End Namespace
