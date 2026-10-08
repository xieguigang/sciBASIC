# LiteJs

轻量级 JavaScript 脚本解析器 —— **词法分析 → 表达式对象（AST）→ 树遍历解释器 + JS→VB.NET 代码翻译器**。纯 VB.NET / .NET 10 / 仅 BCL，零第三方依赖。

## 架构

```
JavaScript 源码
   │ Lexer       token 流（含 ASI 行断跟踪）
   │ Parser      递归下降 → 表达式对象/语句对象（Ast.vb）
   ├──→ Interpreter   树遍历求值（作用域链/闭包/控制流信号）
   └──→ CodeGenerator AST → 可编译 VB.NET 代码
                        （函数→lambda 闭包、for→Do-Loop+update 标签、
                         作用域重命名、VB 关键字转义）
                ↓ dotnet build
          生成的 VB.NET 程序 —— 输出与解释器逐字节一致
```

**单一语义源**：`Runtime.JsRuntime` 同时服务解释器与生成代码——弱类型转换、Truthy、`==`/`===`、字符串拼接规则、数组/字符串方法、内置对象（console/Math/JSON/Object/Array）只有一份实现，保证两条执行路径行为一致。

## 支持的语言元素（图灵完备）

变量声明（var/let/const）· 赋值（含复合 `+= -= *= /= %=` 与解构目标成员/索引）· 后缀/前缀 `++ --` · 全套数学表达式（`+ - * / % **` 右结合幂、一元、三元、`&& || !`、比较、`== === != !==`）· 函数声明/函数表达式/**箭头函数**（含单表达式体）· **闭包**（作用域链，计数器实例互相独立）· 递归 · if/else · for（continue 仍执行 update，break 不执行）· for-in · while · do-while · break/continue · try/catch/finally/throw（控制流信号穿透 try，符合 JS 语义）· 对象/数组字面量 · 方法调用与成员访问 · typeof · 模板字符串（`${}`）· 字符串转义（\n \t \uXXXX…）

## 用法

```
LiteJs.Cli /demo                 # 内置演示：解析→解释→翻译→编译→运行→逐行对比
LiteJs.Cli /run file.js          # 解释执行
LiteJs.Cli /translate file.js [out.vb]   # 翻译为 VB.NET
```

## 生成代码示例

```javascript
function fib(n) { if (n < 2) return n; return fib(n-1) + fib(n-2); }
```

```vb
Dim fib As Func(Of Object(), Object) = Function(args__1 As JsValue())
    Dim n As JsValue = If(args__1.Length > 0, args__1(0), JsRuntime.Undef)
    If JsTruthy(JsRuntime.JsLt(n, 2R)) Then Return n
    Return JsRuntime.JsAdd(JsRuntime.JsInvoke(fib, New JsValue() {JsRuntime.JsSub(n, 1R)}), ...)
End Function
```

## 测试

```
dotnet test    ' 22 项全绿
```

覆盖：Lexer token/ASI/转义/未闭合报错 · Parser 优先级/ASI/错误位置/箭头函数 · 解释器（变量/算术/弱类型 ==、函数/递归/闭包独立性、控制流含 do-while 至少执行一次、try-catch-finally 嵌套、控制流信号不被 catch 吞掉、数组方法/字符串/typeof/拼接）· CodeGenerator 关键字转义/幂运算 e2e · CLI 冒烟。`/demo` 自含端到端验证（编译并 diff 逐行输出）。

## 实现要点（踩坑记录）

- **VB 闭包提升**：同一方法内所有 lambda 的局部变量必须方法级唯一（兄弟作用域也算冲突，BC30616）→ Scope 根节点维护方法级 used-name 集合
- **for 的 continue 语义**：JS `continue` 会执行 for-update；生成的 Do-Loop 把 update 放在标签前，continue 编译为 `GoTo __updN`（break 仍是 Exit Do，不执行 update）
- **循环语句 break/continue 目标**：While/Do/For Each 分别需要 Exit While/Exit Do/Exit For → 循环栈；函数体是循环边界（lambda 生成时保存/清空/恢复栈）
- **VB Imports 别名不能指向泛型构造类型**（`Imports F = Func(Of ...)` 非法）→ 直接写全名
- **三字符字面量**：`""""c`（4 个引号）才是双引号字符字面量
