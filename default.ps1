properties { 
  $base_dir  = resolve-path .
  $lib_dir = "$base_dir\SharedLibs"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\Rhino.Security-vs2008.sln" 
  $version = "1.3.0.0"
  $humanReadableversion = "1.3"
  $tools_dir = "$base_dir\Tools"
  $release_dir = "$base_dir\Release"
  $uploadCategory = "Rhino-Security"
  $uploader = "..\Uploader\S3Uploader.exe"
  $xunitVersion = "1.9.2"
  $xunitPath = "$base_dir\packages\xunit.runners.${xunitVersion}\tools"
} 

include .\psake_ext.ps1
include .\SharedLibs\build-ext\x64detection.ps1
	
task default -depends Release

task Clean { 
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue 
  remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue 
  Build-SharedLibs-For-Processor 
} 

task Init -depends Clean { 
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Security\Properties\AssemblyInfo.cs" `
		-title "Rhino Security $version" `
		-description "Security Library for NHibernate" `
		-company "Hibernating Rhinos" `
		-product "Rhino Security $version" `
		-version $version `
		-copyright "Hibernating Rhinos & Ayende Rahien 2004 - 2009"
		
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Security.Tests\Properties\AssemblyInfo.cs" `
		-title "Rhino Security Tests $version" `
		-description "Security Library for NHibernate" `
		-company "Hibernating Rhinos" `
		-product "Rhino Security Tests $version" `
		-version $version `
		-clsCompliant "false" `
		-copyright "Hibernating Rhinos & Ayende Rahien 2004 - 2009"
		
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Security.ActiveRecord\Properties\AssemblyInfo.cs" `
		-title "Rhino Security $version" `
		-description "Security Library for NHibernate" `
		-company "Hibernating Rhinos" `
		-product "Rhino Security $version" `
		-version $version `
		-copyright "Hibernating Rhinos & Ayende Rahien 2004 - 2009"
		
	new-item $release_dir -itemType directory 
	new-item $buildartifacts_dir -itemType directory 
} 

task Compile -depends Init { 
  & msbuild "$sln_file" "/p:OutDir=$build_dir\\" /p:Configuration=Release
  if ($lastExitCode -ne 0) {
        throw "Error: Failed to execute msbuild"
  }

} 

task Test -depends Compile {
  $old = pwd
  cd $build_dir
  exec "$xunitPath\xunit.console.clr4.exe" "$build_dir\Rhino.Security.Tests.dll"
  cd $old		
}

task Release -depends Test {
	& $tools_dir\zip.exe -9 -A -j `
		$release_dir\Rhino.Security-$humanReadableversion-Build-$env:ccnetnumericlabel.zip `
		$build_dir\Rhino.Security.dll `
		Rhino.Security\Rhino.Security.xml `
		license.txt `
		"How to use.txt" `
		acknowledgements.txt
	if ($lastExitCode -ne 0) {
        throw "Error: Failed to execute ZIP command"
    }
}

task Upload -depends Release {
	Write-Host "Starting upload"
	if (Test-Path $uploader) {
		$log = $env:push_msg 
    if($log -eq $null -or $log.Length -eq 0) {
      $log = git log -n 1 --oneline		
    }
		&$uploader "$uploadCategory" "$release_dir\Rhino.Security-$humanReadableversion-Build-$env:ccnetnumericlabel.zip" "$log"
		
		if ($lastExitCode -ne 0) {
      write-host "Failed to upload to S3: $lastExitCode"
			throw "Error: Failed to publish build"
		}
	}
	else {
		Write-Host "could not find upload script $uploadScript, skipping upload"
	}
}
