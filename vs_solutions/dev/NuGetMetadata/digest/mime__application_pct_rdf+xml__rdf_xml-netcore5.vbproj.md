# mime/application%rdf+xml/rdf_xml-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.application.rdf_xml
- AssemblyName  : Microsoft.VisualBasic.MIME.application.rdf_xml
- TargetFramework: net10.0
- Source files  : 14
- Existing Title: RDF/XML and Turtle Triple Serialization Model
- Existing Desc : Supports the application/rdf+xml MIME type for sciBASIC#: XML serialization models for RDF descriptions, bags and DCMI metadata, plus an RDF 1.1 Turtle parser that streams subject-predicate-object triples and re-materializes them as RDF entities.
- Existing Tags : scibasic;rdf;rdf-xml;turtle;semantic-web;dublin-core;dcmi

## Namespaces
- Turtle  [files: 5]

## Public types
- Class Array (DataModel\Bag.vb)
- Class li (DataModel\Bag.vb)
- Class DCMI (DataModel\DCMI.vb) - RDF 都柏林核心元数据倡议 (http://w3school.com.cn/rdf/rdf_dublin.asp) 都柏林核心元数据倡议 (DCMI) 已创建了一些供描述文档的预定义属性。
- Class Resource (DataModel\Resource.vb)
- Module DataTypes (DataTypes.vb) - http://www.w3.org/2001/XMLSchema
- Module NamespaceDoc (NamespaceDoc.vb) - RDF/XML serialization models for the ``application/rdf+xml`` MIME type: RDF description, bag and DCMI metadata objects that can be read from and written to RDF/XML documents. 在进行RDF反序列化读取操作的时候似乎存在一个BUG
- Class RDFEntity (RDFEntity.vb)
- Class RDFProperty (RDFProperty.vb) - property value with data type
- Class RDFType (RDFProperty.vb)
- Class EntityProperty (RDFProperty.vb) - RDF DataValue
- Module BuildObject (Turtle\BuildObject.vb)
- Module NamespaceDoc (Turtle\NamespaceDoc.vb) - A RDF 1.1 Turtle parser that streams subject-predicate-object triples and materializes them into the RDF object model.
- Class Triple (Turtle\Triple.vb) - object properties
- Class Relation (Turtle\Triple.vb) - property data
- Class ttl_property (Turtle\ttl_property.vb) - A simple key-value pair tuple data
- Class TurtleFile (Turtle\TurtleFile.vb) - RDF 1.1 Turtle Terse RDF Triple Language

## Notable public members
- Public Property list As li()
- Public Overrides Function ToString() As String
- Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
- Public Property resource As String
- Public Overrides Function ToString() As String
- Public Property Contributor As String
- Public Property Coverage As String
- Public Property Creator As String
- Public Property Format As String
- Public Property Description As String
- Public Property Identifier As String
- Public Property Language As String
- Public Property Publisher As String
- Public Property Relation As String
- Public Property Rights As String
- Public Property Source As String
- Public Property Subject As String
- Public Property Title As String
- Public Property Type As String
- Public Property about As String
- Public Property type As RDFType
- Public Property label As String
- Public Property comment As String
- Public Overrides Function ToString() As String
- Public Property resource As String
- Public Overrides Function ToString() As String
- Public Const dtString As String = "http://www.w3.org/2001/XMLSchema#string"
- Public Const dtInteger As String = "http://www.w3.org/2001/XMLSchema#int"
- Public Const dtDouble As String = "http://www.w3.org/2001/XMLSchema#float"
- Public Function SchemaDataType(x As EntityProperty) As Type
- Public Function SchemaDataType(type As Type) As String
- Public Property description As T()
- Public Const XmlnsNamespace$ = "http://www.w3.org/1999/02/22-rdf-syntax-ns#"
- Public Const rdfs As String = "http://www.w3.org/2000/01/rdf-schema#"
- Public Const xmlns_nil As String = "nil"
- Public Property range As RDFProperty
- Public Property comment As RDFProperty()
- Public Property about As String Implements INamedValue.Key, IReadOnlyId.Identity
- Public Overloads Property Properties As Dictionary(Of String, RDFEntity)
- Public Overrides Function ToString() As String
- Public Function GetTypeName() As String
- Public Property value As String()
- Protected Sub New(dt As String)
- Protected Sub New(type As Type)
- Public Function ParseValue() As Object
- Public Overrides Function ToString() As String
- Public Iterator Function PopulateObjects(ttl As IEnumerable(Of Triple)) As IEnumerable(Of RDFEntity)
- Public Property subject As String
- Public Property relations As Relation()
- Public Overrides Function ToString() As String
- Public Property predicate As String
- Public Property objs As String()
- Public Overrides Function ToString() As String
- Public Property subject As String
- Public Property value As String
- Public Overrides Function ToString() As String
- Public Shared Iterator Function LoadTuples(file As Stream) As IEnumerable(Of ttl_property)
- Public Iterator Function ReadObjects() As IEnumerable(Of Triple)
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection.Generic
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Language.Values
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Text
- System.IO
- System.Runtime.CompilerServices
- System.Xml.Serialization

## File tree
- DataModel\Bag.vb
- DataModel\DCMI.vb
- DataModel\Description.vb
- DataModel\Resource.vb
- DataTypes.vb
- NamespaceDoc.vb
- RDF.vb
- RDFEntity.vb
- RDFProperty.vb
- Turtle\BuildObject.vb
- Turtle\NamespaceDoc.vb
- Turtle\Triple.vb
- Turtle\ttl_property.vb
- Turtle\TurtleFile.vb

