# Collections
_namespace: [Microsoft.VisualBasic.Language.Java](./index.md)_

This class consists exclusively of static methods that operate on or return
 collections. It contains polymorphic algorithms that operate on
 collections, "wrappers", which return a new collection backed by a
 specified collection, and a few other odds and ends.
 
 <p>The methods of this class all throw a NullPointerException
 if the collections or class objects provided to them are null.
 
 <p>The documentation for the polymorphic algorithms contained in this class
 generally includes a brief description of the implementation. Such
 descriptions should be regarded as implementation notes, rather than
 parts of the specification. Implementors should feel free to
 substitute other algorithms, so long as the specification itself is adhered
 to. (For example, the algorithm used by sort does not have to be
 a mergesort, but it does have to be stable.)
 
 <p>The "destructive" algorithms contained in this [Class], that is, the
 algorithms that modify the collection on which they operate, are specified
 to throw UnsupportedOperationException if the collection does not
 support the appropriate mutation primitive(s), such as the set
 method. These algorithms may, but are not required to, throw this
 exception if an invocation would have no effect on the collection. For
 example, invoking the sort method on an unmodifiable list that is
 already sorted may or may not throw UnsupportedOperationException.
 
 <p>This class is a member of the
 
 Java Collections Framework.
 
 @author Josh Bloch
 @author Neal Gafter



### Methods

#### binarySearch``1
```csharp
Microsoft.VisualBasic.Language.Java.Collections.binarySearch``1(Microsoft.VisualBasic.Language.List{``0},``0,System.Collections.Generic.IComparer{``0})
```
Searches the specified list for the specified object using the binary
 search algorithm. The list must be sorted into ascending order
 according to the specified comparator (as by the
 #sort(List, Comparator) sort(List, Comparator)
 method), prior to making this call. If it is
 not sorted, the results are undefined. If the list contains multiple
 elements equal to the specified object, there is no guarantee which one
 will be found.
 
 <p>This method runs in log(n) time for a "random access" list (which
 provides near-constant-time positional access). If the specified list
 does not implement the RandomAccess interface and is large,
 this method will do an iterator-based binary search that performs
 O(n) link traversals and O(log n) element comparisons.

|Parameter Name|Remarks|
|--------------|-------|
|list| the list to be searched. |
|key| the key to be searched for. |
|c| the comparator by which the list is ordered.
         A null value indicates that the elements'
         Comparable natural ordering should be used. |


_returns:  the index of the search key, if it is contained in the list;
         otherwise, (-(insertion point) - 1).  The
         insertion point is defined as the point at which the
         key would be inserted into the list: the index of the first
         element greater than the key, or list.size() if all
         elements in the list are less than the specified key.  Note
         that this guarantees that the return value will be >= 0 if
         and only if the key is found. _

#### get``1
```csharp
Microsoft.VisualBasic.Language.Java.Collections.get``1(System.Collections.Generic.IEnumerator{``0},System.Int32)
```
Gets the ith element from the given list by repositioning the specified
 list listIterator.


