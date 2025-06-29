# Project Plan: Meeting Point Finder

**Goal:** Develop a full-stack application to suggest an optimal meeting point given multiple starting addresses and a single destination, leveraging Google APIs for geocoding and route calculations.

---

### Phase 1: Project Setup & Initial Configuration

1.  **Create Project Directories:**
    *   Establish a root directory (e.g., `MeetingPointApp`).
    *   Inside, create `backend` (for C# API) and `frontend` (for React app) subdirectories.
2.  **Backend (C# .NET API) Setup:**
    *   Create a new .NET Web API project within the `backend` directory.
    *   Configure `appsettings.json` for Google API key storage (or recommend environment variables for production).
3.  **Frontend (React TypeScript) Setup:**
    *   Create a new React project with TypeScript within the `frontend` directory using `create-react-app` or Vite.
    *   Install necessary dependencies (e.g., `axios` for API calls, `@react-google-maps/api` for map integration).
4.  **Google API Key Acquisition:**
    *   **Action for User:** Obtain a Google Cloud API Key. Enable the following APIs in your Google Cloud project:
        *   **Geocoding API:** For converting addresses to coordinates.
        *   **Directions API:** For calculating routes and travel times.
        *   **Distance Matrix API:** (Optional, but useful) For calculating distances/travel times between multiple origins and destinations.
    *   **Security Note:** Advise on restricting the API key to specific IP addresses (for backend) and HTTP referrers (for frontend) for security.

### Phase 2: Backend Development (C# .NET API)

1.  **Define Data Models (DTOs):**
    *   Create C# classes for request payloads (e.g., `MeetingPointRequest` containing lists of starting addresses and a destination address) and response payloads (e.g., `MeetingPointResponse` containing the suggested meeting point address, coordinates, and perhaps travel times).
2.  **Google Maps Service Integration:**
    *   Create a dedicated service class (e.g., `GoogleMapsService`) to encapsulate all interactions with Google APIs.
    *   Implement methods for:
        *   `GeocodeAddress(string address)`: Converts a human-readable address to latitude/longitude coordinates.
        *   `CalculateRoute(LatLng origin, LatLng destination)`: Calculates route details (e.g., duration, distance).
        *   `CalculateDistanceMatrix(List<LatLng> origins, List<LatLng> destinations)`: (If using Distance Matrix API) Calculates travel times/distances between multiple points.
3.  **Meeting Point Business Logic:**
    *   Implement the core logic within a service (e.g., `MeetingPointCalculatorService`).
    *   **Initial Meeting Point Algorithm (for prototype):**
        1.  Geocode all starting addresses and the destination address.
        2.  Calculate the geographic centroid (average latitude and longitude) of all starting points. This will be the initial candidate for the meeting point.
        3.  (Optional but recommended for better results): Use the Google Directions API to find a more practical meeting point. For example, iterate through a few points around the centroid or along the routes to the destination, and select the one that minimizes the *sum of travel times* from all starting points to that meeting point.
        4.  Calculate the route from the chosen meeting point to the final destination.
4.  **API Controller:**
    *   Create an API controller (e.g., `MeetingPointController`) with a single endpoint (e.g., `POST /api/meetingpoint`).
    *   This endpoint will receive the request DTO, call the `MeetingPointCalculatorService`, and return the response DTO.
5.  **Error Handling & Validation:**
    *   Implement input validation for addresses.
    *   Handle potential errors from Google API calls (e.g., invalid addresses, API limits).

### Phase 3: Frontend Development (React with TypeScript)

1.  **Component Structure:**
    *   Create main components: `App.tsx`, `AddressInputForm.tsx`, `MeetingPointDisplay.tsx`, `MapComponent.tsx`.
2.  **Address Input Form (`AddressInputForm.tsx`):**
    *   Allow users to dynamically add multiple starting address input fields.
    *   Include a single input field for the destination address.
    *   Add a "Calculate Meeting Point" button.
    *   Implement basic client-side validation (e.g., ensuring fields are not empty).
3.  **API Integration:**
    *   Use `axios` or the built-in `fetch` API to send requests to your C# backend.
    *   Handle loading states and display error messages to the user.
4.  **Meeting Point Display (`MeetingPointDisplay.tsx`):
    *   Display the suggested meeting point address and any relevant travel time information received from the API.
5.  **Map Visualization (`MapComponent.tsx`):**
    *   Integrate a Google Maps component (e.g., `@react-google-maps/api`).
    *   Display markers for all starting points, the suggested meeting point, and the final destination.
    *   Draw routes from starting points to the meeting point, and from the meeting point to the destination.
6.  **State Management:**
    *   Use React Hooks (e.g., `useState`, `useEffect`) to manage form inputs, API response data, loading states, and errors.

### Phase 4: Local Development & Testing

1.  **Run Backend:** Instructions on how to run the C# .NET API (e.g., `dotnet run` from the `backend` directory).
2.  **Run Frontend:** Instructions on how to run the React development server (e.g., `npm start` or `yarn start` from the `frontend` directory).
3.  **Testing:**
    *   Manually test with various valid and invalid addresses.
    *   Verify that the meeting point is displayed correctly on the UI and map.
    *   Check error handling for invalid inputs or API failures.
