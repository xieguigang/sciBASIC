#Region "Microsoft.VisualBasic::9f03784a46918f03ad63405448272672, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\ImageUtils.vb"

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

    '   Total Lines: 30
    '    Code Lines: 26 (86.67%)
    ' Comment Lines: 2 (6.67%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (6.67%)
    '     File Size: 1.12 KB


    '     Module ImageUtils
    ' 
    '         Function: GetImageWidthSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging.BitmapImage.StreamWriter

    Module ImageUtils

        Friend Function GetImageWidthSize(width As Integer, colorBit As BitmapColorBit) As Integer
            Dim bitCount = width
            Select Case colorBit
                Case BitmapColorBit.Bit1
                    bitCount *= 1
                Case BitmapColorBit.Bit4
                    bitCount *= 4
                Case BitmapColorBit.Bit8
                    bitCount *= 8
                Case BitmapColorBit.Bit24
                    bitCount *= 24
                Case BitmapColorBit.Bit32
                    bitCount *= 32
                Case Else
                    Throw New InvalidOperationException($"Invalid color bit. : {colorBit}")
            End Select
            ' 8 bit
            Dim bitMod = bitCount Mod 8
            Dim bitTotal = bitCount + If(bitMod = 0, 0, 8 - bitMod)
            ' 4 byte
            Dim byteCount = bitTotal / 8
            Dim byteMod = byteCount Mod 4
            Return byteCount + If(byteMod = 0, 0, 4 - byteMod)
        End Function
    End Module
End Namespace
