#Region "Microsoft.VisualBasic::ecb9bed2cc6c856e7a0e5d62f3e8708c, Data\BinaryData\DataStorage\netCDF\Components\Dimension.vb"

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

    '     Structure Dimension
    ' 
    '         Properties: [Boolean], [Byte], [Double], [Integer], [Long]
    '                     [Short], Float, Text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace netCDF.Components

    ''' <summary>
    ''' ``[name => size]``
    ''' </summary>
    ''' 
    <XmlType("dim", [Namespace]:=Xml.netCDF)>
    Public Structure Dimension

        ''' <summary>
        ''' String with the name of the dimension
        ''' </summary>
        <XmlAttribute> Dim name As String
        ''' <summary>
        ''' Number with the size of the dimension
        ''' </summary>
        <XmlText>
        Dim size As Integer

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function

        Public Shared ReadOnly Property [Double] As Dimension
            Get
                Return New Dimension With {.name = GetType(Double).FullName, .size = 8}
            End Get
        End Property

        Public Shared ReadOnly Property [Long] As Dimension
            Get
                Return New Dimension With {.name = GetType(Long).FullName, .size = 8}
            End Get
        End Property

        Public Shared ReadOnly Property Float As Dimension
            Get
                Return New Dimension With {.name = GetType(Single).FullName, .size = 4}
            End Get
        End Property

        Public Shared ReadOnly Property [Short] As Dimension
            Get
                Return New Dimension With {.name = GetType(Short).FullName, .size = 2}
            End Get
        End Property

        Public Shared ReadOnly Property [Integer] As Dimension
            Get
                Return New Dimension With {.name = GetType(Integer).FullName, .size = 4}
            End Get
        End Property

        Public Shared ReadOnly Property [Byte] As Dimension
            Get
                Return New Dimension With {.name = GetType(Byte).FullName, .size = 1}
            End Get
        End Property

        Public Shared ReadOnly Property [Boolean] As Dimension
            Get
                Return New Dimension With {.name = GetType(Boolean).FullName, .size = 1}
            End Get
        End Property

        Public Shared ReadOnly Property Text(fixedChars As Integer) As Dimension
            Get
                Return New Dimension With {.name = GetType(String).FullName, .size = fixedChars}
            End Get
        End Property
    End Structure

End Namespace
