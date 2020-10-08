#Region "Microsoft.VisualBasic::fc911ab3b71686af272382ccf4950067, Data\BinaryData\BinaryData\Stream\ByteOrder.vb"

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

' Enum ByteOrder
' 
' 
'  
' 
' 
' 
' Module ByteOrderHelper
' 
'     Properties: SystemByteOrder
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: AsNetworkByteOrderBuffer, NeedsReversion, networkByteOrderBigEndian, networkByteOrderLittleEndian
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Represents the possible endianness of binary data.
''' </summary>
Public Enum ByteOrder As UShort
    ''' <summary>
    ''' The binary data is present in big endian.
    ''' </summary>
    BigEndian = &HFEFF

    ''' <summary>
    ''' The binary data is present in little endian.
    ''' </summary>
    LittleEndian = &HFFFE
End Enum

''' <summary>
''' Represents helper methods to handle data byte order.
''' </summary>
<HideModuleName> Public Module ByteOrderHelper

    Dim _systemByteOrder As ByteOrder
    Dim _networkByteOrderConvertor As Func(Of Double, Byte())

    ''' <summary>
    ''' Gets the <see cref="ByteOrder"/> of the system executing the assembly.
    ''' </summary>
    Public ReadOnly Property SystemByteOrder() As ByteOrder
        Get
            If _systemByteOrder = 0 Then
                _systemByteOrder = If(BitConverter.IsLittleEndian, ByteOrder.LittleEndian, ByteOrder.BigEndian)
            End If
            Return _systemByteOrder
        End Get
    End Property

    Sub New()
        If BitConverter.IsLittleEndian Then
            _networkByteOrderConvertor = AddressOf networkByteOrderLittleEndian
        Else
            _networkByteOrderConvertor = AddressOf BitConverter.GetBytes
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function NeedsReversion(order As ByteOrder) As Boolean
        Return order <> ByteOrderHelper.SystemByteOrder
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBytes(d As Double) As Byte()
        Return _networkByteOrderConvertor(d)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsNetworkByteOrderBuffer(data As IEnumerable(Of Double)) As Byte()
        Return data _
            .Select(_networkByteOrderConvertor) _
            .IteratesALL _
            .ToArray
    End Function

    Private Function networkByteOrderLittleEndian(d As Double) As Byte()
        Dim chunk As Byte() = BitConverter.GetBytes(d)
        Call Array.Reverse(chunk)
        Return chunk
    End Function
End Module
