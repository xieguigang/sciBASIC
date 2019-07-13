#Region "Microsoft.VisualBasic::95fb1dad6e52689ae619566b466a7ad2, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\Protocols\POST.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class InitPOSTBack
    ' 
    '         Properties: Portal, uid
    ' 
    '     Class UserId
    ' 
    '         Properties: sId, uid
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NETProtocol.Protocols

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
