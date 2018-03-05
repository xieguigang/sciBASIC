#Region "Microsoft.VisualBasic::7e5ec03b76d64c817ab6905b5eefed61, Data\BinaryData\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: IOtest, Main, reflectionTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Module Module1

    Sub Main()

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

            Dim write = writer.CreateWriter(Of int)

            p1 = write(New int(-9876))
            p2 = write(New int(88888888))

        End Using

        Dim reader As New StringReader(path)
        Dim read = reader.CreateReader(Of int)

        Console.WriteLine(read(0))
        Console.WriteLine(read(p1))

        Pause()
    End Sub
End Module
