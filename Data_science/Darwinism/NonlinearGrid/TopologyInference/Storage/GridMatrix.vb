Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class GridMatrix : Inherits XmlDataModel

    Public Property [error] As Double

    Public Property direction As NumericVector
    Public Property [const] As Constants

    <XmlElement("correlations")>
    Public Property correlations As NumericVector()
    ' <XmlElement("weights")>
    ' Public Property weights As NumericVector()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CreateSystem() As GridSystem
        Return New GridSystem With {
            .A = direction.vector,
            .C = correlations _
                .Select(Function(r, i)
                            Return New Correlation With {
                                .B = r.vector,
                                .BC = If([const] Is Nothing, 0, [const].B(i))
                            }
                        End Function) _
                .ToArray,
            .AC = If([const] Is Nothing, 0, [const].A)
        }
        '    .P = weights 
        '        .Select(Function(r)
        '                    Return New PWeight With {
        '                        .W = r.vector
        '                    }
        '                End Function) _
        '        .ToArray
        '}
    End Function

End Class

Public Class Constants
    Public Property A As Double
    Public Property B As NumericVector
End Class