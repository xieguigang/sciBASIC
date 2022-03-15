#Region "Microsoft.VisualBasic::cea5cbb45b44fe85ea4c456f18c96230, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\EdgeBundling\Mingle\Options.vb"

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

    '   Total Lines: 11
    '    Code Lines: 7
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 266.00 B


    '     Class Options
    ' 
    '         Properties: angleStrength, sort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts.EdgeBundling.Mingle

    Public Class Options

        Public Property angleStrength As Double?
        Public Property sort As Func(Of Node, Double)

    End Class
End Namespace
