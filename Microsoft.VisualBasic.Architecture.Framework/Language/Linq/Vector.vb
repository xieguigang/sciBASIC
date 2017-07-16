Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Who = Microsoft.VisualBasic.Which

Namespace Language

    Public Class Vector(Of T) : Implements IEnumerable(Of T)

        ReadOnly buffer As T()

        Public ReadOnly Property Length As Integer
            Get
                Return buffer.Length
            End Get
        End Property

#Region ""
        ''' <summary>
        ''' The last elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Last As T
            Get
                If Length = 0 Then
                    Return Nothing
                Else
                    Return buffer(Length - 1)
                End If
            End Get
            Set(value As T)
                If Length = 0 Then
                    Throw New IndexOutOfRangeException
                Else
                    buffer(Length - 1) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' The first elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property First As T
            Get
                If Length = 0 Then
                    Return Nothing
                Else
                    Return buffer(0)
                End If
            End Get
            Set(value As T)
                If Length = 0 Then
                    Throw New IndexOutOfRangeException
                Else
                    buffer(Scan0) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="args">同时支持boolean和integer</param>
        ''' <returns></returns>
        Default Public Overloads Property Item(args As Object) As List(Of T)
            Get
                Dim index = Indexer.Indexing(args)
                Return Me(index)
            End Get
            Set
                Dim index = Indexer.Indexing(args)
                Me(index) = Value
            End Set
        End Property

        ''' <summary>
        ''' This indexer property is using for the ODEs-system computing.
        ''' (这个是为了ODEs计算模块所准备的一个数据接口)
        ''' </summary>
        ''' <param name="address"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(address As IAddress(Of Integer)) As T
            Get
                Return Item(address.Address)
            End Get
            Set(value As T)
                Item(address.Address) = value
            End Set
        End Property

        ''' <summary>
        ''' Can accept negative number as the index value, negative value means ``<see cref="Count"/> - n``, 
        ''' example as ``list(-1)``: means the last element in this list: ``list(list.Count -1)``
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index%) As T
            Get
                If index < 0 Then
                    index = Count + index  ' -1 -> count -1
                End If
                Return buffer(index)
            End Get
            Set(value As T)
                If index < 0 Then
                    index = Count + index  ' -1 -> count -1
                End If

                buffer(index) = value
            End Set
        End Property

        ''' <summary>
        ''' Using a index vector expression to select/update many elements from this list collection.
        ''' </summary>
        ''' <param name="exp$">
        ''' + ``1``, index=1
        ''' + ``1:8``, index=1, count=8
        ''' + ``1->8``, index from 1 to 8
        ''' + ``8->1``, index from 8 to 1
        ''' + ``1,2,3,4``, index=1 or  2 or 3 or 4
        ''' </param>
        ''' <returns></returns>
        Default Public Overloads Property Item(exp$) As List(Of T)
            Get
                Dim list As New List(Of T)

                For Each i% In exp.TranslateIndex
                    list += Item(index:=i)
                Next

                Return list
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In exp.TranslateIndex.SeqIterator
                    Item(index:=+i) = value(i.i)
                Next
            End Set
        End Property

        Default Public Overloads Property Item(range As IntRange) As List(Of T)
            Get
                Return New List(Of T)(Me.Skip(range.Min).Take(range.Length))
            End Get
            Set(value As List(Of T))
                Dim indices As Integer() = range.ToArray

                For i As Integer = 0 To indices.Length - 1
                    Item(index:=indices(i)) = value(i)
                Next
            End Set
        End Property

        Default Public Overloads Property Item(indices As IEnumerable(Of Integer)) As List(Of T)
            Get
                Return New List(Of T)(indices.Select(Function(i) Item(index:=i)))
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In indices.SeqIterator
                    Item(index:=+i) = value(i.i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Select all of the elements from this list collection is any of them match the condition expression: <paramref name="where"/>
        ''' </summary>
        ''' <param name="[where]"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item([where] As Predicate(Of T)) As T()
            Get
                Return buffer.Where(Function(o) where(o)).ToArray
            End Get
        End Property

        Default Public Overloads Property Item(booleans As IEnumerable(Of Boolean)) As T()
            Get
                Return Me(Who.IsTrue(booleans))
            End Get
            Set(value As T())
                For Each i In booleans.SeqIterator
                    If i.value Then
                        buffer(i) = value(i)
                    End If
                Next
            End Set
        End Property
#End Region

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x In buffer
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace