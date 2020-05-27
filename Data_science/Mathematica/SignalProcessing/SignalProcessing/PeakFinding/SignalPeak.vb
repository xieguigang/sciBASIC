#Region "Microsoft.VisualBasic::a892db87c395ca10c4076a0698abd7c1, Data_science\Mathematica\SignalProcessing\SignalProcessing\PeakFinding\SignalPeak.vb"

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
    '         Properties: rtmax, rtmin
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

        Default Public ReadOnly Property tick(index As Integer) As ITimeSignal
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return region(index)
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

        Public Function Subset(rtmin As Double, rtmax As Double) As SignalPeak
            Return New SignalPeak With {
                .integration = integration,
                .region = region _
                    .Where(Function(a) a.time >= rtmin AndAlso a.time <= rtmax) _
                    .ToArray
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{rtmin}, {rtmax}] {region.Length} ticks"
        End Function

    End Structure
End Namespace
