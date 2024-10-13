#Region "Microsoft.VisualBasic::6eefc5ab270d42896c71e33970a80b7d, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapPaletteData.vb"

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

    '   Total Lines: 23
    '    Code Lines: 19 (82.61%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (17.39%)
    '     File Size: 604 B


    '     Structure BitmapPaletteData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.StreamWriter

    <StructLayout(LayoutKind.Explicit, Pack:=1)>
    Friend Structure BitmapPaletteData

        <FieldOffset(0)>
        Public Red As Byte
        <FieldOffset(1)>
        Public Green As Byte
        <FieldOffset(2)>
        Public Blue As Byte
        <FieldOffset(3)>
        Public Reserve As Byte

        Public Sub New(red As Byte, green As Byte, blue As Byte)
            Me.Red = red
            Me.Green = green
            Me.Blue = blue
        End Sub
    End Structure
End Namespace
