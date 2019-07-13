#Region "Microsoft.VisualBasic::7298a9a30a05a1c83524147a5d214f5c, Microsoft.VisualBasic.Core\ApplicationServices\VBDev\XmlDoc\Serialization\APIExtensions.vb"

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
