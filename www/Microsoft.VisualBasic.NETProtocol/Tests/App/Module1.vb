﻿#Region "Microsoft.VisualBasic::e8a7d0a53186b9901589b24c2561fbf7, www\Microsoft.VisualBasic.NETProtocol\Tests\App\Module1.vb"

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

    '   Total Lines: 17
    '    Code Lines: 10 (58.82%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (41.18%)
    '     File Size: 405 B


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Module1

    Sub Main()

        Pause()

        Dim user As New Microsoft.VisualBasic.Net.NETProtocol.User(New Microsoft.VisualBasic.Net.IPEndPoint("127.0.0.1", 6354), 1234)
        AddHandler user.PushMessage, Sub(x)
                                         Call x.GetUTF8String.__DEBUG_ECHO
                                     End Sub


        Pause()

    End Sub

End Module
