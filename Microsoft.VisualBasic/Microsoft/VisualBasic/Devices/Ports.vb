Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO.Ports
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Ports
        ' Methods
        Public Function OpenSerialPort(portName As String) As SerialPort
            Dim port1 As New SerialPort(portName)
            port1.Open()
            Return port1
        End Function

        Public Function OpenSerialPort(portName As String, baudRate As Integer) As SerialPort
            Dim port1 As New SerialPort(portName, baudRate)
            port1.Open()
            Return port1
        End Function

        Public Function OpenSerialPort(portName As String, baudRate As Integer, parity As Parity) As SerialPort
            Dim port1 As New SerialPort(portName, baudRate, parity)
            port1.Open()
            Return port1
        End Function

        Public Function OpenSerialPort(portName As String, baudRate As Integer, parity As Parity, dataBits As Integer) As SerialPort
            Dim port1 As New SerialPort(portName, baudRate, parity, dataBits)
            port1.Open()
            Return port1
        End Function

        Public Function OpenSerialPort(portName As String, baudRate As Integer, parity As Parity, dataBits As Integer, stopBits As StopBits) As SerialPort
            Dim port1 As New SerialPort(portName, baudRate, parity, dataBits, stopBits)
            port1.Open()
            Return port1
        End Function


        ' Properties
        Public ReadOnly Property SerialPortNames As ReadOnlyCollection(Of String)
            Get
                Dim list As New List(Of String)
                Dim str As String
                For Each str In SerialPort.GetPortNames
                    list.Add(str)
                Next
                Return New ReadOnlyCollection(Of String)(list)
            End Get
        End Property

    End Class
End Namespace

