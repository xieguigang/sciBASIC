#include "stdlib.dll"

imports stdlib

public class P

    public shared function RowSumSq(x as single(), cols as integer, i as integer) as single
        dim s as single = 0

        for k = 0 to cols - 1
            dim v = x(i * cols + k)
            s += v * v
        next

        return s
    end function

    ' two sequential ifs, single return
    public shared function DistanceV2(dot as single(), rowSumSq as single(), rows as integer,
                                      i as integer, j as integer) as single
        dim d = rowSumSq(i) + rowSumSq(j) - 2 * dot(i * rows + j)

        if d < 0 then
            d = 0
        end if
        if i = j then
            d = 0
        end if

        return System.Math.Sqrt(d)
    end function

    ' nested if / else with a single assignment each
    public shared function DistanceV3(dot as single(), rowSumSq as single(), rows as integer,
                                      i as integer, j as integer) as single
        dim d = rowSumSq(i) + rowSumSq(j) - 2 * dot(i * rows + j)
        dim r as single = 0

        if i = j then
            r = 0
        else
            if d < 0 then
                r = 0
            else
                r = System.Math.Sqrt(d)
            end if
        end if

        return r
    end function

    ' one if only
    public shared function DistanceV4(dot as single(), rowSumSq as single(), rows as integer,
                                      i as integer, j as integer) as single
        dim d = rowSumSq(i) + rowSumSq(j) - 2 * dot(i * rows + j)

        if d < 0 then
            d = 0
        end if

        return System.Math.Sqrt(d)
    end function

end class

for each name in New String() {"RowSumSq", "DistanceV2", "DistanceV3", "DistanceV4"}
    try
        dim k = cuda.compile(GetType(P).GetMethod(name))
        call console.WriteLine($"{name}: OK {k.indexMode}")
    catch ex as Exception
        call console.WriteLine($"{name}: FAIL {ex.Message}")
    end try
next

call console.WriteLine("done: t5")
