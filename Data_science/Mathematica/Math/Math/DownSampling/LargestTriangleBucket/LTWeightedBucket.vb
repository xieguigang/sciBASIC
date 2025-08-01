#Region "Microsoft.VisualBasic::64edd92a482078772dc87eda227a349d, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LTWeightedBucket.vb"

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

    '   Total Lines: 193
    '    Code Lines: 152 (78.76%)
    ' Comment Lines: 16 (8.29%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 25 (12.95%)
    '     File Size: 7.02 KB


    '     Class LTWeightedBucket
    ' 
    '         Properties: sse
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: [get], [select], average, calcSSE, copy
    '                   Equals, GetEnumerator, GetHashCode, IEnumerable_GetEnumerator, sequarErrors
    '                   size, ToString
    ' 
    '         Sub: add, initSize, selectInto
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.LargestTriangleBucket

    Public Class LTWeightedBucket : Implements IEnumerable(Of WeightedEvent), Bucket

        Private index As Integer = 0
        Private events() As WeightedEvent
        Private selected As WeightedEvent

        ''' <summary>
        ''' a virtual event represents the average value in next bucket
        ''' </summary>
        Private m_average As WeightedEvent

        Public Sub New()

        End Sub

        Public Sub New(ITimeSignal As WeightedEvent)
            index = 1
            events = New WeightedEvent() {ITimeSignal}
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

        Public Overridable Sub selectInto(result As IList(Of ITimeSignal)) Implements Bucket.selectInto
            For Each e As WeightedEvent In [select]()
                result.Add(e.Event)
            Next e
        End Sub

        Public Overridable Sub add(e As ITimeSignal) Implements Bucket.add
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

        ''' <summary>
        ''' a virtual event represents the average value in next bucket
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function average() As WeightedEvent
            If Nothing Is m_average Then
                If index = 1 Then
                    m_average = events(0)
                Else
                    Dim valueSum As Double = 0
                    Dim timeSum As Long = 0
                    For i As Integer = 0 To index - 1
                        Dim e As ITimeSignal = events(i)
                        valueSum += e.intensity
                        timeSum += e.time
                    Next i
                    m_average = New WeightedEvent(timeSum \ index, valueSum / index)
                End If
            End If
            Return m_average
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

        ''' <summary>
        ''' -1 means SSE not calculated yet
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property sse() As Double

        ''' <summary>
        ''' Calculate sum of squared errors, with one event in adjacent buckets overlapping
        ''' 
        ''' </summary>
        Public Overridable Function calcSSE(last As LTWeightedBucket, [next] As LTWeightedBucket) As Double
            If sse <= -1 Then
                Dim lastVal As Double = last.get(last.size() - 1).Value
                Dim nextVal As Double = [next].get(0).Value
                Dim avg As Double = lastVal + nextVal
                For i As Integer = 0 To index - 1
                    Dim e As ITimeSignal = events(i)
                    avg += e.intensity
                Next i
                avg = avg / (index + 2)
                Dim lastSe As Double = sequarErrors(lastVal, avg)
                Dim nextSe As Double = sequarErrors(nextVal, avg)
                _sse = lastSe + nextSe
                For i As Integer = 0 To index - 1
                    Dim e As ITimeSignal = events(i)
                    _sse += sequarErrors(e.intensity, avg)
                Next i
            End If
            Return sse
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
