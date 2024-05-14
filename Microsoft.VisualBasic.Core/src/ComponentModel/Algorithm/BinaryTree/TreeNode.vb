#Region "Microsoft.VisualBasic::523e6160b6e3ad2429574d55f7ed2f04, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\TreeNode.vb"

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

    '   Total Lines: 149
    '    Code Lines: 76
    ' Comment Lines: 52
    '   Blank Lines: 21
    '     File Size: 5.25 KB


    '     Class BinaryTree
    ' 
    '         Properties: Key, Left, Members, MemberSize, QualifiedName
    '                     Right, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString, viewQualifiedName
    ' 
    '         Sub: Copy, SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' The binary tree node.
    ''' 
    ''' (如果执行的是聚类操作的话，可以通过!values字典属性来获取簇的结果)
    ''' 
    ''' 在这里只是二叉树的节点实现，具体的二叉树构建可以通过使用下面的算法来完成：
    ''' 
    ''' + <see cref="AVLTree(Of K, V)"/> 
    ''' 
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    Public Class BinaryTree(Of K, V) : Implements Value(Of V).IValueOf

        Dim _key As K

        ''' <summary>
        ''' 键名是唯一的，赋值之后就不可以改变了
        ''' </summary>
        ''' <returns></returns>
        Public Property Key As K
            Get
                Return _key
            End Get
            Friend Set(value As K)
                _key = value
            End Set
        End Property


        ''' <summary>
        ''' 与当前的这个键名相对应的键值可以根据需求发生改变，即可以被任意赋值
        ''' 
        ''' 如果是进行聚类操作的话，可以通过``!values`` as <see cref="List(Of V)"/>
        ''' 来添加簇成员
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As V Implements Value(Of V).IValueOf.Value
        Public Property Left As BinaryTree(Of K, V)
        Public Property Right As BinaryTree(Of K, V)

        ''' <summary>
        ''' Additional values that using for the binary tree algorithm.
        ''' </summary>
        ReadOnly additionals As New Dictionary(Of String, Object)

        ''' <summary>
        ''' Full name of current node
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property QualifiedName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return viewQualifiedName(Me)
            End Get
        End Property

        Default Public ReadOnly Property Item(key As String) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return additionals.TryGetValue(key)
            End Get
        End Property

        ''' <summary>
        ''' Get cluster members
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Members As V()
            Get
                Return DirectCast(Me!values, IEnumerable(Of V)).ToArray
            End Get
        End Property

        Public ReadOnly Property MemberSize As Integer
            Get
                Return DirectCast(Me!values, IEnumerable(Of V)).Count
            End Get
        End Property

        ReadOnly defaultView As New [Default](Of Func(Of K, String))(Function(key) Scripting.ToString(key))

        ''' <summary>
        ''' the given <paramref name="value"/> will be added 
        ''' into the member list by default in this constructor 
        ''' function.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="parent"></param>
        ''' <param name="toString">Default debug view is <see cref="Scripting.ToString"/></param>
        Sub New(key As K, value As V,
                Optional parent As BinaryTree(Of K, V) = Nothing,
                Optional toString As Func(Of K, String) = Nothing)

            Me.Key = key
            Me.Value = value

            Call SetValue("name", (toString Or defaultView)(key))
            Call SetValue("parent", parent)
            Call SetValue("values", New List(Of V))

            DirectCast(Me!values, List(Of V)).Add(value)
        End Sub

        ''' <summary>
        ''' Set <see cref="additionals"/> value by using a key value tuple.
        ''' </summary>
        ''' <param name="key$"></param>
        ''' <param name="value"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetValue(key$, value As Object)
            additionals(key) = value
        End Sub

        Public Sub Copy(source As BinaryTree(Of K, V))
            Key = source.Key
            Value = source.Value
            additionals.Clear()
            additionals.AddRange(source.additionals)
        End Sub

        Private Shared Function viewQualifiedName(node As BinaryTree(Of K, V)) As String
            Dim additionals = node.additionals
            Dim parent = TryCast(additionals!parent, BinaryTree(Of K, V))
            Dim name$ = additionals!name

            If parent Is Nothing Then
                Return "/"
            Else
                Return parent.QualifiedName & "/" & name
            End If
        End Function

        ''' <summary>
        ''' Display debug view as: ``[key, value]``
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"[{additionals!name}, {Value}]"
        End Function
    End Class
End Namespace
