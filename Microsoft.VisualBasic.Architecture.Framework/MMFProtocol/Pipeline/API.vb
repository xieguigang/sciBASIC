Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace MMFProtocol.Pipeline

    Public Module API

        Public Const PeplinePort As Integer = 5687

        Enum Protocols As Long
            Allocation = 4556122
            Destroy = -345639845
        End Enum

        Public ReadOnly Property Protocol As Long =
            New Protocol(GetType(API.Protocols)).EntryPoint

        Public Function Delete(var As String, Optional port As Integer = API.PeplinePort) As Boolean
            Dim invoke As New Net.AsynInvoke("127.0.0.1", port)
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
            Dim invoke As New Net.AsynInvoke("127.0.0.1", port)
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