Imports Microsoft.VisualBasic.Serialization.JSON

Namespace COW

    ''' <summary>
    ''' This is the parameters of correlation optimized warping algorithm.
    ''' Please see Nielsen et.al. J. Chromatogr. A 805, 17–35 (1998).
    ''' </summary>
    Public Class CowParameter

        Public Property ReferenceID As Integer
        Public Property Slack As Integer
        Public Property SegmentSize As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace