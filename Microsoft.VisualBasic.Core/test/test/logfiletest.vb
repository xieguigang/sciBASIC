#Region "Microsoft.VisualBasic::a1f31e651ee1c0863715b2183d5ca933, Microsoft.VisualBasic.Core\test\test\logfiletest.vb"

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

    '   Total Lines: 26
    '    Code Lines: 20 (76.92%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (23.08%)
    '     File Size: 697 B


    ' Module logfiletest
    ' 
    '     Sub: download, logprint, readerTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Module logfiletest

    Sub logprint()
        Call "debug message is here".debug
        Call New Action(AddressOf download).benchmark
        Call "invalid data!".warning
        Call "hi, welcome".info
        Call "missing file for run startup!".error
        Call "new message recived".logging

        Pause()
    End Sub

    Sub download()
        Call Thread.Sleep(3.25 * 1000)
    End Sub

    Sub readerTest()
        Dim logs = LogReader.Parse("C:\Users\Administrator\AppData\Local\BioDeep\pipeline_calls_2024-02.txt").ToArray

        Pause()
    End Sub
End Module
