# HR Leave Management System - Comprehensive Project Overview

An enterprise-grade **HR Leave-Management System** developed as the final project for the DataScience Middle East Internship[cite: 1]. The system enforces strict multi-layered business validation, secure role-based authorization, clean architectural patterns, and a robust standalone Angular user interface[cite: 1].

---

## 🏛️ Deep-Dive Architecture & Layer Responsibilities

The backend strictly implements **Clean Architecture**, enforcing unidirectional dependency flow across layers[cite: 1]:

* **`HrSystem.Domain`**: 
  * Acts as the core foundation containing enterprise entities, business rules, enums, and value objects[cite: 1].
  * Completely isolated with **zero dependencies** on external frameworks, databases (EF Core), HTTP protocols, or UI[cite: 1].
* **`HrSystem.Application`**: 
  * Manages application use cases, DTOs, service interfaces, validation pipelines, and custom **Result patterns**[cite: 1].
  * Depends solely on the `Domain` layer[cite: 1].
* **`HrSystem.Infrastructure`**: 
  * Implements database persistence using **Entity Framework Core**, database migrations, LINQ repositories, and JWT token generation[cite: 1].
  * Implements abstractions defined by the Application layer[cite: 1].
* **`HrSystem.Api`**: 
  * Contains thin API controllers, dependency injection configuration, and centralized exception/Problem Details error mapping[cite: 1].
  * Contains zero business logic calculations[cite: 1].
* **`HrSystem.UnitTests`**: 
  * Dedicated xUnit test suite targeting core domain logic, edge cases, and policy validation workflows[cite: 1].

---

## ⚙️ Core Business Rules & Technical Specifications

### 1. Opt-In / Opt-Out State Machine
* **Default Status**: Active employees automatically start as **Opted-In** unless overridden by Admin configuration[cite: 1].
* **Restrictions**: Opted-out employees are strictly blocked from submitting new leave requests; historical logs remain untouched[cite: 1].
* **Conditions to Opt-Out**: An employee can only opt-out if they have **no Pending requests** and **no Approved requests with a future start date**[cite: 1].
* **Balance Preservation**: Opting out freezes current leave balances without resetting them, allowing seamless continuation upon re-opting in[cite: 1].
* **Admin Controls**: Admins can configure self opt-out permissions, re-opt-in cooldown periods in days, or force status updates with a mandatory audit reason[cite: 1].

### 2. Leave Types & Validations
* **Vacation**: Consumes vacation balance; bounded by Admin-configured minimum notice days and maximum consecutive business days limits[cite: 1].
* **Day Off**: Consumes day-off balance and must charge **exactly 1 business day**[cite: 1].
* **Sick Leave**: Consumes sick leave balance; restricted to today or backdated strictly within the configured backdate window (future sick leaves are invalid)[cite: 1].

### 3. Smart Date & Calendar Calculations
* **Business Days Only**: Saturdays, Sundays, and configured public holidays are automatically excluded from charging balances[cite: 1].
* **Zero-Day Validation**: Requests consisting entirely of non-business days or weekends are marked as invalid[cite: 1].
* **Calendar Boundaries**: Requests cannot cross calendar-year boundaries within the mandatory scope[cite: 1].
* **Policy Snapshotting**: Upon submission, requests capture and freeze the exact policy rules and version active at that moment, shielding existing requests from future admin updates[cite: 1].

### 4. Overlap & Balance Reservation
* **Overlap Protection**: Pending and approved requests block overlapping dates; rejected and cancelled requests do not block overlaps[cite: 1].
* **Balance Reservation**: Both Pending and Approved states reserve balance to prevent concurrent overspending[cite: 1].
* **Dynamic Releasing**: Rejecting or cancelling requests automatically releases reserved days back to the employee's available balance[cite: 1].

---

## 🔒 Security, Roles & Authorization Matrix

The application enforces fine-grained role-based security using JSON Web Tokens (JWT) containing stable user identifiers and distinct roles[cite: 1]:

| Capability / Action | Employee | Manager | Admin |
| :--- | :---: | :---: | :---: |
| **View/Update Own Profile & Login** | ✅ | ✅ | ✅ |
| **Manage Own Leave Requests (Create/Cancel/Edit)** | ✅ | ✅ | ❌ (Not Required) |
| **Self Opt-In / Opt-Out Participation** | ✅ | ✅ | ✅ (Can Force Change) |
| **View Team / Direct Report Requests** | ❌ | ✅ (Direct Reports Only) | ✅ (All Requests) |
| **Approve / Reject Requests** | ❌ | ✅ (Direct Reports Only) | ✅ (Manager Requests / Overrides) |
| **Configure System Policies, Holidays & Users** | ❌ | ❌ | ✅ |

* **Manager Self-Leave Rule**: When a manager submits personal leave, it routes directly to **Admin approval** to prevent self-approval conflicts[cite: 1].

---

## 🚨 Error Handling & RFC 7807 Problem Details

The API avoids generic error responses and uses a consistent **Problem Details** format carrying stable application error codes for UI consumption[cite: 1]:
* `400 Bad Request`: Validation failures (e.g., bad dates, day-off exceeding 1 day)[cite: 1].
* `401 Unauthorized`: Missing, expired, or invalid JWT tokens[cite: 1].
* `403 Forbidden`: Scope violations (e.g., accessing another employee's record or managing outside team boundaries)[cite: 1].
* `404 Not Found`: Missing resources, users, or policies[cite: 1].
* `409 Conflict`: Business rule conflicts (e.g., `leave.overlap`, `participation.already_opted_out`, insufficient balance)[cite: 1].

---

## 🧪 Unit Testing Coverage

The backend includes comprehensive automated unit tests (`dotnet test`) verifying core domain integrity[cite: 1]:
* Business-day calculations across weekends and public holidays[cite: 1].
* Multi-state overlap matrix detection[cite: 1].
* Balance reservation, deduction, and release accuracy[cite: 1].
* Vacation notice limits and maximum consecutive rules[cite: 1].
* State transition validation and prevention of duplicate final actions[cite: 1].
