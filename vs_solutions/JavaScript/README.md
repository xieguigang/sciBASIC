# LiteJs

轻量级 JavaScript 脚本引擎 —— **词法分析 → 表达式对象（AST）→ 树遍历解释器 + JS→VB.NET 代码翻译器**。纯 VB.NET / .NET 10 / 仅 BCL，零第三方依赖。

## 架构

```
JavaScript 源码
   │ Lexer       token 流（含 ASI 行断跟踪、模板字符串、正则字面量歧义消解）
   │ Parser      递归下降 → 表达式对象/语句对象（Ast.vb）
   ├──→ Interpreter   树遍历求值（作用域链/闭包/原型链/控制流信号/微任务队列）
   └──→ CodeGenerator AST → 可编译 VB.NET 代码
                        （函数→lambda 闭包、for→Do-Loop+update 标签、
                         switch→If+GoTo 链保贯穿、class→原型字典构造、
                         作用域重命名、VB 关键字转义）
                ↓ dotnet build
          生成的 VB.NET 程序 —— 输出与解释器逐字节一致
```

**单一语义源**：`Runtime.JsRuntime` 同时服务解释器与生成代码——弱类型转换、Truthy、`==`/`===`、字符串拼接规则、数组/字符串方法、内置对象（console/Math/JSON/Object/Array/Date/RegExp/Error/Promise）只有一份实现，保证两条执行路径行为一致。

**宿主互操作**：`Runtime/ClrInterop.vb`、`ClrObject.vb`、`ClrMemberCache.vb`、`ClrConvert.vb`、`ClrAttributes.vb` 把 .NET 对象/类型以反射方式桥接成脚本对象，两条执行路径共用同一套成员解析与类型转换。

## 支持的语言元素

**声明与赋值**：`var`（函数级提升）/ `let` / `const`（块级、每次迭代新绑定）· 赋值（含 `+= -= *= /= %=`）· 解构赋值 `[a, b] = x` / `({a, b} = o)` · 后缀/前缀 `++ --`

**表达式**：全套算术（`+ - * / % **` 右结合）· 位运算 `& | ^ ~ << >> >>>`（ToInt32 语义）· 比较/相等 · `&& || ??`（空值合并，短路）· `!`/`typeof`/`void`/`delete` · 三元 · 逗号运算符 · `in` · `instanceof` · 可选链 `a?.b` / `a?.[i]` / `a?.()`（整链短路）· 展开 `...`（调用/数组/对象）· 模板字符串（`` `a${x}b` ``，支持嵌套与转义）· 正则字面量 `/re/flags`

**语句**：`if/else` · `for`（含 `let` 每迭代绑定）· `for-in` · `for-of` · `while` · `do-while` · `switch/case/default`（严格相等 + 贯穿语义）· 标签语句与标签 `break`/`continue` · `break`/`continue`/`return`/`throw` · `try/catch/finally` · 空语句

**函数**：函数声明/表达式 · 箭头函数（词法 `this`/`arguments`）· 默认参数 `f(a, b = 1)` · 剩余参数 `f(...rest)` · 闭包 · 递归 · `arguments` 对象 · `async`/`await`（含异步箭头与异步方法）· 生成器 `function*` / `yield`

**面向对象**：`class` 声明与表达式 · `constructor` · 实例方法/静态方法/静态字段 · 实例字段（含 `this` 引用）· getter/setter · 计算键 `[k]` · `extends` / `super`（`super.m()` 与 `super(...)`）· `new` · `this` 动态绑定 · `instanceof` · 原型链（`__proto__` 字典链）

