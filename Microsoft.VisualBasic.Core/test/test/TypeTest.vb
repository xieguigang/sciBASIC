#Region "Microsoft.VisualBasic::184ff26ae6041bdd7436707be88407dd, Microsoft.VisualBasic.Core\test\test\TypeTest.vb"

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

    '   Total Lines: 100
    '    Code Lines: 72
    ' Comment Lines: 0
    '   Blank Lines: 28
    '     File Size: 2.89 KB


    ' Enum FlagCombos
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Module TypeTest
    ' 
    '     Function: CharArray
    ' 
    '     Sub: descriptionTest, Main1, patternMatch, test
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Enum FlagCombos

    A = 2
    B = 4
    <Description("The last one")> C = 8

End Enum

Module TypeTest

    Sub descriptionTest()

        Call Console.WriteLine(FlagCombos.A.Description)
        Call Console.WriteLine(FlagCombos.B.Description)
        Call Console.WriteLine(FlagCombos.C.Description)

        Call Console.WriteLine((FlagCombos.A Or FlagCombos.B Or FlagCombos.C).Description)

        Pause()
    End Sub

    Sub Main1()
        Call descriptionTest()
        Call patternMatch()


        Call test()

        Dim o As [Variant](Of String, Integer(), Char())

        o = "string"

        Console.WriteLine(CStr(o))
        Console.WriteLine(o Like GetType(String))
        Console.WriteLine(o Like GetType(Integer()))
        Console.WriteLine(o Like GetType(Char()))

        o = {1, 2, 1231, 31, 23, 12}
        Console.WriteLine(CType(o, Integer()).GetJson)
        Console.WriteLine(o Like GetType(String))
        Console.WriteLine(o Like GetType(Integer()))
        Console.WriteLine(o Like GetType(Char()))

        o = {"f"c, "s"c, "d"c, "f"c, "s"c, "d"c, "f"c, "s"c, "d"c, "f"c, "s"c, "d"c, "f"c}
        Console.WriteLine(CType(o, Char()).GetJson)
        Console.WriteLine(o Like GetType(String))
        Console.WriteLine(o Like GetType(Integer()))
        Console.WriteLine(o Like GetType(Char()))

        Pause()
    End Sub

    Sub patternMatch()
        Dim src As [Variant](Of String(), Integer(), Byte()) = {23, 4, 2342, 42}

        Select Case src
            Case src Like GetType(String())
                Console.WriteLine("is string")
            Case src Like GetType(Integer())
                Console.WriteLine("is integer")
            Case src Like GetType(Byte())
                Console.WriteLine("is bytes")
            Case Else
                Throw New Exception
        End Select

        Pause()
    End Sub

    Sub test()

        Dim chars As [Variant](Of Char(), Integer())

        chars = CharArray("Hello world!", False)

        Console.WriteLine(CType(chars, Char()).GetJson)
        Console.WriteLine(chars Like GetType(Integer()))
        Console.WriteLine(chars Like GetType(Char()))

        chars = CharArray("Hello world!", True)

        Console.WriteLine(CType(chars, Integer()).GetJson)
        Console.WriteLine(chars Like GetType(Integer()))
        Console.WriteLine(chars Like GetType(Char()))

        Pause()
    End Sub

    Public Function CharArray(s As String, ascii As Boolean) As [Variant](Of Char(), Integer())
        If ascii Then
            Return s.Select(Function(c) AscW(c)).ToArray
        Else
            Return s.ToArray
        End If
    End Function
End Module
