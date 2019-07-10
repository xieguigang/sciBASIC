Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class GridMatrix : Inherits XmlDataModel

    Public Property [error] As Double

    Public Property direction As NumericVector

    <XmlElement("matrix")>
    Public Property matrix As NumericVector()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CreateSystem() As GridSystem
        Return New GridSystem With {
            .A = direction.vector,
            .C = matrix _
                .Select(Function(r)
                            Return New Correlation With {
                                .B = r.vector
                            }
                        End Function) _
                .ToArray
        }
    End Function

End Class
