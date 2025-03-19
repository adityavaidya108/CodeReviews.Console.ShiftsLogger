# Shift Logger Application

## Features
* This is an application where workers and their shifts are recorded. In this Application, each shift is assigned to exactly one worker, establishing a one-to-many relationship. A worker can have multiple shifts, but each shift uniquely belongs to a single worker.There can be multiple workers.
* There are  two applications: the Web API and the UI that will consume it.
* All validation and user input happen in the UI app. The front-end project has try-catch blocks around the API calls so it handles unexpected errors (i.e. the API isn't running or returns a 500 error.)
* The Web API is developed using Entity Framework, as it takes care of the CRUD operations needed. It was tested separately using Postman before creating the UI project.

    ![Screenshot](/images/Postman.png)

## To run, create a .env file in the root directory of the Web Api(ShiftLogger.AdityaVaidya) and add the following properties
* CONNECTION_STRING=Connection string to your database


To create database locally run the below command in your NuGet Package Manager Console:
* dotnet ef migrations add InitialCreate
* dotnet ef database update

Now, start the  Web API (ShiftLogger.AdityaVaidya) project and the console app(ShiftLogger.ShiftTrack) separately. 
