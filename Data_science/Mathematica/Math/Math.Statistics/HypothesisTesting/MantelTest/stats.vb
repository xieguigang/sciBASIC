#Region "Microsoft.VisualBasic::e58e08771c0d93646acb94cc2e5e2e9e, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\MantelTest\stats.vb"

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


' Code Statistics:

'   Total Lines: 600
'    Code Lines: 389 (64.83%)
' Comment Lines: 115 (19.17%)
'    - Xml Docs: 79.13%
' 
'   Blank Lines: 96 (16.00%)
'     File Size: 19.92 KB


'     Module stats
' 
'         Function: ect, moy, pmt_perm, pmt_perm_exact, smt_perm
'                   smt_perm_exact, sompx, sompxy, somx, somx2
' 
'         Sub: norm, resid, shake
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math
Imports rand2 = Microsoft.VisualBasic.Math.RandomExtensions

'      zt - Simple and partial Mantel Test - version 1.1
'      copyright (c) Eric Bonnet 2001 - 2007
' 
'      This program is free software; you can redistribute it and/or modify
'      it under the terms of the GNU General Public License as published by
'      the Free Software Foundation; either version 2 of the License, or
'      (at your option) any later version.
' 
'      This program is distributed in the hope that it will be useful,
'      but WITHOUT ANY WARRANTY; without even the implied warranty of
'      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'      GNU General Public License for more details.
' 
'      You should have received a copy of the GNU General Public License
'      along with this program; if not, write to the Free Software
'      Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307
'        
' 
'      Here are all computation routines.
'      The permutation procedure was written by Glenn C Rhoads,
'      and is used here with the kind permission from the author.
'      http://remus.rutgers.edu/~rhoads/Code/perm_lex.c
'      Thanks, Glenn !
' 

