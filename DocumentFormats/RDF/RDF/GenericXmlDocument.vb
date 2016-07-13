#Region "Microsoft.VisualBasic::49ab62a2d30aa339bdf7a20f1644a7ce, ..\VisualBasic_AppFramework\DocumentFormats\RDF\RDF\GenericXmlDocument.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Xml

Namespace DocumentStream

    Public Class GenericXmlDocument
        Public Property Uri As String
        Public Property DocumentNodes As Node()

        Public Shared Function LoadDocument(Uri As String) As GenericXmlDocument
            Dim Document As GenericXmlDocument = New GenericXmlDocument With {.Uri = Uri}
            Dim XMLDoc As XmlDocument = New XmlDocument()

            Call XMLDoc.Load(Uri)
            Document.DocumentNodes = (From Node As XmlNode In XMLDoc.ChildNodes Select DocumentStream.Node.CreateObject(Node)).ToArray
            Return Document
        End Function

        Public Shared Function CreateObjectFromXmlText(xml As String) As GenericXmlDocument
            Dim XMLDoc As XmlDocument = New XmlDocument()
            Call XMLDoc.LoadXml(xml)

            Dim Document As GenericXmlDocument =
                New GenericXmlDocument With
                {
                    .DocumentNodes = (From Node As XmlNode In XMLDoc.ChildNodes Select DocumentStream.Node.CreateObject(Node)).ToArray}

            Return Document
        End Function

        Public Overrides Function ToString() As String
            Return Uri
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="NodePath">Path使用/或者\进行分割</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetNodesValue(NodePath As String) As Node()
            Dim Tokens As String() = GetTokens(NodePath)
            Dim Node As Node = DocumentNodes.Last
            Dim NodeList As Node() = Nothing

            For Each NodeName As String In Tokens
                Dim LQuery = (From NodeObject In Node.ChildNodes Where String.Equals(NodeName, NodeObject.Name) Select NodeObject).ToArray
                NodeList = LQuery

                If LQuery.Count = 1 Then
                    Node = LQuery.First
                Else
                    Exit For
                End If
            Next

            Return NodeList
        End Function

        Private Shared Function GetTokens(Path As String) As String()
            Dim Tokens As List(Of String) = New List(Of String)
            Dim TempChunk As String() = Path.Split(CChar("\"))
            For Each item In TempChunk
                Call Tokens.AddRange(item.Split(CChar("/")))
            Next

            Return Tokens.ToArray
        End Function
    End Class

    Public Class ElementNode
        Public Property Name As String
        Public Property Value As String
        ''' <summary>
        ''' 内部节点的XML字符串
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property InternalText As String
        Public Property Attributes As Attribute()

        Public Class Attribute
            Public Property Name As String
            Public Property Value As String

            Public Overrides Function ToString() As String
                Return String.Format("{0} = {1}", Name, Value)
            End Function

            Public Shared Function CreateObject(XmlNode As XmlNode) As Attribute()
                If XmlNode.Attributes Is Nothing Then
                    Return New Attribute() {}
                End If
                Dim LQuery = (From attrNode As XmlNode
                              In XmlNode.Attributes
                              Select New Attribute With {.Name = attrNode.Name, .Value = attrNode.Value}).ToArray
                Return LQuery
            End Function
        End Class

        Public Shared Function CreateObject(XmlNode As XmlNode) As ElementNode
            Dim Node As ElementNode = New ElementNode With {.Name = XmlNode.Name, .Value = XmlNode.Value, .InternalText = XmlNode.InnerXml}
            Node.Attributes = Attribute.CreateObject(XmlNode)
            Node.Value = GetNodeValue(XmlNode)
            Return Node
        End Function

        Protected Friend Shared Function GetNodeValue(XmlNode As XmlNode) As String
            Dim LQuery = (From Node As XmlNode In XmlNode.ChildNodes Where Node.NodeType = XmlNodeType.Text Select Node.Value).ToArray
            If LQuery.IsNullOrEmpty Then
                Return ""
            Else
                Return LQuery.First
            End If
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0} = {1}", Name, Value)
        End Function

        Public Shared Function IsXmlElementNode(XmlNode As XmlNode) As Boolean
            Dim LQuery = (From Node As XmlNode In XmlNode.ChildNodes Where Node.NodeType = XmlNodeType.Text Select 1).ToArray
            Return Not LQuery.IsNullOrEmpty
        End Function
    End Class

    Public Class Node : Inherits ElementNode

        Public Property ChildNodes As Node()
        Public Property ElementNodes As ElementNode()

        Public Overrides Function ToString() As String
            Return String.Format("{0} = {1}", Name, Value)
        End Function

        Public Shared Shadows Function CreateObject(XmlNode As XmlNode) As Node
            Dim Node As Node = New Node With {.Name = XmlNode.Name, .Value = XmlNode.Value, .InternalText = XmlNode.InnerXml}
            Node.ElementNodes = (From item As XmlNode
                                 In XmlNode.ChildNodes
                                 Where ElementNode.IsXmlElementNode(item)
                                 Select ElementNode.CreateObject(item)).ToArray
            Node.Attributes = ElementNode.Attribute.CreateObject(XmlNode)
            Node.Value = ElementNode.GetNodeValue(XmlNode)
            Dim LQuery = From ChildNode As XmlNode
                         In XmlNode.ChildNodes
                         Where Not ElementNode.IsXmlElementNode(ChildNode)
                         Select Node.CreateObject(ChildNode) '
            Node.ChildNodes = LQuery.ToArray

            Return Node
        End Function
    End Class
End Namespace
