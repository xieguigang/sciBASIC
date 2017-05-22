Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    ''' <summary>
    ''' 字典之中的键名都是一个条件表达式，selector的数据源为``<see cref="NodeData"/>``和``<see cref="EdgeData"/>``
    ''' </summary>
    Public Class StyleJSON

        Public Property nodes As Dictionary(Of String, NodeStyle)
        Public Property edge As Dictionary(Of String, EdgeStyle)
        ''' <summary>
        ''' 这个指的是node label
        ''' </summary>
        ''' <returns></returns>
        Public Property labels As Dictionary(Of String, LabelStyle)

    End Class

    Public MustInherit Class Styles

        ''' <summary>
        ''' 线条的样式
        ''' </summary>
        ''' <returns></returns>
        Public Property stroke As String
        ''' <summary>
        ''' 节点的填充颜色，边的填充颜色，以及文本的填充颜色
        ''' </summary>
        ''' <returns></returns>
        Public Property fill As String

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class

    Public Class NodeStyle : Inherits Styles

        ''' <summary>
        ''' 节点大小的表达式
        ''' </summary>
        ''' <returns></returns>
        Public Property size As String
        ''' <summary>
        ''' 节点的形状的表达式
        ''' </summary>
        ''' <returns></returns>
        Public Property shape As String

    End Class

    Public Class EdgeStyle : Inherits Styles
    End Class

    Public Class LabelStyle : Inherits Styles

        ''' <summary>
        ''' CSS字体表达式
        ''' </summary>
        ''' <returns></returns>
        Public Property FontCSS As String

    End Class
End Namespace