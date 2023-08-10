Public Class Datum

    Private m_val As Double
    Private probs As Double()

    Public Sub New(ByVal value As String, ByVal components As Integer)
        m_val = Double.Parse(value)
        probs = New Double(components - 1) {}
        For i = 0 To probs.Length - 1
            probs(i) = 0.0
        Next
    End Sub

    Public Overridable Function val() As Double
        Return m_val
    End Function

    Public Overridable Sub setProb(ByVal i As Integer, ByVal val As Double)
        probs(i) = val
    End Sub

    Public Overridable Function getProb(ByVal i As Integer) As Double
        Return probs(i)
    End Function
End Class
