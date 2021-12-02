A library to print out a nicely formatted table in a console application C#

> Translate from https://github.com/minhhungit/ConsoleTableExt

### Feature
- Support [box-drawing characters](https://en.wikipedia.org/wiki/Box-drawing_character)
- Table alignment (left right and center)
- Column alignment (left/right/center)
- Table can have TITLE, can change text color and background color of title, support title alignment (left/right/center)
- Support power char-map, strong customization ability
- Contain some popular formas like Markdown table...
- Support text formatter (include header)
- Support many kind data type: DataTable, List<object>...
- Support metadata row (placed at top or bottom of table)
- Column min-length 
- support .NET Framework >= 3.5, .NET core
- ...

### How to use:

```csharp
var tableData = new List<List<object>>
{
    new List<object>{ "Sakura Yamamoto", "Support Engineer", "London", 46},
    new List<object>{ "Serge Baldwin", "Data Coordinator", "San Francisco", 28, "something else" },
    new List<object>{ "Shad Decker", "Regional Director", "Edinburgh"},
};
```

**Simple example with default format:**

```csharp
ConsoleTableBuilder
    .From(tableData)
    .ExportAndWriteLine();
```

**More example with existing format Alternative:**

```csharp
ConsoleTableBuilder
    .From(tableData)
    .WithFormat(ConsoleTableBuilderFormat.Alternative)
    .ExportAndWriteLine(TableAligntment.Center);
```

**Advance example with custom format using CharMap:**

```csharp
ConsoleTableBuilder
    .From(tableData)
    .WithTitle("CONTACTS ", ConsoleColor.Yellow, ConsoleColor.DarkGray)
    .WithColumn("Id", "First Name", "Sur Name")
    .WithMinLength(new Dictionary<int, int> {
        { 1, 25 },
        { 2, 25 }
    })
    .WithTextAlignment(new Dictionary<int, TextAligntment>
    {
        {2, TextAligntment.Right }
    })
    .WithCharMapDefinition(new Dictionary<CharMapPositions, char> {
        {CharMapPositions.BottomLeft, '=' },
        {CharMapPositions.BottomCenter, '=' },
        {CharMapPositions.BottomRight, '=' },
        {CharMapPositions.BorderTop, '=' },
        {CharMapPositions.BorderBottom, '=' },
        {CharMapPositions.BorderLeft, '|' },
        {CharMapPositions.BorderRight, '|' },
        {CharMapPositions.DividerY, '|' },
    })
    .WithHeaderCharMapDefinition(new Dictionary<HeaderCharMapPositions, char> {
        {HeaderCharMapPositions.TopLeft, '=' },
        {HeaderCharMapPositions.TopCenter, '=' },
        {HeaderCharMapPositions.TopRight, '=' },
        {HeaderCharMapPositions.BottomLeft, '|' },
        {HeaderCharMapPositions.BottomCenter, '-' },
        {HeaderCharMapPositions.BottomRight, '|' },
        {HeaderCharMapPositions.Divider, '|' },
        {HeaderCharMapPositions.BorderTop, '=' },
        {HeaderCharMapPositions.BorderBottom, '-' },
        {HeaderCharMapPositions.BorderLeft, '|' },
        {HeaderCharMapPositions.BorderRight, '|' },
    })
    .ExportAndWriteLine(TableAligntment.Right);
```

<img src="wiki/Images/demo1.png" style="width: 100%;" />

Check more demo here https://github.com/minhhungit/ConsoleTableExt/blob/master/Src/ConsoleTableApp/Program.cs

<img src="wiki/Images/demo.png" style="width: 100%;" />

### Char Map Definition

<img src="wiki/Images/CharMapPositions.png" style="width: 100%;" />

### Header Char Map

<img src="wiki/Images/HeaderCharMapPositions.png" style="width: 100%;" />
