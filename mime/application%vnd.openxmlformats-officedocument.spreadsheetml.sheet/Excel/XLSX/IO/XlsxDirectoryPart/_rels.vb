#Region "Microsoft.VisualBasic::5e0e4640a5a4d0d67fefcdda3f2e7490, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\XlsxDirectoryPart\_rels.vb"

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

    '   Total Lines: 48
    '    Code Lines: 33 (68.75%)
    ' Comment Lines: 8 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (14.58%)
    '     File Size: 1.58 KB


    '     Class _rels
    ' 
    '         Properties: rels, workbook
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: _name
    ' 
    '         Sub: _loadContents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML._rels
Imports Microsoft.VisualBasic.Text.Xml

Namespace XLSX.Model.Directory

    Public Class _rels : Inherits XlsxDirectoryPart

        ''' <summary>
        ''' ``.rels``
        ''' </summary>
        ''' <returns></returns>
        Public Property rels As rels
        ''' <summary>
        ''' ``workbook.xml.rels``
        ''' </summary>
        ''' <returns></returns>
        Public Property workbook As rels

        Friend Sub New(data As ZipPackage)
            Call MyBase.New(data.data)
        End Sub

        Sub New(fs As IFileSystemEnvironment, parent As String)
            Call MyBase.New(fs, parent)
        End Sub

        Protected Overrides Sub _loadContents()
            If CheckInternalFileExists("/.rels") Then
                rels = New rels With {
                    .document = ReadInternalFileText("/.rels") _
                        .LoadFromXml(Of OpenXml.rels)
                }
            End If
            If CheckInternalFileExists("/workbook.xml.rels") Then
                workbook = New rels With {
                    .document = ReadInternalFileText("/workbook.xml.rels") _
                        .LoadFromXml(Of OpenXml.rels)
                }
            End If
        End Sub

        Protected Overrides Function _name() As String
            Return NameOf(_rels)
        End Function
    End Class
End Namespace
