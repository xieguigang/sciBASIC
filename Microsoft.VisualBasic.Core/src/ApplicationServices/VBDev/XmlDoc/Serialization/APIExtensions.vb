#Region "Microsoft.VisualBasic::83bd69d6cfd51024ea5c96353d5ba9fe, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\Serialization\APIExtensions.vb"

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

    '   Total Lines: 143
    '    Code Lines: 93 (65.03%)
    ' Comment Lines: 18 (12.59%)
    '    - Xml Docs: 55.56%
    ' 
    '   Blank Lines: 32 (22.38%)
    '     File Size: 5.24 KB


    '     Module APIExtensions
    ' 
    '         Properties: Types
    ' 
    '         Function: __boldParam, __trans, Load, TrimAssemblyDoc
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace ApplicationServices.Development.XmlDoc.Serialization

    <Package("Assembly.Doc.API")>
    Public Module APIExtensions

        ''' <summary>
        ''' NDoc supports this by recognising a special NamespaceDoc class located in each namespace
        ''' 
        ''' > https://stackoverflow.com/questions/793210/xml-documentation-for-a-namespace
        ''' 
        ''' 这个名字的对象类型主要是被用来标记命名空间的注释信息使用的
        ''' </summary>
        Public Const NamespaceDoc$ = NameOf(NamespaceDoc)

        Public ReadOnly Property Types As New Dictionary(Of Char, memberTypes) From {
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
                Throw
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

        <Extension>
        Public Function TrimAssemblyDoc(doc As String) As String
            Dim sb As New StringBuilder(doc)
            Dim ms As String() = Regex.Matches(doc, cref, RegexICSng).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            ms = Regex.Matches(sb.ToString, cref2, RegexICSng).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            ms = Regex.Matches(sb.ToString, crefFull, RegexICSng).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            doc = sb.__boldParam

            Return doc
        End Function

        <Extension> Private Function __boldParam(sb As StringBuilder) As String
            Dim ms As String() = Regex.Matches(sb.ToString, paramRef, RegexICSng).ToArray

            For Each m As String In ms
                Dim bold As String = m.__trans
                bold = Mid(bold, 2, bold.Length - 2)
                bold = $"**{bold}**"
                Call sb.Replace(m, bold)
            Next

            ms = Regex.Matches(sb.ToString, paramRef2, RegexICSng).ToArray

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

            'ms = Regex.Matches(sb.ToString, example, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            'For Each m As String In ms
            '    Call sb.Replace(m, "==" & m.GetValue & "==")
            'Next

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
