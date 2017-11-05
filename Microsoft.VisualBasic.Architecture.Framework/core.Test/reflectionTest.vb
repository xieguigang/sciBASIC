Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection

Public Module reflectionTest

    Sub Main()
        Dim dll = "G:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\core.Test\bin\Debug\Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a.dll"

        Dim entry = RunDllEntryPoint.GetDllMethod(Assembly.LoadFile(dll), "test")
    End Sub
End Module
