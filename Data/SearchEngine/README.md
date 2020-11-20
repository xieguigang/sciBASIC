# Data Object Search Engine

## Query Syntax

##### 1. Normal Search

Just inputs the normal words that want to performed in the search action, example by using this statement as query:

```
Hello world!
```

And then it will be tokenlize as two words: ``hello`` and ``world``, these two words will match any text that contains the string ``hello`` or ``world`` by default.

###### 1.1 Using Wildcards

This search engine allowed wildcards search: ``*`` for any length any characters match, and ``%`` allowed just one any character match, example as:

```
abc*d
abc%d
```

+ ``abc*d`` will match ``abc...blabla...d``
+ ``abc%d`` will match ``abc.d``, and this text ``abc...blabla...d`` will match failure.

###### 1.2 Using Regexp

If your query expression its first char is ``#``, then query will be translate as regexp search, for example:

```
#\d+
```

this query expression will translate as regexp ``\d+`` match any number string.

###### 1.3 Using Levenshtein

If your query expression its first char is ``~``, then query will be translate as ``Levenshtein`` calculate for string similarity match:

```
~abcd
```

``~abcd`` will match any string that looks like to ``abcd`` with similarity score **0.85** cutoff.

##### 2. Must have Search

```
"ABC D"
```

``"ABC D"`` will match ``EEEABC DEE`` as it contains the entire query string ``ABC D``, ``ABC+D`` will match failure, as it can not contains ``ABC D``

##### 3. Logical Expression

Query can also using the logical expression for advance search, allowed operators are ``AND``, ``OR``, ``NOT``, and you can also using brackets ``(`` and ``)`` to changes the calculate priority, example:

```
'#\d+' AND ("ABC D" OR NOT 22)
```

This query expression can be explained as: **the result should match an any length numeric string (``\d+``) andalso the result should contains the entire string ``ABC D`` or not contains number string ``22``.**

##### 4. Field Search

This search engine can specific the search field in the object by specifc the field name, example:

```
Name:'#\d+' OR ID:55
```

This query expression can explain as: the query result should found a numeric sub string in the Property which named ``Name`` or ``ID`` property contains string ``55``.

## Code Usage

#### 1. Build Query Expression

```vbnet
Imports Microsoft.VisualBasic.Data.IO.SearchEngine

' Using the ``Build`` extension function for compile your query string into expression model
Dim query = "#\d+".Build(anyDefault:=Tokens.op_OR, allowInStr:=False)
```

#### 2. Expression Query

The you can performance the string or object match by using the function in the resulted ``Expression`` model:

For test match a string:

```vbnet
Dim result As Boolean = query.Match(text:="23333")
```

For test match a .NET object:

```vbnet
Dim obj As New NamedValue(Of String) With {
    .Name = "Hello world!",
    .Value = "23333",
    .Description = "Test Object"
}
Dim def As New IObject(GetType(NamedValue(Of String)))

result = query.Evaluate(def, obj)
```

Then this will makes very easy for integrated this engine into your application if you search for a search engine, example as for test match from a .NET object collection:

```vbnet
Dim def As New IObject(GetType(NamedValue(Of String)))
Dim source As IEnumerable(Of NamedValue(Of String))
Dim query = <user_inputs>.Build(anyDefault:=Tokens.op_OR, allowInStr:=False)

Dim out = From x
          In source
          Where query.Evaluate(def, x)
          Select x

For Each x In out
    ' Display on your Form to show result to your user.
    ' blablabla
Next
```