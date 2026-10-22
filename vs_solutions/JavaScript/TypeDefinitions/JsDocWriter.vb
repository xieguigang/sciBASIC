#Region "Microsoft.VisualBasic::3b6e1a47d9c05f82e4a7b0c96d3f1582, vs_solutions\JavaScript\TypeDefinitions\JsDocWriter.vb"

    ' Author:
    ' 
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
    ' GNU General PUBLIC License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:

    '     Module JsDocWriter
    ' 
    '         Function: ForMember, ForType

    ' /********************************************************************************/

#End Region

Imports System.Text

Imports doc = Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Namespace TypeDefinitions

    ''' <summary>
    ''' Renders the XML documentation model (<see cref="doc.ProjectType"/> /
    ''' <see cref="doc.ProjectMember"/>) as a JSDoc comment block, so the generated
    ''' ``.d.ts`` carries the same IntelliSense text a .NET IDE would show.
    ''' </summary>
    Friend Module JsDocWriter

        ''' <summary>JSDoc block for a type; empty when there is nothing to say.</summary>
        Friend Function ForType(pt As doc.ProjectType, obsolete As Boolean, deprecated As String, indent As String) As String
            If pt Is Nothing Then
                If Not obsolete Then Return ""
                Return Build(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, True, deprecated, Nothing, indent)
            End If

            Return Build(pt.Summary, Nothing, Nothing, Nothing, pt.Remarks, pt.TypeParams,
                         obsolete, deprecated, Nothing, indent)
        End Function

        ''' <summary>JSDoc block for a member; empty when there is nothing to say.</summary>
        Friend Function ForMember(pm As doc.ProjectMember, obsolete As Boolean, deprecated As String,
                                  paramNames As Dictionary(Of String, String), indent As String) As String
            If pm Is Nothing Then
                If Not obsolete Then Return ""
                Return Build(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, True, deprecated, Nothing, indent)
            End If

            Return Build(pm.Summary, pm.Params, pm.Returns, pm.example, pm.Remarks, pm.TypeParams,
                         obsolete, deprecated, paramNames, indent)
        End Function

        ' ======================================================== rendering

        Private Function Build(summary As String, params As doc.param(), returns As String, example As String,
                               remarks As String, typeParams As doc.typeparam(),
                               obsolete As Boolean, deprecated As String,
                               paramNames As Dictionary(Of String, String), indent As String) As String

            Dim lines As New List(Of String)
            Dim body As String = Clean(summary)

            If body <> "" Then lines.AddRange(SplitLines(body))

            Dim notes As String = Clean(remarks)

            If notes <> "" Then
                If lines.Count > 0 Then lines.Add("")
                lines.AddRange(SplitLines(notes))
            End If

            If params IsNot Nothing Then
                For Each p As doc.param In params
                    Dim text As String = Clean(p.text)

                    If text = "" Then Continue For

                    Dim rendered As String = Nothing

                    If paramNames Is Nothing OrElse Not paramNames.TryGetValue(p.name, rendered) Then
                        rendered = p.name
                    End If

                    lines.Add("@param " & rendered & " " & text)
                Next
            End If

            Dim ret As String = Clean(returns)

            If ret <> "" Then lines.Add("@returns " & ret)

            If typeParams IsNot Nothing Then
                For Each tp As doc.typeparam In typeParams
                    Dim text As String = Clean(tp.text)
                    lines.Add("@template " & tp.name & If(text = "", "", " " & text))
                Next
            End If

            Dim sample As String = Clean(example)

            If sample <> "" Then
                lines.Add("@example")
                lines.AddRange(SplitLines(sample).Select(Function(line) "    " & line))
            End If

            If obsolete Then
                lines.Add("@deprecated" & If(String.IsNullOrEmpty(deprecated), "", " " & Clean(deprecated)))
            End If

            If lines.Count = 0 Then Return ""

            Dim sb As New StringBuilder

            sb.Append(indent).AppendLine("/**")

            For Each line As String In lines
                sb.Append(indent).Append(" * ").AppendLine(Escape(line).TrimEnd())
            Next

            sb.Append(indent).AppendLine(" */")

            Return sb.ToString()
        End Function

        Private Function Clean(text As String) As String
            If String.IsNullOrEmpty(text) Then Return ""

            Dim s As String = text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
            Dim lines As String() = s.Split(vbLf)

            For i As Integer = 0 To lines.Length - 1
                lines(i) = lines(i).TrimEnd()
            Next

            Return String.Join(vbLf, lines).Trim()
        End Function

        Private Function SplitLines(text As String) As List(Of String)
            Return text.Split(vbLf).ToList()
        End Function

        ''' <summary>A ``*/`` inside the comment text would close the block early.</summary>
        Private Function Escape(text As String) As String
            If text.IndexOf("*/", StringComparison.Ordinal) < 0 Then Return text
            Return text.Replace("*/", "*\/")
        End Function

    End Module

End Namespace
