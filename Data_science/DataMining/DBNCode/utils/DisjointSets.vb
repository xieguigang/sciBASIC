
Namespace utils
    ''' <summary>
    ''' A disjoint sets ADT implemented with a Union-Find data structure. Performs
    ''' union-by-rank and path compression. Implemented using arrays.
    ''' 
    ''' Elements are represented by ints, numbered from zero.
    ''' 
    ''' Each disjoint set has one element designated as its root. Negative values
    ''' indicate the element is the root of a set. The absolute value of a negative
    ''' value is the number of elements in the set. Positive values are an index to
    ''' where the root was last known to be. If the set has been unioned with
    ''' another, the last known root will point to a more recent root.
    ''' 
    ''' @author Mark Allen Weiss revised 7/21/00 by Matt Fleming
    ''' </summary>

    Public Class DisjointSets
        Private array As Integer()

        ''' <summary>
        ''' Constructs a disjoint sets object.
        ''' </summary>
        ''' <param name="numElements">
        '''            the initial number of elements and also the initial number of
        '''            disjoint sets, since every element is initially in its own
        '''            set. </param>
        Public Sub New(numElements As Integer)
            array = New Integer(numElements - 1) {}
            For i = 0 To array.Length - 1
                array(i) = -1
            Next
        End Sub

        ' /**
        ' * Unites two disjoint sets into a single set. A union-by-rank
        ' * heuristic is used to choose the new root.
        ' *
        ' * @param a the root element of the first set.
        ' * @param b the root element of the second set.
        ' 
        ' public void union(int root1, int root2) {
        ' if (array[root2] < array[root1]) // root2 is deeper
        ' array[root1] = root2; // make root2 new root
        ' else {
        ' if (array[root1] == array[root2])
        ' array[root1]--; // update height if same
        ' array[root2] = root1; // make root1 new root
        ' }
        ' }

        ''' <summary>
        ''' Unites two disjoint sets into a single set, maintaining the root of the
        ''' first set.
        ''' </summary>
        ''' <param name="root1"> </param>
        ''' <param name="root2"> </param>
        Public Overridable Sub union(root1 As Integer, root2 As Integer)
            array(root2) = root1
        End Sub

        ''' <summary>
        ''' Finds the (int) name of the set containing a given element. Performs path
        ''' compression along the way.
        ''' </summary>
        ''' <param name="x">
        '''            the element sought. </param>
        ''' <returns> the set containing x. </returns>
        Public Overridable Function find(x As Integer) As Integer
            If array(x) < 0 Then
                Return x ' x is the root of the tree; return it
            Else
                ' Find out who the root is; compress path by making the root
                ' x's parent.
                array(x) = find(array(x))
                Return array(x) ' Return the root
            End If
        End Function

    End Class

End Namespace
