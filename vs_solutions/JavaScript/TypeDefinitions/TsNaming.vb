#Region "Microsoft.VisualBasic::9d1f4a82c6b05e3f70a2d9c8b41e5f63, vs_solutions\JavaScript\TypeDefinitions\TsNaming.vb"

    ' Author:
    ' 
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

    '     Module TsNaming
    ' 
    '         Function: DeclarationPath, Sanitize, StripArity

    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace TypeDefinitions

    ''' <summary>
    ''' Identifier and name helpers shared by the scanner, the type mapper and the emitter.
    ''' </summary>
    Friend Module TsNaming

        ''' <summary>
        ''' Words that are illegal as a bare identifier in a TypeScript declaration slot
        ''' (type parameter names, type names). Only these slots are sanitised — member
        ''' names are emitted verbatim because the bridge matches them exactly.
        ''' </summary>
        Private ReadOnly reserved As New HashSet(Of String)(StringComparer.Ordinal) From {
            "abstract", "any", "as", "async", "await", "bigint", "boolean", "break", "case",
            "catch", "class", "const", "constructor", "continue", "debugger", "declare",
            "default", "delete", "do", "else", "enum", "export", "extends", "false", "finally",
            "for", "from", "function", "get", "if", "implements", "import", "in", "infer",
            "instanceof", "interface", "is", "keyof", "let", "module", "namespace", "never",
            "new", "null", "number", "object", "of", "private", "protected", "public",
            "readonly", "return", "set", "static", "string", "super", "switch", "symbol",
            "this", "throw", "true", "try", "type", "typeof", "undefined", "unique", "unknown",
            "var", "void", "while", "with", "yield"
        }

        ''' <summary>
        ''' Identifiers the TypeScript default libraries (lib.es5 / lib.es2015+ /
        ''' lib.dom / lib.scripthost / lib.decorators) declare as globals. A global alias
        ''' carrying one of these names would shadow the library identifier and break
        ''' every project that references this file (TS6200), so such aliases are
        ''' skipped — the qualified namespace form stays available. Conservative
        ''' snapshot: a name missing here only means the alias is emitted, which is
        ''' harmless for names the libraries do not declare.
        ''' </summary>
        Friend ReadOnly TsLibGlobals As New HashSet(Of String)(StringComparer.Ordinal) From {
            "Object", "Function", "Boolean", "Symbol", "Error", "EvalError", "RangeError",
            "ReferenceError", "SyntaxError", "TypeError", "URIError", "AggregateError",
            "Number", "Math", "Date", "String", "RegExp", "Array", "ArrayBuffer",
            "SharedArrayBuffer", "DataView", "Atomics", "Int8Array", "Uint8Array",
            "Uint8ClampedArray", "Int16Array", "Uint16Array", "Int32Array", "Uint32Array",
            "Float32Array", "Float64Array", "BigInt64Array", "BigUint64Array", "JSON",
            "Promise", "Map", "Set", "WeakMap", "WeakSet", "WeakRef", "FinalizationRegistry",
            "Proxy", "Reflect", "Iterable", "Iterator", "IterableIterator", "Generator",
            "AsyncGenerator", "AsyncIterable", "AsyncIterableIterator", "IteratorResult",
            "ArrayLike", "ReadonlyArray", "Partial", "Required", "Readonly", "Pick",
            "Record", "Exclude", "Extract", "NonNullable", "ReturnType", "InstanceType",
            "Omit", "Awaited", "ThisType", "Uppercase", "Lowercase", "Capitalize",
            "Uncapitalize", "globalThis", "undefined", "NaN", "Infinity", "BigInt",
            "console", "fetch", "alert", "confirm", "prompt", "setTimeout", "clearTimeout",
            "setInterval", "clearInterval", "queueMicrotask", "structuredClone", "atob",
            "btoa", "crypto", "performance", "requestAnimationFrame", "cancelAnimationFrame",
            "requestIdleCallback", "cancelIdleCallback", "matchMedia", "getComputedStyle",
            "reportError", "open", "close", "focus", "blur", "print", "postMessage",
            "addEventListener", "removeEventListener", "dispatchEvent", "scroll", "scrollTo",
            "scrollBy", "CustomEvent", "Event", "EventTarget", "UIEvent", "MouseEvent",
            "KeyboardEvent", "FocusEvent", "WheelEvent", "InputEvent", "CompositionEvent",
            "DragEvent", "PointerEvent", "TouchEvent", "AnimationEvent", "TransitionEvent",
            "MessageEvent", "MessageChannel", "MessagePort", "ErrorEvent",
            "PromiseRejectionEvent", "ProgressEvent", "StorageEvent", "SubmitEvent",
            "BeforeUnloadEvent", "HashChangeEvent", "PopStateEvent", "PageTransitionEvent",
            "Blob", "File", "FileList", "FileReader", "FormData", "Headers", "Request",
            "Response", "URL", "URLSearchParams", "WebSocket", "Worker", "AbortController",
            "AbortSignal", "Image", "ImageData", "ImageBitmap", "CanvasRenderingContext2D",
            "OffscreenCanvas", "CanvasGradient", "CanvasPattern", "Path2D", "TextMetrics",
            "FontFace", "FontFaceSet", "Range", "DoubleRange", "Selection", "NodeFilter",
            "NodeIterator", "TreeWalker", "Node", "NodeList", "HTMLCollection",
            "HTMLOptionsCollection", "Element", "Text", "Document", "DocumentFragment",
            "DocumentType", "Attr", "CharacterData", "Comment", "ProcessingInstruction",
            "CDATASection", "ShadowRoot", "XMLDocument", "DOMParser", "XMLSerializer",
            "MutationObserver", "MutationRecord", "IntersectionObserver",
            "ResizeObserver", "PerformanceObserver", "TextEncoder", "TextDecoder",
            "TextEncoderStream", "TextDecoderStream", "Storage", "localStorage",
            "sessionStorage", "History", "Location", "Navigator", "Screen", "Window",
            "XMLHttpRequest", "XMLHttpRequestEventTarget", "XMLHttpRequestUpload",
            "ActiveXObject", "Enumerator", "VBArray", "ITextWriter", "FillMode",
            "WritableStream", "ReadableStream", "TransformStream", "CompressionStream",
            "DecompressionStream", "ByteLengthQueuingStrategy", "CountQueuingStrategy",
            "ReadableStreamDefaultReader", "ReadableStreamDefaultController",
            "WritableStreamDefaultWriter", "WritableStreamDefaultController",
            "TransformStreamDefaultController", "SubtleCrypto", "CryptoKey", "CryptoKeyPair"
        }

        ''' <summary>Strip the CLR generic arity suffix (``Foo`1`` → ``Foo``).</summary>
        Friend Function StripArity(name As String) As String
            If name Is Nothing Then Return ""
            Dim tick As Integer = name.IndexOf("`"c)
            If tick > 0 Then Return name.Substring(0, tick)
            Return name
        End Function

        ''' <summary>Make a name safe for a TypeScript declaration slot.</summary>
        Friend Function Sanitize(name As String) As String
            If String.IsNullOrEmpty(name) Then Return "_"
            If reserved.Contains(name) Then Return name & "_"
            Return name
        End Function

        ''' <summary>
        ''' The dotted TypeScript declaration path of a CLR type: namespace plus the
        ''' declaring-type chain (nested types become ``Ns.Outer.Inner``), generic arity
        ''' stripped. Generic arguments are NOT rendered here — the type mapper owns
        ''' them, because an argument may be a generic parameter or a mapped type.
        ''' </summary>
        Friend Function DeclarationPath(t As Type) As String
            If t Is Nothing Then Return "any"
            Return PathOf(t)
        End Function

        ''' <summary>The dotted path of a (possibly nested, possibly generic) type definition.</summary>
        Private Function PathOf(t As Type) As String
            Dim parts As New List(Of String)
            Dim cur As Type = t

            While cur IsNot Nothing
                parts.Insert(0, StripArity(cur.Name))
                cur = cur.DeclaringType
            End While

            Dim ns As String = t.Namespace

            If Not String.IsNullOrEmpty(ns) Then parts.Insert(0, ns)

            Return String.Join(".", parts)
        End Function

    End Module

End Namespace
