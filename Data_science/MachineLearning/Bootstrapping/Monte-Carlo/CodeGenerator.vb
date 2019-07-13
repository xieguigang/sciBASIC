#Region "Microsoft.VisualBasic::f94715148a8c93f8c1d6244523d6924b, Data_science\MachineLearning\Bootstrapping\Monte-Carlo\CodeGenerator.vb"

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

    '     Module CodeGenerator
    ' 
    '         Function: ConstAlpha, ConstBeta, SNonLinear, SPowerAlpha, SPowerBeta
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace MonteCarlo

    Public Module CodeGenerator

        <Extension> Public Function ConstAlpha(v$) As String
            Return v & "_alpha#"
        End Function

        <Extension> Public Function ConstBeta(v$) As String
            Return v & "_beta#"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="v$">当前的方程的微分变量</param>
        ''' <param name="x$">当前的方程项</param>
        ''' <returns></returns>
        <Extension> Public Function SPowerAlpha(v$, x$) As String
            Return v & "_" & x & "_alpha#"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="v$">当前的方程的微分变量</param>
        ''' <param name="x$">当前的方程项</param>
        ''' <returns></returns>
        <Extension> Public Function SPowerBeta(v$, x$) As String
            Return v & "_" & x & "_beta#"
        End Function

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
            Call code.AppendLine("Imports Microsoft.VisualBasic.Math.Calculus")
            Call code.AppendLine("Imports Microsoft.VisualBasic.Math.LinearAlgebra")
            Call code.AppendLine()

            If Not String.IsNullOrEmpty([namespace]) Then
                Call code.AppendLine($"Namespace {[namespace]}")
                Call code.AppendLine()
            End If

            Call code.AppendLine($"Public Class {name$} : Inherits MonteCarlo.Model")
            Call code.AppendLine()

            ' Generates the constants

            For Each v$ In var
                Call code.AppendLine($"Dim {(v).ConstAlpha }, {(v).ConstBeta}")
            Next
            Call code.AppendLine()

            ' Generates the S-powers

            For Each v$ In var
                Dim a As New List(Of String)
                Dim b As New List(Of String)

                For Each x$ In var
                    a += SPowerAlpha(v, x)
                    b += SPowerBeta(v, x)
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
                    a += $"({x} ^ {SPowerAlpha(v, x)})"
                    b += $"({x} ^ {SPowerBeta(v, x)})"
                Next

                Dim alpha$ = (v).ConstAlpha & " * " & a.JoinBy(" * ")
                Dim beta$ = (v).ConstBeta & " * " & b.JoinBy(" * ")

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
