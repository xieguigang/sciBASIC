#Region "Microsoft.VisualBasic::cf5109d4e8b23dd0b855d79e680987f8, Microsoft.VisualBasic.Core\src\Text\Parser\CharStream.vb"

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

    '   Total Lines: 86
    '    Code Lines: 66 (76.74%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (23.26%)
    '     File Size: 2.65 KB


    '     Class CharStream
    ' 
    '         Properties: EndRead
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ReadNext, ReadNextC
    '         Operators: +, <>, =, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.Linq

Namespace Text.Parser

    Public Class CharStream

        ReadOnly str As StreamReader
        ReadOnly readLine As Func(Of String)

        Public ReadOnly Property EndRead As Boolean
            Get
                Return str.EndOfStream AndAlso buffer.EndRead
            End Get
        End Property

        Dim buffer As CharPtr

        Sub New()
        End Sub

        Sub New(s As StreamReader, Optional tqdm As Boolean = True)
            str = s

            If tqdm Then
                readLine = TqdmWrapper.StreamReader(s)
            Else
                readLine = AddressOf s.ReadLine
            End If
        End Sub

        Public Function ReadNext() As Char
            If buffer Is Nothing OrElse buffer.EndRead Then
                buffer = readLine()
            End If

            Return ++buffer
        End Function

        Private Function ReadNextC() As SeqValue(Of Char)
            If buffer Is Nothing OrElse buffer.EndRead Then
                buffer = readLine()
            End If

            Return +buffer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(chars As CharStream) As SeqValue(Of Char)
            Return chars.ReadNextC
        End Operator

        Public Shared Operator Like(chars As CharStream, str As String) As Boolean
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(str)
            Dim s As Stream = chars.str.BaseStream

            If s.Length <> bytes.Length Then
                Return False
            Else
                Dim chunk As Byte() = New Byte(bytes.Length - 1) {}
                Call s.Seek(Scan0, SeekOrigin.Begin)
                Call s.Read(chunk, Scan0, chunk.Length)
                Return chunk.SequenceEqual(bytes)
            End If
        End Operator

        Public Shared Operator =(chars As CharStream, str As String) As Boolean
            Return chars Like str
        End Operator

        Public Shared Operator <>(chars As CharStream, str As String) As Boolean
            Return Not chars Like str
        End Operator

        Public Shared Widening Operator CType(str As String) As CharStream
            Dim s As New MemoryStream(Encoding.UTF8.GetBytes(str))
            Dim reader As New StreamReader(s)
            Return New CharStream(reader)
        End Operator

    End Class

End Namespace
