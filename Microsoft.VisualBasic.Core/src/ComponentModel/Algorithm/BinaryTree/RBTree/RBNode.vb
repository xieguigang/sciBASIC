#Region "Microsoft.VisualBasic::9433a555ccbd8a2e99c97c7302b143e2, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\RBTree\RBNode.vb"

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

    '   Total Lines: 47
    '    Code Lines: 34
    ' Comment Lines: 5
    '   Blank Lines: 8
    '     File Size: 1.56 KB


    '     Class RBNode
    ' 
    '         Properties: Child, Red
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IsRed, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' 与键名所对应的数据是存储在<see cref="BinaryTree(Of K, V).Value"/>之中的
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    Public Class RBNode(Of K, V) : Inherits BinaryTree(Of K, V)

        Public Property Red As Boolean

        Public Property Child(dir As Boolean) As RBNode(Of K, V)
            Get
                If dir Then
                    Return Right
                Else
                    Return Left
                End If
            End Get
            Set(value As RBNode(Of K, V))
                If dir Then
                    Right = value
                Else
                    Left = value
                End If
            End Set
        End Property

        Public Sub New(key As K, value As V,
                       Optional parent As BinaryTree(Of K, V) = Nothing,
                       Optional toString As Func(Of K, String) = Nothing)

            MyBase.New(key, value, parent, toString)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{If(Red, "Red", "Black")}] {MyBase.ToString}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function IsRed(node As RBNode(Of K, V)) As Boolean
            Return Not node Is Nothing AndAlso node.Red
        End Function
    End Class
End Namespace
