﻿#Region "Microsoft.VisualBasic::ea9469c038b3cd33fdf7903bf19d7089, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/SignalProcessing/SignalProcessing//Filters/ZeroEliminator.vb"

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

    '   Total Lines: 117
    '    Code Lines: 48
    ' Comment Lines: 59
    '   Blank Lines: 10
    '     File Size: 4.04 KB


    '     Class ZeroEliminator
    ' 
    '         Properties: AlignToLeft
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
    ''' Eliminates zeros from data - starting from the first non-zero element, ending
    ''' at the last non-zero element. More specifically:
    ''' <para>
    ''' <ul>
    ''' <li>
    ''' Let <tt>l</tt> be the index of the first non-zero element in data,</li>
    ''' <li>let <tt>r</tt> be the index of the last non-zero element in data</li>
    ''' </ul>
    ''' then for every element <tt>e</tt> which index is <tt>i</tt> such that:
    ''' <tt>l <i/> <r/></tt> and <tt>e == 0</tt>, <tt>e</tt> is replaced with element <tt>e'</tt>
    ''' with index <tt>j</tt> such that:
    ''' <ul>
    ''' <li><tt>l </> <i/></tt> and <tt>e' <> 0</tt> and for all indexes
    ''' <tt>k: j <k/> <i/></tt> - when <seealso cref="isAlignToLeft()"/>
    ''' is true</li>
    ''' <li><tt>i <j/> </></tt> and <tt>e' <> 0</tt> and for all indexes
    ''' <tt>k: i <k/> <j/></tt> - otherwise</li>
    ''' </ul>
    ''' </para>
    ''' Example:
    ''' <para>
    ''' Given data: <tt>[0,0,0,1,2,0,3,0,0,4,0]</tt> result of applying
    ''' ZeroEliminator is: <tt>[0,0,0,1,2,2,3,3,3,4,0]</tt> if
    ''' <seealso cref="isAlignToLeft()"/> is true;
    ''' <tt>[0,0,0,1,2,3,3,4,4,4,0]</tt> - otherwise
    ''' </para>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary></para></summary>   
    Public Class ZeroEliminator
        Implements Preprocessor

        Private alignToLeftField As Boolean

        ''' <summary>
        ''' Default constructor: {@code alignToLeft} is {@code false}
        ''' </summary>
        ''' <seealso cref=""/> </seealso>
        Public Sub New()

        End Sub

        ''' 
        ''' <param name="alignToLeft">
        '''            if {@code true} zeros will be replaced with non-zero element
        '''            to the left, if {@code false} - to the right </param>
        Public Sub New(alignToLeft As Boolean)
            alignToLeftField = alignToLeft
        End Sub

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            Dim n = data.Length
            Dim l = 0, r = 0
            ' seek first non-zero cell
            For i = 0 To n - 1
                If data(i) <> 0 Then
                    l = i
                    Exit For
                End If
            Next
            ' seek last non-zero cell
            For i = n - 1 To 0 Step -1
                If data(i) <> 0 Then
                    r = i
                    Exit For
                End If
            Next
            ' eliminate 0s
            If alignToLeftField Then
                For i = l + 1 To r - 1
                    If data(i) = 0 Then
                        data(i) = data(i - 1)
                    End If
                Next
            Else
                For i = r - 1 To l + 1 Step -1
                    If data(i) = 0 Then
                        data(i) = data(i + 1)
                    End If
                Next
            End If
        End Sub

        ''' 
        ''' <returns> {@code alignToLeft} </returns>
        Public Overridable Property AlignToLeft As Boolean
            Get
                Return alignToLeftField
            End Get
            Set(value As Boolean)
                alignToLeftField = value
            End Set
        End Property


    End Class

End Namespace
