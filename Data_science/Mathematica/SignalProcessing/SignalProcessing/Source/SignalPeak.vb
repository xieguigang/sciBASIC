#Region "Microsoft.VisualBasic::e16f35d05434851f5386c4ef4d0d7b04, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\SignalPeak.vb"

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

    '   Total Lines: 30
    '    Code Lines: 23 (76.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (23.33%)
    '     File Size: 995 B


    '     Class SignalPeak
    ' 
    '         Properties: max_intensity, offset, width
    ' 
    '         Function: GetSignalData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Distributions
Imports signalData = Microsoft.VisualBasic.Math.SignalProcessing.Signal

Namespace Source

    Public Class SignalPeak

        Public Property offset As Double
        Public Property max_intensity As Double
        Public Property width As Double

        Public Function GetSignalData(dt As Double, Optional scale As Double = 1.5) As signalData
            Dim tick As New List(Of (time As Double, value As Double))
            Dim right As Double = width * scale
            Dim xi As Double = -right
            Dim center As Double = width / 2

            Do While xi < right
                tick.Add((xi + offset, Gaussian.Gaussian(xi, max_intensity, center, width)))
                xi += dt
            Loop

            Return New signalData(
                (From ti In tick Select ti.time),
                (From ti In tick Select ti.value)
            )
        End Function

        Public Function GetSignalData(x As Double()) As Double()
            Dim center As Double = offset + width / 2
            Dim y As Double() = Gaussian.Gaussian(x, max_intensity, center, width)
            Return y
        End Function

    End Class
End Namespace
