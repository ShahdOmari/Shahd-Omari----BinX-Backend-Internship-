# Cardiac Monitoring System — Entity Relationship Diagram

Rendered automatically by GitHub from Mermaid syntax below — no external
diagramming tool required, and stays version-controlled alongside the schema
it documents.

```mermaid
erDiagram
    PATIENTS ||--o{ VITALSIGNS : "has many"
    PATIENTS ||--o{ MEDICATIONS : "has many"
    PATIENTS ||--o{ APPOINTMENTS : "has many"
    ASPNETUSERS ||--o{ ASPNETUSERROLES : "has"
    ASPNETROLES ||--o{ ASPNETUSERROLES : "has"

    PATIENTS {
        int Id PK
        string FullName
        datetime DateOfBirth
        string Gender
    }

    VITALSIGNS {
        int Id PK
        int PatientId FK
        int HeartRateBpm
        int SystolicBp
        int DiastolicBp
        double OxygenSaturationPercent
        datetime RecordedAtUtc
        string RiskLevel
    }

    MEDICATIONS {
        int Id PK
        int PatientId FK
        string Name
        double DosageMg
        string Frequency
    }

    APPOINTMENTS {
        int Id PK
        int PatientId FK
        datetime ScheduledAtUtc
        string DoctorName
        string Reason
    }

    ASPNETUSERS {
        string Id PK
        string Email
        string PasswordHash
    }

    ASPNETROLES {
        string Id PK
        string Name
    }

    ASPNETUSERROLES {
        string UserId FK
        string RoleId FK
    }
```

## Normalization Confirmation (3NF)

- **1NF** — every column holds a single atomic value (no comma-separated lists;
  e.g. `Medication.Frequency` is a single descriptive string, not a multi-value field).
- **2NF** — no composite primary keys in the domain tables, so partial-key
  dependency doesn't apply; every non-key column depends on the whole key (`Id`).
- **3NF** — no non-key column depends on another non-key column. `Patient.FullName`
  is stored only on `Patients`, never duplicated onto `VitalSigns`/`Medications`/
  `Appointments` even though those tables reference a patient — each fact lives in
  exactly one place.

## Relationships

- `Patient` → `VitalSign` : one-to-many, cascade delete
- `Patient` → `Medication` : one-to-many, cascade delete
- `Patient` → `Appointment` : one-to-many, cascade delete
- `AspNetUser` → `AspNetRole` : many-to-many via `AspNetUserRoles` (standard
  ASP.NET Core Identity schema)
