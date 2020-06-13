Imports System.Drawing
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace Graph

    Public Class NodeData : Inherits GraphData

        ''' <summary>
        ''' Get length of the <see cref="neighbours"/> index array
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property neighborhoods As Integer
            Get
                If neighbours Is Nothing Then
                    Return 0
                Else
                    Return neighbours.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' 这个主要是为了兼容圆形或者矩形之类的大小信息
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Double()

        ''' <summary>
        ''' Mass weight
        ''' </summary>
        ''' <returns></returns>
        Public Property mass As Single

        ''' <summary>
        ''' For 2d layout <see cref="FDGVector2"/> / 3d layout <see cref="FDGVector3"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property initialPostion As AbstractVector
        Public Property origID As String
        Public Property force As Point

        ''' <summary>
        ''' 颜色<see cref="SolidBrush"/>或者绘图<see cref="TextureBrush"/>
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore>
        Public Property color As Brush

        <DumpNode>
        Public Property weights As Double()

        ''' <summary>
        ''' 与本节点相连接的其他节点的<see cref="Node.Label">编号</see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <DumpNode>
        Public Property neighbours As Integer()

        Public Sub New()
            MyBase.New()

            mass = 1.0F
            initialPostion = Nothing
            ' for merging the graph
            origID = ""
        End Sub

        Sub New(copy As NodeData)
            Me.color = copy.color
            Me.force = copy.force
            Me.initialPostion = copy.initialPostion
            Me.label = copy.label
            Me.mass = copy.mass
            Me.neighbours = copy.neighbours.SafeQuery.ToArray
            Me.origID = copy.origID
            Me.Properties = New Dictionary(Of String, String)(copy.Properties)
            Me.size = If(copy.size Is Nothing, {}, copy.size.ToArray)
            Me.weights = copy.weights.SafeQuery.ToArray
        End Sub

        Public Function Clone() As NodeData
            Return DirectCast(Me.MemberwiseClone, NodeData)
        End Function

        Public Overrides Function ToString() As String
            Return initialPostion.ToString
        End Function
    End Class
End Namespace