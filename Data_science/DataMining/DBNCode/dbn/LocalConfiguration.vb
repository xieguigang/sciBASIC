#Region "Microsoft.VisualBasic::507cd97007f1ff892e98d2800a8cde67, Data_science\DataMining\DBNCode\dbn\LocalConfiguration.vb"

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

    '   Total Lines: 195
    '    Code Lines: 117 (60.00%)
    ' Comment Lines: 46 (23.59%)
    '    - Xml Docs: 97.83%
    ' 
    '   Blank Lines: 32 (16.41%)
    '     File Size: 7.66 KB


    '     Class LocalConfiguration
    ' 
    '         Properties: ChildRange, ConsiderChild, NumParameters, ParentsRange
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: matches, nextChild, nextParents
    ' 
    '         Sub: resetChild, resetParents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace dbn

    Public Class LocalConfiguration
        Inherits Configuration

        Private parentIndices As Integer()

        ''' <summary>
        ''' If true, considers the child value when matching an observation with the
        ''' current configuration. In this case, N_{ijk} is what is being counted.
        ''' </summary>
        Private considerChildField As Boolean = True

        ''' <summary>
        ''' Allocates the configuration array and sets all parents and the child to
        ''' their first value. All input nodes must lie in the range
        ''' [0,attributes.size()[.
        ''' </summary>
        ''' <param name="attributes">
        '''            list of attributes characterizing the nodes </param>
        ''' <param name="parentNodesPast">
        '''            list of parent nodes in t </param>
        ''' <param name="parentNodesPresent">
        '''            list of parent nodes in t+1 </param>
        ''' <param name="childNode">
        '''            child node in t+1 </param>
        Public Sub New(attributes As IList(Of Attribute), markovLag As Integer, parentNodesPast As IList(Of Integer), parentNodesPresent As IList(Of Integer), childNode As Integer)
            MyBase.New(attributes, markovLag)
            reset()

            Dim n = attributes.Count
            Dim numParentsPast = If(parentNodesPast IsNot Nothing, parentNodesPast.Count, 0)
            Dim numParentsPresent = If(parentNodesPresent IsNot Nothing, parentNodesPresent.Count, 0)
            Dim numParents = numParentsPast + numParentsPresent

            parentIndices = New Integer(numParents - 1) {}
            Dim i = 0

            If parentNodesPast IsNot Nothing Then
                ' parentNodesPast ints are already shifted
                For Each parentNode As Integer? In parentNodesPast
                    parentIndices(std.Min(Threading.Interlocked.Increment(i), i - 1)) = parentNode.Value
                Next
            End If

            If parentNodesPresent IsNot Nothing Then
                For Each parentNode As Integer? In parentNodesPresent
                    parentIndices(std.Min(Threading.Interlocked.Increment(i), i - 1)) = parentNode.Value + markovLag * n
                Next
            End If

            resetParents()

            Me.childNode = childNode
            resetChild()
        End Sub

        Public Sub New(attributes As IList(Of Attribute), markovLag As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer)
            Me.New(attributes, markovLag, parentNodesPast, (If(parentNodePresent IsNot Nothing, New List(Of Integer) From {
                parentNodePresent
            }, Nothing)), childNode)
        End Sub

        Public Sub New(attributes As IList(Of Attribute), markovLag As Integer, parentNodes As IList(Of Integer), childNode As Integer)
            Me.New(attributes, markovLag, parentNodes, CType(Nothing, IList(Of Integer)), childNode)
        End Sub

        ''' <summary>
        ''' Sets whether the child value should be considered when matching an
        ''' observation with the current configuration.
        ''' </summary>
        ''' <param name="state"> </param>
        Public Overridable WriteOnly Property ConsiderChild As Boolean
            Set(value As Boolean)
                considerChildField = value
            End Set
        End Property

        Public Overridable Function matches(observation As Integer()) As Boolean

            Dim n = attributes.Count

            For i = 0 To configuration.Length - 1
                If configuration(i) > -1 Then
                    If observation(i) <> configuration(i) Then
                        If considerChildField OrElse i <> childNode + n * markovLag Then
                            Return False
                        End If
                    End If
                End If
            Next
            Return True
        End Function

        ''' <summary>
        ''' Updates the configuration of parents' values by incrementing the current
        ''' configuration in lexicographical order. If there isn't a new
        ''' configuration, resets the parents' values and returns false.
        ''' </summary>
        ''' <returns> true if a new parents' configuration is generated. </returns>
        Public Overridable Function nextParents() As Boolean

            Dim n = attributes.Count

            If parentIndices.Length = 0 Then
                resetParents()
                Return False
            End If

            For i = 0 To parentIndices.Length - 1
                If Threading.Interlocked.Increment(configuration(parentIndices(i))) < attributes(parentIndices(i) Mod n).size() Then
                    Exit For
                Else
                    configuration(parentIndices(i)) = 0
                    If i = parentIndices.Length - 1 Then
                        resetParents()
                        Return False
                    End If
                End If
            Next
            Return True
        End Function

        ''' <summary>
        ''' Sets all parents to their first value.
        ''' </summary>
        Public Overridable Sub resetParents()
            For i = 0 To parentIndices.Length - 1
                configuration(parentIndices(i)) = 0
            Next
        End Sub

        ''' <summary>
        ''' Increments the child value maintaining the current configuration of
        ''' parents' values. If there are no more child values, resets to the first
        ''' value and returns false.
        ''' </summary>
        ''' <returns> true if a new child value is generated. </returns>
        Public Overridable Function nextChild() As Boolean

            Dim n = attributes.Count

            If Threading.Interlocked.Increment(configuration(n * markovLag + childNode)) < attributes(childNode).size() Then
                Return True
            Else
                resetChild()
                Return False
            End If

        End Function

        ''' <summary>
        ''' Sets the child node to its first value.
        ''' </summary>
        Public Overridable Sub resetChild()
            Dim n = attributes.Count
            configuration(n * markovLag + childNode) = 0
        End Sub

        Public Overridable ReadOnly Property ParentsRange As Integer
            Get
                If parentIndices.Length = 0 Then
                    Return 0
                End If
                Dim n = attributes.Count
                Dim result = 1
                For i = 0 To parentIndices.Length - 1
                    result *= attributes(parentIndices(i) Mod n).size()
                Next
                Return result
            End Get
        End Property

        Public Overridable ReadOnly Property ChildRange As Integer
            Get
                Return attributes(childNode).size()
            End Get
        End Property

        ''' <summary>
        ''' Calculates the number of parameters required to specify a distribution,
        ''' according to the list of parents and the child.
        ''' </summary>
        ''' <returns> the number of parameters to specify </returns>
        Public Overridable ReadOnly Property NumParameters As Integer
            Get
                Return ParentsRange * (ChildRange - 1)
            End Get
        End Property

    End Class

End Namespace

