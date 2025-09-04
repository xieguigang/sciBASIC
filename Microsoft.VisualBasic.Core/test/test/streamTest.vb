#Region "Microsoft.VisualBasic::dd97186ced904da0bec33fd8bd7f7fa4, Microsoft.VisualBasic.Core\test\test\streamTest.vb"

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

    '   Total Lines: 82
    '    Code Lines: 55 (67.07%)
    ' Comment Lines: 5 (6.10%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (26.83%)
    '     File Size: 2.24 KB


    ' Module streamTest
    ' 
    '     Sub: Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Net.Http

Module streamTest

    Sub dataUriStreamtest()
        Dim data As String = "Z:\New Text Document.txt".ReadAllText
        Dim uri As DataURI = DataURI.URIParser(data)

        Call uri.ToStream.FlushStream("Z:/aaa.png")

        Pause()
    End Sub

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

        If Not buf.SequenceEqual(helloWorld) Then
            Throw New InvalidProgramException
        End If

        Pause()
    End Sub
End Module