Namespace Hypothesis.Mantel

    Public Module stats

        ''' <summary>
        ''' compute simple sum of elements in a half-matrix
        ''' </summary>
        ''' <param name="a">matrix pointer, size of the half-matrix</param>
        ''' <param name="[stop]"></param>
        ''' <returns>sum as double</returns>
        Public Function somx(a As Double()(), [stop] As Integer) As Double
            Dim i As Integer
            Dim j As Integer
            Dim ret As Double = 0

            For i = 0 To [stop] - 1

                For j = 0 To i
                    ret += a(i)(j)
                Next
            Next

            Return ret
        End Function

        ''' <summary>
        ''' compute square sum of elements in a half-matrix
        ''' </summary>
        ''' <param name="a">matrix pointer, size of the half-matrix</param>
        ''' <param name="[stop]"></param>
        ''' <returns>square sum as double</returns>
        Public Function somx2(a As Double()(), [stop] As Integer) As Double
            Dim i As Integer
            Dim j As Integer
            Dim ret As Double = 0

            For i = 0 To [stop] - 1

                For j = 0 To i
                    ret += a(i)(j) * a(i)(j)
                Next
            Next

            Return ret
        End Function

        ''' <summary>
        ''' compute mean for a half-matrix
        ''' </summary>
        ''' <param name="a">matrix pointer, size of the half-matrix</param>
        ''' <param name="[stop]"></param>
        ''' <returns>mean as double</returns>
        Public Function moy(a As Double()(), [stop] As Integer) As Double
            Dim i As Integer
            Dim j As Integer
            Dim N As Integer
            Dim ret As Double = 0
            N = [stop] * ([stop] - 1) / 2 + [stop]

            For i = 0 To [stop] - 1

                For j = 0 To i
                    ret += a(i)(j)
                Next
            Next

            ret = ret / N
            Return ret
        End Function

        ''' <summary>
        ''' compute standard deviation for a half-matrix
        ''' </summary>
        ''' <param name="a">matrix pointer, size of the half-matrix</param>
        ''' <param name="[stop]"></param>
        ''' <returns>standard deviation as double</returns>
        Public Function ect(a As Double()(), [stop] As Integer) As Double
            Dim N As Integer
            Dim ret As Double = 0
            Dim lsomx = somx(a, [stop])
            Dim lsomx2 = somx2(a, [stop])
            N = [stop] * ([stop] - 1) / 2 + [stop]
            ret = std.Sqrt((lsomx2 - lsomx * lsomx / N) / (N - 1))
            Return ret
        End Function

        ''' <summary>
        ''' shaking elements of a vector at random 
        ''' </summary>
        ''' <param name="a">array pointer, size of the array</param>
        ''' <param name="f"></param>
        Public Sub shake(a As Integer(), f As Integer)
            Dim i As Integer
            Dim aleat As Integer
            Dim tmp As Integer

            For i = 0 To f - 1 - 1
                aleat = i + (1 + rand2.NextNumber() Mod (f - i - 1))
                tmp = a(i)
                a(i) = a(aleat)
                a(aleat) = tmp
            Next
        End Sub

        ''' <summary>
        ''' compute square sum of mean deviations for a half matrix
        ''' </summary>
        ''' <param name="a">matrix pointer, size of the half-matrix</param>
        ''' <param name="[stop]"></param>
        ''' <returns>sum as double</returns>
        Public Function sompx(a As Double()(), [stop] As Integer) As Double
            Dim ret As Double = 0
            Dim N As Integer
            Dim lsomx = somx(a, [stop])
            Dim lsomx2 = somx2(a, [stop])
            N = [stop] * ([stop] - 1) / 2 + [stop]
            ret = lsomx2 - lsomx * lsomx / N
            Return ret
        End Function

        ''' <summary>
        ''' sum x-mean(x) * y-mean(y) for two half matrices
        ''' </summary>
        ''' <param name="a">matrix A pointer, matrix B pointer, size of the matrices, mean matrix A, mean matrix B</param>
        ''' <param name="b"></param>
        ''' <param name="[stop]"></param>
        ''' <param name="lmoyA"></param>
        ''' <param name="lmoyB"></param>
        ''' <returns>sum as double</returns>
        Public Function sompxy(a As Double()(), b As Double()(), [stop] As Integer, lmoyA As Double, lmoyB As Double) As Double
            Dim i As Integer
            Dim j As Integer
            Dim ret As Double = 0

            For i = 0 To [stop] - 1

                For j = 0 To i
                    ret += a(i)(j) * b(i)(j) - lmoyA * lmoyB
                Next
            Next

            Return ret
        End Function

        ''' <summary>
        ''' compute residuals of half-matrix A against half-matrix B
        ''' </summary>
        ''' <param name="a">matrix A pointer, matrix B pointer, size of the matrices, mean matrix A, mean matrix B</param>
        ''' <param name="b"></param>
        ''' <param name="[stop]"></param>
        ''' <param name="lmoyA"></param>
        ''' <param name="lmoyB"></param>
        Public Sub resid(a As Double()(), b As Double()(), [stop] As Integer, lmoyA As Double, lmoyB As Double)
            Dim coef_b As Double
            Dim coef_a As Double
            Dim i As Integer
            Dim j As Integer
            coef_b = sompxy(a, b, [stop], lmoyA, lmoyB) / sompx(b, [stop])
            coef_a = lmoyA - coef_b * lmoyB

            For i = 0 To [stop] - 1

                For j = 0 To i
                    a(i)(j) = a(i)(j) - (coef_a + coef_b * b(i)(j))
                Next
            Next
        End Sub

        ''' <summary>
        ''' normalization of a half-matrix
        ''' </summary>
        ''' <param name="a">
        ''' matrix pointer, size of the matrix      
        ''' </param>
        ''' <param name="[stop]"></param>
        Public Sub norm(a As Double()(), [stop] As Integer)
            Dim lmoya As Double
            Dim lecta As Double
            Dim i As Integer
            Dim j As Integer
            lmoya = moy(a, [stop])
            lecta = ect(a, [stop])

            For i = 0 To [stop] - 1

                For j = 0 To i
                    a(i)(j) = (a(i)(j) - lmoya) / lecta
                Next
            Next
        End Sub

        ''' <summary>
        ''' randomization procedure for partial Mantel test
        ''' </summary>
        ''' <param name="A">matrix A pointer, matrix B pointer, matrix C pointer, r_bc pointer, struct for parameters</param>
        ''' <param name="B"></param>
        ''' <param name="C"></param>
        ''' <param name="r_bc"></param>
        ''' <param name="p"></param>
        ''' <returns>1 if ok</returns>
        Public Function pmt_perm(A As Double()(), B As Double()(), C As Double()(), ByRef r_bc As Double, ByRef p As Result) As Integer
            Dim i As Integer
            Dim j As Integer
            Dim r As Integer
            Dim cptega As Integer
            Dim cptinf As Integer
            Dim cptsup As Integer
            Dim r_ab As Double
            Dim r_ac As Double
            Dim rrand As Double
            Dim epsilon As Double
            Dim ord As Integer()
            cptega = 1
            cptsup = 0
            cptinf = 0
            epsilon = 0.0000001
            ord = New Integer(p.matsize - 1) {}
            If ord Is Nothing Then Return -1

            For i = 0 To p.matsize - 1
                ord(i) = i
            Next

            For r = 0 To p.numrand - 1
                rrand = 0
                shake(ord, p.matsize)
                r_ab = 0

                For i = 1 To p.matsize - 1

                    For j = 0 To i - 1

                        If ord(j) < ord(i) Then
                            r_ab += B(i - 1)(j) * A(ord(i) - 1)(ord(j))
                        Else
                            r_ab += B(i - 1)(j) * A(ord(j) - 1)(ord(i))
                        End If
                    Next
                Next

                r_ab = r_ab / (p.numelt - 1)
                r_ac = 0

                For i = 1 To p.matsize - 1

                    For j = 0 To i - 1

                        If ord(j) < ord(i) Then
                            r_ac += C(i - 1)(j) * A(ord(i) - 1)(ord(j))
                        Else
                            r_ac += C(i - 1)(j) * A(ord(j) - 1)(ord(i))
                        End If
                    Next
                Next

                r_ac = r_ac / (p.numelt - 1)
                rrand = (r_ab - r_ac * r_bc) / (std.Sqrt(1 - r_ac * r_ac) * std.Sqrt(1 - r_bc * r_bc))

                If std.Abs(rrand - p.coef) <= epsilon * std.Abs(rrand) Then
                    cptega += 1
                Else
                    If rrand > p.coef Then cptsup += 1
                    If rrand < p.coef Then cptinf += 1
                End If
            Next

            Erase ord

            If p.coef < 0 Then
                p.proba = (cptinf + cptega) / (p.numrand + 1)
            Else
                p.proba = (cptsup + cptega) / (p.numrand + 1)
            End If

            Return 1
        End Function

        ''' <summary>
        ''' exact permutation procedure for partial Mantel test
        ''' </summary>
        ''' <param name="A">matrix A pointer, matrix B pointer, matrix C pointer, r_bc pointer, struct for results</param>
        ''' <param name="B"></param>
        ''' <param name="C"></param>
        ''' <param name="r_bc"></param>
        ''' <param name="p"></param>
        ''' <returns>1 if ok </returns>
        Public Function pmt_perm_exact(A As Double()(), B As Double()(), C As Double()(), ByRef r_bc As Double, ByRef p As Result) As Integer
            Dim i As Integer
            Dim j As Integer
            Dim r As Integer
            Dim s As Integer
            Dim li As Integer
            Dim lj As Integer
            Dim temp As Integer
            Dim n As Integer
            Dim cpt As Integer
            Dim cptega As Integer
            Dim cptinf As Integer
            Dim cptsup As Integer
            Dim ord As Integer()
            Dim r_ab As Double
            Dim r_ac As Double
            Dim rrand As Double
            Dim epsilon As Double
            epsilon = 0.0000001
            ord = New Integer(p.matsize - 1) {}
            If ord Is Nothing Then Return -1

            For i = 0 To p.matsize - 1
                ord(i) = i
            Next

            i = 1
            n = p.matsize - 1
            cptega = 0
            cptsup = 0
            cptinf = 0
            cpt = 0

            While i >= 0
                cpt += 1
                rrand = 0
                r_ab = 0
                r_ac = 0

                For li = 1 To p.matsize - 1

                    For lj = 0 To li - 1

                        If ord(lj) < ord(li) Then
                            r_ab += B(li - 1)(lj) * A(ord(li) - 1)(ord(lj))
                        Else
                            r_ab += B(li - 1)(lj) * A(ord(lj) - 1)(ord(li))
                        End If
                    Next
                Next

                r_ab = r_ab / (p.numelt - 1)

                For li = 1 To p.matsize - 1

                    For lj = 0 To li - 1

                        If ord(lj) < ord(li) Then
                            r_ac += C(li - 1)(lj) * A(ord(li) - 1)(ord(lj))
                        Else
                            r_ac += C(li - 1)(lj) * A(ord(lj) - 1)(ord(li))
                        End If
                    Next
                Next

                r_ac = r_ac / (p.numelt - 1)
                rrand = (r_ab - r_ac * r_bc) / (std.Sqrt(1 - r_ac * r_ac) * std.Sqrt(1 - r_bc * r_bc))

                If std.Abs(rrand - p.coef) <= epsilon * std.Abs(rrand) Then
                    cptega += 1
                Else
                    If rrand > p.coef Then cptsup += 1
                    If rrand < p.coef Then cptinf += 1
                End If

                i = n - 1

                While ord(i) > ord(i + 1)
                    i -= 1
                End While

                j = n

                While ord(i) > ord(j)
                    j -= 1
                End While

                If i >= 0 Then
                    temp = ord(i)
                    ord(i) = ord(j)
                    ord(j) = temp
                    r = n
                    s = i + 1

                    While r > s
                        temp = ord(r)
                        ord(r) = ord(s)
                        ord(s) = temp
                        r -= 1
                        s += 1
                    End While
                End If
            End While

            If p.coef < 0 Then
                p.proba = (cptinf + cptega) / cpt
            Else
                p.proba = (cptsup + cptega) / cpt
            End If

            Erase ord

            Return 1
        End Function

        ''' <summary>
        ''' randomization procedure for simple Mantel test
        ''' </summary>
        ''' <param name="A">matrix A pointer, matrix B pointer, struct for results</param>
        ''' <param name="B"></param>
        ''' <param name="p"></param>
        ''' <returns>1 if ok</returns>
        Public Function smt_perm(A As Double()(), B As Double()(), ByRef p As Result) As Integer
            Dim i As Integer
            Dim j As Integer
            Dim r As Integer
            Dim cptega As Integer
            Dim cptinf As Integer
            Dim cptsup As Integer
            Dim zrand As Double
            Dim epsilon As Double
            Dim ord As Integer()
            epsilon = 0.0000001
            ord = New Integer(p.matsize - 1) {}
            If ord Is Nothing Then Return -1

            For i = 0 To p.matsize - 1
                ord(i) = i
            Next

            cptega = 1
            cptsup = 0
            cptinf = 0
            zrand = 0

            For r = 0 To p.numrand - 1
                zrand = 0
                shake(ord, p.matsize)

                For i = 1 To p.matsize - 1

                    For j = 0 To i - 1

                        If ord(j) < ord(i) Then
                            zrand += A(i - 1)(j) * B(ord(i) - 1)(ord(j))
                        Else
                            zrand += A(i - 1)(j) * B(ord(j) - 1)(ord(i))
                        End If
                    Next
                Next

                zrand = zrand / (p.numelt - 1)

                If std.Abs(zrand - p.coef) <= epsilon * std.Abs(zrand) Then
                    cptega += 1
                Else
                    If zrand > p.coef Then cptsup += 1
                    If zrand < p.coef Then cptinf += 1
                End If
            Next

            If p.coef < 0 Then
                p.proba = (cptinf + cptega) / (p.numrand + 1)
            Else
                p.proba = (cptsup + cptega) / (p.numrand + 1)
            End If

            Erase ord

            Return 1
        End Function

        ''' <summary>
        ''' exact permutation procedure for simple Mantel test
        ''' </summary>
        ''' <param name="A">matrix pointer A, matrix pointer B, struct for results</param>
        ''' <param name="B"></param>
        ''' <param name="p"></param>
        ''' <returns>1 if ok</returns>
        Public Function smt_perm_exact(A As Double()(), B As Double()(), ByRef p As Result) As Integer
            Dim i As Integer
            Dim j As Integer
            Dim r As Integer
            Dim s As Integer
            Dim li As Integer
            Dim lj As Integer
            Dim temp As Integer
            Dim n As Integer
            Dim cpt As Integer
            Dim cptega As Integer
            Dim cptinf As Integer
            Dim cptsup As Integer
            Dim ord As Integer()
            Dim zrand As Double
            Dim epsilon As Double
            epsilon = 0.0000001
            ord = New Integer(p.matsize - 1) {}
            If ord Is Nothing Then Return -1

            For i = 0 To p.matsize - 1
                ord(i) = i
            Next

            i = 1
            n = p.matsize - 1
            cptega = 0
            cptsup = 0
            cptinf = 0
            cpt = 0

            While i >= 0
                cpt += 1
                zrand = 0

                For li = 1 To p.matsize - 1

                    For lj = 0 To li - 1

                        If ord(lj) < ord(li) Then
                            zrand += A(li - 1)(lj) * B(ord(li) - 1)(ord(lj))
                        Else
                            zrand += A(li - 1)(lj) * B(ord(lj) - 1)(ord(li))
                        End If
                    Next
                Next

                zrand = zrand / (p.numelt - 1)

                If std.Abs(zrand - p.coef) <= epsilon * std.Abs(zrand) Then
                    cptega += 1
                Else
                    If zrand > p.coef Then cptsup += 1
                    If zrand < p.coef Then cptinf += 1
                End If

                i = n - 1

                While ord(i) > ord(i + 1)
                    i -= 1
                End While

                j = n

                While ord(i) > ord(j)
                    j -= 1
                End While

                If i >= 0 Then
                    temp = ord(i)
                    ord(i) = ord(j)
                    ord(j) = temp
                    r = n
                    s = i + 1

                    While r > s
                        temp = ord(r)
                        ord(r) = ord(s)
                        ord(s) = temp
                        r -= 1
                        s += 1
                    End While
                End If
            End While

            If p.coef < 0 Then
                p.proba = (cptinf + cptega) / cpt
            Else
                p.proba = (cptsup + cptega) / cpt
            End If

            Erase ord

            Return 1
        End Function
    End Module
End Namespace
