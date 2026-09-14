' ============================================================================
'  Tuple tutorial
'
'  This script demonstrates tuple support in the VBS (VisualBasic.Scripting)
'  language:
'      1) deconstructing a tuple into several named variables in a single
'         ``Dim`` statement
'      2) building an array of tuples from a literal
'      3) deconstructing each tuple while iterating it with ``For Each``
'
'  Run:
'      vbs.exe tutorials\VBS\tuple\tuple.vb
'
'  Expected output:
'      demo test result of variable tuple deconstruct in vb:
'      a := 1
'      b := 2
'      (a+b) := 3
'      a => 123.0000
'      b => 456.0000
'      c => 789.0000
' ============================================================================

' Deconstruct the tuple ``(1, 2)`` into two typed variables in a single line.
' ``a`` and ``b`` become ordinary Double variables holding 1 and 2.

Dim (a, b as double) = (1, 2)  

console.writeline("demo test result of variable tuple deconstruct in vb:")

console.writeLine($"a := {a}")
console.writeLine($"b := {b}")
console.writeLine($"(a+b) := {a + b}")

' An array literal whose elements are tuples of (String, Integer).

dim tuples = {
    ("a", 123), ("b", 456), ("c", 789)
}

' Each item is deconstructed into a String and an Integer while iterating.

for each (str as string, int) in tuples
    call console.writeline($"{str} => {int:F4}")
next
