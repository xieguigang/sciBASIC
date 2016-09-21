# DataFrame System for VisualBasic Data Science

## Basic Usage

##### Read/Write csv Data

```vbnet
Dim csv As File = File.Load("./visitors.csv")

' access row data
For Each row As RowObject In csv
    Call row.__DEBUG_ECHO

    ' access columns in a row
    For Each col As String In row
        ' blablabla
    Next
Next

' set row data
csv(5) = New RowObject({"string", "data"})
' set column data in a row
csv(5)(5) = "yes!"
' or
Dim r12345 As RowObject = csv(5)
r12345(6) = "no?"

Call csv.Save("./visitors_updated.csv", Encodings.ASCII)
```

|Example code Screenshots    |
|----------------------------|
|![](./Example/csvData.png)  |
|![](./Example/rowData.png)  |
|![](./Example/rowModify.png)|