**内置对象**：`console`（log/info/warn/error）· `Math`（含三角函数、`log2/log10/cbrt/imul/clz32/fround` 等）· `JSON`（`parse` / `stringify`，第三参数缩进）· `Object`（keys/values/entries/assign/fromEntries/getOwnPropertyNames/create/freeze）· `Array`（push/pop/shift/unshift/join/concat/slice/splice/indexOf/lastIndexOf/includes/reverse/sort/map/filter/reduce/reduceRight/forEach/find/findIndex/findLast/some/every/flat/flatMap/fill/at/keys/values/entries）· `String`（charAt/charCodeAt/at/substring/slice/indexOf/lastIndexOf/toUpperCase/toLowerCase/trim/trimStart/trimEnd/padStart/padEnd/split/replace/replaceAll/includes/startsWith/endsWith/repeat/concat/codePointAt/match/matchAll/search/`String.fromCharCode`）· `Number`（isInteger/isFinite/isNaN/isSafeInteger/parseInt/parseFloat/`toString(radix)`/toFixed/常量）· `Date` · `RegExp`（`test`/`exec`/`lastIndex`，与字符串的 `match`/`replace`/`split`/`search`/`matchAll` 互操作）· `Error` 层级（Error/TypeError/RangeError/SyntaxError/ReferenceError，`name`/`message`）· `Promise`（`then`/`catch`/`finally`、`resolve`/`reject`/`all`/`allSettled`/`race`/`any`）

**作用域语义**：`var` 函数级提升、`let/const` 块级、`const` 重赋值抛 TypeError、非严格模式隐式全局（`x++` 未声明变量）、`typeof null === "object"`、箭头函数词法 `this`/`arguments`

## 用法

```vb
' 解释执行
Dim io As ScriptIO = New LogTextIO
Dim engine As New Interpreter(io)
engine.Run(Parser.Parse(scriptText))

' 翻译为可编译的 VB.NET
Dim vbCode = New CodeGenerator().Generate(Parser.Parse(scriptText), "GeneratedScript")
```

`Interpreter.DefineGlobal(name, value)` 可注册宿主委托（`Func(Of Object(), Object)`)作为脚本可见的全局函数；传入任意 CLR 对象或 `Type` 时，则通过下面的反射桥接暴露给脚本。

## 宿主互操作：CLR 反射桥接

把一个 .NET 对象（或类型）交给脚本，脚本即可像操作普通 JS 对象一样读写它的成员——成员由反射解析，无需接口、包装类或胶水代码。

```vb
Dim io As ScriptIO = New LogTextIO
Dim engine As New Interpreter(io)

' 1) 注入单个对象（或 Type）：属性/字段读写 + 方法调用
engine.DefineGlobal("host", myObject)
engine.DefineGlobal("Console", GetType(Console))     ' 静态成员 + new Console()

' 2) 批量注入并同时发布到宿主注册表，翻译产物也能取到同一批对象
Dim host As New ClrHost()
host.Add("host", myObject)
host.Add("json", GetType(JSON))
engine.InstallHost(host)
```

```javascript
host.Title                      // 读属性
host.Title = 'x'                // 写属性（只读成员写入抛 TypeError）
host.Counter++                  // 公开字段与属性一视同仁
host.Add(1, 2)                  // 重载按实参数量/类型择优，支持 params 与可选参数
host.items[0]; host.items.length; for (const v of host.items) {}   // 索引器 / IEnumerable
'Title' in host; Object.keys(host); var copy = { ...host };        // 成员枚举与展开
JSON.stringify(host)            // 序列化可见成员（方法被跳过）
Console.WriteLine('x')          // 静态成员
var c = new MyType(); c instanceof MyType                          // new / instanceof
try { host.Explode(); } catch (e) { e.name }                       // .NET 异常 → 可捕获的 JS 错误
host.Fetch().then(v => ...)     // Task(Of T) → Promise（await 同样可用）
```

**成员可见性**：默认暴露全部公开成员（含继承成员）。`<JsName("x")>` 起别名（成员名精确匹配，不做驼峰隐式映射），`<JsIgnore>` 隐藏成员；某类型上只要出现一个 `<JsExpose>` 成员，该类型即切换为白名单模式（仅标注成员可见），也可用 `ClrInterop.ExplicitOnly = True` 对所有类型强制白名单。

