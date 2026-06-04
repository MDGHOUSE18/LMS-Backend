

# 💰 Loan Management System (Backend)

A simple and secure backend API for managing loan applications. Built for Indian financial institutions to automate loan processing from application to approval.

## 📌 What This Project Does

- ✅ Users can register and apply for loans online
- ✅ Automatic eligibility checking (age, salary, debt ratio)
- ✅ KYC document upload and verification
- ✅ EMI calculation with payment schedule
- ✅ Multi-stage approval workflow (Officer reviews and approves)
- ✅ Complete audit trail for compliance

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server
- Visual Studio or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/MDGHOUSE18/LMS-Backend.git
   cd LMS-Backend/src/LMS.API
   ```

2. **Update connection string** in `appsettings.json`
   ```json
   "ConnectionStrings": {
     "LMSDB_LOCAL": "Server=localhost;Database=LMS_DB;Trusted_Connection=True;"
   }
   ```

3. **Create database**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**
   ```
   http://localhost:5000/swagger
   ```

## 🔑 Key Features

### For Customers
- **Apply for loans** (₹50,000 to ₹50,00,000)
- **Upload KYC documents** (Aadhaar, PAN, Salary Slip)
- **Track application status** in real-time
- **View EMI schedule** and payment details

### For Loan Officers
- **Review pending applications**
- **Verify documents**
- **Approve or reject loans**
- **View dashboard** with portfolio summary

### System Features
- **Auto-eligibility check**: Age (21-60), Min Salary (₹25K), Debt Ratio (≤50%)
- **EMI Calculator**: Standard formula with amortization schedule
- **Secure Authentication**: JWT tokens with role-based access
- **Audit Logging**: Track all actions for compliance

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 8 | Backend Framework |
| ASP.NET Core Web API | REST API |
| Entity Framework Core | Database ORM |
| SQL Server | Database |
| JWT | Authentication |
| Swagger | API Documentation |

## 📁 Project Structure

```
LMS-Backend/
├── LMS.Domain/           # Entities and business rules
├── LMS.Application/      # Services and business logic
├── LMS.Infrastructure/   # Database and external services
└── LMS.API/             # Controllers and endpoints
```

## 🔐 Authentication

All API endpoints (except login/register) require a JWT token.

**1. Register**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass@123",
  "name": "John Doe",
  "mobile": "9876543210",
  "dateOfBirth": "1990-01-01",
  "role": "Customer"
}
```

**2. Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass@123"
}

Response:
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "dGhpcyB...",
  "expiresIn": 3600
}
```

**3. Use Token**
Add to request headers:
```
Authorization: Bearer {your-token}
```

## 📝 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get token

### Loans
- `POST /api/loan/create` - Create loan application
- `POST /api/loan/submit/{id}` - Submit for approval
- `GET /api/loan/{id}` - Get loan details
- `PUT /api/loan/approve/{id}` - Approve loan (Officer only)
- `PUT /api/loan/reject/{id}` - Reject loan (Officer only)

### Documents
- `POST /api/documents/upload/{loanId}` - Upload KYC document
- `GET /api/documents/loan/{loanId}` - Get all documents
- `PUT /api/documents/verify` - Verify document (Officer only)

### Dashboard
- `GET /api/dashboard` - Get user dashboard data

*Full API documentation available at `/swagger`*

## 📊 Loan Eligibility Rules

The system automatically checks:

| Criteria | Requirement |
|----------|-------------|
| **Age** | 21 - 60 years |
| **Monthly Salary** | Minimum ₹25,000 |
| **Debt Ratio** | (Existing EMI + New EMI) / Income ≤ 50% |
| **Loan Amount** | ≤ 20 × Monthly Income |

## 🧮 EMI Calculation

Uses standard formula:
```
EMI = [P × R × (1+R)ⁿ] / [(1+R)ⁿ – 1]

Where:
P = Loan Amount
R = Monthly Interest Rate
n = Tenure in Months
```

**Example**: ₹5,00,000 @ 10% for 60 months = ₹10,607/month

## 🔒 Security Features

- ✅ JWT token-based authentication
- ✅ Password hashing with Bcrypt
- ✅ Role-based access control (Customer/Officer/Admin)
- ✅ HTTPS/TLS encryption
- ✅ Audit logging for all actions
- ✅ Rate limiting on APIs

##  Compliance (RBI Guidelines)

- KYC document verification (Aadhaar, PAN)
- Data retention: 7 years for KYC, 10 years for audit logs
- Complete audit trail with user, timestamp, and IP
- PII data masking in logs

## 🧪 Testing

Run unit tests:
```bash
dotnet test
```

## 📄 License

MIT License - feel free to use for learning and portfolio purposes.

## 👨‍💻 Author

**Bangar Mahammed Ghouse**  
[GitHub](https://github.com/MDGHOUSE18)

