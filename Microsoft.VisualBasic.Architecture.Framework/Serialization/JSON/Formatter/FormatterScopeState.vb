#Region "Microsoft.VisualBasic::825a83ffbbe8dddcddc7c2dc18f51d86, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Serialization\JSON\Formatter\FormatterScopeState.vb"

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

Imports System.Collections.Generic

Namespace Serialization.JSON.Formatter.Internals

    Friend NotInheritable Class FormatterScopeState
        Public Enum JsonScope
            [Object]
            Array
        End Enum

        ReadOnly scopeStack As New Stack(Of JsonScope)()

        Public ReadOnly Property IsTopTypeArray() As Boolean
            Get
                Return scopeStack.Count > 0 AndAlso scopeStack.Peek() = JsonScope.Array
            End Get
        End Property

        Public ReadOnly Property ScopeDepth() As Integer
            Get
                Return scopeStack.Count
            End Get
        End Property

        Public Sub PushObjectContextOntoStack()
            scopeStack.Push(JsonScope.[Object])
        End Sub

        Public Function PopJsonType() As JsonScope
            Return scopeStack.Pop()
        End Function

        Public Sub PushJsonArrayType()
            scopeStack.Push(JsonScope.Array)
        End Sub
    End Class
End Namespace
