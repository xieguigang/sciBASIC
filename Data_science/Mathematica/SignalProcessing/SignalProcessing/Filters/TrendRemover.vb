#Region "Microsoft.VisualBasic::9eeb9d3f43d54f4b944371c6c85f07aa, Data_science\Mathematica\SignalProcessing\SignalProcessing\Filters\TrendRemover.vb"

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

    '   Total Lines: 63
    '    Code Lines: 24
    ' Comment Lines: 34
    '   Blank Lines: 5
    '     File Size: 2.29 KB


    '     Class TrendRemover
    ' 
    '         Sub: apply, retrend
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  Copyright [2009] [Marcin Rzeźnicki]
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
' http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' 

Namespace Filters
    ''' <summary>
    ''' De-trends data by setting straight line between the first and the last point
    ''' and subtracting it from data. Having applied filters to data you should
    ''' reverse detrending by using <seealso cref="TrendRemover.retrend"/>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class TrendRemover
        Implements Preprocessor

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            ' de-trend data so to avoid boundary distortion
            ' we will achieve this by setting straight line from end to beginning
            ' and subtracting it from the trend
            Dim n = data.Length
            If n <= 2 Then
                Return
            End If
            Dim y0 = data(0)
            Dim slope = (data(n - 1) - y0) / (n - 1)
            For x As Integer = 0 To n - 1
                data(x) -= slope * x + y0
            Next
        End Sub

        ''' <summary>
        ''' Reverses the effect of <seealso cref="apply"/> by modifying {@code
        ''' newData}
        ''' </summary>
        ''' <param name="newData">
        '''            processed data </param>
        ''' <param name="data">
        '''            original data </param>
        Public Overridable Sub retrend(newData As Double(), data As Double())
            Dim n = data.Length
            Dim y0 = data(0)
            Dim slope = (data(n - 1) - y0) / (n - 1)
            For x As Integer = 0 To n - 1
                newData(x) += slope * x + y0
            Next
        End Sub

    End Class

End Namespace
