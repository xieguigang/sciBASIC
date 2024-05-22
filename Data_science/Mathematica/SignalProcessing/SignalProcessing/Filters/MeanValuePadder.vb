#Region "Microsoft.VisualBasic::00b6dcb01b1b2bc8f58e31a549ff11e0, Data_science\Mathematica\SignalProcessing\SignalProcessing\Filters\MeanValuePadder.vb"

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

    '   Total Lines: 155
    '    Code Lines: 73 (47.10%)
    ' Comment Lines: 71 (45.81%)
    '    - Xml Docs: 63.38%
    ' 
    '   Blank Lines: 11 (7.10%)
    '     File Size: 5.75 KB


    '     Class MeanValuePadder
    ' 
    '         Properties: PaddingLeft, PaddingRight, WindowLength
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

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
    ''' Pads data to left and/or right.:
    ''' 
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
    ''' <tt>0 </tt>, <tt>e</tt> is replaced with arithmetic mean of
    ''' <tt>data[l]..data[l + window_length/2 - 1]</tt> (left padding)</li>
    ''' <li>
    ''' <tt>r <i/> <data.length/></tt>, <tt>e</tt> is replaced with arithmetic mean of
    ''' <tt>data[r - window_length/2 + 1]..data[r]</tt> (right padding)</li>
    ''' </ul>
    ''' </para>
    ''' Example:
    ''' <para>
    ''' Given data: <tt>[0,0,0,1,2,1,3,1,2,4,0]</tt> result of applying
    ''' MeanValuePadder with <seealso cref="WindowLength"/> = 4 is:
    ''' <tt>[1.5,1.5,1.5,1,2,1,3,1,2,4,0]</tt> in case of {@link #isPaddingLeft()
    ''' left padding}; <tt>[0,0,0,1,2,1,3,1,2,4,3]</tt> in case of
    ''' <seealso cref="PaddingRight"/>;
    ''' </para>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class MeanValuePadder
        Implements Preprocessor

        Private m_windowLength As Integer

        ''' 
        ''' <param name="windowLength">
        '''            window length of filter which will be used to smooth data.
        '''            Padding will use half of {@code windowLength} length. In this
        '''            way padding will be suited to smoothing operation </param>
        Public Sub New(windowLength As Integer)
            If windowLength < 0 Then
                Throw New ArgumentException("windowLength < 0")
            End If
            m_windowLength = windowLength
        End Sub

        ''' 
        ''' <param name="windowLength">
        '''            window length of filter which will be used to smooth data.
        '''            Padding will use half of {@code windowLength} length. In this
        '''            way padding will be suited to smoothing operation </param>
        ''' <param name="paddingLeft">
        '''            enables or disables left padding </param>
        ''' <param name="paddingRight">
        '''            enables or disables left padding </param>
        Public Sub New(windowLength As Integer, paddingLeft As Boolean, paddingRight As Boolean)
            If windowLength < 0 Then
                Throw New ArgumentException("windowLength < 0")
            End If
            m_windowLength = windowLength
            _PaddingLeft = paddingLeft
            _PaddingRight = paddingRight
        End Sub

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            ' padding values with average of last (WINDOW_LENGTH / 2) points
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
                Dim avg As Double = 0
                Dim m As Integer = std.Min(l + m_windowLength / 2, n)
                For i = l To m - 1
                    avg += data(i)
                Next
                avg /= m - l
                For i = 0 To l - 1
                    data(i) = avg
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
                Dim avg As Double = 0
                Dim m As Integer = std.Min(m_windowLength / 2, r + 1)
                For i = 0 To m - 1
                    avg += data(r - i)
                Next
                avg /= m
                For i = r + 1 To n - 1
                    data(i) = avg
                Next
            End If
        End Sub

        ''' 
        ''' <returns> {@code windowLength} </returns>
        Public Overridable Property WindowLength As Integer
            Get
                Return m_windowLength
            End Get
            Set(value As Integer)
                If value < 0 Then
                    Throw New ArgumentException("windowLength < 0")
                End If
                m_windowLength = value
            End Set
        End Property

        ''' 
        ''' <returns> {@code paddingLeft} </returns>
        Public Overridable Property PaddingLeft As Boolean = True

        ''' 
        ''' <returns> {@code paddingRight} </returns>
        Public Overridable Property PaddingRight As Boolean = True

    End Class

End Namespace
