JsonFormatterPlus
=================

This library provides some simple JSON formatting / pretty printing functionality for .NET.
It was originally based on Mark Rogers' [JsonPrettyPrinterPlus](http://www.markdavidrogers.com/oxitesample/Blog/json-pretty-printerbeautifier-library-for-net), but has evolved from there.

### Installation

`PM> Install-Package JsonFormatterPlus`

### Example

Pretty-printing a JSON string:

```vbnet
Dim formattedJson As String = JsonFormatter.Format(unformattedJson)
```

Minifying a JSON string:

```vbnet
Dim minifiedJson As String = JsonFormatter.Minify(json)
```

(I told you it was simple!)

### What sets this apart from JsonPrettyPrinterPlus?

``JsonPrettyPrinterPlus`` works very well. However, I wanted to make the project suitable for .NET 2.0, 3.0, and the various client profiles. This meant that the extension methods and the dependency on ``System.Web.Extensions`` had to go!
A few extra 'quality of life' changes were also made, such as providing JSON minification, etc.

### License

``JsonFormatterPlus`` is distributed under the MIT license.
