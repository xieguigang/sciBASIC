#Region "Microsoft.VisualBasic::4c22c5e88fb617d378d886d043c6219f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Value\IfValue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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

Namespace Language.Values

    Public Class IfValue(Of T As {IComparable, Structure})
        Inherits Value(Of T)

        Public Overloads Shared Widening Operator CType(o As T) As IfValue(Of T)
            Return New IfValue(Of T) With {.value = o}
        End Operator

        ''' <summary>
        ''' 如果相等就返回<paramref name="o"/>
        ''' </summary>
        ''' <param name="[if]"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =([if] As IfValue(Of T), o As T) As T
            If [if].value.CompareTo(o) = 0 Then
                Return o
            Else
                Return Nothing
            End If
        End Operator

        Public Overloads Shared Operator <>([if] As IfValue(Of T), o As T) As T
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' 如果相等就返回<paramref name="o"/>
        ''' </summary>
        ''' <param name="[if]"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =([if] As IfValue(Of T), o As IfValue(Of T)) As T
            If [if].value.CompareTo(o.value) = 0 Then
                Return o.value
            Else
                Return Nothing
            End If
        End Operator

        Public Overloads Shared Operator <>([if] As IfValue(Of T), o As IfValue(Of T)) As T
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace
