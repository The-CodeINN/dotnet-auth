# ASP.NET 8 Product API with Enhanced User Management and MailKit Email Service

This project is a web application built using ASP.NET 8 Web API that provides enhanced user management features, product CRUD operations, and email functionality using MailKit.

## Features

- User Registration and Login with JWT authentication
- Email confirmation for new user registrations
- Welcome email for new users
- Password reset functionality
- Refresh token mechanism
- CRUD operations for Products (not implemented in this snippet, but can be added similarly to the User operations)
- SQLite database with Entity Framework Core
- MailKit integration for email services with Gmail SMTP
- Swagger/OpenAPI documentation

## Project Structure

The project follows a layered architecture for better organization and separation of concerns:

- Controllers: Handle incoming requests, validate them, and forward to appropriate services.
- Services: Manage business logic and interact with repositories.
- Repositories: Handle data access and interact with the database.
- Models: Define data schemas for the application.
- Middleware: Custom middleware for error handling.

## Setup and Configuration

1. Clone the repository
2. Update the `appsettings.json` file with your JWT secret key, Gmail account details, and application URL
3. For Gmail, you need to use an App Password. Follow these steps to set it up:
   - Go to your Google Account settings
   - Navigate to Security
   - Under "Signing in to Google," select App Passwords (you may need to enable 2-Step Verification first)
   - Select "Mail" and "Other (Custom name)" from the dropdown menus
   - Enter a name for the app (e.g., "ASP.NET API")
   - Click "Generate" and use the generated password in your `appsettings.json`
4. Run the following commands in the project directory:
   ```
   dotnet ef database update
   dotnet run
   ```

## API Endpoints

### User Management

- POST /api/user/register - Register a new user
- POST /api/user/login - Login and receive JWT tokens
- POST /api/user/refresh-token - Refresh the access token
- POST /api/user/send-password-reset-email - Request a password reset
- POST /api/user/reset-password - Reset the password
- POST /api/user/verify-email - Verify user's email
- POST /api/user/logout - Logout (invalidate refresh token)
- GET /api/user/me - Get current user's information

### Product Management (to be implemented)

- GET /api/product - Get all products (with pagination and filtering)
- GET /api/product/{id} - Get a specific product
- POST /api/product - Create a new product
- PUT /api/product/{id} - Update an existing product
- DELETE /api/product/{id} - Delete a product

## Security

- Passwords are hashed using BCrypt before storage
- JWT tokens are used for authentication with refresh token mechanism
- HTTPS is enforced for all communications
- Email verification is required for new accounts
- Password reset functionality is available

## Email Templates

The application includes HTML email templates for:

- Welcome email
- Email confirmation

These templates are responsive and styled for a better user experience.

## Future Improvements

- Add unit and integration tests
- Implement rate limiting to prevent abuse
- Add logging for better debugging and monitoring
- Create more email templates for various user interactions
