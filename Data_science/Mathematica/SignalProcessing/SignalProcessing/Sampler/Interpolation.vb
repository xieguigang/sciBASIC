#Region "Microsoft.VisualBasic::f54b7c5f5214d29dc4942e05e3722d96, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\Interpolation.vb"

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

    '   Total Lines: 39
    '    Code Lines: 31 (79.49%)
    ' Comment Lines: 3 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (12.82%)
    '     File Size: 1.61 KB


    ' Module Interpolation
    ' 
    '     Function: (+2 Overloads) BSpline
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Interpolation

''' <summary>
''' make the signal data interpolation
''' </summary>
Public Module Interpolation

    <Extension>
    Public Function BSpline(Of T As ITimeSignal)(source As IEnumerable(Of T),
                                                 Optional degree As Double = 2,
                                                 Optional res As Double = 5) As IEnumerable(Of TimeSignal)
        Dim points As PointF() = source _
            .SafeQuery _
            .Select(Function(ti)
                        Return New PointF(ti.time, ti.intensity)
                    End Function) _
            .ToArray
        Dim interpolate = B_Spline.Compute(points, degree, res).ToArray

        Return From i As PointF In interpolate Select New TimeSignal(i)
    End Function

    <Extension>
    Public Function BSpline(Of T As ITimeSignal)(source As IEnumerable(Of T), activator As Func(Of Single, Single, T), Optional degree As Double = 2, Optional res As Double = 5) As IEnumerable(Of T)
        Dim points As PointF() = source _
           .SafeQuery _
           .Select(Function(ti)
                       Return New PointF(ti.time, ti.intensity)
                   End Function) _
           .ToArray
        Dim interpolate = B_Spline.Compute(points, degree, res).ToArray

        Return From i As PointF In interpolate Select activator(i.X, i.Y)
    End Function
End Module

