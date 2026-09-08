## 实现原理

脚本引擎会通过脚本文件中的元数据解析，动态编译生成assembly，具体过程为：引擎会首先从脚本中解析出``#include``元数据，得到引用的程序集。然后通过正则表达式提取出类型定义（class，structure，interface，enum）代码块，处理顶层函数为匿名函数，最后将得到的代码生成下面的固定命名空间以及类型的完整代码：

例如，从输入的脚本代码：

```vbnet
#include "abc.dll"

' vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"

Public Class Test
    Public A As Integer

    Public Function ToString() As String
       ' 从外部dll模块 "abc.dll" 文件中导入
       Return ABC.Internal.Seed(A) 
    End Function
End Class

Call console.writeLine(new Test With {.A = A})

' 顶层函数
Public Function HelloWorld As String
    Return "hello world!"
End Function

If B Then
  Call Console.writeLine(HelloWorld())
End If
```

经过脚本引擎处理后，将会变为下面所示的正式代码，从而能够被正常的编译：

```vbnet
Namespace DynamicDll                                         ' 在脚本引擎中固定的顶层命令空间
    Module Program                                           ' 在脚本引擎中固定的代码容器
       Public Function Main(args As CommandLine) As Integer  ' 在脚本引擎中固定的执行函数
            Dim A As Integer = args("--a")
            Dim B As Boolean = args("--flag")

            Call console.writeLine(new Test With {.A = A})

            Dim HelloWorld = Function() As String            ' 顶层函数会被重构为匿名函数
                                Return "hello world!"
                             End Function

            If B Then
                Call Console.writeLine(HelloWorld())
            End If
       End Function
    End Module

    Public Class Test
        Public A As Integer

        Public Function ToString() As String
            ' imported from "abc.dll"
            Return ABC.Seed(A) 
        End Function
    End Class
End Namespace
```

## 脚本语法

支持的vb.net脚本的语法与正常的vb.net源代码的语法保持一致，除了下面的一些不同：

#### 1. 获取命令行参数

VB.NET脚本可以直接通过下面的语法来获取得到命令行参数：

```vbnet
' vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"
```

本质上是脚本引擎所创建的虚拟函数Main方法中的args变量的引用

#### 2. 引用外部.NET CLR DLL程序集文件

可以使用下面的语法来引用dll文件(采用``#include``预编译命令)：

```vbnet
#include "/path/to/assembly.dll"
```

所导入的外部dll程序集的路径可以为绝对路径，或者相对于脚本文件的相对路径。在脚本引擎中会将这里引用的dll路径统一解析为绝对路径

#### 3. 顶层代码语句

对于正规的vb.net代码而言，代码语句（例如函数，方法，表达式）必须要放入到一个类型容器中，例如必须要放入到Class申明代码块之中。而在当前的这个脚本引擎中，代码语句可以直接出现在最顶层，例如

```vbnet
' 顶层语句
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"

' 顶层函数
Public Function HelloWorld As String
    Return "hello world!"
End Function
```

这些顶层表达式，在解析阶段，编译之前会被解析出来，并放入到一个虚拟的Main方法之中