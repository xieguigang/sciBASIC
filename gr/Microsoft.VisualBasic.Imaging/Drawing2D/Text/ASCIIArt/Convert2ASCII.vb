#Region "Microsoft.VisualBasic::c9b49f530f9064b6fc59d7ab8d9f014b, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\ASCIIArt\Convert2ASCII.vb"

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
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports sys = System.Math

Namespace Drawing2D.Text.ASCIIArt

    ''' <summary>
    ''' Program that converts images to ASCII art images
    ''' 
    ''' > https://github.com/juangallostra/Image2ASCII
    ''' </summary>
    Public Module HelperMethods

        <Extension>
        Public Function ASCIIImage(text$, Optional font$ = CSSFont.Win7Normal, Optional characters As WeightedChar() = Nothing) As String
            Dim image As Image = text.DrawText(Color.Black, Color.White, , CSSFont.TryParse(font).GDIObject)
            Return image.Convert2ASCII
        End Function

        ''' <summary>
        ''' Image input <paramref name="monoImage"/> should be processed by <see cref="Binarization"/> or <see cref="Grayscale"/>, without colors.
        ''' </summary>
        ''' <param name="monoImage"></param>
        ''' <param name="characters"></param>
        ''' <returns></returns>
        <Extension> Public Function Convert2ASCII(monoImage As Image, Optional characters As WeightedChar() = Nothing) As String
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
            Dim out As New MemoryStream

            Using writer As New StreamWriter(out, Encoding.ASCII)
                Call monoImage.WriteASCIIStream(writer, characters)
            End Using

            Return Encoding.ASCII.GetString(out.ToArray)
        End Function

        <Extension>
        Public Sub WriteASCIIStream(monoImage As Image, out As StreamWriter, Optional characters As WeightedChar() = Nothing)

            If characters Is Nothing Then
                characters = CharSet.GenerateFontWeights.ToArray
            End If

            Using BlackAndWhite As BitmapBuffer = BitmapBuffer.FromImage(monoImage)
                For j As Integer = 0 To monoImage.Height - 1
                    Dim line As New List(Of String)() From {}

                    For i As Integer = 0 To monoImage.Width - 1
                        ' COLUMN
                        Dim pixel As Color = BlackAndWhite.GetPixel(i, j)
                        Dim targetvalue As Double = (CInt(pixel.R) + CInt(pixel.G) + CInt(pixel.B)) \ 3
                        Dim closestchar As WeightedChar =
                            characters _
                            .Where(Function(t)
                                       Return sys.Abs(t.Weight - targetvalue) = characters.Min(Function(e) sys.Abs(e.Weight - targetvalue))
                                   End Function) _
                            .FirstOrDefault()

                        Call line.Add(closestchar.Character)
                    Next

                    Call out.WriteLine(line.JoinBy(""))
                Next
            End Using
        End Sub

        <Extension> Public Function DrawText(text$, textColor As Color, backColor As Color, Optional WidthAndHeight As SizeF = Nothing, Optional font As Font = Nothing) As Image
            Dim textSize As SizeF

            If font Is Nothing Then
                font = SystemFonts.DefaultFont
            End If

            ' Get char width for insertion point calculation purposes
            Using dummy_img As Image = New Bitmap(1, 1), dummy_drawing As Graphics = Graphics.FromImage(dummy_img)
                textSize = dummy_drawing.MeasureString(text, font)
            End Using

            If WidthAndHeight.IsEmpty Then
                WidthAndHeight = textSize
            End If

            ' Create a new image of the right size
            Dim img As New Bitmap(CInt(sys.Truncate(WidthAndHeight.Width)), CInt(sys.Truncate(WidthAndHeight.Height)))

            Using Drawing = Graphics.FromImage(img) ' Get a graphics object

                ' Create a brush for the text
                Dim textBrush As Brush = New SolidBrush(textColor)

                ' Paint the background
                Call Drawing.Clear(backColor)
                Call Drawing.DrawString(text, font, textBrush, (WidthAndHeight.Width - textSize.Width) / 2, 0)

                Return img
            End Using
        End Function
    End Module
End Namespace
