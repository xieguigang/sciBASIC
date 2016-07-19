Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection

    Public Class BucketDictionary(Of K, V)

        ReadOnly __buckets As New List(Of Dictionary(Of K, V))
        ReadOnly bucketSize As Integer

        Sub New(bucketSize As Integer)
            Me.bucketSize = bucketSize
        End Sub


    End Class

    Public Module BucketDictionaryExtensions

        <Extension>
        Public Function CreateObject(Of T, K, V)(source As IEnumerable(Of T), getKey As Func(Of T, K), getValue As Func(Of T, V)) As BucketDictionary(Of K, V)
            Throw New Exception
        End Function
    End Module
End Namespace