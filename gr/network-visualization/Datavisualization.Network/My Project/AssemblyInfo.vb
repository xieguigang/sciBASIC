Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' 有关程序集的常规信息通过下列特性集
' 控制。更改这些特性值可修改
' 与程序集关联的信息。

' 查看程序集特性的值
#If netcore5 = 0 Then
<Assembly: AssemblyTitle("This module provides some important network operation algorithms")> 
<Assembly: AssemblyDescription("This module provides some important network operation algorithms")>
<Assembly: AssemblyCompany("GPL3")>
<Assembly: AssemblyProduct("network-visualization")>
<Assembly: AssemblyCopyright("Copyright © xieguigang 2014")>
<Assembly: AssemblyTrademark("sciBASIC")>

<Assembly: ComVisible(False)>

'如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
<Assembly: Guid("0e1114d5-5581-4a84-901c-bb45bce58195")> 

' 程序集的版本信息由下面四个值组成: 
'
'      主版本
'      次版本
'      生成号
'      修订号
'
' 可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值，
' 方法是按如下所示使用“*”: 
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("2.0.3.546")> 
<Assembly: AssemblyFileVersion("1.4.0.0")> 
#end if