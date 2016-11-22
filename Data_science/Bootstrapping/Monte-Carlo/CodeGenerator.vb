Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace MonteCarlo

    Public Module CodeGenerator

        ''' <summary>
        ''' Generates the S-system non-linear model VisualBasic Class.
        ''' </summary>
        ''' <param name="var$"></param>
        ''' <param name="name">
        ''' Class Name, this value should match the VisualBasic identifier rule.
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function SNonLinear(var$(), name$, Optional [namespace] As String = Nothing) As String
            Dim code As New StringBuilder

            Call code.AppendLine("Imports Microsoft.VisualBasic.Data.Bootstrapping")
            Call code.AppendLine("Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo")
            Call code.AppendLine("Imports Microsoft.VisualBasic.Mathematical.Calculus")
            Call code.AppendLine("Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra")
            Call code.AppendLine()

            If Not String.IsNullOrEmpty([namespace]) Then
                Call code.AppendLine($"Namespace {[namespace]}")
                Call code.AppendLine()
            End If

            Call code.AppendLine($"Public Class {name$} : Inherits MonteCarlo.Model")
            Call code.AppendLine()

            ' Generates the constants
            Dim calpha = Function(v$) v & "_alpha#"
            Dim cbeta = Function(v$) v & "_beta#"

            For Each v$ In var
                Call code.AppendLine($"Dim {calpha(v)}, {cbeta(v)}")
            Next
            Call code.AppendLine()

            ' Generates the S-powers
            Dim spalpha = Function(v$, x$) v & "_" & x & "_alpha#"
            Dim spbeta = Function(v$, x$) v & "_" & x & "_beta#"

            For Each v$ In var
                Dim a As New List(Of String)
                Dim b As New List(Of String)

                For Each x$ In var
                    a += spalpha(v, x)
                    b += spbeta(v, x)
                Next

                Dim line$ = $"Dim {a.JoinBy(", ")}, {b.JoinBy(", ")}"

                Call code.AppendLine(line)
            Next
            Call code.AppendLine()

            ' Generates the variables
            For Each v$ In var
                Call code.AppendLine($"Dim {v} As var")
            Next

            ' Generates the S-system non-linear model
            Call code.AppendLine()
            Call code.AppendLine("Protected Overrides Sub func(dx As Double, ByRef dy As Vector)")
            Call code.AppendLine()

            For Each v$ In var
                Dim a As New List(Of String)
                Dim b As New List(Of String)

                For Each x In var
                    a += $"({x} ^ {spalpha(v, x)})"
                    b += $"({x} ^ {spbeta(v, x)})"
                Next

                Dim alpha$ = calpha(v) & " * " & a.JoinBy(" * ")
                Dim beta$ = cbeta(v) & " * " & b.JoinBy(" * ")

                Call code.AppendLine($"dy({v}) = {alpha} - {beta}")
            Next

            Call code.AppendLine()
            Call code.AppendLine("End Sub")
            Call code.AppendLine()
            Call code.AppendLine("        
Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
    Throw New NotImplementedException
End Function

Public Overrides Function params() As VariableModel()
    Throw New NotImplementedException
End Function

Public Overrides Function yinit() As VariableModel()
    Throw New NotImplementedException
End Function")
            Call code.AppendLine()
            Call code.AppendLine("End Class")

            If Not String.IsNullOrEmpty([namespace]) Then
                Call code.AppendLine("End Namespace")
            End If

            Return code.ToString
        End Function
    End Module
End Namespace