#Region "Microsoft.VisualBasic::e70faa7f2455322daa334d649b1713eb, gr\Microsoft.VisualBasic.Imaging\TextureResourceLoader.vb"

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

    '   Total Lines: 78
    '    Code Lines: 53 (67.95%)
    ' Comment Lines: 10 (12.82%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 15 (19.23%)
    '     File Size: 2.68 KB


    ' Module TextureResourceLoader
    ' 
    '     Function: AdjustColor, LoadInternalDefaultResource, LoadTextureResource
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage

''' <summary>
''' Default texture brush provider
''' </summary>
Public Module TextureResourceLoader

    ''' <summary>
    ''' 按照指定的资源图片和参数进行纹理资源的剪裁处理
    ''' </summary>
    ''' <param name="res"></param>
    ''' <param name="Size"></param>
    ''' <param name="IntervalWidth">纹理模块之间在水平上的间隔宽度</param>
    ''' <param name="IntervalHeight">纹理模块之间在竖直方向上的间隔宽度</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    Public Iterator Function LoadTextureResource(res As Image, Size As Size, IntervalWidth As Integer, IntervalHeight As Integer) As IEnumerable(Of Bitmap)
        Dim X As Integer
        Dim Y As Integer
        Dim spirit As BitmapBuffer = BitmapBuffer.FromImage(res)

        Do While True
            Dim resToken As New Bitmap(Size.Width, Size.Height)

            ' make copy of a rection in location x,y and rectangle in size
            ' from spirit texture resource to resToken bitmap
            For xi As Integer = 0 To Size.Width
                For yi As Integer = 0 To Size.Height
                    resToken.SetPixel(xi, yi, spirit.GetPixel(xi + X, yi + Y))
                Next
            Next

            Yield resToken

            X += IntervalWidth + Size.Width

            If X >= res.Width Then
                X = 0
                Y += Size.Height + IntervalHeight
            End If

            If Y >= res.Height Then
                Exit Do
            End If
        Loop
    End Function

    ''' <summary>
    ''' adjust of the image color of the texture image
    ''' </summary>
    ''' <param name="Image"></param>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function AdjustColor(Image As Image, Color As Color) As Image
        Dim res As BitmapBuffer = BitmapBuffer.FromImage(Image)
        Dim X, Y As Integer

        Do While True
            Dim p As Color = res.GetPixel(X, Y)
            Dim R As Integer = (CInt(p.R) + CInt(Color.R)) / 2
            Dim G As Integer = (CInt(p.G) + CInt(Color.G)) / 2
            Dim B As Integer = (CInt(p.B) + CInt(Color.B)) / 2
            Dim A As Integer = (CInt(p.A) + CInt(Color.A)) / 2

            Call res.SetPixel(X, Y, Color.FromArgb(A, R, G, B))

            X += 1

            If X >= res.Width Then
                X = 0
                Y += 1
            End If

            If Y >= res.Height Then
                Exit Do
            End If
        Loop

        Return res.GetImage
    End Function

    ''' <summary>
    ''' Load default internal texture image resource
    ''' </summary>
    ''' <returns></returns>
    Public Function LoadInternalDefaultResource() As Bitmap()
        Dim [default] As Bitmap

#If NET48 Then
        [default] = My.Resources.DefaultTexture
#Else
        [default] = Bitmap.FromStream(My.Resources.ResourceManager.GetStream("DefaultTexture"))
#End If

        Return TextureResourceLoader.LoadTextureResource([default], New Size(27, 19), 6, 6).ToArray
    End Function
End Module
