#Region "Microsoft.VisualBasic::059c951ca286d9afcfaed66a4aa5c35f, Microsoft.VisualBasic.Core\test\test\Module1.vb"

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

    '   Total Lines: 33
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 780 B


    ' Module Module1
    ' 
    '     Sub: iniTest, Main1, mytest2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.My.JavaScript.ES6

Module Module1

    Sub mytest2()
        Dim map As New Map





    End Sub

    Sub Main1()
        Call iniTest()
    End Sub

    Sub iniTest()
        Using ini As New IniFile("./dddd.inf")
            Call ini.WriteValue("AAAA", "msg", "hello world", "what")
            Call ini.WriteValue("BBBB", 123, 9999, "op")
            Call ini.WriteValue("AAAA", "msg22222", "no!!!")
            Call ini.WriteValue("AAAA", "date", Now.ToString, "is now!")

            Call ini.WriteComment("BBBB", "HHHHHHHHHHHHHHHHHHHHHHHHHHHHW")
        End Using

        Dim debugView = New IniFile("./dddd.inf")

        Pause()
    End Sub
End Module
