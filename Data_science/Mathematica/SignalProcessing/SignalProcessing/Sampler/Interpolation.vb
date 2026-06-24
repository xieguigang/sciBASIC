#Region "Microsoft.VisualBasic::141e320ef8cc4649e7b200ecab9ec4c3, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\Interpolation.vb"

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

    '   Total Lines: 50
    '    Code Lines: 31 (62.00%)
    ' Comment Lines: 14 (28.00%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 5 (10.00%)
    '     File Size: 1.95 KB


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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="activator">
    ''' cast data to the <typeparamref name="T"/> object
    ''' </param>
    ''' <param name="degree"></param>
    ''' <param name="res"></param>
    ''' <returns></returns>
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
