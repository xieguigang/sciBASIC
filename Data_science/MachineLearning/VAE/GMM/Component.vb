Public Class Component
    Private weightField, meanField, stdevField As Double

    Public Sub New(ByVal weight As Double, ByVal mean As Double, ByVal stdev As Double)
        weightField = weight
        meanField = mean
        stdevField = stdev
    End Sub

    Public Overridable Property Weight As Double
        Get
            Return weightField
        End Get
        Set(ByVal value As Double)
            weightField = value
        End Set
    End Property

    Public Overridable Property Mean As Double
        Get
            Return meanField
        End Get
        Set(ByVal value As Double)
            meanField = value
        End Set
    End Property

    Public Overridable Property Stdev As Double
        Get
            Return stdevField
        End Get
        Set(ByVal value As Double)
            stdevField = value
        End Set
    End Property



End Class
