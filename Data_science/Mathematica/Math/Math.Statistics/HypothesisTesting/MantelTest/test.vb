
' 
'     zt - Simple and partial Mantel Test - version 1.1
'     copyright (c) Eric Bonnet 2001 - 2007
' 
'     This program is free software; you can redistribute it and/or modify
'     it under the terms of the GNU General Public License as published by
'     the Free Software Foundation; either version 2 of the License, or
'     (at your option) any later version.
' 
'     This program is distributed in the hope that it will be useful,
'     but WITHOUT ANY WARRANTY; without even the implied warranty of
'     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'     GNU General Public License for more details.
' 
'     You should have received a copy of the GNU General Public License
'     along with this program; if not, write to the Free Software
'     Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307
'        

Imports System.Runtime.CompilerServices

Namespace Hypothesis.Mantel

    <HideModuleName>
    Public Module test

        Public Const MIN_MAT_SIZE As Integer = 5
        Public Const MAX_EXACT_SIZE As Integer = 12
        Public Const EXACT_PROC_SIZE As Integer = 8

        <Extension>
        Public Function test(model As Model, matA As Double()(), matB As Double()(), matC As Double()()) As Integer
            Dim res As Integer
            Dim NA = matA.Length
            Dim NB = matB.Length
            Dim NC = matC.Length

            model.numelt = model.matsize * (model.matsize - 1) / 2

            ' test matrices size 
            If model.partial = False Then
                If NA <> NB Then
                    Throw New InvalidProgramException("Error: matrices of different size.")
                End If
            Else
                If NA <> NB OrElse NA <> NC OrElse NB <> NC Then
                    Throw New InvalidProgramException("Error: matrices of different size.")
                End If
            End If

            If model.matsize < MIN_MAT_SIZE Then
                Throw New InvalidProgramException($"Error: matrix size must be >= {MIN_MAT_SIZE}!")
            End If
            If model.exact AndAlso model.matsize > MAX_EXACT_SIZE Then
                Throw New InvalidProgramException($"Error: matrix size is too big for exact permutations (should be <= {MAX_EXACT_SIZE}).")
            End If

            ' force exact permutation procedure if size too small 
            If model.matsize < EXACT_PROC_SIZE Then
                model.exact = True
            End If
            ' define and test number of randomizations 
            If model.exact = False Then
                model.numrand = Now().GetHashCode

                If model.numrand < 99 OrElse model.numrand > 999999999 Then
                    Throw New InvalidProgramException("Error: Number of iterations must be between 99 and 999999999.")
                End If
            End If

            If model.exact Then model.numrand = fact(model.matsize)

            If model.partial = 0 Then
                Console.Write("simple ")
            Else
                Console.Write("partial ")

                If model.raw = 1 Then
                    Console.Write("raw ")
                Else
                    Console.Write("residuals ")
                End If
            End If

            If model.exact Then
                Console.Write("exact ")
            End If

            Console.Write(vbLf & vbLf)
            Console.Write("Randomizing..." & vbLf & vbLf)

            ' launch the test 
            If model.partial Then
                res = stats.pmt(matA, matB, matC, model)

                If res <> 0 Then
                    Console.Write("r =" & vbTab & vbTab & vbTab & "{0:f}" & vbLf, model.coef)
                    Console.Write("p =" & vbTab & vbTab & vbTab & "{0:f} (one-tailed)" & vbLf & vbLf, model.proba)
                Else
                    Console.Write("An error has occurred during permutation procedure." & vbLf & "Please retry." & vbLf & vbLf)
                End If
            Else
                res = stats.smt(matA, matB, model)

                If res <> 0 Then
                    Console.Write("r =" & vbTab & vbTab & vbTab & "{0:f}" & vbLf, model.coef)
                    Console.Write("p =" & vbTab & vbTab & vbTab & "{0:f} (one-tailed)" & vbLf & vbLf, model.proba)
                Else
                    Console.Write("An error has occurred during permutation procedure." & vbLf & "Please retry." & vbLf & vbLf)
                End If
            End If

            Return res
        End Function

    End Module
End Namespace