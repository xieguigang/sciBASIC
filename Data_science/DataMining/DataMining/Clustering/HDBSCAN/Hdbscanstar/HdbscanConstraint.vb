#Region "Microsoft.VisualBasic::872cc80b8510f66a5f569356ab37e92a, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Hdbscanstar\HdbscanConstraint.vb"

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

    '   Total Lines: 34
    '    Code Lines: 21 (61.76%)
    ' Comment Lines: 9 (26.47%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (11.76%)
    '     File Size: 1.25 KB


    '     Class HdbscanConstraint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetConstraintType, GetPointA, GetPointB
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HDBSCAN.Hdbscanstar
    ''' <summary>
    ''' A clustering constraint (either a must-link or cannot-link constraint between two points).
    ''' </summary>
    Public Class HdbscanConstraint
        Private ReadOnly _constraintType As HdbscanConstraintType
        Private ReadOnly _pointA As Integer
        Private ReadOnly _pointB As Integer

        ''' <summary>
        ''' Creates a new constraint.
        ''' </summary>
        ''' <param name="pointA">The first point involved in the constraint</param>
        ''' <param name="pointB">The second point involved in the constraint</param>
        ''' <param name="type">The constraint type</param>
        Public Sub New(pointA As Integer, pointB As Integer, type As HdbscanConstraintType)
            _pointA = pointA
            _pointB = pointB
            _constraintType = type
        End Sub

        Public Function GetPointA() As Integer
            Return _pointA
        End Function

        Public Function GetPointB() As Integer
            Return _pointB
        End Function

        Public Function GetConstraintType() As HdbscanConstraintType
            Return _constraintType
        End Function
    End Class
End Namespace
