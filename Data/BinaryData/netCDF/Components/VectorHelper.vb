#Region "Microsoft.VisualBasic::b5c07199c1c35961486e6246d7868b35, Data\BinaryData\netCDF\Components\VectorHelper.vb"

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

    '   Total Lines: 59
    '    Code Lines: 53 (89.83%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (10.17%)
    '     File Size: 2.46 KB


    '     Module VectorHelper
    ' 
    '         Function: FromAny, vectorAuto
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Components

    <HideModuleName>
    Public Module VectorHelper

        Public Function FromAny(data As Array, type As CDFDataTypes) As ICDFDataVector
            Select Case type
                Case CDFDataTypes.NC_BYTE
                    Dim bytes As Byte()

                    If TypeOf data Is Byte() Then
                        bytes = data
                    ElseIf data.AsObjectEnumerator.All(Function(obj) TypeOf obj Is Byte()) Then
                        bytes = data.AsObjectEnumerator _
                            .Select(Function(obj)
                                        Return DirectCast(obj, Byte())(Scan0)
                                    End Function) _
                            .ToArray
                    Else
                        bytes = data.As(Of Byte).ToArray
                    End If

                    Return CType(bytes, bytes)
                Case CDFDataTypes.BOOLEAN
                    Return CType(data.vectorAuto(Of Boolean), flags)
                Case CDFDataTypes.NC_CHAR
                    Return CType(data.vectorAuto(Of Char), chars)
                Case CDFDataTypes.NC_DOUBLE
                    Return CType(data.vectorAuto(Of Double), doubles)
                Case CDFDataTypes.NC_FLOAT
                    Return CType(data.vectorAuto(Of Single), floats)
                Case CDFDataTypes.NC_INT
                    Return CType(data.vectorAuto(Of Integer), integers)
                Case CDFDataTypes.NC_SHORT
                    Return CType(data.vectorAuto(Of Short), shorts)
                Case CDFDataTypes.NC_INT64
                    Return CType(data.vectorAuto(Of Long), longs)
                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function vectorAuto(Of T)(data As Array) As T()
            If TypeOf data Is T() Then
                Return DirectCast(data, T())
            Else
                Return data.As(Of T).ToArray
            End If
        End Function
    End Module
End Namespace
