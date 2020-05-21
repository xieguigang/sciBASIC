
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