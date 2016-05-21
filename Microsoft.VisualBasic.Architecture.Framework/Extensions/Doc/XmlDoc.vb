Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel

Public Class XmlDoc : Implements ISaveHandle

    Public Const XmlDeclares As String = "<\?xml.+?>"

    Public Enum XmlEncodings
        UTF8
        UTF16
    End Enum

    Dim sb As StringBuilder

    Public Property version As String
    Public Property standalone As Boolean
    Public Property encoding As XmlDoc.XmlEncodings

    Sub New(xml As String)
        sb = New StringBuilder(xml)

        Dim [declare] As New XmlDeclaration(
            Regex.Match(sb.ToString,
                        XmlDeclares,
                        RegexICSng).Value)
        version = [declare].version
        standalone = [declare].standalone
        encoding = [declare].encoding
    End Sub

    Public Overrides Function ToString() As String
        Dim [declare] As String = Regex.Match(sb.ToString, XmlDeclares, RegexICSng).Value
        Dim setDeclare As New XmlDeclaration With {
            .encoding = encoding,
            .standalone = standalone,
            .version = version
        }

        Call sb.Replace([declare], setDeclare.ToString)

        Return sb.ToString
    End Function

    Public Shared Function FromObject(Of T As Class)(x As T) As XmlDoc
        Return New XmlDoc(x.GetXml)
    End Function

    Public Function SaveTo(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
        Return Me.ToString.SaveTo(Path, encoding)
    End Function

    Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return SaveTo(Path, encoding.GetEncodings)
    End Function
End Class

Public Structure XmlDeclaration

    Public version As String
    Public standalone As Boolean
    Public encoding As XmlDoc.XmlEncodings

    Sub New(declares As String)
        Dim s As String

        s = Regex.Match(declares, "encoding=""\S+""", RegexICSng).Value
        encoding = EncodingParser(s.GetString)
        s = Regex.Match(declares, "standalone=""\S+""", RegexICSng).Value
        standalone = s.GetString.getBoolean
        s = Regex.Match(declares, "version=""\S+""", RegexICSng).Value
        version = s.GetString
    End Sub

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

    Public Shared Function EncodingParser(enc As String) As XmlDoc.XmlEncodings
        If String.Equals(enc, "utf-8", StringComparison.OrdinalIgnoreCase) Then
            Return XmlDoc.XmlEncodings.UTF8
        Else
            Return XmlDoc.XmlEncodings.UTF16
        End If
    End Function

    Public Shared Function XmlStandaloneString(standalone As Boolean) As String
        Return If(standalone, "yes", "no")
    End Function
End Structure