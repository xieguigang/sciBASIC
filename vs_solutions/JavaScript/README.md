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

**Node.js 兼容模块**（模拟 `require('fs'|'path'|'Buffer'|'process')` 的全局 `fs` / `path` / `Buffer` / `process`）：`fs` 提供同步（`readFileSync`/`writeFileSync`/`existsSync`/`readdirSync`/`mkdirSync`/`rmdirSync`/`statSync`/`unlinkSync`/`appendFileSync`/`copyFileSync`/`renameSync`）、回调异步（`readFile(path, cb)` 等，以微任务延迟结算，行为等同 Node）与 Promise（`fs.promises.*` 可 `await`）三种形态，并暴露 `fs.constants`；`Buffer` 提供 `from`/`alloc`/`allocUnsafe`/`concat`/`slice`/`subarray`/`toString(encoding)`/`equals`；`path` 提供 `join`/`resolve`/`basename`/`dirname`/`extname`/`normalize`/`parse`/`isAbsolute` 及属性 `sep`/`delimiter`；`process` 提供 `cwd()`/`platform`/`arch`/`argv`/`env`，以及由宿主注入的 `__dirname`/`__filename`。相对路径相对 `process.cwd()` 解析，错误映射为带 `code`（`ENOENT`/`EACCES`/…）/`syscall`/`path` 的 JS `Error`。两条执行路径均通过 `JsRuntime` 单一语义源提供上述能力（生成产物的 `Run` 入口已调用 `GlobalIdentifiers()` 完成模块初始化）。

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

## import：CLR 程序集与 JS 模块

`import` 只允许出现在脚本顶层，并在主体执行前完成绑定（解释执行与翻译产物行为一致）。`from` 的取值决定语义：**以 `.dll` 结尾 ⇒ 反射打开 .NET 程序集并导入命名空间**，否则按相对路径加载 `.js` 模块。

```javascript
// 1) CLR 命名空间：程序集内该命名空间的 public 直属类型 → 别名下的类型对象
import Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL as msil from 'Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.dll';
var syntax = msil.MethodDecompiler.Decompile(method);   // 静态成员（VB Module 同理）
typeof msil.MethodDecompiler                            // "function"
Object.keys(msil)                                       // 类型名列表

// 省略 as：别名取命名空间最后一段
import Foo.Bar from 'x.dll';                            // 绑定 Bar

// 2) JS 模块（相对导入方脚本所在目录；可省略 .js，也会尝试 index.js）
import * as utils from './utils.js';                    // 全部顶层声明
import { add as plus, helper } from './math.js';        // 按名取（支持 as 改名）
import u from './utils.js';                             // 无 export：等同命名空间对象
import './shim.js';                                     // 仅执行（副作用），重复导入只执行一次
```

**CLR 解析顺序**：已加载程序集 → 显式 dll 路径（绝对路径或相对脚本目录）→ `Assembly.Load(简单名)` → 宿主探测目录（`ClrInterop.AddAssemblySearchPath`）→ 宿主回调（`ClrInterop.AssemblyResolver`）→ 抛脚本可捕获错误（消息列出尝试过的位置）。命名空间映射是**扁平**的：不含子命名空间、不含嵌套类型、跳过开放泛型定义，结果按（程序集, 命名空间）缓存。

**模块语义**：按绝对路径缓存（同一模块只执行一次）、循环导入明确报错、模块体在独立函数作用域中执行；`var`/函数/类声明属于全局作用域（相当于经典 script include），`let`/`const` 与函数局部变量对外不可见。**因为未实现 `export`**，模块的"导出面"就是它顶层的 `var/let/const/function/class` 声明。

**宿主侧**：`JsRuntime.ScriptBaseDirectory` 指定相对路径的基准目录（默认取 `process` 的脚本目录，其次应用目录）；`JsRuntime.ClearImportCaches()` 清空程序集/命名空间/模块缓存。

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

## TypeScript 定义文件生成（.d.ts）

为编译好的 CLR 程序集生成 TypeScript 声明文件，供 VS Code 等 IDE 在编写 JavaScript 时给出 .NET 类型智能提示。生成器复用与脚本桥接**同一套**成员解析与类型转换规则（`ClrMembers` / `ClrConvert`），因此 IDE 提示与脚本实际行为不会互相矛盾。

```bash
njs --dts <assembly.dll> [-o <out.d.ts>] [--xmldoc <path>] \
    [--no-globals] [--explicit-only] [--include-nonpublic]
```

- `--xmldoc` 省略时自动探测 dll 同目录的同名 `.xml`（即编译器产出的 XML 注释文档），用 `ProjectSpace.CreateDocProject` 解析并生成 JSDoc；文档缺失则无注释、不报错。
- 默认输出当前目录下的 `<程序集名>.d.ts`（UTF-8 无 BOM，确定性排序、无时间戳，可 diff、可入库）。
- 库 API：`TypeDefinitionGenerator.Generate(assemblyPath, options)` / `.Write(assemblyPath, outputPath, options)`（`Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.TypeDefinitions`）。

**输出形态**：每个 CLR 命名空间一个 `declare namespace`，嵌套类型通过同名 `namespace` 块做声明合并；文件末尾为每个公开类型追加全局别名——

```ts
declare namespace MyLib.Core {
    /** 从 XML 注释文档生成的 JSDoc。 */
    class Widget {
        Name: string;
        Compute(factor: number): number;
        static Create(): Widget;
    }
}
type Widget = MyLib.Core.Widget;
declare const Widget: typeof MyLib.Core.Widget;
```

