#Region "Microsoft.VisualBasic::df0bdbd4c52cd76070e1d42317b31620, gr\network-visualization\NetworkEditor\Layout\CircularRunner.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20 (74.07%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (25.93%)
    '     File Size: 934 B


    '     Class CircularRunner
    ' 
    '         Properties: Name
    ' 
    '         Function: GetParameters
    ' 
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Circular

Namespace NetworkEditor.Layout

    Public Class CircularRunner : Implements ILayoutRunner

        Private params As New CircularLayoutParameters

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Circular"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, CircularLayoutParameters)
            params = par
            Call CircularLayout.LayoutNodes(g, par)
        End Sub
    End Class

End Namespace

