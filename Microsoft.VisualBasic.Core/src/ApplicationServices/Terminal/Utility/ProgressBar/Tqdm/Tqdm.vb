﻿#Region "Microsoft.VisualBasic::692659da3520170afecd29a1b37413d4, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\Tqdm\Tqdm.vb"

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

    '   Total Lines: 158
    '    Code Lines: 84 (53.16%)
    ' Comment Lines: 54 (34.18%)
    '    - Xml Docs: 98.15%
    ' 
    '   Blank Lines: 20 (12.66%)
    '     File Size: 8.61 KB


    '     Module TqdmWrapper
    ' 
    '         Function: InternalWrap, (+4 Overloads) Wrap
    '         Delegate Function
    ' 
    '             Function: Range, WrapStreamReader
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit

Namespace ApplicationServices.Terminal.ProgressBar.Tqdm

    ''' <summary>
    ''' The Tqdm class offers utility functions to wrap collections and enumerables with a ProgressBar, 
    ''' providing a simple and effective way to track and display progress in console applications for various iterative operations.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/shaltielshmid/TqdmSharp
    ''' </remarks>
    Public Module TqdmWrapper

        ''' <summary>
        ''' Wraps a collection with a progress bar for iteration, providing visual feedback on progress.
        ''' </summary>
        ''' <param name="collection">The collection to iterate over.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns> 
        Public Function Wrap(Of T)(collection As ICollection(Of T),
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            Dim __ As ProgressBar = Nothing
            Return Wrap(collection, __, width, printsPerSecond, useColor)
        End Function

        ''' <summary>
        ''' Wraps an enumerable with a specified total count with a progress bar for iteration, providing visual feedback on progress.
        ''' </summary>
        ''' <param name="enumerable">The enumerable to iterate over.</param>
        ''' <param name="total">The total number of expected items in the enumerable.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns>
        Public Function Wrap(Of T)(enumerable As IEnumerable(Of T), total As Integer,
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            Dim __ As ProgressBar = Nothing
            Return Wrap(enumerable, total, __, width, printsPerSecond, useColor)
        End Function

        ''' <summary>
        ''' Wraps a collection with a progress bar for iteration and provides the progress bar instance for external control (like custom labels).
        ''' </summary>
        ''' <param name="collection">The collection to iterate over.</param>
        ''' <param name="bar">The progress bar instance used for tracking.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns>
        ''' 
        <Extension>
        Public Function Wrap(Of T)(collection As ICollection(Of T), <Out> ByRef bar As ProgressBar,
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            If collection Is Nothing Then
                Return New T() {}
            Else
                Return Wrap(collection, collection.Count, bar, width, printsPerSecond, useColor)
            End If
        End Function

        ''' <summary>
        ''' Wraps an enumerable with a specified total count with a progress bar for iteration and provides the progress bar instance for external control (like custom labels).
        ''' </summary>
        ''' <param name="enumerable">The enumerable to iterate over.</param>
        ''' <param name="total">The total number of items in the enumerable.</param>
        ''' <param name="bar">The progress bar instance used for tracking.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns>
        Public Function Wrap(Of T)(enumerable As IEnumerable(Of T), total As Integer, <Out> ByRef bar As ProgressBar,
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            bar = New ProgressBar(total:=total, width:=width, printsPerSecond:=printsPerSecond, useColor:=useColor)
            Return InternalWrap(enumerable, total, bar)
        End Function

        Private Iterator Function InternalWrap(Of T)(enumerable As IEnumerable(Of T), total As Integer, bar As ProgressBar) As IEnumerable(Of T)
            Dim count = 0
            For Each item In enumerable
                bar.Progress(count, total)
                count += 1
                Yield item
            Next
            bar.Finish()
        End Function

        Public Delegate Function RequestStreamProgressLocation(Of T)(ByRef getOffset As Long, bar As ProgressBar) As T

        Public Iterator Function WrapStreamReader(Of T)(bytesOfStream As Long, request As RequestStreamProgressLocation(Of T)) As IEnumerable(Of T)
            Dim offset As Long = Scan0
            Dim page_unit As ByteSize = ByteSize.B

            If bytesOfStream > 2 * ByteSize.GB Then
                bytesOfStream /= ByteSize.KB
                page_unit = ByteSize.KB
            ElseIf bytesOfStream > 2 * 1024 * ByteSize.GB Then
                bytesOfStream /= ByteSize.MB
                page_unit = ByteSize.MB
            End If

            Dim bar As New ProgressBar(total:=bytesOfStream, printsPerSecond:=1) With {
                .UpdateDynamicConfigs = False,
                .FormatTaskCounter = Function(byte_pages)
                                         Return StringFormats.Lanudry(bytes:=byte_pages * page_unit)
                                     End Function
            }

            Do While offset < bytesOfStream
                bar.Progress(offset / page_unit, bytesOfStream)
                bar.SetLabel(StringFormats.Lanudry(offset / (bar.ElapsedSeconds + 1)) & "/s")

                Yield request(offset, bar)
            Loop

            Call bar.Finish()
        End Function

        ''' <summary>
        ''' Generates a sequence of integral numbers within a specified range.
        ''' </summary>
        ''' <param name="start">The value of the first integer in the sequence.</param>
        ''' <param name="count">The number of sequential integers to generate.</param>
        ''' <param name="bar"></param>
        ''' <param name="width"></param>
        ''' <param name="printsPerSecond"></param>
        ''' <param name="useColor"></param>
        ''' <returns>a range of sequential integral numbers.</returns>
        Public Function Range(start As Integer, count As Integer,
                              <Out>
                              Optional ByRef bar As ProgressBar = Nothing,
                              Optional width As Integer = 40,
                              Optional printsPerSecond As Integer = 10,
                              Optional useColor As Boolean = False) As IEnumerable(Of Integer)

            Return Enumerable.Range(start, count).ToArray.Wrap(bar,
                width:=width,
                printsPerSecond:=printsPerSecond,
                useColor:=useColor
            )
        End Function
    End Module
End Namespace