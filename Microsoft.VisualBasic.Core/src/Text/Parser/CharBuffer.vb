#Region "Microsoft.VisualBasic::eb357249dc56432f5f04e838312e9825, sciBASIC#\Microsoft.VisualBasic.Core\src\Text\Parser\CharBuffer.vb"

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

'   Total Lines: 174
'    Code Lines: 112
' Comment Lines: 31
'   Blank Lines: 31
'     File Size: 5.16 KB


'     Class CharBuffer
' 
'         Properties: isInteger, Last, Size
' 
'         Function: Add, GetLastOrDefault, Pop, PopAllChars, ToString
' 
'         Sub: Clear
' 
'         Operators: *, (+3 Overloads) +, <, (+2 Overloads) <>, (+2 Overloads) =
'                    >, (+2 Overloads) Like
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Text.Parser

    ''' <summary>
    ''' A buffer list of the characters
    ''' </summary>
    Public Class CharBuffer

        ''' <summary>
        ''' the current buffer data list
        ''' </summary>
        ReadOnly buffer As New List(Of Char)

        ''' <summary>
        ''' get char from the buffer list via a given index value
        ''' </summary>
        ''' <param name="i">
        ''' 使用负数表示从尾到头
        ''' </param>
        ''' <returns></returns>
        Default Public ReadOnly Property GetChar(i As Integer) As Char
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer(i)
            End Get
        End Property

        ''' <summary>
        ''' get current size of the data in this char buffer object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Count
            End Get
        End Property

        ''' <summary>
        ''' get the last char in current buffer list data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' get the last char unsafe, this property may crashed the program if 
        ''' the <see cref="buffer"/> list data contains no elements. for get 
        ''' the last char safely, use the <see cref="GetLastOrDefault()"/> 
        ''' function.
        ''' </remarks>
        Public ReadOnly Property Last As Char
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                'If buffer.Count = 0 Then
                '    Return Nothing
                'End If
                Return buffer(buffer.Count - 1)
            End Get
        End Property

        ''' <summary>
        ''' test if current chars is like the integer string pattern
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isInteger As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.All(Function(c) Char.IsDigit(c))
            End Get
        End Property

        ''' <summary>
        ''' add a char into current buffer data list
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        Public Function Add(c As Char) As CharBuffer
            Call buffer.Add(c)
            Return Me
        End Function

        ''' <summary>
        ''' get the last char from current buffer list data safely, if the 
        ''' internal buffer list has no element data, then this function 
        ''' will returns nothing 
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLastOrDefault() As Char
            If buffer.Count = 0 Then
                Return Nothing
            Else
                Return buffer(buffer.Count - 1)
            End If
        End Function

        ''' <summary>
        ''' clear the internal buffer list data
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear()
            Call buffer.Clear()
        End Sub

        ''' <summary>
        ''' populate out the last char from the internal buffer list
        ''' </summary>
        ''' <returns></returns>
        Public Function Pop() As Char
            Dim last As Char = Me.Last
            Call buffer.RemoveLast
            Return last
        End Function

        ''' <summary>
        ''' populate all chars in current object and then clear the buffer list
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function PopAllChars() As Char()
            Return buffer.PopAll
        End Function

        ''' <summary>
        ''' text
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return buffer.CharString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(c As Char) As CharBuffer
            Return New CharBuffer + c
        End Operator

        Public Shared Operator +(buf As CharBuffer, c As Char) As CharBuffer
            buf.buffer.Add(c)
            Return buf
        End Operator

        Public Shared Operator +(buf As CharBuffer, c As Char?) As CharBuffer
            buf.buffer.Add(c)
            Return buf
        End Operator

        Public Shared Operator +(buf As CharBuffer, c As Value(Of Char)) As CharBuffer
            buf.buffer.Add(c.Value)
            Return buf
        End Operator

        Public Shared Operator *(buf As CharBuffer, n As Integer) As CharBuffer
            If n = 0 Then
                buf.Clear()
            Else
                Dim template As Char() = buf.buffer.ToArray

                For i As Integer = 1 To n - 1
                    buf.buffer.AddRange(template)
                Next
            End If

            Return buf
        End Operator

        ''' <summary>
        ''' string equals?
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="test"></param>
        ''' <returns></returns>
        Public Shared Operator =(buf As CharBuffer, test As String) As Boolean
            If buf <> test.Length Then
                Return False
            End If

            For i As Integer = 0 To test.Length - 1
                If buf.buffer(i) <> test(i) Then
                    Return False
                End If
            Next

            Return True
        End Operator

        ''' <summary>
        ''' string not equals?
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="test"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(buf As CharBuffer, test As String) As Boolean
            Return Not buf = test
        End Operator

        ''' <summary>
        ''' test current char buffer is equals to the given char?
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="test"></param>
        ''' <returns></returns>
        Public Shared Operator =(buf As CharBuffer, test As Char) As Boolean
            If buf.Size <> 1 Then
                Return False
            End If

            Return buf.buffer(Scan0) = test
        End Operator

        Public Shared Operator <>(buf As CharBuffer, test As Char) As Boolean
            Return Not buf = test
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count = size
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count <> size
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count > size
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(buf As CharBuffer, size As Integer) As Boolean
            Return buf.buffer.Count < size
        End Operator

        Public Shared Operator Like(buf As CharBuffer, any As String()) As Boolean
            Dim str As String = buf.ToString

            For Each right As String In any
                If str = right Then
                    Return True
                End If
            Next

            Return False
        End Operator
    End Class
End Namespace
