# vs_solutions/JavaScript/LiteJs.vbproj

- RootNamespace : Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
- AssemblyName  : Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
- TargetFramework: net10.0
- Source files  : 16
- Existing Title: 
- Existing Desc : Lightweight JavaScript parser, tree-walking interpreter and JS-to-VB.NET code generator. Pure VB.NET, .NET BCL only.
- Existing Tags : 

## Namespaces
- Runtime  [files: 7]

## Public types
- Enum TokenType (Language\TokenType.vb)
- Module NamespaceDoc (Runtime\NamespaceDoc.vb) - The single source of JavaScript dynamic semantics used by BOTH the interpreter and the generated VB.NET code: weak typing, truthiness, loose/strict equality, member/index access, invocation and all builtin
- Module JsRuntime (Runtime\Runtime.vb)
- Class LogTextIO (Runtime\ScriptIO.vb)
- Class ConsoleIO (Runtime\ScriptIO.vb)

## Notable public members
- Public Sub New(value As JsValue)
- Public Sub New(name As String)
- Public Sub New(op As String, operand As Expression)
- Public Sub New(op As String, target As Expression, isPrefix As Boolean)
- Public Sub New(op As String, left As Expression, right As Expression)
- Public Sub New(op As String, left As Expression, right As Expression)
- Public Sub New(op As String, target As Expression, value As Expression)
- Public Sub New(test As Expression, consequent As Expression, alternate As Expression)
- Public Sub New(callee As Expression, args As List(Of Expression))
- Public Sub New(obj As Expression, name As Object, computed As Boolean)
- Public Sub New(name As String, parameters As String(), body As List(Of Statement))
- Public Sub New(elements As List(Of Expression))
- Public Sub New(properties As List(Of KeyValuePair(Of String, Expression)))
- Public Sub New(body As List(Of Statement))
- Public Sub New(body As List(Of Statement))
- Public Sub New(name As String, init As Expression)
- Public Sub New(kind As String, declarations As List(Of VarDeclarant))
- Public Sub New(name As String, parameters As String(), body As List(Of Statement))
- Public Sub New(test As Expression, thenBranch As Statement, elseBranch As Statement)
- Public Sub New(init As Object, test As Expression, update As Expression, body As Statement)
- Public Sub New(varName As String, objectExpr As Expression, body As Statement)
- Public Sub New(test As Expression, body As Statement)
- Public Sub New(body As Statement, test As Expression)
- Public Sub New(value As Expression)
- Public Sub New(value As Expression)
- Public Sub New(tryBlock As List(Of Statement), catchParam As String,
- Public Sub New(expr As Expression)
- Public Function Generate(program As Program, Optional className As String = "GeneratedScript") As String
- Friend Function GenExpr(e As Expression, scope As Scope) As String
- Public Sub Indent()
- Public Sub UnIndent()
- Public Sub Line(Optional text As String = "")
- Public Sub WriteLine()
- Public Sub Paste(text As String)
- Public Overrides Function ToString() As String
- Public Function Push() As Scope
- Public Function DefineLocal(name As String) As String
- Public Function Resolve(name As String) As String
- Public Function ResolveOrThrow(name As String) As String
- Public Sub New(parent As Environment)
- Public Function TryGet(name As String, ByRef value As JsValue) As Boolean
- Public Sub Define(name As String, value As JsValue, isConst As Boolean)
- Public Sub Assign(name As String, value As JsValue)
- Public Function LookupOrThrow(name As String) As JsValue
- Public Overrides Function FindSlot(name As String, throwIfNotFound As Boolean) As ScriptSlot
- Public Function TryGet(name As String, ByRef value As Object) As Boolean
- Public Sub Define(name As String, value As Object, isConst As Boolean)
- Public Sub Assign(name As String, value As Object)
- Public Function LookupObjectOrThrow(name As String) As Object
- Protected Overrides Sub CheckReadOnly(slot As ScriptSlot, name As String)
- Public Sub New(Optional io As ScriptIO = Nothing)
- Public Sub Run(program As Program)
- Public Sub DefineGlobal(name As String, value As Object)
- Public Sub New(value As JsValue)
- Public Shared Function Lex(source As String) As List(Of Token)
- Public Sub New(message As String)
- Public Shared Function Parse(source As String) As Program
- Public ReadOnly Property Value As Object
- Public ReadOnly Property LineBreakBefore As Boolean
- Public ReadOnly Property Location As String
- ... and 69 more

## Imports
- Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
- Microsoft.VisualBasic.MIME.application.json
- Microsoft.VisualBasic.MIME.application.json.Javascript
- Microsoft.VisualBasic.Scripting.Runtime
- Microsoft.VisualBasic.Scripting.TokenIcer
- System.Globalization
- System.Text
- System.Text.RegularExpressions

## File tree
- AST\Ast.vb
- CodeGenerator.vb
- Environment.vb
- Interpreter.vb
- Language\Lexer.vb
- Language\ParseException.vb
- Language\Parser.vb
- Language\Token.vb
- Language\TokenType.vb
- Runtime\BuiltinMethod.vb
- Runtime\JsRuntimeException.vb
- Runtime\NamespaceDoc.vb
- Runtime\NativeObject.vb
- Runtime\Runtime.vb
- Runtime\ScriptIO.vb
- Runtime\UndefinedMarker.vb

