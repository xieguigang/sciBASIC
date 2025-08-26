#Region "Microsoft.VisualBasic::318ed5029670885b6b78fad2dc21fc4e, Microsoft.VisualBasic.Core\test\test\logfiletest.vb"

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

'   Total Lines: 11
'    Code Lines: 7 (63.64%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 4 (36.36%)
'     File Size: 278 B


' Module logfiletest
' 
'     Sub: readerTest
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
