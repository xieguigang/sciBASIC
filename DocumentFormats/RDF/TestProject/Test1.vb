Imports Microsoft.VisualBasic.DocumentFormat.RDF
Imports Microsoft.VisualBasic.DocumentFormat.RDF.Serialization

<RDFNamespaceImports("cd", "http://www.recshop.fake/cd#")>
Public Class RDFD

    <RDFElement("cd")> Public Property CDList As CD()

    <RDFDescription(About:="http://www.recshop.fake/cd/Empire Burlesque")>
    <RDFType("cd")>
    Public Class CD : Inherits RDFEntity
        <RDFElement("artist")> Public Property Artist As String
        <RDFElement("country")> Public Property Country As String
        <RDFElement("company")> Public Property Company As String
        <RDFElement("price")> Public Property Price As String
        <RDFElement("year")> Public Property Year As String

        <RDFIgnore> Public Property IgnoredProperty As KeyValuePair(Of Integer, String)
    End Class
End Class
