#Region "Microsoft.VisualBasic::edf41763fa59764d514da90e85ab0f5f, Data_science\DataMining\DBNCode\utils\Forest.vb"

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

    '   Total Lines: 104
    '    Code Lines: 52 (50.00%)
    ' Comment Lines: 39 (37.50%)
    '    - Xml Docs: 10.26%
    ' 
    '   Blank Lines: 13 (12.50%)
    '     File Size: 3.95 KB


    '     Class Forest
    ' 
    '         Properties: Empty, Root
    ' 
    '         Function: add, ToString
    ' 
    '         Sub: deleteUp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace utils

    Public Class Forest(Of T)

        Private roots As IDictionary(Of T, TreeNode(Of T)) = New Dictionary(Of T, TreeNode(Of T))()

        Public Overridable Function add(nodeData As T, childrenData As IList(Of T)) As TreeNode(Of T)
            Dim node As TreeNode(Of T) = New TreeNode(Of T)(nodeData)
            For Each childData In childrenData
                'check if child is already in the forest as root
                If roots.ContainsKey(childData) Then
                    Dim childNode As TreeNode(Of T) = roots(childData)
                    roots.Remove(childData)

                    node.addChild(childNode)
                Else
                    node.addChild(childData)
                End If
            Next
            roots(nodeData) = node
            Return node
        End Function

        Public Overridable ReadOnly Property Root As TreeNode(Of T)
            Get
                Return roots.Values.First()
            End Get
        End Property

        '	/**
        '	 * Deletes the nodes belonging to the path between source and target.
        '	 * Source must be a root of the forest and target must be a leaf and
        '	 * a descendant from source. All orphaned children (descendants of path
        '	 * nodes) become roots of the forest.
        '	 * @param source Root source node.
        '	 * @param target Leaf target node.
        '	 
        '	public void deletePath(TreeNode<T> source, TreeNode<T> target){
        '		
        '		T sourceData = source.getData();
        '		TreeNode<T> sourceRoot = roots.remove(sourceData);
        '		assert source == sourceRoot;
        '		
        '		try{
        '			List<TreeNode<T>> newRoots = sourceRoot.deleteDown(target);
        '			if (newRoots == null){
        '				// path not found, puts the root back
        '				roots.put(sourceData, sourceRoot);
        '				throw new IllegalArgumentException("There is no path between source and target.");
        '			}
        '			else{
        '				for (TreeNode<T> newRoot : newRoots){
        '					roots.put(newRoot.getData(), newRoot);
        '				}
        '			}
        '		}
        '		catch (IllegalArgumentException e) {
        '			e.printStackTrace();
        '			System.exit(1);
        '		}		
        '	}

        ''' <summary>
        ''' Deletes the nodes belonging to the path between leaf and up to the root.
        ''' All orphaned children (descendants of path nodes) become roots of the forest. </summary>
        ''' <param name="leaf"> node with no children  </param>
        Public Overridable Sub deleteUp(leaf As TreeNode(Of T))
            Dim newRoots As IList(Of TreeNode(Of T)) = Nothing
            '		System.out.println("deleting up "+leaf.getData());
            Try
                newRoots = leaf.deleteUp(roots)
            Catch e As ArgumentException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
                Environment.Exit(1)
            End Try
            For Each newRoot As TreeNode(Of T) In newRoots
                '			System.out.println("putting new root "+newRoot.getData());			
                roots(newRoot.Data) = newRoot
            Next
        End Sub

        Public Overridable ReadOnly Property Empty As Boolean
            Get
                Return roots.Count = 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            sb.Append("Forest contains " & roots.Count.ToString() & " trees." & ls)
            For Each treeRoot As TreeNode(Of T) In roots.Values
                sb.Append(treeRoot.ToString() & ls)
            Next
            Return sb.ToString()
        End Function


    End Class

End Namespace

