Imports System.Text.RegularExpressions

Namespace Text.Xml

    Public Enum XmlEncodings
        UTF8
        UTF16
        GB2312
    End Enum

    Public Structure XmlDeclaration

        Public version As String
        Public standalone As Boolean
        Public encoding As XmlEncodings

        Sub New(declares As String)
            Dim s As String

            s = Regex.Match(declares, "encoding=""\S+""", RegexICSng).Value
            encoding = EncodingParser(s.GetStackValue("""", """"))
            s = Regex.Match(declares, "standalone=""\S+""", RegexICSng).Value
            standalone = s.GetStackValue("""", """").getBoolean
            s = Regex.Match(declares, "version=""\S+""", RegexICSng).Value
            version = s.GetStackValue("""", """")
        End Sub

        Public Shared ReadOnly Property [Default] As XmlDeclaration =
            New XmlDeclaration With {
                .version = "1.0",
                .standalone = True,
                .encoding = XmlEncodings.UTF16
        }

        ''' <summary>
        ''' &lt;?xml version="{<see cref="version"/>}" encoding="{<see cref="encoding"/>}" standalone="{<see cref="standalone"/>}"?>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(version) Then
                version = "1.0"
            End If
            Return $"<?xml version=""{version}"" encoding=""{XmlEncodingString(encoding)}"" standalone=""{XmlStandaloneString(standalone)}""?>"
        End Function

        Public Shared Function XmlEncodingString(enc As XmlEncodings) As String
            Return If(enc = XmlEncodings.UTF8, "utf-8", "utf-16")
        End Function

        Public Shared Function EncodingParser(enc As String) As XmlEncodings
            If String.Equals(enc, "utf-8", StringComparison.OrdinalIgnoreCase) Then
                Return XmlEncodings.UTF8
            Else
                Return XmlEncodings.UTF16
            End If
        End Function

        Public Shared Function XmlStandaloneString(standalone As Boolean) As String
            Return If(standalone, "yes", "no")
        End Function
    End Structure
End Namespace