' declare a CLR strong type variable by ``Dim`` in visual basic language

Dim a as integer
a = 123
console.writeline($"a := {a}")

' declare a CLR dynamic type variable by ``let`` in visual basic language
let b = 456
console.writeline($"b := {b}")

b = new with {.a = 123, .b = 456}

console.writeline($"b.a := {b.a}")
console.writeline($"b.b := {b.b}")

b = false

console.writeline($"check boolean := {b}")