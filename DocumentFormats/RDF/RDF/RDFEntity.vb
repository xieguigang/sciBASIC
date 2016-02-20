Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

''' <summary>
''' 在rdf之中被描述的对象实体
''' </summary>
''' 
<XmlType("Description")>
Public MustInherit Class RDFEntity : Implements sIdEnumerable, IReadOnlyId

    ''' <summary>
    ''' [资源] 是可拥有 URI 的任何事物
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("about")> Public Property Resource As String Implements sIdEnumerable.Identifier, IReadOnlyId.locusId
    ''' <summary>
    ''' [属性]   是拥有名称的资源
    ''' [属性值] 是某个属性的值，(请注意一个属性值可以是另外一个<see cref="Resource"/>）
    ''' xml文档在rdf反序列化之后，原有的类型定义之中除了自有的属性被保留下来了之外，具备指向其他资源的属性都被保存在了这个属性字典之中
    ''' </summary>
    ''' <returns></returns>
    <XmlIgnore>
    Public Property Properties As Dictionary(Of String, RDFEntity)
End Class
