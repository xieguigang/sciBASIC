Imports System.Runtime.CompilerServices

Namespace ApplicationServices

    Public Class PerformanceCounter

        Dim t0 As Date = Now
        Dim spans As New List(Of Counter)
        Dim checkpoint As Date

        Public ReadOnly Property Top As Counter()
            Get
                Return spans.OrderByDescending(Function(t) t.span1).ToArray
            End Get
        End Property

        <DebuggerStepThrough>
        Public Function [Set]() As PerformanceCounter
            t0 = Now
            checkpoint = Now
            spans.Clear()
            Return Me
        End Function

        Public Sub Mark(title As String)
            spans.Add(New Counter With {
                .task = title,
                .start = checkpoint,
                .span0 = Now - t0,
                .span1 = Now - checkpoint
            })
            checkpoint = Now
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCounters() As IEnumerable(Of Counter)
            Return spans.AsEnumerable
        End Function

        Public Overrides Function ToString() As String
            Return $"{spans.Count} samples, total time {StringFormats.ReadableElapsedTime(spans.Last.span0.TotalMilliseconds)}"
        End Function
    End Class

    Public Class Counter

        Public Property task As String
        Public Property start As Date
        Public Property span0 As TimeSpan
        Public Property span1 As TimeSpan

        Public Overrides Function ToString() As String
            Return $"[{task}]{vbTab}{StringFormats.Lanudry(span1)} @ {start.ToString}"
        End Function

    End Class
End Namespace