**类型转换**：数值 ↔ `Double/Int32/Int64/Single/Decimal`/枚举（按名或数值赋值）/`Nullable`；字符串 ↔ `String/Char/Guid/Date`；布尔 ↔ `Boolean`；`null`/`undefined` ↔ `Nothing`（值类型参数除外）。返回值中的嵌套对象保持为代理（可继续点号访问），`DateTime` → `JsDate`，`Task`/`Task(Of T)` → `Promise`。

**错误边界**：桥接调用抛出的 .NET 异常被规范化为带 `name`（.NET 异常类型名）与 `message` 的 JS 错误对象，脚本可直接 `catch`；`e.clrException` 取原始异常实例，宿主侧用 `ClrInterop.ToHostException(ex)` 还原为原始 .NET 异常。

**翻译路径一致**：生成代码的入口为 `Run(io As ScriptIO, Optional host As ClrHost = Nothing)`，接受同一批宿主对象；脚本中的未知全局名解析到宿主注册表（`ClrInterop.Register`），未注册的名字抛普通 `ReferenceError`。

## 生成代码示例

```javascript
class Square extends Shape {
    constructor(side) { super(side * side); this.side = side; }
    describe() { return 'square(' + this.side + ') -> ' + super.describe(); }
}
var sq = new Square(3);
console.log(sq.describe(), sq instanceof Shape);
```

```vb
Dim Square As JsValue = Nothing
Square = JsRuntime.JsDefineClass("Square", CType(Function(args__9 As JsValue())
                                                     Dim side As JsValue = ...
                                                     JsRuntime.JsSuperConstructor(New JsValue() {JsRuntime.JsMul(side, side)})
                                                     JsRuntime.JsSet(this, "side", side)
                                                 End Function, Func(Of Object(), Object)),
                                 Shape,
                                 New (String, String, Object)() {("i", "describe", CType(Function(args__10 As JsValue())
        Return JsRuntime.JsAdd(JsRuntime.JsAdd("square(", JsRuntime.JsStr(JsRuntime.JsGet(this, "side"))), ...)
    End Function, Func(Of Object(), Object)))})
```

## 语义取舍（与浏览器 JavaScript 的差异）

- **无事件循环**：Promise 同步结算，微任务队列在 `await` 处与脚本结束时排空（`Promise.resolve(1).then(f); g()` 仍是 `g` 先于 `f`，与 JS 一致）；`setTimeout` 等宿主定时器不存在。
- **生成器为 eager 预计算**：`function*` 函数体在首次调用时一次性执行完并收集全部 `yield` 值，因此 `for...of` / 展开 / `next()` 的消费行为与 JS 一致，但**无限生成器会不返回**；`yield` 表达式的值即其参数（不支持 `next(v)` 回传）。
- **正则**：由 .NET `Regex` 承载；`g`/`i`/`m`/`s` 支持，`y`（sticky）近似实现，`u` 等价于 .NET 的默认 unicode 行为。
- **Promise.any 拒绝值**：返回拒绝原因数组（无 AggregateError 类型）。
- **类的静态字段**：在类定义时求值；`class A { static x = A }` 这类自引用会拿到未绑定的类名（罕用）。
- **CLR 桥接的取舍**：
  - **脚本函数不转 .NET 委托**：需要委托/回调（如 LINQ `Where`）的 .NET 方法无法从脚本调用，命中委托形参时报可读错误（可从宿主传入委托实例代替）。
  - **值类型（struct）写入不回写**：`host.point.X = 1` 只作用于桥接持有的装箱副本，不改变来源属性；引用类型对象即本体，写入对宿主可见。
  - **Task 需能同步完成**：`Task`/`Task(Of T)` 在 `ClrInterop.TaskWaitTimeoutMs`（默认 5000ms）内未结算则报错——引擎没有事件循环，无法等待。
  - **成员名精确匹配**：不做 camelCase↔PascalCase 隐式映射，别名请用 `<JsName>`。
  - **索引器限单参数**：`obj[i]` 映射到单参数索引器；`Item(x, y)` 这类多参数索引器不可用。
  - **集合不是 JS 数组**：集合代理支持下标、`length`、for-of 与展开，`Object.keys` 给出索引，但没有 `map`/`filter`/`push` 等数组方法（需先 `[...host.items]` 转换为数组）。
