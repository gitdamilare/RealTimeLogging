# Tecan

## Task  
Our customers are interested in statistical data of their Tecan products. To facilitate this, design a system to gather and display run time data. Our robots write log files in a specific predefined structure. Once a log file reaches a certain size, it is zipped, and logging continues in a new log file. These log files shall be analyzed, and the statistical information shall be displayed on a web application

1. -[x] Analysis of multiple log files at the same time (Priority 1)
2. -[x] Extraction of the following data:
    - [x] Number of errors and warnings (Priority 1)
    - [x] Overall run time (Priority 3)
    - [x] The complete number of messages which have been parsed (Priority 3)
3. -[x] Extendable to analyze more data from the log files in the future (Priority 2)
4. -[x] Display the following statistical data:
    - [x] Number of errors and warnings (Priority 1)
    - [x] Overall run time (Priority 3)
    - [x] The complete number of messages which have been parsed (Priority 3)
5. -[x] Refresh the UI when new statistical data has been received (Priority 1)

Please use the C# .NET Core framework and Microsoft Azure where applicable, apart from that you are free to choose any technology stack you are comfortable with. Harnessing Serverless technologies is a major plus.

## Solution WorkFlow - Real-Time Statistical Dashboard
<img alt="portfolio_view" src="https://github.com/gitdamilare/TecanSolution/blob/main/workflow.png">

## Prerequirements 
* Visual Studio 2019
* .NET Core SDK 3.1
* SQL Server
* Angualr CLI 9.0 above
* Azure SDK 

## Running this Project
> **All Restful APIs,Azure functions, Database are hosted on Azure already and connected to the Frontend project**, 
> **Running just the frontend gets the application fired up**

* Clone this project 
* Open the ReportLogFrontend project in VS Code or command line. Run the following command to get all the packages in your project.
```
npm install 
```
* Now you can run the frontend project with following command. 
> _Note CROS Issues :cop: : if it not running on port 4200, make changes in package.json and angular.json // or add --port 4200 to command_  
```
ng serve
```
if you are still facing some issues run the following command and resolve the issues if you have any.
```
npm audix fix
```

 ## BackEnd API Documentation
[Swagger UI](https://reportlogapiservices.azurewebsites.net/swagger/index.html)

## Still wants to run Services and Azure Functions Locally :+1:

* Setup up the right connection string from appsetting.json and make changes from startup.cs
* cd into the ReportLogEntityFrameworkCore project and run the following command
```
Add-Migration "<name>"
```
```
Update-Database
```
* In the logBlobTriggerFunction project, make changes to host.json or local.setting.json connections string

* Run logBlobTriggerFunction and ReportLogAPI project together. 
[How to: Set multiple startup projects](https://docs.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2019)

## Performance Improvement Plan
* When file is uploaded, process it and then make a Signal R call to notify all connected devices (append to the existing data in the UI) before saving to Database
* Use Azure CosmosDB or MongoDB, a little performance bump up 
