# XML Toolkit with XSD Models, MathML and XPath Queries

General purpose XML helpers for sciBASIC#: an XSD schema data model, MathML parsing and rendering, a small XPath-like query engine and a generic element tree with reflection based object loading.

## Overview
- XSD element data model (schema, element, complexType/simpleType, attribute, sequence, restriction) that maps XSD documents onto serializable classes.
- MathML content markup: parse `<math>` / `<apply>` trees into expression objects, or read a lambda expression directly from MathML text.
- Lightweight XPath query engine: parse a path expression into a selector chain and evaluate it against any `IXmlDocumentTree` implementation.
- Generic `XmlElement` tree parsed from raw XML text, plus a graph writer that hydrates plain objects by reflection.

## Key Types
- `Microsoft.VisualBasic.MIME.application.xml.XmlElement` — generic element node with name, namespace, attributes, child elements, text and comments.
- `XmlParser` — parses XML text into the generic `XmlElement` tree (`XmlElement.ParseXmlText` shortcut).
- `XPath.XPath` / `XPath.XPathParser` — expression model (child, root, current node, parent, attribute selectors) and its parser.
- `XPath.XPathQuery` — executes a parsed expression with `QuerySingle` / `QueryAll` over an `IXmlDocumentTree`.
- `MathML.LambdaExpression` / `MathML.Expression.BinaryExpression` / `SymbolExpression` / `MathFunctionExpression` — parsed math expressions with a `ToString` renderer.
- `MathML.ContentBuilder` — converts MathML XML into expression objects (`ParseXml`).
- `xsd.schema` / `xsd.element` / `xsd.complexType` / `xsd.simpleType` — the XSD document object model.
- `XmlGeneric.GraphWriter` — loads XML into object graphs (`Load`, `LoadXml(Of T)`).

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Dim root As XmlElement = XmlParser.ParseXml(xmlText)
Dim f As LambdaExpression = LambdaExpression.FromMathML(root)

Console.WriteLine(f.ToString())
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.application.xml`
- TargetFramework: `net10.0-windows;net10.0`
- Tags: `scibasic;xml;xsd;mathml;xpath`

## License
GPL-3.0-or-later
