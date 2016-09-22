Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 绘图区域的参数
''' </summary>
Public Structure GraphicsRegion

    Public Size As Size
    Public Margin As Size

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Structure