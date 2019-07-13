#Region "Microsoft.VisualBasic::2a59f7cff3f138c0ce372ef833dab3dd, gr\network-visualization\Datavisualization.Network\IO\FileStream\csv\Node.vb"

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

    '     Class Node
    ' 
    '         Properties: ID, NodeType
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CopyTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace FileStream

    ''' <summary>
    ''' An node entity in the target network.(这个对象里面包含了网络之中的节点的实体的最基本的定义：节点的标识符以及节点的类型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Node : Inherits IDynamicsTable
        Implements INamedValue
        Implements INode

        ''' <summary>
        ''' 这个节点的标识符
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property ID As String Implements INamedValue.Key, INode.Id
        ''' <summary>
        ''' Node data groups identifier.(这个节点的分组类型的定义)
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property NodeType As String Implements INode.NodeType

        Public Overrides Function ToString() As String
            Return ID
        End Function

        Sub New()
        End Sub

        Sub New(sId As String)
            ID = sId
        End Sub

        Sub New(sid As String, type As String)
            Call Me.New(sid)
            NodeType = type
        End Sub

        Public Function CopyTo(Of T As Node)() As T
            Dim newNode As T = Activator.CreateInstance(Of T)()
            newNode.ID = ID
            newNode.NodeType = NodeType
            newNode.Properties = New Dictionary(Of String, String)(Properties)

            Return newNode
        End Function
    End Class
End Namespace
