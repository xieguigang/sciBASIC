#Region "Microsoft.VisualBasic::1d19fe63b279eb9a79c08775376e129b, Data_science\Mathematica\SignalProcessing\SignalProcessing\WaveletTransform\Constructors\WaveletConstructor.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class WaveletConstructor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections
Imports System.Linq

Namespace WaveletTransform

    Public MustInherit Class WaveletConstructor : Implements IEnumerable(Of Wavelet)

        ReadOnly wavelets As Wavelet()

        Sub New(wavelets As IEnumerable(Of Wavelet))
            Me.wavelets = wavelets.ToArray
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Wavelet) Implements IEnumerable(Of Wavelet).GetEnumerator
            For Each wl In wavelets
                Yield wl
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
