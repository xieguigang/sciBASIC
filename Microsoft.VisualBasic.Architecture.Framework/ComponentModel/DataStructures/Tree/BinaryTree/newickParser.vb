#Region "Microsoft.VisualBasic::13af97ed546a6ac140b05f1134662067, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Tree\BinaryTree\newickParser.vb"

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

Imports System.Text

Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' http://www.evolgenius.info/evolview/
    ''' </summary>
    Public Module NewickParser

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="input"></param>
        ''' <param name="hashTranslate">可以通过这个对象将节点编号映射为名称</param>
        ''' <returns></returns>
        Public Function TreeParser(Of T)(input As String, Optional hashTranslate As Dictionary(Of String, String) = Nothing) As TreeNode(Of T)
            Dim ROOT As TreeNode(Of T) = New TreeNode(Of T)
            If hashTranslate Is Nothing Then
                hashTranslate = New Dictionary(Of String, String)
            End If
            Call TreeParser(input, hashTranslate, ROOT)
            Return ROOT
        End Function

        ''' <summary>
        ''' created: Oct 20, 2013 : a better and easier to maintain parser for newick and nexus trees
        ''' NOTE: this is a recursive function </summary>
        ''' <param name="inputstr"> : input tree string </param>
        ''' <param name="hashTranslate"> : aliases for lead nodes (for nexsus format) </param>
        ''' <param name="iNode"> : current internal node; == rootNode the first time 'newickParser' is called  </param>
        Public Sub TreeParser(Of T)(inputstr As String, hashTranslate As Dictionary(Of String, String), ByRef iNode As TreeNode(Of T))
            inputstr = inputstr.Trim()

            ' NOTE: the input string should look like this: (A,B,(C,D)E)F
            ' the first char has to be (
            ' first, get what's between the first and the last Parentheses, and what's after the last right Parentheses
            ' for example, your tree : (A,B,(C,D)E)F will be split into two parts:
            '   A,B,(C,D)E = ...
            '   F = tail string
            Dim tailString As String = ""

            If Not inputstr.Length = 0 Then
                ' remove trailing ';'
                While inputstr.EndsWith(";")

                    ' is this really necessary???
                    inputstr = inputstr.Substring(0, inputstr.Length - 1)
                End While
                For idx As Integer = inputstr.Length - 1 To 0 Step -1
                    If inputstr(idx) = ")"c Then
                        tailString = inputstr.Substring(idx + 1)

                        ' change input str from (A,B,(C,D)E)F to A,B,(C,D)E
                        inputstr = inputstr.Substring(1, idx - 1)
                        ' !!!!!
                        Exit For
                    End If
                Next
            End If

            ' if the string4internalNode string is not empty

            ' now go through what's between the parentheses and get the leaf nodes
            '   (A,B,(C,D)E)F = original tree
            '   A,B,(C,D)E = the part the following codes will deal with
            If Not inputstr.Length = 0 Then

                ' split current input string into substrings, each is a daughtor node of current internal node
                ' if your input string is like this: A,B,(C,D)E
                ' it will be split into the following three daughter strings:
                '  A
                '  B
                '  (C,D)E
                ' accordingly, three daughter nodes will be created, two are leaf nodes and one is an internal node 
                Dim brackets As Integer = 0, leftParenthesis As Integer = 0, commas As Integer = 0
                Dim sb As New StringBuilder()

                For Each c As Char In inputstr
                    If (c = ","c OrElse c = ")"c) AndAlso brackets = 0 Then
                        ' ',' usually indicates the end of an node; is || c == ')' really necessary ???
                        ' make daugher nodes
                        Dim daughter = sb.ToString()
                        If leftParenthesis > 0 AndAlso commas > 0 Then
                            NewickParser.TreeParser(daughter, hashTranslate, __makeInternalNode("", False, iNode))
                        Else
                            ' a leaf daughter 
                            ' parse information for current daughter node
                            parseInforAndMakeNewLeafNode(daughter, hashTranslate, iNode)
                        End If

                        ' reset some variables
                        sb = New StringBuilder()
                        leftParenthesis = 0
                    Else
                        sb.Append(c)
                        ' ',' will not be recored
                        If c = ","c Then
                            commas += 1
                        End If
                    End If

                    '  brackets is used to find the contents between a pair of matching ()s
                    '  how does this work???
                    '  
                    '  here is how the value of brackets changes if your input string is like this :
                    '  (A,B,(C,D)E)F
                    '  1    2   1 0 # value of brackets ... 
                    '  +    +   - - # operation
                    '  ^          ^ # contents between these two () will be extracted = A,B,(C,D)E
                    '  
                    '  --- 
                    '  variable 'leftParenthesis' is used to indicate whether current daughter node is likely a internal node; 
                    '  however, this alone cannot garrentee this, because the name of a leaf node may contain Parenthesis
                    '  therefore I use 'leftParenthesis' and 'commas' together to indicate an internal node  
                    If c = "("c Then
                        brackets += 1
                        leftParenthesis += 1
                    ElseIf c = ")"c Then
                        brackets -= 1
                    End If
                Next

                ' deal with the last daughter 
                Dim LastDaughter As String = sb.ToString()

                If leftParenthesis > 0 AndAlso commas > 0 Then
                    NewickParser.TreeParser(LastDaughter, hashTranslate, __makeInternalNode("", False, iNode))
                Else
                    ' a leaf daughter 
                    ' parse information for current daughter node
                    parseInforAndMakeNewLeafNode(LastDaughter, hashTranslate, iNode)

                End If
            End If
        End Sub

        ''' <summary>
        ''' Dec 5, 2011; can be used to make rootnode
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="isroot"></param>
        ''' <param name="parentnode"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function __makeInternalNode(Of T)(id As String, isroot As Boolean, ByRef parentnode As TreeNode(Of T)) As TreeNode(Of T)
            Dim NewNodeObject As New TreeNode(Of T)(id, Nothing)

            ' dec 5, 2011
            If Not isroot AndAlso parentnode IsNot Nothing Then
                parentnode += NewNodeObject
            End If

            Return NewNodeObject
        End Function

        ''' <summary>
        ''' created on Oct 20, 2013 
        ''' input: the leafstr to be parsed, the internal node the leaf node has to be added to 
        ''' </summary>
        Private Sub parseInforAndMakeNewLeafNode(Of T)(leafstr As String, hashTranslate As Dictionary(Of String, String), iNode As TreeNode(Of T))
            leafstr = leafstr.Trim()

            ' parse a leaf node,
            ' possibilities are:
            ' 1. ,, - leaf node is not named (???)
            ' 2. A  - named leaf node
            ' 3. :0.1 - unamed leaf node with branch length
            ' 4. A:0.1 - named leaf node with branch length
            If leafstr.Length = 0 Then
                ' case 1
                __makeLeafNode("", iNode)
            Else
                ' split it into two parts
                Dim parts As String() = leafstr.StringSplit(":", True)

                ' now deal with part 1, two possibilities: named / unamed leaf node
                Dim part1 As String = parts(0)
                If part1.Length = 0 Then
                    __makeLeafNode("", iNode)
                Else
                    Dim leafNodeName As String = part1.Replace("'", "").Replace("""", "")
                    leafNodeName = If((hashTranslate IsNot Nothing AndAlso hashTranslate.ContainsKey(leafNodeName)), hashTranslate(leafNodeName), leafNodeName)
                    __makeLeafNode(leafNodeName, iNode)
                End If
            End If
        End Sub

        Private Function __makeLeafNode(Of T)(id As String, ByRef parentnode As TreeNode(Of T)) As TreeNode(Of T)
            Dim leafnode As New TreeNode(Of T)(id, Nothing)
            parentnode += leafnode
            Return leafnode
        End Function
    End Module
End Namespace
