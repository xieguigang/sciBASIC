#Region "Microsoft.VisualBasic::5866b0563a6297e715c6205f0c57b93e, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\BucketDictionary.vb"

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
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Collection

    ''' <summary>
    ''' An ultralarge size dictionary object.
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    Public Class BucketDictionary(Of K, V)
        Implements IReadOnlyDictionary(Of K, V)

        Friend ReadOnly __buckets As New List(Of Dictionary(Of K, V))
        ''' <summary>
        ''' 每一个字典之中的最大的元素数目
        ''' </summary>
        ReadOnly bucketSize As Integer

        Sub New(bucketSize As Integer)
            Me.bucketSize = bucketSize
        End Sub

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of K, V)).Count
            Get
                Return __buckets.Sum(Function(x) x.Count)
            End Get
        End Property

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

                    Dim min = LinqAPI.DefaultFirst(Of Dictionary(Of K, V)) <=
                        From x As Dictionary(Of K, V)
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
            Get
                Return __buckets.Select(Function(x) x.Keys).IteratesALL
            End Get
        End Property

        Public ReadOnly Property Values As IEnumerable(Of V) Implements IReadOnlyDictionary(Of K, V).Values
            Get
                Return __buckets.Select(Function(x) x.Values).IteratesALL
            End Get
        End Property

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
        Public Function CreateBuckets(Of T, K, V)(source As IEnumerable(Of T), getKey As Func(Of T, K), getValue As Func(Of T, V), Optional size As Integer = Short.MaxValue * 10) As BucketDictionary(Of K, V)
            Dim hash As New BucketDictionary(Of K, V)(size)
            Dim bucket As New Dictionary(Of K, V)

            For Each x In source
                Dim key As K = getKey(x)
                Dim value As V = getValue(x)
                Call bucket.Add(key, value)
                If bucket.Count >= size Then
                    hash.__buckets.Add(bucket)
                    bucket = New Dictionary(Of K, V)
                End If
            Next

            hash.__buckets.Add(bucket)

            Return hash
        End Function
    End Module
End Namespace