- **无模块系统**：`import` / `export` 未实现（脚本按单文件执行）。

## 测试

```
dotnet test        ' 115 项全绿
```

覆盖：Lexer（token/ASI/转义/模板/正则歧义/数字分隔符）· Parser（优先级/ASI/错误位置/箭头/标签/解构）· 解释器（变量/算术/弱类型/闭包/控制流/异常/数组方法/对象/class/继承/super/正则/Date/Error/Promise/async-await/生成器）· 语义修正（`typeof null`、`var` 提升、`!=` 翻译）· **CLR 反射桥接**（`LiteJsClrInteropTests` + `ClrHostFixture`：属性/字段读写、只读与只写报错、重载与 params、索引器、集合迭代/展开/`length`、`Object.keys`/`in`/`delete`/`JSON.stringify`/展开拷贝、静态成员、`new`/`instanceof`、枚举/日期/Guid/struct 转换、Task→Promise、异常捕获与还原、`[JsName]`/`[JsIgnore]`/`[JsExpose]`、宿主注册表与批量注入、生成代码结构断言）· CodeGenerator 结构断言 · **产物编译期校验**：`test/artifacts/GeneratedScript.vb` 由 `test/artifacts/demo.js` 生成并编译进测试程序集（`ArtifactSyncTests` 断言其与生成器输出逐字节一致；改动生成器后用 `LITEJS_REGEN_ARTIFACTS=1` 重新生成）。

## 实现要点（踩坑记录）

- **VB 闭包提升**：同一方法内所有 lambda 的局部变量必须方法级唯一（兄弟作用域也算冲突，BC30616）→ Scope 根节点维护方法级 used-name 集合
- **for 的 continue 语义**：JS `continue` 会执行 for-update；生成的 Do-Loop 把 update 放在标签前，continue 编译为 `GoTo __updN`（break 仍是 Exit Do）
- **标签 break/continue**：跨嵌套循环必须用 `GoTo` 标签，VB 的 `Exit While`/`Continue For` 只作用于最内层循环
- **switch**：VB `Select Case` 无贯穿且用松散比较 → 生成 `If JsStrictEq(...) Then GoTo __caseN` 分发链 + 顺序贯穿
- **var 提升**：VB `Dim` 是块级，JS `var` 是函数级 → 函数体顶部统一 `Dim x As JsValue = JsRuntime.Undef`，声明处只赋值
- **可选链**：链根用 IIFE `Try/Catch OptionalChainShortCircuit` 短路到 `undefined`，与解释器的内部信号一致
- **`this`**：运行时的调用上下文栈（保存/恢复）承载，生成代码在函数入口读 `JsRuntime.CurrentThis`；箭头函数不建立自有绑定
- **VB 保留字陷阱**：`Global`/`Step`/`Exit`/`Single`/`Path`(大小写不敏感) 不能作标识符；`Path` 与局部变量 `path` 冲突；`Microsoft.VisualBasic.Environment` 遮蔽 `System.Environment`
- **生成代码的命名空间导入**：真实命名空间是 `Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime`，需**同时**普通导入（裸 `JsRuntime.X`）与别名导入（`LiteJsRuntime.X`）——别名导入不会把成员引入作用域
- **VB Imports 别名不能指向泛型构造类型**（`Imports F = Func(Of ...)` 非法）→ 直接写全名
- **三字符字面量**：`""""c`（4 个引号）才是双引号字符字面量
