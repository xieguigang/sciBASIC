#Region "Microsoft.VisualBasic::c5706c3f4b1b1444dd9a66483ff133aa, gr\PdfImage\Driver.vb"

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

    '   Total Lines: 44
    '    Code Lines: 35
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.54 KB


    ' Module Driver
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CreateImage, OpenDevice
    ' 
    '     Sub: Init
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Module Driver

    Sub New()
        ImageDriver.pdfDriver = AddressOf OpenDevice
        ImageDriver.getPdfImage = AddressOf CreateImage
    End Sub

    Private Function CreateImage(g As IGraphics, size As Size, padding As Padding) As GraphicsData
        Return New PdfImage(g, size, padding)
    End Function

    Public Function OpenDevice(size As Size) As PdfGraphics
        Dim buffer As New MemoryStream
        Dim stream As New PdfDocument(size.Width, size.Height, 1, buffer)
        Dim info = PdfInfo.CreatePdfInfo(stream)
        Dim localTime = DateTime.Now

        info.Title("Article Example")
        info.Author(Environment.UserName)
        info.Keywords("PDF, .NET, C#, Library, Document Creator")
        info.Subject("PDF File Writer C# Class Library (Version 1.15.0)")
        info.CreationDate(localTime)
        info.ModDate(localTime)
        info.Creator("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)
        info.Producer("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)

        Dim page As New PdfPage(stream)
        Dim g As New PdfGraphics(size, page, buffer)

        Return g
    End Function

    Public Sub Init()
        Call Console.WriteLine()
    End Sub

End Module
