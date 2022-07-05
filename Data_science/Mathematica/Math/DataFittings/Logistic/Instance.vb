Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' classify training model
''' </summary>
Public Class Instance

    ''' <summary>
    ''' the real label data
    ''' </summary>
    ''' <returns></returns>
    Public Property label As Integer
    ''' <summary>
    ''' the object properties vector
    ''' </summary>
    ''' <returns></returns>
    Public Property x As Double()

    Public ReadOnly Property featureSize As Integer
        Get
            Return x.Length
        End Get
    End Property

    Public Sub New(label As Integer, x As Integer())
        Me.label = label
        Me.x = x.Select(Function(d) CDbl(d)).ToArray
    End Sub

    Public Sub New(label As Integer, x As Double())
        Me.label = label
        Me.x = x
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{label}] {x.GetJson}"
    End Function
End Class