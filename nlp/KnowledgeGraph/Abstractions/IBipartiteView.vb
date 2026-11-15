Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic

' NOTE: no Namespace declaration. RootNamespace of this project is
' Microsoft.VisualBasic.Data.NLP.Knowledge, so all new types live next to
' KnowledgeGraph / SimilarityMetrics / EntityDisambiguator.

''' <summary>
''' A read-only view over a bipartite knowledge network (entities on one side,
''' attributes on the other). All new analysis algorithms depend only on this
''' contract so they can be reused by any graph implementation, not just by
''' Microsoft.VisualBasic.Data.NLP.Knowledge.KnowledgeGraph.
''' </summary>
''' <remarks>
''' Identifiers are opaque integers. Implementations must return a defensive
''' copy from the HashSet-returning methods, because callers are allowed to
''' mutate the result (e.g. IntersectWith).
''' </remarks>
Public Interface IBipartiteView

    ''' <summary>All entity identifiers.</summary>
    ReadOnly Property EntityIds As IEnumerable(Of Integer)

    ''' <summary>All attribute identifiers.</summary>
    ReadOnly Property AttributeIds As IEnumerable(Of Integer)

    ''' <summary>Attributes connected to an entity (defensive copy).</summary>
    Function GetEntityAttributes(entityId As Integer) As HashSet(Of Integer)

    ''' <summary>Entities connected to an attribute (defensive copy).</summary>
    Function GetAttributeEntities(attributeId As Integer) As HashSet(Of Integer)

    ''' <summary>Attributes shared by two entities (defensive copy).</summary>
    Function GetSharedAttributes(entityA As Integer, entityB As Integer) As HashSet(Of Integer)

    Function GetEntityDegree(entityId As Integer) As Integer
    Function GetAttributeDegree(attributeId As Integer) As Integer

    ''' <summary>
    ''' Confidence of one entity-attribute edge. Implementations without
    ''' explicit weights should return 1.0.
    ''' </summary>
    Function GetEdgeWeight(entityId As Integer, attributeId As Integer) As Double

    ''' <summary>Field/group of an attribute (e.g. name, formula, cas).</summary>
    Function GetAttributeCategory(attributeId As Integer) As String

    Function GetAttributeName(attributeId As Integer) As String
    Function GetEntityName(entityId As Integer) As String
    Function GetEntityLanguage(entityId As Integer) As String

    ''' <summary>Resolve a name to an entity id; -1 when not found.</summary>
    Function FindEntityByName(name As String) As Integer

End Interface
