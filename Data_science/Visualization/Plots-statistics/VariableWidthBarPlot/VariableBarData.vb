Public Class VariableBarData

    Public Property Name As String
    Public Property Data As (width#, height#)
    Public Property Color As String

    Public Overrides Function ToString() As String
        Return $"{Name} [{Data.width} @ {Data.height}]"
    End Function
End Class