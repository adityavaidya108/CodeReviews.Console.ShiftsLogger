﻿using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using ShiftLogger.ShiftTrack.Views;
using ShiftLogger.ShiftTrack.Models;

namespace ShiftLogger.ShiftTrack;

internal class App
{
    public async Task InitializeClientAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        await GetSelectionAsync(client);
    }

    private async Task GetSelectionAsync(HttpClient client)
    {
        string selection = Display.GetSelection("Welcome to Shift Logger", new[] { "View Workers/Shifts", "Create a Worker/Shift", "Edit Worker/Shift", "Delete Worker/Shift", "Exit Application" });
        while (selection != "Exit Application")
        {
            if (selection == "View Workers/Shifts")
            {
                selection = Display.GetSelection("What do you wish to do?", new[] { "View Workers", "View shifts", "Go to Main Menu" });
                if (selection == "View Workers")
                    await DisplayWorkersAsync(client);
                else if (selection == "View shifts")
                {
                    await DisplayShiftsAsync(client);
                }
                    
            }
            else if (selection == "Create a Worker/Shift")
            {
                selection = Display.GetSelection("What do you wish to do?", new[] { "Create a Worker", "Create a shift", "Go to Main Menu" });
                if (selection == "Create a Worker")
                {
                    Worker worker = Display.GetWorkerDetails();
                    await PostWorkerAsync(client, worker);
                }
                else if (selection == "Create a shift")
                {
                    bool workerExist = await DisplayWorkersAsync(client);
                    if (workerExist)
                    {
                        int workerId = Display.GetWorkerId();
                        bool workerIdExists = await CheckWorkerExistsAsync(client, workerId);
                        if (workerIdExists)
                        {
                            Shift shift = Display.GetShiftDetails(workerId);
                            await PostShiftAsync(client, shift);
                        }
                        else
                        {
                            Console.WriteLine("The entered worker does not exist. Please enter valid workerId");
                            Console.ReadLine();
                        }
                    }
                    else
                        Console.WriteLine("No worker exists to create a shift. Kindly create worker first");
                }
            }
            else if (selection == "Edit Worker/Shift")
            {
                selection = Display.GetSelection("What do you wish to do?", new[] { "Edit A Worker", "Edit a shift", "Go to Main Menu" });
                if (selection == "Edit A Worker")
                {
                    bool workerExist = await DisplayWorkersAsync(client);
                    if (workerExist)
                    {
                        int workerId = Display.GetWorkerId();
                        bool workerExists = await CheckWorkerExistsAsync(client, workerId);
                        if (workerExists)
                        {
                            Worker worker = Display.GetWorkerDetails();
                            worker.WorkerId = workerId;
                            await PutWorkersAsync(client, worker);
                        }
                        else
                        {
                            Console.WriteLine("The given worker id does not exist.");
                            Console.ReadLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("No worker exists to edit. Kindly create worker first");
                        Console.ReadLine();
                    }
                        
                }
                else if (selection == "Edit a shift")
                {
                    bool shiftsExist = await DisplayShiftsAsync(client);
                    if (shiftsExist)
                    {
                        int shiftId = Display.GetShiftIdEdit();
                        bool shiftIdExists = await CheckShiftExistsAsync(client,shiftId);
                        if (!shiftIdExists)
                        {
                            Console.WriteLine("The given shiftId does not exist. Please enter a valid shiftId");
                            Console.ReadLine();
                        }
                        else
                        {
                            Shift shift = Display.GetEditedShiftDetails();
                            int workerId = await GetWorkerIdForShift(client, shiftId);
                            if (workerId != -1)
                            {
                                shift.ShiftId = shiftId;
                                shift.WorkerId = workerId;
                                await PutShiftAsync(client, shift);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No shifts exist.Cannot edit.Press any key to continue");
                        Console.ReadLine();
                    }
                }
            }
            else if (selection == "Delete Worker/Shift")
            {
                selection = Display.GetSelection("What do you wish to do?", new[] { "Delete a Worker", "Delete a shift", "Go to Main Menu" });
                if (selection == "Delete a Worker")
                {
                    bool workerExist = await DisplayWorkersAsync(client);
                    if (workerExist) {
                        int workerId = Display.GetWorkerId();
                        await DeleteWorkerAsync(client, workerId);
                    }
                    else
                    {
                        Console.WriteLine("No workers present to delete. Kindly add some workers");
                        Console.ReadLine();
                    }
                }
                else if (selection == "Delete a shift")
                {
                    bool shiftsExist = await DisplayShiftsAsync(client);
                    if (shiftsExist)
                    {
                        int shiftId = Display.GetShiftId();
                        await DeleteShiftAsync(client, shiftId);
                    }
                    else
                    {
                        Console.WriteLine("No shifts exist.Cannot Delete");
                        Console.ReadLine();
                    }
                }
            }
            Console.Clear();
            selection = Display.GetSelection("Welcome to Shift Logger", new[] { "View Workers/Shifts", "Create a Worker/Shift", "Edit Worker/Shift", "Delete Worker/Shift", "Exit Application" });
        }

    }

    private async Task<bool> DisplayWorkersAsync(HttpClient client)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json = await client.GetStringAsync($"https://localhost:7134/api/Workers");
            List<Worker>? workers = JsonSerializer.Deserialize<List<Worker>>(json, options);
            if(workers.Count == 0)
            {
                Console.WriteLine("No workers exist. Cannot create shifts. Press any key to continue");
                return false;
            }
            else
                Display.DisplayWorkers(workers, new string[] { "Worker ID", "Name", "Email ID" });
            Console.WriteLine("Press any key to continue");
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("Request timed out.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
        finally
        {
            Console.ReadLine();
        }
    }
    private async Task<bool> DisplayShiftsAsync(HttpClient client)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string shiftJson = await client.GetStringAsync($"https://localhost:7134/api/Shifts");
            List<Shift>? shifts = JsonSerializer.Deserialize<List<Shift>>(shiftJson, options);
            if (shifts.Count == 0)
            {
                Console.WriteLine("No shifts exist. Cannot display.");
                return false;
            }
            else
            {
                string workerJson = await client.GetStringAsync($"https://localhost:7134/api/Workers");
                List<Worker>? workers = JsonSerializer.Deserialize<List<Worker>>(workerJson, options);
                var shiftDetails = from shift in shifts
                                   join worker in workers on shift.WorkerId equals worker.WorkerId
                                   select new
                                   {
                                       shift.ShiftId,
                                       WorkerName = worker.Name,
                                       shift.Date,
                                       shift.StartTime,
                                       shift.EndTime,
                                       shift.Duration
                                   };
                Display.DisplayShifts(shiftDetails.ToList(), new string[] { "ShiftID", "Worker Name", "Date", "Start Time", "End Time", "Duration" });
                Console.WriteLine("Press any key to continue");
                return true;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("Request timed out.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task PostWorkerAsync(HttpClient client, Worker worker)
    {
        try
        {
            var url = "https://localhost:7134/api/Workers";
            var jsonContent = new StringContent(JsonSerializer.Serialize(worker), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Worker added successfully: {responseContent}");
            }
            else
            {
                Console.WriteLine($"Failed to add worker. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("Request timed out.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task PostShiftAsync(HttpClient client, Shift shift)
    {
        try
        {
            var url = "https://localhost:7134/api/Shifts";
            var jsonContent = new StringContent(JsonSerializer.Serialize(shift), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Shift added successfully.Press any key to continue");
            }
            else
            {
                Console.WriteLine($"Failed to add shift. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out. {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task DeleteWorkerAsync(HttpClient client, int workerId)
    {
        try
        {
            var url = $"https://localhost:7134/api/Workers/{workerId}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Worker with ID {workerId} deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to delete worker. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out. {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task<int> GetWorkerIdForShift(HttpClient client, int shiftId)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var url = $"https://localhost:7134/api/Shifts/{shiftId}";
            var response = await client.GetStringAsync(url);
            Shift? shift = JsonSerializer.Deserialize<Shift>(response, options);
            if (shift != null)
            {
                return shift.WorkerId;
            }
            else
            {
                Console.WriteLine("Shift not found.");
                return -1;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return -1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return -1;
        }
    }


    private async Task DeleteShiftAsync(HttpClient client, int shiftId)
    {
        try
        {
            var url = $"https://localhost:7134/api/Shifts/{shiftId}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Worker with ID {shiftId} deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to delete worker. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out. {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task PutShiftAsync(HttpClient client, Shift shift)
    {
        try
        {
            var url = $"https://localhost:7134/api/Shifts/{shift.ShiftId}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(shift), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var response = await client.PutAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Shift edited successfully: {responseContent}");
            }
            else
            {
                Console.WriteLine($"Failed to edit shift. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out. {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task PutWorkersAsync(HttpClient client, Worker worker)
    {
        try
        {
            var url = $"https://localhost:7134/api/Workers/{worker.WorkerId}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(worker), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var response = await client.PutAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Worker edited successfully");
            }
            else
            {
                Console.WriteLine($"Failed to edit worker. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("Request timed out.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private async Task<bool> CheckWorkerExistsAsync(HttpClient client, int workerId)
    {
        try
        {
            var url = $"https://localhost:7134/api/Workers/{workerId}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            else
            {
                Console.WriteLine($"Unexpected status code: {response.StatusCode}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> CheckShiftExistsAsync(HttpClient client, int shiftId)
    {
        try
        {
            var url = $"https://localhost:7134/api/Shifts/{shiftId}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            else
            {
                Console.WriteLine($"Unexpected status code: {response.StatusCode}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

}