#Region "Microsoft.VisualBasic::ee3a58e9f99bcd84b6199a644d8969df, Data\BinaryData\BinaryData\XDR\Unpacker.vb"

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

    '   Total Lines: 51
    '    Code Lines: 41 (80.39%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (19.61%)
    '     File Size: 1.59 KB


    '     Class Unpacker
    ' 
    '         Properties: EndOfStream, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Read, UnpackDouble, UnpackInteger
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports System.Runtime.CompilerServices

Namespace Xdr

    Public Class Unpacker : Implements IByteReader

        ReadOnly data As BinaryDataReader

        Public Property Position As Long
            Get
                Return data.Position
            End Get
            Set(value As Long)
                data.Position = value
            End Set
        End Property

        Public ReadOnly Property EndOfStream As Boolean Implements IByteReader.EndOfStream
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return data.EndOfStream
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New(data As BinaryDataReader)
            Me.data = data
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function UnpackInteger() As Object
            Return XdrEncoding.DecodeInt32(Me)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function UnpackDouble() As Object
            Return XdrEncoding.DecodeDouble(Me)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Read(count As UInteger) As Byte() Implements IByteReader.Read
            Return data.ReadBytes(count)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Read() As Byte Implements IByteReader.Read
            Return data.ReadByte
        End Function
    End Class
End Namespace
