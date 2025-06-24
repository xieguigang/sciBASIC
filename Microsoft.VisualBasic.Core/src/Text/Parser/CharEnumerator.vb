#Region "Microsoft.VisualBasic::0c58965277ad160b5e9898ac2d07601f, Microsoft.VisualBasic.Core\src\Text\Parser\CharEnumerator.vb"

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

'   Total Lines: 109
'    Code Lines: 68 (62.39%)
' Comment Lines: 24 (22.02%)
'    - Xml Docs: 95.83%
' 
'   Blank Lines: 17 (15.60%)
'     File Size: 3.62 KB


'     Class CharPtr
' 
'         Properties: Remaining
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: PeekNext, PopNext, ToString
'         Operators: <>, =, (+2 Overloads) Like, (+2 Overloads) Not
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Text.Parser

    ''' <summary>
    ''' Char enumerator
    ''' </summary>
    Public Class CharPtr : Inherits Pointer(Of Char)

        ''' <summary>
        ''' 查看还有多少字符没有被处理完
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Remaining As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Skip(index + 1).CharString
            End Get
        End Property

        Sub New(data As String)
            ' 可能会存在空字符串
            Call MyBase.New(data.SafeQuery)
        End Sub

        Public Overloads Function PeekNext(length As Integer) As String
            Dim buf As New List(Of Char)

            For i As Integer = 0 To length - 1
                buf += Me(i)
            Next

            Return buf.CharString
        End Function

        Public Function PopNext(length As Integer) As String
            Dim result As String = PeekNext(length)
            index += length
            Return result
        End Function

        ''' <summary>
        ''' 这个调试试图函数会将当前的读取位置给标记出来
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim previous$ = buffer.Take(index).CharString
            Dim current As Char = Me.Current
            Dim remaining$ = Me.Remaining

            Return $"{previous} ->[{current}]<- {remaining}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator Not(chars As CharPtr) As Boolean
            Return Not chars.EndRead
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(str As String) As CharPtr
            If str Is Nothing Then
                Return Nothing
            End If

            Return New CharPtr(str)
        End Operator

        ''' <summary>
        ''' construct an in-memory dataset
        ''' </summary>
        ''' <param name="str">the in-memory string data</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(str As StringBuilder) As CharPtr
            Return New CharPtr(str.ToString)
        End Operator

        ''' <summary>
        ''' check of the string equals?
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(str As CharPtr, text As String) As Boolean
            If str Is Nothing Then
                Return text Is Nothing
            Else
                Return New String(str.buffer) = text
            End If
        End Operator

        Public Overloads Shared Operator <>(str As CharPtr, text As String) As Boolean
            Return Not str = text
        End Operator

        ''' <summary>
        ''' Check of the text equals?
        ''' </summary>
        ''' <param name="str">text1</param>
        ''' <param name="text">text2</param>
        ''' <returns></returns>
        Public Shared Operator Like(str As CharPtr, text As String) As Boolean
            If str Is Nothing Then
                Return text Is Nothing
            Else
                Return New String(str.buffer).TextEquals(text)
            End If
        End Operator
    End Class
End Namespace
