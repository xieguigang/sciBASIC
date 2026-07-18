#Region "Microsoft.VisualBasic::37c441ad5a1e6b28f62c3c3d7706b7b1, gr\network-visualization\NetworkEditor\Layout\SpringForceRunner.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (27.27%)
    '     File Size: 1.11 KB


    '     Class SpringForceRunner
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
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce

Namespace NetworkEditor.Layout

    Public Class SpringForceRunner : Implements ILayoutRunner

        Private params As ForceDirectedArgs = ForceDirectedArgs.DefaultNew()

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Spring Force"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, ForceDirectedArgs)
            params = par

            If g.CheckZero() Then
                Call g.doRandomLayout()
            End If

            Call g.doForceLayout(par)
        End Sub
    End Class

End Namespace

