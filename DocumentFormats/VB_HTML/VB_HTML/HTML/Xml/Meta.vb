Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HTML.XmlMeta

    Public Class CSS

        <XmlAttribute> Public Property type As String
            Get
                Return "text/css"
            End Get
            Set(value As String)
                ' ReadOnly, Do Nothing
            End Set
        End Property

        <XmlText> Public Property style As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
