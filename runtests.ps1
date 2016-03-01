# Run the basic sets of tests using powershell
$vsversions = @("12.0", "14.0", "15.0")

$vstest =  ''
foreach($vsversion in $vsversions) {
	$path = "C:\Program Files (x86)\Microsoft Visual Studio $vsversion\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
	if (Test-Path $path) {
		$vstest = $path
	}
}

if ($vstest -eq '') {
	echo "Could not find valid vstest.console"
	return
}

$tests = @("NpmTests", "NodeTests", "ProfilerTests", "JSAnalysisTests", "MockVsTests", "Nodejs.Tests.UI")
$testFiles = $tests | % { ".\BuildOutput\Release14.0\Tests\$_.dll" }

& "$vstest" @args $testFiles
