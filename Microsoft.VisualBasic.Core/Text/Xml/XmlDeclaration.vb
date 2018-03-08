#Region "Microsoft.VisualBasic::454c46bb934180cf2b277e3e958f8f10, Microsoft.VisualBasic.Core\Text\Xml\XmlDeclaration.vb"

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

    '     Enum XmlEncodings
    ' 
    '         GB2312, UTF16, UTF8
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
    '         Function: EncodingParser, ToString, XmlEncodingString, XmlStandaloneString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
            standalone = s.GetStackValue("""", """").ParseBoolean
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
