<div align="center">
  <img src="https://img.shields.io/badge/AP%20Cleaning%20Service-Backend%20API-392C3A?style=for-the-badge&labelColor=5DADE2" alt="AP Cleaning Service Backend">
  
  # 🚀 AP Cleaning Service - Backend 
  
  <p align="center">
    <strong>RESTful API for booking and dispatch management platform</strong>
  </p>

  <p align="center">
    Built with excellence by <strong>WebCraft Solutions</strong> | 2025
  </p>

  <p align="center">
    <a href="#-features">Features</a>
    •
    <a href="#-tech-stack">Tech Stack</a>
    •
    <a href="#-installation">Installation</a>
    •
    <a href="#-api-documentation">API Docs</a>
    •
    <a href="#-deployment">Deployment</a>
  </p>

  <p align="center">
    <img src="https://img.shields.io/badge/Node.js-18.x-339933?style=flat-square&logo=node.js&logoColor=white" alt="Node.js">
    <img src="https://img.shields.io/badge/Express-4.18-000000?style=flat-square&logo=express&logoColor=white" alt="Express">
    <img src="https://img.shields.io/badge/TypeScript-5.0-3178C6?style=flat-square&logo=typescript&logoColor=white" alt="TypeScript">
    <img src="https://img.shields.io/badge/Azure%20SQL-Database-0078D4?style=flat-square&logo=microsoft-azure&logoColor=white" alt="Azure SQL">
    <img src="https://img.shields.io/badge/JWT-Auth-000000?style=flat-square&logo=json-web-tokens&logoColor=white" alt="JWT">
  </p>

  <p align="center">
    <img src="https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square" alt="Build">
    <img src="https://img.shields.io/badge/Coverage-75%25-yellow?style=flat-square" alt="Coverage">
    <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="License">
  </p>
