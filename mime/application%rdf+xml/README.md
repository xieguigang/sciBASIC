# RDF/XML and Turtle Triple Serialization Model

XML serialization models for RDF documents, plus an RDF 1.1 Turtle reader, covering the `application/rdf+xml` MIME type in sciBASIC#.

## Overview
- XmlSerializer data model for RDF documents: `RDF(Of T)` root container, description elements, resource references and rdf:Bag / rdf:li collections.
- Dublin Core (DCMI) metadata fields plus XSD data-type mapping for typed literal values.
- RDF 1.1 Turtle (TTL) stream reader that yields subject-predicate-object triples and re-materializes them as `RDFEntity` objects.
- Base classes (`Description`, `RDFEntity`, `RDFProperty`) meant to be inherited by application-specific metadata readers.

## Key Types
- `Microsoft.VisualBasic.MIME.application.rdf_xml.RDF(Of T As Description)` — must-inherit root element of an rdf:RDF document; sets the rdf/rdfs namespaces.
- `Description` / `RDFEntity` / `RDFProperty` — description node, typed entity node and property model with range and comments.
- `DCMI` — Dublin Core metadata properties (title, creator, subject, publisher, rights, ...).
- `Resource` / `RDFType` / `Array` / `li` — rdf:resource references, rdf:type, bag containers and list items.
- `DataTypes` — XSD data type URIs plus mapping between .NET types and RDF literal types.
- `Turtle.TurtleFile` — streaming RDF 1.1 Turtle parser; `ReadObjects()` yields `Triple`, handles `@prefix`.
- `Turtle.Triple` / `Turtle.Relation` / `Turtle.ttl_property` — parsed subject with its predicate/object relations.
- `Turtle.BuildObject` — materializes `RDFEntity` objects from a triple sequence.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.application.rdf_xml.Turtle

Using ttl As New TurtleFile("data.ttl")
    For Each triple As Triple In ttl.ReadObjects()
        For Each rel As Relation In triple.relations
            Console.WriteLine($"{triple.subject} {rel.predicate} {String.Join(",", rel.objs)}")
        Next
    Next
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.application.rdf_xml`
- TargetFramework: `net10.0-windows;net10.0`
- Tags: `scibasic;rdf;semantic-web;turtle;linked-data`

## License
GPL-3.0-or-later
