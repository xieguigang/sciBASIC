#Region "Microsoft.VisualBasic::8a82bedaeac21f69a31f2da7a127dd05, Data\BinaryData\msgpack\Constants\MsgPackFormats.vb"

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

    '   Total Lines: 60
    '    Code Lines: 47 (78.33%)
    ' Comment Lines: 9 (15.00%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 4 (6.67%)
    '     File Size: 2.38 KB


    '     Class MsgPackFormats
    ' 
    '         Function: GetEnums
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5 = 0 Then
Imports System.Linq
#End If

Namespace Constants

    ''' <summary>
    ''' 枚举所有的基础类型常量
    ''' </summary>
    ''' <remarks>
    ''' 20211029
    ''' 
    ''' 因为有些常量的值会出现重复，所以使用Enum枚举类型来表示会
    ''' 产生符号冲突。在这里就只用普通的常量值来表示了
    ''' </remarks>
    Public NotInheritable Class MsgPackFormats

        Public Const NIL As Byte = &HC0
        Public Const FLOAT_32 As Byte = &HCA
        Public Const FLOAT_64 As Byte = &HCB
        Public Const [DOUBLE] As Byte = &HCB
        Public Const UINT_8 As Byte = &HCC
        Public Const UNSIGNED_INTEGER_8 As Byte = &HCC
        Public Const UINT_16 As Byte = &HCD
        Public Const UNSIGNED_INTEGER_16 As Byte = &HCD
        Public Const UINT_32 As Byte = &HCE
        Public Const UNSIGNED_INTEGER_32 As Byte = &HCE
        Public Const UINT_64 As Byte = &HCF
        Public Const UNSIGNED_INTEGER_64 As Byte = &HCF
        Public Const INT_8 As Byte = &HD0
        Public Const INTEGER_8 As Byte = &HD0
        Public Const INT_16 As Byte = &HD1
        Public Const INTEGER_16 As Byte = &HD1
        Public Const INT_32 As Byte = &HD2
        Public Const INTEGER_32 As Byte = &HD2
        Public Const INT_64 As Byte = &HD3
        Public Const INTEGER_64 As Byte = &HD3
        Public Const STR_8 As Byte = &HD9
        Public Const STRING_8 As Byte = &HD9
        Public Const STR_16 As Byte = &HDA
        Public Const STRING_16 As Byte = &HDA
        Public Const STR_32 As Byte = &HDB
        Public Const STRING_32 As Byte = &HDB
        Public Const ARRAY_16 As Byte = &HDC
        Public Const ARRAY_32 As Byte = &HDD
        Public Const MAP_16 As Byte = &HDE
        Public Const MAP_32 As Byte = &HDF

        Public Shared Function GetEnums() As Dictionary(Of String, Byte)
            Static enums As Dictionary(Of String, Byte) = GetType(MsgPackFormats) _
                .GetFields _
                .Where(Function(f) f.IsLiteral) _
                .ToDictionary(Function(f) f.Name,
                              Function(f)
                                  Return CByte(f.GetValue(Nothing))
                              End Function)
            Return enums
        End Function
    End Class
End Namespace
