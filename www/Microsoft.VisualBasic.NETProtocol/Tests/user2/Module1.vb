#Region "Microsoft.VisualBasic::101ee968f9cdc4161724ea01d84d4ec0, ..\sciBASIC#\www\Microsoft.VisualBasic.NETProtocol\Tests\user2\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Module Module1

    Sub Main()
        Dim clint As New Microsoft.VisualBasic.Net.NETProtocol.InternetTime.SNTPClient("time.apple.com")

        Call clint.__DEBUG_ECHO


        clint.Connect(False)
        Call clint.__DEBUG_ECHO
        Pause()

        Dim user As New Microsoft.VisualBasic.Net.NETProtocol.User(New Microsoft.VisualBasic.Net.IPEndPoint("127.0.0.1", 6354), 12234)



        Pause()
    End Sub

End Module
