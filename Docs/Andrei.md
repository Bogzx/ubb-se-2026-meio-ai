### 1. Formal Requirements
*   **Requirement 1:** The system must automatically scrape external web sources to find trailer videos and related content for specific movies in the database.
*   **Requirement 2:** The system must store the scraped video data (video URLs, titles, and associated Movie IDs) persistently in the long-term database.
*   **Requirement 3:** The system must execute the web scraping and database insertion processes autonomously as a background service, without requiring manual user interaction.
*   **Owner:** Andrei
*   **Cross-Team Dependencies:** Database team (for schema additions/storage operations) and Backend logic team (for scheduling/executing the web scraper background service).

### 2. Diagram Blueprint
*   **Use Case Diagram Additions:** 
    *   *Actor:* Automated System (Scraper)
    *   *Use Cases:* "Scrape Movie Video Content", "Store Scraped Videos to Database"
*   **Database Schema Additions:** 
    *   *Tables:* `MovieVideo`
    *   *Columns:* `VideoID` (Primary Key), `MovieID` (Foreign Key linked to `Movie` table), `VideoURL` (String), `VideoTitle` (String), `DateScraped` (DateTime).
    *   *Relationships:* One-to-Many between `Movie` and `MovieVideo` (One movie can have multiple associated videos/trailers).
*   **Class Diagram (MVVM) Additions:** 
    *   *(Note: The scraper itself operates strictly on the backend, but adhering to the MVVM requirement, the app will need UI to display these trailers later).*
    *   *Models:* `MovieVideo`
    *   *Views:* `MovieTrailerPlayerView` (The screen where users can see the successfully scraped trailers)
    *   *ViewModels:* `MovieTrailerPlayerViewModel` (Controls the UI state, fetching scraped DB entries to display)
    *   *Utils/Services:* `WebScraperBackgroundService`, `VideoIngestionService`, `DatabaseContext`

### 3. Project Management Tasks (Max 30-Minutes Each)
*(Note: Since this is purely a background service pitch, the UI tasks pertain to displaying the end result of the scraper's hard work.)*

**Database & Models**
*   **Task:** Define `MovieVideo` Entity Model
    *   **Description:** Create the backend class representation for the `MovieVideo` model, including properties: `VideoID`, `MovieID`, `VideoURL`, and `VideoTitle`. Max 30 mins effort.
*   **Task:** Create Database Migration for `MovieVideo` Table
    *   **Description:** Write the SQL script or ORM migration file to generate the `MovieVideo` table, ensuring the foreign key constraint to the main `Movie` table is established. Max 30 mins effort.
*   **Task:** Implement `MovieVideo` Repository (Write Operations)
    *   **Description:** Setup the repository/DAO layer function to accept a list of newly scraped `MovieVideo` objects and Bulk Insert them into the long-term database. Max 30 mins effort.
*   **Task:** Implement `MovieVideo` Repository (Read Operations)
    *   **Description:** Setup a repository function to query and return all video records matching a specific `MovieID` for future UI consumption. Max 30 mins effort.

**Backend Services & ViewModels**
*   **Task:** Setup Local `WebScraperBackgroundService` Interface
    *   **Description:** Define the `IWebScraperService` interface containing a single method `ScrapeVideosForMovie(string movieTitle)` returning a list of raw video URLs. Max 30 mins effort.
*   **Task:** Implement Network HTTP Trigger for Scraper
    *   **Description:** Write the basic REST/HTTP client code inside the scraper service that pings the target search website (e.g., YouTube/IMDB search route) using a movie's name. Max 30 mins effort.
*   **Task:** Implement HTML/DOM Parsing Logic
    *   **Description:** Parse the raw HTML response fetched in the previous task. Extract the target `<video>` or `<iframe>` src URLs accurately. Max 30 mins effort.
*   **Task:** Develop `VideoIngestionService` Coordinator Logic
    *   **Description:** Create a coordinator class that finds a movie without a trailer, calls the Scraper service, maps the found URLs to `MovieVideo` entities, and sends them to the Database Repository. Max 30 mins effort.
*   **Task:** Create `MovieTrailerPlayerViewModel` Structure
    *   **Description:** Build the ViewModel that accepts a `MovieID` in its constructor, pulls the scraped database URLs from the read repository, and populates an ObservableCollection. Max 30 mins effort.

**GUI (Views)**
*   **Task:** Scaffold `MovieTrailerPlayerView` Layout
    *   **Description:** Create the skeleton UI view. Include a placeholder for the embedded video player and a basic list container for "Other Trailers". Max 30 mins effort.
*   **Task:** Bind UI Video Player to ViewModel Data
    *   **Description:** Set up data binding so the video player component's `Source` property reflects the `SelectedVideoUrl` defined inside the `MovieTrailerPlayerViewModel`. Max 30 mins effort.
*   **Task:** Implement "No Trailers Found" Empty State
    *   **Description:** Add a responsive UI text block to the View that displays "No trailers scraped yet for this movie", conditionally visible based on the ViewModel's list count. Max 30 mins effort.
