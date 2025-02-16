#Region "Microsoft.VisualBasic::8093aee2e02a419e066d78b257ba3c9a, Data_science\Graph\Model\Tree\TermTree.vb"

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

    '   Total Lines: 79
    '    Code Lines: 55 (69.62%)
    ' Comment Lines: 11 (13.92%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 13 (16.46%)
    '     File Size: 2.38 KB


    ' Class TermTree
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) Add, FindRoot, newChild, Visit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection

''' <summary>
''' A tree with string term as key
''' </summary>
''' 
<DataContract>
Public Class TermTree(Of T) : Inherits Tree(Of T, String)

    Default Public Overloads Property Child(path As String) As T
        Get
            Return Visit(path.Split("/"c)).Data
        End Get
        Set(value As T)
            Visit(path.Split("/"c)).Data = value
        End Set
    End Property

    Sub New()
        Call MyBase.New(qualDeli:="/")
    End Sub

    Public Function Visit(path As String()) As TermTree(Of T)
        If path.Length = 1 Then
            Return MyBase.Child(path(Scan0))
        Else
            Return Visit(path.Skip(1).ToArray)
        End If
    End Function

    Private Function newChild(name As String, value As T) As TermTree(Of T)
        Return New TermTree(Of T) With {
            .Data = value,
            .ID = Me.ID + Childs.Count,
            .label = name,
            .Parent = Me,
            .Childs = New Dictionary(Of String, Tree(Of T, String))
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path">Path tokens should seperated with delimiter ``/``.</param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Add(path As String, value As T) As TermTree(Of T)
        Return Add(path.Trim("/"c).Split("/"c), value)
    End Function

    Public Function Add(path As String(), value As T) As TermTree(Of T)
        Dim next$ = path(Scan0)

        If path.Length = 1 Then
            Childs.Add([next], newChild([next], value))

            ' return this new tree leaf
            Return Childs([next])
        Else
            If Not Childs.ContainsKey([next]) Then
                Childs.Add([next], newChild([next], Nothing))
            End If

            Return DirectCast(Childs([next]), TermTree(Of T)).Add(path.Skip(1).ToArray, value)
        End If
    End Function

    Public Shared Function FindRoot(tree As TermTree(Of T)) As TermTree(Of T)
        Dim parent As TermTree(Of T) = tree

        Do While parent.Parent IsNot Nothing
            parent = parent.Parent
        Loop

        Return parent
    End Function
End Class
