Imports System.Runtime.CompilerServices

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

        Sub New()
        End Sub

        Sub New(buckets As IEnumerable(Of IEnumerable(Of T)))
            For Each block As IEnumerable(Of T) In buckets
                Call Me.buckets.Add(block.ToArray)
            Next
        End Sub

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