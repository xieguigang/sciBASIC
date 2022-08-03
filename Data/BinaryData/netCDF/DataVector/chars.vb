#Region "Microsoft.VisualBasic::578bf8b55884cf5d813cf444998099c1, sciBASIC#\Data\BinaryData\netCDF\DataVector\chars.vb"

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

    '   Total Lines: 34
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.21 KB


    '     Class chars
    ' 
    '         Properties: cdfDataType
    ' 
    '         Function: LoadJSON
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DataVector

    Public Class chars : Inherits CDFData(Of Char)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.CHAR
            End Get
        End Property

        Public Function LoadJSON(Of T)() As T
            Return New String(buffer).LoadJSON(Of T)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As String) As chars
            Return New chars With {.buffer = data.ToArray}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Char()) As chars
            Return New chars With {.buffer = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(chars As chars) As String
            Return New String(chars.buffer)
        End Operator
    End Class
End Namespace
