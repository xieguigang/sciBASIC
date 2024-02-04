﻿#Region "Microsoft.VisualBasic::eb065e625b7649b46bf1f9cc3efcac56, sciBASIC#\Data\BinaryData\netCDF\DataVector\CDFData.vb"

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

    '   Total Lines: 155
    '    Code Lines: 127
    ' Comment Lines: 10
    '   Blank Lines: 18
    '     File Size: 6.94 KB


    '     Class CDFData
    ' 
    '         Properties: genericValue, ICDFDataVector_length
    ' 
    '         Function: GetBuffer, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http

Namespace DataVector

    ''' <summary>
    '''  存储在CDF文件之中的数据的统一接口模块
    ''' </summary>
    Public MustInherit Class CDFData(Of T) : Inherits Vector(Of T)
        Implements ICDFDataVector

        Public MustOverride ReadOnly Property cdfDataType As CDFDataTypes Implements ICDFDataVector.cdfDataType

        Public ReadOnly Property genericValue As Array Implements ICDFDataVector.genericValue
            Get
                Return buffer
            End Get
        End Property

        Private ReadOnly Property ICDFDataVector_length As Integer Implements ICDFDataVector.length
            Get
                Return buffer.Length
            End Get
        End Property

        Default Public Overloads Property Item(i As Integer) As T
            Get
                Return buffer(i)
            End Get
            Set(value As T)
                buffer(i) = value
            End Set
        End Property

        Default Public Overloads Property Item(i As i32) As T
            Get
                Return buffer(i)
            End Get
            Set(value As T)
                buffer(i) = value
            End Set
        End Property

        ''' <summary>
        ''' get vector parts by range
        ''' </summary>
        ''' <param name="rangeMin">the start index</param>
        ''' <param name="rangeMax">the ends index</param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item(rangeMin As Integer, rangeMax As Integer) As T()
            Get
                Dim size As Integer = rangeMax - rangeMin
                Dim vec As T() = New T(size - 1) {}

                Call System.Array.ConstrainedCopy(buffer, rangeMin, vec, Scan0, size)

                Return vec
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim stringify$
            Dim range As DoubleRange = Nothing
            Dim rangeStr$

            Select Case cdfDataType
                Case CDFDataTypes.NC_BYTE : stringify = DirectCast(genericValue, Byte()).ToBase64String
                Case CDFDataTypes.NC_CHAR : stringify = DirectCast(genericValue, Char()).CharString
                Case CDFDataTypes.NC_DOUBLE : stringify = DirectCast(genericValue, Double()).Select(Function(d) d.ToString("G3")).JoinBy(",")
                Case CDFDataTypes.NC_FLOAT : stringify = DirectCast(genericValue, Single()).Select(Function(d) d.ToString("G3")).JoinBy(",")
                Case CDFDataTypes.NC_INT : stringify = DirectCast(genericValue, Integer()).JoinBy(",")
                Case CDFDataTypes.NC_SHORT : stringify = DirectCast(genericValue, Short()).JoinBy(",")
                Case CDFDataTypes.NC_INT64 : stringify = DirectCast(genericValue, Long()).JoinBy(",")
                Case CDFDataTypes.BOOLEAN : stringify = DirectCast(genericValue, Boolean()).Select(Function(b) If(b, 1, 0)).JoinBy(",")
                Case Else
                    Return "invalid!"
            End Select

            Select Case cdfDataType
                Case CDFDataTypes.NC_BYTE : range = DirectCast(genericValue, Byte()).Select(Function(b) CDbl(b)).ToArray
                Case CDFDataTypes.NC_DOUBLE : range = DirectCast(genericValue, Double()).ToArray
                Case CDFDataTypes.NC_FLOAT : range = DirectCast(genericValue, Single()).Select(Function(d) CDbl(d)).ToArray
                Case CDFDataTypes.NC_INT : range = DirectCast(genericValue, Integer()).Select(Function(i) CDbl(i)).ToArray
                Case CDFDataTypes.NC_SHORT : range = DirectCast(genericValue, Short()).Select(Function(s) CDbl(s)).ToArray
                Case CDFDataTypes.NC_INT64 : range = DirectCast(genericValue, Long()).Select(Function(l) CDbl(l)).ToArray
                Case Else
                    ' do nothing
            End Select

            If Not range Is Nothing Then
                rangeStr = $"; min:{range.Min}, max:{range.Max}"
            Else
                rangeStr = $""
            End If
            If (stringify.Length > 50) Then
                stringify = stringify.Substring(0, 50)
            End If
            If (cdfDataType <> CDFDataTypes.undefined) Then
                stringify &= $" (length: ${Me.Length})"
            End If

            Return $"[{cdfDataType}{range}] {stringify}"
        End Function

        Public Function GetBuffer(encoding As Encoding) As Byte() Implements ICDFDataVector.GetBuffer
            Dim chunks As Byte()()

            Select Case cdfDataType
                Case CDFDataTypes.NC_BYTE : Return DirectCast(CObj(Me), bytes).Array
                Case CDFDataTypes.BOOLEAN : Return DirectCast(CObj(Me), flags).Array.Select(Function(b) CByte(If(b, 1, 0))).ToArray
                Case CDFDataTypes.NC_CHAR : Return encoding.GetBytes(DirectCast(CObj(Me), chars).CharString)
                Case CDFDataTypes.NC_DOUBLE
                    chunks = DirectCast(CObj(Me), doubles).Array _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .ToArray
                Case CDFDataTypes.NC_FLOAT
                    chunks = DirectCast(CObj(Me), floats).Array _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .ToArray
                Case CDFDataTypes.NC_INT
                    chunks = DirectCast(CObj(Me), integers).Array _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .ToArray
                Case CDFDataTypes.NC_SHORT
                    chunks = DirectCast(CObj(Me), shorts).Array _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .ToArray
                Case CDFDataTypes.NC_INT64
                    chunks = DirectCast(CObj(Me), longs).Array _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .ToArray
                Case Else
                    Throw New NotImplementedException(cdfDataType.Description)
            End Select

            If BitConverter.IsLittleEndian Then
                Return chunks _
                    .Select(Function(c)
                                System.Array.Reverse(c)
                                Return c
                            End Function) _
                    .IteratesALL _
                    .ToArray
            Else
                Return chunks.IteratesALL.ToArray
            End If
        End Function
    End Class
End Namespace
