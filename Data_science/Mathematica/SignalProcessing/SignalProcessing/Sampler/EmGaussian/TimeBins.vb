#Region "Microsoft.VisualBasic::a490b5129df8aee6825e242275be213d, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\EmGaussian\TimeBins.vb"

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

    '   Total Lines: 69
    '    Code Lines: 43 (62.32%)
    ' Comment Lines: 12 (17.39%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (20.29%)
    '     File Size: 2.40 KB


    '     Module TimeBins
    ' 
    '         Function: (+2 Overloads) Compose, TimeBins
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace EmGaussian

    Public Module TimeBins

        <Extension>
        Public Function TimeBins(sig As GeneralSignal, Optional max As Double = 100) As Double()
            Dim norm As Integer() = ((New Vector(sig.Strength) / sig.Strength.Max) * max).AsInteger
            Dim pool As New List(Of Double)

            For i As Integer = 0 To norm.Length - 1
                pool.AddRange(Replicate(sig.Measures(i), n:=norm(i)))
            Next

            Return pool.ToArray
        End Function

        ''' <summary>
        ''' compose a signal data based on a collection of the gauss peak data
        ''' </summary>
        ''' <param name="peaks"></param>
        ''' <param name="xmin"></param>
        ''' <param name="xmax"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Compose(peaks As IEnumerable(Of Variable),
                                xmin As Double,
                                xmax As Double,
                                Optional res As Integer = 1000) As GeneralSignal

            Dim dx As Double = (xmax - xmin) / res
            Dim x_axis As Double() = seq(xmin, xmax, by:=dx).ToArray
            Dim sig As GeneralSignal = peaks.Compose(x_axis)

            Return sig
        End Function

        ''' <summary>
        ''' compose a signal data based on a collection of the gauss peak data
        ''' </summary>
        ''' <param name="peaks"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Compose(peaks As IEnumerable(Of Variable), x_axis As Double()) As GeneralSignal
            Dim y_axis As Double() = New Double(x_axis.Length - 1) {}
            Dim v As Variable() = peaks.ToArray

            For i As Integer = 0 To x_axis.Length - 1
                Dim sum As Double = 0

                For Each peak As Variable In v
                    sum += peak.gaussian(x_axis(i))
                Next

                y_axis(i) = sum
            Next

            Return New GeneralSignal With {
                .Measures = x_axis,
                .Strength = y_axis,
                .description = "composed signal",
                .weight = 1
            }
        End Function

    End Module
End Namespace
