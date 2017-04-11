Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Text.Xml.Linq

    Public Class Entity : Implements INamedValue

        <XmlAttribute>
        Public Property ID As String Implements IKeyedEntity(Of String).Key
        <XmlElement>
        Public Property Properties As PropertyValue()

        Public Overrides Function ToString() As String
            Return GetXml
        End Function
    End Class
End Namespace