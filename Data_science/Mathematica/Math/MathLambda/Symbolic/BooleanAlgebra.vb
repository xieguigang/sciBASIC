' /********************************************************************************/
'
'     Module BooleanAlgebra
' 
'         Boolean logic minimisation. A truth table is enumerated over the given
'         variables and the function value, then the Quine-McCluskey algorithm
'         produces the minimal Sum-Of-Products (SOP) and Product-Of-Sums (POS)
'         forms. The result is returned both as a readable string and as an
'         expression tree (Add = OR, Mul = AND, UnaryNot = NOT).
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module BooleanAlgebra

        ''' <summary>
        ''' Enumerate the truth table of a boolean function over the given variables
        ''' and return the list of minterms (integer codes) for which the function is
        ''' true. Variable <paramref name="vars"/>(0) is the most significant bit.
        ''' </summary>
        Public Function TruthTable(vars As String(), f As Func(Of Boolean(), Boolean)) As Integer()
            Dim n = vars.Length
            Dim result As New List(Of Integer)
            Dim assign(n - 1) As Boolean

            For code As Integer = 0 To (1 << n) - 1
                For k As Integer = 0 To n - 1
                    assign(k) = ((code >> (n - 1 - k)) And 1) = 1
                Next
                If f(assign) Then result.Add(code)
            Next

            Return result.ToArray
        End Function

        ''' <summary>
        ''' Quine-McCluskey minimisation. Returns the prime implicants as strings of
        ''' 0/1/- (one per variable, most significant first).
        ''' </summary>
        Public Function QuineMcCluskey(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As String()
            Dim primes = computePrimes(minterms, dontCares, vars.Length)
            Dim cover = selectCover(primes, minterms)
            Dim out As New List(Of String)
            For Each p In cover
                out.Add(implicantToString(p, vars))
            Next
            Return out.ToArray
        End Function

        ''' <summary>
        ''' Minimal Sum-Of-Products as an expression tree.
        ''' </summary>
        Public Function QMCSimplifySOP(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As Expression
            Dim primes = computePrimes(minterms, dontCares, vars.Length)
            Dim cover = selectCover(primes, minterms)
            If cover.Count = 0 Then Return MakeLiteral(0)
            Dim acc As Expression = Nothing
            For Each p In cover
                Dim prod = implicantToExpr(p, vars, sop:=True)
                acc = If(acc Is Nothing, prod, Add(acc, prod))
            Next
            Return acc
        End Function

        ''' <summary>
        ''' Minimal Product-Of-Sums as an expression tree.
        ''' </summary>
        Public Function QMCSimplifyPOS(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As Expression
            Dim primes = computePrimes(minterms, dontCares, vars.Length)
            Dim cover = selectCover(primes, minterms)
            If cover.Count = 0 Then Return MakeLiteral(1)
            Dim acc As Expression = Nothing
            For Each p In cover
                Dim sum = implicantToExpr(p, vars, sop:=False)
                acc = If(acc Is Nothing, sum, Mul(acc, sum))
            Next
            Return acc
        End Function

        ' ------------------------------------------------------------------
        ' Quine-McCluskey core
        ' ------------------------------------------------------------------

        Private Class Implicant
            Public value As Integer
            Public mask As Integer
            Public minterms As New List(Of Integer)
        End Class

        Private Function computePrimes(minterms As Integer(), dontCares As Integer(), n As Integer) As List(Of Implicant)
            Dim current As New List(Of Implicant)
            For Each m In minterms
                current.Add(New Implicant With {.value = m, .mask = 0})
                current.Last.minterms.Add(m)
            Next
            If dontCares IsNot Nothing Then
                For Each d In dontCares
                    current.Add(New Implicant With {.value = d, .mask = 0})
                    current.Last.minterms.Add(d)
                Next
            End If

            If current.Count = 0 Then Return New List(Of Implicant)

            Dim primes As New List(Of Implicant)
            Do
                current = combineStep(current, primes)
            Loop While current.Count > 0

            Return dedupe(primes)
        End Function

        Private Function combineStep(current As List(Of Implicant), ByRef primes As List(Of Implicant)) As List(Of Implicant)
            Dim used(current.Count - 1) As Boolean
            Dim nextList As New List(Of Implicant)

            For i As Integer = 0 To current.Count - 1
                For j As Integer = i + 1 To current.Count - 1
                    If current(i).mask = current(j).mask Then
                        Dim diff = current(i).value Xor current(j).value
                        If diff <> 0 AndAlso (diff And (diff - 1)) = 0 Then
                            used(i) = True
                            used(j) = True
                            Dim combined As New Implicant With {
                                .value = current(i).value And (Not diff),
                                .mask = current(i).mask Or diff
                            }
                            combined.minterms.AddRange(current(i).minterms)
                            combined.minterms.AddRange(current(j).minterms)
                            nextList.Add(combined)
                        End If
                    End If
                Next
            Next

            For i As Integer = 0 To current.Count - 1
                If Not used(i) Then primes.Add(current(i))
            Next

            Return dedupe(nextList)
        End Function

        Private Function dedupe(list As List(Of Implicant)) As List(Of Implicant)
            Dim seen As New HashSet(Of String)
            Dim result As New List(Of Implicant)
            For Each imp In list
                Dim key = imp.value & ":" & imp.mask
                If Not seen.Contains(key) Then
                    seen.Add(key)
                    result.Add(imp)
                End If
            Next
            Return result
        End Function

        Private Function selectCover(primes As List(Of Implicant), minterms As Integer()) As List(Of Implicant)
            Dim required = New HashSet(Of Integer)(minterms)
            If required.Count = 0 Then Return New List(Of Implicant)

            ' Essential prime implicants.
            Dim covered As New HashSet(Of Integer)
            Dim cover As New List(Of Implicant)

            For Each p In primes
                Dim onlyCovered = p.minterms.Where(Function(m) required.Contains(m) AndAlso Not covered.Contains(m)).ToList
                Dim count = p.minterms.Where(Function(m) required.Contains(m)).Count
                ' A prime is essential when it is the only one covering some required minterm.
            Next

            ' Build coverage lists per required minterm.
            Dim covering As New Dictionary(Of Integer, List(Of Integer))
            For Each m In required
                covering(m) = New List(Of Integer)
            Next
            For pi As Integer = 0 To primes.Count - 1
                For Each m In primes(pi).minterms
                    If required.Contains(m) Then covering(m).Add(pi)
                Next
            Next

            ' Essential primes.
            For Each m In required
                If covering(m).Count = 1 Then
                    Dim pi = covering(m)(0)
                    If Not cover.Contains(primes(pi)) Then
                        cover.Add(primes(pi))
                        For Each mm In primes(pi).minterms
                            covered.Add(mm)
                        Next
                    End If
                End If
            Next

            ' Greedy cover of the remaining minterms.
            Dim remaining = required.Where(Function(m) Not covered.Contains(m)).ToList
            While remaining.Count > 0
                Dim bestPi = -1, bestCount = -1
                For pi As Integer = 0 To primes.Count - 1
                    Dim cnt = primes(pi).minterms.Where(Function(m) remaining.Contains(m)).Count
                    If cnt > bestCount Then
                        bestCount = cnt
                        bestPi = pi
                    End If
                Next
                If bestPi < 0 Then Exit While
                cover.Add(primes(bestPi))
                For Each m In primes(bestPi).minterms
                    covered.Add(m)
                Next
                remaining = required.Where(Function(m) Not covered.Contains(m)).ToList
            End While

            Return cover
        End Function

        ' ------------------------------------------------------------------
        ' Rendering
        ' ------------------------------------------------------------------

        Private Function implicantToString(imp As Implicant, vars As String()) As String
            Dim n = vars.Length
            Dim parts As New List(Of String)
            For k As Integer = 0 To n - 1
                Dim bit = n - 1 - k
                Dim dash = (imp.mask >> bit) And 1
                If dash = 1 Then
                    parts.Add("-")
                Else
                    parts.Add(If((imp.value >> bit) And 1, "1", "0"))
                End If
            Next
            Return String.Join("", parts)
        End Function

        Private Function implicantToExpr(imp As Implicant, vars As String(), sop As Boolean) As Expression
            Dim n = vars.Length
            Dim literals As New List(Of Expression)

            For k As Integer = 0 To n - 1
                Dim bit = n - 1 - k
                Dim dash = (imp.mask >> bit) And 1
                If dash = 1 Then Continue For

                Dim isOne = ((imp.value >> bit) And 1) = 1
                Dim lit As Expression = New SymbolExpression(vars(k))
                If sop Then
                    If Not isOne Then lit = New UnaryNot With {.value = lit}
                Else
                    If isOne Then lit = New UnaryNot With {.value = lit}
                End If
                literals.Add(lit)
            Next

            If literals.Count = 0 Then
                Return If(sop, MakeLiteral(1), MakeLiteral(0))
            End If

            Dim acc = literals(0)
            For i As Integer = 1 To literals.Count - 1
                acc = If(sop, Mul(acc, literals(i)), Add(acc, literals(i)))
            Next
            Return acc
        End Function
    End Module
End Namespace
