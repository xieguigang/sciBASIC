#Region "Microsoft.VisualBasic::ba70f38f26d9de0046c8f5099e0e7e8a, Data\BinaryData\netCDF\Components\variable.vb"

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

    '   Total Lines: 105
    '    Code Lines: 45
    ' Comment Lines: 50
    '   Blank Lines: 10
    '     File Size: 3.67 KB


    '     Class variable
    ' 
    '         Properties: attributes, dimensionality, dimensions, name, offset
    '                     record, size, type, value
    ' 
    '         Function: FindAttribute, GetRegion, ToString
    ' 
    '     Enum dimensionality
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data

Namespace Components

    ''' <summary>
    ''' 变量对象,CDF文件之中的实验数据之类的数据都是保存于这个对象之中的
    ''' </summary>
    Public Class variable

        ''' <summary>
        ''' String with the name of the variable
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        ''' <summary>
        ''' Array with the dimension IDs of the variable.
        ''' (<see cref="Header.dimensions"/>)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Dimension ID (index into dim_list) for variable
        ''' shape. We say this Is a "record variable" if And only
        ''' if the first dimension Is the record dimension.
        ''' </remarks>
        <XmlAttribute> Public Property dimensions As Integer()

        Public ReadOnly Property dimensionality As dimensionality
            Get
                If dimensions.IsNullOrEmpty Then
                    Return dimensionality.scalar
                ElseIf dimensions.Length = 1 Then
                    Return dimensionality.vector
                Else
                    Return dimensionality.matrix
                End If
            End Get
        End Property

        ''' <summary>
        ''' Array with the attributes of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property attributes As attribute()
        ''' <summary>
        ''' String with the type of the variable
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property type As CDFDataTypes
        ''' <summary>
        ''' Number with the size of the variable.(在文件之中的数据字节大小)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Variable size. If not a record variable, the amount
        ''' of space in bytes allocated to the variable's data.
        ''' </remarks>
        <XmlAttribute> Public Property size As Integer
        ''' <summary>
        ''' Number with the offset where of the variable begins
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property offset As UInteger
        ''' <summary>
        ''' True if Is a record variable, false otherwise
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property record As Boolean

        ''' <summary>
        ''' 惰性求值的属性
        ''' </summary>
        ''' <returns></returns>
        Public Property value As ICDFDataVector

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindAttribute(name As String) As attribute
            Return attributes.FirstOrDefault(Function(a) a.name = name)
        End Function

        Public Function GetRegion() As BufferRegion
            Return New BufferRegion With {
                .position = offset,
                .size = size
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name}[offset={offset}] As {type.Description}"
        End Function
    End Class

    ''' <summary>
    ''' dimensionality (rank) of the variable
    ''' 
    ''' 0 for scalar, 1 for vector, 2 for matrix
    ''' </summary>
    Public Enum dimensionality
        scalar = 0
        vector = 1
        matrix = 2
    End Enum
End Namespace
