#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd self test model"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System.Xml.Serialization

''' <summary>
''' A sample model used by the XSD self test. It must stay a top level public
''' class: XmlReflectionImporter refuses nested / non-public types.
''' </summary>
<XmlRoot("bookOrder", Namespace:="urn:books")>
Public Class BookOrder

    <XmlAttribute("id")>
    Public Property Id As String

    <XmlElement("customer")>
    Public Property Customer As String

    <XmlElement("quantity")>
    Public Property Quantity As Integer

    <XmlElement("total")>
    Public Property Total As Decimal

    <XmlArray("lines"), XmlArrayItem("line")>
    Public Property Lines As OrderLine()

    <XmlElement("status")>
    Public Property Status As OrderStatus

    <XmlElement("note")>
    Public Property Note As String

    ''' <summary>Create a representative instance for the round trip test.</summary>
    Public Shared Function CreateSample() As BookOrder
        Return New BookOrder With {
            .Id = "order-2026-001",
            .Customer = "Ada Lovelace",
            .Quantity = 2,
            .Total = 42.5D,
            .Status = OrderStatus.Shipped,
            .Note = "first batch",
            .Lines = New OrderLine() {
                New OrderLine With {.Sku = "bk-1", .Title = "The Analytical Engine", .Price = 20.25D},
                New OrderLine With {.Sku = "bk-2", .Title = "Notes on XML", .Price = 22.25D}
            }
        }
    End Function
End Class

''' <summary>A nested complex type used by <see cref="BookOrder"/>.</summary>
Public Class OrderLine

    <XmlAttribute("sku")>
    Public Property Sku As String

    <XmlElement("title")>
    Public Property Title As String

    <XmlElement("price")>
    Public Property Price As Decimal
End Class

''' <summary>An enumeration exported as an XSD simpleType with facets.</summary>
Public Enum OrderStatus
    <XmlEnum("new")> [New]
    <XmlEnum("shipped")> Shipped
    <XmlEnum("cancelled")> Cancelled
End Enum
