Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices

Module streamTest

    Public Sub Main1()
        Dim demo As Byte() = New Byte(1024 * 5 + 6) {}
        Dim b As Byte = 0
        Dim test_file As String = $"{App.HOME}/test.dat"

        For i As Integer = 0 To demo.Length - 1
            demo(i) = b

            If b = 255 Then
                b = 0
            Else
                b += 1
            End If
        Next

        demo(1026) = 199
        demo(1027) = 1

        Dim helloWorld As Byte() = Encoding.ASCII.GetBytes("Hello World!!!")
        Dim offset As Integer = 2045

        For i As Integer = 0 To helloWorld.Length - 1
            demo(offset + i) = helloWorld(i)
        Next

        Call demo.FlushStream(test_file)

        Dim stream = MemoryStreamPool.FromFile(test_file, buffer_size:=1024)

        ' seek test
        stream.Position = 1025
        ' p++
        stream.ReadByte()

        ' test stream reader
        Dim b199_1026 As Byte = stream.ReadByte
        Dim b1_1027 As Byte = stream.ReadByte

        If b199_1026 <> 199 OrElse b1_1027 <> 1 Then
            Throw New InvalidProgramException
        End If

        ' test of across different blocks
        stream.Position = 1023

        Dim b1023 = stream.ReadByte
        Dim b1024 = stream.ReadByte
        Dim b1025 = stream.ReadByte
        Dim b1026 = stream.ReadByte
        Dim b1027 = stream.ReadByte

        If b1026 <> 199 OrElse b1027 <> 1 Then
            Throw New InvalidProgramException
        End If

        ' read region across different blocks
        stream.Position = offset

        Dim buf As Byte() = New Byte(helloWorld.Length - 1) {}
        Dim base As Integer = offset

        Call stream.Read(buf, Scan0, buf.Length)

        For i As Integer = 0 To buf.Length - 1
            Call Console.WriteLine($"[{base}] {buf(i)} {Encoding.ASCII.GetString({buf(i)})}")
            base += 1
        Next

        Call Console.WriteLine(Encoding.ASCII.GetString(buf))

        Pause()
    End Sub
End Module
