Imports Microsoft.VisualBasic.Serialization

Namespace Protocols

    Public Class InitPOSTBack
        ''' <summary>
        ''' 长连接socket的端口终点
        ''' </summary>
        ''' <returns></returns>
        Public Property Portal As IPEndPoint
        Public Property uid As Long
    End Class

    Public Class UserId
        Public Property uid As Long
        Public Property sId As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class


End Namespace