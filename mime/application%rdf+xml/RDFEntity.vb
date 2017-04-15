#Region "Microsoft.VisualBasic::eb782f1f9072bcff97af46ce30d01e92, ..\sciBASIC#\mime\RDF\RDFEntity.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' 在rdf之中被描述的对象实体
''' </summary>
''' 
<XmlType(RDF.RDF_PREFIX & "Description")>
Public MustInherit Class RDFEntity : Inherits RDFProperty
    Implements INamedValue, IReadOnlyId

    Public Property range As RDFProperty
    Public Property comment As RDFProperty

    ''' <summary>
    ''' rdf:ID
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute(RDF.RDF_PREFIX & "ID")> Public Property RDFId As String

    ''' <summary>
    ''' [资源] 是可拥有 URI 的任何事物
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute(RDF.RDF_PREFIX & "about")> Public Property about As String Implements INamedValue.Key, IReadOnlyId.Identity
    ''' <summary>
    ''' [属性]   是拥有名称的资源
    ''' [属性值] 是某个属性的值，(请注意一个属性值可以是另外一个<see cref="Resource"/>）
    ''' xml文档在rdf反序列化之后，原有的类型定义之中除了自有的属性被保留下来了之外，具备指向其他资源的属性都被保存在了这个属性字典之中
    ''' </summary>
    ''' <returns></returns>
    <XmlIgnore>
    Public Property Properties As Dictionary(Of String, RDFEntity)

    Public Overrides Function ToString() As String
        Return RDFId & "  // " & about
    End Function
End Class

Public Class RDFProperty : Inherits EntityProperty

End Class

''' <summary>
''' 
''' </summary>
''' <remarks>
''' 2016.5.29
''' 
''' 请注意，在这里的对<see cref="ClassObject"/>类型的继承是为了解决simpleContent的BUG的:
''' 
''' System.Exception: 
''' SMRUCC.genomics.AnalysisTools.DataVisualization.Interaction.Cytoscape.DocumentFormat.CytoscapeGraphView.GraphAttribute 
''' ---> System.InvalidOperationException: There was an error reflecting type 'SMRUCC.genomics.AnalysisTools.DataVisualization.Interaction.Cytoscape.DocumentFormat.CytoscapeGraphView.GraphAttribute'. 
''' ---> System.InvalidOperationException: There was an error reflecting property 'RDF'. 
''' ---> System.InvalidOperationException: There was an error reflecting type 'SMRUCC.genomics.AnalysisTools.DataVisualization.Interaction.Cytoscape.DocumentFormat.CytoscapeGraphView.DocumentElements.NetworkMetadata'. 
''' ---> System.InvalidOperationException: Cannot serialize object of type '<see cref="RDFEntity"/>'. 
''' 
''' Base type '<see cref="RDFProperty"/>' has simpleContent and can only be extended by adding XmlAttribute elements. 
''' Please consider changing XmlText member of the base class to string array.
''' </remarks>
Public MustInherit Class EntityProperty : Inherits ClassObject

    ''' <summary>
    ''' rdf:datatype
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute(RDF.RDF_PREFIX & "datatype")> Public Property dataType As String
    ''' <summary>
    ''' rdf:resource
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute(RDF.RDF_PREFIX & "resource")> Public Property resource As String
    <XmlText> Public Property value As String

    Sub New()
    End Sub

    Protected Sub New(dt As String)
        dataType = dt
    End Sub

    Protected Sub New(type As Type)
        Call Me.New(type.SchemaDataType)
    End Sub

    Public Overrides Function ToString() As String
        Return $"({Me.SchemaDataType.ToString}) {value}; resource: {resource}"
    End Function
End Class
