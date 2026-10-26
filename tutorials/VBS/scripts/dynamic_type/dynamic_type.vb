#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ============================================================================
'  Dynamic type tutorial
'
'  This script demonstrates the two variable declarations available in the VBS
'  (VisualBasic.Scripting) language and how a dynamically typed variable can be
'  reassigned to values of different shapes at runtime.
'
'      Dim a As Integer  -> a CLR strong-typed variable; the declared type is
'                           fixed for the whole lifetime of the variable
'      let b = 456       -> a CLR dynamic/weak-typed variable; the runtime
'                           infers the type from the assigned value and infers
'                           it again on every new assignment
'
'  Expected output:
'      a := 123
'      b := 456
'      b.a := 123
'      b.b := 456
'      check boolean := False
' ============================================================================

' Declare a CLR strong-typed variable with ``Dim`` in the Visual Basic language.
' The variable is permanently an Integer, so a value of another type would fail.

Dim a as integer
a = 123
console.writeline($"a := {a}")

' Declare a CLR dynamic/weak-typed variable with ``let`` in the Visual Basic
' language. Here ``b`` starts as an Integer, is then reassigned to an anonymous
' object and finally to a Boolean -- its runtime type follows the assigned value.

let b = 456
console.writeline($"b := {b}")

' ``b`` is re-inferred as an anonymous type with two fields (a and b).
b = new with {.a = 123, .b = 456}

console.writeline($"b.a := {b.a}")
console.writeline($"b.b := {b.b}")

' ``b`` is re-inferred once more, this time as a Boolean.
b = false

console.writeline($"check boolean := {b}")

' ---------------------------------------------------------------------------
' The dynamic values are ordinary CLR numbers, so they can be handed to the
' standard library directly: build a one column table and compute statistics.
' ---------------------------------------------------------------------------
let values = New Double() {a, 456, 789}

console.writeline($"stats: count = {values.length}, sum = {stats.sum(values)}, mean = {stats.mean(values):F4}")

dim dataset = table.fromColumns(New String() {"value"}, New Double()() {values}, table.names("row", 3))

console.writeline($"table: {dataset.nsamples} samples x {dataset.nfeatures} feature")
console.writeline("csv : " & write.csv(dataset, here("dynamic_type.csv")))

console.writeline("done: dynamic_type")
