#Region "Microsoft.VisualBasic::ab85ef7bf77c9d598a80c3525b1d4c13, vs_solutions\tutorials\core.test\LogFileTest.vb"

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

    ' Module LogFileTest
    ' 
    '     Sub: Main, PrintTest, WriteTes
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::8fc81cb896b9d27851d6b33e234c6ff9, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module LogFileTest
    ' 
    '     Sub: Main, PrintTest, WriteTes
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::32bf033d93b65934e75dc90194c7a96a, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module LogFileTest
    ' 
    '     Sub: Main, PrintTest, WriteTes
    ' 
    ' 
    ' 

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Module LogFileTest

    Sub Main()
        WriteTes()
        PrintTest()
    End Sub

    Const path$ = "./test.log"

    Sub WriteTes()
        Using log As New LogFile(path)
            Call log.writeline("123", "test")
        End Using
    End Sub

    Sub PrintTest()

    End Sub
End Module