</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Database Setup](#-database-setup)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Authentication](#-authentication)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Team](#-team)
- [License](#-license)

---

## 🌟 Overview

The **AP Cleaning Service Backend** is a robust RESTful API built with Node.js and Express.js, providing secure and scalable backend services for the cleaning service booking platform. It handles user authentication, booking management, payment processing, and real-time notifications.

### Key Highlights

- **RESTful Architecture** - Clean API design following REST principles
- **Secure Authentication** - JWT-based auth with refresh tokens
- **Payment Integration** - PayFast gateway for South African payments
- **Real-time Notifications** - SMS via Twilio, Email via SendGrid
- **Scalable Database** - Azure SQL Database with optimized queries
- **Comprehensive Testing** - 75%+ code coverage with automated tests

---

## ✨ Features

### Core Functionality

- ✅ **User Management** - Registration, login, profile management
- ✅ **Booking System** - Create, update, cancel bookings
- ✅ **Service Management** - Dynamic service catalog
- ✅ **Availability Checking** - Real-time cleaner availability
- ✅ **Payment Processing** - Secure PayFast integration
- ✅ **Driver Assignment** - Automated dispatch system
- ✅ **Notifications** - Email and SMS confirmations
- ✅ **Admin Dashboard API** - Analytics and reporting endpoints
- ✅ **Role-Based Access** - Customer, Cleaner, Driver, Admin roles
- ✅ **File Uploads** - Profile pictures and documents

### Security Features

- 🔒 JWT Authentication with refresh tokens
- 🔒 Password hashing with bcrypt
- 🔒 Rate limiting and DDoS protection
- 🔒 SQL injection prevention
- 🔒 XSS protection
- 🔒 CORS configuration
- 🔒 Input validation and sanitization
- 🔒 API key authentication for external services

---

## 🛠 Tech Stack

| Technology | Purpose |
|------------|---------|
| **Node.js** v18 | Runtime environment |
| **Express.js** v4.18 | Web framework |
| **TypeScript** v5.0 | Type safety |
| **Azure SQL Database** | Primary database |
| **Redis** | Caching and sessions |
| **JWT** | Authentication |
| **bcrypt** | Password hashing |
| **PayFast** | Payment processing |
| **Twilio** | SMS notifications |
| **SendGrid** | Email service |
| **Winston** | Logging |
| **Jest** | Testing framework |
| **Swagger** | API documentation |

---

## 💻 Installation

### Prerequisites

- Node.js v16.0.0 or higher
- npm v8.0.0 or higher
- Azure SQL Database instance
- Redis server (optional for caching)

### Setup Instructions

1. **Clone the repository**
```bash
git clone https://github.com/iamphineas/APCleaningBackend.git
cd APCleaningBackend
```

2. **Install dependencies**
```bash
npm install
```

3. **Create environment file**
```bash
cp .env.example .env
# Update with your configuration
```

4. **Run database migrations**
```bash
npm run migrate
```

5. **Seed database (optional)**
```bash
npm run seed
```

6. **Start development server**
```bash
npm run dev
```

The API will be available at `http://localhost:5000`

---

## ⚙ Configuration

### Environment Variables

Create a `.env` file in the root directory:

```env
# Server Configuration
NODE_ENV=development
PORT=5000
API_VERSION=v1

# Database Configuration
DB_HOST=your-azure-sql-server.database.windows.net
DB_PORT=1433
DB_NAME=apcleaning
DB_USER=your-username
DB_PASSWORD=your-password
DB_ENCRYPT=true

# Authentication
JWT_SECRET=your-super-secret-jwt-key
JWT_EXPIRE=1h
JWT_REFRESH_SECRET=your-refresh-token-secret
JWT_REFRESH_EXPIRE=7d

# PayFast Configuration
PAYFAST_MERCHANT_ID=your-merchant-id
PAYFAST_MERCHANT_KEY=your-merchant-key
PAYFAST_PASSPHRASE=your-passphrase
PAYFAST_URL=https://sandbox.payfast.co.za/eng/process

# Twilio SMS
TWILIO_ACCOUNT_SID=your-account-sid
TWILIO_AUTH_TOKEN=your-auth-token
TWILIO_PHONE_NUMBER=+27123456789

# SendGrid Email
SENDGRID_API_KEY=your-sendgrid-api-key
SENDGRID_FROM_EMAIL=noreply@apcleaning.co.za
SENDGRID_FROM_NAME=AP Cleaning Service

# Redis Cache (Optional)
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# Logging
LOG_LEVEL=debug
LOG_FILE=logs/app.log
```

---

## 🗄 Database Setup

### Azure SQL Database Schema

```sql
-- Users Table
CREATE TABLE Users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100),
    last_name NVARCHAR(100),
    phone NVARCHAR(20),
    role NVARCHAR(50) DEFAULT 'customer',
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Services Table
CREATE TABLE Services (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    base_price DECIMAL(10, 2) NOT NULL,
    duration_hours INT NOT NULL,
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Bookings Table
CREATE TABLE Bookings (
    id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT FOREIGN KEY REFERENCES Users(id),
    service_id INT FOREIGN KEY REFERENCES Services(id),
    cleaner_id INT FOREIGN KEY REFERENCES Users(id),
    driver_id INT FOREIGN KEY REFERENCES Users(id),
    booking_date DATE NOT NULL,
    booking_time TIME NOT NULL,
    address NVARCHAR(500) NOT NULL,
    status NVARCHAR(50) DEFAULT 'pending',
    total_price DECIMAL(10, 2) NOT NULL,
    payment_status NVARCHAR(50) DEFAULT 'unpaid',
    notes NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Payments Table
CREATE TABLE Payments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    booking_id INT FOREIGN KEY REFERENCES Bookings(id),
    amount DECIMAL(10, 2) NOT NULL,
    payment_method NVARCHAR(50),
    transaction_id NVARCHAR(255),
    status NVARCHAR(50),
    payfast_reference NVARCHAR(255),
    created_at DATETIME2 DEFAULT GETDATE()
);
```

### Migration Commands

```bash
# Run migrations
npm run migrate

# Rollback migrations
npm run migrate:rollback

# Create new migration
npm run migrate:create -- --name create_users_table
```

---

## 📚 API Documentation

### Base URL
```
Development: http://localhost:5000/api/v1
Production: https://api.apcleaning.co.za/v1
```

### Authentication Endpoints

#### Register User
```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "first_name": "John",
  "last_name": "Doe",
  "phone": "+27123456789"
}
```

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

#### Refresh Token
```http
POST /auth/refresh
Content-Type: application/json

{
  "refresh_token": "your-refresh-token"
}
```

### Booking Endpoints

#### Create Booking
```http
POST /bookings
Authorization: Bearer {token}
Content-Type: application/json

{
  "service_id": 1,
  "booking_date": "2025-03-15",
  "booking_time": "09:00",
  "address": "123 Main St, Cape Town",
  "notes": "Please ring doorbell"
}
```

#### Get User Bookings
```http
GET /bookings
Authorization: Bearer {token}
```

#### Get Booking Details
```http
GET /bookings/{id}
Authorization: Bearer {token}
```

#### Cancel Booking
```http
PUT /bookings/{id}/cancel
Authorization: Bearer {token}
```

### Service Endpoints

#### Get All Services
```http
GET /services
```

#### Get Service Details
```http
GET /services/{id}
```

### Availability Endpoints

#### Check Availability
```http
POST /availability/check
Content-Type: application/json

{
  "date": "2025-03-15",
  "time": "09:00",
  "service_id": 1
}
```

### Payment Endpoints

#### Initialize Payment
```http
POST /payments/initialize
Authorization: Bearer {token}
Content-Type: application/json

{
  "booking_id": 123,
  "amount": 500.00
}
```

#### Verify Payment
```http
POST /payments/verify/{transaction_id}
```

### Admin Endpoints

#### Get Dashboard Stats
```http
GET /admin/dashboard
Authorization: Bearer {admin-token}
```

#### Get All Bookings
```http
GET /admin/bookings
Authorization: Bearer {admin-token}
```

#### Assign Cleaner
```http
PUT /admin/bookings/{id}/assign-cleaner
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "cleaner_id": 5
}
```

### Response Format

#### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    // Response data
  }
}
```

#### Error Response
```json
{
  "success": false,
  "message": "Error message",
  "error": {
    "code": "ERROR_CODE",
    "details": "Detailed error information"
  }
}
```

### Status Codes

| Code | Description |
|------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Access denied |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 422 | Unprocessable Entity - Validation failed |
| 500 | Internal Server Error |

---

## 📁 Project Structure

```
APCleaningBackend/
├── 📂 src/
│   ├── 📂 config/         # Configuration files
│   │   ├── database.ts    # Database connection
│   │   ├── redis.ts       # Redis configuration
│   │   └── swagger.ts     # Swagger setup
│   │
│   ├── 📂 controllers/    # Request handlers
│   │   ├── auth.controller.ts
│   │   ├── booking.controller.ts
│   │   ├── service.controller.ts
│   │   ├── payment.controller.ts
│   │   └── admin.controller.ts
│   │
│   ├── 📂 middleware/     # Express middleware
│   │   ├── auth.middleware.ts
│   │   ├── validation.middleware.ts
│   │   ├── error.middleware.ts
│   │   └── rateLimiter.middleware.ts
│   │
│   ├── 📂 models/         # Database models
│   │   ├── user.model.ts
│   │   ├── booking.model.ts
│   │   ├── service.model.ts
│   │   └── payment.model.ts
│   │
│   ├── 📂 routes/         # API routes
│   │   ├── auth.routes.ts
│   │   ├── booking.routes.ts
│   │   ├── service.routes.ts
│   │   └── index.ts
│   │
│   ├── 📂 services/       # Business logic
│   │   ├── auth.service.ts
│   │   ├── booking.service.ts
│   │   ├── payment.service.ts
│   │   ├── email.service.ts
│   │   └── sms.service.ts
│   │
│   ├── 📂 utils/          # Utility functions
│   │   ├── logger.ts
│   │   ├── validators.ts
│   │   └── helpers.ts
│   │
│   ├── 📂 types/          # TypeScript types
│   │   └── index.ts
│   │
│   └── app.ts             # Express app setup
│   └── server.ts          # Server entry point
│
├── 📂 migrations/         # Database migrations
├── 📂 tests/              # Test files
├── 📂 logs/               # Application logs
├── .env.example           # Environment template
├── .gitignore            # Git ignore rules
├── package.json          # Dependencies
├── tsconfig.json         # TypeScript config
└── README.md             # Documentation
```

---

## 🔐 Authentication

### JWT Token Structure

```typescript
interface TokenPayload {
  id: number;
  email: string;
  role: string;
  iat: number;
  exp: number;
}
```

### Protected Routes

```typescript
// Middleware usage example
import { authenticate, authorize } from './middleware/auth';

