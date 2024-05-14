#Region "Microsoft.VisualBasic::5173c7c68a0a2f6f52c2aa0aa5affd85, Data_science\Mathematica\SignalProcessing\SignalProcessing\Filters\ContinuousPadder.vb"

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

    '   Total Lines: 112
    '    Code Lines: 41
    ' Comment Lines: 61
    '   Blank Lines: 10
    '     File Size: 3.85 KB


    '     Class ContinuousPadder
    ' 
    '         Properties: PaddingLeft, PaddingRight
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: apply
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
    ''' Pads data to left and/or right, starting from the first (last) non-zero cell
    ''' and extending it to the beginning (end) of data. More specifically:
    ''' <para>
    ''' <ul>
    ''' <li>
    ''' Let <tt>l</tt> be the index of the first non-zero element in data (for left
    ''' padding),</li>
    ''' <li>let <tt>r</tt> be the index of the last non-zero element in data (for
    ''' right padding)</li>
    ''' </ul>
    ''' then for every element <tt>e</tt> which index is <tt>i</tt> such that:
    ''' <ul>
    ''' <li>
    ''' <tt>0</tt>, <tt>e</tt> is replaced with element <tt>data[l]</tt>
    ''' (left padding)</li>
    ''' <li>
    ''' <tt>r <i/> <data.length/></tt>, <tt>e</tt> is replaced with element
    ''' <tt>data[r]</tt> (right padding)</li>
    ''' </ul>
    ''' </para>
    ''' Example:
    ''' <para>
    ''' Given data: <tt>[0,0,0,1,2,1,3,1,2,4,0]</tt> result of applying
    ''' ContinuousPadder is: <tt>[1,1,1,1,2,1,3,1,2,4,0]</tt> in case of
    ''' <seealso cref="PaddingLeft"/>; <tt>[0,0,0,1,2,1,3,1,2,4,4]</tt> in
    ''' case of <seealso cref="PaddingRight"/>;
    ''' </para>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class ContinuousPadder : Implements Preprocessor

        ''' 
        ''' <returns> {@code paddingLeft} </returns>
        Public Overridable Property PaddingLeft As Boolean = True
        ''' 
        ''' <returns> {@code paddingRight} </returns>
        Public Overridable Property PaddingRight As Boolean = True


        ''' <summary>
        ''' Default construcot. Both left and right padding are turned on
        ''' </summary>
        Public Sub New()

        End Sub

        ''' 
        ''' <param name="paddingLeft">
        '''            enables or disables left padding </param>
        ''' <param name="paddingRight">
        '''            enables or disables right padding </param>
        Public Sub New(paddingLeft As Boolean, paddingRight As Boolean)
            _PaddingLeft = paddingLeft
            _PaddingRight = paddingRight
        End Sub

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            Dim n = data.Length
            If PaddingLeft Then
                Dim l = 0
                ' seek first non-zero cell
                For i = 0 To n - 1
                    If data(i) <> 0 Then
                        l = i
                        Exit For
                    End If
                Next
                Dim y0 = data(l)
                For i = 0 To l - 1
                    data(i) = y0
                Next
            End If
            If PaddingRight Then
                Dim r = 0
                ' seek last non-zero cell
                For i = n - 1 To 0 Step -1
                    If data(i) <> 0 Then
                        r = i
                        Exit For
                    End If
                Next
                Dim ynr = data(r)
                For i = r + 1 To n - 1
                    data(i) = ynr
                Next
            End If
        End Sub

    End Class

End Namespace
