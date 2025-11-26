#Region "Microsoft.VisualBasic::5c3db3283c8a30f237d5f59a7e9d32d5, gr\Drawing-net4.8\Utils.vb"

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

    '   Total Lines: 73
    '    Code Lines: 51 (69.86%)
    ' Comment Lines: 10 (13.70%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (16.44%)
    '     File Size: 2.75 KB


    '     Module Utils
    ' 
    '         Function: ResizeUnscaled, ResizeUnscaledByHeight, TrimRoundAvatar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Bitmap = System.Drawing.Bitmap
Imports Image = System.Drawing.Image
Imports TextureBrush = System.Drawing.TextureBrush

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Tools function for processing on <see cref="Image"/>/<see cref="Bitmap"/>
    ''' </summary>
    Public Module Utils

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ResizeUnscaled(image As Image, width%) As Image
            Return image.Resize(width, image.Height * (width / image.Width), onlyResizeIfWider:=width > image.Width)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ResizeUnscaledByHeight(image As Image, height%) As Image
            Return image.ResizeUnscaled(image.Width * (height / image.Height))
        End Function

        ''' <summary>
        ''' 图片剪裁为圆形的头像
        ''' </summary>
        ''' <param name="resAvatar">要求为正方形或者近似正方形</param>
        ''' <param name="OutSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function TrimRoundAvatar(resAvatar As Image, OutSize As Integer) As Image
            If resAvatar Is Nothing Then
                Return Nothing
            End If

            SyncLock resAvatar
                Dim Bitmap As New Bitmap(OutSize, OutSize)

                resAvatar = DirectCast(resAvatar.Clone, Image)
                resAvatar = resAvatar.ResizeUnscaledByHeight(OutSize)

                Using g = Graphics.FromImage(Bitmap)
                    Dim image As New TextureBrush(resAvatar)

                    With g
                        .CompositingQuality = CompositingQuality.HighQuality
                        .InterpolationMode = InterpolationMode.HighQualityBicubic
                        .SmoothingMode = SmoothingMode.HighQuality
                        .TextRenderingHint = TextRenderingHint.ClearTypeGridFit

#If NET48 Then
                        Call .FillPie(image, Bitmap.EntireImage, 0, 360)
#Else
                        Call .FillPie(image, New Rectangle(New Point, Bitmap.Size), 0, 360)
#End If

                    End With

                    Return Bitmap
                End Using
            End SyncLock
        End Function
    End Module
End Namespace
