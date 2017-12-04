#Region "Microsoft.VisualBasic::5da61710b9f3913d032b8974672a1e4c, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO.vb"

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

Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Text
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
                    .ToXML _
                    .SaveTo(workbook, Encoding.UTF8)
                Call .sharedStrings _
                    .ToXML _
                    .SaveTo(sharedStrings, Encoding.UTF8)

                Call xlsx.ContentTypes _
                    .ToXML _
                    .SaveTo(ContentTypes, Encoding.UTF8)
            End With
        ElseIf xlsx.modify("worksheet.update") > -1 Then
            Call xlsx.xl.worksheets.Save()
            Call xlsx.xl.sharedStrings _
                .ToXML _
                .SaveTo(sharedStrings, Encoding.UTF8)
        End If

        ' 重新进行zip打包
        Call GZip.DirectoryArchive(xlsx.ROOT, path, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)

        Return True
    End Function

    <Extension>
    Public Function ToXML(Of T)(obj As T) As String
        Dim xml As New XmlDoc(obj.GetXml(xmlEncoding:=XmlEncodings.UTF8))
        xml.xmlns.xsi = Nothing
        xml.xmlns.xsd = Nothing
        xml.standalone = True

        Dim out$ = ASCII.TrimNonPrintings(xml.ToString)
        Return out
    End Function
End Module
