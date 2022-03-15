#Region "Microsoft.VisualBasic::791253ac0ae3855858015a1cc34e76ac, sciBASIC#\tutorials\core.test\LinuxRunTest\child\Module1.vb"

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

    '   Total Lines: 62
    '    Code Lines: 15
    ' Comment Lines: 29
    '   Blank Lines: 18
    '     File Size: 1.06 KB


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::79e000a08e00363a08f3a622b2d25fa1, ..\..\core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::bfa5e9bdb2a53100de723fd333a6c2c0, ..\..\core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' 
    ' 

#End Region

Imports Microsoft.VisualBasic.Language.UnixBash

Module Module1

    Sub Main()

        Call App.Command.__DEBUG_ECHO

        Dim ps1 As New PS1("[\u@\h \W \A #\#]\$ ")

        For i As Integer = 0 To 100
            Call $"{ps1.ToString}  ---> {i}%".__DEBUG_ECHO
            Call Threading.Thread.Sleep(300)
        Next
    End Sub

End Module
