Dim (a, b as double) = (1, 2)  

console.writeline("demo test result of variable tuple deconstruct in vb:")

console.writeLine($"a := {a}")
console.writeLine($"b := {b}")
console.writeLine($"(a+b) := {a + b}")

dim tuples = {
    ("a", 123), ("b", 456), ("c", 789)
}

for each (str as string, int) in tuples
    call console.writeline($"{str} => {int:F4}")
next