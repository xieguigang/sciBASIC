#Region "Microsoft.VisualBasic::1d1bc74cb87f1ba998ef280cb45dad19, mime\application%pdf\PDF.vb"

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

    '   Total Lines: 15
    '    Code Lines: 12 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (20.00%)
    '     File Size: 445 B


    ' Module PDF
    ' 
    '     Function: GetText
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Public Module PDF

    Public Iterator Function GetText(file As Stream) As IEnumerable(Of String)
        Using reader As New PdfReader(file)
            Dim extractor As New TextExtractor(reader)
            Dim pages = reader.GetPages()

            For i As Integer = 0 To pages.Count - 1
                Yield extractor.ExtractFromPage(pages(i))
            Next
        End Using
    End Function
End Module
