#Region "Microsoft.VisualBasic::d7611750539f844002f3ed828cb69583, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\PerformanceCounter.vb"

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

    '   Total Lines: 68
    '    Code Lines: 47
    ' Comment Lines: 8
    '   Blank Lines: 13
    '     File Size: 2.08 KB


    '     Class PerformanceCounter
    ' 
    '         Properties: Top
    ' 
    '         Function: [Set], GetCounters, Mark, ToString
    ' 
    '     Class TimeCounter
    ' 
    '         Properties: span0, span1, start, task
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices

    Public Class PerformanceCounter

        Dim t0 As Date = Now
        Dim spans As New List(Of TimeCounter)
        Dim checkpoint As Date

        Public ReadOnly Property Top As TimeCounter()
            Get
                Return spans.OrderByDescending(Function(t) t.span1).ToArray
            End Get
        End Property

        ''' <summary>
        ''' reset the counter
        ''' </summary>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Function [Set]() As PerformanceCounter
            t0 = Now
            checkpoint = Now
            spans.Clear()
            Return Me
        End Function

        ''' <summary>
        ''' create a checkpoint
        ''' </summary>
        ''' <param name="title"></param>
        Public Function Mark(title As String) As TimeCounter
            Dim _checkpoint As New TimeCounter With {
                .task = title,
                .start = checkpoint,
                .span0 = Now - t0,
                .span1 = Now - checkpoint
            }
            spans.Add(_checkpoint)
            checkpoint = Now

            Return _checkpoint
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCounters() As IEnumerable(Of TimeCounter)
            Return spans.AsEnumerable
        End Function

        Public Overrides Function ToString() As String
            Return $"{spans.Count} samples, total time {StringFormats.ReadableElapsedTime(spans.Last.span0.TotalMilliseconds)}"
        End Function
    End Class

    Public Class TimeCounter

        Public Property task As String
        Public Property start As Date
        Public Property span0 As TimeSpan
        Public Property span1 As TimeSpan

        Public Overrides Function ToString() As String
            Return $"[{task}]{vbTab}{StringFormats.Lanudry(span1)} @ {start.ToString}"
        End Function

    End Class
End Namespace
