Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.MaxMin


    ''' <summary>
    ''' Bucket that selects the first, the last event and events with maximum or minimum value
    ''' </summary>
    Public Class PIPlotBucket
        Inherits MMBucket

        Public Sub New()
        End Sub

        Public Sub New(size As Integer)
            MyBase.New(size)
        End Sub

        Public Sub New(e As ITimeSignal)
            MyBase.New(e)
        End Sub

        Public Overrides Sub selectInto(result As IList(Of ITimeSignal))
            Dim temp As IList(Of ITimeSignal) = New List(Of ITimeSignal)()
            MyBase.selectInto(temp)
            Dim [set] As New HashSet(Of ITimeSignal)()
            If temp.Count > 0 Then
                [set].Add(events(0))

                For Each item In temp
                    Call [set].Add(item)
                Next

                [set].Add(events(events.Count - 1))
            End If
            CType(result, List(Of ITimeSignal)).AddRange([set])
        End Sub

    End Class

End Namespace