#Region "Microsoft.VisualBasic::7ee8b954122de62ffd70a0e41cce19dc, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\Serialization\DocAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.HtmlParser

Namespace SoftwareToolkits.XmlDoc.Serialization

    <PackageNamespace("Assembly.Doc.API")>
    Public Module DocAPI

        Dim libraries As Dictionary(Of String, Libraries) =
            Enums(Of Libraries) _
            .ToDictionary(Function(x) x.ToString.ToLower)

        ''' <summary>
        ''' 类型名称的大小写不敏感
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns>查找失败的时候默认是返回<see cref="Serialization.Libraries.Github"/></returns>
        <Extension>
        Public Function GetLibraryType(type As Value(Of String)) As Libraries
            If libraries.ContainsKey(type = LCase(+type)) Then
                Return libraries(+type)
            Else
                Return Serialization.Libraries.Github
            End If
        End Function

        Public ReadOnly Property Types As Dictionary(Of Char, memberTypes) =
            New Dictionary(Of Char, memberTypes) From {
 _
            {"T"c, memberTypes.Type},
            {"F"c, memberTypes.Filed},
            {"M"c, memberTypes.Method},
            {"P"c, memberTypes.Property},
            {"E"c, memberTypes.Event}
        }

        <ExportAPI("Load")>
        Public Function Load(path As String) As Doc
            Try
                Return path.LoadXml(Of Doc)(preprocess:=AddressOf TrimAssemblyDoc)
            Catch ex As Exception
                Call ex.PrintException
                Throw ex
            End Try
        End Function

        Const cref As String = "<see(also)? cref=""[^""]+?""/>"
        Const cref2 As String = "<see(also)? cref=""[^""]+?"">\s*</see(also)?>"
        Const crefFull As String = "<see(also)? cref=""[^""]+?"">.+?</see(also)?>"

        Const paramRef As String = "<(type)?paramref name=""[^""]+""/>"
        Const paramRef2 As String = "<(type)?paramref name=""[^""]+"">\s*</(type)?paramref>"
        Const paramRefFull As String = "<(type)?paramref name=""[^""]+"">.+?</(type)?paramref>"

        Const code As String = "<code>.+?</code>"
        Const example As String = "<example>.*?</example>"

        Public Function TrimAssemblyDoc(doc As String) As String
            Dim sb As StringBuilder = New StringBuilder(doc)
            Dim ms As String() = Regex.Matches(doc, cref, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            ms = Regex.Matches(sb.ToString, cref2, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            ms = Regex.Matches(sb.ToString, crefFull, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            doc = sb.__boldParam

            Return doc
        End Function

        <Extension> Private Function __boldParam(sb As StringBuilder) As String
            Dim ms As String() = Regex.Matches(sb.ToString, paramRef, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Dim bold As String = m.__trans
                bold = Mid(bold, 2, bold.Length - 2)
                bold = $"**{bold}**"
                Call sb.Replace(m, bold)
            Next

            ms = Regex.Matches(sb.ToString, paramRef2, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Dim bold As String = m.__trans
                bold = Mid(bold, 2, bold.Length - 2)
                bold = $"**{bold}**"
                Call sb.Replace(m, bold)
            Next

            ms = Regex.Matches(sb.ToString, paramRefFull, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Dim bold As String = m.__trans
                Dim name As String = Regex.Match(bold, "``[^`]*``").Value
                ' bold = bold.Replace(name, (Mid(name, 2, name.Length - 2)))
                ' bold = $"**{bold}**"
                Call sb.Replace(m, bold)
            Next

            ms = Regex.Matches(sb.ToString, code, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "'" & m.GetValue & "'")
            Next

            ms = Regex.Matches(sb.ToString, example, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "==" & m.GetValue & "==")
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 这里会将双引号替换成为markdown里面的inline code形式
        ''' </summary>
        ''' <param name="cref"></param>
        ''' <returns></returns>
        <Extension> Private Function __trans(cref As String) As String
            Dim m As String = Regex.Match(cref, "="".+?""").Value
            Dim alt As String = cref.GetValue

            m = Mid(m, 2)

            If String.IsNullOrEmpty(alt) Then
            Else
                m &= $"[{alt}]"
            End If

            m = m.Replace("""", "``")

            Return m
        End Function
    End Module
End Namespace
