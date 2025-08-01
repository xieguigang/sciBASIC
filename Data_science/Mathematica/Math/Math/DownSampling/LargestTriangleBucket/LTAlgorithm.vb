Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling.LargestTriangleBucket


    ''' <summary>
    ''' Largest Triangle Bucket Algorithm family.
    ''' <ul>
    ''' <li>LTOB: Largest Triangle One Bucket</li>
    ''' <li>LTTB: Largest Triangle Three Bucket</li>
    ''' <li>LTD: Largest Triangle Dynamic (three bucket)</li>
    ''' </ul>
    ''' </summary>
    Public Class LTAlgorithm
        Inherits BucketBasedAlgorithm(Of LTWeightedBucket, WeightedEvent)

        Protected Friend triangle As New Triangle()
        Protected Friend wcalc_Conflict As LTWeightCalculator

        Friend Sub New()
        End Sub

        Protected Friend Overrides Function prepare(data As IList(Of ITimeSignal)) As IList(Of WeightedEvent)
            Dim result As IList(Of WeightedEvent) = New List(Of WeightedEvent)(data.Count)
            For Each ITimeSignal As ITimeSignal In data
                result.Add(New WeightedEvent(ITimeSignal))
            Next ITimeSignal
            Return result
        End Function

        Protected Friend Overrides Sub beforeSelect(buckets As IList(Of LTWeightedBucket), threshold As Integer)
            wcalc_Conflict.calcWeight(triangle, buckets)
        End Sub

        Public Overridable WriteOnly Property Wcalc As LTWeightCalculator
            Set(wcalc As LTWeightCalculator)
                Me.wcalc_Conflict = wcalc
            End Set
        End Property

        Public Overrides Function ToString() As String
            Dim name As String = "LT"
            If TypeOf Me.wcalc_Conflict Is LTOneBucketWeightCalculator Then
                name &= "O"
            ElseIf TypeOf Me.wcalc_Conflict Is LTThreeBucketWeightCalculator Then
                name &= "T"
            End If
            If TypeOf Me.spliter Is LTDynamicBucketSplitter Then
                name &= "D"
            Else
                name &= "B"
            End If
            Return name
        End Function

    End Class

End Namespace