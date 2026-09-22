#Region "Microsoft.VisualBasic::2287f110d456115650673ce1fec6fbfd, vs_solutions\VBS\src\VBScript\ScriptMetadata.vb"

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

    '   Total Lines: 131
    '    Code Lines: 67 (51.15%)
    ' Comment Lines: 38 (29.01%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 26 (19.85%)
    '     File Size: 5.45 KB


    '     Class ScriptMetadata
    ' 
    '         Properties: Author, Package, Title, Version
    ' 
    '         Function: [Meta], BuildAttributes, Parse, Quote, Unquote
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 脚本文件头部预处理指令(<c>#package</c> / <c>#author</c> / <c>#title</c> / <c>#version</c>)
    ''' 所声明的程序集元数据。
    ''' </summary>
    ''' <remarks>
    ''' 这些指令在 <see cref="VBScript.ParseScript"/> 阶段被解析出来:
    ''' <list type="bullet">
    ''' <item><c>#package</c>: 动态编译 assembly 的名称(不生成特性);</item>
    ''' <item><c>#author</c> : 生成 assembly 级别的 <c>AssemblyCompanyAttribute</c>;</item>
    ''' <item><c>#title</c>  : 生成 assembly 级别的 <c>AssemblyTitleAttribute</c>;</item>
    ''' <item><c>#version</c>: 生成 assembly 级别的 <c>AssemblyVersionAttribute</c>。</item>
    ''' </list>
    ''' </remarks>
    Public Class ScriptMetadata

        ''' <summary>#package 指令所声明的 assembly 名称</summary>
        Public Property Package As String

        ''' <summary>#author 指令所声明的 assembly company</summary>
        Public Property Author As String

        ''' <summary>#title 指令所声明的 assembly title</summary>
        Public Property Title As String

        ''' <summary>#version 指令所声明的 assembly version</summary>
        Public Property Version As String

        ''' <summary>
        ''' 指令行匹配模式: 键名之后允许出现可选的等号, 值可以为带引号的字符串或者裸标记。
        ''' 值使用 <c>[^\n]*</c> 以便兼容 CRLF 行尾(末尾的 <c>\r</c> 由 <see cref="Unquote"/> 清理)。
        ''' </summary>
        Friend Const DirectivePattern As String =
            "^\s*#(?<key>package|author|title|version)\b\s*=?\s*(?<val>[^\n]*)$"

        ''' <summary>VB 字符串字面量之中的双引号字符</summary>
        Public Const QuoteChar As String = """"

        ''' <summary>
        ''' 从脚本源代码之中解析出全部的程序集元数据声明指令。
        ''' 同一个指令出现多次时, 以最后一次声明的值为准。
        ''' </summary>
        ''' <param name="source">脚本源代码文本</param>
        Public Shared Function Parse(source As String) As ScriptMetadata
            Dim meta As New ScriptMetadata

            If String.IsNullOrEmpty(source) Then
                Return meta
            End If

            For Each m As Match In Regex.Matches(source, DirectivePattern,
                                                RegexOptions.Multiline Or RegexOptions.IgnoreCase)

                Dim val As String = Unquote(m.Groups("val").Value)

                If String.IsNullOrEmpty(val) Then
                    Continue For
                End If

                Select Case m.Groups("key").Value.ToLower()
                    Case "package" : meta.Package = val
                    Case "author" : meta.Author = val
                    Case "title" : meta.Title = val
                    Case "version" : meta.Version = val
                End Select
            Next

            Return meta
        End Function

        ''' <summary>
        ''' 生成 assembly 级别的特性声明代码行。
        ''' 注意: 这些代码行必须放置在生成代码的 Imports 之后、Namespace 之前。
        ''' </summary>
        Public Function BuildAttributes() As String()
            Dim attrs As New List(Of String)

            If Not String.IsNullOrEmpty(Author) Then
                Call attrs.Add($"<Assembly: System.Reflection.AssemblyCompanyAttribute({Quote(Author)})>")
            End If

            If Not String.IsNullOrEmpty(Title) Then
                Call attrs.Add($"<Assembly: System.Reflection.AssemblyTitleAttribute({Quote(Title)})>")
            End If

            If Not String.IsNullOrEmpty(Version) Then
                Call attrs.Add($"<Assembly: System.Reflection.AssemblyVersionAttribute({Quote(Version)})>")
            End If

            Return attrs.ToArray()
        End Function

        ''' <summary>
        ''' 按名称(大小写不敏感)读取元数据值, 未知名称返回 Nothing
        ''' </summary>
        Public Function [Meta](key As String) As String
            If String.IsNullOrEmpty(key) Then
                Return Nothing
            End If

            Select Case key.Trim().ToLower()
                Case "package" : Return Package
                Case "author" : Return Author
                Case "title" : Return Title
                Case "version" : Return Version
                Case Else : Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' 将字符串转义为 VB 字符串字面量(包含两端的双引号)
        ''' </summary>
        Public Shared Function Quote(raw As String) As String
            Return QuoteChar & If(raw, "").Replace(QuoteChar, QuoteChar & QuoteChar) & QuoteChar
        End Function

        ''' <summary>去除值两端成对的引号并清理空白</summary>
        Private Shared Function Unquote(raw As String) As String
            Dim val As String = If(raw, "").Trim()

            If val.Length >= 2 AndAlso val.StartsWith(QuoteChar) AndAlso val.EndsWith(QuoteChar) Then
                val = val.Substring(1, val.Length - 2)
            End If

            Return val.Trim()
        End Function
    End Class
End Namespace

