#include "stdlib.dll"

imports stdlib

dim names = table.names("p", 3)

call console.WriteLine($"table ok: {String.Join(",", names)}")
call console.WriteLine($"done: t3")
