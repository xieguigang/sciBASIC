#Region "Microsoft.VisualBasic::d6fc50e8d571fb7e37285a9f5e7c4828, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Set\Set.vb"

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

    '     Class [Set]
    ' 
    '         Properties: IsEmpty, Length
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: Contains, Equals, GetHashCode, IEnumerable_GetEnumerator, Remove
    '                   ToArray, ToString
    ' 
    '         Sub: Add, Clear, Dispose
    ' 
    '         Operators: -, +, <>, =, (+2 Overloads) And
    '                    (+4 Overloads) Or
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' Represents an unordered grouping of unique hetrogenous members.
    ''' (这个对象的功能和List类似，但是这个对象的主要的作用是进行一些集合运算：使用AND求交集以及使用OR求并集的)
    ''' </summary>
    ''' 
    <Package("Set",
             Category:=APICategories.UtilityTools,
             Description:="Represents an unordered grouping of unique hetrogenous members.",
             Url:="http://www.codeproject.com/Articles/10806/A-Generic-Set-Data-Structure",
             Publisher:="Sean Michael Murphy")>
    Public Class [Set]
        Implements IEnumerable
        Implements IDisposable
        Implements IsEmpty

#Region "Private Members"
        Protected Friend _members As New ArrayList()
        Protected _behaviour As BadBehaviourResponses = BadBehaviourResponses.BeAggressive
        ''' <summary>
        ''' 如何判断两个元素是否相同？
        ''' </summary>
        Protected Friend _equals As Func(Of Object, Object, Boolean)
#End Region

#Region "ctors"
        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        Public Sub New(Optional equals As Func(Of Object, Object, Boolean) = Nothing)
            _equals = equals
        End Sub

        Protected Sub New()
            _behaviour = BadBehaviourResponses.BeAggressive
        End Sub

        ''' <summary>
        ''' Constructor called when the source data is an array of <see cref="[Set]">Sets</see>.  They will
        ''' be unioned together, with addition exceptions quietly eaten.
        ''' </summary>
        ''' <param name="sources">The source array of <see cref="[Set]">Set</see> objects.</param>
        Public Sub New(sources As [Set](), Optional equals As Func(Of Object, Object, Boolean) = Nothing)
            _behaviour = BadBehaviourResponses.BeCool
            _equals = equals

            For Each initialSet As [Set] In sources
                For Each o As Object In initialSet
                    Call Me.Add(o)
                Next
            Next

            _behaviour = BadBehaviourResponses.BeAggressive
        End Sub

        Sub New(source As IEnumerable, Optional equals As Func(Of Object, Object, Boolean) = Nothing)
            _behaviour = BadBehaviourResponses.BeCool
            _equals = equals

            For Each o As Object In source
                Call Me.Add(o)
            Next

            _behaviour = BadBehaviourResponses.BeAggressive
        End Sub
#End Region

#Region "Public Methods"
        ''' <summary>
        ''' Empty the set of all members.
        ''' </summary>
        Public Sub Clear()
            _members.Clear()
        End Sub

        ''' <summary>
        ''' A method to determine whether the <see cref="[Set]">Set</see> has members.
        ''' </summary>
        ''' <returns>True is there are members, false if there are 0 members.</returns>
        Public ReadOnly Property IsEmpty() As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _members.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' Remove a member from the <see cref="[Set]">Set</see>.
        ''' </summary>
        ''' <param name="target">The member to remove.</param>
        ''' <returns>True if a member was removed, false if nothing was found that 
        ''' was removed.</returns>
        Public Function Remove(target As Object) As Boolean
            For i As Int32 = 0 To _members.Count - 1
                If _equals(_members(i), target) Then
                    _members.RemoveAt(i)
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' Method to add an <see cref="Object">Object</see> to the set.  The new member 
        ''' must be unique.
        ''' </summary>
        ''' <param name="member"><see cref="Object">Object</see> to add.</param>
        ''' <exception cref="InvalidOperationException">If the member being added is
        ''' already a member of the set an InvalidOperationException is thrown.</exception>
        Public Sub Add(member As Object)
            For i As Int32 = 0 To _members.Count - 1
                If _equals(_members(i), member) Then
                    If _behaviour = BadBehaviourResponses.BeAggressive Then
                        Throw New ArgumentException(member.ToString() + " already in set in position " + (i + 1).ToString() + ".")
                    Else
                        Return
                    End If
                End If
            Next

            _members.Add(member)
        End Sub

        ''' <summary>
        ''' Method to determine if a given object is a member of the set.
        ''' </summary>
        ''' <param name="target">The object to look for in the set.</param>
        ''' <returns>True if it is a member of the <see cref="[Set]">Set</see>, false if not.</returns>
        Public Function Contains(target As Object) As Boolean
            For Each o As Object In _members
                If _equals(o, target) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' Copies the members of the <see cref="[Set]">Set</see> to an array of 
        ''' <see cref="Object">Objects</see>.
        ''' </summary>
        ''' <returns>An <see cref="Object">Object</see> array copies of the 
        ''' elements of the <see cref="[Set]">Set</see></returns>
        Public Function ToArray() As Object()
            Return _members.ToArray()
        End Function
#End Region

#Region "Accessor"
        ''' <summary>
        ''' Public accessor for the members of the <see cref="[Set]">Set</see>.
        ''' </summary>
        Default Public ReadOnly Property Item(index As Int32) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _members(index)
            End Get
        End Property
#End Region

        ''' <summary>
        ''' The number of members of the set.
        ''' </summary>
        Public ReadOnly Property Length() As Int32
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _members.Count
            End Get
        End Property

#Region "Overloaded Operators"
        ''' <summary>
        ''' If the Set is created by casting an array to it, add the members of
        ''' the array through the Add method, so if the array has dupes an error
        ''' will occur.
        ''' </summary>
        ''' <param name="array">The array with the objects to initialize the array.</param>
        ''' <returns>A new Set object based on the members of the array.</returns>
        ''' <exception cref="InvalidCastException">If the array contains duplicate
        ''' elements, an InvalidCastException will result.</exception>
        Public Shared Narrowing Operator CType(array As Array) As [Set]
            Dim s As New [Set]()

            For Each o As [Object] In array
                Try
                    s.Add(o)
                Catch e As ArgumentException
                    Throw New InvalidCastException("Array contained duplicates and can't be cast to a Set.", e)
                End Try
            Next

            Return s
        End Operator

        ''' <summary>
        ''' Performs a union of two sets. The elements can exists 
        ''' in <paramref name="s1"/> or <paramref name="s2"/>.
        ''' (求并集)
        ''' </summary>
        ''' <param name="s1">Any set.</param>
        ''' <param name="s2">Any set.</param>
        ''' <returns>A new <see cref="[Set]">Set</see> object that contains all of the
        ''' members of each of the input sets.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(s1 As [Set], s2 As [Set]) As [Set]
            Return New [Set](s1.ToArray + s2.ToArray.AsList, s1._equals)
        End Operator

        Public Shared Operator Or(s1 As [Set], s2 As IEnumerable) As [Set]
            Dim sb As New [Set](s2, s1._equals)
            Return s1 Or sb
        End Operator

        ''' <summary>
        ''' Performs an intersection of two sets, the elements should exists 
        ''' in <paramref name="s1"/> and <paramref name="s2"/>.
        ''' (求交集)
        ''' </summary>
        ''' <param name="s1">Any set.</param>
        ''' <param name="s2">Any set.</param>
        ''' <returns>A new <see cref="[Set]">Set</see> object that contains the members
        ''' that were common to both of the input sets.</returns>
        Public Shared Operator And(s1 As [Set], s2 As [Set]) As [Set]
            Dim result As New [Set](s1._equals)

            result._behaviour = BadBehaviourResponses.BeCool

            If s1.Length > s2.Length Then
                For Each o As Object In s1
                    If s2.Contains(o) Then
                        result.Add(o)
                    End If
                Next
            Else
                For Each o As Object In s2
                    If s1.Contains(o) Then
                        result.Add(o)
                    End If
                Next
            End If

            result._behaviour = BadBehaviourResponses.BeAggressive

            Return result
        End Operator

        ''' <summary>
        ''' 求两个集合的并集，将两个集合之中的所有元素都合并在一起，这个操作符会忽略掉重复出现的元素
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns></returns>
        Public Shared Operator +(s1 As [Set], s2 As [Set]) As [Set]
            Return s1 Or s2
        End Operator

        ''' <summary>
        ''' except(差集)集合运算：先将其中完全重复的数据行删除，再返回只在第一个集合中出现，在第二个集合中不出现的所有行。
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns></returns>
        Public Shared Operator -(s1 As [Set], s2 As [Set]) As [Set]
            Dim inter As [Set] = s1 And s2  ' 交集

            For Each x In inter      ' 将集合1之中所有的交集元素移除即可
                Call s1.Remove(x)
            Next

            Return s1
        End Operator

        ''' <summary>
        ''' Overloaded == operator to determine if 2 sets are equal.
        ''' </summary>
        ''' <param name="s1">Any set.</param>
        ''' <param name="s2">Any set.</param>
        ''' <returns>True if the two comparison sets have the same number of elements, and
        ''' all of the elements of set s1 are contained in s2.</returns>
        Public Shared Operator =(s1 As [Set], s2 As [Set]) As Boolean
            If s1.Length <> s2.Length Then
                Return False
            End If

            For Each o As Object In s1
                If Not s2.Contains(o) Then
                    Return False
                End If
            Next

            Return True
        End Operator

        ''' <summary>
        ''' Overloaded != operator to determine if 2 sets are unequal.
        ''' </summary>
        ''' <param name="s1">A benchmark set.</param>
        ''' <param name="s2">The set to compare against the benchmark.</param>
        ''' <returns>True if the two comparison sets fail the equality (==) test,
        ''' false if the pass the equality test.</returns>
        Public Shared Operator <>(s1 As [Set], s2 As [Set]) As Boolean
            Return Not (s1 = s2)
        End Operator
#End Region

#Region "Overridden Members"
        ''' <summary>
        ''' Determines whether two <see cref="[Set]">Set</see> instances are equal.
        ''' </summary>
        ''' <param name="obj">The <see cref="[Set]">Set</see> to compare to the current Set.</param>
        ''' <returns>true if the specified <see cref="[Set]">Set</see> is equal to the current 
        ''' Set; otherwise, false.</returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim o As [Set] = TryCast(obj, [Set])

            If obj Is Nothing Then
                Return False
            End If

            Return (Me Is o)
        End Function

        ''' <summary>
        ''' Serves as a hash function for a particular type, suitable for use in hashing 
        ''' algorithms and data structures like a hash table.
        ''' </summary>
        ''' <returns>A hash code for the current <see cref="[Set]">Set</see>.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetHashCode() As Integer
            Return MyBase.GetHashCode()
        End Function

        ''' <summary>
        ''' Returns a <see cref="String">String</see> that represents the current
        ''' <see cref="[Set]">Set</see>.
        ''' </summary>
        ''' <returns>A <see cref="String">String</see> that represents the current
        ''' <see cref="[Set]">Set</see>.</returns>
        Public Overrides Function ToString() As String
            Dim contents$ =
                (From o As Object In _members Select Scripting.ToString(o)) _
                .JoinBy(", ")
            Return $"{{ {contents} }}"
        End Function
#End Region

        ''' <summary>
        ''' Returns an enumerator that can iterate through a collection.
        ''' </summary>
        ''' <returns>An <see cref="IEnumerator">IEnumerator</see> that can be 
        ''' used to iterate through the collection.</returns>
        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            For Each x As Object In Me._members
                Yield x
            Next
        End Function

        ''' <summary>
        ''' Performs cleanup tasks on the <see cref="[Set]">Set</see> object.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Dispose() Implements IDisposable.Dispose
            _members.Clear()
        End Sub
    End Class
End Namespace
