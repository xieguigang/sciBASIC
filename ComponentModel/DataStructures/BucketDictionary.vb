#Region "Microsoft.VisualBasic::f75d2f33ac4ac1a36e313aad67ca0b54, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\BucketDictionary.vb"

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

    '     Class BucketDictionary
    ' 
    '         Properties: Count, Keys, Values
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ContainsKey, GetEnumerator, IEnumerable_GetEnumerator, ToString, TryGetValue
    ' 
    '     Module BucketDictionaryExtensions
    ' 
    '         Function: (+2 Overloads) CreateBuckets
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Collection

    ''' <summary>
    ''' An ultralarge size dictionary object.
    ''' (当你发现一个数据集合非常的大的时候，一个字典会出现溢出，则这个时候就需要这个超大容量的Bucket字典容器了)
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    Public Class BucketDictionary(Of K, V) : Implements IReadOnlyDictionary(Of K, V)

        Friend ReadOnly __buckets As New List(Of Dictionary(Of K, V))
        ''' <summary>
        ''' 每一个字典之中的最大的元素数目
        ''' </summary>
        ReadOnly bucketSize As Integer

        Sub New(bucketSize As Integer)
            Me.bucketSize = bucketSize
        End Sub

        Sub New()
            Call Me.New(Short.MaxValue * 10)
        End Sub

        ''' <summary>
        ''' 获取这个超大的字典集合之中的对象的数量总数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of K, V)).Count
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return __buckets.Sum(Function(x) x.Count)
            End Get
        End Property

        ''' <summary>
        ''' 注意，不要直接使用这个方法来添加新的数据，使用<see cref="BucketDictionaryExtensions"/>的方法会更加高效
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Default Public Property Item(key As K) As V Implements IReadOnlyDictionary(Of K, V).Item
            Get
                For Each hash In __buckets
                    If hash.ContainsKey(key) Then
                        Return hash(key)
                    End If
                Next

                Return Nothing
            End Get
            Set(value As V)
                If __buckets.Count = 0 Then
                    Call __buckets.Add(New Dictionary(Of K, V) From {{key, value}})
                Else
                    For Each hash In __buckets
                        If hash.ContainsKey(key) Then
                            hash(key) = value
                            Return
                        End If
                    Next

                    Dim min = LinqAPI.DefaultFirst(Of Dictionary(Of K, V)) _
 _
                        () <= From x As Dictionary(Of K, V)
                              In __buckets
                              Select x
                              Order By x.Count Ascending

                    min(key) = value

                    If min.Count >= bucketSize Then
                        Call __buckets.Add(New Dictionary(Of K, V))
                    End If
                End If
            End Set
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of K) Implements IReadOnlyDictionary(Of K, V).Keys
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return __buckets.Select(Function(x) x.Keys).IteratesALL
            End Get
        End Property

        Public ReadOnly Property Values As IEnumerable(Of V) Implements IReadOnlyDictionary(Of K, V).Values
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return __buckets.Select(Function(x) x.Values).IteratesALL
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"Tuple of [{GetType(K).Name}, {GetType(V).Name}] with {__buckets.Count} buckets."
        End Function

        Public Function ContainsKey(key As K) As Boolean Implements IReadOnlyDictionary(Of K, V).ContainsKey
            For Each hash In __buckets
                If hash.ContainsKey(key) Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of K, V)) Implements IEnumerable(Of KeyValuePair(Of K, V)).GetEnumerator
            For Each hash In __buckets
                For Each x In hash
                    Yield x
                Next
            Next
        End Function

        Public Function TryGetValue(key As K, ByRef value As V) As Boolean Implements IReadOnlyDictionary(Of K, V).TryGetValue
            For Each hash In __buckets
                If hash.ContainsKey(key) Then
                    value = hash(key)
                    Return True
                End If
            Next

            Return False
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    Public Module BucketDictionaryExtensions

        <Extension>
        Public Function CreateBuckets(Of T, K, V)(source As IEnumerable(Of T), getKey As Func(Of T, K), getValue As Func(Of T, V), Optional size% = Short.MaxValue * 10) As BucketDictionary(Of K, V)
            Dim table As New BucketDictionary(Of K, V)(size)
            Dim bucket As New Dictionary(Of K, V)

            For Each x As T In source
                Dim key As K = getKey(x)
                Dim value As V = getValue(x)

                Call bucket.Add(key, value)

                If bucket.Count >= size Then
                    table.__buckets.Add(bucket)
                    bucket = New Dictionary(Of K, V)
                End If
            Next

            table.__buckets.Add(bucket)

            Return table
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateBuckets(Of K, V)(source As IEnumerable(Of (K, V)), Optional size% = Short.MaxValue * 10) As BucketDictionary(Of K, V)
            Return source.CreateBuckets(Function(t) t.Item1, Function(t) t.Item2, size:=size)
        End Function
    End Module
End Namespace
