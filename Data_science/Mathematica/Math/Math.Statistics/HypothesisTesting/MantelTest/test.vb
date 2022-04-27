Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

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

Namespace Hypothesis.Mantel

    <HideModuleName>
    Public Module test

        Public Const MIN_MAT_SIZE As Integer = 5
        Public Const MAX_EXACT_SIZE As Integer = 12
        Public Const EXACT_PROC_SIZE As Integer = 8

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function test(model As Model, matA As Double()(), matB As Double()(), matC As Double()()) As Result
            Return New Result(model).test(matA, matB, matC)
        End Function

        <Extension>
        Private Function test(model As Result, matA As Double()(), matB As Double()(), matC As Double()()) As Result
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
                If Not Mantel.test.pmt(matA, matB, matC, model) Then
                    Throw New InvalidProgramException("An error has occurred during permutation procedure.")
                End If
            Else
                If Not Mantel.test.smt(matA, matB, model) Then
                    Throw New InvalidProgramException("An error has occurred during permutation procedure.")
                End If
            End If

            Return model
        End Function

        ''' <summary>
        ''' partial Mantel test
        ''' </summary>
        ''' <param name="A">matrix A pointer, matrix B pointer, matrix C pointer, struct of parameters (see rr.h)</param>
        ''' <param name="B"></param>
        ''' <param name="C"></param>
        ''' <param name="p"></param>
        ''' <returns>1 if ok</returns>
        Public Function pmt(A As Double()(), B As Double()(), C As Double()(), ByRef p As Result) As Boolean
            Dim moyA As Double
            Dim moyC As Double
            Dim i As Integer
            Dim j As Integer
            Dim N As Integer
            Dim r_abc As Double = 0
            Dim r_ab As Double = 0
            Dim r_ac As Double = 0
            Dim r_bc As Double = 0
            Dim ret = 0
            N = p.matsize - 1

            ' computing mean and standard deviation 
            moyA = moy(A, N)
            moyC = moy(C, N)

            ' computing residuals 
            If p.raw = 0 Then resid(A, C, N, moyA, moyC)

            ' normalization 
            norm(A, N)
            norm(B, N)
            norm(C, N)

            ' computing initial r_ab 
            For i = 0 To N - 1

                For j = 0 To i
                    r_ab += A(i)(j) * B(i)(j)
                Next
            Next

            r_ab = r_ab / (p.numelt - 1)

            ' computing initial r_ac 
            For i = 0 To N - 1

                For j = 0 To i
                    r_ac += A(i)(j) * C(i)(j)
                Next
            Next

            r_ac = r_ac / (p.numelt - 1)

            ' computing initial r_bc 
            For i = 0 To N - 1

                For j = 0 To i
                    r_bc += B(i)(j) * C(i)(j)
                Next
            Next

            r_bc = r_bc / (p.numelt - 1)

            ' computing initial r_abc 
            r_abc = (r_ab - r_ac * r_bc) / (stdNum.Sqrt(1 - r_ac * r_ac) * stdNum.Sqrt(1 - r_bc * r_bc))
            p.coef = r_abc

            ' force exact permutation ? 
            If p.exact = 0 Then
                ret = pmt_perm(A, B, C, r_bc, p)
            Else
                ret = pmt_perm_exact(A, B, C, r_bc, p)
            End If

            Return ret
        End Function

        ''' <summary>
        ''' simple Mantel test
        ''' </summary>
        ''' <param name="A">matrix A pointer, matrix B pointer, struct for results  </param>
        ''' <param name="B"></param>
        ''' <param name="p"></param>
        ''' <returns>1 if ok</returns>
        Public Function smt(A As Double()(), B As Double()(), ByRef p As Result) As Boolean
            Dim i As Integer
            Dim j As Integer
            Dim zini As Double
            Dim N = p.matsize - 1
            Dim ret = 0

            Call norm(A, N)
            Call norm(B, N)

            ' computing initial z 
            zini = 0

            For i = 0 To N - 1

                For j = 0 To i
                    zini += A(i)(j) * B(i)(j)
                Next
            Next

            p.coef = zini / (p.numelt - 1)

            If p.exact = 0 Then
                ret = smt_perm(A, B, p)
            Else
                ret = smt_perm_exact(A, B, p)
            End If

            Return ret
        End Function
    End Module
End Namespace