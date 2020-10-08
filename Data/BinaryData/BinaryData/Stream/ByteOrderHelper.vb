Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Represents helper methods to handle data byte order.
''' </summary>
<HideModuleName>
Public Module ByteOrderHelper

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
