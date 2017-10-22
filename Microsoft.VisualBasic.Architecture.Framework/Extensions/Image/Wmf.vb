#Region "Microsoft.VisualBasic::505abb825744496f1a191fc7a76cba98, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Wmf.vb"

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

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Namespace Imaging

    Public Class Wmf : Inherits GDICanvas
        Implements IDisposable

        ReadOnly curMetafile As Metafile
        ReadOnly gSource As Graphics
        ReadOnly hdc As IntPtr

        ''' <summary>
        ''' The file path of the target wmf image file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FilePath As String
        Public Overrides ReadOnly Property Size As Size

        Sub New(size As Size, save$, Optional backgroundColor$ = NameOf(Color.Transparent))
            Dim bitmap As New Bitmap(size.Width, size.Height)

            gSource = Graphics.FromImage(bitmap)
            gSource.Clear(backgroundColor.TranslateColor)

            hdc = gSource.GetHdc()
            size = bitmap.Size
            curMetafile = New Metafile(save, hdc)
            Graphics = Graphics.FromImage(curMetafile)
            Graphics.SmoothingMode = SmoothingMode.HighQuality

            FilePath = save
        End Sub

        Private Sub __release()
            Call gSource.ReleaseHdc(hdc)
            Call Graphics.Dispose()
            Call gSource.Dispose()
        End Sub

        Public Overrides Sub Dispose()
            Call __release()
            MyBase.Dispose()
        End Sub
    End Class
End Namespace
