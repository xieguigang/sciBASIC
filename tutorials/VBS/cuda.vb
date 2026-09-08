#include "Microsoft.VisualBasic.Computing.ILCuda.dll"

imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Public Function RunListKernels() As Integer
    Console.WriteLine("内核注册表（框架内置内核）")
    Console.WriteLine()

    For Each info As KernelInfo In KernelCatalog.All()
        Console.WriteLine($"  {info}")
    Next

    Console.WriteLine()
    Console.WriteLine($"  共 {KernelCatalog.All().Count} 个内核。")
    Return 0
End Function

Call RunListKernels()