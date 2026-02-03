# API Documentation for UI – Organization & Patient

Base URL: `api/Organization` and `api/Patient`.  
All endpoints require **Authorization** (e.g. Bearer token).

**Response envelope (all APIs):**

- `success` / `isSuccess`: boolean  
- `data`: response payload (type below)  
- `totalCount`: number (for list/dropdown when applicable)  
- `errors`: string[]  
- `message`: string  

---

## 1. Kroll (PMS) – Organization

*Prefix: `api/Organization/kroll/` — data from Kroll DB.*

### 1.1 Organization dropdown (Kroll)

**GET** `api/Organization/kroll/dropdown`

**Request (query):**

| Name           | Type   | Required | Description      |
|----------------|--------|----------|------------------|
| searchKeyword  | string | No       | Filter by name   |

**Response:** `ApplicationResult<List<OrganizationResponseDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": 0,
      "organizationExternalId": 0,
      "name": "string",
      "address": "string",
      "defaultEmail": "string",
      "createdDate": "2024-01-01T00:00:00Z",
      "wardIds": null,
      "wards": []
    }
  ],
  "totalCount": 0
}
```

---

### 1.2 Organization list (Kroll)

**GET** `api/Organization/kroll/get`

**Request (query):**

| Name            | Type    | Required | Description        |
|-----------------|---------|----------|--------------------|
| pageNumber      | int     | No       | Default: 1        |
| pageSize        | int     | No       | Default: 50       |
| searchKeyword   | string  | No       | Filter by name     |
| sortByAscending | boolean | No       | Default: false    |

**Response:** `ApplicationResult<List<OrganizationResponseDto>>`  
Same `data` shape as 1.1, with `totalCount` for pagination.

---

### 1.3 Wards list (Kroll)

**GET** `api/Organization/kroll/getWards`

**Request (query):**

| Name            | Type   | Required | Description      |
|-----------------|--------|----------|------------------|
| organizationId  | long   | Yes      | Kroll org ID     |
| searchKeyword   | string | No       | Filter wards     |

**Response:** `ApplicationResult<List<WardResponseDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": 0,
      "name": "string",
      "externalId": 0
    }
  ]
}
```

---

### 1.4 Wards dropdown (Kroll)

**GET** `api/Organization/kroll/wards/dropdown`

**Request (query):**

| Name           | Type | Required | Description  |
|----------------|------|----------|-------------|
| organizationId | long | Yes      | Kroll org ID |

**Response:** `ApplicationResult<List<WardResponseDto>>`  
Same `data` shape as 1.3.

---

### 1.5 Save organization from Kroll

**POST** `api/Organization/kroll/save`

**Request (body):**

```json
{
  "organizationId": 0,
  "wardIds": [ 0, 0 ]
}
```

| Name           | Type    | Required | Description                    |
|----------------|---------|----------|--------------------------------|
| organizationId | long    | Yes      | Organization ID from Kroll    |
| wardIds        | long[]  | No       | Ward IDs from Kroll to link    |

**Response:** `ApplicationResult<bool>`

```json
{
  "success": true,
  "data": true
}
```

---

## 2. Organization (local – no Kroll prefix)

### 2.1 Organization list (local)

**GET** `api/Organization/get`

**Request (query):**

| Name                  | Type   | Required | Description        |
|-----------------------|--------|----------|--------------------|
| pageNumber            | int    | No       | Default: 1        |
| pageSize              | int    | No       | Default: 50        |
| searchKeyword         | string | No       | Name/address/email  |
| orderBy               | string | No       | Default: CreatedDate |
| sortByAscending       | bool   | No       | Default: false     |
| organizationId        | long   | No       | Filter by org ID   |
| organizationExternalId| long   | No       | Filter by external ID |
| ward                  | string | No       | Comma-separated ward IDs |

**Response:** `ApplicationResult<List<OrganizationResponseDto>>`  
`data` includes `wards` and `wardIds`; `totalCount` for pagination.

---

### 2.2 Organization dropdown (local)

**GET** `api/Organization/dropdown`

**Request (query):** Same as other dropdowns (e.g. optional `searchKeyword` if supported).

**Response:** `ApplicationResult<List<OrganizationDropdownDto>>`  
(Id, Name, etc. for dropdown use.)

---

### 2.3 Wards list (local)

**GET** `api/Organization/getWards`

**Request (query):**

| Name          | Type | Required | Description   |
|---------------|------|----------|---------------|
| wardId        | int  | No       | Filter by ward |
| pageNumber    | int  | No       | Default: 1    |
| pageSize      | int  | No       | Default: 50   |
| searchKeyword | string | No     | Filter        |

**Response:** `ApplicationResult<List<WardPageResponseDTO>>`  
(Id, Name, ExternalId, OrganizationId, OrganizationName, CreatedDate, ModifiedDate.)

---

