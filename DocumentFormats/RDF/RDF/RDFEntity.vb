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
    Implements sIdEnumerable, IReadOnlyId

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
    <XmlAttribute(RDF.RDF_PREFIX & "about")> Public Property about As String Implements sIdEnumerable.Identifier, IReadOnlyId.Identity
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
''' LANS.SystemsBiology.AnalysisTools.DataVisualization.Interaction.Cytoscape.DocumentFormat.CytoscapeGraphView.GraphAttribute 
''' ---> System.InvalidOperationException: There was an error reflecting type 'LANS.SystemsBiology.AnalysisTools.DataVisualization.Interaction.Cytoscape.DocumentFormat.CytoscapeGraphView.GraphAttribute'. 
''' ---> System.InvalidOperationException: There was an error reflecting property 'RDF'. 
''' ---> System.InvalidOperationException: There was an error reflecting type 'LANS.SystemsBiology.AnalysisTools.DataVisualization.Interaction.Cytoscape.DocumentFormat.CytoscapeGraphView.DocumentElements.NetworkMetadata'. 
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