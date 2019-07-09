Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class GridMatrix : Inherits XmlDataModel

    Public Property [error] As Double

    Public Property direction As NumericVector

    <XmlElement("matrix")>
    Public Property matrix As NumericVector()

End Class
