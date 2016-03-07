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

        Const cref As String = "<see cref="".+?""/>"

        Private Function __trim(doc As String) As String
            Dim sb As StringBuilder = New StringBuilder(doc)
            Dim ms As String() = Regex.Matches(doc, cref, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray

            For Each m As String In ms
                Call sb.Replace(m, "@" & m.__trans)
            Next

            Return sb.ToString
        End Function

        <Extension> Private Function __trans(cref As String) As String
            Dim m As String = Regex.Match(cref, "="".+?""").Value
            m = Mid(m, 2)
            Return m
        End Function
    End Module
End Namespace