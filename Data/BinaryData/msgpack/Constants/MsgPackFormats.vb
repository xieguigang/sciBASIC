#Region "Microsoft.VisualBasic::fc878262f94be028b26b7a5ea3461937, Data\BinaryData\msgpack\Constants\Formats.vb"

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

'     Class Formats
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Namespace Constants

    Public Enum MsgPackFormats As Byte
        NIL = &HC0
        FLOAT_32 = &HCA
        FLOAT_64 = &HCB
        [DOUBLE] = &HCB
        UINT_8 = &HCC
        UNSIGNED_INTEGER_8 = &HCC
        UINT_16 = &HCD
        UNSIGNED_INTEGER_16 = &HCD
        UINT_32 = &HCE
        UNSIGNED_INTEGER_32 = &HCE
        UINT_64 = &HCF
        UNSIGNED_INTEGER_64 = &HCF
        INT_8 = &HD0
        INTEGER_8 = &HD0
        INT_16 = &HD1
        INTEGER_16 = &HD1
        INT_32 = &HD2
        INTEGER_32 = &HD2
        INT_64 = &HD3
        INTEGER_64 = &HD3
        STR_8 = &HD9
        STRING_8 = &HD9
        STR_16 = &HDA
        STRING_16 = &HDA
        STR_32 = &HDB
        STRING_32 = &HDB
        ARRAY_16 = &HDC
        ARRAY_32 = &HDD
        MAP_16 = &HDE
        MAP_32 = &HDF
    End Enum
End Namespace
