function Build-SharedLibs-For-Processor 
{
	$is64 = $(if([IntPtr]::Size -eq 8) { $true } else { $false })
	if ($is64)
	{
		Copy-Item "$lib_dir\x64\*" "$lib_dir"
	}
	else
	{
		Copy-Item "$lib_dir\x86\*" "$lib_dir"
	}
}