#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

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
'  The second half feeds the very same tuple data into the standard library
'  (table + stats + stdio.write) to show that language values and library api
'  fit together without any glue code.
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

' ---------------------------------------------------------------------------
' From language values to a standard library table:
'   row names = the tuple keys, one feature column = the tuple values
' ---------------------------------------------------------------------------
dim row_names as new list(of string)(tuples.length - 1) 
dim values as new list(of double)(tuples.length - 1) 

for each (str as string, int) in tuples
    row_names.add(str)
    values.add(int) 
next

dim dataset = table.fromColumns(New String() {"value"}, New Double()() {values.toarray}, row_names)

call console.WriteLine($"table: {dataset.nsamples} samples x {dataset.nfeatures} feature, names = {String.Join(", ", table.rowNames(dataset))}")
call console.WriteLine($"stats: sum = {values.sum}, mean = {values.mean}, median = {values.median}")

call console.WriteLine("csv : " & write.csv(dataset, here("tuple.csv")))

call console.WriteLine("done: tuple")
