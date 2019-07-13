#Region "Microsoft.VisualBasic::27027b5df5b26c037d3512bb2a33963e, Microsoft.VisualBasic.Core\Text\Xml\XmlDeclaration.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum XmlEncodings
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Structure XmlDeclaration
    ' 
    '         Properties: [Default]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EncodingParser, ToString, XmlStandaloneString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language.Default
Imports r = System.Text.RegularExpressions.Regex

Namespace Text.Xml

    Public Enum XmlEncodings
        <Description("utf-8")> UTF8
        <Description("utf-16")> UTF16
        <Description("gb2312")> GB2312
    End Enum

    Public Structure XmlDeclaration

        Public version As String
        Public standalone As Boolean
        Public encoding As XmlEncodings

        Sub New(declares As String)
            Dim s As String

            s = r.Match(declares, "encoding=""\S+""", RegexICSng).Value
            encoding = EncodingParser(s.GetStackValue("""", """"))
            s = r.Match(declares, "standalone=""\S+""", RegexICSng).Value
            standalone = s.GetStackValue("""", """").ParseBoolean
            s = r.Match(declares, "version=""\S+""", RegexICSng).Value
            version = s.GetStackValue("""", """")
        End Sub

        Public Shared ReadOnly Property [Default] As New XmlDeclaration With {
            .version = defaultVersion1_0,
            .standalone = True,
            .encoding = XmlEncodings.UTF16
        }

        Shared ReadOnly defaultVersion1_0 As [Default](Of String) = "1.0"

        ''' <summary>
        ''' &lt;?xml version="{<see cref="version"/>}" encoding="{<see cref="encoding"/>}" standalone="{<see cref="standalone"/>}"?>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim attr As New Dictionary(Of String, String) From {
                {"version", version Or defaultVersion1_0},
                {"encoding", encoding.Description},
                {"standalone", XmlStandaloneString(standalone)}
            }
            Return $"<?xml {attr.Select(Function(a) $"{a.Key}=""{a.Value}""").JoinBy(" ")} ?>"
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
