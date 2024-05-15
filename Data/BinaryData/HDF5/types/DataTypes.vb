#Region "Microsoft.VisualBasic::d45a6445ca7d8d53966a92f60e206c37, Data\BinaryData\HDF5\types\DataTypes.vb"

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

    '   Total Lines: 58
    '    Code Lines: 17
    ' Comment Lines: 39
    '   Blank Lines: 2
    '     File Size: 1.52 KB


    '     Class NamespaceDoc
    ' 
    ' 
    ' 
    '     Enum DataTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace type

    ''' <summary>
    ''' The HDF5 primitive data types model, classes representing the data types of HDF5 datasets.
    ''' </summary>
    Friend Class NamespaceDoc
    End Class

    ''' <summary>
    ''' 对一些在HDF5文件之中的基础数据类型的枚举，例如长整型，双精度，字符串等
    ''' </summary>
    Public Enum DataTypes
        ''' <summary>
        ''' Fixed-Point
        ''' </summary>
        DATATYPE_FIXED_POINT = 0
        ''' <summary>
        ''' Floating-point
        ''' </summary>
        DATATYPE_FLOATING_POINT = 1
        ''' <summary>
        ''' Time
        ''' </summary>
        DATATYPE_TIME = 2
        ''' <summary>
        ''' String
        ''' </summary>
        DATATYPE_STRING = 3
        ''' <summary>
        ''' Bit field
        ''' </summary>
        DATATYPE_BIT_FIELD = 4
        ''' <summary>
        ''' Opaque
        ''' </summary>
        DATATYPE_OPAQUE = 5
        ''' <summary>
        ''' Compound
        ''' </summary>
        DATATYPE_COMPOUND = 6
        ''' <summary>
        ''' Reference
        ''' </summary>
        DATATYPE_REFERENCE = 7
        ''' <summary>
        ''' Enumerated
        ''' </summary>
        DATATYPE_ENUMS = 8
        ''' <summary>
        ''' Variable-Length
        ''' </summary>
        DATATYPE_VARIABLE_LENGTH = 9
        ''' <summary>
        ''' Array
        ''' </summary>
        DATATYPE_ARRAY = 10
    End Enum
End Namespace
