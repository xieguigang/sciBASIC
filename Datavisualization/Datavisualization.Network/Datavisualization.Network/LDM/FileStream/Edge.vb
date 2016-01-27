Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DataVisualization.Network.LDM.Abstract

Namespace FileStream

    ''' <summary>
    ''' The edge between the two nodes in the network.(节点与节点之间的相互关系)
    ''' </summary>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlType("VisualizeNode")>
    Public Class NetworkEdge : Inherits INetComponent
        Implements I_InteractionModel, INetworkEdge

        Public Shared Function Contains(edge As I_InteractionModel, node As String) As Boolean
            Return String.Equals(node, edge.FromNode, StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(node, edge.ToNode, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Const REFLECTION_ID_MAPPING_FROM_NODE As String = "fromNode"
        Public Const REFLECTION_ID_MAPPING_TO_NODE As String = "toNode"
        Public Const REFLECTION_ID_MAPPING_CONFIDENCE As String = "confidence"
        Public Const REFLECTION_ID_MAPPING_INTERACTION_TYPE As String = "InteractionType"

        <Column("fromNode")> <Xml.Serialization.XmlAttribute("Node_a")> Public Overridable Property FromNode As String Implements I_InteractionModel.FromNode
        <Column("toNode")> <Xml.Serialization.XmlAttribute("Node_b")> Public Overridable Property ToNode As String Implements I_InteractionModel.ToNode
        <Xml.Serialization.XmlAttribute("confidence")> Public Overridable Property Confidence As Double Implements INetworkEdge.Confidence
        <Column("InteractionType")> Public Overridable Property InteractionType As String Implements INetworkEdge.InteractionType

        ''' <summary>
        ''' 返回没有方向性的统一标识符
        ''' </summary>
        ''' <returns></returns>
        Public Function GetNullDirectedGuid() As String
            Dim array = {FromNode, ToNode}.OrderBy(Function(s) s)
            Return String.Format("[{0}] {1};{2}", InteractionType, array.First, array.Last)
        End Function

        Public Function GetDirectedGuid() As String
            Return $"{FromNode} {InteractionType} {ToNode}"
        End Function

        ''' <summary>
        ''' 起始节点是否是终止节点
        ''' </summary>
        ''' <returns></returns>
        <Ignored> Public ReadOnly Property SelfLoop As Boolean
            Get
                Return String.Equals(FromNode, ToNode, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        Public Overloads Shared Function Equals(Model As I_InteractionModel, Node1 As String, Node2 As String) As Boolean
            If String.Equals(Model.FromNode, Node1, StringComparison.OrdinalIgnoreCase) Then
                Return String.Equals(Model.ToNode, Node2, StringComparison.OrdinalIgnoreCase)
            ElseIf String.Equals(Model.ToNode, Node1, StringComparison.OrdinalIgnoreCase) Then
                Return String.Equals(Model.FromNode, Node2, StringComparison.OrdinalIgnoreCase)
            Else
                Return False
            End If
        End Function

        Public Shared Function GetConnectedNode(Node As I_InteractionModel, a As String) As String
            If String.Equals(Node.FromNode, a) Then
                Return Node.ToNode
            ElseIf String.Equals(Node.ToNode, a) Then
                Return Node.FromNode
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' 假若存在连接则返回相对的节点，否则返回空字符串
        ''' </summary>
        ''' <param name="Node"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetConnectedNode(Node As String) As String
            Return GetConnectedNode(Me, Node)
        End Function

        Public Overloads Function Equals(Id1 As String, Id2 As String) As Boolean
            Return (String.Equals(FromNode, Id1) AndAlso
                String.Equals(ToNode, Id2)) OrElse
                (String.Equals(FromNode, Id2) AndAlso
                String.Equals(ToNode, Id1))
        End Function

        Public Function IsEqual(OtherNode As NetworkEdge) As Boolean
            Return String.Equals(FromNode, OtherNode.FromNode) AndAlso
                String.Equals(ToNode, OtherNode.ToNode) AndAlso
                String.Equals(InteractionType, OtherNode.InteractionType) AndAlso
                Confidence = OtherNode.Confidence
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(ToNode) Then
                Return FromNode
            Else
                If String.IsNullOrEmpty(InteractionType) Then
                    Return String.Format("{0} --> {1}", FromNode, ToNode)
                Else
                    Return String.Format("{0} {1} {2}", FromNode, InteractionType, ToNode)
                End If
            End If
        End Function

        Public Shared Function GetNode(Node1 As String, Node2 As String, Network As NetworkEdge()) As NetworkEdge
            Dim LQuery = (From Node As NetworkEdge
                          In Network
                          Where String.Equals(Node1, Node.FromNode) AndAlso
                              String.Equals(Node2, Node.ToNode)
                          Select Node).ToArray

            If LQuery.Length > 0 Then Return LQuery(Scan0)

            LQuery = (From Node As NetworkEdge
                      In Network
                      Where String.Equals(Node1, Node.ToNode) AndAlso
                              String.Equals(Node2, Node.FromNode)
                      Select Node).ToArray
            If LQuery.IsNullOrEmpty Then
                Return Nothing
            Else
                Return LQuery.First
            End If
        End Function
    End Class
End Namespace