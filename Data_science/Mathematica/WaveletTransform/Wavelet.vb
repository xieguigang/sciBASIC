Imports System.Runtime.CompilerServices

Public Class Wavelet

    Friend DecompositionLow As Double()
    Friend DecompositionHigh As Double()

    Friend ReconstructionLow As Double()
    Friend ReconstructionHigh As Double()

    Public Name As String

    Public ReadOnly Property FilterLength() As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return DecompositionLow.Length
        End Get
    End Property

    ''' <summary>
    ''' For orthogonal wavelets
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="DecLowPass#"></param>
    Public Sub New(name$, DecLowPass#())
        Me.Name = name

        DecompositionLow = DecLowPass
        DecompositionHigh = New Double(DecLowPass.Length - 1) {}
        ReconstructionLow = New Double(DecLowPass.Length - 1) {}
        ReconstructionHigh = New Double(DecLowPass.Length - 1) {}

        For i As Integer = 0 To DecLowPass.Length - 1
            DecompositionHigh(i) = DecLowPass(DecLowPass.Length - i - 1)
            If i Mod 2 <> 0 Then
                DecompositionHigh(i) *= -1
            End If
            ReconstructionLow(i) = DecLowPass(DecLowPass.Length - i - 1)
        Next
        For i As Integer = 0 To DecLowPass.Length - 1
            ReconstructionHigh(i) = DecompositionHigh(DecLowPass.Length - i - 1)
        Next
    End Sub
End Class
