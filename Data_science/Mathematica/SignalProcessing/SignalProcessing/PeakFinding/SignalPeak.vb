#Region "Microsoft.VisualBasic::973b6a6783589d8620ce54d6db07767d, Data_science\Mathematica\SignalProcessing\SignalProcessing\PeakFinding\SignalPeak.vb"

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

    '   Total Lines: 100
    '    Code Lines: 78 (78.00%)
    ' Comment Lines: 3 (3.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (19.00%)
    '     File Size: 3.26 KB


    '     Structure SignalPeak
    ' 
    '         Properties: isEmpty, rt, rtmax, rtmin, signalMax
    '                     snratio
    ' 
    '         Function: Subset, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language.Default

Namespace PeakFinding

    Public Structure SignalPeak : Implements IsEmpty

        Dim region As ITimeSignal()

        ''' <summary>
        ''' the integration area percentage, value [0,100]
        ''' </summary>
        Dim integration As Double
        Dim baseline As Double

        Public ReadOnly Property isEmpty As Boolean Implements IsEmpty.IsEmpty
            Get
                Return region.IsNullOrEmpty AndAlso integration = 0 AndAlso baseline = 0
            End Get
        End Property

        Default Public ReadOnly Property tick(index As Integer) As ITimeSignal
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return region(index)
            End Get
        End Property

        Public ReadOnly Property rt As Double
            Get
                If region.IsNullOrEmpty Then
                    Return 0
                End If

                Return region _
                    .OrderByDescending(Function(a) a.intensity) _
                    .FirstOrDefault.time
            End Get
        End Property

        Public ReadOnly Property rtmin As Double
            Get
                If region.IsNullOrEmpty Then
                    Return 0
                End If

                Return region.First.time
            End Get
        End Property

        Public ReadOnly Property rtmax As Double
            Get
                If region.IsNullOrEmpty Then
                    Return 0
                End If

                Return region.Last.time
            End Get
        End Property

        Public ReadOnly Property signalMax As Double
            Get
                If region.IsNullOrEmpty Then
                    Return 0
                End If

                Return Aggregate tick As ITimeSignal
                       In region
                       Let data As Double = tick.intensity
                       Into Max(data)
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Subset(rtmin As Double, rtmax As Double) As SignalPeak
            Return New SignalPeak With {
                .integration = integration,
                .region = region _
                    .Where(Function(a) a.time >= rtmin AndAlso a.time <= rtmax) _
                    .ToArray
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{rtmin.ToString("F1")}s, {rtmax.ToString("F1")}s] {region.Length} ticks:  {region.Select(Function(a) a.intensity.ToString("F0")).JoinBy(", ")}"
        End Function

    End Structure
End Namespace
