#Region "Microsoft.VisualBasic::e0b785981827b97a6f472aaaf87e656a, sciBASIC#\Data\BinaryData\DataStorage\My Project\AssemblyInfo.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 36
    '    Code Lines: 15
    ' Comment Lines: 15
    '   Blank Lines: 6
    '     File Size: 1.18 KB


    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' 有关程序集的一般信息由以下
' 控制。更改这些特性值可修改
' 与程序集关联的信息。

'查看程序集特性的值
#If netcore5 = 0 Then
<Assembly: AssemblyTitle("Binary data storage provider base on netCDF/HDF5")>
<Assembly: AssemblyDescription("Binary data storage provider base on netCDF/HDF5")>
<Assembly: AssemblyCompany("sciBASIC.NET")>
<Assembly: AssemblyProduct("DataStorage")>
<Assembly: AssemblyCopyright("Copyright © sciBASIC.NET 2019")>
<Assembly: AssemblyTrademark("")>

<Assembly: ComVisible(False)>

'如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
<Assembly: Guid("d71c3ff7-d1e8-43d5-9c26-7732ec1bade7")>

' 程序集的版本信息由下列四个值组成: 
'
'      主版本
'      次版本
'      生成号
'      修订号
'
' 可以指定所有值，也可以使用以下所示的 "*" 预置版本号和修订号
' 方法是按如下所示使用“*”: :
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.50.*")>
<Assembly: AssemblyFileVersion("2.1.*")>
#end if
