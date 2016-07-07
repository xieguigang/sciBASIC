#Region "Microsoft.VisualBasic::f1e33e0db01e39ee1a793c1ea437ad69, ..\Cli Tools\LicenseMgr\LicenseMgr\LicenseInfo.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
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

Module LicenseInfo

    Dim __content As SoftwareToolkits.LicenseInfo

    Public Property info As SoftwareToolkits.LicenseInfo
        Get
            If __content Is Nothing Then
                __content = New SoftwareToolkits.LicenseInfo
            End If

            Return __content
        End Get
        Set(value As SoftwareToolkits.LicenseInfo)
            __content = value
        End Set
    End Property

End Module
