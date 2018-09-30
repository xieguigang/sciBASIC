Imports System.Collections
Imports System.Linq

Public MustInherit Class WaveletConstructor : Implements IEnumerable(Of Wavelet)

    ReadOnly wavelets As Wavelet()

    Sub New(wavelets As IEnumerable(Of Wavelet))
        Me.wavelets = wavelets.ToArray
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of Wavelet) Implements IEnumerable(Of Wavelet).GetEnumerator
        Throw New NotImplementedException()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException()
    End Function
End Class
