#Region "Microsoft.VisualBasic::221dea0227f74273b85f02afa7c50459, Data\BinaryData\BinaryData\Stream\ByteOrder.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



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
    '     Function: NeedsReversion
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function NeedsReversion(order As ByteOrder) As Boolean
        Return order <> ByteOrderHelper.SystemByteOrder
    End Function
End Module