// Protected route - requires authentication
router.get('/profile', authenticate, getProfile);

// Admin only route
router.get('/admin/users', authenticate, authorize('admin'), getAllUsers);

// Multiple roles
router.get('/bookings', authenticate, authorize(['admin', 'cleaner']), getBookings);
```

---

## 🧪 Testing

### Running Tests

```bash
# Run all tests
npm test

# Run with coverage
npm run test:coverage

# Run specific test file
npm test -- auth.test.ts

# Run in watch mode
npm run test:watch
```

### Test Structure

```typescript
// Example test: auth.test.ts
describe('Auth Controller', () => {
  describe('POST /auth/register', () => {
    it('should register a new user', async () => {
      const response = await request(app)
        .post('/api/v1/auth/register')
        .send({
          email: 'test@example.com',
          password: 'Test123!',
          first_name: 'Test',
          last_name: 'User'
        });

      expect(response.status).toBe(201);
      expect(response.body.success).toBe(true);
      expect(response.body.data).toHaveProperty('token');
    });

    it('should not register user with existing email', async () => {
      const response = await request(app)
        .post('/api/v1/auth/register')
        .send({
          email: 'existing@example.com',
          password: 'Test123!'
        });

      expect(response.status).toBe(409);
      expect(response.body.success).toBe(false);
    });
  });
});
```

---

## 🚀 Deployment

### Azure App Service Deployment

1. **Build the application**
```bash
npm run build
```

2. **Azure CLI deployment**
```bash
# Login to Azure
az login

