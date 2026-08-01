#Region "Microsoft.VisualBasic::3330717e407558323c980fd1cf1b46c6, vs_solutions\dev\VisualStudio\VBProject\Project\ProjectFiles.vb"

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

    '   Total Lines: 109
    '    Code Lines: 95 (87.16%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (12.84%)
    '     File Size: 4.46 KB


    '     Module ProjectFiles
    ' 
    '         Function: CollectCompileFiles, GetRelativePath, GlobMatch, IsExcludedByDefault, NormalizePath
    '                   ReadProperty
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text.RegularExpressions

Namespace VBProj

    Module ProjectFiles

        Public Function CollectCompileFiles(doc As XDocument, ns As XNamespace, projDir As String) As String()
            Dim includes As New List(Of String)
            Dim removes As New List(Of String)

            If doc.Root IsNot Nothing Then
                For Each ig In doc.Root.Elements(ns + "ItemGroup")
                    For Each c In ig.Elements(ns + "Compile")
                        Dim inc = c.Attribute("Include")?.Value
                        If inc IsNot Nothing Then includes.Add(NormalizePath(inc))
                        Dim remAttr = c.Attribute("Remove")?.Value
                        If remAttr IsNot Nothing Then removes.Add(NormalizePath(remAttr))
                    Next
                Next
            End If

            Dim defaultDisabled As Boolean = ReadProperty(doc, ns, "EnableDefaultCompileItems").Equals("false", StringComparison.OrdinalIgnoreCase)

            Dim result As New List(Of String)

            If includes.Count = 0 AndAlso Not defaultDisabled Then
                If Directory.Exists(projDir) Then
                    Try
                        For Each f In Directory.GetFiles(projDir, "*.vb", SearchOption.AllDirectories)
                            Dim rel = GetRelativePath(projDir, f)
                            If Not IsExcludedByDefault(rel) Then
                                result.Add(rel)
                            End If
                        Next
                    Catch
                    End Try
                End If
            Else
                result.AddRange(includes)
            End If

            If removes.Count > 0 Then
                result.RemoveAll(Function(p) removes.Any(Function(r) GlobMatch(r, p)))
            End If

            Return result.ToArray()
        End Function

        Private Function ReadProperty(doc As XDocument, ns As XNamespace, name As String) As String
            If doc.Root Is Nothing Then Return ""
            For Each pg In doc.Root.Elements(ns + "PropertyGroup")
                Dim el = pg.Element(ns + name)
                If el IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(el.Value) Then
                    Return el.Value.Trim()
                End If
            Next
            Return ""
        End Function

        Public Function NormalizePath(p As String) As String
            Dim s = p.Trim()
            While s.StartsWith(".\") OrElse s.StartsWith("./")
                s = s.Substring(2)
            End While
            Return s.Replace("/", "\")
        End Function

        Friend Function GetRelativePath(baseDir As String, file As String) As String
            Dim b = Path.GetFullPath(baseDir).TrimEnd("\"c, "/"c) & "\"
            Dim f = Path.GetFullPath(file)
            Dim uriB = New Uri(b)
            Dim uriF = New Uri(f)
            Dim rel = Uri.UnescapeDataString(uriB.MakeRelativeUri(uriF).ToString())
            Return rel.Replace("/", "\")
        End Function

        Friend Function IsExcludedByDefault(rel As String) As Boolean
            Dim lower = rel.Replace("\", "/").ToLowerInvariant()
            Return lower.Contains("/obj/") OrElse lower.Contains("/bin/") OrElse lower.StartsWith("obj/") OrElse lower.StartsWith("bin/")
        End Function

        Private Function GlobMatch(pattern As String, path As String) As Boolean
            Dim p = pattern.Replace("\", "/").ToLowerInvariant()
            Dim s = path.Replace("\", "/").ToLowerInvariant()
            Dim rx As String = "^"
            Dim i As Integer = 0
            While i < p.Length
                Dim c As Char = p(i)
                If c = "*"c Then
                    If i + 1 < p.Length AndAlso p(i + 1) = "*"c Then
                        rx &= ".*"
                        i += 1
                        If i + 1 < p.Length AndAlso p(i + 1) = "/"c Then i += 1
                    Else
                        rx &= "[^/]*"
                    End If
                ElseIf c = "?"c Then
                    rx &= "."
                Else
                    rx &= Regex.Escape(c.ToString())
                End If
                i += 1
            End While
            rx &= "$"
            Return Regex.IsMatch(s, rx)
        End Function
    End Module
End Namespace
