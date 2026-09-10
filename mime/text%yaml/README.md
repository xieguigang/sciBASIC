# YAML to JSON Object Model Parser and Front Matter Reader

YAML reader for sciBASIC# that turns YAML text into the shared `JsonElement` model, deserializes documents into typed classes, and reads markdown front matter.

## Overview
- Block mappings, block sequences and nested structures parsed into `JsonElement` objects from `Microsoft.VisualBasic.MIME.application.json`.
- Flow collections, block scalars with `|`/`>` style and `+`/`-` chomping indicators, and multi-line quoted scalars handled in a pre-processing pass.
- Anchors (`&name`), aliases (`*name`) and merge keys (`<<: *name`) resolved through a per-parser anchor table; comments stripped outside quoted strings; automatic scalar type inference.
- Generic deserialization helpers that map a YAML file or string straight onto a .NET class.
- Lightweight markdown front-matter reader for `---` delimited blocks at the top of a document.

## Key Types
- `Microsoft.VisualBasic.MIME.text.yaml.YamlParser` — the parser: `Parse` for YAML text and `ParseFile` for a file, both returning a `JsonElement`.
- `Microsoft.VisualBasic.MIME.text.yaml.YamlLine` — one pre-processed line of YAML with its indentation level and list marker.
- `Microsoft.VisualBasic.MIME.text.yaml.MultiLineState` — tracks multi-line string collection and block scalar chomping during pre-processing.
- `Microsoft.VisualBasic.MIME.text.yaml.Serialization` — module with `LoadYAML` and `LoadYAMLDocument` for typed deserialization.
- `Microsoft.VisualBasic.MIME.text.yaml.YamlFrontMatterParser` — module with `Parse` (front matter to dictionary) and `StripFrontMatter` (body only).

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.text.yaml

Dim parser As New YamlParser()
Dim root As JsonElement = parser.ParseFile("config.yml")

' deserialize into a typed model
Dim config As AppConfig = Serialization.LoadYAML(Of AppConfig)("config.yml")
Dim fromText As AppConfig = Serialization.LoadYAMLDocument(Of AppConfig)(yamlText)

' markdown front matter
Dim meta As Dictionary(Of String, String) = YamlFrontMatterParser.Parse(markdownText)
Dim body As String = YamlFrontMatterParser.StripFrontMatter(markdownText)
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.text.yaml`
- TargetFramework: `net10.0`
- Tags: `scibasic;yaml;parser;json;front-matter`

## License
GPL-3.0-or-later