# Create resource group
az group create --name APCleaningRG --location "South Africa North"

# Create App Service plan
az appservice plan create --name APCleaningPlan --resource-group APCleaningRG --sku B1 --is-linux

# Create Web App
az webapp create --resource-group APCleaningRG --plan APCleaningPlan --name ap-cleaning-backend --runtime "NODE|18-lts"

# Configure environment variables
az webapp config appsettings set --resource-group APCleaningRG --name ap-cleaning-backend --settings NODE_ENV=production

# Deploy code
az webapp deployment source config-zip --resource-group APCleaningRG --name ap-cleaning-backend --src deploy.zip
```

### Docker Deployment

```dockerfile
# Dockerfile
FROM node:18-alpine

WORKDIR /app

COPY package*.json ./
RUN npm ci --only=production

COPY . .
RUN npm run build

EXPOSE 5000

CMD ["node", "dist/server.js"]
```

```bash
# Build and run Docker container
docker build -t ap-cleaning-backend .
docker run -p 5000:5000 --env-file .env ap-cleaning-backend
```

### GitHub Actions CI/CD

```yaml
name: Deploy Backend

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - run: npm ci
      - run: npm test
      - run: npm run build
      
      - uses: azure/webapps-deploy@v2
        with:
          app-name: ap-cleaning-backend
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
```

---

## 👥 Team

### WebCraft Solutions Development Team

| Name | Role | Responsibilities |
|------|------|------------------|
| **Alwande Ngcobo** | Co-Project Manager, Backend Lead | API architecture, database design, security implementation |
| **Arshad Bhula** | Co-Project Manager, Full-Stack Developer | API development, testing, documentation |
| **Troy Krause** | DevOps Engineer, Backend Developer | CI/CD pipelines, Azure deployment, API optimization |
| **Plamedi Minambo** | Backend Developer | Authentication system, payment integration, API endpoints |
| **Jordan Gardiner** | Frontend Developer | API integration, testing |
| **Sibusiso Sikhosana** | UI/UX Designer | API requirements, user flow design |

---

## 📝 License

This project is licensed under the MIT License.

```
MIT License

Copyright (c) 2025 WebCraft Solutions

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

---

<div align="center">
  <p>
    <strong>Built with ❤️ by WebCraft Solutions</strong>
  </p>
  <p>
    <sub>⭐ Star us on GitHub — it helps!</sub>
  </p>
</div>
