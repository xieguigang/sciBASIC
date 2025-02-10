﻿#Region "Microsoft.VisualBasic::9c8d0a4a8380c9de0dd35a75d9a8b119, Data_science\Visualization\Visualization\BinaryTree\EntityNode.vb"

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
    '     File Size: 1.11 KB


    '     Class EntityNode
    ' 
    '         Properties: EntityID, MySelf, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __pathName, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree

Namespace KMeans

    Public Class EntityNode : Inherits TreeNodeBase(Of EntityNode)

        Public ReadOnly Property EntityID As String
        Public ReadOnly Property Type As String

        Public Sub New(name As String, type$)
            MyBase.New(__pathName(name))
            Me.Type = type
            Me.EntityID = name
        End Sub

        Shared ReadOnly virtualPath As New Regex("\[\d+\]\d+(\.\d+)*")

        Private Shared Function __pathName(name$) As String
            If virtualPath.Match(name).Value = name Then
                Return name.Split("."c).Last
            Else
                Return name
            End If
        End Function

        Public Overrides ReadOnly Property MySelf As EntityNode
            Get
                Return Me
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return MyClass.FullyQualifiedName
        End Function
    End Class
End Namespace
