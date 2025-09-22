
# AP Cleaning Services – Backend (.NET API)

This repository contains the **ASP.NET Core Web API** backend for AP Cleaning Services. The backend powers customer bookings, payments, account management, and admin/driver dashboards. It is designed for scalability, security, and seamless integration with the React frontend.

---

## 🎯 Purpose
The backend provides secure endpoints for handling booking requests, processing payments, managing users, and supporting business workflows such as driver and cleaner assignment.

---

## ✨ Features
- 📅 **Booking Management** – Endpoints for creating, updating, and retrieving bookings.
- 💳 **Payment Integration** – Secure payment processing with upfront requirement.
- 👤 **Authentication & Authorization** – Role-based access (Customer, Admin, Driver).
- 📊 **Admin & Driver Dashboards** – Manage assignments and monitor activity.
- 🛒 **E-commerce API (Future)** – Placeholder endpoints for products.

---

## 🚀 Tech Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core** for ORM
- **SQL Server

---

## ⚙️ Setup Instructions

1. Clone the repository:
   
```
git clone https://github.com/your-org/ap-cleaning-backend.git
cd ap-cleaning-backend
```

2. Configure database connection in appsettings.json:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=APCleaning;User Id=sa;Password=your_password;"
}
```

3. Run migrations:

```
dotnet ef database update
```

4. Start the API:

**Default URL:** http://localhost:5000

```
dotnet run
```

---

## 📂 Project Structure

```
APCleaning.Api/
 ┣ Controllers/     # API endpoints
 ┣ Models/          # Entity models
 ┣ Data/            # Database context, migrations
 ┣ Services/        # Business logic
 ┗ Program.cs
```

---

## 🔮 Future Enhancements

- Expand e-commerce endpoints for product management.
- Analytics for admins (bookings, payments, cleaner performance).
- Notifications (SMS/email reminders).

---
