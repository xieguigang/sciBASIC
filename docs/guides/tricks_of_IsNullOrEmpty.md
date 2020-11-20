# The tricks of IsNullorEmpty

I'm usually using a shared function from string type ``String.IsNullOrEmpty`` to determined that the user input string is nothing or is a empty string. And by works with the collection type, some time this ``String.IsNullOrEmpty`` liked operation is necessary as well:

+ In most of situation of my data processing, such as text data parser, some text field is not exists in the source line, so that the parser may returns a empty string collection or nothing due to the exception
+ In the object deserialization operations of the Xml, YAML or JSON, collection type as property may be nothing or is a empty collection after the serialization

So that a extension method ``IsNullOrEmpty`` of collection type is required for deal with these situation.

## The init edition for collection

In the early time of my programming, I'm using the ``IsNullOrEmpty`` like this:

```vbnet
''' <summary>
''' This object collection is a null object or contains zero count items.
''' NOTE: Do not applied this function on the Linq Expression due to the performance issue.
''' (判断某一个对象集合是否为空，请注意，由于在这里是使用了集合的Count进行判断是否有元素，
''' 所以这个函数可能不是太适合用于Linq的非立即查询)
''' </summary>
''' <typeparam name="T"></typeparam>
''' <param name="source"></param>
''' <returns></returns>
''' <remarks></remarks>
<Extension>
Public Function IsNullOrEmpty(Of T)(source As IEnumerable(Of T)) As Boolean
    Return source Is Nothing OrElse source.Count = 0
End Function
```

As in the early time of my programming, **collection type just involve in array, list, and dictionary**. And the extension function show above just works fine, as the ``Count`` extension method just iterate the elements and give back the elements number.

### Things get worse on Linq

The Linq expression results of two effects: **``LinqAPI.Exec(of T)`` in VisualBasic results a data collection, and the expression of linq just results a data producer.**

+ When iterate on the data collection, you just enumerate the datas, so that this iteration operation is just very rapid; 
+ When iterate on the data producer, you are not only enumerate all of the datas, and also producing new datas as your result data source.

```vbnet
Imports Microsoft.VisualBasic.Linq

' data collection result
Dim source As T() =
    LinqAPI.Exec(of T) <= From x As TIn
                          In dataSource
                          Let blablabla...
                          Where blablabla
                          Let blablabla
                          Select New T With {
                              .blablabla...
                          }
' data producer result
Dim source As IEnumerable(Of T) = 
    From x As TIn
    In dataSource
    Let blablabla...
    Where blablabla
    Let blablabla
    Select New T With {
        .blablabla...
    }
```

And this initial version ``IsNullOrEmpty`` is still works fine on the first Linq expression as ``source`` is a data collection, and ``Count`` method just enumerate the datas, and the procedure is very fast; But things get worsening on the second Linq expression, as the second expression result a data producer, you are not only enumerate the result datas, and also needs the expression to produce new objects as your result source. So that the ``Count`` function will enumerate the entire producer source and then reutrns the element counts, and this makes the program performance very worst.

## Solved with For loop

As we can using ``For Each`` loop to enumerate all of the elements in the collection, and if the collection have not object, then the ``For`` loop will not happened, so that we can using a ``For`` loop to determined that the source collection have element or not without enumerate the entired collection, Here is the second edition for the generic collection ``IsNullOrEmpty``:

```vbnet
''' <summary>
''' This object collection is a null object or contains zero count items.
''' </summary>
''' <typeparam name="T"></typeparam>
''' <param name="source"></param>
''' <returns></returns>
''' <remarks></remarks>
<Extension> Public Function IsNullOrEmpty(Of T)(source As IEnumerable(Of T)) As Boolean
    If source Is Nothing Then Return True

    Dim i As Integer = -1

    For Each x As T In source
        i += 1   ' 假若是存在元素的，则i的值会为零
        Return False  ' If is not empty, then this For loop will be used.
    Next

    ' 由于没有元素，所以For循环没有进行，i变量的值没有发生变化
    If i = -1 Then ' 使用count拓展进行判断或导致Linq被执行两次，现在使用FirstOrDefault来判断，主需要查看第一个元素而不是便利整个Linq查询枚举，从而提高了效率
        Return True  ' Due to the reason of source is empty, no elements, so that i value is not changed as the For loop didn't used.
    Else
        Return False
    End If

    Return False
End Function
```

Now using ``IsNullOrEmpty`` just works happy again with all of the enumeration collection type, both data collection and data producer works well with this extension method:

> As at the first of the function, we just detecting that target is nothing or not?
> And then we using a ``For`` loop just needs iterate the first element in the source, instead of using ``Count`` method to enumerate all of the elements, so that this is works perfect for the two linq expression type.
> In the ``For`` loop, if the source collection is empty, then the ``For`` loop will never happend, and the value of variable ``i`` will not changed. So that we can determined that in this situation the source collection is Empty.

And here I have add some extensions method for the common specific cllection types, for the code performance improvements.

```vbnet
    <Extension> Public Function IsNullOrEmpty(Of TKey, TValue)(dict As Dictionary(Of TKey, TValue)) As Boolean
        If dict Is Nothing Then
            Return True
        End If
        Return dict.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of T)(queue As Queue(Of T)) As Boolean
        If queue Is Nothing Then
            Return True
        End If
        Return queue.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of T)(list As List(Of T)) As Boolean
        If list Is Nothing Then
            Return True
        End If
        Return list.Count = 0
    End Function

    ''' <summary>
    ''' This object array is a null object or contains zero count items.(判断某一个对象数组是否为空)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function IsNullOrEmpty(Of T)(array As T()) As Boolean
        Return array Is Nothing OrElse array.Length = 0
    End Function
```