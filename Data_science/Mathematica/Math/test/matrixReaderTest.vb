#Region "Microsoft.VisualBasic::bb5d0af101a651cd6d3a2b5c52cf0ef2, sciBASIC#\Data_science\Mathematica\Math\test\matrixReaderTest.vb"

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

    '   Total Lines: 14
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 446.00 B


    ' Module matrixReaderTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Math.DataFrame.MatrixMarket
Imports Microsoft.VisualBasic.My.FrameworkInternal

Module matrixReaderTest

    Sub Main()
        DoConfiguration.ConfigMemory(MemoryLoads.Heavy)

        Dim mtx = MTXFormat.ReadMatrix("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\DataFrame\MatrixMarket\west0655.mtx".Open(FileMode.Open, [readOnly]:=True))

        Pause()
    End Sub
End Module
