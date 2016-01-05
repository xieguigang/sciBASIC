Imports System
Imports System.Collections
Imports System.ComponentModel

Namespace Microsoft.VisualBasic
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend NotInheritable Class ForEachEnum
        Implements IEnumerator, IDisposable
        ' Methods
        Public Sub New(coll As Collection)
            Me.mCollectionObject = coll
            Me.Reset()
        End Sub

        Public Sub Adjust(Node As Node, Type As AdjustIndexType)
            If ((Not Node Is Nothing) AndAlso Not Me.mDisposed) Then
                Dim type As AdjustIndexType = type
                If (type <> AdjustIndexType.Insert) Then
                    If (type <> AdjustIndexType.Remove) Then
                        Return
                    End If
                Else
                    If ((Not Me.mCurrent Is Nothing) AndAlso (Node Is Me.mCurrent.m_Next)) Then
                        Me.mNext = Node
                    End If
                    Return
                End If
                If ((Not Node Is Me.mCurrent) AndAlso (Node Is Me.mNext)) Then
                    Me.mNext = Me.mNext.m_Next
                End If
            End If
        End Sub

        Friend Sub AdjustOnListCleared()
            Me.mNext = Nothing
        End Sub

        Private Sub Dispose() Implements IDisposable.Dispose
            If Not Me.mDisposed Then
                Me.mCollectionObject.RemoveIterator(Me.WeakRef)
                Me.mDisposed = True
            End If
            Me.mCurrent = Nothing
            Me.mNext = Nothing
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            If Not Me.mDisposed Then
                If Me.mAtBeginning Then
                    Me.mAtBeginning = False
                    Me.mNext = Me.mCollectionObject.GetFirstListNode
                End If
                If (Me.mNext Is Nothing) Then
                    Me.Dispose
                    Return False
                End If
                Me.mCurrent = Me.mNext
                If (Not Me.mCurrent Is Nothing) Then
                    Me.mNext = Me.mCurrent.m_Next
                    Return True
                End If
                Me.Dispose
            End If
            Return False
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            If Me.mDisposed Then
                Me.mCollectionObject.AddIterator(Me.WeakRef)
                Me.mDisposed = False
            End If
            Me.mCurrent = Nothing
            Me.mNext = Nothing
            Me.mAtBeginning = True
        End Sub


        ' Properties
        Public ReadOnly Property Current As Object
            Get
                If (Me.mCurrent Is Nothing) Then
                    Return Nothing
                End If
                Return Me.mCurrent.m_Value
            End Get
        End Property

        <DynamicallyInvokableAttribute> _
        Public ReadOnly Property System.Collections.IEnumerator.Current As Object
            Get
                If (Me.mCurrent Is Nothing) Then
                    Return Nothing
                End If
                Return Me.mCurrent.m_Value
            End Get
        End Property


        ' Fields
        Private mAtBeginning As Boolean
        Private mCollectionObject As Collection
        Private mCurrent As Node
        Private mDisposed As Boolean = False
        Private mNext As Node
        Friend WeakRef As WeakReference

        ' Nested Types
        Friend Enum AdjustIndexType
            ' Fields
            Insert = 0
            Remove = 1
        End Enum
    End Class
End Namespace

