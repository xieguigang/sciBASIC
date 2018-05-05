Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
Imports System.Security
Imports System.Threading
Imports Microsoft.VisualBasic.Collection
Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.ForEachEnum

Namespace Microsoft.VisualBasic
    <Serializable, DefaultMember("Item"), DebuggerTypeProxy(GetType(CollectionDebugView)), DebuggerDisplay("Count = {Count}")> _
    Public NotInheritable Class Collection
        Implements ICollection, IList, ISerializable, IDeserializationCallback
        ' Methods
        Public Sub New()
            Me.Initialize(Utils.GetCultureInfo, 0)
        End Sub

        Private Sub New(info As SerializationInfo, context As StreamingContext)
            Me.m_DeserializationInfo = info
        End Sub

        Public Sub Add(Item As Object, Optional Key As String = Nothing, Optional Before As Object = Nothing, Optional After As Object = Nothing)
            If ((Not Before Is Nothing) AndAlso (Not After Is Nothing)) Then
                Throw New ArgumentException(Utils.GetResourceString("Collection_BeforeAfterExclusive"))
            End If
            Dim node As New Node(Key, Item)
            If (Not Key Is Nothing) Then
                Try
                    Me.m_KeyedNodesHash.Add(Key, node)
                Catch exception As ArgumentException
                    Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Collection_DuplicateKey")), &H1C9)
                End Try
            End If
            Try
                If ((Before Is Nothing) AndAlso (After Is Nothing)) Then
                    Me.m_ItemsList.Add(node)
                ElseIf (Not Before Is Nothing) Then
                    Dim key As String = TryCast(Before, String)
                    If (Not key Is Nothing) Then
                        Dim node2 As Node = Nothing
                        If Not Me.m_KeyedNodesHash.TryGetValue(key, node2) Then
                            Dim args As String() = New String() {"Before"}
                            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                        End If
                        Me.m_ItemsList.InsertBefore(node, node2)
                    Else
                        Me.m_ItemsList.Insert((Conversions.ToInteger(Before) - 1), node)
                    End If
                Else
                    Dim str2 As String = TryCast(After, String)
                    If (Not str2 Is Nothing) Then
                        Dim node3 As Node = Nothing
                        If Not Me.m_KeyedNodesHash.TryGetValue(str2, node3) Then
                            Dim textArray2 As String() = New String() {"After"}
                            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
                        End If
                        Me.m_ItemsList.InsertAfter(node, node3)
                    Else
                        Me.m_ItemsList.Insert(Conversions.ToInteger(After), node)
                    End If
                End If
            Catch exception2 As OutOfMemoryException
                Throw
            Catch exception3 As ThreadAbortException
                Throw
            Catch exception4 As StackOverflowException
                Throw
            Catch exception5 As Exception
                If (Not Key Is Nothing) Then
                    Me.m_KeyedNodesHash.Remove(Key)
                End If
                Throw
            End Try
            Me.AdjustEnumeratorsOnNodeInserted(node)
        End Sub

        Friend Sub AddIterator(weakref As WeakReference)
            Me.m_Iterators.Add(weakref)
        End Sub

        Private Sub AdjustEnumeratorsHelper(NewOrRemovedNode As Node, Type As AdjustIndexType)
            Dim i As Integer = (Me.m_Iterators.Count - 1)
            Do While (i >= 0)
                Dim reference As WeakReference = DirectCast(Me.m_Iterators.Item(i), WeakReference)
                If reference.IsAlive Then
                    Dim target As ForEachEnum = DirectCast(reference.Target, ForEachEnum)
                    If (Not target Is Nothing) Then
                        target.Adjust(NewOrRemovedNode, Type)
                    End If
                Else
                    Me.m_Iterators.RemoveAt(i)
                End If
                i -= 1
            Loop
        End Sub

        Private Sub AdjustEnumeratorsOnNodeInserted(NewNode As Node)
            Me.AdjustEnumeratorsHelper(NewNode, AdjustIndexType.Insert)
        End Sub

        Private Sub AdjustEnumeratorsOnNodeRemoved(RemovedNode As Node)
            Me.AdjustEnumeratorsHelper(RemovedNode, AdjustIndexType.Remove)
        End Sub

        Public Sub Clear()
            Me.m_KeyedNodesHash.Clear()
            Me.m_ItemsList.Clear()
            Dim i As Integer = (Me.m_Iterators.Count - 1)
            Do While (i >= 0)
                Dim reference As WeakReference = DirectCast(Me.m_Iterators.Item(i), WeakReference)
                If reference.IsAlive Then
                    Dim target As ForEachEnum = DirectCast(reference.Target, ForEachEnum)
                    If (Not target Is Nothing) Then
                        target.AdjustOnListCleared()
                    End If
                Else
                    Me.m_Iterators.RemoveAt(i)
                End If
                i -= 1
            Loop
        End Sub

        Public Function Contains(Key As String) As Boolean
            If (Key Is Nothing) Then
                Dim args As String() = New String() {"Key"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Return Me.m_KeyedNodesHash.ContainsKey(Key)
        End Function

        Public Function GetEnumerator() As IEnumerator
            Dim i As Integer = (Me.m_Iterators.Count - 1)
            Do While (i >= 0)
                If Not DirectCast(Me.m_Iterators.Item(i), WeakReference).IsAlive Then
                    Me.m_Iterators.RemoveAt(i)
                End If
                i -= 1
            Loop
            Dim target As New ForEachEnum(Me)
            Dim reference As New WeakReference(target)
            target.WeakRef = reference
            Me.m_Iterators.Add(reference)
            Return target
        End Function

        Friend Function GetFirstListNode() As Node
            Return Me.m_ItemsList.GetFirstListNode
        End Function

        <SecurityCritical>
        Private Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
            Dim strArray As String() = New String(((Me.Count - 1) + 1) - 1) {}
            Dim objArray As Object() = New Object(((Me.Count - 1) + 1) - 1) {}
            Dim firstListNode As Node = Me.GetFirstListNode
            Dim num2 As Integer = 0
            Do While (Not firstListNode Is Nothing)
                Dim num As Integer
                If (Not firstListNode.m_Key Is Nothing) Then
                    num2 += 1
                End If
                strArray(num) = firstListNode.m_Key
                objArray(num) = firstListNode.m_Value
                num += 1
                firstListNode = firstListNode.m_Next
            Loop
            info.AddValue("Keys", strArray, GetType(String()))
            info.AddValue("KeysCount", num2, GetType(Integer))
            info.AddValue("Values", objArray, GetType(Object()))
            info.AddValue("CultureInfo", Me.m_CultureInfo)
        End Sub

        Private Sub ICollectionCopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
            Dim num As Integer
            If (array Is Nothing) Then
                Dim args As String() = New String() {"array"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            If (array.Rank <> 1) Then
                Dim textArray2 As String() = New String() {"array"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", textArray2))
            End If
            If ((index < 0) OrElse ((array.Length - index) < Me.Count)) Then
                Dim textArray3 As String() = New String() {"index"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray3))
            End If
            Dim objArray As Object() = TryCast(array, Object())
            If (Not objArray Is Nothing) Then
                Dim count As Integer = Me.Count
                num = 1
                Do While (num <= count)
                    objArray(((index + num) - 1)) = Me.Item(num)
                    num += 1
                Loop
            Else
                Dim num3 As Integer = Me.Count
                num = 1
                Do While (num <= num3)
                    array.SetValue(Me.Item(num), CInt(((index + num) - 1)))
                    num += 1
                Loop
            End If
        End Sub

        Private Function ICollectionGetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator
        End Function

        Private Function IListAdd(value As Object) As Integer Implements IList.Add
            Me.Add(value, Nothing, Nothing, Nothing)
            Return (Me.m_ItemsList.Count - 1)
        End Function

        Private Sub IListClear() Implements IList.Clear
            Me.Clear()
        End Sub

        Private Function IListContains(value As Object) As Boolean Implements IList.Contains
            Return (Me.IListIndexOf(value) <> -1)
        End Function

        Private Function IListIndexOf(value As Object) As Integer Implements IList.IndexOf
            Return Me.m_ItemsList.IndexOfValue(value)
        End Function

        Private Sub IListInsert(index As Integer, value As Object) Implements IList.Insert
            Dim node As New Node(Nothing, value)
            Me.m_ItemsList.Insert(index, node)
            Me.AdjustEnumeratorsOnNodeInserted(node)
        End Sub

        Private Sub IListRemove(value As Object) Implements IList.Remove
            Dim index As Integer = Me.IListIndexOf(value)
            If (index <> -1) Then
                Me.IListRemoveAt(index)
            End If
        End Sub

        Private Sub IListRemoveAt(index As Integer) Implements IList.RemoveAt
            Dim removedNode As Node = Me.m_ItemsList.RemoveAt(index)
            Me.AdjustEnumeratorsOnNodeRemoved(removedNode)
            If (Not removedNode.m_Key Is Nothing) Then
                Me.m_KeyedNodesHash.Remove(removedNode.m_Key)
            End If
            removedNode.m_Prev = Nothing
            removedNode.m_Next = Nothing
        End Sub

        Private Sub IndexCheck(Index As Integer)
            If ((Index < 1) OrElse (Index > Me.m_ItemsList.Count)) Then
                Throw New IndexOutOfRangeException(Utils.GetResourceString("Argument_CollectionIndex"))
            End If
        End Sub

        Private Sub Initialize(CultureInfo As CultureInfo, Optional StartingHashCapacity As Integer = 0)
            If (StartingHashCapacity > 0) Then
                Me.m_KeyedNodesHash = New Dictionary(Of String, Node)(StartingHashCapacity, StringComparer.Create(CultureInfo, True))
            Else
                Me.m_KeyedNodesHash = New Dictionary(Of String, Node)(StringComparer.Create(CultureInfo, True))
            End If
            Me.m_ItemsList = New FastList
            Me.m_Iterators = New ArrayList
            Me.m_CultureInfo = CultureInfo
        End Sub

        Private Function InternalItemsList() As FastList
            Return Me.m_ItemsList
        End Function

        Private Sub OnDeserialization(sender As Object) Implements IDeserializationCallback.OnDeserialization
            Try
                Dim cultureInfo As CultureInfo = DirectCast(Me.m_DeserializationInfo.GetValue("CultureInfo", GetType(CultureInfo)), CultureInfo)
                If (cultureInfo Is Nothing) Then
                    Throw New SerializationException(Utils.GetResourceString("Serialization_MissingCultureInfo"))
                End If
                Dim strArray As String() = DirectCast(Me.m_DeserializationInfo.GetValue("Keys", GetType(String())), String())
                Dim objArray As Object() = DirectCast(Me.m_DeserializationInfo.GetValue("Values", GetType(Object())), Object())
                If (strArray Is Nothing) Then
                    Throw New SerializationException(Utils.GetResourceString("Serialization_MissingKeys"))
                End If
                If (objArray Is Nothing) Then
                    Throw New SerializationException(Utils.GetResourceString("Serialization_MissingValues"))
                End If
                If (strArray.Length <> objArray.Length) Then
                    Throw New SerializationException(Utils.GetResourceString("Serialization_KeyValueDifferentSizes"))
                End If
                Dim startingHashCapacity As Integer = Me.m_DeserializationInfo.GetInt32("KeysCount")
                If ((startingHashCapacity < 0) OrElse (startingHashCapacity > strArray.Length)) Then
                    startingHashCapacity = 0
                End If
                Me.Initialize(cultureInfo, startingHashCapacity)
                Dim num2 As Integer = (strArray.Length - 1)
                Dim i As Integer = 0
                Do While (i <= num2)
                    Me.Add(objArray(i), strArray(i), Nothing, Nothing)
                    i += 1
                Loop
                Me.m_DeserializationInfo = Nothing
            Finally
                If (Not Me.m_DeserializationInfo Is Nothing) Then
                    Me.m_DeserializationInfo = Nothing
                    Me.Initialize(Utils.GetCultureInfo, 0)
                End If
            End Try
        End Sub

        Public Sub Remove(Index As Integer)
            Me.IndexCheck(Index)
            Dim removedNode As Node = Me.m_ItemsList.RemoveAt((Index - 1))
            Me.AdjustEnumeratorsOnNodeRemoved(removedNode)
            If (Not removedNode.m_Key Is Nothing) Then
                Me.m_KeyedNodesHash.Remove(removedNode.m_Key)
            End If
            removedNode.m_Prev = Nothing
            removedNode.m_Next = Nothing
        End Sub

        Public Sub Remove(Key As String)
            Dim node As Node = Nothing
            If Me.m_KeyedNodesHash.TryGetValue(Key, node) Then
                Me.AdjustEnumeratorsOnNodeRemoved(node)
                Me.m_KeyedNodesHash.Remove(Key)
                Me.m_ItemsList.RemoveNode(node)
                node.m_Prev = Nothing
                node.m_Next = Nothing
            Else
                Dim args As String() = New String() {"Key"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
        End Sub

        Friend Sub RemoveIterator(weakref As WeakReference)
            Me.m_Iterators.Remove(weakref)
        End Sub


        ' Properties
        Public ReadOnly Property Count As Integer
            Get
                Return Me.m_ItemsList.Count
            End Get
        End Property

        Private ReadOnly Property ICollectionCount As Integer
            Get
                Return Me.m_ItemsList.Count
            End Get
        End Property

        Private ReadOnly Property ICollectionIsSynchronized As Boolean
            Get
                Return False
            End Get
        End Property

        Private ReadOnly Property ICollectionSyncRoot As Object
            Get
                Return Me
            End Get
        End Property

        Private ReadOnly Property IListIsFixedSize As Boolean
            Get
                Return False
            End Get
        End Property

        Private ReadOnly Property IListIsReadOnly As Boolean
            Get
                Return False
            End Get
        End Property

        Private Property IListItem(index As Integer) As Object
            Get
                Return Me.m_ItemsList.Item(index).m_Value
            End Get
            Set(value As Object)
                Me.m_ItemsList.Item(index).m_Value = value
            End Set
        End Property

        Default Public ReadOnly Property Item(Index As Integer) As Object
            Get
                Me.IndexCheck(Index)
                Return Me.m_ItemsList.Item((Index - 1)).m_Value
            End Get
        End Property

        Default Public ReadOnly Property Item(Key As String) As Object
            Get
                If (Key Is Nothing) Then
                    Throw New IndexOutOfRangeException(Utils.GetResourceString("Argument_CollectionIndex"))
                End If
                Dim node As Node = Nothing
                If Not Me.m_KeyedNodesHash.TryGetValue(Key, node) Then
                    Dim args As String() = New String() {"Index"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                End If
                Return node.m_Value
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Default Public ReadOnly Property Item(Index As Object) As Object
            Get
                Dim num As Integer
                If ((TypeOf Index Is String OrElse TypeOf Index Is Char) OrElse TypeOf Index Is Char()) Then
                    Dim str As String = Conversions.ToString(Index)
                    Return Me.Item(str)
                End If
                Try
                    num = Conversions.ToInteger(Index)
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception6 As Exception
                    Dim args As String() = New String() {"Index"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                End Try
                Return Me.Item(num)
            End Get
        End Property

        <DynamicallyInvokableAttribute>
        Private ReadOnly Property Count As Integer Implements System.Collections.ICollection.Count
            Get
                Return Me.m_ItemsList.Count
            End Get
        End Property

        <DynamicallyInvokableAttribute>
        Private ReadOnly Property IsSynchronized As Boolean Implements System.Collections.ICollection.IsSynchronized
            Get
                Return False
            End Get
        End Property

        <DynamicallyInvokableAttribute>
        Private ReadOnly Property SyncRoot As Object Implements System.Collections.ICollection.SyncRoot
            Get
                Return Me
            End Get
        End Property

        <DynamicallyInvokableAttribute>
        Private ReadOnly Property IsFixedSize As Boolean Implements System.Collections.IList.IsFixedSize
            Get
                Return False
            End Get
        End Property

        <DynamicallyInvokableAttribute>
        Private ReadOnly Property IsReadOnly As Boolean Implements System.Collections.IList.IsReadOnly
            Get
                Return False
            End Get
        End Property

        <DynamicallyInvokableAttribute>
        Private Property Item(index As Integer) As Object Implements System.Collections.IList.Item
            Get
                Return Me.m_ItemsList.Item(index).m_Value
            End Get
            Set(value As Object)
                Me.m_ItemsList.Item(index).m_Value = value
            End Set
        End Property


        ' Fields
        Private m_CultureInfo As CultureInfo
        Private m_DeserializationInfo As SerializationInfo
        Private m_ItemsList As FastList
        Private m_Iterators As ArrayList
        Private m_KeyedNodesHash As Dictionary(Of String, Node)
        Private Const SERIALIZATIONKEY_CULTUREINFO As String = "CultureInfo"
        Private Const SERIALIZATIONKEY_KEYS As String = "Keys"
        Private Const SERIALIZATIONKEY_KEYSCOUNT As String = "KeysCount"
        Private Const SERIALIZATIONKEY_VALUES As String = "Values"

        ' Nested Types
        Friend NotInheritable Class CollectionDebugView
            ' Methods
            Public Sub New(RealClass As Collection)
                Me.m_InstanceBeingWatched = RealClass
            End Sub


            ' Properties
            <DebuggerBrowsable(DebuggerBrowsableState.RootHidden)>
            Public ReadOnly Property Items As Object()
                Get
                    Dim count As Integer = Me.m_InstanceBeingWatched.Count
                    If (count = 0) Then
                        Return Nothing
                    End If
                    Dim objArray2 As Object() = New Object((count + 1) - 1) {}
                    objArray2(0) = Utils.GetResourceString("EmptyPlaceHolderMessage")
                    Dim num2 As Integer = count
                    Dim i As Integer = 1
                    Do While (i <= num2)
                        Dim node As Node = Me.m_InstanceBeingWatched.InternalItemsList.Item((i - 1))
                        objArray2(i) = New KeyValuePair(node.m_Key, node.m_Value)
                        i += 1
                    Loop
                    Return objArray2
                End Get
            End Property


            ' Fields
            <DebuggerBrowsable(DebuggerBrowsableState.Never)>
            Private m_InstanceBeingWatched As Collection
        End Class

        Private NotInheritable Class FastList
            ' Methods
            Friend Sub New()
            End Sub

            Friend Sub Add(Node As Node)
                Dim numRef As Integer
                If (Me.m_StartOfList Is Nothing) Then
                    Me.m_StartOfList = Node
                Else
                    Me.m_EndOfList.m_Next = Node
                    Node.m_Prev = Me.m_EndOfList
                End If
                Me.m_EndOfList = Node
                numRef = CInt(AddressOf Me.m_Count) = (numRef + 1)
            End Sub

            Friend Sub Clear()
                Me.m_StartOfList = Nothing
                Me.m_EndOfList = Nothing
                Me.m_Count = 0
            End Sub

            Friend Function Count() As Integer
                Return Me.m_Count
            End Function

            Private Function DataIsEqual(obj1 As Object, obj2 As Object) As Boolean
                Return ((obj1 Is obj2) OrElse ((obj1.GetType Is obj2.GetType) AndAlso Object.Equals(obj1, obj2)))
            End Function

            Private Sub DeleteNode(NodeToBeDeleted As Node, PrevNode As Node)
                Dim numRef As Integer
                If (PrevNode Is Nothing) Then
                    Me.m_StartOfList = Me.m_StartOfList.m_Next
                    If (Me.m_StartOfList Is Nothing) Then
                        Me.m_EndOfList = Nothing
                    Else
                        Me.m_StartOfList.m_Prev = Nothing
                    End If
                Else
                    PrevNode.m_Next = NodeToBeDeleted.m_Next
                    If (PrevNode.m_Next Is Nothing) Then
                        Me.m_EndOfList = PrevNode
                    Else
                        PrevNode.m_Next.m_Prev = PrevNode
                    End If
                End If
                numRef = CInt(AddressOf Me.m_Count) = (numRef - 1)
            End Sub

            Friend Function GetFirstListNode() As Node
                Return Me.m_StartOfList
            End Function

            Private Function GetNodeAtIndex(Index As Integer, ByRef Optional PrevNode As Node = Nothing) As Node
                Dim startOfList As Node = Me.m_StartOfList
                Dim num As Integer = 0
                PrevNode = Nothing
                Do While ((num < Index) AndAlso (Not startOfList Is Nothing))
                    PrevNode = startOfList
                    startOfList = startOfList.m_Next
                    num += 1
                Loop
                Return startOfList
            End Function

            Friend Function IndexOfValue(Value As Object) As Integer
                Dim startOfList As Node = Me.m_StartOfList
                Dim i As Integer = 0
                Do While (Not startOfList Is Nothing)
                    If Me.DataIsEqual(startOfList.m_Value, Value) Then
                        Return i
                    End If
                    startOfList = startOfList.m_Next
                    i += 1
                Loop
                Return -1
            End Function

            Friend Sub Insert(Index As Integer, Node As Node)
                Dim prevNode As Node = Nothing
                If ((Index < 0) OrElse (Index > Me.m_Count)) Then
                    Throw New ArgumentOutOfRangeException("Index")
                End If
                Dim nodeAtIndex As Node = Me.GetNodeAtIndex(Index, prevNode)
                Me.Insert(Node, prevNode, nodeAtIndex)
            End Sub

            Private Sub Insert(Node As Node, PrevNode As Node, CurrentNode As Node)
                Dim numRef As Integer
                Node.m_Next = CurrentNode
                If (Not CurrentNode Is Nothing) Then
                    CurrentNode.m_Prev = Node
                End If
                If (PrevNode Is Nothing) Then
                    Me.m_StartOfList = Node
                Else
                    PrevNode.m_Next = Node
                    Node.m_Prev = PrevNode
                End If
                If (Node.m_Next Is Nothing) Then
                    Me.m_EndOfList = Node
                End If
                numRef = CInt(AddressOf Me.m_Count) = (numRef + 1)
            End Sub

            Friend Sub InsertAfter(Node As Node, NodeToInsertAfter As Node)
                Me.Insert(Node, NodeToInsertAfter, NodeToInsertAfter.m_Next)
            End Sub

            Friend Sub InsertBefore(Node As Node, NodeToInsertBefore As Node)
                Me.Insert(Node, NodeToInsertBefore.m_Prev, NodeToInsertBefore)
            End Sub

            Friend Function RemoveAt(Index As Integer) As Node
                Dim startOfList As Node = Me.m_StartOfList
                Dim num As Integer = 0
                Dim prevNode As Node = Nothing
                Do While ((num < Index) AndAlso (Not startOfList Is Nothing))
                    prevNode = startOfList
                    startOfList = startOfList.m_Next
                    num += 1
                Loop
                If (startOfList Is Nothing) Then
                    Throw New ArgumentOutOfRangeException("Index")
                End If
                Me.DeleteNode(startOfList, prevNode)
                Return startOfList
            End Function

            Friend Sub RemoveNode(NodeToBeDeleted As Node)
                Me.DeleteNode(NodeToBeDeleted, NodeToBeDeleted.m_Prev)
            End Sub


            ' Properties
            Friend ReadOnly Property Item(Index As Integer) As Node
                Get
                    Dim prevNode As Node = Nothing
                    Dim nodeAtIndex As Node = Me.GetNodeAtIndex(Index, prevNode)
                    If (nodeAtIndex Is Nothing) Then
                        Throw New ArgumentOutOfRangeException("Index")
                    End If
                    Return nodeAtIndex
                End Get
            End Property


            ' Fields
            Private m_Count As Integer = 0
            Private m_EndOfList As Node
            Private m_StartOfList As Node
        End Class

        <StructLayout(LayoutKind.Sequential)>
        Private Structure KeyValuePair
            Private m_Key As Object
            Private m_Value As Object
            Friend Sub New(NewKey As Object, NewValue As Object)
                Me = New KeyValuePair
                Me.m_Key = NewKey
                Me.m_Value = NewValue
            End Sub

            Public ReadOnly Property Key As Object
                Get
                    Return Me.m_Key
                End Get
            End Property

            Public ReadOnly Property Value As Object
                Get
                    Return Me.m_Value
                End Get
            End Property

        End Structure

        Friend NotInheritable Class Node
            ' Methods
            Friend Sub New(Key As String, Value As Object)
                Me.m_Value = Value
                Me.m_Key = Key
            End Sub


            ' Fields
            Friend m_Key As String
            Friend m_Next As Node
            Friend m_Prev As Node
            Friend m_Value As Object
        End Class
    End Class
End Namespace