### 2.4 Wards dropdown (local)

**GET** `api/Organization/wards/dropdown`

**Request (query):**

| Name           | Type | Required | Description   |
|----------------|------|----------|---------------|
| organizationId | long | Yes      | Local org ID  |

**Response:** `ApplicationResult<List<WardResponseDto>>`  
Same shape as 1.3.

---

### 2.5 Save organization (local)

**POST** `api/Organization/save`

**Request (body):**

```json
{
  "name": "string",
  "organizationExternalId": 0,
  "wardIds": [ 0 ],
  "address": "string",
  "defaultEmail": "string"
}
```

| Name                   | Type   | Required | Description           |
|------------------------|--------|----------|-----------------------|
| name                   | string | Yes      | Organization name     |
| organizationExternalId | long   | Yes      | External (e.g. Kroll) ID |
| wardIds                | long[] | No       | Local ward IDs        |
| address                | string | No       | Address               |
| defaultEmail           | string | No       | Email                 |

**Response:** `ApplicationResult<long>`

```json
{
  "success": true,
  "data": 0
}
```

`data` = created or existing organization Id (local).

---

## 3. Kroll (PMS) – Patient

*Patient dropdown uses Kroll DB; route has no `/kroll` prefix.*

### 3.1 Patient dropdown (from Kroll)

**GET** `api/Patient/dropdown`

**Request (query):**

| Name          | Type   | Required | Description     |
|---------------|--------|----------|-----------------|
| searchKeyword | string | No       | Filter by name  |

**Response:** `ApplicationResult<List<PatientResponseDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": 0,
      "patientId": 0,
      "name": "string",
      "address": "string",
      "defaultEmail": "string",
      "status": "active",
      "createdDate": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 0
}
```

---

### 3.2 Save patient from Kroll

**POST** `api/Patient/kroll/save`

**Request (body):**

```json
{
  "patientId": [ 0, 0 ]
}
```

| Name      | Type   | Required | Description              |
|-----------|--------|----------|--------------------------|
| patientId | long[] | Yes      | Patient IDs from Kroll   |

**Response:** `ApplicationResult<bool>`

```json
{
  "success": true,
  "data": true
}
```

---

## 4. Patient (local – no Kroll prefix)

### 4.1 Patient list

**GET** `api/Patient/list`

**Request (query):**

| Name            | Type   | Required | Description     |
|-----------------|--------|----------|-----------------|
| pageNumber      | int    | No       | Default: 1      |
| pageSize        | int    | No       | Default: 50     |
| searchKeyword   | string | No       | Filter by name  |
| orderBy         | string | No       | Default: CreatedDate |
| sortByAscending | bool   | No       | Default: false  |

**Response:** `ApplicationResult<List<PatientResponseDto>>`  
Same `data` shape as 3.1; `totalCount` for pagination.

---

### 4.2 Save patient (local)

**POST** `api/Patient/save`

**Request (body):**

```json
{
  "name": "string",
  "patientId": 0,
  "address": "string",
  "defaultEmail": "string",
  "status": "active"
}
```

| Name         | Type   | Required | Description        |
|--------------|--------|----------|--------------------|
| name         | string | Yes      | Patient name       |
| patientId    | long   | Yes      | External (Kroll) ID |
| address      | string | No       | Address            |
| defaultEmail | string | No       | Email              |
| status       | string | No       | Default: "active"  |

**Response:** `ApplicationResult<long>`

```json
{
  "success": true,
  "data": 0
}
```

`data` = created patient Id (local).

---

## Quick reference

| Purpose                    | Method | Endpoint                              | Source   |
|---------------------------|--------|----------------------------------------|----------|
| Org dropdown (Kroll)      | GET    | api/Organization/kroll/dropdown         | Kroll    |
| Org list (Kroll)          | GET    | api/Organization/kroll/get             | Kroll    |
| Wards list (Kroll)        | GET    | api/Organization/kroll/getWards        | Kroll    |
| Wards dropdown (Kroll)    | GET    | api/Organization/kroll/wards/dropdown   | Kroll    |
| Save org from Kroll       | POST   | api/Organization/kroll/save            | Kroll    |
| Org list (local)          | GET    | api/Organization/get                   | Local    |
| Org dropdown (local)      | GET    | api/Organization/dropdown              | Local    |
| Wards list (local)        | GET    | api/Organization/getWards              | Local    |
| Wards dropdown (local)    | GET    | api/Organization/wards/dropdown        | Local    |
| Save org (local)          | POST   | api/Organization/save                  | Local    |
| Patient dropdown (Kroll)  | GET    | api/Patient/dropdown                   | Kroll    |
| Save patient from Kroll   | POST   | api/Patient/kroll/save                 | Kroll    |
| Patient list              | GET    | api/Patient/list                       | Kroll    |
| Save patient (local)      | POST   | api/Patient/save                       | Local    |
