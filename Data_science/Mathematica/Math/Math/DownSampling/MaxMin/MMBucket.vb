Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin


    ''' <summary>
    ''' Bucket that selects events with maximum or minimum value
    ''' </summary>
    Public Class MMBucket
        Implements Bucket

        Protected Friend events As IList(Of ITimeSignal) = New List(Of ITimeSignal)()

        Public Sub New()
        End Sub

        Public Sub New(e As ITimeSignal)
            events.Add(e)
        End Sub

        Public Sub New(size As Integer)

        End Sub

        Public Overridable Sub selectInto(result As IList(Of ITimeSignal)) Implements Bucket.selectInto
            If events.Count <= 1 Then
                CType(result, List(Of ITimeSignal)).AddRange(events)
                Return
            End If
            Dim maxEvt As ITimeSignal = Nothing
            Dim minEvt As ITimeSignal = Nothing
            Dim max As Double = Double.Epsilon
            Dim min As Double = Double.MaxValue
            For Each e As ITimeSignal In events
                Dim val As Double = e.Value
                If val > max Then
                    maxEvt = e
                    max = e.Value
                End If
                If val < min Then
                    minEvt = e
                    min = e.Value
                End If
            Next e
            If maxEvt IsNot Nothing AndAlso minEvt IsNot Nothing Then
                Dim maxFirst As Boolean = maxEvt.time < minEvt.time
                If maxFirst Then
                    result.Add(maxEvt)
                    result.Add(minEvt)
                Else
                    result.Add(minEvt)
                    result.Add(maxEvt)
                End If
            ElseIf maxEvt Is Nothing AndAlso minEvt IsNot Nothing Then
                result.Add(minEvt)
            ElseIf maxEvt IsNot Nothing AndAlso minEvt Is Nothing Then
                result.Add(maxEvt)
            End If
        End Sub

        Public Overridable Sub add(e As ITimeSignal) Implements Bucket.add
            events.Add(e)
        End Sub

    End Class

End Namespace