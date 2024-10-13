#Region "Microsoft.VisualBasic::a767f348838f9487733ae4a03f3100e8, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapFileHeader.vb"

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
    '    Code Lines: 25 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (16.67%)
    '     File Size: 825 B


    '     Structure BitmapFileHeader
    ' 
    '         Function: GetDefault
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.StreamWriter

    <StructLayout(LayoutKind.Explicit, Pack:=1)>
    Friend Structure BitmapFileHeader

        <FieldOffset(0)>
        Public Type As UShort
        <FieldOffset(2)>
        Public Size As UInteger
        <FieldOffset(6)>
        Public Reserved1 As UShort
        <FieldOffset(8)>
        Public Reserved2 As UShort
        <FieldOffset(10)>
        Public OffBits As UInteger

        Public Shared Function GetDefault() As BitmapFileHeader
            Return New BitmapFileHeader With {
                .Type = &H4D42,
                .Size = 0,
                .Reserved1 = 0,
                .Reserved2 = 0,
                .OffBits = 0
            }
        End Function

    End Structure
End Namespace
