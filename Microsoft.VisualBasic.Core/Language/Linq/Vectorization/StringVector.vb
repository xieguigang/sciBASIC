#Region "Microsoft.VisualBasic::b3a6a3b66fc156c868468c8499cea068, ..\sciBASIC#\Microsoft.VisualBasic.Core\Language\Linq\Vectorization\StringVector.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Vectorization

    Public Class StringVector : Inherits Vector(Of String)

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return buffer.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(buffer As IEnumerable(Of String))
            Me.buffer = buffer.SafeQuery.ToArray
        End Sub

        Public Shared Widening Operator CType(list As List(Of String)) As StringVector
            Return New StringVector(list)
        End Operator

        Public Shared Widening Operator CType(array As String()) As StringVector
            Return New StringVector(array)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator &(s1 As StringVector, s2$) As StringVector
            Return New StringVector(s1.Select(Function(s) s & s2))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(s1 As StringVector, s2$) As BooleanVector
            Return Not s1 = s2
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(s1 As StringVector, s2$) As BooleanVector
            Return New BooleanVector(s1.Select(Function(s) s = s2))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function InStr(string1 As StringVector, string2$, Optional method As CompareMethod = CompareMethod.Binary) As Vector(Of Integer)
            Return string1.Select(Function(str) Strings.InStr(str, string2, method)).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsPattern(strings As StringVector, pattern$, Optional opt As RegexOptions = RegexICSng) As BooleanVector
            Return strings.Select(Function(s) s.IsPattern(pattern, opt)).AsVector
        End Function
    End Class
End Namespace
