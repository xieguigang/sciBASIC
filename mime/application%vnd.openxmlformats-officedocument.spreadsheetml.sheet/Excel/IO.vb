Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Public Module IO

    ' /
    '  +------- <_rels>
    '           + .rels
    '  +------- <docProps>
    '  +------- <xl>
    '  +------- [Content_Types].xml

    Public Function CreateReader(xlsx$) As File
        Dim ROOT$ = App.GetAppSysTempFile(".zip", App.PID)

        Call GZip.ImprovedExtractToDirectory(xlsx, ROOT, Overwrite.Always)

        Dim contentType As ContentTypes = (ROOT & "/[Content_Types].xml").LoadXml(Of ContentTypes)
        Dim rels As New _rels(ROOT)
        Dim docProps As New docProps(ROOT)
        Dim xl As New xl(ROOT)
        Dim file As New File With {
            .ContentTypes = contentType,
            ._rels = rels,
            .docProps = docProps,
            .xl = xl,
            .FilePath = xlsx,
            .ROOT = ROOT
        }

        Return file
    End Function

    ''' <summary>
    ''' Save the Xlsx file data to a specific <paramref name="path"/> location.
    ''' </summary>
    ''' <param name="xlsx"></param>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    <Extension> Public Function SaveTo(xlsx As File, path$) As Boolean
        Dim workbook$ = xlsx.ROOT & "/xl/workbook.xml"
        Dim sharedStrings = xlsx.ROOT & "/xl/sharedStrings.xml"
        Dim ContentTypes$ = xlsx.ROOT & "/[Content_Types].xml"

        If xlsx.modify("worksheet.add") > -1 Then
            With xlsx.xl
                Call .worksheets.Save()
                Call .workbook _
                    .GetXml(xmlEncoding:=XmlEncodings.UTF8) _
                    .SaveTo(workbook, Encoding.UTF8)
                Call .sharedStrings _
                    .GetXml(xmlEncoding:=XmlEncodings.UTF8) _
                    .SaveTo(sharedStrings, Encoding.UTF8)

                Call xlsx.ContentTypes _
                    .GetXml(xmlEncoding:=XmlEncodings.UTF8) _
                    .SaveTo(ContentTypes, Encoding.UTF8)
            End With
        ElseIf xlsx.modify("worksheet.update") > -1 Then
            Call xlsx.xl.worksheets.Save()
            Call xlsx.xl.sharedStrings _
                .GetXml(xmlEncoding:=XmlEncodings.UTF8) _
                .SaveTo(sharedStrings, Encoding.UTF8)
        End If

        ' 重新进行zip打包
        Call GZip.DirectoryArchive(xlsx.ROOT, path, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)

        Return True
    End Function
End Module
