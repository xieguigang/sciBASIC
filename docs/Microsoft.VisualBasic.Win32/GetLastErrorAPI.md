# GetLastErrorAPI
_namespace: [Microsoft.VisualBasic.Win32](./index.md)_

Wrapper for the returns value of api @``M:Microsoft.VisualBasic.Win32.GetLastErrorAPI.GetLastError``

> 
>  @``T:Microsoft.VisualBasic.Win32.GetLastErrorAPI.ErrorCodes`` reference msdn:
>  
>  + https://support.microsoft.com/en-us/kb/155011
>  + https://support.microsoft.com/EN-US/kb/155012
>  


### Methods

#### GetLastError
```csharp
Microsoft.VisualBasic.Win32.GetLastErrorAPI.GetLastError
```
针对之前调用的api函数，用这个函数取得扩展错误信息（在vb里使用：在vb中，用Err对象的``GetLastError``
 属性获取``GetLastError``的值。这样做是必要的，因为在api调用返回以及vb调用继续执行期间，
 vb有时会重设``GetLastError``的值）
 
 ``GetLastError``返回的值通过在api函数中调用``SetLastError``或``SetLastErrorEx``设置。函数
 并无必要设置上一次错误信息，所以即使一次``GetLastError``调用返回的是零值，也不能
 担保函数已成功执行。只有在函数调用返回一个错误结果时，这个函数指出的错误结果
 才是有效的。通常，只有在函数返回一个错误结果，而且已知函数会设置``GetLastError``
 变量的前提下，才应访问``GetLastError``；这时能保证获得有效的结果。``SetLastError``函
 数主要在对api函数进行模拟的dll函数中使用。
> 
>  ``GetLastError``返回的值通过在api函数中调用``SetLastError``或``SetLastErrorEx``设置。函数并无必要设置上一次错误信息，
>  所以即使一次``GetLastError``调用返回的是零值，也不能担保函数已成功执行。只有在函数调用返回一个错误结果时，
>  这个函数指出的错误结果才是有效的。通常，只有在函数返回一个错误结果，而且已知函数会设置``GetLastError``变量的前提下，
>  才应访问``GetLastError``；这时能保证获得有效的结果。``SetLastError``函数主要在对api函数进行模拟的dll函数中使用，
>  所以对vb应用程序来说是没有意义的
>  

#### GetLastErrorCode
```csharp
Microsoft.VisualBasic.Win32.GetLastErrorAPI.GetLastErrorCode
```
Retrieves the calling thread's last-error code value. The last-error code is maintained on a per-thread basis. 
 Multiple threads do not overwrite each other's last-error code.

_returns: 
 The return value is the calling thread's last-error code.
 
 The Return Value section of the documentation for each function that sets the last-error code notes the conditions 
 under which the function sets the last-error code. Most functions that set the thread's last-error code set it 
 when they fail. However, some functions also set the last-error code when they succeed. If the function is not 
 documented to set the last-error code, the value returned by this function is simply the most recent last-error 
 code to have been set; some functions set the last-error code to 0 on success and others do not.
 _
> 
>  Functions executed by the calling thread set this value by calling the SetLastError function. You should call the 
>  GetLastError function immediately when a function's return value indicates that such a call will return useful data. 
>  That is because some functions call SetLastError with a zero when they succeed, wiping out the error code set by 
>  the most recently failed function.
>  To obtain an error string for system error codes, use the FormatMessage function. For a complete list of error codes 
>  provided by the operating system, see System Error Codes.
>  The error codes returned by a function are Not part of the Windows API specification And can vary by operating system 
>  Or device driver. For this reason, we cannot provide the complete list of error codes that can be returned by each 
>  function. There are also many functions whose documentation does Not include even a partial list of error codes that 
>  can be returned.
>  Error codes are 32-bit values (bit 31 Is the most significant bit). Bit 29 Is reserved For application-defined Error 
>  codes; no system Error code has this bit Set. If you are defining an Error code For your application, Set this bit 
>  To one. That indicates that the Error code has been defined by an application, And ensures that your Error code does 
>  Not conflict With any Error codes defined by the system.
>  
>  To convert a system error into an HRESULT value, use the HRESULT_FROM_WIN32 macro.
>  


