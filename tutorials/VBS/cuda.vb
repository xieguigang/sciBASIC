#include "Microsoft.VisualBasic.Computing.ILCuda.dll"

imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Public Function RunListKernels() As Integer
    ConsoleReporter.PrintTitle("内核注册表（框架内置 + demo 注册）")

    For Each info As KernelInfo In KernelCatalog.All()
        Console.WriteLine($"  {info}")
    Next

    Console.WriteLine()
    Console.WriteLine($"  共 {KernelCatalog.All().Count} 个内核。")
    Return 0
End Function

Call RunListKernels()