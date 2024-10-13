#Region "Microsoft.VisualBasic::ed1dda5563410a59b26a6ce5c5ddb336, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\ASCIIArt\CharSet.vb"

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

'   Total Lines: 176
'    Code Lines: 111 (63.07%)
' Comment Lines: 39 (22.16%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 26 (14.77%)
'     File Size: 8.72 KB


'     Module CharSet
' 
'         Function: (+2 Overloads) GenerateFontWeights, GetDotMatrix, (+2 Overloads) GetGeneralSize, GetWeight, LinearMap
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Linq

Namespace Drawing2D.Text.ASCIIArt

    Public Module CharSet

        Const FrogASCII$ = "
⣿⣿⣿⣿⣿⣿⢟⣡⣴⣶⣶⣦⣌⡛⠟⣋⣩⣬⣭⣭⡛⢿⣿⣿⣿⣿
⣿⣿⣿⣿⠋⢰⣿⣿⠿⣛⣛⣙⣛⠻⢆⢻⣿⠿⠿⠿⣿⡄⠻⣿⣿⣿ 
⣿⣿⣿⠃⢠⣿⣿⣶⣿⣿⡿⠿⢟⣛⣒⠐⠲⣶⡶⠿⠶⠶⠦⠄⠙⢿ 
⣿⠋⣠⠄⣿⣿⣿⠟⡛⢅⣠⡵⡐⠲⣶⣶⣥⡠⣤⣵⠆⠄⠰⣦⣤⡀ 
⠇⣰⣿⣼⣿⣿⣧⣤⡸⢿⣿⡀⠂⠁⣸⣿⣿⣿⣿⣇⠄⠈⢀⣿⣿⠿ 
⣰⣿⣿⣿⣿⣿⣿⣿⣷⣤⣈⣙⠶⢾⠭⢉⣁⣴⢯⣭⣵⣶⠾⠓⢀⣴
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣉⣤⣴⣾⣿⣿⣦⣄⣤⣤⣄⠄⢿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⠿⠿⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣇⠈⢿
⣿⣿⣿⣿⣿⣿⡟⣰⣞⣛⡒⢒⠤⠦⢬⣉⣉⣉⣉⣉⣉⣉⡥⠴⠂⢸
⠻⣿⣿⣿⣿⣏⠻⢌⣉⣉⣩⣉⡛⣛⠒⠶⠶⠶⠶⠶⠶⠶⠶⠂⣸⣿
⣥⣈⠙⡻⠿⠿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣾⣿⠿⠛⢉⣠⣶⣶⣿⣿
⣿⣿⣿⣶⣬⣅⣒⣒⡂⠈⠭⠭⠭⠭⠭⢉⣁⣄⡀⢾⣿⣿⣿⣿⣿⣿
"

        Friend ReadOnly DefaultFont As [Default](Of Font) = New Font("Tahoma", 8.0F)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDotMatrix(Optional font As Font = Nothing) As WeightedChar()
            Return FrogASCII.TrimNewLine("").Trim.AsEnumerable.Distinct.GenerateFontWeights(font)
        End Function

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GenerateFontWeights(Optional font As Font = Nothing) As WeightedChar()
            Dim allPrintables = Enumerable _
                .Range(32, 126) _
                .Select(AddressOf Convert.ToChar) _
                .ToArray

            ' New object to hold Image, Weight and Char of new character
            ' For i As Integer = 32 To 126
            Return allPrintables.GenerateFontWeights(font Or DefaultFont)
        End Function

        <Extension>
        Public Function GenerateFontWeights(chars As IEnumerable(Of Char), Optional font As Font = Nothing) As WeightedChar()
            ' Collect chars, their Images and weights in a list of WeightedChar
            Dim weightedChars As New List(Of WeightedChar)()
            Dim charList = chars.SafeQuery.Distinct.ToArray
            Dim commonsize As SizeF = charList.GetGeneralSize(font Or DefaultFont)

            ' Get standard size (nxn square), which will be common to all CharImages
            For Each c As Char In chars.SafeQuery.Distinct
                ' Iterate through Chars
                Dim forweighting = New WeightedChar()

                If Not Char.IsControl(c) Then
                    forweighting.Weight = c.GetWeight(commonsize)
                    ' Get character weight
                    forweighting.Character = c.ToString()
                    ' Get character representation (the actual char)
                    ' Get character Image
                    forweighting.CharacterImage = DirectCast(HelperMethods.DrawText(
                        text:=c.ToString(),
                        textColor:=Color.Black,
                        backColor:=Color.White,
                        WidthAndHeight:=commonsize
                    ), Bitmap)
                End If

                weightedChars.Add(forweighting)
            Next

            weightedChars = weightedChars.LinearMap()
            ' Linearly map character weights to be in the range 0-255 
            ' -> mapping linearly from: MinCalcWeight - MaxCalcWeight to 0-255; 
            ' This is done to be able to directly map pixels to characters
            Return weightedChars.ToArray
        End Function

#Region "[GenerateFontWeights Helper methods]"

        <Extension>
        Private Function GetGeneralSize(allPrintables As Char(), font As Font) As SizeF
            ' Create a dummy bitmap just to get a graphics object
            Using g As IGraphics = DriverLoad.CreateGraphicsDevice(New Size(1, 1), driver:=Drivers.GDI)
                Return g.GetGeneralSize(allPrintables, font)
            End Using
        End Function

        <Extension>
        Public Function GetGeneralSize(g As IGraphics, chars As IEnumerable(Of Char), font As Font) As SizeF
            Dim generalsize As New SizeF(0, 0)

            ' Iterate through contemplated characters calculating necessary width
            For Each c As Char In chars
                ' Measure the string to see its dimensions using the graphics object
                Dim textSize As SizeF = g.MeasureString(c.ToString(), font)

                ' Update, if necessary, the max width and height
                If textSize.Width > generalsize.Width Then
                    generalsize.Width = textSize.Width
                End If
                If textSize.Height > generalsize.Height Then
                    generalsize.Height = textSize.Height
                End If
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
            Dim charImage = HelperMethods.DrawText(c.ToString(), Color.Black, Color.White, size)
            Dim totalsum As Double = 0

            Using btm As BitmapBuffer = BitmapBuffer.FromImage(charImage)
                For i As Integer = 0 To btm.Width - 1
                    For j As Integer = 0 To btm.Height - 1
                        Dim pixel As Color = btm.GetPixel(i, j)
                        totalsum += (CInt(pixel.R) + CInt(pixel.G) + CInt(pixel.B)) \ 3
                    Next
                Next
            End Using

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
