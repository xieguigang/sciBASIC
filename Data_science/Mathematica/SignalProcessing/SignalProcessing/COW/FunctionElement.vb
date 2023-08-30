
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace COW

    ''' <summary>
    ''' This class is used in dynamic programming algorithm of CowAlignment.cs.
    ''' </summary>
    Friend Class FunctionElement

        Public Property Warp As Integer
        Public Property Score As Double
        Public Property Trace As TraceDirection

        Public Sub New(score As Double, trace As TraceDirection)
            _Score = score
            _Trace = trace
        End Sub

        Public Sub New(score As Double, warp As Integer)
            _Score = score
            _Warp = warp
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
