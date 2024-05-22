#Region "Microsoft.VisualBasic::78ae0e9c997ea30f3d869721f0811da8, Data_science\MachineLearning\MachineLearning\SVM\Scaling.vb"

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

    '   Total Lines: 60
    '    Code Lines: 25 (41.67%)
    ' Comment Lines: 27 (45.00%)
    '    - Xml Docs: 40.74%
    ' 
    '   Blank Lines: 8 (13.33%)
    '     File Size: 2.35 KB


    '     Module Scaling
    ' 
    '         Function: Scale
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder

Namespace SVM

    ''' <summary>
    ''' Deals with the scaling of Problems so they have uniform ranges across all dimensions in order to
    ''' result in better SVM performance.
    ''' </summary>
    Public Module Scaling

        ''' <summary>
        ''' Scales a problem using the provided range.  
        ''' This will not affect the parameter.
        ''' </summary>
        ''' <param name="prob">The problem to scale</param>
        ''' <param name="range">The Range transform to use in scaling</param>
        ''' <returns>The Scaled problem</returns>
        <Extension()>
        Public Function Scale(range As IRangeTransform, prob As Problem) As Problem
            Dim scaledProblem As New Problem(
                y:={},
                x:=New Node(prob.count - 1)() {},
                maxIndex:=prob.maxIndex
            ) With {
                .dimensionNames = prob.dimensionNames,
                .Y = New ColorClass(prob.count - 1) {}
            }

            For i = 0 To scaledProblem.count - 1
                scaledProblem.X(i) = New Node(prob.X(i).Length - 1) {}

                For j = 0 To scaledProblem.X(i).Length - 1
                    scaledProblem.X(i)(j) = New Node(prob.X(i)(j).index, range.Transform(prob.X(i)(j).value, prob.X(i)(j).index))
                Next

                scaledProblem.Y(i) = prob.Y(i)
            Next

            Return scaledProblem
        End Function
    End Module
End Namespace
