Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SoftwareToolkits.XmlDoc

    <PackageNamespace("Assembly.Doc.API")>
    Public Module DocAPI

        <ExportAPI("Load")>
        Public Function Load(path As String) As Doc
            Return path.LoadXml(Of Doc)(preprocess:=AddressOf __trim)
        End Function

        Const cref As String = "<see cref=""[^""]+?""/>"
        Const cref2 As String = "<see cref=""[^""]+?"">\s*</see>"
        Const crefFull As String = "<see cref=""[^""]+?"">.+?</see>"

        Private Function __trim(doc As String) As String
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

            Call sb.SaveTo("x:\ffff.xml")

            Return sb.ToString
        End Function

        <Extension> Private Function __trans(cref As String) As String
            Dim m As String = Regex.Match(cref, "="".+?""").Value
            Dim alt As String = cref.GetValue
            m = Mid(m, 2) & $"[{alt}]"
            Return m
        End Function
    End Module
End Namespace