# TestRail Nunit Sync

TestRail Nunit Sync is a .NET library for synchronizing NUnit3 test cases & test results with Gurock TestRail.

## Installation

Download this project and build locally to produce the executable.

## Usage

#### Syncing Test Cases

```bash
TestRail_Nunit_Sync 
 --nunit-test-cases-file="C:\NUnitTestCases.xml" 
 --testrail-url="your-instance.testrail.net"
 --testrail-user-email="test@example.com" 
 --testrail-user-password="password123"
 --testrail-project-id="7"
```

#### Syncing Test Results

```bash
TestRail_Nunit_Sync 
 --nunit-test-results-file="C:\NUnitTestResults.xml" 
 --testrail-run-name="SIT Automated Test Run"
 --testrail-url="your-instance.testrail.net"
 --testrail-user-email="test@example.com" 
 --testrail-user-password="password123"
 --testrail-project-id="7"
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as needed.

## License
[MIT](https://choosealicense.com/licenses/mit/)