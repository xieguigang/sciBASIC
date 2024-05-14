#Region "Microsoft.VisualBasic::766812eb8e5139bb33a1e1eefbeb9209, gr\Microsoft.VisualBasic.Imaging\Drivers\Models\WmfData.vb"

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

    '   Total Lines: 59
    '    Code Lines: 44
    ' Comment Lines: 4
    '   Blank Lines: 11
    '     File Size: 1.99 KB


    '     Class WmfData
    ' 
    '         Properties: Driver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDataURI, (+3 Overloads) Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace Driver

    Public Class WmfData : Inherits GraphicsData
        Implements SaveGdiBitmap

        Public Overrides ReadOnly Property Driver As Drivers
            Get
                Return Drivers.WMF
            End Get
        End Property

        ReadOnly buffer As MemoryStream

        Public Sub New(img As Object, size As Size, padding As Padding)
            MyBase.New(img, size, padding)

            ' the wmf metafile temp file
            ' which its file path is generated from function 
            ' wmfTmp
            ' in graphics plot helper api
            If Not TypeOf img Is Stream Then
                Throw New InvalidDataException("The input img data should be a temporary wmf meta file stream!")
            Else
                buffer = img
                buffer.Seek(Scan0, SeekOrigin.Begin)
            End If
        End Sub

        Public Overrides Function GetDataURI() As DataURI
            Dim uri As New DataURI(buffer, content_type)
            Call buffer.Seek(Scan0, SeekOrigin.Begin)
            Return uri
        End Function

        Public Overrides Function Save(path As String) As Boolean
            Return buffer.FlushStream(path)
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            Call out.Seek(Scan0, SeekOrigin.Begin)
            Call buffer.CopyTo(out)
            Call buffer.Seek(Scan0, SeekOrigin.Begin)

            Return True
        End Function

        Public Overloads Function Save(stream As Stream, format As ImageFormat) As Boolean Implements SaveGdiBitmap.Save
            Return Save(stream)
        End Function
    End Class
End Namespace
