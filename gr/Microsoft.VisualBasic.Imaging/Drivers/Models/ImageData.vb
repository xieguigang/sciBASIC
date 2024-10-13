#Region "Microsoft.VisualBasic::c819054e3eec0c406079a8d97fae681f, gr\Microsoft.VisualBasic.Imaging\Drivers\Models\ImageData.vb"

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

    '   Total Lines: 124
    '    Code Lines: 84 (67.74%)
    ' Comment Lines: 20 (16.13%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (16.13%)
    '     File Size: 3.88 KB


    '     Class ImageData
    ' 
    '         Properties: DefaultFormat, Driver, Image
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: GetDataURI, (+3 Overloads) Save
    ' 
    '         Sub: Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

#If NET48 Then
Imports Microsoft.VisualBasic.Drawing
#End If

Namespace Driver

    ''' <summary>
    ''' Get image value from <see cref="ImageData.Image"/>
    ''' </summary>
    Public Class ImageData : Inherits GraphicsData
        Implements SaveGdiBitmap

        ''' <summary>
        ''' GDI+ image
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Image As Image

        Public Sub New(img As Object, size As Size, padding As Padding)
            MyBase.New(img, size, padding)

            If img.GetType() Is GetType(Bitmap) Then
                Image = CType(DirectCast(img, Bitmap), Image)
            Else
                Image = DirectCast(img, Image)
            End If
        End Sub

        Sub New(image As Image)
            Call Me.New(image, image.Size, New Padding)
        End Sub

        Sub New(bitmap As Bitmap)
            Call Me.New(bitmap, bitmap.Size, New Padding)
        End Sub

        ''' <summary>
        ''' Default image save format for <see cref="Bitmap"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property DefaultFormat As ImageFormats = ImageFormats.Png

        Public Overrides ReadOnly Property Driver As Drivers
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Drivers.GDI
            End Get
        End Property

        Public Overrides Function GetDataURI() As DataURI
            Return New DataURI(Image)
        End Function

        Const InvalidSuffix$ = "The gdi+ image file save path: {0} ending with *.svg file extension suffix!"

        ''' <summary>
        ''' Save the image as png
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Overrides Function Save(path As String) As Boolean
            If path.ExtensionSuffix.TextEquals("svg") Then
                Call String.Format(InvalidSuffix, path.ToFileURL).Warning
            End If

#If NET48 Then
            Return Image.SaveAs(path, ImageData.DefaultFormat)
#Else
            Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True)
                Return Save(s, ImageData.DefaultFormat)
            End Using
#End If
        End Function

        Public Overloads Function Save(stream As Stream, format As ImageFormats) As Boolean Implements SaveGdiBitmap.Save
            Try
#If NET48 Then
                Call Image.Save(stream, format.GetFormat)
#Else
                Call Image.Save(stream, format)
#End If
            Catch ex As Exception
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            Try
#If NET48 Then
                Call Image.Save(out, DefaultFormat.GetFormat)
#Else
                Call Image.Save(out, DefaultFormat)
#End If
            Catch ex As Exception
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' 当进行连续绘图操作的时候，如果不释放image的内存会导致内存泄漏？？？
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)

            If Not Image Is Nothing Then
                Call Image.Dispose()
            End If
        End Sub
    End Class
End Namespace
