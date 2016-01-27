Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DataVisualization.Network.LDM.Abstract
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace FileStream

    Public MustInherit Class INetComponent

        Dim _properties As Dictionary(Of String, String)

        <Meta(GetType(String))>
        Public Property Properties As Dictionary(Of String, String)
            Get
                If _properties Is Nothing Then
                    _properties = New Dictionary(Of String, String)
                End If
                Return _properties
            End Get
            Set(value As Dictionary(Of String, String))
                _properties = value
            End Set
        End Property
    End Class

    ''' <summary>
    ''' An node entity in the target network.(这个对象里面包含了网络之中的节点的实体的最基本的定义：节点的标识符以及节点的类型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Node : Inherits INetComponent

        Implements sIdEnumerable
        Implements INode

        ''' <summary>
        ''' 这个节点的标识符
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Identifier As String Implements sIdEnumerable.Identifier, INode.Identifer
        ''' <summary>
        ''' 这个节点的类型的定义
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property NodeType As String Implements INode.NodeType

        Public Const REFLECTION_ID_MAPPING_IDENTIFIER As String = "Identifier"
        Public Const REFLECTION_ID_MAPPING_NODETYPE As String = "NodeType"

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        Public Function CopyTo(Of T As Node)() As T
            Dim NewNode As T = Activator.CreateInstance(Of T)()
            NewNode.Identifier = Identifier
            NewNode.NodeType = NodeType

            Return NewNode
        End Function
    End Class
End Namespace