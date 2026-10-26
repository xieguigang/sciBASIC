#include "stdlib.dll"

imports stdlib

public class A

    ''' <summary>row sum</summary>
    public shared function RowSum(x as single(), cols as integer, i as integer) as single
        dim s as single = 0

        for k = 0 to cols - 1
            s += x(i * cols + k)
        next

        return s
    end function

    ''' <summary>multi line signature</summary>
    public shared function CorrelationCell(dot as single(), rowSum as single(), rowSumSq as single(),
                                           rows as integer, cols as integer, i as integer, j as integer) as single
        dim n = cols
        dim covariance = dot(i * rows + j) - n * (rowSum(i) / n) * (rowSum(j) / n)
        dim variance_i = rowSumSq(i) - n * (rowSum(i) / n) * (rowSum(i) / n)
        dim denominator = System.Math.Sqrt(variance_i)

        if denominator <= 0 then
            return 0
        end if

        return covariance / denominator
    end function

end class

dim data = New Single() {1, 2, 3, 4, 5, 6}
dim sums = New Single() {1, 2}
dim sumsSq = New Single() {1, 4}

call console.WriteLine($"rowSum = {A.RowSum(data, 3, 0)}")
call console.WriteLine($"cell = {A.CorrelationCell(data, sums, sumsSq, 2, 3, 0, 1)}")
call console.WriteLine("done: t1")
