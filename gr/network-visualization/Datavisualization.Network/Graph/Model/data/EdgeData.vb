Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graph

    Public Class EdgeData : Inherits GraphData

        ''' <summary>
        ''' 这个属性值一般是由两个节点之间的坐标位置所计算出来的欧几里得距离
        ''' </summary>
        ''' <returns></returns>
        Public Property length As Single
        Public Property bends As XYMetaHandle()
        Public Property color As SolidBrush

        Public Sub New()
            MyBase.New()

            length = 1.0F
        End Sub

        ''' <summary>
        ''' Value copy
        ''' </summary>
        ''' <param name="copy"></param>
        Sub New(copy As EdgeData)
            Me.label = copy.label
            Me.length = copy.length
            Me.Properties = New Dictionary(Of String, String)(copy.Properties)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function Clone() As EdgeData
            Return DirectCast(Me.MemberwiseClone, EdgeData)
        End Function
    End Class
End Namespace