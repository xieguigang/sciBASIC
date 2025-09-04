#Region "Microsoft.VisualBasic::def46b661022a93b327232589e0f90b6, Microsoft.VisualBasic.Core\test\Program.vb"

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

    '   Total Lines: 21
    '    Code Lines: 16 (76.19%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (23.81%)
    '     File Size: 499 B


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Module Program
    Sub Main(args As String())
        Call streamTest.dataUriStreamtest()

        Call logprint()
        Call memoryTest.runTest()
        Call numberParserTest.Main1()

        Call logfiletest.readerTest()
        Call group_test.RunGroup()

        Call enumeratorTestProgram.Mai2n()
        Call terminalTest.Main1()

        Console.WriteLine("Hello World!")

        Call markdownDisplayTest.Main1()
        Call SIMDTest.Main1()
        Call streamTest.Main1()
    End Sub
End Module
