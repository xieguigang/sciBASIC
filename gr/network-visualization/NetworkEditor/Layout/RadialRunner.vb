#Region "Microsoft.VisualBasic::ea430cd2daf234927444bdb269c253bd, gr\network-visualization\NetworkEditor\Layout\RadialRunner.vb"

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

    '   Total Lines: 29
    '    Code Lines: 21 (72.41%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (27.59%)
    '     File Size: 986 B


    '     Class RadialRunner
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
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Radial

Namespace NetworkEditor.Layout

    Public Class RadialRunner : Implements ILayoutRunner

        Private params As New RadialLayoutParameters

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Radial"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, RadialLayoutParameters)
            params = par

            Call LayoutHelper.EnsurePositions(g, 1000, 1000)
            Call RadialLayout.LayoutNodes(g, par)
        End Sub
    End Class

End Namespace

