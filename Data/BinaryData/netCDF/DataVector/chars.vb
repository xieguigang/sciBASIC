#Region "Microsoft.VisualBasic::d84e44ed49d918ade553e4a03928dcfc, sciBASIC#\Data\BinaryData\netCDF\DataVector\chars.vb"

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

    '   Total Lines: 54
    '    Code Lines: 35
    ' Comment Lines: 9
    '   Blank Lines: 10
    '     File Size: 1.79 KB


    '     Class chars
    ' 
    '         Properties: cdfDataType
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: LoadJSON
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DataVector

    ''' <summary>
    ''' A single string value is a char vector
    ''' </summary>
    Public Class chars : Inherits CDFData(Of Char)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.CHAR
            End Get
        End Property

        Sub New(str As String)
            buffer = str.ToArray
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' convert the string collection into json string
        ''' </summary>
        ''' <param name="strs"></param>
        Sub New(strs As IEnumerable(Of String))
            buffer = strs.ToArray.GetJson.ToArray
        End Sub

        ''' <summary>
        ''' treat the char data in this vector as the json string, and the load string vector from the json string
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Function LoadJSON(Of T)(Optional strict As Boolean = True) As T
            Return New String(buffer).LoadJSON(Of T)(throwEx:=strict)
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
