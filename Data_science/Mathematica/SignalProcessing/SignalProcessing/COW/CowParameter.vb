#Region "Microsoft.VisualBasic::1ffeda6dfebe15334156917d3cbe0fc2, Data_science\Mathematica\SignalProcessing\SignalProcessing\COW\CowParameter.vb"

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

    '   Total Lines: 62
    '    Code Lines: 39 (62.90%)
    ' Comment Lines: 12 (19.35%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (17.74%)
    '     File Size: 2.79 KB


    '     Class CowParameter
    ' 
    '         Properties: ReferenceID, SegmentSize, Slack
    ' 
    '         Function: AutomaticParameterDefinder, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace COW

    ''' <summary>
    ''' This is the parameters of correlation optimized warping algorithm.
    ''' Please see Nielsen et.al. J. Chromatogr. A 805, 17–35 (1998).
    ''' </summary>
    Public Class CowParameter

        Public Property ReferenceID As Integer
        Public Property Slack As Integer
        Public Property SegmentSize As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' The point of dynamic programming based alignment is to get the suitable reference chromatogram.
        ''' Selecting the reference chromatogram which should look like 'center' of chromatograms will be better to get nice alignment results.
        ''' So, this program is used to get the suitable reference chromatogram from imported chromatograms.
        ''' Please see Tsugawa et. al. Front. Genet. 5:471, 2015
        ''' </summary>
        ''' <param name="chromatograms"></param>
        ''' <returns></returns>
        Public Shared Function AutomaticParameterDefinder(chromatograms As List(Of Double())) As CowParameter
            Dim chromatogramsNumber = chromatograms(0).Length - 3
            Dim gravityArray = New Double(chromatogramsNumber - 1) {}
            Dim totalIntensity, sum As Double, maxIntensity = Double.MinValue

            For i As Integer = 0 To chromatogramsNumber - 1
                sum = 0
                totalIntensity = 0

                For j = 0 To chromatograms.Count - 1
                    sum += chromatograms(j)(1) * chromatograms(j)(3 + i)
                    totalIntensity += chromatograms(j)(3 + i)
                    If maxIntensity < chromatograms(j)(3 + i) Then maxIntensity = chromatograms(j)(3 + i)
                Next

                gravityArray(i) = sum / totalIntensity
            Next

            Dim maxGravity, minGravity, centerGravity As Double
            maxGravity = gravityArray.Max
            minGravity = gravityArray.Min
            centerGravity = (maxGravity + minGravity) / 2

            Dim referenceID = which.Min((New Vector(gravityArray) - centerGravity).Abs)
            Dim slack As Integer = (maxGravity - minGravity) * (chromatograms(chromatograms.Count - 1)(0) - chromatograms(0)(0)) / (chromatograms(chromatograms.Count - 1)(1) - chromatograms(0)(1))
            Dim alignmentParameter As New CowParameter() With {
                .ReferenceID = referenceID,
                .Slack = slack
            }

            Return alignmentParameter
        End Function
    End Class
End Namespace
