#Region "Microsoft.VisualBasic::891f516ce3925fe4c76767849b38e842, ..\sciBASIC#\Data_science\SVM\SVM\view\State.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.DataMining.SVM.Model

Public Class State

    Friend ReadOnly outerInstance As CartesianCoordinateSystem

    Public Property Line As Line
    Public ReadOnly Property Points As List(Of LabeledPoint)

    Sub New(outerInstance As CartesianCoordinateSystem)
        Me.New(outerInstance, New List(Of LabeledPoint)(), Nothing)
    End Sub

    Sub New(outerInstance As CartesianCoordinateSystem, points As IList(Of LabeledPoint), line As Line)
        Me.outerInstance = outerInstance
        Me.Points = New List(Of LabeledPoint)(points.Count)

        For Each p As LabeledPoint In points
            Me.Points.Add(p.Clone())
        Next

        If line IsNot Nothing Then
            Me.Line = line.Clone()
        End If
    End Sub

    Sub AddPoint(point As LabeledPoint)
        Points.Add(point)
    End Sub

    Sub RemovePoint(point As LabeledPoint)
        Points.Remove(point)
    End Sub

    Sub ClearPoints()
        Points.Clear()
    End Sub

    Public Overrides Function Equals(o As Object) As Boolean
        If TypeOf o Is State Then

            Dim state As State = CType(o, State)

            If Not LabeledPoint.ListEqual(state.Points, Points) Then Return False
            If Me.Line Is Nothing AndAlso state.Line Is Nothing Then Return True
            If Me.Line IsNot Nothing AndAlso state.Line IsNot Nothing Then
                Return state.Line.Equals(Me.Line)
            End If

            Return False
        End If

        Return MyBase.Equals(o)
    End Function
End Class
