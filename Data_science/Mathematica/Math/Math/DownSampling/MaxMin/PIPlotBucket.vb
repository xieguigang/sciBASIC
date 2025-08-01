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

        Public Sub New(e As [Event])
            MyBase.New(e)
        End Sub

        Public Overrides Sub selectInto(result As IList(Of [Event]))
            Dim temp As IList(Of [Event]) = New List(Of [Event])()
            MyBase.selectInto(temp)
            Dim [set] As New HashSet(Of [Event])()
            If temp.Count > 0 Then
                [set].Add(events(0))

                For Each item In temp
                    Call [set].Add(item)
                Next

                [set].Add(events(events.Count - 1))
            End If
            CType(result, List(Of [Event])).AddRange([set])
        End Sub

    End Class

End Namespace