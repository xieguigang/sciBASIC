# mime/text%yaml/text.yaml.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.text.yaml
- AssemblyName  : Microsoft.VisualBasic.MIME.text.yaml
- TargetFramework: net10.0
- Source files  : 5
- Existing Title: YAML to JSON Object Model Parser and Front Matter Reader
- Existing Desc : Parses YAML for the sciBASIC# framework into the JsonElement model with block mappings, sequences, flow collections, block scalars, anchors, aliases and comments, plus a lightweight markdown front-matter reader.
- Existing Tags : scibasic;yaml;parser;json;front-matter

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MIME.text.yaml)

## Public types
- Class MultiLineState (MultiLineState.vb) - Tracks the state for multi-line string collection during pre-processing.
- Module Serialization (Serialization.vb)
- Module YamlFrontMatterParser (YamlFrontMatterParser.vb) - A valid front-matter block is delimited by "---" lines at the very top of the markdown file: ---
- Class YamlLine (YamlLine.vb) - Represents a single pre-processed line of YAML text with its indentation level.
- Class YamlParser (YamlParser.vb) - A YAML document parser that converts YAML text into JsonElement model objects. The parser supports block mappings, block sequences, multi-line strings, flow collections, anchors/aliases, and automatic scalar type inference.

## Notable public members
- Public Property Style As Char
- Public Property ChompPlus As Boolean
- Public Property ChompMinus As Boolean
- Public Property IndentIndicator As Integer
- Public Property Key As String
- Public Property Lines As New List(Of String)
- Public Property BaseIndent As Integer = -1
- Public Property StartLineNumber As Integer
- Public Function LoadYAML(Of T As {New, Class})(path As String) As T
- Public Function LoadYAMLDocument(Of T As {New, Class})(yaml As String) As T
- Public Function Parse(markdownContent As String) As Dictionary(Of String, String)
- Public Function StripFrontMatter(markdownContent As String) As String
- Public Property Indent As Integer
- Public Property Content As String
- Public Property LineNumber As Integer
- Public ReadOnly Property IsListItem As Boolean
- Public Overrides Function ToString() As String
- Public Function Parse(yamlText As String) As JsonElement
- Public Function ParseFile(filePath As String) As JsonElement
- Public Sub New()
- Public Sub Reset()

## Imports
- Microsoft.VisualBasic.MIME.application.json
- Microsoft.VisualBasic.MIME.application.json.Javascript
- std = System.Math
- System.Runtime.CompilerServices
- System.Text
- System.Text.RegularExpressions

## File tree
- MultiLineState.vb
- Serialization.vb
- YamlFrontMatterParser.vb
- YamlLine.vb
- YamlParser.vb

