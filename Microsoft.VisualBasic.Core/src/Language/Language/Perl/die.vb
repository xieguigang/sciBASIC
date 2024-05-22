#Region "Microsoft.VisualBasic::e65fee26f9a132bfa486b08e28236137, Microsoft.VisualBasic.Core\src\Language\Language\Perl\die.vb"

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

    '   Total Lines: 94
    '    Code Lines: 54 (57.45%)
    ' Comment Lines: 20 (21.28%)
    '    - Xml Docs: 85.00%
    ' 
    '   Blank Lines: 20 (21.28%)
    '     File Size: 3.31 KB


    '     Structure ExceptionHandle
    ' 
    '         Function: [Default]
    '         Operators: (+2 Overloads) Or
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.Default

Namespace Language.Perl

    ''' <summary>
    ''' Syntax supports for perl language like exception handler
    ''' 
    ''' ```vbnet
    ''' Dim value = func() or die("message")
    ''' ```
    ''' </summary>
    Public Structure ExceptionHandle

        Dim message$
        Dim failure As Predicate(Of Object)

        Shared ReadOnly defaultHandler As New Predicate(Of Object)(AddressOf [Default])

        ''' <summary>
        ''' Returns True means test failure(<paramref name="obj"/> is nothing or empty!)
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Shared Function [Default](obj As Object) As Boolean
            If obj Is Nothing Then
                Return True
            End If

            Select Case obj.GetType
                Case GetType(Boolean)
                    Return False = DirectCast(obj, Boolean)

                    ' 对于字符串而言，判断是否为空的标准则是看字符串是否为空或者空字符串
                Case GetType(String)
                    Return String.IsNullOrEmpty(DirectCast(obj, String))

                Case GetType(Double)
                    Return CDbl(obj).IsNaNImaginary

                Case GetType(TimeSpan)
                    Return DirectCast(obj, TimeSpan) = TimeSpan.Zero

                Case GetType(Color)
                    Return DirectCast(obj, Color).IsEmpty

                Case GetType(Rectangle)
                    Return DirectCast(obj, Rectangle).IsEmpty

                Case GetType(RectangleF)
                    Return DirectCast(obj, RectangleF).IsEmpty

                Case GetType(Point)
                    Return DirectCast(obj, Point).IsEmpty

                Case GetType(PointF)
                    Return DirectCast(obj, PointF).IsEmpty

                Case Else

                    With obj.GetType
                        If .IsInheritsFrom(GetType(Array)) Then
                            Return DirectCast(obj, Array).Length = 0

                        ElseIf .ImplementInterface(GetType(IEnumerable)) Then
                            Return (From o In DirectCast(obj, IEnumerable).AsQueryable).Count = 0

                        ElseIf .ImplementInterface(GetType(IsEmpty)) Then
                            Return DirectCast(obj, IsEmpty).IsEmpty

                        Else
                            ' False表示没有错误
                            Return False
                        End If
                    End With
            End Select
        End Function

        ''' <summary>
        ''' Perl like exception handler syntax for testing the result is failure or not?
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="h"></param>
        ''' <returns></returns>
        Public Shared Operator Or(result As Object, h As ExceptionHandle) As Object
            If h.failure(result) Then
                Throw New Exception(h.message)
            Else
                Return result
            End If
        End Operator
    End Structure
End Namespace
