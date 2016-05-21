Imports System.Text

Public Class XmlDoc

    Public Const XmlEncoding As String = "<\?xml.+?>"

    Public Enum XmlEncodings
        UTF8
        UTF16
    End Enum

    Dim sb As StringBuilder

    Sub New(xml As String)
        sb = New StringBuilder(xml)
    End Sub

    Public Overrides Function ToString() As String
        Return sb.ToString
    End Function

    Public Shared Function FromObject(Of T As Class)(x As T) As XmlDoc
        Return New XmlDoc(x.GetXml)
    End Function
End Class

Public Structure XmlDeclaration

    Public version As String
    Public standalone As Boolean
    Public encoding As XmlDoc.XmlEncodings

    Public Shared ReadOnly Property [Default] As XmlDeclaration =
        New XmlDeclaration With {
            .version = "1.0",
            .standalone = True,
            .encoding = XmlDoc.XmlEncodings.UTF16
    }

    ''' <summary>
    ''' &lt;?xml version="{<see cref="version"/>}" standalone="{<see cref="standalone"/>}" encoding="{<see cref="encoding"/>}"?>
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return $"<?xml version=""{version}"" standalone=""{XmlStandaloneString(standalone)}"" encoding=""{XmlEncodingString(encoding)}""?>"
    End Function

    Public Shared Function XmlEncodingString(enc As XmlDoc.XmlEncodings) As String
        Return If(enc = XmlDoc.XmlEncodings.UTF8, "utf-8", "utf-16")
    End Function

    Public Shared Function XmlStandaloneString(standalone As Boolean) As String
        Return If(standalone, "yes", "no")
    End Function
End Structure