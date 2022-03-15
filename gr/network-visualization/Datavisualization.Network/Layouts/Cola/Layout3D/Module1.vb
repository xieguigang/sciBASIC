#Region "Microsoft.VisualBasic::6f80cab578cdaaf40a82d831cf378d70, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout3D\Module1.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 385.00 B


    '     Class IConstraint
    ' 
    '         Properties: axis, gap, left, right
    ' 
    '     Class LinkSepAccessor
    ' 
    '         Properties: axis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Layouts.Cola

    Public Class IConstraint
        Public Property axis As String
        Public Property left() As Double
        Public Property right() As Double
        Public Property gap() As Double
    End Class

    Public Class LinkSepAccessor(Of Link)
        Inherits LinkAccessor

        Public Property axis As String

    End Class
End Namespace
