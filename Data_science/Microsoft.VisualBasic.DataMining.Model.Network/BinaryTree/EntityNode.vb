#Region "Microsoft.VisualBasic::5407a51a92a6ecca194ff19686faf21e, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Model.Network\BinaryTree\EntityNode.vb"

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


Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class EntityNode
        Inherits TreeNodeBase(Of EntityNode)

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
