#Region "Microsoft.VisualBasic::b85dac381fcfc20cac61b03ac478292c, sciBASIC#\tutorials\core.test\unitTest.vb"

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

    '   Total Lines: 18
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 568.00 B


    ' Module unitTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges

Module unitTest

    Sub Main()
        Dim KB As New UnitValue(Of ByteSize)(128 * 1024 * 1024, ByteSize.KB)

        Call KB.__DEBUG_ECHO
        Call KB.ScaleTo(ByteSize.GB).__DEBUG_ECHO
        Call KB.ScaleTo(ByteSize.B).__DEBUG_ECHO
        Call KB.ScaleTo(ByteSize.MB).__DEBUG_ECHO
        Call KB.ScaleTo(ByteSize.TB).__DEBUG_ECHO
        Call KB.ScaleTo(ByteSize.KB).__DEBUG_ECHO
        Call KB.ScaleTo(ByteSize.TB).ScaleTo(ByteSize.MB).__DEBUG_ECHO

        Pause()
    End Sub
End Module
