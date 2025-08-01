Namespace DownSampling.LargestTriangleBucket


    Public Class LTWeightedBucket
        Implements IEnumerable(Of WeightedEvent), Bucket

        Private index As Integer = 0
        Private events() As WeightedEvent
        Private selected As WeightedEvent
        ' a virtual event represents the average value in next bucket
        Private average_Conflict As WeightedEvent
        ' -1 means SSE not calculated yet
        Private sse_Conflict As Double = -1

        Public Sub New()

        End Sub

        Public Sub New([event] As WeightedEvent)
            index = 1
            events = New WeightedEvent() {[event]}
        End Sub

        Public Sub New(size As Integer)
            If size <= 0 Then
                Throw New System.ArgumentException("Bucket size must be positive")
            End If
            events = New WeightedEvent(size - 1) {}
        End Sub

        Public Overridable Function copy() As LTWeightedBucket
            Dim b As New LTWeightedBucket(events.Length)
            b.index = index
            For i As Integer = 0 To index - 1
                b.events(i) = New WeightedEvent(events(i).Event)
            Next i
            Return b
        End Function

        Public Overridable Sub initSize(size As Integer)
            If events Is Nothing Then
                events = New WeightedEvent(size - 1) {}
            End If
        End Sub

        Public Overridable Sub selectInto(result As IList(Of [Event])) Implements Bucket.selectInto
            For Each e As WeightedEvent In [select]()
                result.Add(e.Event)
            Next e
        End Sub

        Public Overridable Sub add(e As [Event]) Implements Bucket.add
            If index < events.Length Then
                events(index) = DirectCast(e, WeightedEvent)
                index += 1
            End If
        End Sub

        Public Overridable Function [get](i As Integer) As WeightedEvent
            Return If(i < index, events(i), Nothing)
        End Function

        Public Overridable Function size() As Integer
            Return index
        End Function

        Public Overridable Function average() As WeightedEvent
            If Nothing Is average_Conflict Then
                If index = 1 Then
                    average_Conflict = events(0)
                Else
                    Dim valueSum As Double = 0
                    Dim timeSum As Long = 0
                    For i As Integer = 0 To index - 1
                        Dim e As [Event] = events(i)
                        valueSum += e.Value
                        timeSum += e.Time
                    Next i
                    average_Conflict = New WeightedEvent(timeSum \ index, valueSum / index)
                End If
            End If
            Return average_Conflict
        End Function

        Public Overridable Function [select]() As WeightedEvent()
            If index = 0 Then
                Return New WeightedEvent() {}
            End If
            If Nothing Is selected Then
                If index = 1 Then
                    selected = events(0)
                Else
                    Dim max As Double = Double.Epsilon
                    Dim maxIndex As Integer = 0
                    For i As Integer = 0 To index - 1
                        Dim w As Double = events(i).Weight
                        If w > max Then
                            maxIndex = i
                            max = w
                        End If
                    Next i
                    selected = events(maxIndex)
                End If
            End If
            Return New WeightedEvent() {selected}
        End Function

        Public Overridable Function sse() As Double
            Return sse_Conflict
        End Function

        ''' <summary>
        ''' Calculate sum of squared errors, with one event in adjacent buckets overlapping
        ''' 
        ''' </summary>
        Public Overridable Function calcSSE(last As LTWeightedBucket, [next] As LTWeightedBucket) As Double
            If sse_Conflict = -1 Then
                Dim lastVal As Double = last.get(last.size() - 1).Value
                Dim nextVal As Double = [next].get(0).Value
                Dim avg As Double = lastVal + nextVal
                For i As Integer = 0 To index - 1
                    Dim e As [Event] = events(i)
                    avg += e.Value
                Next i
                avg = avg / (index + 2)
                Dim lastSe As Double = sequarErrors(lastVal, avg)
                Dim nextSe As Double = sequarErrors(nextVal, avg)
                sse_Conflict = lastSe + nextSe
                For i As Integer = 0 To index - 1
                    Dim e As [Event] = events(i)
                    sse_Conflict += sequarErrors(e.Value, avg)
                Next i
            End If
            Return sse_Conflict
        End Function

        Public Overrides Function ToString() As String
            ' Return Arrays.toString(events)
            Throw New NotImplementedException
        End Function

        Public Overrides Function GetHashCode() As Integer
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + events.Select(Function(evt) evt.GetHashCode).Sum
            result = prime * result + index
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Me.GetType() <> obj.GetType() Then
                Return False
            End If
            Dim other As LTWeightedBucket = DirectCast(obj, LTWeightedBucket)
            If Not events.SequenceEqual(other.events) Then
                Return False
            End If
            If index <> other.index Then
                Return False
            End If
            Return True
        End Function

        Private Function sequarErrors(d As Double, avg As Double) As Double
            Dim e As Double = d - avg
            Return e * e
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of WeightedEvent) Implements IEnumerable(Of WeightedEvent).GetEnumerator
            For Each evt As WeightedEvent In events
                Yield evt
            Next
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class

End Namespace