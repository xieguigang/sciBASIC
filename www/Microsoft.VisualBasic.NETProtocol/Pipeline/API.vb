#Region "Microsoft.VisualBasic::297d5eb7cdac3a5525ad20a7976b5344, www\Microsoft.VisualBasic.NETProtocol\Pipeline\API.vb"

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

    '   Total Lines: 94
    '    Code Lines: 58 (61.70%)
    ' Comment Lines: 22 (23.40%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 14 (14.89%)
    '     File Size: 3.52 KB


    '     Module API
    ' 
    ' 
    '         Enum Protocols
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: Protocol
    ' 
    '     Function: Delete, IsRef, (+2 Overloads) TryGetValue, WriteData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Parallel.MMFProtocol
Imports Microsoft.VisualBasic.Serialization

Namespace MMFProtocol.Pipeline

    Public Module API

        Public Const PeplinePort As Integer = 5687

        Enum Protocols As Long
            Allocation = 4556122
            Destroy = -345639845
        End Enum

        Public ReadOnly Property Protocol As Long =
            New ProtocolAttribute(GetType(API.Protocols)).EntryPoint

        Public Function Delete(var As String, Optional port As Integer = API.PeplinePort) As Boolean
            Dim invoke As New TcpRequest("127.0.0.1", port)
            Dim action As New RequestStream(API.Protocol, Protocols.Destroy, var)
            Dim resp As RequestStream = invoke.SendMessage(action)
            Return resp.Protocol = HTTP_RFC.RFC_OK
        End Function

        ''' <summary>
        ''' 生成的映射位置为:  &lt;var>:&lt;ChunkSize>
        ''' </summary>
        ''' <param name="var"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function WriteData(var As String, value As RawStream, Optional port As Integer = API.PeplinePort) As Boolean
            Dim buf As Byte() = value.Serialize
            Dim chunkSize As Long = buf.Length
            Dim ref As String = $"{var}:{chunkSize}"
            Dim invoke As New TcpRequest("127.0.0.1", port)
            Dim action As New RequestStream(API.Protocol, Protocols.Allocation, ref)
            Dim resp As RequestStream = invoke.SendMessage(action)
            Dim writer As New MapStream.MSWriter(var, chunkSize)

            Call writer.WriteStream(buf)

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="var">$var or var</param>
        ''' <returns></returns>
        Public Function TryGetValue(var As String) As Byte()
            If var.First = "$"c Then
                var = Mid(var, 2)
            End If

            Dim tokens As String() = var.Split(":"c)
            Dim size As Long = Scripting.CTypeDynamic(Of Long)(tokens(1))
            Dim reader As New MapStream.MSIOReader(tokens(Scan0), Nothing, size)
            Dim buf As Byte() = reader.Read.byteData
            Return buf
        End Function

        ''' <summary>
        ''' 不存在的话会返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="var"></param>
        ''' <returns></returns>
        Public Function TryGetValue(Of T As RawStream)(var As String) As T
            Dim raw As Byte() = API.TryGetValue(var)
            If raw.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim obj As Object = Activator.CreateInstance(GetType(T), {raw})
            Dim x As T = DirectCast(obj, T)
            Return x
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="addr">$var:size</param>
        ''' <returns></returns>
        Public Function IsRef(addr As String) As Boolean
            Dim s As String = Regex.Match(addr, "\$.+?:\d+").Value
            Return String.Equals(addr, s)
        End Function
    End Module
End Namespace
