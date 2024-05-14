#Region "Microsoft.VisualBasic::a39d54db36a4ea62618cd72a76de6603, Data_science\Mathematica\SignalProcessing\SignalProcessing\WaveletTransform\Wavelet.vb"

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

    '   Total Lines: 47
    '    Code Lines: 33
    ' Comment Lines: 5
    '   Blank Lines: 9
    '     File Size: 1.61 KB


    '     Class Wavelet
    ' 
    '         Properties: FilterLength
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace WaveletTransform

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
End Namespace
