## Serialize the Dynamics Dictionary

By using this dynamics serializer, imports this namespace at first:

```vbnet
Imports Microsoft.VisualBasic.MIME.JSON.ExtendedDictionary
```

Here is a dynamics object example:

```vbnet
' An object that extends a dictionary type
Public Class TestDynamicsObject : Inherits Dictionary(Of String, NamedValue(Of Integer()))
    Public Property Tarray As Double()
    Public Property str As String
    Public Property Tarray2 As String()
End Class
```

And we initialize this object using code:

```vbnet
Dim t As New TestDynamicsObject With {
    .Tarray = {1, 2, 3, 4, 5, 6, 7, 8},
    .str = "12345" & vbCrLf & "67890",
    .Tarray2 = {
        "xxoo", "1234", "6789", "50"
    }
}

Call t.Add("1234", New NamedValue(Of Integer())("x1", {100, 200, 3}))
Call t.Add("2333", New NamedValue(Of Integer())("x2", {-10, 203, 3}))
```

If we serialize this object which it is inherits from the dictionary type, then we just gets the dictionary data, example:

```vbnet
Call t.GetJson(True).SaveTo("./test_out.json")
```

The extended property will not included in the output json:

```json
{
  "1234": {
    "Description": null,
    "Name": "x1",
    "x": [ 100, 200, 3 ]
  },
  "2333": {
    "Description": null,
    "Name": "x2",
    "x": [ -10, 203, 3 ]
  }
}
```

Then we can serialize this dynamics object by using the extended serializer function:

```vbnet
Dim json$ = GetExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(t)
Call json.SaveTo("./test_out2.json")
```

This extended serializer function outputs:

```json
{
  "1234": {
    "Description": null,
    "Name": "x1",
    "x": [ 100, 200, 3 ]
  },
  "2333": {
    "Description": null,
    "Name": "x2",
    "x": [ -10, 203, 3 ]
  },
  "Tarray": [ 1, 2, 3, 4, 5, 6, 7, 8 ],
  "str": "12345\u000d\u000a67890",
  "Tarray2": [ "xxoo", "1234", "6789", "50" ]
}
```

This output result json is what we want. For load the dynamics json using deserialization:

```vbnet
Dim t2 = LoadExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(json)

' [DEBUG 2016/11/9 20:46:09] [1,2,3,4,5,6,7,8]
Call t2.Tarray.GetJson.debug
' [DEBUG 2016/11/9 20:46:10] ["xxoo","1234","6789","50"]
Call t2.Tarray2.GetJson.debug
```

The dynamics data was loaded into the correctly properties.





https://cdn.earthdata.nasa.gov/conduit/upload/497/ESDS-RFC-022v1.pdf