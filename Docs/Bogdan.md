### 1. Formal Requirements
*   **Requirement 1:** The system must present a sequential, interactive stack of movie cards (showing poster and title) to the authenticated user, sourced from the external Movie table.
    *   **Verified by:** Unit test confirming that the card queue is populated with movies the user has not yet swiped on, and that each card shows the correct poster and title.
*   **Requirement 2:** The system must allow the user to swipe a card right to "like" or left to "dislike/skip" the movie.
    *   **Verified by:** UI integration test confirming that a right-drag beyond 30% of card width triggers a "like" action, and a left-drag beyond 30% triggers a "dislike" action.
*   **Requirement 3:** The system must update the user's `UserMoviePreference` score for the swiped movie after every swipe action — increasing the score by **+1.0** on a right-swipe and decreasing it by **−0.5** on a left-swipe. If no preference row exists, one is created with an initial score of **0.0** before applying the delta.
    *   **Verified by:** Unit test asserting that after a right-swipe the score increases by exactly 1.0 and after a left-swipe the score decreases by exactly 0.5. Integration test confirming correct upsert when no prior row exists.
*   **Requirement 4:** The system must persist all preference score changes to the long-term database immediately after each swipe.
    *   **Verified by:** Integration test confirming that a `UserMoviePreference` row is written/updated in the database within the same request cycle as the swipe action.
*   **Owner:** Bogdan
*   **Cross-Team Dependencies:**
    *   **External Group:** Depends on the other group's `Movie` table for the movie cards displayed during swiping.
    *   **Gabi:** Gabi's tournament also writes to `UserMoviePreference` — coordinate on the upsert logic and score deltas (Bogdan: right-swipe +1.0 / left-swipe −0.5; Gabi: tournament winner boost +2.0).
    *   **Tudor:** Tudor's reel-like feature also boosts `UserMoviePreference` scores (Tudor: reel-like boost +1.5) — coordinate on the shared `UserMoviePreferenceModel`.
    *   **Madi:** Madi's personality matching reads from `UserMoviePreference` — the scores Bogdan writes are the input for matching.

---

### 2. Diagram Blueprint

*   **Use Case Diagram Additions:**
    *   **Actor:** Authenticated User
    *   **Use Cases:** `Swipe on Movie Card`, `Update Movie Preference Score`, `Load Next Movie Cards`.

*   **Database Schema Additions:**
    *   *(This feature does NOT create new tables. It reads from the external `Movie` table and writes to the shared `UserMoviePreference` table.)*
    *   **Shared Table: `UserMoviePreference`** (UserId FK, MovieId FK, Score FLOAT, LastModified DATETIME)
        *   Right-swipe → increase Score by +1.0 for that (UserId, MovieId) row.
        *   Left-swipe → decrease Score by −0.5 for that (UserId, MovieId) row.
        *   If no row exists yet, create one with an initial score of 0.0 before applying the delta.

*   **Class Diagram (MVVM) Additions:**
    *   *Models:* `MovieCardModel` (projection of external Movie data for display), `UserMoviePreferenceModel`
    *   *Views:* `MovieSwipeView`, `SwipeResultSummaryView` (optional — shows recent swipe stats)
    *   *ViewModels:* `MovieSwipeViewModel` (controls the card deck, swipe state, and score updates)
    *   *Utils/Services:* `ISwipeService` / `SwipeService` (handles score updates to `UserMoviePreference` AND fetches unswiped movies from the external Movie table), `IPreferenceRepository` / `PreferenceRepository` (data access for `UserMoviePreference`)

---

### 3. Project Management Tasks (Max 30-Minutes Each)

**Database & Models**
*   **Task:** Define `UserMoviePreference` Table Schema
    *   **Description:** Design and create the database migration for the `UserMoviePreference` table with columns: `UserMoviePreferenceId` (PK), `UserId` (FK → User), `MovieId` (FK → Movie), `Score` (FLOAT), `LastModified` (DATETIME). Add a UNIQUE constraint on the (`UserId`, `MovieId`) pair to prevent duplicates. Max 30 mins effort.
*   **Task:** Create `MovieCardModel` Data Class
    *   **Description:** Code the Model class representing the display data for a swipe card: `MovieId`, `Title`, `PosterUrl`, `PrimaryGenre`. This is a read-only projection from the external Movie table. Max 30 mins effort.
