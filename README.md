# School Lunch Helper
The School Lunch Helper is designed to help schools sell lunch to their students quickly and easily. It has three GUI programs, the Card Scanner UI that acts as a cash register for scanning student cards with a barcode reader, the Lunch Manager which allows the user to view transactions, edit students' balances, and add and edit food items, and the Student Manager which allows CSVs of students to be imported into the database.


# Features
## Card Scanner UI
- Can run completely offline in the event of a network outage
- Transactions that have not been sent to the database are stored in a JSON file to ensure that they are not lost in the event of the program crashing
- Seamlessly integrates with barcode scanners
- Automatically sends transactions to the database when connected to a network

## Lunch Manager
- View and search transactions, filter by date
- Add and remove from students' balances
- Search students by name or student number
- Export CSVs of transactions and students
- Add, edit, delete, and search through types of food
- Autosaves changes

## Student Manager
- Export CSVs of students at any school
- Import CSVs of students to add to the database
- Imported CSVs' columns do not need to be consistent with the way the program stores students - choose which CSV column maps to which field


# Usage
The program will need to be prepared and compiled before it can be ran. See the preparation instructions below in the database preparation and configuration files sections.

### Using a Barcode Scanner
To use a barcode scanner, ensure that the barcode scanner can be used with a Windows computer, then plug it in and let the driver install. It should now work with the Card Scanner UI.


# Database Preparation
The School Lunch Helper was designed for and tested on a SQL database. Entity Framework Core (EF Core) is used to create migrations for and communicate with the database.

## Schools
The "Schools" database table requires a record of each school using the system along with a unique school ID number. These ID numbers are used in configuration files. 

## Create Migrations with EF Core
Creating database migrations with EF Core's Designer is simple - the instructions can be found [here](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=vs).

## Adding Students to the Database
The Student Manager GUI can be used to add students individually via a dialog window or by importing CSV files.

### Importing a CSV
Pressing the "Import CSV" button on the Student Manager's GUI will open a window that allows you to choose a file and then choose which columns in the file should match to which fields. Match the columns properly then press the "Import" button.

## Connection String
The connection string must be saved in Azure Keyvault. You will need to put this URL in a configuration file.


# Configuration Files
The LunchAPI, LunchManager, StudentManager, and CardScannerUI each require one or multiple configuration files. This section outlines their requirements.

## Lunch API
The Lunch API requires two configuration files:

### EfCoreDesignerSettings.config
This file contains important settings for EF Core's designer. Change the "keyvaultEndpoint" key's value to the Azure Keyvault URL that contains the connection string.

Format:
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<appSettings>
		<add key="keyvaultEndpoint" value="Azure Keyvault URL that has the connection string"/>
	</appSettings>
</configuration>
```
### launchSettings.json
Change the "keyvaultEndpoint" key's value to the Azure Keyvault URL that contains the connection string. 
Format:
```
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:31821",
      "sslPort": 44347
    }
  },
  "profiles": {
    
    "LunchAPI": {
      "commandName": "Project",
      "dotnetRunMessages": "true",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "KEYVAULT_ENDPOINT": "Azure Keyvault URL that has the connection string"
      }
    }
  }
}
```

## Lunch Manager and Card Scanner UI
The Lunch Manager and Card Scanner UI each require a configuration file named App.config. The "thisSchool" key's value should be the ID number in the database of the school the program is being used for. Change the "apiUri" key's value to your API's URI.

The files must be formatted like this:
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<appSettings>
		<add key="apiUri" value="the API's URI"/>
		<add key="thisSchool" value="school's ID number"/>
	</appSettings>
</configuration>
```

## Student Manager
The Student Manager requires one configuration file named App.config. Change the "apiUri" key's value to your API's URI.

The file should be formatted like this:
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<appSettings>
		<add key="apiUri" value="the API's URI"/>
	</appSettings>
</configuration>
```

