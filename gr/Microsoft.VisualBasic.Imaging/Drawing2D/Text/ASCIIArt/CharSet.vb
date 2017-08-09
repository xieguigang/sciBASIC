#Region "Microsoft.VisualBasic::ab92f9edcd72f781928aa8d21461c028, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\ASCIIArt\CharSet.vb"

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
Imports System.Runtime.CompilerServices
Imports System.Math

Namespace Drawing2D.Vector.Text.ASCIIArt

    Public Module CharSet

        Public Class WeightedChar

            Public Property Character As String
            Public Property CharacterImage As Bitmap
            Public Property Weight As Double

            Public Overrides Function ToString() As String
                Return $"{Character} ({Weight})"
            End Function
        End Class

        '
        '         * The methods contained in this class are executed at the inizialization
        '         * of the program, as the results from their calculations are needed for
        '         * the image conversion and are not user-dependent, thus not depending from
        '         * the actions taken by the user.
        '         * 
        '         * It essentially does, foreach ASCII character between 32-126, the following:
        '         *  - Calculate its weight, defined as: number of black pixels / number of pixels in character image
        '         *      Note: To understand the process in depth, please read the code and comments below.
        '         *  - Generate a square shaped image of the character, with the character in question located in the 
        '         *    center of the image. The character is drawn in black on top of a white background.
        '         *  - Associate the character in question (char type) to its image and weight.
        '         *  
        '         * All this character information (character, weight and image) is stored in a custom class (WeightedChar)
        '         * which can hold the 3 properties.
        '         * 
        '         * All the classes resulting from the calculations are stored in a List so we can access the results.
        '         
        Public Function GenerateFontWeights() As List(Of WeightedChar)
            ' Collect chars, their Images and weights in a list of WeightedChar
            Dim WeightedChars As New List(Of WeightedChar)()

            Dim commonsize As SizeF = GetGeneralSize()
            ' Get standard size (nxn square), which will be common to all CharImages
            For i As Integer = 32 To 126
                ' Iterate through Chars
                Dim forweighting = New WeightedChar()
                ' New object to hold Image, Weight and Char of new character
                Dim c As Char = Convert.ToChar(i)
                If Not Char.IsControl(c) Then
                    forweighting.Weight = c.GetWeight(commonsize)
                    ' Get character weight
                    forweighting.Character = c.ToString()
                    ' Get character representation (the actual char)
                    ' Get character Image
                    forweighting.CharacterImage = DirectCast(HelperMethods.DrawText(c.ToString(), Color.Black, Color.White, commonsize), Bitmap)
                End If
                WeightedChars.Add(forweighting)
            Next

            WeightedChars = WeightedChars.LinearMap()
            ' Linearly map character weights to be in the range 0-255 -> mapping linearly from: MinCalcWeight - MaxCalcWeight to 0-255; 
            ' This is done to be able to directly map pixels to characters
            Return WeightedChars
        End Function

#Region "[GenerateFontWeights Helper methods]"

        Private Function GetGeneralSize() As SizeF
            Dim generalsize As New SizeF(0, 0)

            For i As Integer = 32 To 126
                ' Iterate through contemplated characters calculating necessary width
                Dim c As Char = Convert.ToChar(i)
                ' Create a dummy bitmap just to get a graphics object
                Using img As Image = New Bitmap(1, 1), drawing As Graphics = Graphics.FromImage(img)

                    ' Measure the string to see its dimensions using the graphics object
                    Dim textSize As SizeF = drawing.MeasureString(c.ToString(), SystemFonts.DefaultFont)
                    ' Update, if necessary, the max width and height
                    If textSize.Width > generalsize.Width Then
                        generalsize.Width = textSize.Width
                    End If
                    If textSize.Height > generalsize.Height Then
                        generalsize.Height = textSize.Height
                    End If
                End Using
            Next

            ' Make sure generalsize is made of integers 
            generalsize.Width = CInt(Truncate(generalsize.Width))
            generalsize.Height = CInt(Truncate(generalsize.Height))
            ' and size defines a square to maintain image proportions
            ' as the ASCII transformation will be 1 pixel = 1 character Image
            ' thus substituting one pixel by one character image
            If generalsize.Width > generalsize.Height Then
                generalsize.Height = generalsize.Width
            Else
                generalsize.Width = generalsize.Height
            End If

            Return generalsize
        End Function

        <Extension> Private Function GetWeight(c As Char, size As SizeF) As Double
            Dim CharImage = HelperMethods.DrawText(c.ToString(), Color.Black, Color.White, size)

            Dim btm As New Bitmap(CharImage)
            Dim totalsum As Double = 0

            For i As Integer = 0 To btm.Width - 1
                For j As Integer = 0 To btm.Height - 1
                    Dim pixel As Color = btm.GetPixel(i, j)
                    totalsum = totalsum + (CInt(pixel.R) + CInt(pixel.G) + CInt(pixel.B)) \ 3
                Next
            Next
            ' Weight = (sum of (R+G+B)/3 for all pixels in image) / Area. (Where Area = Width*Height )
            Return totalsum / (size.Height * size.Width)
        End Function

        <Extension> Private Function LinearMap(characters As List(Of WeightedChar)) As List(Of WeightedChar)
            Dim max As Double = characters.Max(Function(c) c.Weight)
            Dim min As Double = characters.Min(Function(c) c.Weight)
            Dim range As Double = 255
            ' y = mx + n (where y c (0-255))
            Dim slope As Double = range / (max - min)
            Dim n As Double = -min * slope
            For Each charactertomap As WeightedChar In characters
                charactertomap.Weight = slope * charactertomap.Weight + n
            Next
            Return characters
        End Function
#End Region
    End Module
End Namespace

