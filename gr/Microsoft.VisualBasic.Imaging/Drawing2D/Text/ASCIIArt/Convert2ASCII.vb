#Region "Microsoft.VisualBasic::76f82b0faad8a6d25cd5bf4fbec615ae, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\ASCIIArt\Convert2ASCII.vb"

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

'   Total Lines: 141
'    Code Lines: 76 (53.90%)
' Comment Lines: 46 (32.62%)
'    - Xml Docs: 41.30%
' 
'   Blank Lines: 19 (13.48%)
'     File Size: 6.52 KB


'     Module HelperMethods
' 
'         Function: ASCIIImage, Convert2ASCII, DrawText
' 
'         Sub: WriteASCIIStream, writeInternal
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports std = System.Math

Namespace Drawing2D.Text.ASCIIArt

    ''' <summary>
    ''' Program that converts images to ASCII art images
    ''' 
    ''' > https://github.com/juangallostra/Image2ASCII
    ''' </summary>
    Public Module HelperMethods

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ASCIIImage(text$,
                                   Optional font$ = CSSFont.Win7Normal,
                                   Optional characters As WeightedChar() = Nothing) As String
            Return text _
                .DrawText(Color.Black, Color.White, , CSSEnvirnment.Empty.GetFont(CSSFont.TryParse(font))) _
                .Convert2ASCII(characters)
        End Function

        ''' <summary>
        ''' Image input <paramref name="monoImage"/> should be processed by <see cref="Binarization"/> or <see cref="Grayscale"/>, without colors.
        ''' </summary>
        ''' <param name="monoImage"></param>
        ''' <param name="characters"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Convert2ASCII(monoImage As Image, Optional characters As WeightedChar() = Nothing) As String
            '
            '             * ALGORITHM:
            '             * 
            '             *  1- Get target Image size (w=Width,h=Height)
            '             *  2- Create Result Image with white background and size W = w*character_image_width
            '             *                                                        H = h*character_image_height
            '             *  3- Create empty string to hold the text   
            '             *  
            '             *  4- for (int j=0;j=target_Image_Height;j++) --> ALL ROWS 
            '             *       5- Create row text string
            '             *       for (int i=0;i=target_Image_Width;i++) --> ALL COLUMNS
            '             *          6- Get target pixel weight
            '             *          7- Get closest weight from character list
            '             *          8- Paste character image in position w = i*character_image_width
            '             *                                               h = j*character_image_height
            '             *            ¡¡ Be careful with coordinate system when placing Images !!
            '             *          9- Add (string)character to row text string
            '             *       10- Add row text string to text holding string
            '             *  11 - return resulting Image & Text
            '            
            Using out As New MemoryStream
                Using writer As New StreamWriter(out, Encoding.ASCII)
                    Call monoImage.WriteASCIIStream(writer, characters)
                End Using

                Return Encoding.ASCII.GetString(out.ToArray)
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub WriteASCIIStream(monoImage As Image, out As StreamWriter, Optional characters As WeightedChar() = Nothing)
            Call monoImage.writeInternal(out, characters Or WeightedChar.getDefaultCharSet)
        End Sub

        <Extension>
        Private Sub writeInternal(monoImage As Image, out As StreamWriter, characters As WeightedChar())
            Using blackAndWhite As BitmapBuffer = BitmapBuffer.FromImage(monoImage)
                For j As Integer = 0 To monoImage.Height - 1
                    Dim line As New List(Of String)() From {}

                    For i As Integer = 0 To monoImage.Width - 1
                        ' COLUMN
                        Dim pixel As Color = blackAndWhite.GetPixel(i, j)
                        Dim targetvalue As Double = (CInt(pixel.R) + CInt(pixel.G) + CInt(pixel.B)) \ 3
                        Dim closestchar As WeightedChar =
                            characters _
                            .Where(Function(t)
                                       Return std.Abs(t.Weight - targetvalue) = characters.Min(Function(e) std.Abs(e.Weight - targetvalue))
                                   End Function) _
                            .FirstOrDefault()

                        Call line.Add(closestchar.Character)
                    Next

                    Call out.WriteLine(line.JoinBy(""))
                Next
            End Using
        End Sub

        ReadOnly defaultFont As [Default](Of Font) = CharSet.DefaultFont

        ''' <summary>
        ''' 将字符转换为图像
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="textColor"></param>
        ''' <param name="backColor"></param>
        ''' <param name="WidthAndHeight"></param>
        ''' <param name="fontStyle"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DrawText(text$,
                                 textColor As Color,
                                 backColor As Color,
                                 Optional WidthAndHeight As SizeF = Nothing,
                                 Optional fontStyle As Font = Nothing) As Image

            ' Get char width for insertion point calculation purposes
            Dim font As Font = fontStyle Or defaultFont
            Dim textSize As SizeF = FontFace.MeasureString(text, font)

            If WidthAndHeight.IsEmpty Then
                WidthAndHeight = textSize
            End If

            ' Create a new image of the right size
            Dim w% = CInt(std.Truncate(WidthAndHeight.Width))
            Dim h% = CInt(std.Truncate(WidthAndHeight.Height))
            Dim img As New Bitmap(w, h)

            ' Get a graphics object
            Using drawing = DriverLoad.CreateGraphicsDevice(img, driver:=Drivers.GDI)

                ' Create a brush for the text
                Dim textBrush As New SolidBrush(textColor)
                Dim x = (WidthAndHeight.Width - textSize.Width) / 2

                ' Paint the background
                Call drawing.Clear(backColor)
                Call drawing.DrawString(text, font, textBrush, x, 0)

                Return img
            End Using
        End Function
    End Module
End Namespace
