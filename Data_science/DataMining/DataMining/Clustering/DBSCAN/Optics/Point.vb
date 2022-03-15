#Region "Microsoft.VisualBasic::11353639adb2b70b8bf0142f16a56d47, sciBASIC#\Data_science\DataMining\DataMining\Clustering\DBSCAN\Optics\Point.vb"

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

    '   Total Lines: 13
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 329.00 B


    '     Class Point
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.My.JavaScript

Namespace DBSCAN

    Public Class Point
        Public processed As Boolean = False
        Public reachabilityDistance As Double
        Public attribute As JavaScriptObject
        Public id As String
        Public color As Color
    End Class
End Namespace
