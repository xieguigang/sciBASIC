Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 手工指定Axis的数据
''' </summary>
Public Class AxisValue

    Public Property Range As DoubleRange
    Public Property Tick As Double
    Public Property Title As String
    Public Property Font As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Class

Public Structure AxisData

    Dim X, Y As AxisValue

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Structure