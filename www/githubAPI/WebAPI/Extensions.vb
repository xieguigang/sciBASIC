#Region "Microsoft.VisualBasic::10099b769f7bc7dad43ec17bc4cfca96, www\githubAPI\WebAPI\Extensions.vb"

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

    '   Total Lines: 32
    '    Code Lines: 22
    ' Comment Lines: 6
    '   Blank Lines: 4
    '     File Size: 1.10 KB


    '     Module Extensions
    ' 
    '         Function: GetAvatar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace WebAPI

    Public Module Extensions

        ''' <summary>
        ''' Download user avatar image or read from local cache
        ''' </summary>
        ''' <param name="user"></param>
        ''' <param name="noCache"></param>
        ''' <returns></returns>
        <Extension> Public Function GetAvatar(user As User, Optional noCache As Boolean = False) As Image
            Dim local$ = App.SysTemp & $"/Github/avatars/{user.login}.png"

            If noCache Then
                Call user.avatar_url.DownloadFile(save:=local)
                Return local.LoadImage
            Else
                If local.FileLength > 0 Then
                    Return local.LoadImage
                Else
                    Call user.avatar_url.DownloadFile(save:=local)
                    Return local.LoadImage
                End If
            End If
        End Function
    End Module
End Namespace
