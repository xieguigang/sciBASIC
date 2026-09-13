' =============================================================
' demo: #package / #author / #title / #version 预处理指令
'
'   vbs ./test/test_metadata.vb
'
' 运行之后可以分别通过魔法方法与反射读回这些元数据。
' =============================================================
#package "VBScript.Demo.Metadata"
#author "xieguigang"
#title "VBS Metadata Demo"
#version "1.2.3.4"

Call Console.WriteLine("---------- 魔法方法读回指令 ----------")
Call Console.WriteLine("Package()        = " & Package())
Call Console.WriteLine("Author()         = " & Author())
Call Console.WriteLine("Title()          = " & Title())
Call Console.WriteLine("Version()        = " & Version())
Call Console.WriteLine("Meta(""title"")    = " & Meta("title"))
Call Console.WriteLine("Meta(""unknown"")  = " & If(Meta("unknown") Is Nothing, "<Nothing>", Meta("unknown")))

Call Console.WriteLine()
Call Console.WriteLine("---------- 反射读回 assembly ----------")

Dim asm = Self()

Call Console.WriteLine("AssemblyName     = " & asm.GetName().Name)
Call Console.WriteLine("AssemblyVersion  = " & asm.GetName().Version.ToString())

Dim company = asm.GetCustomAttributes(GetType(System.Reflection.AssemblyCompanyAttribute), False)
Dim titleAttr = asm.GetCustomAttributes(GetType(System.Reflection.AssemblyTitleAttribute), False)

Call Console.WriteLine("AssemblyCompany  = " & CType(company(0), System.Reflection.AssemblyCompanyAttribute).Company)
Call Console.WriteLine("AssemblyTitle    = " & CType(titleAttr(0), System.Reflection.AssemblyTitleAttribute).Title)
