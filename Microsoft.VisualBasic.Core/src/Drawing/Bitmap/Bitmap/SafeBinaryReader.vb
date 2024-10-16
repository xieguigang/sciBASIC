#Region "Microsoft.VisualBasic::bcda9249fd29f7701a100da6bbbe074e, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\SafeBinaryReader.vb"

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

    '   Total Lines: 41
    '    Code Lines: 32 (78.05%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (21.95%)
    '     File Size: 1.36 KB


    '     Class SafeBinaryReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ReadInt16, ReadInt32, ReadInt64, ReadUInt16, ReadUInt32
    '                   ReadUInt64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Imaging.BitmapImage.FileStream

    Public Class SafeBinaryReader : Inherits BinaryReader

        Public Sub New(stream As Stream)
            MyBase.New(stream)
        End Sub

        Public Overrides Function ReadInt16() As Short
            Dim data = MyBase.ReadBytes(2)
            If Not BitConverter.IsLittleEndian Then Array.Reverse(data)
            Return BitConverter.ToInt16(data, 0)
        End Function

        Public Overrides Function ReadUInt16() As UShort
            Return CUShort(ReadInt16())
        End Function

        Public Overrides Function ReadInt32() As Integer
            Dim data = MyBase.ReadBytes(4)
            If Not BitConverter.IsLittleEndian Then Array.Reverse(data)
            Return BitConverter.ToInt32(data, 0)
        End Function

        Public Overrides Function ReadUInt32() As UInteger
            Return CUInt(ReadUInt32())
        End Function

        Public Overrides Function ReadInt64() As Long
            Dim data = MyBase.ReadBytes(8)
            If Not BitConverter.IsLittleEndian Then Array.Reverse(data)
            Return BitConverter.ToInt64(data, 0)
        End Function

        Public Overrides Function ReadUInt64() As ULong
            Return CULng(ReadInt64())
        End Function
    End Class
End Namespace

