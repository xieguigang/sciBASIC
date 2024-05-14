#Region "Microsoft.VisualBasic::c1b6405d1db6978c951801af46f93b40, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\Distance.vb"

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

    '   Total Lines: 57
    '    Code Lines: 30
    ' Comment Lines: 17
    '   Blank Lines: 10
    '     File Size: 1.99 KB


    '     Class Distance
    ' 
    '         Properties: Distance, NaN, Weight
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Clone, compareTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
'*****************************************************************************
' Copyright 2015 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace Hierarchy

    Public Class Distance : Implements IComparable(Of Distance), ICloneable

        Public Property Distance As Double
        Public Property Weight As Double

        Public ReadOnly Property NaN As Boolean
            Get
                Return Double.IsNaN(Distance)
            End Get
        End Property

        Public Sub New()
            Me.New(0.0)
        End Sub

        Public Sub New(distance As Double)
            Me.New(distance, 1.0)
        End Sub

        Public Sub New(distance As Double, weight As Double)
            Me.Distance = distance
            Me.Weight = weight
        End Sub

        Public Function compareTo(distance As Distance) As Integer Implements IComparable(Of Distance).CompareTo
            Return If(distance Is Nothing, 1, Me.Distance.CompareTo(distance.Distance))
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("distance : {0:F2}, weight : {1:F2}", Distance, Weight)
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Distance(Distance, Weight)
        End Function
    End Class
End Namespace
