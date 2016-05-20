Namespace ComponentModel.Collection.Generic

    Public Interface IPairItem(Of TItem1, TItem2)
        Property Key1 As TItem1
        Property Key2 As TItem2

        ''' <summary>
        ''' Call by the method <see cref="IEnumerations.GetItem"></see>
        ''' </summary>
        ''' <param name="pairItem"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Equals(pairItem As IPairItem(Of TItem1, TItem2)) As Boolean
    End Interface

    Public Interface IHandle
        Property Handle As Long
    End Interface

    ''' <summary>
    ''' This type of object have a <see cref="sIdEnumerable.Identifier"></see> property to unique identified itself in a collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface sIdEnumerable

        ''' <summary>
        ''' The unique identifer in the object collection. Unique-Id of the target implements object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Identifier As String
    End Interface

    Public Interface IReadOnlyId

        ''' <summary>
        ''' The unique identifer in the object collection. Unique-Id of the target implements object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Identity As String
    End Interface
End Namespace