*   **Task:** Create `UserMoviePreferenceModel` Data Class *(shared — Bogdan is the owner)*
    *   **Description:** Code the Model class mirroring the shared `UserMoviePreference` table: `UserMoviePreferenceId`, `UserId`, `MovieId`, `Score`, `LastModified`. Add getters/setters. This is the canonical shared model — Gabi, Tudor, and Madi must use this same class. Max 30 mins effort.
*   **Task:** Implement `IPreferenceRepository` Interface & `PreferenceRepository`
    *   **Description:** Define the repository interface with methods: `UpsertPreferenceAsync(UserMoviePreferenceModel)` and `GetUnswipedMovieIdsAsync(int userId) → List<int>`. Implement the concrete class using the database context. Max 30 mins effort.
*   **Task:** Implement Unswiped Movies Query
    *   **Description:** Write the repository method to fetch movies from the external Movie table that the current user has NOT yet swiped on (i.e., no matching row in `UserMoviePreference`). Return as a list of `MovieCardModel`. Max 30 mins effort.

**Backend Services & ViewModels**
*   **Task:** Scaffold `MovieSwipeViewModel`
    *   **Description:** Create the `MovieSwipeViewModel` class inheriting from `ViewModelBase`. Define observable properties: `CurrentCard` (MovieCardModel), `CardQueue` (ObservableCollection), `IsLoading` (bool). Implement `INotifyPropertyChanged` via the base class. Max 30 mins effort.
*   **Task:** Implement `SwipeRightCommand`
    *   **Description:** Create the ViewModel Command for swiping right. Take the `CurrentCard`, call `ISwipeService.UpdatePreferenceScoreAsync(userId, movieId, isLiked: true)` to boost the movie's score by +1.0 in `UserMoviePreference`, then advance to the next card. Max 30 mins effort.
*   **Task:** Implement `SwipeLeftCommand`
    *   **Description:** Create the ViewModel Command for swiping left. Call `ISwipeService.UpdatePreferenceScoreAsync(userId, movieId, isLiked: false)` to decrease the movie's score by −0.5, then advance to the next card. Max 30 mins effort.
*   **Task:** Implement Card Queue Auto-Refill Logic
    *   **Description:** Write logic inside `MovieSwipeViewModel` to hold a queue of the next 5 cards. When the queue drops to 2, automatically request more unswiped movies from `ISwipeService.GetUnswipedMoviesAsync()`. Max 30 mins effort.
*   **Task:** Define `ISwipeService` Interface
    *   **Description:** Create the service interface with methods: `UpdatePreferenceScoreAsync(userId, movieId, isLiked)` and `GetUnswipedMoviesAsync(userId, count)`. Max 30 mins effort.
*   **Task:** Implement `SwipeService` Score Logic
    *   **Description:** Implement the concrete service. On like (isLiked=true): increase score by +1.0. On dislike (isLiked=false): decrease score by −0.5. Use the `IPreferenceRepository.UpsertPreferenceAsync()` method. If no row exists, create with initial score 0.0 then apply delta. Max 30 mins effort.
*   **Task:** Implement ViewModel Error Handling
    *   **Description:** Add try-catch and logging around all service calls in `MovieSwipeViewModel` so database failures don't crash the swiping UI. Max 30 mins effort.

**GUI (Views)**
*   **Task:** Create `MovieSwipeView` Skeleton Layout
    *   **Description:** Create the base UI layout file. Set up the main structural container that will center the movie cards on screen. Max 30 mins effort.
*   **Task:** Design Static `MovieCard` UI Component
    *   **Description:** Design the visual aesthetics of a single movie card (Image taking up 70%, Title/Genre taking up 30%, rounded corners, subtle shadow). Max 30 mins effort.
*   **Task:** Wire Data Binding for `MovieSwipeView`
    *   **Description:** Connect `MovieSwipeView` to its ViewModel. Bind the top card UI element to `CurrentCard`. Max 30 mins effort.
*   **Task:** Implement Swipe Gesture Event Listeners
    *   **Description:** Add touch-and-drag X/Y coordinate event listeners onto the `MovieCard` UI component. Max 30 mins effort.
*   **Task:** Implement Drag-Threshold Decision Logic
    *   **Description:** Write calculations to determine if a drag exceeded the "Swipe Decision Margin" (e.g., dragged > 30% off-screen), and execute the correct ViewModel Command on release. Max 30 mins effort.
*   **Task:** Bind Visual Overlay Opacity ("Like" / "Nope")
    *   **Description:** Add Green 'LIKE' and Red 'NOPE' text overlays to the card. Bind their opacity to the drag distance from center. Max 30 mins effort.
