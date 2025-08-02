# MeterReadingApp

## Summary

This API is designed to accepts csv files including these three columns **AccountId** **MeterReadingDateTime** and **MeterReadValue**. Two example files can be found at **MeterReadingApi/TestFiles**.

The API has an optional Blazor Client UI which can be launched alongside the API to provide an alternate user interface, offering the same functionality.

## Setup

In order to use the application, there must be an active database connection. A file containing the setup scripts required to prepare the database for testing can be found at **MeterReadingApi/TestFiles/DatabaseSetupScript.txt**.

Additional appsettings for some info should be stored locally in the users secrets.json files. To implement these settings, add the following to your config in these two projects (amending values as required):

**MeterReadingApi:**
```
  "Authentication": {
    "SecretKey": "ThisIsTheDemoSecretKeyForLocalUseOnly",
    "Issuer": "https://localhost:7047",
    "Audience": "MeterReadingApi"
  },
  "ConnectionStrings": {
    "Default": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EnsekDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;"
  }
```

**MeterReadingApiClient:**
```
  "ApiUrl": "https://localhost:7047/api/"
```

## Notes

Authentication has been set up and is enforced for the API for one test user:

**UserName: ensek**

**Password: Test1!**

***This test user validation is for local development testing only*** 

Before any deployments, this validation *must* be replaced with a call to a reputable authentication platform such as Auth0.
