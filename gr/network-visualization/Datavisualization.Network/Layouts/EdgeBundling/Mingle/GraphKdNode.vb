#Region "Microsoft.VisualBasic::0576caf4a806d9d4496978dcd4bbff45, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\EdgeBundling\Mingle\GraphKdNode.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 459.00 B


    '     Class GraphKdNode
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts.EdgeBundling.Mingle

    Public Class GraphKdNode

        Friend x, y, z, w As Double
        Friend v As Node

        Sub New()
        End Sub

        Sub New(v As Node)
            Me.v = v
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{x}, {y}, {z}, {w}]"
        End Function

    End Class
End Namespace
