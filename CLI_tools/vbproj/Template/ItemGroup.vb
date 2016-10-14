Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ItemGroup

    <XmlElement("Reference")>
    Public Property References As IncludeItem()
    <XmlElement("Import")>
    Public Property [Imports] As IncludeItem()
    <XmlElement("Compile")>
    Public Property Compiles As Compile()
    <XmlElement("None")>
    Public Property Nones As None()
    <XmlElement("BootstrapperPackage")>
    Public Property BootstrapperPackages As BootstrapperPackage()
    <XmlElement("EmbeddedResource")>
    Public Property EmbeddedResources As EmbeddedResource()
    <XmlElement("Content")>
    Public Property Contents As Content()
    <XmlElement("ProjectReference")>
    Public Property ProjectReferences As ProjectReference()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class ProjectReference : Inherits IncludeItem
    Public Property Project As String
    Public Property Name As String
End Class

Public Class IncludeItem

    <XmlAttribute>
    Public Property Include As String
    Public Property HintPath As String
    Public Property [Private] As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class Compile : Inherits IncludeItem
    Public Property AutoGen As String
    Public Property DesignTime As String
    Public Property DependentUpon As String
    Public Property DesignTimeSharedInput As String
    Public Property SubType As String
End Class

Public Class None : Inherits IncludeItem
    Public Property Generator As String
    Public Property LastGenOutput As String
    Public Property CustomToolNamespace As String
End Class

Public Class EmbeddedResource : Inherits IncludeItem
    Public Property DependentUpon As String
    Public Property Generator As String
    Public Property LastGenOutput As String
    Public Property CustomToolNamespace As String
    Public Property SubType As String
End Class

Public Class BootstrapperPackage : Inherits IncludeItem
    Public Property Visible As String
    Public Property ProductName As String
    Public Property Install As String
End Class

Public Class Content : Inherits IncludeItem

End Class