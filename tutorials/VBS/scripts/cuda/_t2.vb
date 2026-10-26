#include "stdlib.dll"

imports stdlib

call console.WriteLine($"available = {cuda.available()}")
call console.WriteLine($"device = {cuda.deviceName()}")
call console.WriteLine("done: t2")
