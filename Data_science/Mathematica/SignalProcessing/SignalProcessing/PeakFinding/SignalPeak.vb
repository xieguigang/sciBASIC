#Region "Microsoft.VisualBasic::54c2e31ba41305770897dd95e7e5fb75, Data_science\Mathematica\SignalProcessing\SignalProcessing\PeakFinding\SignalPeak.vb"

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

    '     Structure SignalPeak
    ' 
    '         Properties: rtmax, rtmin, snratio
    ' 
    '         Function: Subset, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace PeakFinding

    Public Structure SignalPeak

        Dim region As ITimeSignal()
        Dim integration As Double
        Dim baseline As Double

        Default Public ReadOnly Property tick(index As Integer) As ITimeSignal
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return region(index)
            End Get
        End Property

        Public ReadOnly Property rt As Double
            Get
                Return region _
                    .OrderByDescending(Function(a) a.intensity) _
                    .FirstOrDefault _
                    .time
            End Get
        End Property

        Public ReadOnly Property rtmin As Double
            Get
                Return region.First.time
            End Get
        End Property

        Public ReadOnly Property rtmax As Double
            Get
                Return region.Last.time
            End Get
        End Property

        Public ReadOnly Property snratio As Double
            Get
                Dim baseline As Double = Me.baseline
                Dim signals As Double = region.Sum(Function(a) a.intensity - baseline)
                Dim sn As Double = SignalProcessing.SNRatio(signals, baseline * region.Length)

                Return sn
            End Get
        End Property

        Public Function Subset(rtmin As Double, rtmax As Double) As SignalPeak
            Return New SignalPeak With {
                .integration = integration,
                .region = region _
                    .Where(Function(a) a.time >= rtmin AndAlso a.time <= rtmax) _
                    .ToArray
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{rtmin}, {rtmax}] {region.Length} ticks:  {region.Select(Function(a) a.intensity).JoinBy(", ")}"
        End Function

    End Structure
End Namespace
