Public Class Utils
    Function findSequence(sequence As Array, states)
        Return sequence.reduce(Function(all, curr)
                                   Return all.push(states.findIndex(x >= x.state = curr))
                                   Return all
                               End Function, {})
    End Function

    Function gamma(alpha#, beta#, forward#) As Double
        Return (alpha * beta) / forward
    End Function

    Function xi(alpha#, trans#, emiss#, beta#, forward#) As Double
        Return (alpha * trans * emiss * beta) / forward
    End Function

    Function calculateProb(stateTrans2, init, states)
        Return New prob With {
   .sequenceProb = Function(sequence)
                       Return findSequence(sequence, states).reduce(Function(total, curr As Array, i, arr)
                                                                        If (i = 0) Then
                                                                            total += init(curr)
                                                                        Else
                                                                            total *= stateTrans2(arr(i - 1))(curr)
                                                                        End If

                                                                        Return total
                                                                    End Function, 0)
                   End Function
}
    End Function
End Class

