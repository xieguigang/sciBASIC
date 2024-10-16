#Region "Microsoft.VisualBasic::cc96c83021c7732fe4d31b36a77345aa, gr\Drawing-net4.8\Graphics\Wmf.vb"

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

    '   Total Lines: 171
    '    Code Lines: 94 (54.97%)
    ' Comment Lines: 53 (30.99%)
    '    - Xml Docs: 83.02%
    ' 
    '   Blank Lines: 24 (14.04%)
    '     File Size: 7.45 KB


    ' Class Wmf
    ' 
    '     Properties: Driver, Size, wmfFile
    ' 
    '     Constructor: (+4 Overloads) Sub New
    '     Sub: DrawCircle, DrawString, Flush, initg, ReleaseHandle
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

''' <summary>
''' Windows Metafile (WMF) is an image file format originally designed for Microsoft Windows in the 1990s. 
''' Windows Metafiles are intended to be portable between applications and may contain both vector graphics 
''' and bitmap components. It acts in a similar manner to SVG files.
''' 
''' Essentially, a WMF file stores a list of function calls that have to be issued to the Windows Graphics 
''' Device Interface (GDI) layer to display an image on screen. Since some GDI functions accept pointers 
''' to callback functions for error handling, a WMF file may erroneously include executable code.
''' 
''' WMF Is a 16-bit format introduced in Windows 3.0. It Is the native vector format for Microsoft Office 
''' applications such as Word, PowerPoint, And Publisher. As of 2015 revision 12 of the Windows Metafile 
''' Format specification Is available for online reading Or download as PDF.
''' </summary>
''' <remarks>
''' The original 16 bit WMF file format was fully specified in volume 4 of the 1992 Windows 3.1 SDK documentation
''' (at least if combined with the descriptions of the individual functions and structures in the other volumes), 
''' but that specification was vague about a few details. These manuals were published as printed books available 
''' in bookstores with no click through EULA or other unusual licensing restrictions (just a general warning that 
''' if purchased as part of a software bundle, the software would be subject to one).
''' 
''' Over time the existence Of that historic specification was largely forgotten And some alternative implementations 
''' resorted To reverse engineering To figure out the file format from existing WMF files, which was difficult And 
''' Error prone. In September 2006, Microsoft again published the WMF file format specification In the context Of 
''' the Microsoft Open Specification Promise, promising To Not assert patent rights To file format implementors. 
''' </remarks>
Public Class Wmf : Inherits GDICanvas
    Implements IDisposable

    ''' <summary>
    ''' 矢量图的数据存储在这个元数据文件里面
    ''' </summary>
    ReadOnly vectorMetafile As Metafile
    ReadOnly hdc As IntPtr
    ReadOnly bounds As Size
    ReadOnly stream As Stream

    ''' <summary>
    ''' The file path of the target wmf image file.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property wmfFile As String

    Public Overrides ReadOnly Property Size As Size
        Get
            Return bounds
        End Get
    End Property

    Public Overrides ReadOnly Property Driver As Drivers
        Get
            Return Drivers.WMF
        End Get
    End Property

    ''' <summary>
    ''' The WMF format was designed to be executed by the Windows GDI layer in order to restore the image, but as 
    ''' the WMF binary files contain the definition of the GDI graphic primitives that constitute this image, it is 
    ''' possible to design alternative libraries that render WMF binary files, like the Kyktir application does, or 
    ''' convert them into other graphic formats. For example, the Batik library is able to render WMF files and 
    ''' convert them to their Scalable Vector Graphics (SVG) equivalent. The Vector Graphics package of the FreeHEP 
    ''' Java library allows the saving of Java2D drawings as Enhanced Metafiles (EMF). Inkscape and XnView can export 
    ''' to WMF or EMF.
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="save$"></param>
    ''' <param name="backgroundColor$"></param>
    Sub New(size As Size, save$, Optional backgroundColor$ = NameOf(Color.Transparent))
        Call Me.New(
            bitmap:=New Bitmap(size.Width, size.Height),
            stream:=save.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False),
            backgroundColor:=backgroundColor
        )

        wmfFile = save
    End Sub

    Sub New(size As Size, save As Stream, Optional backgroundColor$ = NameOf(Color.Transparent), Optional dpi As Size = Nothing)
        Call Me.New(
            bitmap:=New Bitmap(size.Width, size.Height),
            stream:=save,
            backgroundColor:=backgroundColor,
            dpi:=dpi
        )
    End Sub

    Sub New(bitmap As Bitmap, stream As Stream, Optional backgroundColor$ = NameOf(Color.Transparent), Optional dpi As Size = Nothing)
        Dim size As Size = bitmap.Size
        Dim bounds As New RectangleF(0, 0, size.Width, size.Height)

        If Not (dpi.Width = 0 OrElse dpi.Height = 0) Then
            Call bitmap.SetResolution(dpi.Width, dpi.Height)
        End If

        ' Make a Graphics object so we can use its hDC as a reference.
        Dim gSource = Graphics.FromImage(bitmap)
        Dim hdc As IntPtr = gSource.GetHdc()

        Me.hdc = hdc
        Me.bounds = bounds.Size.ToSize

        ' Make the Metafile, using the reference hDC.
        Me.vectorMetafile = New Metafile(stream, hdc, bounds, MetafileFrameUnit.Pixel)
        Me.stream = stream

        Call gSource.ReleaseHdc(hdc)
        ' Make a Graphics object and draw.
        Call initg(vectorMetafile)
        Call g.Clear(backgroundColor.TranslateColor)
    End Sub

    Private Sub initg(vectorMetafile As Metafile)
        ' Make a Graphics object and draw.
        g = Graphics.FromImage(vectorMetafile)
        g.SmoothingMode = SmoothingMode.HighQuality
        g.PageUnit = GraphicsUnit.Pixel
        g.CompositingQuality = CompositingQuality.HighQuality
        g.CompositingMode = CompositingMode.SourceOver
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
    End Sub

    Sub New(gr As Graphics2D, stream As Stream)
        Dim gSource As Graphics = gr.Graphics
        Dim size As Size = gr.Size
        Dim bounds As New RectangleF(0, 0, size.Width, size.Height)

        ' Make a Graphics object so we can use its hDC as a reference.
        Dim hdc As IntPtr = gSource.GetHdc()

        Me.hdc = hdc
        Me.bounds = bounds.Size.ToSize

        ' Make the Metafile, using the reference hDC.
        Me.vectorMetafile = New Metafile(stream, hdc, bounds, MetafileFrameUnit.Pixel)
        Me.stream = stream

        Call gSource.ReleaseHdc(hdc)
        Call initg(vectorMetafile)
    End Sub

    Public Overrides Sub Flush()
        Call MyBase.Flush()
        Call stream.Flush()
    End Sub

    ''' <summary>
    ''' Close the metafile and free resources.
    ''' </summary>
    Protected Overrides Sub ReleaseHandle()
        Call Graphics.Dispose()
        Call vectorMetafile.Dispose()
        Call stream.Flush()
    End Sub

    Public Overrides Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius As Single)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)
        Throw New NotImplementedException()
    End Sub
End Class
