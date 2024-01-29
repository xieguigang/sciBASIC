Imports System.Runtime.CompilerServices

Namespace ApplicationServices

    Public Class PerformanceCounter

        Dim t0 As Date = Now
        Dim spans As New List(Of TimeCounter)
        Dim checkpoint As Date

        Public ReadOnly Property Top As TimeCounter()
            Get
                Return spans.OrderByDescending(Function(t) t.span1).ToArray
            End Get
        End Property

        ''' <summary>
        ''' reset the counter
        ''' </summary>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Function [Set]() As PerformanceCounter
            t0 = Now
            checkpoint = Now
            spans.Clear()
            Return Me
        End Function

        ''' <summary>
        ''' create a checkpoint
        ''' </summary>
        ''' <param name="title"></param>
        Public Function Mark(title As String) As TimeCounter
            Dim _checkpoint As New TimeCounter With {
                .task = title,
                .start = checkpoint,
                .span0 = Now - t0,
                .span1 = Now - checkpoint
            }
            spans.Add(_checkpoint)
            checkpoint = Now

            Return _checkpoint
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCounters() As IEnumerable(Of TimeCounter)
            Return spans.AsEnumerable
        End Function

        Public Overrides Function ToString() As String
            Return $"{spans.Count} samples, total time {StringFormats.ReadableElapsedTime(spans.Last.span0.TotalMilliseconds)}"
        End Function
    End Class

    Public Class TimeCounter

        Public Property task As String
        Public Property start As Date
        Public Property span0 As TimeSpan
        Public Property span1 As TimeSpan

        Public Overrides Function ToString() As String
            Return $"[{task}]{vbTab}{StringFormats.Lanudry(span1)} @ {start.ToString}"
        End Function

    End Class
End Namespace