#Region "Microsoft.VisualBasic::d4a46639c3d7423d580cc2994e9b2995, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\Tqdm\Tqdm.vb"

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

'   Total Lines: 197
'    Code Lines: 108 (54.82%)
' Comment Lines: 64 (32.49%)
'    - Xml Docs: 82.81%
' 
'   Blank Lines: 25 (12.69%)
'     File Size: 10.43 KB


'     Module TqdmWrapper
' 
'         Function: InternalWrap, (+5 Overloads) Wrap
'         Delegate Function
' 
'             Function: Range, WrapStreamReader
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
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
                                   Optional useColor As Boolean = False,
                                   Optional wrap_console As Boolean = True) As IEnumerable(Of T)

            Dim __ As ProgressBar = Nothing
            Return Wrap(collection, __, width, printsPerSecond, useColor,
                        wrap_console:=wrap_console)
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
                                   Optional useColor As Boolean = False,
                                   Optional wrap_console As Boolean = True) As IEnumerable(Of T)

            Dim __ As ProgressBar = Nothing
            Return Wrap(enumerable, total, __, width, printsPerSecond, useColor,
                        wrap_console:=wrap_console)
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
                                   Optional useColor As Boolean = False,
                                   Optional wrap_console As Boolean = True) As IEnumerable(Of T)

            If collection Is Nothing Then
                Return New T() {}
            Else
                Return Wrap(collection, collection.Count, bar, width, printsPerSecond, useColor,
                            wrap_console:=wrap_console)
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
                                   Optional useColor As Boolean = False,
                                   Optional wrap_console As Boolean = True) As IEnumerable(Of T)

            ' 20241225 set this parameter to FALSE in winform application
            ' for avoid call the console related function 
            ' which could be crashed the application in non-console application
            If Not wrap_console Then
                Return enumerable
            End If

            bar = New ProgressBar(total:=total, width:=width, printsPerSecond:=printsPerSecond, useColor:=useColor)
            Return InternalWrap(enumerable, total, bar)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Wrap(total As Integer,
                             Optional width As Integer = 40,
                             Optional printsPerSecond As Integer = 10,
                             Optional useColor As Boolean = False) As ProgressBar

            Return New ProgressBar(
                total,
                width:=width,
                printsPerSecond:=printsPerSecond,
                useColor:=useColor
            )
        End Function

        Private Iterator Function InternalWrap(Of T)(enumerable As IEnumerable(Of T), total As Integer, bar As ProgressBar) As IEnumerable(Of T)
            Dim count As Integer = 0

            For Each item As T In enumerable
                bar.Progress(count, total)
                count += 1
                Yield item
            Next

            Call bar.Finish()
        End Function

        Public Delegate Function RequestStreamProgressLocation(Of T)(ByRef getOffset As Long, bar As ProgressBar) As T

        Public Iterator Function WrapStreamReader(Of T)(bytesOfStream As Long, request As RequestStreamProgressLocation(Of T)) As IEnumerable(Of T)
            Dim offset As Long = Scan0
            Dim page_unit As ByteSize = ByteSize.B

            If bytesOfStream > 2 * 1024 * ByteSize.GB Then
                ' file size is more than 2TB
                bytesOfStream /= ByteSize.MB
                page_unit = ByteSize.MB
            ElseIf bytesOfStream > 2 * ByteSize.GB Then
                ' file size is more than 2GB
                bytesOfStream /= ByteSize.KB
                page_unit = ByteSize.KB
            End If

            Dim bar As New ProgressBar(total:=bytesOfStream, printsPerSecond:=1) With {
                .UpdateDynamicConfigs = False,
                .FormatTaskCounter = Function(byte_pages)
                                         Return StringFormats.Lanudry(bytes:=byte_pages * page_unit)
                                     End Function
            }

            ' 20240901
            '
            ' offset in unit bytes
            ' bytes of stream in different unit which is converted via page_unit
            ' so needs convert the offset in same page_unit factor scale
            Do While (offset / page_unit) < bytesOfStream
                Call bar.Progress(offset / page_unit, bytesOfStream)
                Call bar.SetLabel(StringFormats.Lanudry(offset / (bar.ElapsedSeconds + 1)) & "/s")

                Yield request(offset, bar)
            Loop

            Call bar.Finish()
        End Function

        Public Function StreamReader(str As StreamReader) As Func(Of String)
            Dim page_unit As ByteSize = ByteSize.B
            Dim bytesOfStream As Long = str.BaseStream.Length

            If bytesOfStream > 2 * 1024 * ByteSize.GB Then
                ' file size is more than 2TB
                bytesOfStream /= ByteSize.MB
                page_unit = ByteSize.MB
            ElseIf bytesOfStream > 2 * ByteSize.GB Then
                ' file size is more than 2GB
                bytesOfStream /= ByteSize.KB
                page_unit = ByteSize.KB
            End If

            Dim bar As New ProgressBar(total:=bytesOfStream, printsPerSecond:=1) With {
                .UpdateDynamicConfigs = False,
                .FormatTaskCounter = Function(byte_pages)
                                         Return StringFormats.Lanudry(bytes:=byte_pages * page_unit)
                                     End Function
            }

            Return Function() As String
                       If str.EndOfStream Then
                           Call bar.Finish()
                           Return Nothing
                       End If

                       Dim line As String = str.ReadLine
                       Dim offset As Long = str.BaseStream.Position

                       Call bar.Progress(offset / page_unit, bytesOfStream)
                       Call bar.SetLabel(StringFormats.Lanudry(offset / (bar.ElapsedSeconds + 1)) & "/s")

                       Return line
                   End Function
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function StreamReader(s As Stream) As Func(Of String)
            Return StreamReader(New StreamReader(s))
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
                              Optional useColor As Boolean = False,
                              Optional wrap_console As Boolean = True) As IEnumerable(Of Integer)

            Return Enumerable.Range(start, count).ToArray.Wrap(bar,
                width:=width,
                printsPerSecond:=printsPerSecond,
                useColor:=useColor,
                wrap_console:=wrap_console
            )
        End Function
    End Module
End Namespace
