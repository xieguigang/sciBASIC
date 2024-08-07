#Region "Microsoft.VisualBasic::0310c313f5b1e2c9c0eab5262bfa7ae0, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\XlsxDirectoryPart\XlsxDirectoryPart.vb"

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

    '   Total Lines: 51
    '    Code Lines: 41 (80.39%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (19.61%)
    '     File Size: 2.73 KB


    '     Class XlsxDirectoryPart
    ' 
    '         Properties: folder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: InternalFileName, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices

Namespace XLSX.Model.Directory

#If False Then
    An XLSX file is packaged using the Open Packaging Conventions (OPC/OOXML_2012, itself based on ZIP_6_2_0). 
    The package can be explored, by opening with ZIP software, typically by changing the file extension to .zip. 
    The top level of a minimal package will typically have three folders (_rels, docProps, and xl) and one file
    part ([Content_Types].xml). The xl folder holds the primary content of the document including the file part 
    workbook.xml and a worksheets folder containing a file for each worksheet, as well as other files and folders 
    that support functionality (such as controlling calculation order) and presentation (such as formatting 
    styles for cells) for the spreadsheet. Any embedded graphics are also stored in the xl folder as additional 
    parts. The other folders and parts at the top level of the package support efficient navigation and manipulation 
    of the package:

    + _rels is a Relationships folder, containing a single file .rels (which may be hidden from file listings, depending
      on operating system and settings). It lists and links to the key parts in the package, using URIs to identify the 
      type of relationship of each key part to the package. In particular it specifies a relationship to the primary 
      officeDocument (typically named /xl/workbook.xml ) and typically to parts within docProps as core and extended 
      properties.
    + docProps is a folder that contains properties for the document as a whole, typically including a set of core properties, 
      a set of extended or application-specific properties, and a thumbnail preview for the document.
    + [Content_Types].xml is a file part, a mandatory part in any OPC package, that lists the content types (using MIME 
      Internet Media Types as defined in RFC 6838) for parts within the package.
#End If

    Public MustInherit Class XlsxDirectoryPart

        Protected ReadOnly fs As IFileSystemEnvironment
        Protected ReadOnly subdir As String

        Sub New(workdir As IFileSystemEnvironment, Optional parent As String = "/")
            fs = workdir
            subdir = parent & "/" & _name()

            Call _loadContents()
        End Sub

        Protected MustOverride Function _name() As String
        Protected MustOverride Sub _loadContents()

        Protected Function CheckInternalFileExists(name As String) As Boolean
            Return fs.FileExists($"/{subdir}/{name}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Function ReadInternalFileText(name As String) As String
            Return fs.ReadAllText($"/{subdir}/{name}")
        End Function

        Protected Sub scanXmlFiles(parseText As Action(Of String, String))

        End Sub

        Protected Sub scanFiles(filter As String, parseText As Action(Of String, String))

        End Sub

        Public Overrides Function ToString() As String
            Return "/" & subdir
        End Function
    End Class
End Namespace
