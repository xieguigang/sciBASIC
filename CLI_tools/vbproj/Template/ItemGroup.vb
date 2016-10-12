Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ItemGroup

    <XmlElement("Reference")>
    Public Property References As IncludeItem()
    <XmlElement("Import")>
    Public Property [Imports] As IncludeItem()
    <XmlElement("Compile")>
    Public Property Compiles As IncludeItem()
    <XmlElement("None")>
    Public Property Nones As IncludeItem()
    <XmlElement("BootstrapperPackage")>
    Public Property BootstrapperPackages As ItemGroup()
    <XmlElement("EmbeddedResource")>
    Public Property EmbeddedResources As ItemGroup()
    <XmlElement("Content")>
    Public Property Contents As ItemGroup()
    <XmlElement("ProjectReference")>
    Public Property ProjectReferences As ItemGroup()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class IncludeItem

    <XmlAttribute>
    Public Property Include As String
    Public Property DependentUpon As String
    Public Property SubType As String
    Public Property AutoGen As String
    Public Property DesignTime As String
    Public Property DesignTimeSharedInput As String
    Public Property Generator As String
    Public Property LastGenOutput As String
    Public Property CustomToolNamespace As String
    Public Property Visible As String
    Public Property ProductName As String
    Public Property Install As String

    Public Property HintPath As String
    Public Property [Private] As String
    Public Property Project As String
    Public Property Name As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class