#Region "Microsoft.VisualBasic::6ce88adae808dcc86be6348061a89d5a, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapInfoHeader.vb"

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

    '   Total Lines: 48
    '    Code Lines: 43 (89.58%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (10.42%)
    '     File Size: 1.46 KB


    '     Structure BitmapInfoHeader
    ' 
    '         Function: GetDefault
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.StreamWriter

    <StructLayout(LayoutKind.Explicit, Pack:=1)>
    Friend Structure BitmapInfoHeader

        <FieldOffset(0)>
        Public Size As UInteger
        <FieldOffset(4)>
        Public Width As Integer
        <FieldOffset(8)>
        Public Height As Integer
        <FieldOffset(12)>
        Public Planes As Short
        <FieldOffset(14)>
        Public BitCount As UShort
        <FieldOffset(16)>
        Public Compression As UInteger
        <FieldOffset(20)>
        Public SizeImage As UInteger
        <FieldOffset(24)>
        Public XPixPerMeter As Integer
        <FieldOffset(28)>
        Public YPixPerMeter As Integer
        <FieldOffset(32)>
        Public ClrUsed As UInteger
        <FieldOffset(36)>
        Public CirImportant As UInteger

        Public Shared Function GetDefault() As BitmapInfoHeader
            Return New BitmapInfoHeader With {
                .Size = 40,
                .Width = 0,
                .Height = 0,
                .Planes = 1,
                .BitCount = BitmapColorBit.Bit1,
                .Compression = BitmapCompression.Rgb,
                .SizeImage = 0,
                .XPixPerMeter = 0,
                .YPixPerMeter = 0,
                .ClrUsed = 0,
                .CirImportant = 0
            }
        End Function

    End Structure
End Namespace
