#Region "Microsoft.VisualBasic::36007936e2a73a5f5b9ad2bfcd6d42e8, gr\PdfImage\PdfImage.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 6 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.66 KB


    ' Class PdfImage
    ' 
    '     Properties: Driver
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetDataURI, (+2 Overloads) Save
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Public Class PdfImage : Inherits GraphicsData

    ReadOnly image As PdfGraphics
    ReadOnly pdf As MemoryStream

    Public Overrides ReadOnly Property Driver As Drivers
        Get
            Return Drivers.PDF
        End Get
    End Property

    ''' <summary>
    ''' <paramref name="img"/> parameter value is <see cref="PdfGraphics"/>
    ''' </summary>
    ''' <param name="img"><see cref="PdfGraphics"/></param>
    ''' <param name="size"></param>
    ''' <param name="padding"></param>
    Public Sub New(img As Object, size As Size, padding As Padding)
        MyBase.New(img, size, padding)

        image = DirectCast(img, PdfGraphics)
        pdf = image.buffer

        Call image.page.Document.CreateFile()
        Call pdf.Seek(Scan0, SeekOrigin.Begin)
    End Sub

    Public Overrides Function GetDataURI() As DataURI
        Using buffer As New MemoryStream
            Call Save(buffer)
            Return New DataURI(buffer, "application/pdf")
        End Using
    End Function

    Public Overrides Function Save(path As String) As Boolean
        Using file As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Return Save(file)
        End Using
    End Function

    Public Overrides Function Save(out As Stream) As Boolean
        Call pdf.CopyTo(out)
        Call pdf.Seek(Scan0, SeekOrigin.Begin)
        Call out.Flush()

        Return True
    End Function
End Class
