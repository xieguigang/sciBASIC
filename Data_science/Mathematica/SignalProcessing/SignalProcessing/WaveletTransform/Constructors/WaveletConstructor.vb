#Region "Microsoft.VisualBasic::1d19fe63b279eb9a79c08775376e129b, Data_science\Mathematica\SignalProcessing\SignalProcessing\WaveletTransform\Constructors\WaveletConstructor.vb"

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

    '   Total Lines: 24
    '    Code Lines: 18 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (25.00%)
    '     File Size: 759 B


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
