#Region "Microsoft.VisualBasic::61c1e954ab5b1b2692e6084b6ac0215e, Data_science\DataMining\DynamicProgramming\Generic\GenericSymbol.vb"

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

    '   Total Lines: 37
    '    Code Lines: 29 (78.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.18 KB


    ' Class GenericSymbol
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: getEmpty, getEquals, ToChar
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming

Public Class GenericSymbol(Of T)

    Friend ReadOnly m_equals As Func(Of T, T, Boolean)
    Friend ReadOnly m_similarity As Func(Of T, T, Double)
    Friend ReadOnly m_viewChar As Func(Of T, Char)
    Friend ReadOnly m_empty As Func(Of T)

    Sub New(equals As Func(Of T, T, Boolean), similarity As Func(Of T, T, Double), toChar As Func(Of T, Char), Optional empty As Func(Of T) = Nothing)
        Me.m_equals = equals
        Me.m_similarity = similarity
        Me.m_viewChar = toChar

        If empty Is Nothing Then
            m_empty = Function() Nothing
        Else
            m_empty = empty
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToChar(t As T) As Char
        Return m_viewChar(t)
    End Function

    Public Function equalsTo(x As T, y As T) As Boolean
        Return m_equals(x, y)
    End Function

    Public Function getEquals() As IEquals(Of T)
        Return Function(x, y) m_equals(x, y)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getEmpty() As T
        Return m_empty()
    End Function

End Class
