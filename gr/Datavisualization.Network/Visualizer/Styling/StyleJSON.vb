Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    ''' <summary>
    ''' 字典之中的键名都是一个条件表达式
    ''' </summary>
    Public Class StyleJSON

        Public Property nodes As Dictionary(Of String, Style)
        Public Property edge As Dictionary(Of String, Style)
        ''' <summary>
        ''' 这个指的是node label
        ''' </summary>
        ''' <returns></returns>
        Public Property labels As Dictionary(Of String, Style)

    End Class

    ''' <summary>
    ''' 网络模型对象的绘图样式
    ''' </summary>
    Public Structure Style

        Public Property stroke As String
        Public Property fontCSS As String
        Public Property fill As String
        Public Property size As String
        ''' <summary>
        ''' The display label source name
        ''' </summary>
        ''' <returns></returns>
        Public Property label As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace