#Region "Microsoft.VisualBasic::eb9e83d2c613f022bc3196ca14cfa84c, vs_solutions\dev\LicenseMgr\LicenseMgr\LicenseInfo.vb"

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

    ' Module LicenseInfoExtensions
    ' 
    '     Properties: info
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development

Module LicenseInfoExtensions

    Dim __content As LicenseInfo

    Public Property info As LicenseInfo
        Get
            If __content Is Nothing Then
                __content = New LicenseInfo
            End If

            Return __content
        End Get
        Set(value As LicenseInfo)
            __content = value
        End Set
    End Property

End Module
