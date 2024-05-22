#Region "Microsoft.VisualBasic::d70edc6bcd7ed6ab6fb472f722178e7a, Microsoft.VisualBasic.Core\src\Language\Value\Numeric\chr.vb"

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

    '   Total Lines: 71
    '    Code Lines: 53 (74.65%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (25.35%)
    '     File Size: 2.04 KB


    '     Class chr
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    '         Operators: -, +, <, (+2 Overloads) <=, >
    '                    (+2 Overloads) >=, >>
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    Public Class chr

        ReadOnly [char] As Char

        Sub New([char] As Char)
            Me.char = [char]
        End Sub

        Sub New(i32 As Integer)
            Me.char = ChrW(i32)
        End Sub

        Public Overrides Function ToString() As String
            Return [char]
        End Function

        Public Shared Widening Operator CType(c As Char) As chr
            Return New chr(c)
        End Operator

        Public Shared Widening Operator CType(i As Integer) As chr
            Return New chr(i)
        End Operator

        Public Shared Operator -(c As chr, w As Char) As chr
            Return New chr(AscW(c.char) - AscW(w))
        End Operator

        Public Shared Operator +(c As chr, i As Integer) As chr
            Return New chr(AscW(c.char) + i)
        End Operator

        Public Shared Operator <(c As chr, i As Integer) As Boolean
            Return AscW(c.char) < i
        End Operator

        Public Shared Operator >(c As chr, i As Integer) As Boolean
            Return AscW(c.char) > i
        End Operator

        Public Shared Operator <=(c As chr, i As Integer) As Boolean
            Return AscW(c.char) <= i
        End Operator

        Public Shared Operator >=(c As chr, i As Integer) As Boolean
            Return AscW(c.char) >= i
        End Operator

        Public Shared Operator <=(c As chr, w As Char) As Boolean
            Return c.char <= w
        End Operator

        Public Shared Operator >=(c As chr, w As Char) As Boolean
            Return c.char >= w
        End Operator

        Public Shared Operator >>(c As chr, i As Integer) As Integer
            Return AscW(c.char) >> i
        End Operator

        Public Shared Narrowing Operator CType(c As chr) As Integer
            Return AscW(c.char)
        End Operator

        Public Shared Narrowing Operator CType(c As chr) As Char
            Return c.char
        End Operator
    End Class
End Namespace
