#Region "Microsoft.VisualBasic::446508712398d6c99193d23c063b824e, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\lang\Python\Array.vb"

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

Imports System.Runtime.CompilerServices

Namespace Language.Python

    Public Class Array(Of T) : Inherits List(Of T)

        Public Overloads Property Item(index%) As T
            Get
                If index < 0 Then
                    Return MyBase.Item(Count + index)
                Else
                    Return MyBase.Item(index)
                End If
            End Get
            Set(value As T)
                If index < 0 Then
                    MyBase.Item(Count + index) = value
                Else
                    MyBase.Item(index) = value
                End If
            End Set
        End Property

        Sub New()
            MyBase.New
        End Sub

        Sub New(source As IEnumerable(Of T))
            MyBase.New(source)
        End Sub

    End Class
End Namespace
