#Region "Microsoft.VisualBasic::27027b5df5b26c037d3512bb2a33963e, Microsoft.VisualBasic.Core\src\Text\Xml\XmlDeclaration.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 64
    '    Code Lines: 49
    ' Comment Lines: 4
    '   Blank Lines: 11
    '     File Size: 2.44 KB


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
