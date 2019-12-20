Imports System.IO
Imports System.Xml

Namespace Text.Xml.Linq

    ''' <summary>
    ''' https://stackoverflow.com/questions/12590487/net-xml-deserialization-ignore-namespaces
    ''' </summary>
    Friend Class NamespaceIgnorantXmlTextReader
        Inherits XmlTextReader

        Public Overrides ReadOnly Property NamespaceURI As String
            Get
                Return ""
            End Get
        End Property

        Public Sub New(stream As Stream)
            Call MyBase.New(stream)
        End Sub

        Public Sub New(text As TextReader)
            Call MyBase.New(text)
        End Sub
    End Class
End Namespace