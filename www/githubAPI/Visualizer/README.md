#### Download user github contributions

We can using a simple http ``GET`` method for download the github user contribtion data, due to the reason of the user contribution data is returned in a ``svg`` graphics document, so that we can parsing the return data by using XML method in ``VisualBasic``:

```vbnet
Dim url$ = $"https://github.com/users/{userName}/contributions"

Dim svg$ = url.GET
Dim xml As New XmlDocument

Call xml.LoadXml("<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf & svg)
```

The user github contribution for each day is stored in a rect svg element, with a xml attribute named ``data-count`` for the contribution counts in each day and using a xml attribute named ``data-date`` for the day identified:

```xml
<rect class="day" width="10" height="10" x="13" y="12" fill="#7bc96f" data-count="22" data-date="2016-05-23"/>
```

And these rect data can be read from path: ``svg -> g -> g -> rect``:

```xml
<svg width="676" height="104" class="js-calendar-graph-svg">
  <g transform="translate(16, 20)">
      <g transform="translate(0, 0)">
          <rect class="day" ... data-count="9" data-date="2016-05-22"/>
          <rect class="day" ... data-count="22" data-date="2016-05-23"/>
          <rect class="day" ... data-count="30" data-date="2016-05-24"/>
          <rect class="day" ... data-count="15" data-date="2016-05-25"/>
          <rect class="day" ... data-count="17" data-date="2016-05-26"/>
          <rect class="day" ... data-count="54" data-date="2016-05-27"/>
          <rect class="day" ... data-count="40" data-date="2016-05-28"/>
      </g>
      ...
```

So that we can parsing the returned svg XML by using the code:

```vbnet
Dim g As XmlNodeList = xml _
    .SelectSingleNode("svg") _
    .SelectSingleNode("g") _
    .SelectNodes("g")
Dim contributions As New Dictionary(Of Date, Integer)

For Each week As XmlNode In g
    Dim days = week.SelectNodes("rect")

    For Each day As XmlNode In days
        Dim date$ = day.Attributes.GetNamedItem("data-date").InnerText
        Dim count = day.Attributes.GetNamedItem("data-count").InnerText
        contributions(DateTime.Parse([date])) = CInt(count)
    Next
Next
```


### Github contribution data analysis

##### Streak

A streak on github contribution is define as the day range **without** ``ZERO`` contribution day. So that we can simple split the date ordered contribution result by condition expression: ``day.Value = 0``, here is how and in a very easy way:

```vbnet
Dim streak = contributions.Split(Function(day) day.Value = 0, )
```

So that the longest streak can be calculation by the splited block elements count descending ordering:

```vbnet
Dim LongestStreak = streak _
    .OrderByDescending(Function(days) days.Length) _
    .First
```

And the current streak is the nearest block, the last block is the nearest:

```vbnet
Dim currentStreak = streak.Last
```

##### Contributions

When we add all of the contribution values in the result, then we get all of the contribution values:

```vbnet
Dim total$ = contributions _
    .Sum(Function(day) day.Value) _
    .ToString("N") _
    .Replace(".00", "")
```