#Region "Microsoft.VisualBasic::59bb6ddf767dc05642ee1b72674c4e6c, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\FileIO\IO.vb"

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

    '   Total Lines: 67
    '    Code Lines: 46 (68.66%)
    ' Comment Lines: 11 (16.42%)
    '    - Xml Docs: 45.45%
    ' 
    '   Blank Lines: 10 (14.93%)
    '     File Size: 2.40 KB


    '     Module IO
    ' 
    '         Function: (+2 Overloads) CreateReader, LoadZip, ToXML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Directory
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Namespace XLSX.FileIO

    Public Module IO

        ' /
        '  +------- <_rels>
        '           + .rels
        '  +------- <docProps>
        '  +------- <xl>
        '  +------- [Content_Types].xml

        <Extension>
        Public Function LoadZip(zip As ZipStream) As File
            Return CreateReader(New ZipPackage With {.data = zip, .xlsx = zip.ToString})
        End Function

        Private Function CreateReader(dataArchive As ZipPackage) As File
            Dim contentType As ContentTypes = dataArchive("/[Content_Types].xml").LoadFromXml(Of ContentTypes)
            Dim rels As New _rels(dataArchive)
            Dim docProps As New docProps(dataArchive)
            Dim xl As New xl(dataArchive)
            Dim file As New File(dataArchive) With {
                .ContentTypes = contentType,
                ._rels = rels,
                .docProps = docProps,
                .xl = xl,
                .FilePath = If(DataURI.IsWellFormedUriString(dataArchive.xlsx), "datauri://", dataArchive.xlsx)
            }

            Return file
        End Function

        ''' <summary>
        ''' 解压缩Excel文件然后读取其中的XML数据以构成DataFrame表格 
        ''' </summary>
        ''' <param name="xlsx"></param>
        ''' <returns></returns>
        Public Function CreateReader(xlsx As String) As File
            Dim dataArchive As New ZipPackage With {.xlsx = xlsx}

            If Not dataArchive.ExtractZip Then
                Throw dataArchive.Err
            Else
                Return CreateReader(dataArchive)
            End If
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
End Namespace
