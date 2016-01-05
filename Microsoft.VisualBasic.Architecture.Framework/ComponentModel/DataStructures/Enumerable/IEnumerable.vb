Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection

    Public Interface ILocalSearchHandle

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Matches(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As ILocalSearchHandle()
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Match(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As Boolean
    End Interface
End Namespace

Namespace ComponentModel.Collection.Generic

    Public Interface PairItem(Of TItem1, TItem2)
        Property Key1 As TItem1
        Property Key2 As TItem2

        ''' <summary>
        ''' Call by the method <see cref="IEnumerations.GetItem"></see>
        ''' </summary>
        ''' <param name="pairItem"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Equals(pairItem As PairItem(Of TItem1, TItem2)) As Boolean
    End Interface

    Public Interface IHandle
        Property Handle As Long
    End Interface

    ''' <summary>
    ''' This type of object have a <see cref="IDEnumerable.Identifier"></see> property to unique identified itself in a collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IDEnumerable

        ''' <summary>
        ''' The unique identifer in the object collection.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Identifier As String
    End Interface

    Public Interface IReadOnlyId
        ReadOnly Property Identifier As String
    End Interface
End Namespace
