#Region "Microsoft.VisualBasic::dbf1c334a7be1398444e76dd42f8fba0, Microsoft.VisualBasic.Core\Language\Language\Perl\die.vb"

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

    '     Structure ExceptionHandler
    ' 
    '         Function: [Default]
    '         Operators: (+2 Overloads) Or
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace Language.Perl

    Public Structure ExceptionHandler

        Dim Message$
        Dim failure As Assert(Of Object)

        Shared ReadOnly defaultHandler As New Assert(Of Object)(AddressOf [Default])

        ''' <summary>
        ''' Returns True means test failure
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Shared Function [Default](obj As Object) As Boolean
            If obj Is Nothing Then
                Return True
            End If

            With obj.GetType

                If .ByRef Is GetType(Boolean) Then
                    Return False = DirectCast(obj, Boolean)

                    ' 对于字符串而言，判断是否为空的标准则是看字符串是否为空或者空字符串
                ElseIf .ByRef Is GetType(String) Then
                    Return String.IsNullOrEmpty(DirectCast(obj, String))

                ElseIf .IsInheritsFrom(GetType(Array)) Then
                    Return DirectCast(obj, Array).Length = 0

                ElseIf .ImplementInterface(GetType(IEnumerable)) Then
                    Return DirectCast(obj, IEnumerable).ToArray(Of Object).Length = 0

                ElseIf .ImplementInterface(GetType(IsEmpty)) Then
                    Return DirectCast(obj, IsEmpty).IsEmpty

                ElseIf .ByRef Is GetType(TimeSpan) Then
                    Return DirectCast(obj, TimeSpan) = TimeSpan.Zero

                Else
                    Return False ' False表示没有错误
                End If
            End With
        End Function

        ''' <summary>
        ''' Perl like exception handler syntax for testing the result is failure or not?
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="h"></param>
        ''' <returns></returns>
        Public Shared Operator Or(result As Object, h As ExceptionHandler) As Object
            If h.failure(result) Then
                Throw New Exception(h.Message)
            Else
                Return result
            End If
        End Operator
    End Structure
End Namespace
