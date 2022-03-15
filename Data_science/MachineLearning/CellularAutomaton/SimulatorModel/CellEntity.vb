#Region "Microsoft.VisualBasic::e406ac14d6d65a4eb4dd95457937f332, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\SimulatorModel\CellEntity.vb"

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
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 975.00 B


    ' Class CellEntity
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: getAdjacents
    ' 
    '     Sub: config, Tick
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.GraphTheory

Public Class CellEntity(Of T As Individual) : Inherits GridCell(Of T)

    Protected adjacents As CellEntity(Of T)()

    Sub New(i As Integer, j As Integer, obj As T)
        data = obj
        index = New Point(i, j)
    End Sub

    Friend Sub config(grid As Simulator(Of T))
        Me.adjacents = New CellEntity(Of T)(4 - 1) {}

        adjacents(0) = grid(index.X, index.Y - 1)
        adjacents(1) = grid(index.X, index.Y + 1)
        adjacents(2) = grid(index.X - 1, index.Y)
        adjacents(3) = grid(index.X + 1, index.Y)
    End Sub

    Private Iterator Function getAdjacents() As IEnumerable(Of Individual)
        For Each cell As CellEntity(Of T) In adjacents
            If Not cell Is Nothing Then
                Yield cell.data
            End If
        Next
    End Function

    Sub Tick()
        Call data.Tick(getAdjacents)
    End Sub
End Class
