#Region "Microsoft.VisualBasic::35d325d3625c8644eac5a0c27067b109, Microsoft.VisualBasic.Core\src\Net\Tcp\IPTools\LAN.vb"

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

    '   Total Lines: 180
    '    Code Lines: 103
    ' Comment Lines: 68
    '   Blank Lines: 9
    '     File Size: 8.09 KB


    '     Module LANTools
    ' 
    '         Function: GetAllDevicesOnLAN, GetIPAddress, GetIpNetTable, GetMacAddress, IsMulticast
    '         Structure MIB_IPNETROW
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.Marshal

Namespace Net

    ''' <summary>
    ''' http://www.codeproject.com/Tips/358946/Retrieving-IP-and-MAC-addresses-for-a-LAN
    ''' </summary>
    ''' <code>
    ''' ' Get my PC IP address
    ''' Call Console.WriteLine("My IP : {0}", GetIPAddress())
    ''' 
    ''' ' Get My PC MAC address
    ''' Call Console.WriteLine("My MAC: {0}", GetMacAddress())
    ''' 
    ''' ' Get all devices on network
    ''' Dim all As Dictionary(Of IPAddress, PhysicalAddress) = GetAllDevicesOnLAN()
    ''' For Each kvp As KeyValuePair(Of IPAddress, PhysicalAddress) In all
    '''     Console.WriteLine("IP : {0}" &amp; vbLf &amp; " MAC {1}", kvp.Key, kvp.Value)
    ''' Next
    ''' </code>
    Public Module LANTools

        ''' <summary>
        ''' MIB_IPNETROW structure returned by GetIpNetTable
        ''' DO NOT MODIFY THIS STRUCTURE.
        ''' </summary>
        <StructLayout(LayoutKind.Sequential)>
        Private Structure MIB_IPNETROW
            <MarshalAs(UnmanagedType.U4)>
            Public dwIndex As Integer
            <MarshalAs(UnmanagedType.U4)>
            Public dwPhysAddrLen As Integer
            <MarshalAs(UnmanagedType.U1)>
            Public mac0 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac1 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac2 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac3 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac4 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac5 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac6 As Byte
            <MarshalAs(UnmanagedType.U1)>
            Public mac7 As Byte
            <MarshalAs(UnmanagedType.U4)>
            Public dwAddr As Integer
            <MarshalAs(UnmanagedType.U4)>
            Public dwType As Integer
        End Structure

        ''' <summary>
        ''' GetIpNetTable external method
        ''' </summary>
        ''' <param name="pIpNetTable"></param>
        ''' <param name="pdwSize"></param>
        ''' <param name="bOrder"></param>
        ''' <returns></returns>
        <DllImport("IpHlpApi.dll")>
        Private Function GetIpNetTable(pIpNetTable As IntPtr, <MarshalAs(UnmanagedType.U4)> ByRef pdwSize As Integer, bOrder As Boolean) As <MarshalAs(UnmanagedType.U4)> Integer
        End Function

        ''' <summary>
        ''' Error codes GetIpNetTable returns that we recognise
        ''' </summary>
        Const ERROR_INSUFFICIENT_BUFFER As Integer = 122

        ''' <summary>
        ''' Get the IP and MAC addresses of all known devices on the LAN
        ''' </summary>
        ''' <remarks>
        ''' 1) This table is not updated often - it can take some human-scale time 
        '''    to notice that a device has dropped off the network, or a new device
        '''    has connected.
        ''' 2) This discards non-local devices if they are found - these are multicast
        '''    and can be discarded by IP address range.
        ''' </remarks>
        ''' <returns></returns>
        Public Function GetAllDevicesOnLAN() As Dictionary(Of IPAddress, PhysicalAddress)
            Dim all As New Dictionary(Of IPAddress, PhysicalAddress)()
            ' Add this PC to the list...
            all.Add(GetIPAddress(), GetMacAddress())
            Dim spaceForNetTable As Integer = 0
            ' Get the space needed
            ' We do that by requesting the table, but not giving any space at all.
            ' The return value will tell us how much we actually need.
            GetIpNetTable(IntPtr.Zero, spaceForNetTable, False)
            ' Allocate the space
            ' We use a try-finally block to ensure release.
            Dim rawTable As IntPtr = IntPtr.Zero
            Try
                rawTable = AllocCoTaskMem(spaceForNetTable)
                ' Get the actual data
                Dim errorCode As Integer = GetIpNetTable(rawTable, spaceForNetTable, False)
                If errorCode <> 0 Then
                    ' Failed for some reason - can do no more here.
                    Throw New Exception(String.Format("Unable to retrieve network table. Error code {0}", errorCode))
                End If
                ' Get the rows count
                Dim rowsCount As Integer = ReadInt32(rawTable)
                Dim currentBuffer As New IntPtr(rawTable.ToInt64() + Marshal.SizeOf(GetType(Int32)))
                ' Convert the raw table to individual entries
                Dim rows As MIB_IPNETROW() = New MIB_IPNETROW(rowsCount - 1) {}
                For index As Integer = 0 To rowsCount - 1
                    rows(index) = CType(PtrToStructure(New IntPtr(currentBuffer.ToInt64() + (index * Marshal.SizeOf(GetType(MIB_IPNETROW)))), GetType(MIB_IPNETROW)), MIB_IPNETROW)
                Next
                ' Define the dummy entries list (we can discard these)
                Dim virtualMAC As New PhysicalAddress(New Byte() {0, 0, 0, 0, 0, 0})
                Dim broadcastMAC As New PhysicalAddress(New Byte() {255, 255, 255, 255, 255, 255})
                For Each row As MIB_IPNETROW In rows
                    Dim ip As New IPAddress(BitConverter.GetBytes(row.dwAddr))
                    Dim rawMAC As Byte() = New Byte() {row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5}
                    Dim pa As New PhysicalAddress(rawMAC)
                    If Not pa.Equals(virtualMAC) AndAlso Not pa.Equals(broadcastMAC) AndAlso Not IsMulticast(ip) Then
                        'Console.WriteLine("IP: {0}\t\tMAC: {1}", ip.ToString(), pa.ToString());
                        If Not all.ContainsKey(ip) Then
                            all.Add(ip, pa)
                        End If
                    End If
                Next
            Finally
                ' Release the memory.
                FreeCoTaskMem(rawTable)
            End Try
            Return all
        End Function

        ''' <summary>
        ''' Gets the IP address of the current PC
        ''' </summary>
        ''' <returns></returns>
        Public Function GetIPAddress() As IPAddress
            Dim strHostName As String = Dns.GetHostName()
            Dim ipEntry As IPHostEntry = Dns.GetHostEntry(strHostName)
            Dim addr As IPAddress() = ipEntry.AddressList
            For Each ip As IPAddress In addr
                If Not ip.IsIPv6LinkLocal Then
                    Return (ip)
                End If
            Next
            Return If(addr.Length > 0, addr(0), Nothing)
        End Function

        ''' <summary>
        ''' Gets the MAC address of the current PC.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetMacAddress() As PhysicalAddress
            For Each nic As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
                ' Only consider Ethernet network interfaces
                If nic.NetworkInterfaceType = NetworkInterfaceType.Ethernet AndAlso nic.OperationalStatus = OperationalStatus.Up Then
                    Return nic.GetPhysicalAddress()
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns true if the specified IP address is a multicast address
        ''' </summary>
        ''' <param name="ip"></param>
        ''' <returns></returns>
        Public Function IsMulticast(ip As IPAddress) As Boolean
            Dim result As Boolean = True
            If Not ip.IsIPv6Multicast Then
                Dim highIP As Byte = ip.GetAddressBytes()(0)
                If highIP < 224 OrElse highIP > 239 Then
                    result = False
                End If
            End If
            Return result
        End Function
    End Module
End Namespace
