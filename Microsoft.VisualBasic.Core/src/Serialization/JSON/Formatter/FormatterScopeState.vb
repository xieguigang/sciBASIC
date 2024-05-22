#Region "Microsoft.VisualBasic::2f41f4c5c4d2398be38343660afc9b67, Microsoft.VisualBasic.Core\src\Serialization\JSON\Formatter\FormatterScopeState.vb"

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

    '   Total Lines: 38
    '    Code Lines: 29 (76.32%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (23.68%)
    '     File Size: 1.03 KB


    '     Class FormatterScopeState
    ' 
    ' 
    '         Enum JsonScope
    ' 
    '             [Object], Array
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: IsTopTypeArray, ScopeDepth
    ' 
    '     Function: PopJsonType
    ' 
    '     Sub: PushJsonArrayType, PushObjectContextOntoStack
    ' 
    ' 
    ' /********************************************************************************/

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
