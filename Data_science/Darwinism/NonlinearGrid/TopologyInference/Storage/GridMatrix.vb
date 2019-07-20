#Region "Microsoft.VisualBasic::4866f94686ba7dc8655873f8c6aaa52a, Data_science\Darwinism\NonlinearGrid\TopologyInference\Storage\GridMatrix.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Class GridMatrix
' 
'     Properties: [const], [error], correlations, direction
' 
'     Function: CreateSystem
' 
' Class Constants
' 
'     Properties: A, B
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class GridMatrix : Inherits XmlDataModel

    Public Property [error] As Double

    Public Property direction As NumericVector
    Public Property [const] As Constants

    <XmlElement("correlations")>
    Public Property correlations As NumericVector()


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
    End Function

    Public Overloads Function ToString(lang As Languages) As String
        Dim visitX = Function(i As Integer)
                         Select Case lang
                             Case Languages.TypeScript
                                 Return $"X[{i}]"
                             Case Languages.PHP
                                 Return $"$X[{i}]"
                             Case Languages.R
                                 Return $"X[{i + 1}]"
                             Case Languages.VisualBasic
                                 Return $"X({i})"
                             Case Else
                                 Return $"X({i})"
                         End Select
                     End Function
        Dim pow = Function(x$, y$)
                      Select Case lang
                          Case Languages.VisualBasic, Languages.R
                              Return $"({x} ^ {y})"
                          Case Languages.TypeScript
                              Return $"Math.pow({x}, {y})"
                          Case Else
                              Return $"pow({x}, {y})"
                      End Select
                  End Function
        Dim formulaText As String = [const].A & " + " &
            correlations _
                .Select(Function(c, i)
                            Return $"{[const].B(i)} + " & c _
                                .AsEnumerable _
                                .Select(Function(cj, j)
                                            Return $"({cj} * {visitX(j)}])"
                                        End Function) _
                                .JoinBy(" + ")
                        End Function) _
                .Select(Function(power, i)
                            Return $"({direction(i)} * {pow(visitX(i), power)})"
                        End Function) _
                .JoinBy(" + " & vbCrLf)

        Select Case lang
            Case Languages.VisualBasic
                Return $"Public Function Grid(X As Double()) As Double
    Return {formulaText}
End Function"
            Case Languages.TypeScript
                Return $"export function Grid(X: number[]) : number {{
    return {formulaText};
}}"
            Case Languages.R
                Return $"
Grid <- function(X) {{
    {formulaText};
}}"
            Case Else
                Return $"function Grid($x) {{
    return {formulaText};
}}"
        End Select
    End Function

    Public Overrides Function ToString() As String
        Return ToString(Languages.VisualBasic)
    End Function

End Class

Public Class Constants
    Public Property A As Double
    Public Property B As NumericVector
End Class
