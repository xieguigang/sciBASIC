#Region "Microsoft.VisualBasic::242f52d5f0b66433dd7450d36bd67186, gr\network-visualization\NetworkEditor\Layout\ForceDirectedRunner.vb"

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

    '   Total Lines: 38
    '    Code Lines: 28 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (26.32%)
    '     File Size: 1.38 KB


    '     Class ForceDirectedRunner
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
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.ForceDirected

Namespace NetworkEditor.Layout

    Public Class ForceDirectedRunner : Implements ILayoutRunner

        Private params As New ForceDirectedParameters

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Force-Directed"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, ForceDirectedParameters)
            params = par

            Call LayoutHelper.EnsurePositions(g, par.CanvasWidth, par.CanvasHeight)

            Dim planner As New Planner(g, par)

            For i As Integer = 1 To par.Iterations
                Call planner.Collide()
                If i Mod 50 = 0 AndAlso progress IsNot Nothing Then
                    progress.Invoke($"Force-Directed {i}/{par.Iterations}")
                End If
            Next
        End Sub
    End Class

End Namespace

