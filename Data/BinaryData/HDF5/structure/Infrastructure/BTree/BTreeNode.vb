#Region "Microsoft.VisualBasic::e21898ecaffc6d73cb30d7900e831485, Data\BinaryData\HDF5\structure\Infrastructure\BTree\BTreeNode.vb"

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

    '   Total Lines: 266
    '    Code Lines: 115 (43.23%)
    ' Comment Lines: 111 (41.73%)
    '    - Xml Docs: 69.37%
    ' 
    '   Blank Lines: 40 (15.04%)
    '     File Size: 11.07 KB


    '     Class BTreeNode
    ' 
    '         Properties: isLeaf, leftAddress, level, magic, numberOfEntries
    '                     rightAddress, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [next], hasNext
    ' 
    '         Sub: first, printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' B-tree Nodes
    ''' 
    ''' Version 1 B-trees in HDF5 files are an implementation of the B-link tree. The sibling 
    ''' nodes at a particular level in the tree are stored in a doubly-linked list. See the 
    ''' “Efficient Locking for Concurrent Operations on B-trees” paper by Phillip Lehman and S. 
    ''' Bing Yao as published in the ACM Transactions on Database Systems, Vol. 6, No. 4, 
    ''' December 1981.
    '''
    ''' The B-trees implemented by the file format contain one more key than the number Of children. 
    ''' In other words, Each child pointer out Of a B-tree node has a left key And a right key. 
    ''' The pointers out Of internal nodes point To Sub-trees While the pointers out Of leaf 
    ''' nodes point To symbol nodes And raw data chunks. Aside from that difference, internal 
    ''' nodes And leaf nodes are identical.
    ''' </summary>
    Public Class BTreeNode : Inherits HDF5Ptr
        Implements IMagicBlock

        ''' <summary>
        ''' The ASCII character string “TREE” is used to indicate the beginning of a B-tree node. 
        ''' This gives file consistency checking utilities a better chance of reconstructing a 
        ''' damaged file.
        ''' </summary>
        Public Const signature$ = "TREE"

        Dim layout As Layout
        Dim currentNode As BTreeNode

        ' level 0 only
        Dim entries As List(Of DataChunk)
        ' level > 0 only
        Dim offsets As Integer()()

        ' int[nentries][ndim]; // other levels

        ''' <summary>
        ''' "For raw data chunk nodes, the child pointer is the address of a single raw data chunk"
        ''' </summary>
        Dim childPointer As Long()
        ' long[nentries];

        ''' <summary>
        ''' track iteration; LOOK this seems fishy - why not an iterator ??
        ''' </summary>
        Dim currentEntry As i32

        ''' <summary>
        ''' The ASCII character string “TREE” is used to indicate the beginning of a B-tree node. 
        ''' This gives file consistency checking utilities a better chance of reconstructing a 
        ''' damaged file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property magic As String Implements IMagicBlock.magic
        ''' <summary>
        ''' Each B-tree points to a particular type of data. This field indicates the type of data 
        ''' as well as implying the maximum degree K of the tree and the size of each Key field.
        '''
        ''' + 0. This tree points to group nodes.
        ''' + 1. This tree points to raw data chunk nodes.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property type As BTreeNodeTypes
        ''' <summary>
        ''' The node level indicates the level at which this node appears in the tree (leaf nodes 
        ''' are at level zero). Not only does the level indicate whether child pointers point to 
        ''' sub-trees or to data, but it can also be used to help file consistency checking utilities 
        ''' reconstruct damaged trees.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property level As Integer

        Public ReadOnly Property isLeaf As Boolean
            Get
                Return level = 0
            End Get
        End Property

        ''' <summary>
        ''' This determines the number of children to which this node points. All nodes of a particular 
        ''' type of tree have the same maximum degree, but most nodes will point to less than that 
        ''' number of children. The valid child pointers and keys appear at the beginning of the 
        ''' node and the unused pointers and keys appear at the end of the node. The unused pointers 
        ''' and keys have undefined values.
        ''' </summary>
        Public ReadOnly Property numberOfEntries As Integer

        ''' <summary>
        ''' Address of Left Sibling
        ''' 
        ''' This is the relative file address of the left sibling of the current node. If the current 
        ''' node is the left-most node at this level then this field is the undefined address.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property leftAddress As Long
        ''' <summary>
        ''' Address of Right Sibling
        ''' 
        ''' This is the relative file address of the right sibling of the current node. If the current 
        ''' node is the right-most node at this level then this field is the undefined address.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property rightAddress As Long

        Public Sub New(sb As Superblock, layout As Layout, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)
            Dim magic = [in].readBytes(4)

            Me.layout = layout
            Me.magic = Encoding.ASCII.GetString(magic)

            If Not Me.VerifyMagicSignature(signature) Then
                ' [in].offset -= 4
                ' Call "signature is not valid".Warning
                Call Console.WriteLine("data around 64 bytes nearby:")
                Call Console.WriteLine([in].debugView)

                Throw New IOException("signature is not valid: the block magic should be 'TREE'!")
            End If
            'Else
            ' read node type
            Me.type = [in].readByte
            ' read node level
            Me.level = [in].readByte()
            ' read entries used
            Me.numberOfEntries = [in].readShort()
            'End If

            Dim size As Long = 8 + 2 * sb.sizeOfOffsets + Me.numberOfEntries * (8 + sb.sizeOfOffsets + 8 + layout.numberOfDimensions)
            Dim isLast As Boolean
            Dim dc As DataChunk

            leftAddress = ReadHelper.readO([in], sb)
            rightAddress = ReadHelper.readO([in], sb)

            If Me.isLeaf Then
                ' read all entries as a DataChunk
                Me.entries = New List(Of DataChunk)()

                For i As Integer = 0 To Me.numberOfEntries
                    isLast = (i = Me.numberOfEntries)
                    dc = New DataChunk(sb, [in].offset, layout.numberOfDimensions, isLast)
                    entries.Add(dc)
                Next
            Else
                ' just track the offsets and node addresses
                Me.offsets = RectangularArray.Matrix(Of Integer)(Me.numberOfEntries + 1, layout.numberOfDimensions)
                Me.childPointer = New Long(Me.numberOfEntries) {}

                For i As Integer = 0 To Me.numberOfEntries
                    Dim key As Long = [in].readLong

                    ' skip size, filterMask
                    For j As Integer = 0 To layout.numberOfDimensions - 1
                        Me.offsets(i)(j) = CInt([in].readLong())
                    Next

                    Me.childPointer(i) = If((i = Me.numberOfEntries), -1, ReadHelper.readO([in], sb))
                Next
            End If
        End Sub

        ''' <summary>
        ''' this finds the first entry we dont want to skip.
        ''' entry i goes from [offset(i),offset(i+1))
        ''' we want to skip any entries we dont need, namely those where want >= offset(i+1)
        ''' so keep skipping until ``want &lt; offset(i+1)``
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <param name="sb"></param>
        Public Sub first([in] As BinaryReader, sb As Superblock)
            If Me.level = 0 Then

                ' note nentries-1 - assume dont skip the last one
                '                for (currentEntry = 0; currentEntry < nentries-1; currentEntry++) {
                '                	DataChunk entry = myEntries.get(currentEntry + 1);
                '                	if ((wantOrigin == null) || tiling.compare(wantOrigin, entry.offset) < 0) 
                '                		break;   // LOOK ??
                '                } 
                '                

                Me.currentEntry = 0
            Else
                Me.currentNode = Nothing
                Me.currentEntry = 0

                While Me.currentEntry < Me.numberOfEntries
                    Dim ptr& = Me.childPointer(Me.currentEntry)

                    If ptr > 0 Then
                        Me.currentNode = New BTreeNode(sb, Me.layout, ptr)
                        Me.currentNode.first([in], sb)
                        Exit While
                    End If

                    Me.currentEntry += 1
                End While

                ' heres the case where its the last entry we want; the tiling.compare() above may fail
                If Me.currentNode Is Nothing Then
                    Me.currentEntry = Me.numberOfEntries - 1
                    Me.currentNode = New BTreeNode(sb, Me.layout, Me.childPointer(Me.currentEntry))
                    Me.currentNode.first([in], sb)
                End If
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' LOOK - wouldnt be a bad idea to terminate if possible instead of running through all subsequent entries
        ''' </remarks>
        Public Function hasNext() As Boolean
            If Me.level = 0 Then
                Return (Me.currentEntry < Me.numberOfEntries)
            Else
                If Me.currentNode.hasNext() Then
                    Return True
                End If

                Return (Me.currentEntry < Me.numberOfEntries - 1)
            End If
        End Function

        Public Function [next]([in] As BinaryReader, sb As Superblock) As DataChunk
            If Me.level = 0 Then
                Return Me.entries(++Me.currentEntry)
            Else
                If Me.currentNode.hasNext() Then
                    Return Me.currentNode.[next]([in], sb)
                End If

                Me.currentEntry += 1
                Me.currentNode = New BTreeNode(sb, Me.layout, Me.childPointer(Me.currentEntry))
                Me.currentNode.first([in], sb)

                Return Me.currentNode.[next]([in], sb)
            End If
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Throw New NotImplementedException()
        End Sub
    End Class

End Namespace
