#include "stdlib.dll"

imports stdlib

public class K

    public shared function RowSumA(x as single(), cols as integer, i as integer) as single
        dim s as single = 0

        for k = 0 to cols - 1
            s += x(i * cols + k)
        next

        return s
    end function

    public shared function RowSumB(x as single(), cols as integer, i as integer) as single
        dim s as single = 0
        dim k = 0

        while k < cols
            s = s + x(i * cols + k)
            k = k + 1
        end while

        return s
    end function

    public shared function Cell(dot as single(), i as integer, j as integer) as single
        return dot(i * 2 + j)
    end function

end class

dim data = New Single() {1, 2, 3, 4, 5, 6}

for each name in New String() {"RowSumA", "RowSumB", "Cell"}
    try
        dim k = cuda.compile(GetType(K).GetMethod(name))
        call console.WriteLine($"{name}: OK {k.indexMode}")
    catch ex as Exception
        call console.WriteLine($"{name}: FAIL {ex.Message}")
    end try
next

call console.WriteLine("done: t4")
