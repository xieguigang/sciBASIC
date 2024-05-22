#Region "Microsoft.VisualBasic::fc1e3f9f210ba67cb8e81f458cd16716, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\BufferData\ZipDataPipe.vb"

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

    '   Total Lines: 118
    '    Code Lines: 77 (65.25%)
    ' Comment Lines: 25 (21.19%)
    '    - Xml Docs: 88.00%
    ' 
    '   Blank Lines: 16 (13.56%)
    '     File Size: 4.04 KB


    '     Class ZipDataPipe
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: GetBlocks, Read, TestBufferMagic, UncompressBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel

    ''' <summary>
    ''' compress the in-memory buffer data
    ''' </summary>
    Public Class ZipDataPipe : Inherits DataPipe

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Byte))
            Call MyBase.New(data)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(str As String)
            Call MyBase.New(str)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Double))
            Call MyBase.New(data)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As RawStream)
            Call MyBase.New(data.Serialize)
        End Sub

        ''' <summary>
        ''' extract all bytes data from the input <see cref="MemoryStream"/> for construct a new data package
        ''' </summary>
        ''' <param name="data"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As MemoryStream)
            Call MyBase.New(data.ToArray)
        End Sub

        ''' <summary>
        ''' get data in zip-compressed stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' just returns one block of the data, this function works based on the <see cref="Read"/> function.
        ''' </remarks>
        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Yield Read()
        End Function

        Const o1 As Byte = 8
        Const o2 As Byte = 9
        Const o3 As Byte = 1
        Const o4 As Byte = 0
        Const o5 As Byte = 0
        Const o6 As Byte = 2

        ''' <summary>
        ''' get data in zip-compressed stream
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Read() As Byte()
            Using s As New MemoryStream(data)
                Dim zip As Byte() = ZipStreamExtensions.Zip(s).ToArray
                Dim wrap As Byte() = New Byte(zip.Length + 6 - 1) {}

                ' needs wrapping around additional magic bytes for
                ' avoid confused the stream with the normal zip stream
                wrap(0) = o1
                wrap(1) = o2
                wrap(2) = o3
                wrap(3) = o4
                wrap(4) = o5
                wrap(5) = o6

                Array.ConstrainedCopy(zip, Scan0, wrap, 6, zip.Length)

                Return wrap
            End Using
        End Function

        Public Shared Function TestBufferMagic(wrap As Byte()) As Boolean
            If wrap.IsNullOrEmpty OrElse wrap.Length < 8 Then
                Return False
            Else
                Return wrap(0) = o1 AndAlso ' wrapper magic
                    wrap(1) = o2 AndAlso
                    wrap(2) = o3 AndAlso
                    wrap(3) = o4 AndAlso
                    wrap(4) = o5 AndAlso
                    wrap(5) = o6 AndAlso
                    wrap(6) = 120 AndAlso ' zip magic
                    wrap(7) = 218
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="wrap">the zip data should has the magic header</param>
        ''' <returns></returns>
        Public Shared Function UncompressBuffer(wrap As Byte()) As Byte()
            Dim zip As Byte() = New Byte(wrap.Length - 6 - 1) {}
            Call Array.ConstrainedCopy(wrap, 6, zip, Scan0, zip.Length)
            Return ZipStreamExtensions _
                .UnZipStream(zip, noMagic:=False) _
                .ToArray
        End Function

    End Class
End Namespace
