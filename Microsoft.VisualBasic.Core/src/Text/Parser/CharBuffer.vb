#Region "Microsoft.VisualBasic::c9492e12360a6fadcf25c670f9477a27, Microsoft.VisualBasic.Core\src\Text\Parser\CharBuffer.vb"

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

    '   Total Lines: 350
    '    Code Lines: 201 (57.43%)
    ' Comment Lines: 104 (29.71%)
    '    - Xml Docs: 93.27%
    ' 
    '   Blank Lines: 45 (12.86%)
    '     File Size: 11.43 KB


    '     Class CharBuffer
    ' 
    '         Properties: isInteger, Last, Size
    ' 
    '         Function: (+2 Overloads) Add, AsEnumerable, GetLastOrDefault, Pop, PopAllChars
    '                   (+3 Overloads) StartsWith, ToArray, ToString
    ' 
    '         Sub: Clear
    ' 
    '         Operators: *, (+3 Overloads) +, <, (+3 Overloads) <>, (+3 Overloads) =
    '                    >, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
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
        ''' <remarks>
        ''' the negative value not works here, just test for the integer chars pattern
        ''' </remarks>
        Public ReadOnly Property isInteger As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.All(Function(c) Char.IsDigit(c))
            End Get
        End Property

        Public Function StartsWith(c As Char) As Boolean
            If Size = 0 Then
                Return False
            Else
                Return buffer(Scan0) = c
            End If
        End Function

        ''' <summary>
        ''' test if current char buffer is starts with a specific prefix string
        ''' </summary>
        ''' <param name="prefix"></param>
        ''' <returns></returns>
        Public Function StartsWith(prefix As String) As Boolean
            If Me.Size < prefix Then
                Return False
            Else
                For i As Integer = 0 To prefix.Length - 1
                    If buffer(i) <> prefix(i) Then
                        Return False
                    End If
                Next

                Return True
            End If
        End Function

        Public Function StartsWith(r As Regex) As Boolean
            Dim s As New String(buffer.ToArray)
            Dim m As String = r.Match(s).Value

            If m.StringEmpty Then
                Return False
            ElseIf s.StartsWith(m) Then
                Return True
            Else
                Return False
            End If
        End Function

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
        ''' add a collection of char into current buffer data list
        ''' </summary>
        ''' <param name="chars"></param>
        ''' <returns></returns>
        Public Function Add(chars As String) As CharBuffer
            Call buffer.AddRange(chars)
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

        Public Function AsEnumerable() As IEnumerable(Of Char)
            Return buffer
        End Function

        Public Function ToArray() As Char()
            Return buffer.ToArray
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

        ''' <summary>
        ''' Convert a string object to a char buffer
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        Public Shared Widening Operator CType(str As String) As CharBuffer
            Dim buf As New CharBuffer

            If Not str Is Nothing Then
                For Each c As Char In str
                    Call buf.buffer.Add(c)
                Next
            End If

            Return buf
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
        ''' test current char buffer size is 1 and also is equals to the given char?
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="test"></param>
        ''' <returns>
        ''' this function returns false if the buffer size is not equals to 1
        ''' if the single char in the given buffer is not equals to the given 
        ''' <paramref name="test"/> char, then returns false.
        ''' </returns>
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
            If buf Is Nothing Then
                Return 0 = size
            Else
                Return buf.buffer.Count = size
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(buf As CharBuffer, size As Integer) As Boolean
            If buf Is Nothing Then
                Return 0 = size
            Else
                Return buf.buffer.Count <> size
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(buf As CharBuffer, size As Integer) As Boolean
            If buf Is Nothing Then
                Return 0 > size
            Else
                Return buf.buffer.Count > size
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(buf As CharBuffer, size As Integer) As Boolean
            If buf Is Nothing Then
                Return 0 < size
            Else
                Return buf.buffer.Count < size
            End If
        End Operator

        Public Shared Narrowing Operator CType(chr As CharBuffer) As Char
            If chr > 0 Then
                Return chr.buffer(0)
            Else
                Return Nothing
            End If
        End Operator

        Public Shared Operator Like(buf As CharBuffer, [set] As Index(Of Char)) As Boolean
            If buf <> 1 Then
                Return False
            Else
                Return CChar(buf) Like [set]
            End If
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
