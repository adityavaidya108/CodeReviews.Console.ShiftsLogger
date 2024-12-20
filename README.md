# Shift Logger Application

## Features
* This is an application where we create an API and the console app that will consume it. Here we record a worker's shifts.
* There are  two applications: the Web API and the UI that will call it.
* All validation and user input happen in the UI app. Front-end project has try-catch blocks around the API calls so it handles unexpected errors (i.e. the API isn't running or returns a 500 error.)
* The web Api is developed using Entity Framework, as it takes care of the CRUD operations needed. It was tested separately using Postman before creating the UI project.

    ![Screenshot](/images/Postman.png)
* Following screenshots show some app functionalities.
* Option Menu

    ![Screenshot](/images/Menu.png)
* Create Worker

    ![Screenshot](/images/AddWorker.png)

* Create Shift (All necessary validation for adding the workers and shifts ate taken care for before creating a worker or a shift)
    ![Screenshot](/images/Validation1.png)

* Edit Worker/ Shifts
    ![Screenshot](/images/EditWorker.png)

    ![Screenshot](/images/EditShift1.png)

* Delete Worker/Shift
    ![Screenshot](/images/DeleteShift.png)


## To run create a .env file in the root directory of the Web Api(ShiftLogger.AdityaVaidya) and add the following properties
* CONNECTION_STRING=Connection string to your database


To create database locally run the below command in your NuGet Package Manager Console:
* dotnet ef migrations add InitialCreate
* dotnet ef database update

Now start the  Web API(ShiftLogger.AdityaVaidya) project and the console app(ShiftLogger.ShiftTrack) separately. 
    ![Screenshot](/images/SeparateProjects.png)