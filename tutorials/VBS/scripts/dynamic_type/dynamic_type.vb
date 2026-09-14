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
'  Run:
'      vbs.exe tutorials\VBS\dynamic_type\dynamic_type.vb
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
' The dynamic variable exposes those fields directly.

b = new with {.a = 123, .b = 456}

console.writeline($"b.a := {b.a}")
console.writeline($"b.b := {b.b}")

' ``b`` is re-inferred once more, this time as a Boolean.

b = false

console.writeline($"check boolean := {b}")
