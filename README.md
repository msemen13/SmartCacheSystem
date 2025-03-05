# SmartCache

This project is a simple Orleans-based app for managing compromised email addresses. It allows you to check if an email is breached, add new breached emails, and remove them from the list. 
The app relies on Azure Blob Storage for state persistence, so make sure to emulate it locally.

## What You’ll Need

Before you get started, you’ll need to have a few things installed:

- [.NET SDK](https://dotnet.microsoft.com/download/dotnet)
- [Azurite](https://github.com/Azure/Azurite) for emulating Azure Blob Storage (locally)

## Setting It All Up
Clone the Repository

Start by cloning this repo to your local machine:
```bash
git clone https://github.com/msemen13/SmartCacheSystem.git
```
Once you've cloned the project, install all the necessary dependencies by running:
```bash
dotnet restore
```

Since the app uses Azure Blob Storage for persistent state, you'll need to run an emulator locally. If you haven't already, you can install Azurite via npm:
```bash
npm install -g azurite
```
Then start the emulator by running:
```bash
azurite
```
This will spin up a local Azure Blob Storage emulator. Make sure it’s running before you proceed.

## Run the Application

You need to start both the client and host at the same time.
You can do this  by right clicking on the solution and selecting "Configure Startup Projects". There you select Start for both client and host.

## API Endpoints
Here’s a quick overview of the available endpoints you can use once the app is up and running:

1. Check if an email is breached
Endpoint: GET /checkemail/{email}
Description: Check if the given email has been breached.
Response:
- 200 OK if the email is breached.
- 404 Not Found if the email is not breached.
Example:
```bash
curl http://localhost:5000/checkemail/test@gmail.com
```
2. Add a breached email
Endpoint: POST /addemail
Description: Adds an email to the breached list.
Query Parameter:
- email: The email address to be checked and added to the breached list.
Response:
- 201 Created if the email was successfully added.
- 409 Conflict if the email is already marked as breached.
Example:
```bash
POST http://localhost:<your_port>/addemail?email=test@gmail.com
```
3. Add multiple breached emails
Endpoint: POST /addemails
Description: Adds multiple emails to the breached list.
Body:
```json
{
  "emails": ["test1@gmail.com", "test2@gmail.com"]
}
```
Response:
- 200 OK when all emails are added successfully.
Example:
```bash
curl -X POST http://localhost:<your_port>/addemails -d "{\"emails\": [\"test1@gmail.com\", \"test2@gmail.com\"]}" -H "Content-Type: application/json"
```
4. Remove a breached email
Endpoint: POST /deleteemail
Description: Removes a breached email from the list.
Query Parameter:
- email: The email address to be removed from the breached list.
Response:
- 200 OK when the email is removed successfully.
Example:
```bash
POST http://localhost:<your_port>/deleteemail?email=test@gmail.com
```
Running Tests
You can run the unit tests with:
```bash
dotnet test
```

## API Key Authentication
This API requires an API Key for authentication. Requests without a valid API Key will be rejected.

Using the API Key in API Clients
### Postman
- Open Postman.
- Under the Authorization tab, choose API Key.
- Set Key = X-API-Key.
- Set Value = YourSecureGeneratedApiKey12345.
- Select Add to Header and send your request.
### Manual Request Example (cURL)
```sh
curl -X POST "https://localhost:7046/addemail" \
     -H "X-API-Key: RandomPassKey5489" \
     -H "Content-Type: application/json" \
     -d '{ "email": "test@gmail.com" }'
```

Error Responses
- 401 Unauthorized → API Key is missing or invalid.
- 403 Forbidden → API Key does not have permission to access this resource.

### Configuration
The API key is stored securely in appsettings.json or environment variables:
```json
{
  "ApiKey": "RandomPassKey5489"
}
```
