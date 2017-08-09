#Region "Microsoft.VisualBasic::a1bc2787d407848cfa11170d2cdd06dd, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\Graph\mxGraphModel.vb"

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

Imports System
Imports System.Collections.Generic

''' <summary>
''' $Id: mxGraphModel.java,v 1.1 2012/11/15 13:26:47 gaudenz Exp $
''' Copyright (c) 2007, Gaudenz Alder
''' </summary>
Namespace com.mxgraph.model



	''' <summary>
	''' Extends mxEventSource to implement a graph model. The graph model acts as
	''' a wrapper around the cells which are in charge of storing the actual graph
	''' datastructure. The model acts as a transactional wrapper with event
	''' notification for all changes, whereas the cells contain the atomic
	''' operations for updating the actual datastructure.
	''' 
	''' Layers:
	''' 
	''' The cell hierarchy in the model must have a top-level root cell which
	''' contains the layers (typically one default layer), which in turn contain the
	''' top-level cells of the layers. This means each cell is contained in a layer.
	''' If no layers are required, then all new cells should be added to the default
	''' layer.
	''' 
	''' Layers are useful for hiding and showing groups of cells, or for placing
	''' groups of cells on top of other cells in the display. To identify a layer,
	''' the <isLayer> function is used. It returns true if the parent of the given
	''' cell is the root of the model.
	''' 
	''' This class fires the following events:
	''' 
	''' mxEvent.CHANGE fires when an undoable edit is dispatched. The <code>edit</code>
	''' property contains the mxUndoableEdit. The <code>changes</code> property
	''' contains the list of undoable changes inside the undoable edit. The changes
	''' property is deprecated, please use edit.getChanges() instead.
	''' 
	''' mxEvent.EXECUTE fires between begin- and endUpdate and after an atomic
	''' change was executed in the model. The <code>change</code> property contains
	''' the atomic change that was executed.
	''' 
	''' mxEvent.BEGIN_UPDATE fires after the updateLevel was incremented in
	''' beginUpdate. This event contains no properties.
	''' 
	''' mxEvent.END_UPDATE fires after the updateLevel was decreased in endUpdate
	''' but before any notification or change dispatching. The <code>edit</code>
	''' property contains the current mxUndoableEdit.
	''' 
	''' mxEvent.BEFORE_UNDO fires before the change is dispatched after the update
	''' level has reached 0 in endUpdate. The <code>edit</code> property contains
	''' the current mxUndoableEdit.
	''' 
	''' mxEvent.UNDO fires after the change was dispatched in endUpdate. The
	''' <code>edit</code> property contains the current mxUndoableEdit.
	''' </summary>
	<Serializable> _
	Public Class mxGraphModel

        ''' <summary>
        ''' Holds the root cell, which in turn contains the cells that represent the
        ''' layers of the diagram as child cells. That is, the actual element of the
        ''' diagram are supposed to live in the third generation of cells and below.
        ''' </summary>
        Protected Friend root As mxICell

		''' <summary>
		''' Maps from Ids to cells.
		''' </summary>
		Protected Friend cells As IDictionary(Of String, Object)

		''' <summary>
		''' Specifies if edges should automatically be moved into the nearest common
		''' ancestor of their terminals. Default is true.
		''' </summary>
		Protected Friend maintainEdgeParent As Boolean = True

		''' <summary>
		''' Specifies if the model should automatically create Ids for new cells.
		''' Default is true.
		''' </summary>
		Protected Friend createIds As Boolean = True

		''' <summary>
		''' Specifies the next Id to be created. Initial value is 0.
		''' </summary>
		Protected Friend nextId As Integer = 0

		''' <summary>
		''' Holds the changes for the current transaction. If the transaction is
		''' closed then a new object is created for this variable using
		''' createUndoableEdit.
		''' </summary>
		<NonSerialized> _
		Protected Friend currentEdit As com.mxgraph.util.mxUndoableEdit

		''' <summary>
		''' Counter for the depth of nested transactions. Each call to beginUpdate
		''' increments this counter and each call to endUpdate decrements it. When
		''' the counter reaches 0, the transaction is closed and the respective
		''' events are fired. Initial value is 0.
		''' </summary>
		<NonSerialized> _
		Protected Friend updateLevel As Integer = 0

		''' 
		<NonSerialized> _
		Protected Friend endingUpdate As Boolean = False

		''' <summary>
		''' Constructs a new empty graph model.
		''' </summary>
		Public Sub New()
			Me.New(Nothing)
		End Sub

		''' <summary>
		''' Constructs a new graph model. If no root is specified
		''' then a new root mxCell with a default layer is created.
		''' </summary>
		''' <param name="root"> Cell that represents the root cell. </param>
		Public Sub New(ByVal root As Object)
			currentEdit = createUndoableEdit()

			If root IsNot Nothing Then
				Root = root
			Else
				clear()
			End If
		End Sub

		''' <summary>
		''' Sets a new root using createRoot.
		''' </summary>
		Public Overridable Sub clear()
			Root = createRoot()
		End Sub

		''' 
		Public Overridable Property UpdateLevel As Integer
			Get
				Return updateLevel
			End Get
		End Property

		''' <summary>
		''' Creates a new root cell with a default layer (child 0).
		''' </summary>
		Public Overridable Function createRoot() As Object
			Dim ___root As New mxCell
			___root.insert(New mxCell)

			Return ___root
		End Function

		''' <summary>
		''' Returns the internal lookup table that is used to map from Ids to cells.
		''' </summary>
		Public Overridable Property Cells As IDictionary(Of String, Object)
			Get
				Return cells
			End Get
		End Property

		''' <summary>
		''' Returns the cell for the specified Id or null if no cell can be
		''' found for the given Id.
		''' </summary>
		''' <param name="id"> A string representing the Id of the cell. </param>
		''' <returns> Returns the cell for the given Id. </returns>
		Public Overridable Function getCell(ByVal id As String) As Object
			Dim result As Object = Nothing

			If cells IsNot Nothing Then result = cells(id)
			Return result
		End Function

		''' <summary>
		''' Returns true if the model automatically update parents of edges so that
		''' the edge is contained in the nearest-common-ancestor of its terminals.
		''' </summary>
		''' <returns> Returns true if the model maintains edge parents. </returns>
		Public Overridable Property MaintainEdgeParent As Boolean
			Get
				Return maintainEdgeParent
			End Get
			Set(ByVal maintainEdgeParent As Boolean)
				Me.maintainEdgeParent = maintainEdgeParent
			End Set
		End Property


		''' <summary>
		''' Returns true if the model automatically creates Ids and resolves Id
		''' collisions.
		''' </summary>
		''' <returns> Returns true if the model creates Ids. </returns>
		Public Overridable Property CreateIds As Boolean
			Get
				Return createIds
			End Get
			Set(ByVal value As Boolean)
				createIds = value
			End Set
		End Property


	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getRoot()
	'	 
		Public Property Overrides Root As Object Implements mxIGraphModel.getRoot
			Get
				Return root
			End Get
		End Property

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setRoot(Object)
	'	 
		Public Overrides Function setRoot(ByVal root As Object) As Object Implements mxIGraphModel.setRoot
			execute(New mxRootChange(Me, root))

			Return root
		End Function

		''' <summary>
		''' Inner callback to change the root of the model and update the internal
		''' datastructures, such as cells and nextId. Returns the previous root.
		''' </summary>
		Protected Friend Overridable Function rootChanged(ByVal root As Object) As Object
			Dim oldRoot As Object = Me.root
			Me.root = CType(root, mxICell)

			' Resets counters and datastructures
			nextId = 0
			cells = Nothing
			cellAdded(root)

			Return oldRoot
		End Function

		''' <summary>
		''' Creates a new undoable edit.
		''' </summary>
		Protected Friend Overridable Function createUndoableEdit() As com.mxgraph.util.mxUndoableEdit
			Return New mxUndoableEditAnonymousInnerClassHelper
		End Function

		Private Class mxUndoableEditAnonymousInnerClassHelper
			Inherits com.mxgraph.util.mxUndoableEdit

			Public Overrides Sub dispatch()
				' LATER: Remove changes property (deprecated)
				CType(source, mxGraphModel).fireEvent(New com.mxgraph.util.mxEventObject(com.mxgraph.util.mxEvent.CHANGE, "edit", Me, "changes", changes))
			End Sub
		End Class

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#cloneCells(Object[], boolean)
	'	 
		Public Overrides Function cloneCells(ByVal cells As Object(), ByVal includeChildren As Boolean) As Object() Implements mxIGraphModel.cloneCells
			Dim mapping As IDictionary(Of Object, Object) = New Dictionary(Of Object, Object)
			Dim clones As Object() = New Object(cells.Length - 1){}

			For i As Integer = 0 To cells.Length - 1
				Try
					clones(i) = cloneCell(cells(i), mapping, includeChildren)
				Catch e As CloneNotSupportedException
					' ignore
				End Try
			Next

			For i As Integer = 0 To cells.Length - 1
				restoreClone(clones(i), cells(i), mapping)
			Next

			Return clones
		End Function

		''' <summary>
		''' Inner helper method for cloning cells recursively.
		''' </summary>
		Protected Friend Overridable Function cloneCell(ByVal cell As Object, ByVal mapping As IDictionary(Of Object, Object), ByVal includeChildren As Boolean) As Object
			If TypeOf cell Is mxICell Then
				Dim mxc As mxICell = CType(CType(cell, mxICell).clone(), mxICell)
				mapping(cell) = mxc

				If includeChildren Then
					Dim ___childCount As Integer = getChildCount(cell)

					For i As Integer = 0 To ___childCount - 1
						Dim clone As Object = cloneCell(getChildAt(cell, i), mapping, True)
						mxc.insert(CType(clone, mxICell))
					Next
				End If

				Return mxc
			End If

			Return Nothing
		End Function

		''' <summary>
		''' Inner helper method for restoring the connections in
		''' a network of cloned cells.
		''' </summary>
		Protected Friend Overridable Sub restoreClone(ByVal clone As Object, ByVal cell As Object, ByVal mapping As IDictionary(Of Object, Object))
			If TypeOf clone Is mxICell Then
				Dim mxc As mxICell = CType(clone, mxICell)
				Dim source As Object = getTerminal(cell, True)

				If TypeOf source Is mxICell Then
					Dim tmp As mxICell = CType(mapping(source), mxICell)

					If tmp IsNot Nothing Then tmp.insertEdge(mxc, True)
				End If

				Dim target As Object = getTerminal(cell, False)

				If TypeOf target Is mxICell Then
					Dim tmp As mxICell = CType(mapping(target), mxICell)

					If tmp IsNot Nothing Then tmp.insertEdge(mxc, False)
				End If
			End If

			Dim ___childCount As Integer = getChildCount(clone)

			For i As Integer = 0 To ___childCount - 1
				restoreClone(getChildAt(clone, i), getChildAt(cell, i), mapping)
			Next
		End Sub

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#isAncestor(Object, Object)
	'	 
		Public Overrides Function isAncestor(ByVal parent As Object, ByVal child As Object) As Boolean Implements mxIGraphModel.isAncestor
			Do While child IsNot Nothing AndAlso child IsNot parent
				child = getParent(child)
			Loop

			Return child Is parent
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#contains(Object)
	'	 
		Public Overrides Function contains(ByVal cell As Object) As Boolean Implements mxIGraphModel.contains
			Return isAncestor(Root, cell)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getParent(Object)
	'	 
		Public Overrides Function getParent(ByVal child As Object) As Object Implements mxIGraphModel.getParent
			Return If(TypeOf child Is mxICell, CType(child, mxICell).Parent, Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#add(Object, Object, int)
	'	 
		Public Overrides Function add(ByVal parent As Object, ByVal child As Object, ByVal index As Integer) As Object Implements mxIGraphModel.add
			If child IsNot parent AndAlso parent IsNot Nothing AndAlso child IsNot Nothing Then
				Dim parentChanged As Boolean = parent IsNot getParent(child)
				execute(New mxChildChange(Me, parent, child, index))

				' Maintains the edges parents by moving the edges
				' into the nearest common ancestor of its
				' terminals
				If maintainEdgeParent AndAlso parentChanged Then updateEdgeParents(child)
			End If

			Return child
		End Function

		''' <summary>
		''' Invoked after a cell has been added to a parent. This recursively
		''' creates an Id for the new cell and/or resolves Id collisions.
		''' </summary>
		''' <param name="cell"> Cell that has been added. </param>
		Protected Friend Overridable Sub cellAdded(ByVal cell As Object)
			If TypeOf cell Is mxICell Then
				Dim mxc As mxICell = CType(cell, mxICell)

				If mxc.Id Is Nothing AndAlso CreateIds Then mxc.Id = createId(cell)

				If mxc.Id IsNot Nothing Then
					Dim collision As Object = getCell(mxc.Id)

					If collision IsNot cell Then
						Do While collision IsNot Nothing
							mxc.Id = createId(cell)
							collision = getCell(mxc.Id)
						Loop

						If cells Is Nothing Then cells = New Dictionary(Of String, Object)

						cells(mxc.Id) = cell
					End If
				End If

				' Makes sure IDs of deleted cells are not reused
				Try
					Dim id As Integer = Convert.ToInt32(mxc.Id)
					nextId = Math.Max(nextId, id + 1)
				Catch e As NumberFormatException
					' ignore
				End Try

				Dim ___childCount As Integer = mxc.ChildCount

				For i As Integer = 0 To ___childCount - 1
					cellAdded(mxc.getChildAt(i))
				Next
			End If
		End Sub

		''' <summary>
		''' Creates a new Id for the given cell and increments the global counter
		''' for creating new Ids.
		''' </summary>
		''' <param name="cell"> Cell for which a new Id should be created. </param>
		''' <returns> Returns a new Id for the given cell. </returns>
		Public Overridable Function createId(ByVal cell As Object) As String
			Dim id As String = Convert.ToString(nextId)
			nextId += 1

			Return id
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#remove(Object)
	'	 
		Public Overrides Function remove(ByVal cell As Object) As Object Implements mxIGraphModel.remove
			If cell Is root Then
				Root = Nothing
			ElseIf getParent(cell) IsNot Nothing Then
				execute(New mxChildChange(Me, Nothing, cell))
			End If

			Return cell
		End Function

		''' <summary>
		''' Invoked after a cell has been removed from the model. This recursively
		''' removes the cell from its terminals and removes the mapping from the Id
		''' to the cell.
		''' </summary>
		''' <param name="cell"> Cell that has been removed. </param>
		Protected Friend Overridable Sub cellRemoved(ByVal cell As Object)
			If TypeOf cell Is mxICell Then
				Dim mxc As mxICell = CType(cell, mxICell)
				Dim ___childCount As Integer = mxc.ChildCount

				For i As Integer = 0 To ___childCount - 1
					cellRemoved(mxc.getChildAt(i))
				Next

				If cells IsNot Nothing AndAlso mxc.Id IsNot Nothing Then cells.Remove(mxc.Id)
			End If
		End Sub

		''' <summary>
		''' Inner callback to update the parent of a cell using mxCell.insert
		''' on the parent and return the previous parent.
		''' </summary>
		Protected Friend Overridable Function parentForCellChanged(ByVal cell As Object, ByVal parent As Object, ByVal index As Integer) As Object
			Dim child As mxICell = CType(cell, mxICell)
			Dim previous As mxICell = CType(getParent(cell), mxICell)

			If parent IsNot Nothing Then
				If parent IsNot previous OrElse previous.getIndex(child) <> index Then CType(parent, mxICell).insert(child, index)
			ElseIf previous IsNot Nothing Then
				Dim oldIndex As Integer = previous.getIndex(child)
				previous.remove(oldIndex)
			End If

			' Checks if the previous parent was already in the
			' model and avoids calling cellAdded if it was.
			If (Not contains(previous)) AndAlso parent IsNot Nothing Then
				cellAdded(cell)
			ElseIf parent Is Nothing Then
				cellRemoved(cell)
			End If

			Return previous
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getChildCount(Object)
	'	 
		Public Overrides Function getChildCount(ByVal cell As Object) As Integer Implements mxIGraphModel.getChildCount
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).ChildCount, 0)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getChildAt(Object, int)
	'	 
		Public Overrides Function getChildAt(ByVal parent As Object, ByVal index As Integer) As Object Implements mxIGraphModel.getChildAt
			Return If(TypeOf parent Is mxICell, CType(parent, mxICell).getChildAt(index), Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getTerminal(Object, boolean)
	'	 
		Public Overrides Function getTerminal(ByVal edge As Object, ByVal isSource As Boolean) As Object Implements mxIGraphModel.getTerminal
			Return If(TypeOf edge Is mxICell, CType(edge, mxICell).getTerminal(isSource), Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setTerminal(Object, Object, boolean)
	'	 
		Public Overrides Function setTerminal(ByVal edge As Object, ByVal terminal As Object, ByVal isSource As Boolean) As Object Implements mxIGraphModel.setTerminal
			Dim terminalChanged As Boolean = terminal IsNot getTerminal(edge, isSource)
			execute(New mxTerminalChange(Me, edge, terminal, isSource))

			If maintainEdgeParent AndAlso terminalChanged Then updateEdgeParent(edge, Root)

			Return terminal
		End Function

		''' <summary>
		''' Inner helper function to update the terminal of the edge using
		''' mxCell.insertEdge and return the previous terminal.
		''' </summary>
		Protected Friend Overridable Function terminalForCellChanged(ByVal edge As Object, ByVal terminal As Object, ByVal isSource As Boolean) As Object
			Dim previous As mxICell = CType(getTerminal(edge, isSource), mxICell)

			If terminal IsNot Nothing Then
				CType(terminal, mxICell).insertEdge(CType(edge, mxICell), isSource)
			ElseIf previous IsNot Nothing Then
				previous.removeEdge(CType(edge, mxICell), isSource)
			End If

			Return previous
		End Function

		''' <summary>
		''' Updates the parents of the edges connected to the given cell and all its
		''' descendants so that each edge is contained in the nearest common
		''' ancestor.
		''' </summary>
		''' <param name="cell"> Cell whose edges should be checked and updated. </param>
		Public Overridable Sub updateEdgeParents(ByVal cell As Object)
			updateEdgeParents(cell, Root)
		End Sub

		''' <summary>
		''' Updates the parents of the edges connected to the given cell and all its
		''' descendants so that the edge is contained in the nearest-common-ancestor.
		''' </summary>
		''' <param name="cell"> Cell whose edges should be checked and updated. </param>
		''' <param name="root"> Root of the cell hierarchy that contains all cells. </param>
		Public Overridable Sub updateEdgeParents(ByVal cell As Object, ByVal root As Object)
			' Updates edges on children first
			Dim ___childCount As Integer = getChildCount(cell)

			For i As Integer = 0 To ___childCount - 1
				Dim child As Object = getChildAt(cell, i)
				updateEdgeParents(child, root)
			Next

			' Updates the parents of all connected edges
			Dim ___edgeCount As Integer = getEdgeCount(cell)
			Dim ___edges As IList(Of Object) = New List(Of Object)(___edgeCount)

			For i As Integer = 0 To ___edgeCount - 1
				___edges.Add(getEdgeAt(cell, i))
			Next

			Dim it As IEnumerator(Of Object) = ___edges.GetEnumerator()

			Do While it.MoveNext()
				Dim ___edge As Object = it.Current

				' Updates edge parent if edge and child have
				' a common root node (does not need to be the
				' model root node)
				If isAncestor(root, ___edge) Then updateEdgeParent(___edge, root)
			Loop
		End Sub

		''' <summary>
		''' Inner helper method to update the parent of the specified edge to the
		''' nearest-common-ancestor of its two terminals.
		''' </summary>
		''' <param name="edge"> Specifies the edge to be updated. </param>
		''' <param name="root"> Current root of the model. </param>
		Public Overridable Sub updateEdgeParent(ByVal edge As Object, ByVal root As Object)
			Dim source As Object = getTerminal(edge, True)
			Dim target As Object = getTerminal(edge, False)
			Dim ___cell As Object = Nothing

			' Uses the first non-relative descendants of the source terminal
			Do While source IsNot Nothing AndAlso (Not isEdge(source)) AndAlso getGeometry(source) IsNot Nothing AndAlso getGeometry(source).Relative
				source = getParent(source)
			Loop

			' Uses the first non-relative descendants of the target terminal
			Do While target IsNot Nothing AndAlso (Not isEdge(target)) AndAlso getGeometry(target) IsNot Nothing AndAlso getGeometry(target).Relative
				target = getParent(target)
			Loop

			If isAncestor(root, source) AndAlso isAncestor(root, target) Then
				If source Is target Then
					___cell = getParent(source)
				Else
					___cell = getNearestCommonAncestor(source, target)
				End If

				' Keeps the edge in the same layer
				If ___cell IsNot Nothing AndAlso (getParent(___cell) IsNot root OrElse isAncestor(___cell, edge)) AndAlso getParent(edge) IsNot ___cell Then
					Dim geo As mxGeometry = getGeometry(edge)

					If geo IsNot Nothing Then
						Dim origin1 As com.mxgraph.util.mxPoint = getOrigin(getParent(edge))
						Dim origin2 As com.mxgraph.util.mxPoint = getOrigin(___cell)

						Dim dx As Double = origin2.X - origin1.X
						Dim dy As Double = origin2.Y - origin1.Y

						geo = CType(geo.clone(), mxGeometry)
						geo.translate(-dx, -dy)
						setGeometry(edge, geo)
					End If

					add(___cell, edge, getChildCount(___cell))
				End If
			End If
		End Sub

		''' <summary>
		''' Returns the absolute, accumulated origin for the children inside the
		''' given parent. 
		''' </summary>
		Public Overridable Function getOrigin(ByVal cell As Object) As com.mxgraph.util.mxPoint
			Dim result As com.mxgraph.util.mxPoint = Nothing

			If cell IsNot Nothing Then
				result = getOrigin(getParent(cell))

				If Not isEdge(cell) Then
					Dim geo As mxGeometry = getGeometry(cell)

					If geo IsNot Nothing Then
						result.X = result.X + geo.X
						result.Y = result.Y + geo.Y
					End If
				End If
			Else
				result = New com.mxgraph.util.mxPoint
			End If

			Return result
		End Function

		''' <summary>
		''' Returns the nearest common ancestor for the specified cells.
		''' </summary>
		''' <param name="cell1"> Cell that specifies the first cell in the tree. </param>
		''' <param name="cell2"> Cell that specifies the second cell in the tree. </param>
		''' <returns> Returns the nearest common ancestor of the given cells. </returns>
		Public Overridable Function getNearestCommonAncestor(ByVal cell1 As Object, ByVal cell2 As Object) As Object
			If cell1 IsNot Nothing AndAlso cell2 IsNot Nothing Then
				' Creates the cell path for the second cell
				Dim path As String = mxCellPath.create(CType(cell2, mxICell))

				If path IsNot Nothing AndAlso path.Length > 0 Then
					' Bubbles through the ancestors of the first
					' cell to find the nearest common ancestor.
					Dim ___cell As Object = cell1
					Dim current As String = mxCellPath.create(CType(___cell, mxICell))

					Do While ___cell IsNot Nothing
						Dim ___parent As Object = getParent(___cell)

						' Checks if the cell path is equal to the beginning
						' of the given cell path
						If path.IndexOf(current + mxCellPath.PATH_SEPARATOR) = 0 AndAlso ___parent IsNot Nothing Then Return ___cell

						current = mxCellPath.getParentPath(current)
						___cell = ___parent
					Loop
				End If
			End If

			Return Nothing
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getEdgeCount(Object)
	'	 
		Public Overrides Function getEdgeCount(ByVal cell As Object) As Integer Implements mxIGraphModel.getEdgeCount
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).EdgeCount, 0)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getEdgeAt(Object, int)
	'	 
		Public Overrides Function getEdgeAt(ByVal parent As Object, ByVal index As Integer) As Object Implements mxIGraphModel.getEdgeAt
			Return If(TypeOf parent Is mxICell, CType(parent, mxICell).getEdgeAt(index), Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#isVertex(Object)
	'	 
		Public Overrides Function isVertex(ByVal cell As Object) As Boolean Implements mxIGraphModel.isVertex
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Vertex, False)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#isEdge(Object)
	'	 
		Public Overrides Function isEdge(ByVal cell As Object) As Boolean Implements mxIGraphModel.isEdge
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Edge, False)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#isConnectable(Object)
	'	 
		Public Overrides Function isConnectable(ByVal cell As Object) As Boolean Implements mxIGraphModel.isConnectable
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Connectable, True)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getValue(Object)
	'	 
		Public Overrides Function getValue(ByVal cell As Object) As Object Implements mxIGraphModel.getValue
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Value, Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setValue(Object, Object)
	'	 
		Public Overrides Function setValue(ByVal cell As Object, ByVal value As Object) As Object Implements mxIGraphModel.setValue
			execute(New mxValueChange(Me, cell, value))

			Return value
		End Function

		''' <summary>
		''' Inner callback to update the user object of the given mxCell
		''' using mxCell.setValue and return the previous value,
		''' that is, the return value of mxCell.getValue.
		''' </summary>
		Protected Friend Overridable Function valueForCellChanged(ByVal cell As Object, ByVal value As Object) As Object
			Dim oldValue As Object = CType(cell, mxICell).Value
			CType(cell, mxICell).Value = value

			Return oldValue
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getGeometry(Object)
	'	 
		Public Overrides Function getGeometry(ByVal cell As Object) As mxGeometry Implements mxIGraphModel.getGeometry
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Geometry, Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setGeometry(Object, mxGeometry)
	'	 
		Public Overrides Function setGeometry(ByVal cell As Object, ByVal geometry As mxGeometry) As mxGeometry Implements mxIGraphModel.setGeometry
			If geometry IsNot getGeometry(cell) Then execute(New mxGeometryChange(Me, cell, geometry))

			Return geometry
		End Function

		''' <summary>
		''' Inner callback to update the mxGeometry of the given mxCell using
		''' mxCell.setGeometry and return the previous mxGeometry.
		''' </summary>
		Protected Friend Overridable Function geometryForCellChanged(ByVal cell As Object, ByVal geometry As mxGeometry) As mxGeometry
			Dim previous As mxGeometry = getGeometry(cell)
			CType(cell, mxICell).Geometry = geometry

			Return previous
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#getStyle(Object)
	'	 
		Public Overrides Function getStyle(ByVal cell As Object) As String Implements mxIGraphModel.getStyle
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Style, Nothing)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setStyle(Object, String)
	'	 
		Public Overrides Function setStyle(ByVal cell As Object, ByVal style As String) As String Implements mxIGraphModel.setStyle
			If style Is Nothing OrElse (Not style.Equals(getStyle(cell))) Then execute(New mxStyleChange(Me, cell, style))

			Return style
		End Function

		''' <summary>
		''' Inner callback to update the style of the given mxCell
		''' using mxCell.setStyle and return the previous style.
		''' </summary>
		Protected Friend Overridable Function styleForCellChanged(ByVal cell As Object, ByVal style As String) As String
			Dim previous As String = getStyle(cell)
			CType(cell, mxICell).Style = style

			Return previous
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#isCollapsed(Object)
	'	 
		Public Overrides Function isCollapsed(ByVal cell As Object) As Boolean Implements mxIGraphModel.isCollapsed
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Collapsed, False)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setCollapsed(Object, boolean)
	'	 
		Public Overrides Function setCollapsed(ByVal cell As Object, ByVal collapsed As Boolean) As Boolean Implements mxIGraphModel.setCollapsed
			If collapsed <> isCollapsed(cell) Then execute(New mxCollapseChange(Me, cell, collapsed))

			Return collapsed
		End Function

		''' <summary>
		''' Inner callback to update the collapsed state of the
		''' given mxCell using mxCell.setCollapsed and return
		''' the previous collapsed state.
		''' </summary>
		Protected Friend Overridable Function collapsedStateForCellChanged(ByVal cell As Object, ByVal collapsed As Boolean) As Boolean
			Dim previous As Boolean = isCollapsed(cell)
			CType(cell, mxICell).Collapsed = collapsed

			Return previous
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#isVisible(Object)
	'	 
		Public Overrides Function isVisible(ByVal cell As Object) As Boolean Implements mxIGraphModel.isVisible
			Return If(TypeOf cell Is mxICell, CType(cell, mxICell).Visible, False)
		End Function

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#setVisible(Object, boolean)
	'	 
		Public Overrides Function setVisible(ByVal cell As Object, ByVal visible As Boolean) As Boolean Implements mxIGraphModel.setVisible
			If visible <> isVisible(cell) Then execute(New mxVisibleChange(Me, cell, visible))

			Return visible
		End Function

		''' <summary>
		''' Sets the visible state of the given mxCell using mxVisibleChange and
		''' adds the change to the current transaction.
		''' </summary>
		Protected Friend Overridable Function visibleStateForCellChanged(ByVal cell As Object, ByVal visible As Boolean) As Boolean
			Dim previous As Boolean = isVisible(cell)
			CType(cell, mxICell).Visible = visible

			Return previous
		End Function

		''' <summary>
		''' Executes the given atomic change and adds it to the current edit.
		''' </summary>
		''' <param name="change"> Atomic change to be executed. </param>
		Public Overridable Sub execute(ByVal change As mxAtomicGraphModelChange)
			change.execute()
			beginUpdate()
			currentEdit.add(change)
			fireEvent(New com.mxgraph.util.mxEventObject(com.mxgraph.util.mxEvent.EXECUTE, "change", change))
			endUpdate()
		End Sub

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#beginUpdate()
	'	 
		Public Overrides Sub beginUpdate() Implements mxIGraphModel.beginUpdate
			updateLevel += 1
			fireEvent(New com.mxgraph.util.mxEventObject(com.mxgraph.util.mxEvent.BEGIN_UPDATE))
		End Sub

	'	 (non-Javadoc)
	'	 * @see com.mxgraph.model.mxIGraphModel#endUpdate()
	'	 
		Public Overrides Sub endUpdate() Implements mxIGraphModel.endUpdate
			updateLevel -= 1

			If Not endingUpdate Then
				endingUpdate = updateLevel = 0
				fireEvent(New com.mxgraph.util.mxEventObject(com.mxgraph.util.mxEvent.END_UPDATE, "edit", currentEdit))

				Try
					If endingUpdate AndAlso (Not currentEdit.Empty) Then
						fireEvent(New com.mxgraph.util.mxEventObject(com.mxgraph.util.mxEvent.BEFORE_UNDO, "edit", currentEdit))
						Dim tmp As com.mxgraph.util.mxUndoableEdit = currentEdit
						currentEdit = createUndoableEdit()
						tmp.dispatch()
						fireEvent(New com.mxgraph.util.mxEventObject(com.mxgraph.util.mxEvent.UNDO, "edit", tmp))
					End If
				Finally
					endingUpdate = False
				End Try
			End If
		End Sub

		''' <summary>
		''' Merges the children of the given cell into the given target cell inside
		''' this model. All cells are cloned unless there is a corresponding cell in
		''' the model with the same id, in which case the source cell is ignored and
		''' all edges are connected to the corresponding cell in this model. Edges
		''' are considered to have no identity and are always cloned unless the
		''' cloneAllEdges flag is set to false, in which case edges with the same
		''' id in the target model are reconnected to reflect the terminals of the
		''' source edges.
		''' </summary>
		''' <param name="from"> </param>
		''' <param name="to"> </param>
		''' <param name="cloneAllEdges"> </param>
		Public Overridable Sub mergeChildren(ByVal [from] As mxICell, ByVal [to] As mxICell, ByVal cloneAllEdges As Boolean)
			beginUpdate()
			Try
				Dim mapping As New Dictionary(Of Object, Object)
				mergeChildrenImpl([from], [to], cloneAllEdges, mapping)

				' Post-processes all edges in the mapping and
				' reconnects the terminals to the corresponding
				' cells in the target model
				Dim it As IEnumerator(Of Object) = mapping.Keys.GetEnumerator()

				Do While it.MoveNext()
					Dim ___edge As Object = it.Current
					Dim ___cell As Object = mapping(___edge)
					Dim ___terminal As Object = getTerminal(___edge, True)

					If ___terminal IsNot Nothing Then
						___terminal = mapping(___terminal)
						setTerminal(___cell, ___terminal, True)
					End If

					___terminal = getTerminal(___edge, False)

					If ___terminal IsNot Nothing Then
						___terminal = mapping(___terminal)
						setTerminal(___cell, ___terminal, False)
					End If
				Loop
			Finally
				endUpdate()
			End Try
		End Sub

		''' <summary>
		''' Clones the children of the source cell into the given target cell in
		''' this model and adds an entry to the mapping that maps from the source
		''' cell to the target cell with the same id or the clone of the source cell
		''' that was inserted into this model.
		''' </summary>
		Protected Friend Overridable Sub mergeChildrenImpl(ByVal [from] As mxICell, ByVal [to] As mxICell, ByVal cloneAllEdges As Boolean, ByVal mapping As Dictionary(Of Object, Object))
			beginUpdate()
			Try
				Dim ___childCount As Integer = [from].ChildCount

				For i As Integer = 0 To ___childCount - 1
					Dim ___cell As mxICell = [from].getChildAt(i)
					Dim id As String = ___cell.Id
					Dim target As mxICell = CType(If(id IsNot Nothing AndAlso ((Not isEdge(___cell)) OrElse (Not cloneAllEdges)), getCell(id), Nothing), mxICell)

					' Clones and adds the child if no cell exists for the id
					If target Is Nothing Then
						Dim clone As mxCell = CType(___cell.clone(), mxCell)
						clone.Id = id

						' Do *NOT* use model.add as this will move the edge away
						' from the parent in updateEdgeParent if maintainEdgeParent
						' is enabled in the target model
						target = [to].insert(clone)
						cellAdded(target)
					End If

					' Stores the mapping for later reconnecting edges
					mapping(___cell) = target

					' Recurses
					mergeChildrenImpl(___cell, target, cloneAllEdges, mapping)
				Next
			Finally
				endUpdate()
			End Try
		End Sub

		''' <summary>
		''' Initializes the currentEdit field if the model is deserialized.
		''' </summary>
		Private Sub readObject(ByVal ois As java.io.ObjectInputStream)
			ois.defaultReadObject()
			currentEdit = createUndoableEdit()
		End Sub

		''' <summary>
		''' Returns the number of incoming or outgoing edges.
		''' </summary>
		''' <param name="model"> Graph model that contains the connection data. </param>
		''' <param name="cell"> Cell whose edges should be counted. </param>
		''' <param name="outgoing"> Boolean that specifies if the number of outgoing or
		''' incoming edges should be returned. </param>
		''' <returns> Returns the number of incoming or outgoing edges. </returns>
		Public Shared Function getDirectedEdgeCount(ByVal model As mxIGraphModel, ByVal cell As Object, ByVal outgoing As Boolean) As Integer
			Return getDirectedEdgeCount(model, cell, outgoing, Nothing)
		End Function

		''' <summary>
		''' Returns the number of incoming or outgoing edges, ignoring the given
		''' edge.
		''' </summary>
		''' <param name="model"> Graph model that contains the connection data. </param>
		''' <param name="cell"> Cell whose edges should be counted. </param>
		''' <param name="outgoing"> Boolean that specifies if the number of outgoing or
		''' incoming edges should be returned. </param>
		''' <param name="ignoredEdge"> Object that represents an edge to be ignored. </param>
		''' <returns> Returns the number of incoming or outgoing edges. </returns>
		Public Shared Function getDirectedEdgeCount(ByVal model As mxIGraphModel, ByVal cell As Object, ByVal outgoing As Boolean, ByVal ignoredEdge As Object) As Integer
			Dim count As Integer = 0
			Dim ___edgeCount As Integer = model.getEdgeCount(cell)

			For i As Integer = 0 To ___edgeCount - 1
				Dim ___edge As Object = model.getEdgeAt(cell, i)

				If ___edge IsNot ignoredEdge AndAlso model.getTerminal(___edge, outgoing) Is cell Then count += 1
			Next

			Return count
		End Function

		''' <summary>
		''' Returns all edges connected to this cell including loops.
		''' </summary>
		''' <param name="model"> Model that contains the connection information. </param>
		''' <param name="cell"> Cell whose connections should be returned. </param>
		''' <returns> Returns the array of connected edges for the given cell. </returns>
		Public Shared Function getEdges(ByVal model As mxIGraphModel, ByVal cell As Object) As Object()
			Return getEdges(model, cell, True, True, True)
		End Function

		''' <summary>
		''' Returns all edges connected to this cell without loops.
		''' </summary>
		''' <param name="model"> Model that contains the connection information. </param>
		''' <param name="cell"> Cell whose connections should be returned. </param>
		''' <returns> Returns the connected edges for the given cell. </returns>
		Public Shared Function getConnections(ByVal model As mxIGraphModel, ByVal cell As Object) As Object()
			Return getEdges(model, cell, True, True, False)
		End Function

		''' <summary>
		''' Returns the incoming edges of the given cell without loops.
		''' </summary>
		''' <param name="model"> Graphmodel that contains the edges. </param>
		''' <param name="cell"> Cell whose incoming edges should be returned. </param>
		''' <returns> Returns the incoming edges for the given cell. </returns>
		Public Shared Function getIncomingEdges(ByVal model As mxIGraphModel, ByVal cell As Object) As Object()
			Return getEdges(model, cell, True, False, False)
		End Function

		''' <summary>
		''' Returns the outgoing edges of the given cell without loops.
		''' </summary>
		''' <param name="model"> Graphmodel that contains the edges. </param>
		''' <param name="cell"> Cell whose outgoing edges should be returned. </param>
		''' <returns> Returns the outgoing edges for the given cell. </returns>
		Public Shared Function getOutgoingEdges(ByVal model As mxIGraphModel, ByVal cell As Object) As Object()
			Return getEdges(model, cell, False, True, False)
		End Function

		''' <summary>
		''' Returns all distinct edges connected to this cell.
		''' </summary>
		''' <param name="model"> Model that contains the connection information. </param>
		''' <param name="cell"> Cell whose connections should be returned. </param>
		''' <param name="incoming"> Specifies if incoming edges should be returned. </param>
		''' <param name="outgoing"> Specifies if outgoing edges should be returned. </param>
		''' <param name="includeLoops"> Specifies if loops should be returned. </param>
		''' <returns> Returns the array of connected edges for the given cell. </returns>
		Public Shared Function getEdges(ByVal model As mxIGraphModel, ByVal cell As Object, ByVal incoming As Boolean, ByVal outgoing As Boolean, ByVal includeLoops As Boolean) As Object()
			Dim ___edgeCount As Integer = model.getEdgeCount(cell)
			Dim result As IList(Of Object) = New List(Of Object)(___edgeCount)

			For i As Integer = 0 To ___edgeCount - 1
				Dim ___edge As Object = model.getEdgeAt(cell, i)
				Dim source As Object = model.getTerminal(___edge, True)
				Dim target As Object = model.getTerminal(___edge, False)

				If (includeLoops AndAlso source Is target) OrElse ((source IsNot target) AndAlso ((incoming AndAlso target Is cell) OrElse (outgoing AndAlso source Is cell))) Then result.Add(___edge)
			Next

			Return result.ToArray()
		End Function

		''' <summary>
		''' Returns all edges from the given source to the given target.
		''' </summary>
		''' <param name="model"> The graph model that contains the graph. </param>
		''' <param name="source"> Object that defines the source cell. </param>
		''' <param name="target"> Object that defines the target cell. </param>
		''' <returns> Returns all edges from source to target. </returns>
		Public Shared Function getEdgesBetween(ByVal model As mxIGraphModel, ByVal source As Object, ByVal target As Object) As Object()
			Return getEdgesBetween(model, source, target, False)
		End Function

		''' <summary>
		''' Returns all edges between the given source and target pair. If directed
		''' is true, then only edges from the source to the target are returned,
		''' otherwise, all edges between the two cells are returned.
		''' </summary>
		''' <param name="model"> The graph model that contains the graph. </param>
		''' <param name="source"> Object that defines the source cell. </param>
		''' <param name="target"> Object that defines the target cell. </param>
		''' <param name="directed"> Boolean that specifies if the direction of the edge
		''' should be taken into account. </param>
		''' <returns> Returns all edges between the given source and target. </returns>
		Public Shared Function getEdgesBetween(ByVal model As mxIGraphModel, ByVal source As Object, ByVal target As Object, ByVal directed As Boolean) As Object()
			Dim tmp1 As Integer = model.getEdgeCount(source)
			Dim tmp2 As Integer = model.getEdgeCount(target)

			' Assumes the source has less connected edges
			Dim ___terminal As Object = source
			Dim ___edgeCount As Integer = tmp1

			' Uses the smaller array of connected edges
			' for searching the edge
			If tmp2 < tmp1 Then
				___edgeCount = tmp2
				___terminal = target
			End If

			Dim result As IList(Of Object) = New List(Of Object)(___edgeCount)

			' Checks if the edge is connected to the correct
			' cell and returns the first match
			For i As Integer = 0 To ___edgeCount - 1
				Dim ___edge As Object = model.getEdgeAt(___terminal, i)
				Dim src As Object = model.getTerminal(___edge, True)
				Dim trg As Object = model.getTerminal(___edge, False)
				Dim directedMatch As Boolean = (src Is source) AndAlso (trg Is target)
				Dim oppositeMatch As Boolean = (trg Is source) AndAlso (src Is target)

				If directedMatch OrElse ((Not directed) AndAlso oppositeMatch) Then result.Add(___edge)
			Next

			Return result.ToArray()
		End Function

		''' <summary>
		''' Returns all opposite cells of terminal for the given edges.
		''' </summary>
		''' <param name="model"> Model that contains the connection information. </param>
		''' <param name="edges"> Array of edges to be examined. </param>
		''' <param name="terminal"> Cell that specifies the known end of the edges. </param>
		''' <returns> Returns the opposite cells of the given terminal. </returns>
		Public Shared Function getOpposites(ByVal model As mxIGraphModel, ByVal edges As Object(), ByVal terminal As Object) As Object()
			Return getOpposites(model, edges, terminal, True, True)
		End Function

		''' <summary>
		''' Returns all opposite vertices wrt terminal for the given edges, only
		''' returning sources and/or targets as specified. The result is returned as
		''' an array of mxCells.
		''' </summary>
		''' <param name="model"> Model that contains the connection information. </param>
		''' <param name="edges"> Array of edges to be examined. </param>
		''' <param name="terminal"> Cell that specifies the known end of the edges. </param>
		''' <param name="sources"> Boolean that specifies if source terminals should
		''' be contained in the result. Default is true. </param>
		''' <param name="targets"> Boolean that specifies if target terminals should
		''' be contained in the result. Default is true. </param>
		''' <returns> Returns the array of opposite terminals for the given edges. </returns>
		Public Shared Function getOpposites(ByVal model As mxIGraphModel, ByVal edges As Object(), ByVal terminal As Object, ByVal sources As Boolean, ByVal targets As Boolean) As Object()
			Dim ___terminals As IList(Of Object) = New List(Of Object)

			If edges IsNot Nothing Then
				For i As Integer = 0 To edges.Length - 1
					Dim source As Object = model.getTerminal(edges(i), True)
					Dim target As Object = model.getTerminal(edges(i), False)

					' Checks if the terminal is the source of
					' the edge and if the target should be
					' stored in the result
					If targets AndAlso source Is terminal AndAlso target IsNot Nothing AndAlso target IsNot terminal Then
						___terminals.Add(target)

					' Checks if the terminal is the taget of
					' the edge and if the source should be
					' stored in the result
					ElseIf sources AndAlso target Is terminal AndAlso source IsNot Nothing AndAlso source IsNot terminal Then
						___terminals.Add(source)
					End If
				Next
			End If

			Return ___terminals.ToArray()
		End Function

		''' <summary>
		''' Sets the source and target of the given edge in a single atomic change.
		''' </summary>
		''' <param name="edge"> Cell that specifies the edge. </param>
		''' <param name="source"> Cell that specifies the new source terminal. </param>
		''' <param name="target"> Cell that specifies the new target terminal. </param>
		Public Shared Sub setTerminals(ByVal model As mxIGraphModel, ByVal edge As Object, ByVal source As Object, ByVal target As Object)
			model.beginUpdate()
			Try
				model.setTerminal(edge, source, True)
				model.setTerminal(edge, target, False)
			Finally
				model.endUpdate()
			End Try
		End Sub

		''' <summary>
		''' Returns all children of the given cell regardless of their type.
		''' </summary>
		''' <param name="model"> Model that contains the hierarchical information. </param>
		''' <param name="parent"> Cell whose child vertices or edges should be returned. </param>
		''' <returns> Returns the child vertices and/or edges of the given parent. </returns>
		Public Shared Function getChildren(ByVal model As mxIGraphModel, ByVal parent As Object) As Object()
			Return getChildCells(model, parent, False, False)
		End Function

		''' <summary>
		''' Returns the child vertices of the given parent.
		''' </summary>
		''' <param name="model"> Model that contains the hierarchical information. </param>
		''' <param name="parent"> Cell whose child vertices should be returned. </param>
		''' <returns> Returns the child vertices of the given parent. </returns>
		Public Shared Function getChildVertices(ByVal model As mxIGraphModel, ByVal parent As Object) As Object()
			Return getChildCells(model, parent, True, False)
		End Function

		''' <summary>
		''' Returns the child edges of the given parent.
		''' </summary>
		''' <param name="model"> Model that contains the hierarchical information. </param>
		''' <param name="parent"> Cell whose child edges should be returned. </param>
		''' <returns> Returns the child edges of the given parent. </returns>
		Public Shared Function getChildEdges(ByVal model As mxIGraphModel, ByVal parent As Object) As Object()
			Return getChildCells(model, parent, False, True)
		End Function

		''' <summary>
		''' Returns the children of the given cell that are vertices and/or edges
		''' depending on the arguments. If both arguments are false then all
		''' children are returned regardless of their type.
		''' </summary>
		''' <param name="model"> Model that contains the hierarchical information. </param>
		''' <param name="parent"> Cell whose child vertices or edges should be returned. </param>
		''' <param name="vertices"> Boolean indicating if child vertices should be returned. </param>
		''' <param name="edges"> Boolean indicating if child edges should be returned. </param>
		''' <returns> Returns the child vertices and/or edges of the given parent. </returns>
		Public Shared Function getChildCells(ByVal model As mxIGraphModel, ByVal parent As Object, ByVal vertices As Boolean, ByVal edges As Boolean) As Object()
			Dim ___childCount As Integer = model.getChildCount(parent)
			Dim result As IList(Of Object) = New List(Of Object)(___childCount)

			For i As Integer = 0 To ___childCount - 1
				Dim child As Object = model.getChildAt(parent, i)

				If ((Not edges) AndAlso (Not vertices)) OrElse (edges AndAlso model.isEdge(child)) OrElse (vertices AndAlso model.isVertex(child)) Then result.Add(child)
			Next

			Return result.ToArray()
		End Function

		''' 
		Public Shared Function getParents(ByVal model As mxIGraphModel, ByVal cells As Object()) As Object()
			Dim ___parents As New HashSet(Of Object)

			If cells IsNot Nothing Then
				For i As Integer = 0 To cells.Length - 1
					Dim ___parent As Object = model.getParent(cells(i))

					If ___parent IsNot Nothing Then ___parents.Add(___parent)
				Next
			End If

			Return ___parents.ToArray()
		End Function

		''' 
		Public Shared Function filterCells(ByVal cells As Object(), ByVal filter As Filter) As Object()
			Dim result As List(Of Object) = Nothing

			If cells IsNot Nothing Then
				result = New List(Of Object)(cells.Length)

				For i As Integer = 0 To cells.Length - 1
					If filter.filter(cells(i)) Then result.Add(cells(i))
				Next
			End If

			Return If(result IsNot Nothing, result.ToArray(), Nothing)
		End Function

		''' <summary>
		''' Returns a all descendants of the given cell and the cell itself
		''' as a collection.
		''' </summary>
		Public Shared Function getDescendants(ByVal model As mxIGraphModel, ByVal parent As Object) As ICollection(Of Object)
			Return filterDescendants(model, Nothing, parent)
		End Function

		''' <summary>
		''' Creates a collection of cells using the visitor pattern.
		''' </summary>
		Public Shared Function filterDescendants(ByVal model As mxIGraphModel, ByVal filter As Filter) As ICollection(Of Object)
			Return filterDescendants(model, filter, model.Root)
		End Function

		''' <summary>
		''' Creates a collection of cells using the visitor pattern.
		''' </summary>
		Public Shared Function filterDescendants(ByVal model As mxIGraphModel, ByVal filter As Filter, ByVal parent As Object) As ICollection(Of Object)
			Dim result As IList(Of Object) = New List(Of Object)

			If filter Is Nothing OrElse filter.filter(parent) Then result.Add(parent)

			Dim ___childCount As Integer = model.getChildCount(parent)

			For i As Integer = 0 To ___childCount - 1
				Dim child As Object = model.getChildAt(parent, i)
				result.AddRange(filterDescendants(model, filter, child))
			Next

			Return result
		End Function

		''' <summary>
		''' Function: getTopmostCells
		''' 
		''' Returns the topmost cells of the hierarchy in an array that contains no
		''' desceandants for each <mxCell> that it contains. Duplicates should be
		''' removed in the cells array to improve performance.
		''' 
		''' Parameters:
		''' 
		''' cells - Array of <mxCells> whose topmost ancestors should be returned.
		''' </summary>
		Public Shared Function getTopmostCells(ByVal model As mxIGraphModel, ByVal cells As Object()) As Object()
			Dim hash As java.util.Set(Of Object) = New HashSet(Of Object)
			hash.addAll(java.util.Arrays.asList(cells))
			Dim result As IList(Of Object) = New List(Of Object)(cells.Length)

			For i As Integer = 0 To cells.Length - 1
				Dim ___cell As Object = cells(i)
				Dim topmost As Boolean = True
				Dim ___parent As Object = model.getParent(___cell)

				Do While ___parent IsNot Nothing
					If hash.contains(___parent) Then
						topmost = False
						Exit Do
					End If

					___parent = model.getParent(___parent)
				Loop

				If topmost Then result.Add(___cell)
			Next

			Return result.ToArray()
		End Function

		'
		' Visitor patterns
		'

		''' 
		Public Interface Filter

			''' 
			Function filter(ByVal cell As Object) As Boolean
		End Interface

		'
		' Atomic changes
		'

		Public Class mxRootChange
			Inherits mxAtomicGraphModelChange

			''' <summary>
			''' Holds the new and previous root cell.
			''' </summary>
			Protected Friend root, previous As Object

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal root As Object)
				MyBase.New(model)
				Me.root = root
				previous = root
			End Sub

			''' 
			Public Overridable Property Root As Object
				Set(ByVal value As Object)
					root = value
				End Set
				Get
					Return root
				End Get
			End Property


			''' 
			Public Overridable Property Previous As Object
				Set(ByVal value As Object)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				root = previous
				previous = CType(model, mxGraphModel).rootChanged(previous)
			End Sub

		End Class

		Public Class mxChildChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend parent, previous, child As Object

			''' 
			Protected Friend index, previousIndex As Integer

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, Nothing, 0)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal parent As Object, ByVal child As Object)
				Me.New(model, parent, child, 0)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal parent As Object, ByVal child As Object, ByVal index As Integer)
				MyBase.New(model)
				Me.parent = parent
				previous = Me.parent
				Me.child = child
				Me.index = index
				previousIndex = index
			End Sub

			''' 
			Public Overridable Property Parent As Object
				Set(ByVal value As Object)
					parent = value
				End Set
				Get
					Return parent
				End Get
			End Property


			''' 
			Public Overridable Property Previous As Object
				Set(ByVal value As Object)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' 
			Public Overridable Property Child As Object
				Set(ByVal value As Object)
					child = value
				End Set
				Get
					Return child
				End Get
			End Property


			''' 
			Public Overridable Property Index As Integer
				Set(ByVal value As Integer)
					index = value
				End Set
				Get
					Return index
				End Get
			End Property


			''' 
			Public Overridable Property PreviousIndex As Integer
				Set(ByVal value As Integer)
					previousIndex = value
				End Set
				Get
					Return previousIndex
				End Get
			End Property


			''' <summary>
			''' Gets the source or target terminal field for the given
			''' edge even if the edge is not stored as an incoming or
			''' outgoing edge in the respective terminal.
			''' </summary>
			Protected Friend Overridable Function getTerminal(ByVal edge As Object, ByVal source As Boolean) As Object
				Return model.getTerminal(edge, source)
			End Function

			''' <summary>
			''' Sets the source or target terminal field for the given edge
			''' without inserting an incoming or outgoing edge in the
			''' respective terminal.
			''' </summary>
			Protected Friend Overridable Sub setTerminal(ByVal edge As Object, ByVal terminal As Object, ByVal source As Boolean)
				CType(edge, mxICell).setTerminal(CType(terminal, mxICell), source)
			End Sub

			''' 
			Protected Friend Overridable Sub connect(ByVal cell As Object, ByVal isConnect As Boolean)
				Dim source As Object = getTerminal(cell, True)
				Dim target As Object = getTerminal(cell, False)

				If source IsNot Nothing Then
					If isConnect Then
						CType(model, mxGraphModel).terminalForCellChanged(cell, source, True)
					Else
						CType(model, mxGraphModel).terminalForCellChanged(cell, Nothing, True)
					End If
				End If

				If target IsNot Nothing Then
					If isConnect Then
						CType(model, mxGraphModel).terminalForCellChanged(cell, target, False)
					Else
						CType(model, mxGraphModel).terminalForCellChanged(cell, Nothing, False)
					End If
				End If

				' Stores the previous terminals in the edge
				setTerminal(cell, source, True)
				setTerminal(cell, target, False)

				Dim childCount As Integer = model.getChildCount(cell)

				For i As Integer = 0 To childCount - 1
					connect(model.getChildAt(cell, i), isConnect)
				Next
			End Sub

			''' <summary>
			''' Returns the index of the given child inside the given parent.
			''' </summary>
			Protected Friend Overridable Function getChildIndex(ByVal parent As Object, ByVal child As Object) As Integer
				Return If(TypeOf parent Is mxICell AndAlso TypeOf child Is mxICell, CType(parent, mxICell).getIndex(CType(child, mxICell)), 0)
			End Function

			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				Dim tmp As Object = model.getParent(child)
				Dim tmp2 As Integer = getChildIndex(tmp, child)

				If previous Is Nothing Then connect(child, False)

				tmp = CType(model, mxGraphModel).parentForCellChanged(child, previous, previousIndex)

				If previous IsNot Nothing Then connect(child, True)

				parent = previous
				previous = tmp
				index = previousIndex
				previousIndex = tmp2
			End Sub

		End Class

		Public Class mxTerminalChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend cell, terminal, previous As Object

			''' 
			Protected Friend source As Boolean

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, Nothing, False)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal cell As Object, ByVal terminal As Object, ByVal source As Boolean)
				MyBase.New(model)
				Me.cell = cell
				Me.terminal = terminal
				Me.previous = Me.terminal
				Me.source = source
			End Sub

			''' 
			Public Overridable Property Cell As Object
				Set(ByVal value As Object)
					cell = value
				End Set
				Get
					Return cell
				End Get
			End Property


			''' 
			Public Overridable Property Terminal As Object
				Set(ByVal value As Object)
					terminal = value
				End Set
				Get
					Return terminal
				End Get
			End Property


			''' 
			Public Overridable Property Previous As Object
				Set(ByVal value As Object)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' 
			Public Overridable Property Source As Boolean
				Set(ByVal value As Boolean)
					source = value
				End Set
				Get
					Return source
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				terminal = previous
				previous = CType(model, mxGraphModel).terminalForCellChanged(cell, previous, source)
			End Sub

		End Class

		Public Class mxValueChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend cell, value, previous As Object

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, Nothing)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal cell As Object, ByVal value As Object)
				MyBase.New(model)
				Me.cell = cell
				Me.value = value
				Me.previous = Me.value
			End Sub

			''' 
			Public Overridable Property Cell As Object
				Set(ByVal value As Object)
					cell = value
				End Set
				Get
					Return cell
				End Get
			End Property


			''' 
			Public Overridable Property Value As Object
				Set(ByVal value As Object)
					Me.value = value
				End Set
				Get
					Return value
				End Get
			End Property


			''' 
			Public Overridable Property Previous As Object
				Set(ByVal value As Object)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				value = previous
				previous = CType(model, mxGraphModel).valueForCellChanged(cell, previous)
			End Sub

		End Class

		Public Class mxStyleChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend cell As Object

			''' 
			Protected Friend style, previous As String

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, Nothing)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal cell As Object, ByVal style As String)
				MyBase.New(model)
				Me.cell = cell
				Me.style = style
				Me.previous = Me.style
			End Sub

			''' 
			Public Overridable Property Cell As Object
				Set(ByVal value As Object)
					cell = value
				End Set
				Get
					Return cell
				End Get
			End Property


			''' 
			Public Overridable Property Style As String
				Set(ByVal value As String)
					style = value
				End Set
				Get
					Return style
				End Get
			End Property


			''' 
			Public Overridable Property Previous As String
				Set(ByVal value As String)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				style = previous
				previous = CType(model, mxGraphModel).styleForCellChanged(cell, previous)
			End Sub

		End Class

		Public Class mxGeometryChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend cell As Object

			''' 
			Protected Friend geometry, previous As mxGeometry

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, Nothing)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal cell As Object, ByVal geometry As mxGeometry)
				MyBase.New(model)
				Me.cell = cell
				Me.geometry = geometry
				Me.previous = Me.geometry
			End Sub

			''' 
			Public Overridable Property Cell As Object
				Set(ByVal value As Object)
					cell = value
				End Set
				Get
					Return cell
				End Get
			End Property


			''' 
			Public Overridable Property Geometry As mxGeometry
				Set(ByVal value As mxGeometry)
					geometry = value
				End Set
				Get
					Return geometry
				End Get
			End Property


			''' 
			Public Overridable Property Previous As mxGeometry
				Set(ByVal value As mxGeometry)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				geometry = previous
				previous = CType(model, mxGraphModel).geometryForCellChanged(cell, previous)
			End Sub

		End Class

		Public Class mxCollapseChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend cell As Object

			''' 
			Protected Friend collapsed, previous As Boolean

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, False)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal cell As Object, ByVal collapsed As Boolean)
				MyBase.New(model)
				Me.cell = cell
				Me.collapsed = collapsed
				Me.previous = Me.collapsed
			End Sub

			''' 
			Public Overridable Property Cell As Object
				Set(ByVal value As Object)
					cell = value
				End Set
				Get
					Return cell
				End Get
			End Property


			''' 
			Public Overridable Property Collapsed As Boolean
				Set(ByVal value As Boolean)
					collapsed = value
				End Set
				Get
					Return collapsed
				End Get
			End Property


			''' 
			Public Overridable Property Previous As Boolean
				Set(ByVal value As Boolean)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				collapsed = previous
				previous = CType(model, mxGraphModel).collapsedStateForCellChanged(cell, previous)
			End Sub

		End Class

		Public Class mxVisibleChange
			Inherits mxAtomicGraphModelChange

			''' 
			Protected Friend cell As Object

			''' 
			Protected Friend visible, previous As Boolean

			''' 
			Public Sub New()
				Me.New(Nothing, Nothing, False)
			End Sub

			''' 
			Public Sub New(ByVal model As mxGraphModel, ByVal cell As Object, ByVal visible As Boolean)
				MyBase.New(model)
				Me.cell = cell
				Me.visible = visible
				Me.previous = Me.visible
			End Sub

			''' 
			Public Overridable Property Cell As Object
				Set(ByVal value As Object)
					cell = value
				End Set
				Get
					Return cell
				End Get
			End Property


			''' 
			Public Overridable Property Visible As Boolean
				Set(ByVal value As Boolean)
					visible = value
				End Set
				Get
					Return visible
				End Get
			End Property


			''' 
			Public Overridable Property Previous As Boolean
				Set(ByVal value As Boolean)
					previous = value
				End Set
				Get
					Return previous
				End Get
			End Property


			''' <summary>
			''' Changes the root of the model.
			''' </summary>
			Public Overrides Sub execute()
				visible = previous
				previous = CType(model, mxGraphModel).visibleStateForCellChanged(cell, previous)
			End Sub

		End Class

	End Class

End Namespace