于是脚本里 `new Widget()` / `Widget.Create()` 直接有提示（对应宿主按类型名注册 host global 的用法）。interface / delegate 只生成 `type` 别名（无可构造值）；简单名重复时首个保留、其余输出注释并保留限定名；名字若与 TypeScript 默认库的全局标识符（`Element` / `File` / `URL` / …）冲突则跳过别名，避免遮蔽库标识符。`--no-globals` 关闭整个别名段。

**类型映射**（逐项对齐 `ClrConvert` 的转换语义）：

| CLR | TypeScript |
|---|---|
| 全部数值类型、枚举 | `number`（枚举在桥接上就是数值；枚举声明本身仍生成，可写 `TsState.Running`） |
| String / Char / Guid | `string` |
| Boolean | `boolean` |
| Date / DateTimeOffset | `Date` |
| Task / Task(Of T) | `Promise<void>` / `Promise<T>` |
| Nullable(Of T) | `T \| null` |
| `T[]`、`List(Of T)`、IList / ICollection / IReadOnlyList / IReadOnlyCollection | `T[]` |
| `IEnumerable(Of T)` | `Iterable<T>`（并为此类补 `[Symbol.iterator]`） |
| 本程序集类型 | 声明名（含递归渲染的泛型实参） |
| 委托 | 函数类型 `type X = (a: number) => string;` |
| 其余 BCL 类型 | `any` |

**与桥接一致的取舍**：`<JsName>` 重命名、`<JsIgnore>` 隐藏、`<JsExpose>` 白名单（`--explicit-only` 全局强制）逐字生效；事件不生成（桥接不暴露事件）；单参数索引器生成索引签名；只读属性生成 `readonly`、只写属性生成 `set`；`params` 生成剩余参数，可选参数生成 `name?:` 且 JSDoc 带默认值；VB Module 生成全静态类，`MustInherit` 生成 `protected constructor()`、无公共构造函数生成 `private constructor()`；`[Obsolete]` 生成 `@deprecated`。

**已知限制**：委托参数按函数类型生成，但脚本无法把 JS 函数转成 .NET 委托（运行时报可读错误）；多维数组、`out` / `ref`、BCL 未知类型退化为 `any`；静态成员不能引用类的类型参数（TypeScript 规则），退化为 `any`；名为 `constructor` 的成员被跳过（TypeScript 禁止）；`Iterable` / `Symbol.iterator` 需要 ES2015+ 的 lib 目标。

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
- **import 的取舍**：
  - **`import` 是保留字**（此前可用作标识符），`export` 未实现；因此模块导出面 = 顶层声明，`import x from './m.js'` 的默认位绑定为「模块对象」而非 `default`。
  - **`from '...'` 以 `.dll` 结尾才按程序集解释**：路径式 dll 走 `Assembly.LoadFrom`，具备加载任意本机程序集的能力，只应在可信脚本中使用。
  - **命名空间扁平**：没有子命名空间树，也不能通过别名访问嵌套类型；需要子命名空间时再写一条 import。
  - **模块无 live binding**：`import {a}` 拿到的是模块执行完成后的值；循环导入直接报错，而非部分初始化。
  - **CLR 导入首次扫描一次**：首次导入某命名空间会遍历其导出类型（O(T)），之后均为字典查找。

## 测试

```
dotnet test        ' 167 项（149 项既有 + 18 项 .d.ts 生成器）
```

覆盖：Lexer（token/ASI/转义/模板/正则歧义/数字分隔符）· Parser（优先级/ASI/错误位置/箭头/标签/解构）· 解释器（变量/算术/弱类型/闭包/控制流/异常/数组方法/对象/class/继承/super/正则/Date/Error/Promise/async-await/生成器）· 语义修正（`typeof null`、`var` 提升、`!=` 翻译）· **import**（`LiteJsImportTests`：六种声明形态与顶层限制/错误分支、CLR 命名空间导入与静态调用 `IL.MethodDecompiler.Decompile`、扁平命名空间、显式 dll 路径、宿主回调与探测目录 API、模块命名空间/按名/默认位/副作用/缓存/模块内相对路径/循环导入/缺失模块与缺失绑定的可捕获错误、生成代码结构断言）· **CLR 反射桥接**（`LiteJsClrInteropTests` + `ClrHostFixture`：属性/字段读写、只读与只写报错、重载与 params、索引器、集合迭代/展开/`length`、`Object.keys`/`in`/`delete`/`JSON.stringify`/展开拷贝、静态成员、`new`/`instanceof`、枚举/日期/Guid/struct 转换、Task→Promise、异常捕获与还原、`[JsName]`/`[JsIgnore]`/`[JsExpose]`、宿主注册表与批量注入、生成代码结构断言）· CodeGenerator 结构断言 · **产物编译期校验**：`test/artifacts/GeneratedScript.vb` 由 `test/artifacts/demo.js` 生成并编译进测试程序集（`ArtifactSyncTests` 断言其与生成器输出逐字节一致；改动生成器后用 `LITEJS_REGEN_ARTIFACTS=1` 重新生成）· **TypeScript 定义文件生成**（`TypeDefinitionTests` + `TsFixture` 夹具：文件头与命名空间/嵌套合并、类型映射表逐项、重载与 params/可选参数、可见性属性与白名单、XML 注释 → JSDoc（编译器产物与手写文档两种）、全局别名与重名/库名冲突去重、静态成员与泛型替换、真实程序集端到端）· **Node.js 文件系统 API 对齐**：`test/js/*.js` 为独立脚本，先用真实 `node` 生成 `*.baseline.txt` 基准，再以 `LiteJs.Runner` 运行并逐条对齐（含 `fs` 同步/回调/Promise、`Buffer`、`path`、`process` 与错误路径）；`NodeApiCodegenTests` 断言生成代码引用同一批 `JsRuntime.*Builtin` 且解释器可对真实文件执行读写。

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
