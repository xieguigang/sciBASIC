#Region "Microsoft.VisualBasic::08084fe861a49c3a13ff352c10a88663, sciBASIC#\Data\BinaryData\test\Module1.vb"

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

    '   Total Lines: 67
    '    Code Lines: 44
    ' Comment Lines: 0
    '   Blank Lines: 23
    '     File Size: 1.64 KB


    ' Module Module1
    ' 
    '     Sub: IOtest, Main, reflectionTest, writeTest2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub writeTest2()
        Using out = New BinaryDataWriter("./sssss.txt".Open)
            Dim buffer As New IO.MemoryStream(Encoding.UTF8.GetBytes("Hello world!!! 撒比大师大师框架汇顶科技安徽科技案发后看见爱上" & RandomASCIIString(8192) & "writerBuffer test success;"))

            Call out.Write(buffer)
        End Using

        Pause()
    End Sub

    Sub Main()

        Call writeTest2()

        Call reflectionTest()

        Call IOtest()

    End Sub

    Sub IOtest()
        Dim path = "./sdfsdfsdf.dat"

        Using write As New StringWriter(path)
            Call write.Append("633333333333336")
            Call write.Append({"ASDF", "12345", "00000"})
        End Using

        Dim reader As New StringReader(path)

        Console.WriteLine(reader.ReadString)
        Console.WriteLine(reader.ReadStringArray.GetJson)

        Pause()
    End Sub


    Sub reflectionTest()
        Dim path$ = "./fffffffffffffffffffff.dat"
        Dim p1$
        Dim p2%

        Using writer As New StringWriter(path)

            Dim write = writer.CreateWriter(Of i32)

            p1 = write(New i32(-9876))
            p2 = write(New i32(88888888))

        End Using

        Dim reader As New StringReader(path)
        Dim read = reader.CreateReader(Of i32)

        Console.WriteLine(read(0))
        Console.WriteLine(read(p1))

        Pause()
    End Sub
End Module
